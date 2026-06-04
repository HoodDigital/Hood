using System;
using Hood.Services;

namespace Hood.SchemaTool
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length == 0 || IsHelp(args[0]))
            {
                PrintUsage();
                return args.Length == 0 ? 2 : 0;
            }

            string command = args[0].ToLowerInvariant();
            string connection = GetArg(args, "--connection", "-c") ?? Environment.GetEnvironmentVariable("HOOD_CONNECTION");
            string scripts = GetArg(args, "--scripts", "-s");

            switch (command)
            {
                case "upgrade":
                    if (string.IsNullOrWhiteSpace(connection))
                    {
                        Console.Error.WriteLine("error: a connection string is required (--connection or HOOD_CONNECTION).");
                        return 2;
                    }

                    DatabaseDeployResult result = new DatabaseDeployService().Deploy(connection, scripts);

                    if (result.Successful)
                    {
                        Console.WriteLine($"\nDone. {result.ScriptsApplied.Count} script(s) applied (already-applied scripts were skipped).");
                        return 0;
                    }

                    Console.Error.WriteLine($"\nFailed: {result.Error}");
                    return 1;

                default:
                    Console.Error.WriteLine($"error: unknown command '{command}'.");
                    PrintUsage();
                    return 2;
            }
        }

        private static bool IsHelp(string arg) =>
            arg is "-h" or "--help" or "help" or "-?" or "/?";

        private static string GetArg(string[] args, params string[] names)
        {
            for (int i = 0; i < args.Length - 1; i++)
                foreach (string name in names)
                    if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                        return args[i + 1];
            return null;
        }

        private static void PrintUsage()
        {
            Console.WriteLine(@"hood-schema — apply the Hood CMS database schema (idempotent, forward-only).

Usage:
  hood-schema upgrade --connection ""<sql-connection-string>"" [--scripts <consumer-sql-folder>]

Options:
  --connection, -c   SQL Server connection string (or set the HOOD_CONNECTION env var).
  --scripts,    -s   Optional folder of the consuming project's own .sql scripts, applied
                     after Hood's core schema and journalled in the same SchemaVersions table.

The target database is created if it doesn't exist. Re-running is a clean no-op: DbUp records
every applied script in dbo.SchemaVersions and never re-runs one. No EF migrations are involved.");
        }
    }
}
