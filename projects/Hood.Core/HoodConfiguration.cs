using Hood.Models;
using Microsoft.Extensions.Logging;

namespace Hood.Core
{
    public class HoodConfiguration
    {
        public HoodConfiguration()
        {
            InitializeOnStartup = false;
            Integrations = new Integrations();
        }

        public string SuperAdminEmail { get; set; }
        public bool InitializeOnStartup { get; set; }
        public bool ApplyMigrationsAutomatically { get; set; }
        public LogLevel LogLevel { get; set; }
        public bool BypassCDN { get; set; }
        public string CdnPath { get; set; }
        public Integrations Integrations { get; set; }
    }

    public class Integrations
    {
        public Integrations()
        {
            TinyMCE = "no-api-key";
        }
        public string TinyMCE { get; set; }
    }

    public class Auth0Configuration
    {
        public Auth0Configuration()
        {
        }
        public string Domain { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public bool SetupRemoteOnIntitialize { get; set; }
    }
}