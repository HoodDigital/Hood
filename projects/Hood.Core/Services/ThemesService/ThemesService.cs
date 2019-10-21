using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Hood.Core;

namespace Hood.Services
{
    public class ThemesService : IThemesService
    {
        private Dictionary<string, IConfiguration> _configs;
        private readonly IHostingEnvironment _env;
        public static object scriptLock = new object();

        public ThemesService(IHostingEnvironment env)
        {
            _env = env;
            Reload();
        }

        public void Reload()
        {
            _configs = new Dictionary<string, IConfiguration>();
            string[] themeDirs = { };
            if (System.IO.Directory.Exists(_env.ContentRootPath + "\\Themes\\"))
                themeDirs = themeDirs.Concat(System.IO.Directory.GetDirectories(_env.ContentRootPath + "\\Themes\\")).ToArray();

            foreach (string theme in themeDirs)
            {
                if (System.IO.File.Exists(theme + "/config.json"))
                {
                    var name = theme.Remove(0, theme.LastIndexOf('\\') + 1).ToLower();
                    var builder = new ConfigurationBuilder().SetBasePath(_env.ContentRootPath + "\\themes\\" + name).AddJsonFile("config.json");
                    var configBuilt = builder.Build();
                    _configs.Add(name, configBuilt);
                }
            }

            if (!_configs.ContainsKey("default"))
            {
                IReadOnlyDictionary<string, string> defaultConfig = new Dictionary<string, string>()
                {
                    ["Name"] = "default",
                    ["FullName"] = "Default",
                    ["BaseColour"] = "#C33610",
                    ["Author"] = "Hood Digital",
                    ["Description"] = "The default site theme, uses the default Bootstrap4 UI.",
                    ["PreviewImage"] = "https://hood.azureedge.net/hood/hood-theme.jpg",
                    ["Public"] = "true",
                    ["UI"] = "Bootstrap4"
                };
                _configs.Add("default", new ConfigurationBuilder().AddInMemoryCollection(defaultConfig).Build());
            }
            if (!_configs.ContainsKey("bootstrap3"))
            {
                IReadOnlyDictionary<string, string> defaultConfig = new Dictionary<string, string>()
                {
                    ["Name"] = "bootstrap3",
                    ["FullName"] = "Default (Bootstrap 3)",
                    ["BaseColour"] = "#C33610",
                    ["Author"] = "Hood Digital",
                    ["Description"] = "The default site theme, uses the default Bootstrap3 UI.",
                    ["PreviewImage"] = "https://hood.azureedge.net/hood/hood-theme.jpg",
                    ["Public"] = "true",
                    ["UI"] = "Bootstrap3"
                };
                _configs.Add("default", new ConfigurationBuilder().AddInMemoryCollection(defaultConfig).Build());
            }
        }

        public string CurrentTheme
        {
            get
            {
                string loadTheme = Engine.Settings["Hood.Settings.Theme"];
                if (string.IsNullOrEmpty(loadTheme))
                {
                    Engine.Settings.Set("default", "Hood.Settings.Theme");
                }
                return Engine.Settings["Hood.Settings.Theme"];
            }
        }
        public Theme Current
        {
            get
            {
                if (_configs.ContainsKey(CurrentTheme))
                    return new Theme(_configs[CurrentTheme]);
                else
                    return Default;
            }
        }
        private Theme Default
        {
            get
            {
                IReadOnlyDictionary<string, string> defaultConfig = new Dictionary<string, string>()
                {
                    ["Name"] = "default",
                    ["FullName"] = "Default",
                    ["BaseColour"] = "#C33610",
                    ["Author"] = "Hood - Digital Architects.",
                    ["PreviewImage"] = "https://hood.azureedge.net/hood/hood-theme.jpg",
                    ["Public"] = "true"
                };
                return new Theme(new ConfigurationBuilder().AddInMemoryCollection(defaultConfig).Build());
            }
        }

        public bool SetTheme(string themeName)
        {
            try
            {
                if (Themes.Any(t => t.Name == themeName))
                {
                    Engine.Settings["Hood.Settings.Theme"] = themeName;
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Theme Get(string themeName)
        {
            return Themes.SingleOrDefault(t => t.Name == themeName);
        }

        public List<Theme> Themes
        {
            get
            {
                Reload();
                return _configs.Select(c => new Theme(c.Value)).ToList();
            }
        }

        public bool IsDefault { get { return CurrentTheme != "default"; } }
    }
}
