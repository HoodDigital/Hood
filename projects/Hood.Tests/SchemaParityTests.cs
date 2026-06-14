using System;
using System.IO;
using System.Linq;
using Hood.Services;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Hood.Tests
{
    /// <summary>
    /// Schema-parity / convergence guard. Provisions a fresh standard-Identity database via the
    /// hood-schema runner and asserts the converged shape that an upgraded 6.x database also lands on:
    /// bounded (nvarchar(450)) + indexed user-reference columns, a universal nullable
    /// AspNetRoles.RemoteId, and a nullable AspNetUsers.Anonymous. Also checks the convergence delta
    /// relaxes Anonymous and is idempotent, and that the runner re-run is a clean no-op.
    ///
    /// SkippableFact-gated on a reachable SQL Server: runs in CI (DB provisioned), skips locally without
    /// one. These pins go red on the pre-convergence schema (AuthorId nvarchar(max), no index, no
    /// RemoteId) and green after, so the drift cannot silently return.
    ///
    /// Full from-6.x-baseline upgrade parity (apply a committed 6.x snapshot, upgrade, diff vs fresh)
    /// needs a 6.x baseline fixture the repo doesn't yet carry — that broader parity is tracked separately.
    /// </summary>
    [Collection("Database")]
    public class SchemaParityTests
    {
        private readonly DatabaseFixture _fixture;

        public SchemaParityTests(DatabaseFixture fixture) => _fixture = fixture;

        private string ConnFor(string db) =>
            new SqlConnectionStringBuilder(_fixture.ConnectionString)
            {
                InitialCatalog = db,
            }.ConnectionString;

        private void DropDatabase(string db)
        {
            using var c = new SqlConnection(
                new SqlConnectionStringBuilder(_fixture.ConnectionString)
                {
                    InitialCatalog = "master",
                }.ConnectionString
            );
            c.Open();
            using var cmd = c.CreateCommand();
            cmd.CommandText =
                $"IF DB_ID('{db}') IS NOT NULL BEGIN "
                + $"ALTER DATABASE [{db}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{db}]; END";
            cmd.ExecuteNonQuery();
        }

        private void ProvisionFresh(string db)
        {
            DropDatabase(db);
            var result = new DatabaseDeployService().Deploy(ConnFor(db), null, _ => { });
            Assert.True(result.Successful, result.Error);
        }

        private void Exec(string db, string sql)
        {
            using var c = new SqlConnection(ConnFor(db));
            c.Open();
            foreach (
                var batch in sql.Split(
                        new[] { "\nGO\n", "\nGO\r\n", "\r\nGO\r\n" },
                        StringSplitOptions.RemoveEmptyEntries
                    )
                    .Where(b => !string.IsNullOrWhiteSpace(b))
            )
            {
                using var cmd = c.CreateCommand();
                cmd.CommandText = batch;
                cmd.ExecuteNonQuery();
            }
        }

        private T Scalar<T>(string db, string sql)
        {
            using var c = new SqlConnection(ConnFor(db));
            c.Open();
            using var cmd = c.CreateCommand();
            cmd.CommandText = sql;
            var v = cmd.ExecuteScalar();
            return v == null || v == DBNull.Value ? default : (T)Convert.ChangeType(v, typeof(T));
        }

        private static string ReadEmbedded(string resourceName)
        {
            using var s = typeof(DatabaseDeployService).Assembly.GetManifestResourceStream(
                resourceName
            );
            Assert.NotNull(s);
            using var r = new StreamReader(s!);
            return r.ReadToEnd();
        }

        [SkippableFact]
        public void Fresh_user_reference_columns_are_bounded_450_and_indexed()
        {
            Skip.IfNot(_fixture.Available, _fixture.UnavailableReason);
            const string db = "HoodParity_UserRef";
            try
            {
                ProvisionFresh(db);
                foreach (
                    var (table, col, ix) in new[]
                    {
                        ("HoodContent", "AuthorId", "IX_HoodContent_AuthorId"),
                        ("HoodLogs", "UserId", "IX_HoodLogs_UserId"),
                        ("HoodProperties", "AgentId", "IX_HoodProperties_AgentId"),
                    }
                )
                {
                    // nvarchar(max) reports CHARACTER_MAXIMUM_LENGTH = -1; the converged shape is 450.
                    var maxLen = Scalar<int>(
                        db,
                        $"SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='{table}' AND COLUMN_NAME='{col}'"
                    );
                    Assert.True(
                        maxLen == 450,
                        $"{table}.{col} should be nvarchar(450), was {maxLen}"
                    );

                    var hasIndex = Scalar<int>(
                        db,
                        $"SELECT COUNT(*) FROM sys.indexes WHERE name='{ix}' AND object_id=OBJECT_ID('{table}')"
                    );
                    Assert.True(hasIndex == 1, $"index {ix} missing on {table}");
                }
            }
            finally
            {
                DropDatabase(db);
            }
        }

        [SkippableFact]
        public void Fresh_AspNetRoles_has_RemoteId_and_Anonymous_is_nullable()
        {
            Skip.IfNot(_fixture.Available, _fixture.UnavailableReason);
            const string db = "HoodParity_Roles";
            try
            {
                ProvisionFresh(db);

                var hasRemoteId = Scalar<int>(
                    db,
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='AspNetRoles' AND COLUMN_NAME='RemoteId'"
                );
                Assert.True(hasRemoteId == 1, "AspNetRoles.RemoteId should exist on both backends");

                var anonNullable = Scalar<string>(
                    db,
                    "SELECT IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='AspNetUsers' AND COLUMN_NAME='Anonymous'"
                );
                Assert.Equal("YES", anonNullable);
            }
            finally
            {
                DropDatabase(db);
            }
        }

        [SkippableFact]
        public void Runner_re_run_is_a_clean_no_op()
        {
            Skip.IfNot(_fixture.Available, _fixture.UnavailableReason);
            const string db = "HoodParity_Idem";
            try
            {
                ProvisionFresh(db);
                var second = new DatabaseDeployService().Deploy(ConnFor(db), null, _ => { });
                Assert.True(second.Successful, second.Error);
                Assert.Empty(second.ScriptsApplied);
            }
            finally
            {
                DropDatabase(db);
            }
        }

        [SkippableFact]
        public void Converge_delta_relaxes_anonymous_and_is_idempotent()
        {
            Skip.IfNot(_fixture.Available, _fixture.UnavailableReason);
            const string db = "HoodParity_Converge";
            try
            {
                ProvisionFresh(db);

                // Re-create the pre-convergence 6.x shape: Anonymous NOT NULL.
                Exec(
                    db,
                    "UPDATE [AspNetUsers] SET [Anonymous]=0 WHERE [Anonymous] IS NULL;"
                        + "ALTER TABLE [AspNetUsers] ALTER COLUMN [Anonymous] bit NOT NULL;"
                );
                Assert.Equal(
                    "NO",
                    Scalar<string>(
                        db,
                        "SELECT IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='AspNetUsers' AND COLUMN_NAME='Anonymous'"
                    )
                );

                var converge = ReadEmbedded(
                    "Hood.Core.SchemaScripts.07.00.00/202606131526-converge.sql"
                );
                Exec(db, converge);
                Assert.Equal(
                    "YES",
                    Scalar<string>(
                        db,
                        "SELECT IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='AspNetUsers' AND COLUMN_NAME='Anonymous'"
                    )
                );

                // Idempotent: a second application is a clean no-op.
                Exec(db, converge);
                Assert.Equal(
                    "YES",
                    Scalar<string>(
                        db,
                        "SELECT IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='AspNetUsers' AND COLUMN_NAME='Anonymous'"
                    )
                );
            }
            finally
            {
                DropDatabase(db);
            }
        }
    }
}
