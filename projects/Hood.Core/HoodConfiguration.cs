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
        public bool SeedOnStart { get; set; }
        public LogLevel LogLevel { get; set; }
        public bool BypassCDN { get; set; }
    }
}