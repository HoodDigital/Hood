namespace Hood.Core
{
    public class HoodConfiguration
    {
        public HoodConfiguration()
        {
            SeedOnStart = false;
        }

        public string SuperAdminEmail { get; set; }
        public string LibraryFolder { get; set; }
        public string UI { get; set; }
        public bool SeedOnStart { get; set; }
    }
}