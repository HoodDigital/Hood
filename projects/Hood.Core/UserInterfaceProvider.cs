using Hood.Core;
using Hood.Models;
using Hood.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using System.Collections.Generic;
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
            if (provider == null)
            {
                return new List<string>().ToArray();
            }
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
