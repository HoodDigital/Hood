using Microsoft.Extensions.Logging;

namespace Hood.Core
{
    /// <summary>
    /// Binds the optional <c>Hood</c> configuration section. Every member here defaults sensibly, so
    /// a consumer only needs to set the ones they're overriding — see <see cref="Engine.SiteOwnerEmail"/>
    /// for <see cref="SuperAdminEmail"/> and <see cref="Engine.Resource(string)"/> for the CDN settings.
    /// </summary>
    public class HoodConfiguration
    {
        public HoodConfiguration()
        {
            InitializeOnStartup = false;
            Integrations = new Integrations();
        }

        /// <summary>
        /// Optional override for the site owner's email — leave unset and it comes from the account
        /// created during <c>/install</c> instead.
        /// </summary>
        public string SuperAdminEmail { get; set; }
        public bool InitializeOnStartup { get; set; }
        public LogLevel LogLevel { get; set; }

        /// <summary>Serve Hood's admin/UI assets from the app's own wwwroot instead of the CDN.</summary>
        public bool BypassCDN { get; set; }

        /// <summary>Overrides the CDN base (host + package); Hood still appends <c>@{version}</c>.</summary>
        public string CdnPath { get; set; }

        /// <summary>A complete CDN base URL used verbatim — Hood appends no version segment.</summary>
        public string CdnFullPath { get; set; }
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

    /// <summary>
    /// Binds the optional <c>Identity:Auth0</c> configuration section. Leave the whole section absent
    /// (or <see cref="Domain"/>/<see cref="ClientId"/> unset) and Hood falls back to the standard
    /// ASP.NET Identity/password backend — nothing here is required.
    /// </summary>
    public class Auth0Configuration
    {
        public string Domain { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public bool SetupRemoteOnIntitialize { get; set; }
    }
}
