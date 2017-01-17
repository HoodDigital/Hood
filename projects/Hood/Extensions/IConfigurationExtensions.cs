using Microsoft.Extensions.Configuration;

namespace Hood.Extensions
{
    public static class IConfigurationExtensions
    {
        public static bool ConfigSetup(this IConfiguration config, string flag, params string[] keys)
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

        public static bool CheckSetup(this IConfiguration config, string key)
        {
            if (!string.IsNullOrEmpty(config[key]) && config[key] == "true")
                return true;
            return false;
        }

    }
}
