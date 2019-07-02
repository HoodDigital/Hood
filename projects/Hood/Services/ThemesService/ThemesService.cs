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
        public static object scriptLock = new object();

        public ThemesService(IHostingEnvironment env)
        {
            _configs = new Dictionary<string, IConfiguration>();

            IReadOnlyDictionary<string, string> defaultConfig = new Dictionary<string, string>()
            {
                ["Name"] = "default",
                ["FullName"] = "Default",
                ["BaseColour"] = "#C33610",
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

        public string CurrentTheme
        {
            get
            {
                string loadTheme = Engine.Settings["Hood.Settings.Theme"];
                if (string.IsNullOrEmpty(loadTheme))
                {
                    Engine.Settings.Set("Hood.Settings.Theme", "default");
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

        public List<Theme> Themes
        {
            get
            {
                return _configs.Select(c => new Theme(c.Value)).ToList();
            }
        }

        public bool IsDefault { get { return CurrentTheme != "default"; } }
    }
}
