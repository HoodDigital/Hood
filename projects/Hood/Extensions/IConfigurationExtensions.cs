using Microsoft.Extensions.Configuration;

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
            if (!string.IsNullOrEmpty(config[key]) && config[key] == "true")
                return true;
            return false;
        }

        public static bool ForceHttps(this IConfiguration config)
        {
            if (!string.IsNullOrEmpty(config["UseHttps"]) && config["UseHttps"] == "true")
                return true;
            return false;
        }

    }
}
