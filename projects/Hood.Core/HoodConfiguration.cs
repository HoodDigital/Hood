using Hood.Models;
using Microsoft.Extensions.Logging;

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
        public LogLevel LogLevel { get; set; }
        public bool ScheduledTasks { get; set; }
        public bool BypassCDN { get; set; }
    }
}