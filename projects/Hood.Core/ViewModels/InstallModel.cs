using System;
using Hood.Interfaces;

namespace Hood.ViewModels
{
    public class InstallModel : IInstallSettings
    {
        public InstallModel()
        {
        }

        public bool ViewsInstalled { get; set; } = true;
        public bool DatabaseConfigured { get; set; } = true;
        public bool DatabaseConnectionFailed { get; set; } = false;
        public bool DatabaseMigrationsMissing { get; set; } = false;
        public bool DatabaseSeedFailed { get; set; } = false;
        public bool MigrationNotApplied { get; set; } = false;
        public bool DatabaseMediaTimeout { get; set; } = false;
        public bool SqlNotInstalled { get; set; } = false;
        public bool AdminUserSetupError { get; set; } = false;
        public bool Installed {
            get
            {
                return ViewsInstalled &&
                    DatabaseConfigured &&
                    !DatabaseConnectionFailed &&
                    !DatabaseMigrationsMissing &&
                    !DatabaseSeedFailed &&
                    !MigrationNotApplied &&
                    !AdminUserSetupError;
            }
        }

        public Exception Details { get; set; }
    }
}