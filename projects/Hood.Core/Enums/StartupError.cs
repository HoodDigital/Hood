namespace Hood.Enums
{
    public enum StartupError
    {
        NoConnectionString,
        MigrationMissing,
        MigrationNotApplied,
        DatabaseConnectionFailed,
        DatabaseMediaTimeout,
        DatabaseSeedFailed,
        AdminUserSetupError,
        DatabaseMediaError,
        DatabaseViewsNotInstalled,
        StartupError,
        Auth0Issue,
        DatabaseNotSeeded
    }
}
