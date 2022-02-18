using Hood.Core;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace Hood.Extensions
{
    public static class IConfigurationExtensions
    {
        public static bool IsDatabaseConnected(this IConfiguration config)
        {
            return config.CheckConfiguration("ConnectionString", "ConnectionStrings:DefaultConnection");
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
