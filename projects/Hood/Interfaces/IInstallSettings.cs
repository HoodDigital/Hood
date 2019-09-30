namespace Hood.Interfaces
{
    public interface IInstallSettings
    {
        bool DatabaseConnectionFailed { get; set; }
        bool DatabaseMigrationsMissing { get; set; }
        bool DatabaseSeedFailed { get; set; }
        bool MigrationNotApplied { get; set; }
        bool AdminUserSetupError { get; set; }
        bool Installed { get; }
    }
}
