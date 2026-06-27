using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Hood.Core;
using Hood.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.FileProviders;

namespace Hood.Services
{
    public class TemplateProvider : ITemplateProvider
    {
        private readonly ApplicationPartManager _partManager;
        private readonly IWebHostEnvironment _env;

        public TemplateProvider(ApplicationPartManager partManager, IWebHostEnvironment env)
        {
            _partManager = partManager;
            _env = env;
        }

        /// <summary>
        /// The active theme name, when the engine is far enough up to know it.
        /// </summary>
        private static string ActiveTheme
        {
            get
            {
                if (!Engine.Services.Installed || Engine.Settings == null)
                {
                    return null;
                }
                return Engine.Settings["Hood.Settings.Theme"];
            }
        }

        public Dictionary<string, string> GetTemplates(string templateFolder)
        {
            var templates = new Dictionary<string, string>();
            string theme = ActiveTheme;

            // 1. Physical app/theme templates — consumers running with raw views on disk and
            //    the in-repo dev loop. IFileProvider keeps this cross-platform (the previous
            //    implementation built paths with hard-coded '\' separators and silently failed
            //    on Linux).
            foreach (string relativeDir in PhysicalTemplateFolders(templateFolder, theme))
            {
                foreach (
                    IFileInfo file in _env.ContentRootFileProvider.GetDirectoryContents(relativeDir)
                )
                {
                    if (!file.IsDirectory && file.Name.EndsWith(".cshtml"))
                    {
                        AddTemplate(templates, file.Name);
                    }
                }
            }

            // 2. Compiled views — the consumer app's precompiled views and Hood's UI packages
            //    (the inactive bootstrap flavour's part is removed at startup, so its templates
            //    never appear). No files on the server required.
            var feature = new ViewsFeature();
            _partManager.PopulateFeature(feature);
            foreach (CompiledViewDescriptor descriptor in feature.ViewDescriptors)
            {
                if (IsTemplatePath(descriptor.RelativePath, templateFolder, theme))
                {
                    AddTemplate(templates, Path.GetFileName(descriptor.RelativePath));
                }
            }

            return templates.OrderBy(t => t.Key).ToDictionary(t => t.Key, t => t.Value);
        }

        public string ReadTemplateSource(string templateFolder, string templateName)
        {
            string fileName = $"{templateName}.cshtml";

            // App / active theme physical sources first — mirrors view resolution precedence.
            foreach (string relativeDir in PhysicalTemplateFolders(templateFolder, ActiveTheme))
            {
                IFileInfo file = _env.ContentRootFileProvider.GetFileInfo(
                    $"{relativeDir}/{fileName}"
                );
                if (file.Exists && !file.IsDirectory)
                {
                    using var reader = new StreamReader(file.CreateReadStream());
                    return reader.ReadToEnd();
                }
            }

            // Packaged template sources — embedded in the UI packages alongside the compiled
            // views, specifically so this parser can read them.
            string flavour = _partManager
                .ApplicationParts.Select(p => p.Name)
                .FirstOrDefault(n =>
                    n == UserInterfaceProvider.Bootstrap3Assembly
                    || n == UserInterfaceProvider.Bootstrap4Assembly
                );
            var packagedSources = new List<(string assemblyName, string path)>();
            if (flavour != null)
            {
                packagedSources.Add((flavour, $"UI/{templateFolder}/{fileName}"));
            }
            packagedSources.Add(("Hood.UI.Core", $"BaseUI/{templateFolder}/{fileName}"));

            foreach ((string assemblyName, string path) in packagedSources)
            {
                Assembly assembly = FindPartAssembly(assemblyName);
                if (assembly == null)
                {
                    continue;
                }
                var provider = new EmbeddedFileProvider(assembly, assemblyName);
                IFileInfo file = provider.GetFileInfo(path);
                if (file.Exists)
                {
                    using var reader = new StreamReader(file.CreateReadStream());
                    return reader.ReadToEnd();
                }
            }

            return null;
        }

        /// <summary>
        /// The content-root-relative folders that may hold physical templates, in precedence
        /// order: app views, active theme views, legacy app UI folder.
        /// </summary>
        internal static IEnumerable<string> PhysicalTemplateFolders(
            string templateFolder,
            string theme
        )
        {
            yield return $"Views/{templateFolder}";
            if (theme.IsSet())
            {
                yield return $"Themes/{theme}/Views/{templateFolder}";
            }
            yield return $"UI/{templateFolder}";
        }

        /// <summary>
        /// True when a compiled view path is a selectable template for the folder: packaged
        /// (/UI/, /BaseUI/), app (/Views/) or the active theme. Other themes' compiled views
        /// are excluded.
        /// </summary>
        internal static bool IsTemplatePath(string viewPath, string templateFolder, string theme)
        {
            if (viewPath == null || !viewPath.EndsWith(".cshtml"))
            {
                return false;
            }
            if (
                viewPath.StartsWith($"/UI/{templateFolder}/")
                || viewPath.StartsWith($"/BaseUI/{templateFolder}/")
                || viewPath.StartsWith($"/Views/{templateFolder}/")
            )
            {
                return true;
            }
            return theme.IsSet() && viewPath.StartsWith($"/Themes/{theme}/Views/{templateFolder}/");
        }

        internal static void AddTemplate(Dictionary<string, string> templates, string fileName)
        {
            string key = Path.GetFileNameWithoutExtension(fileName);
            if (!templates.ContainsKey(key))
            {
                templates.Add(key, key.TrimStart('_').Replace("_", " ").ToTitleCase());
            }
        }

        private Assembly FindPartAssembly(string name)
        {
            foreach (ApplicationPart part in _partManager.ApplicationParts)
            {
                if (part.Name != name)
                {
                    continue;
                }
                switch (part)
                {
                    case AssemblyPart assemblyPart:
                        return assemblyPart.Assembly;
                    case CompiledRazorAssemblyPart razorPart:
                        return razorPart.Assembly;
                }
            }
            return null;
        }
    }
}
