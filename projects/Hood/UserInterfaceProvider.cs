using Hood.Core;
using Hood.Models;
using Hood.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Hood
{
    public static class UserInterfaceProvider
    {
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
            IDirectoryContents contents = provider.GetDirectoryContents("");
            System.Collections.Generic.IEnumerable<IFileInfo> dir = contents.Where(p => p.Name.StartsWith(basePath));
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
                    return new EmbeddedFileProvider(
                      typeof(Hood.UI.Bootstrap3.Component).Assembly,
                      "Hood.UI.Bootstrap3"
                  );
                case "Bootstrap4":
                    return new EmbeddedFileProvider(
                      typeof(Hood.UI.Bootstrap4.Component).Assembly,
                      "Hood.UI.Bootstrap4"
                  );
            }
            return null;
        }

        public static EmbeddedFileProvider GetAdminProvider()
        {

            // Get a reference to the assembly that contains the view components
            Assembly assembly = typeof(UserInterfaceProvider).Assembly;
            // Create an EmbeddedFileProvider for that assembly
            EmbeddedFileProvider embeddedFileProvider = new EmbeddedFileProvider(
                assembly,
                "Hood"
            );
            return embeddedFileProvider;
        }

        public static EmbeddedFileProvider GetAccountProvider()
        {

            // Get a reference to the assembly that contains the view components
            Assembly assembly = typeof(Hood.UI.Core.Component).Assembly;
            // Create an EmbeddedFileProvider for that assembly
            EmbeddedFileProvider embeddedFileProvider = new EmbeddedFileProvider(
                assembly,
                "Hood.UI.Core"
            );
            return embeddedFileProvider;
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
