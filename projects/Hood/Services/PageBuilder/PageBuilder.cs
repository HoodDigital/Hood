using BundlerMinifier;
using Hood.Caching;
using Hood.Enums;
using Hood.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Hood.Services
{
    class PageBuilder : IPageBuilder
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHoodCache _cache;
        private readonly BundleFileProcessor _bundleFileProcessor;

        private readonly Dictionary<ResourceLocation, List<FileReferenceMetadata>> _css;
        private readonly Dictionary<ResourceLocation, List<FileReferenceMetadata>> _scripts;
        private readonly Dictionary<ResourceLocation, List<string>> _inlineScripts;

        public PageBuilder(IHostingEnvironment hostingEnvironment, IHoodCache cache)
        {
            _hostingEnvironment = hostingEnvironment;
            _cache = cache;
            _scripts = new Dictionary<ResourceLocation, List<FileReferenceMetadata>>();
            _inlineScripts = new Dictionary<ResourceLocation, List<string>>();
            _css = new Dictionary<ResourceLocation, List<FileReferenceMetadata>>();
            _bundleFileProcessor = new BundleFileProcessor();
        }

        public virtual void AddScriptParts(ResourceLocation location, string src, string debugSrc, bool excludeFromBundle, bool isAsync)
        {
            if (!_scripts.ContainsKey(location))
                _scripts.Add(location, new List<FileReferenceMetadata>());

            if (string.IsNullOrEmpty(src))
                return;

            if (string.IsNullOrEmpty(debugSrc))
                debugSrc = src;

            _scripts[location].Add(new FileReferenceMetadata
            {
                ExcludeFromBundle = excludeFromBundle,
                IsAsync = isAsync,
                Src = src,
                DebugSrc = debugSrc
            });
        }

        public virtual void AppendScriptParts(ResourceLocation location, string src, string debugSrc, bool excludeFromBundle, bool isAsync)
        {
            if (!_scripts.ContainsKey(location))
                _scripts.Add(location, new List<FileReferenceMetadata>());

            if (string.IsNullOrEmpty(src))
                return;

            if (string.IsNullOrEmpty(debugSrc))
                debugSrc = src;

            _scripts[location].Insert(0, new FileReferenceMetadata
            {
                ExcludeFromBundle = excludeFromBundle,
                IsAsync = isAsync,
                Src = src,
                DebugSrc = debugSrc
            });
        }

        private static readonly object accessLock = new object();

        public virtual string GenerateScripts(IUrlHelper urlHelper, ResourceLocation location, bool bundleScripts = false)
        {
            if (!_scripts.ContainsKey(location) || _scripts[location] == null)
                return "";

            if (!_scripts.Any())
                return "";

            var debugModel = _hostingEnvironment.IsDevelopment();

            if (bundleScripts)
            {
                var partsToBundle = _scripts[location]
                    .Where(x => !x.ExcludeFromBundle)
                    .Distinct()
                    .ToArray();
                var partsToDontBundle = _scripts[location]
                    .Where(x => x.ExcludeFromBundle)
                    .Distinct()
                    .ToArray();

                var result = new StringBuilder();

                //parts to  bundle
                if (partsToBundle.Any())
                {
                    //ensure \bundles directory exists
                    var bundlesDirectory = _hostingEnvironment.WebRootPath + "bundles";
                    if (!Directory.Exists(bundlesDirectory))
                        Directory.CreateDirectory(bundlesDirectory);

                    var bundle = new Bundle();
                    foreach (var item in partsToBundle)
                    {
                        new PathString(urlHelper.Content(debugModel ? item.DebugSrc : item.Src))
                            .StartsWithSegments(urlHelper.ActionContext.HttpContext.Request.PathBase, out PathString path);
                        var src = path.Value.TrimStart('/');

                        //check whether this file exists, if not it should be stored into /wwwroot directory
                        if (!File.Exists(_hostingEnvironment.WebRootPath + src))
                            src = $"wwwroot/{src}";

                        bundle.InputFiles.Add(src);
                    }
                    //output file
                    var outputFileName = GetBundleFileName(partsToBundle.Select(x => debugModel ? x.DebugSrc : x.Src).ToArray());
                    bundle.OutputFileName = "wwwroot/bundles/" + outputFileName + ".js";
                    //save
                    var configFilePath = _hostingEnvironment.ContentRootPath + "\\" + outputFileName + ".json";
                    bundle.FileName = configFilePath;
                    lock (accessLock)
                    {
                        var cacheKey = $"Hood.Bundling.ShouldRebuild.{outputFileName}";
                        bool shouldRebuild = _cache.TryGetValue(cacheKey, out shouldRebuild);
                        if (shouldRebuild)
                        {
                            _bundleFileProcessor.Process(configFilePath, new List<Bundle> { bundle });
                            _cache.Add(cacheKey, false, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = new TimeSpan(2, 0, 0) });
                        }
                    }
                    result.AppendFormat("<script src=\"{0}\"></script>", urlHelper.Content("~/bundles/" + outputFileName + ".min.js"));
                    result.Append(Environment.NewLine);
                }
                foreach (var item in partsToDontBundle)
                {
                    var src = debugModel ? item.DebugSrc : item.Src;
                    result.AppendFormat("<script {1}src=\"{0}\"></script>", urlHelper.Content(src), item.IsAsync ? "async " : "");
                    result.Append(Environment.NewLine);
                }
                return result.ToString();
            }
            else
            {
                //bundling is disabled
                var result = new StringBuilder();
                foreach (var item in _scripts[location].Distinct())
                {
                    var src = debugModel ? item.DebugSrc : item.Src;
                    result.AppendFormat("<script {1}src=\"{0}\"></script>", urlHelper.Content(src), item.IsAsync ? "async " : "");
                    result.Append(Environment.NewLine);
                }
                return result.ToString();
            }
        }

        /// <summary>
        /// Add inline script element
        /// </summary>
        /// <param name="location">A location of the script element</param>
        /// <param name="script">Script</param>
        public virtual void AddInlineScriptParts(ResourceLocation location, string script)
        {
            if (!_inlineScripts.ContainsKey(location))
                _inlineScripts.Add(location, new List<string>());

            if (string.IsNullOrEmpty(script))
                return;

            _inlineScripts[location].Add(script);
        }
        /// <summary>
        /// Append inline script element
        /// </summary>
        /// <param name="location">A location of the script element</param>
        /// <param name="script">Script</param>
        public virtual void AppendInlineScriptParts(ResourceLocation location, string script)
        {
            if (!_inlineScripts.ContainsKey(location))
                _inlineScripts.Add(location, new List<string>());

            if (string.IsNullOrEmpty(script))
                return;

            _inlineScripts[location].Insert(0, script);
        }
        /// <summary>
        /// Generate all inline script parts
        /// </summary>
        /// <param name="urlHelper">URL Helper</param>
        /// <param name="location">A location of the script element</param>
        /// <returns>Generated string</returns>
        public virtual string GenerateInlineScripts(IUrlHelper urlHelper, ResourceLocation location)
        {
            if (!_inlineScripts.ContainsKey(location) || _inlineScripts[location] == null)
                return "";

            if (!_inlineScripts.Any())
                return "";

            var result = new StringBuilder();
            foreach (var item in _inlineScripts[location])
            {
                result.Append(item);
                result.Append(Environment.NewLine);
            }
            return result.ToString();
        }

        protected virtual string GetBundleFileName(string[] parts)
        {
            if (parts == null || parts.Length == 0)
                throw new ArgumentException("parts");

            var hash = "";
            using (SHA256 sha = new SHA256Managed())
            {
                var hashInput = "";
                foreach (var part in parts)
                {
                    hashInput += $",{part}";
                }
                var input = sha.ComputeHash(Encoding.Unicode.GetBytes(hashInput));
                hash = WebEncoders.Base64UrlEncode(input);
            }
            return hash.ToSeoUrl();
        }


    }

}
