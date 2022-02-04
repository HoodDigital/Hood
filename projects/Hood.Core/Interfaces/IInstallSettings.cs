using System;

namespace Hood.Interfaces
{
    public interface IInstallSettings
    {
        bool DatabaseConnectionFailed { get; set; }
        bool DatabaseMigrationsMissing { get; set; }
        bool DatabaseSeedFailed { get; set; }
        bool MigrationNotApplied { get; set; }
        bool ViewsInstalled { get; set; }
        bool AdminUserSetupError { get; set; }
        bool DatabaseMediaTimeout { get; set; }
        bool Installed { get; }
        Exception Details { get; set; }
    }
}
