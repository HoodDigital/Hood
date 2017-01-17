using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace Hood.Services
{
    public class ThemesService : IThemesService
    {
        private ISiteConfiguration _siteConfig;
        private Dictionary<string, IConfiguration> _configs;
        public static object scriptLock = new object();

        public ThemesService(IHostingEnvironment env, ISiteConfiguration config, IConfiguration mainConfig)
        {
            _siteConfig = config;

            _configs = new Dictionary<string, IConfiguration>();

            IReadOnlyDictionary<string, string> defaultConfig = new Dictionary<string, string>()
            {
                ["Name"] = "default",
                ["ThemeFullName"] = "Default Bootstrap Theme",
                ["ThemeBaseColour"] = "#C33610",
                ["Author"] = "Hood - Digital Architects.",
                ["PreviewImage"] = "https://hood.blob.core.windows.net/hood/bootstrap.png",
                ["Public"] = "true"
            };
            _configs.Add("default", new ConfigurationBuilder().AddInMemoryCollection(defaultConfig).Build());

            string[] themeDirs = { };
            if (System.IO.Directory.Exists(env.ContentRootPath + "\\Themes\\"))
                themeDirs = themeDirs.Concat(System.IO.Directory.GetDirectories(env.ContentRootPath + "\\Themes\\")).ToArray();

            foreach (string theme in themeDirs)
            {
                if (System.IO.File.Exists(theme + "/config.json"))
                {
                    var name = theme.Remove(0, theme.LastIndexOf('\\') + 1).ToLower();
                    var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath + "\\themes\\" + name).AddJsonFile("config.json");
                    var configBuilt = builder.Build();
                    _configs.Add(name, configBuilt);
                }
            }

        }

        public List<string> Themes
        {
            get
            {
                return _configs.Keys.ToList();
            }
        }

        public IConfiguration Config(string themeName)
        {
            if (_configs.ContainsKey(themeName))
                return _configs[themeName];
            return null;
        }

        public string CurrentTheme
        {
            get
            {
                string loadTheme = _siteConfig["Hood.Settings.Theme"];
                if (string.IsNullOrEmpty(loadTheme))
                {
                    _siteConfig.Set("Hood.Settings.Theme", "default");
                }
                return _siteConfig["Hood.Settings.Theme"];
            }
        }

        public bool SetTheme(string themeName)
        {
            try
            {
                if (ThemeConfigs.ContainsKey(themeName))
                {
                    _siteConfig["Hood.Settings.Theme"] = themeName;
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Dictionary<string, IConfiguration> ThemeConfigs
        {
            get
            {
                return _configs;
            }
        }

        public IConfiguration Current
        {
            get
            {
                return Config(CurrentTheme);
            }
        }
    }
}
