using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hood.Core;
using Hood.Extensions;
using Hood.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Hood
{
    public static class UserInterfaceProvider
    {
        public const string Bootstrap3Assembly = "Hood.UI.Bootstrap3";
        public const string Bootstrap4Assembly = "Hood.UI.Bootstrap4";

        /// <summary>
        /// Resolves the active UI flavour assembly name at startup time — before the engine is
        /// built — replicating GetProvider's precedence (theme.UI overrides Hood:UI config). The
        /// theme name is read straight from the database; if the database isn't available yet
        /// (pre-install) the configuration value wins. Returns null when no bootstrap flavour
        /// is configured — the stock Core (Bootstrap 5) UI serves everything in that case.
        /// </summary>
        public static string GetActiveUIAssembly(IConfiguration config, IWebHostEnvironment env)
        {
            string ui = config["Hood:UI"];
            try
            {
                string themeName = null;
                string connectionString = config["ConnectionStrings:DefaultConnection"];
                if (connectionString.IsSet())
                {
                    using var connection = new SqlConnection(connectionString);
                    connection.Open();
                    using var command = new SqlCommand(
                        "SELECT [Value] FROM [HoodOptions] WHERE [Id] = 'Hood.Settings.Theme'",
                        connection
                    );
                    themeName = (command.ExecuteScalar() as string)?.Trim('"');
                }
                if (themeName.IsSet())
                {
                    Theme theme = new ThemesService(env).Get(themeName);
                    if (theme != null && theme.UI.IsSet())
                    {
                        ui = theme.UI;
                    }
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause — pre-install / unreachable database
            // probe; the configuration fallback below is the intended behaviour.
            catch { }
            return ui switch
            {
                "Bootstrap3" => Bootstrap3Assembly,
                "Bootstrap4" => Bootstrap4Assembly,
                _ => null,
            };
        }

        /// <summary>
        /// Removes the inactive bootstrap flavours' application parts so only the active
        /// flavour's compiled /UI/* views participate in view resolution — or both flavours
        /// when none is configured (the stock Core UI). Switching flavour (changing theme UI)
        /// requires an application restart (HOOD-54).
        /// </summary>
        public static void FilterInactiveUI(
            ApplicationPartManager partManager,
            IConfiguration config,
            IWebHostEnvironment env
        )
        {
            string active = GetActiveUIAssembly(config, env);
            string[] flavours = { Bootstrap3Assembly, Bootstrap4Assembly };
            foreach (
                ApplicationPart part in partManager
                    .ApplicationParts.Where(p => flavours.Contains(p.Name) && p.Name != active)
                    .ToList()
            )
            {
                partManager.ApplicationParts.Remove(part);
            }
        }

        /// <summary>
        /// Gets a directory listing from the embedded files in the Hood assembly.
        /// WARNING, this should only be used for loading files in known definite locations. as the
        /// file provider uses a flat structure, sub-directories will be returned along with the files.
        /// </summary>
        /// <param name="basePath">The base path in the form ~/path/of/the/file.extension</param>
        /// <returns></returns>
        public static string[] GetFiles(string basePath)
        {
            basePath = ReWritePath(basePath);
            EmbeddedFileProvider provider = GetProvider(Engine.Services.Resolve<IConfiguration>());
            if (provider == null)
            {
                return new List<string>().ToArray();
            }
            IDirectoryContents contents = provider.GetDirectoryContents("");
            IEnumerable<IFileInfo> dir = contents.Where(p => p.Name.StartsWith(basePath));
            return dir.Select(f => f.Name.Replace(basePath, "")).ToArray();
        }

        public static EmbeddedFileProvider GetProvider(IConfiguration config)
        {
            // load the theme from the options.
            string ui = config["Hood:UI"];

            IThemesService themeService = Engine.Services.Resolve<IThemesService>();
            Theme theme = themeService.Current;
            if (theme != null)
            {
                ui = theme.UI;
            }

            switch (ui)
            {
                case "Bootstrap3":
                    var bs3Assembly = Engine.ResolveUI("Hood.UI.Bootstrap3");
                    if (bs3Assembly == null)
                        return null;
                    return new EmbeddedFileProvider(bs3Assembly, "Hood.UI.Bootstrap3");
                case "Bootstrap4":
                    var bs4Assembly = Engine.ResolveUI("Hood.UI.Bootstrap4");
                    if (bs4Assembly == null)
                        return null;
                    return new EmbeddedFileProvider(bs4Assembly, "Hood.UI.Bootstrap4");
            }
            return null;
        }

        public static string ReWritePath(string basePath, bool isDirectory = false)
        {
            if (basePath.StartsWith("~"))
            {
                basePath = basePath.TrimStart('~');
            }

            if (!basePath.EndsWith("/") && isDirectory)
            {
                basePath = basePath + "/";
            }

            basePath = basePath.Replace("/", ".");
            basePath = basePath.Replace("-", "_");

            if (basePath.StartsWith("."))
            {
                basePath = basePath.TrimStart('.');
            }

            return basePath;
        }

        public static string ReadAllText(string path)
        {
            path = ReWritePath(path);
            EmbeddedFileProvider provider = GetProvider(Engine.Services.Resolve<IConfiguration>());
            IFileInfo file = provider.GetFileInfo(path);
            Stream contents = file.CreateReadStream();
            StreamReader s = new StreamReader(contents);
            return s.ReadToEnd();
        }
    }
}
