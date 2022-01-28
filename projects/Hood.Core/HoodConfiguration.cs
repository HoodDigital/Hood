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
        public bool ApplyMigrationsAutomatically { get; set; }
        public LogLevel LogLevel { get; set; }
        public bool BypassCDN { get; set; }
    }    
    
    public class Auth0Configuration
    {
        public Auth0Configuration()
        {
        }
        public string Domain { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ApiClient { get; set; }
        public string ApiSecret { get; set; }
    }
}