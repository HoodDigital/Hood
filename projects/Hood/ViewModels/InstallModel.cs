namespace Hood.ViewModels
{
    public class InstallModel
    {
        public InstallModel()
        {
        }

        public bool ViewsInstalled { get; internal set; }
        public bool DatabaseConfigured { get; internal set; }
        public bool DatabaseSeeded { get; internal set; }
        public bool DatabaseMigrated { get; internal set; }
    }
}