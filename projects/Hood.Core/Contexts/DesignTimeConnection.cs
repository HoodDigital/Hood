using System;

namespace Hood.Contexts
{
    /// <summary>
    /// Single source of truth for the design-time (dotnet ef ...) connection string used by the
    /// <c>IDesignTimeDbContextFactory</c> implementations. Reads <c>HOOD_DESIGNTIME_CONNECTION</c>
    /// when set, otherwise falls back to a local SQLEXPRESS default.
    /// <para>
    /// <c>TrustServerCertificate=True</c> is required because Microsoft.Data.SqlClient 5.x+ (pulled by
    /// EF Core 7+) defaults <c>Encrypt=true</c>; without it, design-time connections to a local server
    /// with no valid TLS certificate fail (HOOD-48 breaking-change #9).
    /// </para>
    /// </summary>
    internal static class DesignTimeConnection
    {
        public static string ConnectionString =>
            Environment.GetEnvironmentVariable("HOOD_DESIGNTIME_CONNECTION")
            ?? "Server=localhost\\SQLEXPRESS;Database=Hood.Web;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";
    }
}
