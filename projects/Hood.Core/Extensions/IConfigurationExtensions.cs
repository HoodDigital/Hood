using Hood.Core;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace Hood.Extensions
{
    public static class IConfigurationExtensions
    {
        /// <summary>
        /// Call to ConfigureHood to check all necessary elements are in the configuration. Installation page will show if not all configuration is in place.
        /// </summary>
        /// <param name="config"></param>
        public static bool IsDatabaseConfigured(this IConfiguration config)
        {
            if (Engine.Services.DatabaseConnectionFailed)
            {
                return false;
            }

            if (Engine.Services.DatabaseMigrationsMissing)
            {
                return false;
            }

            if (Engine.Services.MigrationNotApplied)
            {
                return false;
            }

            if (Engine.Services.DatabaseSeedFailed)
            {
                return false;
            }

            return config.IsDatabaseConnected();
        }
        public static bool IsDatabaseConnected(this IConfiguration config)
        {
            return config.CheckConfiguration("ConnectionString", "ConnectionStrings:DefaultConnection");
        }
        public static bool IsApplicationInsightsConfigured(this IConfiguration config)
        {
            return config.CheckConfiguration("ApplicationInsights", "ApplicationInsights:Key");
        }
        public static bool IsFacebookConfigured(this IConfiguration config)
        {
            return config.CheckConfiguration("Facebook", "Authentication:Facebook:AppId", "Authentication:Facebook:Secret");
        }
        public static bool IsGoogleConfigured(this IConfiguration config)
        {
            return config.CheckConfiguration("Google", "Authentication:Google:AppId", "Authentication:Google:Secret");
        }

        public static SqlConnectionStringBuilder GetConnectionSettings(this IConfiguration config)
        {
            string conString = "SERVER=localhost;DATABASE=tree;UID=root;PASSWORD=branch;Min Pool Size = 0;Max Pool Size=200";
            if (config.IsDatabaseConnected())
            {
                conString = config["ConnectionStrings:DefaultConnection"];
            }

            return new SqlConnectionStringBuilder(conString);
        }

        private static bool CheckConfiguration(this IConfiguration config, string flag, params string[] keys)
        {
            foreach (string key in keys)
            {
                if (string.IsNullOrEmpty(config[key]))
                {
                    return false;
                }
            }
            config[flag] = "true";
            return true;
        }

        public static bool IsConfigured(this IConfiguration config, string key)
        {
            if (!string.IsNullOrEmpty(config[key]))
            {
                return true;
            }

            return false;
        }

    }
}
