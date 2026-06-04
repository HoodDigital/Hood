using System;
using System.Collections.Generic;
using System.Linq;
using DbUp;

namespace Hood.Services
{
    public class DatabaseDeployResult
    {
        public bool Successful { get; set; }
        public List<string> ScriptsApplied { get; set; } = new List<string>();
        public string Error { get; set; }
    }

    /// <summary>
    /// Applies Hood's v7 schema (and optionally a consuming project's own SQL) to a database via DbUp.
    /// Idempotent and forward-only: DbUp journals every applied script in <c>dbo.SchemaVersions</c> and
    /// never re-runs one, so re-running the deploy is a clean no-op. No EF migration-history table is
    /// used. Hood's core scripts ship embedded in this assembly (contexts -> v6→v7 update -> views).
    /// </summary>
    public class DatabaseDeployService
    {
        public const string CoreScriptPrefix = "Hood.Core.SchemaScripts.";

        /// <summary>
        /// Apply the schema. Creates the database if it doesn't exist, runs Hood's core scripts, then
        /// (optionally) the consumer's scripts from <paramref name="consumerScriptsPath"/> — both
        /// journalled in the same <c>SchemaVersions</c> table, so the consumer's SQL always runs after Hood's.
        /// </summary>
        public DatabaseDeployResult Deploy(
            string connectionString,
            string consumerScriptsPath = null,
            Action<string> log = null
        )
        {
            log ??= Console.WriteLine;

            if (string.IsNullOrWhiteSpace(connectionString))
                return new DatabaseDeployResult
                {
                    Successful = false,
                    Error = "No connection string was supplied.",
                };

            try
            {
                log("Ensuring database exists...");
                EnsureDatabase.For.SqlDatabase(connectionString);

                var applied = new List<string>();

                // 1) Hood core schema — embedded, ordered by LogicalName.
                log("Applying Hood core schema...");
                var coreResult = DeployChanges
                    .To.SqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(
                        typeof(DatabaseDeployService).Assembly,
                        n => n.StartsWith(CoreScriptPrefix)
                    )
                    .JournalToSqlTable("dbo", "SchemaVersions")
                    .WithTransactionPerScript()
                    .LogToConsole()
                    .Build()
                    .PerformUpgrade();

                if (!coreResult.Successful)
                    return new DatabaseDeployResult
                    {
                        Successful = false,
                        Error = coreResult.Error?.Message ?? "Core schema upgrade failed.",
                    };
                applied.AddRange(coreResult.Scripts.Select(s => s.Name));

                // 2) Consumer project SQL — runs after Hood's core, same journal.
                if (!string.IsNullOrWhiteSpace(consumerScriptsPath))
                {
                    log($"Applying consumer scripts from {consumerScriptsPath}...");
                    var consumerResult = DeployChanges
                        .To.SqlDatabase(connectionString)
                        .WithScriptsFromFileSystem(consumerScriptsPath)
                        .JournalToSqlTable("dbo", "SchemaVersions")
                        .WithTransactionPerScript()
                        .LogToConsole()
                        .Build()
                        .PerformUpgrade();

                    if (!consumerResult.Successful)
                        return new DatabaseDeployResult
                        {
                            Successful = false,
                            Error =
                                consumerResult.Error?.Message ?? "Consumer schema upgrade failed.",
                        };
                    applied.AddRange(consumerResult.Scripts.Select(s => s.Name));
                }

                return new DatabaseDeployResult { Successful = true, ScriptsApplied = applied };
            }
            catch (Exception ex)
            {
                return new DatabaseDeployResult { Successful = false, Error = ex.Message };
            }
        }
    }
}
