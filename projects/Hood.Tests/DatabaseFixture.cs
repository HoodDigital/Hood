using System;
using Hood.Contexts;
using Hood.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hood.Tests
{
    /// <summary>
    /// Shared fixture for the database-backed integration tests. Resolves a connection string from
    /// HOOD_TEST_CONNECTION (falling back to the local Docker dev SQL), probes connectivity once, and
    /// builds contexts against it. Tests use <see cref="Available"/> with SkippableFact so a machine
    /// without the database simply skips these tests rather than failing the whole run; in CI (where the
    /// database is provisioned) they execute.
    /// </summary>
    public class DatabaseFixture
    {
        public string ConnectionString { get; }
        public bool Available { get; }
        public string UnavailableReason { get; }

        public DatabaseFixture()
        {
            ConnectionString =
                Environment.GetEnvironmentVariable("HOOD_TEST_CONNECTION")
                ?? "Server=localhost,14331;Database=Hood.Web;User Id=sa;Password=Hood_Dev_Passw0rd!;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True";

            try
            {
                using var probe = NewHoodDb();
                Available = probe.Database.CanConnect();
                if (!Available)
                {
                    UnavailableReason = "Database is not reachable at the configured connection.";
                }
            }
            catch (Exception ex)
            {
                Available = false;
                UnavailableReason = $"Database probe failed: {ex.Message}";
            }
        }

        private DbContextOptions<T> Options<T>()
            where T : DbContext =>
            new DbContextOptionsBuilder<T>().UseSqlServer(ConnectionString).Options;

        public HoodDbContext NewHoodDb() => new(Options<HoodDbContext>());

        public ContentContext NewContent() => new(Options<ContentContext>());

        public PropertyContext NewProperty() => new(Options<PropertyContext>());

        public IdentityContext NewIdentity() => new(Options<IdentityContext>());

        public Auth0IdentityContext NewAuth0Identity() => new(Options<Auth0IdentityContext>());
    }

    [CollectionDefinition("Database")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }
}
