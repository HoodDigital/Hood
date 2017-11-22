using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace Hood.Services
{
    public class ThemesService : IThemesService
    {
        private ISettingsRepository _settingsConfig;
        private Dictionary<string, IConfiguration> _configs;
        public static object scriptLock = new object();

        public ThemesService(IHostingEnvironment env, ISettingsRepository config, IConfiguration mainConfig)
        {
            _settingsConfig = config;

            _configs = new Dictionary<string, IConfiguration>();

            IReadOnlyDictionary<string, string> defaultConfig = new Dictionary<string, string>()
            {
                ["Name"] = "default",
                ["ThemeFullName"] = "Default",
                ["ThemeBaseColour"] = "#C33610",
                ["Author"] = "Hood - Digital Architects.",
                ["PreviewImage"] = "https://hood.azureedge.net/hood/hood-theme.jpg",
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
                string loadTheme = _settingsConfig["Hood.Settings.Theme"];
                if (string.IsNullOrEmpty(loadTheme))
                {
                    _settingsConfig.Set("Hood.Settings.Theme", "default");
                }
                return _settingsConfig["Hood.Settings.Theme"];
            }
        }

        public bool SetTheme(string themeName)
        {
            try
            {
                if (ThemeConfigs.ContainsKey(themeName))
                {
                    _settingsConfig["Hood.Settings.Theme"] = themeName;
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
