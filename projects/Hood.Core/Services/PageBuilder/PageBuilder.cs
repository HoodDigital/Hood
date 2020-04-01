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
    public class PageBuilder : IPageBuilder
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHoodCache _cache;
        private readonly BundleFileProcessor _bundleFileProcessor;

        private readonly Dictionary<ResourceLocation, List<FileReferenceMetadata>> _css;
        private readonly Dictionary<ResourceLocation, List<FileReferenceMetadata>> _scripts;
        private readonly Dictionary<ResourceLocation, List<string>> _inlineScripts;

        public PageBuilder(IWebHostEnvironment hostingEnvironment, IHoodCache cache)
        {
            _hostingEnvironment = hostingEnvironment;
            _cache = cache;
            _scripts = new Dictionary<ResourceLocation, List<FileReferenceMetadata>>();
            _inlineScripts = new Dictionary<ResourceLocation, List<string>>();
            _css = new Dictionary<ResourceLocation, List<FileReferenceMetadata>>();
            _bundleFileProcessor = new BundleFileProcessor();
        }

        public virtual void AddScript(ResourceLocation location, string src, bool bundle, bool isAsync, bool isDefer)
        {
            if (!_scripts.ContainsKey(location))
                _scripts.Add(location, new List<FileReferenceMetadata>());

            if (string.IsNullOrEmpty(src))
                return;

            _scripts[location].Add(new FileReferenceMetadata
            {
                Bundle = bundle,
                IsAsync = isAsync,
                IsDefer = isDefer,
                Src = src
            });
        }
        public virtual void AddInlineScript(ResourceLocation location, string script)
        {
            if (!_inlineScripts.ContainsKey(location))
                _inlineScripts.Add(location, new List<string>());

            if (string.IsNullOrEmpty(script))
                return;

            _inlineScripts[location].Add(script);
        }
        private static readonly object accessLock = new object();

        public virtual string GenerateScripts(IUrlHelper urlHelper, ResourceLocation location, bool bundleScripts = true)
        {
            if (!_scripts.ContainsKey(location) || _scripts[location] == null)
                return "";

            if (!_scripts.Any())
                return "";

            if (bundleScripts && location.Bundleable() && _hostingEnvironment.EnvironmentName != "Development")
            {
                var partsToBundle = _scripts[location]
                    .Where(x => x.Bundle)
                    .Distinct()
                    .ToArray();
                var partsToDontBundle = _scripts[location]
                    .Where(x => !x.Bundle)
                    .Distinct()
                    .ToArray();

                var result = new StringBuilder();

                if (partsToBundle.Any())
                {
                    var bundlesDirectory = _hostingEnvironment.WebRootPath + "bundles";
                    if (!Directory.Exists(bundlesDirectory))
                        Directory.CreateDirectory(bundlesDirectory);

                    var bundle = new Bundle();
                    foreach (var item in partsToBundle)
                    {
                        if (item.Src.IsAbsoluteUrl())
                        {
                            partsToDontBundle.Append(item);
                            continue;
                        }

                        new PathString(urlHelper.Content(item.Src)).StartsWithSegments(urlHelper.ActionContext.HttpContext.Request.PathBase, out PathString path);
                        
                        var src = path.Value.TrimStart('/');
                        if (!File.Exists(_hostingEnvironment.WebRootPath + src))
                            src = $"wwwroot/{src}";

                        bundle.InputFiles.Add(src);
                    }

                    var bundleSha256 = GetBundleSha256(partsToBundle.Select(x => x.Src).ToArray());
                    bundle.OutputFileName = "wwwroot/bundles/" + location.ToString().ToLower() + ".js";

                    bundle.FileName = _hostingEnvironment.ContentRootPath + "\\" + bundleSha256 + ".json"; ;
                    lock (accessLock)
                    {
                        var cacheKey = $"Hood.Bundling.ShouldRebuild.{bundleSha256}";
                        bool shouldRebuild = !_cache.TryGetValue(cacheKey, out shouldRebuild);
                        if (shouldRebuild)
                        {
                            _bundleFileProcessor.Process(bundle.FileName, new List<Bundle> { bundle });
                            _cache.Add(cacheKey, false, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = new TimeSpan(2, 0, 0) });
                        }
                    }

                    result.AppendFormat("<script src=\"{0}\"></script>", urlHelper.Content("~/bundles/" + location.ToString().ToLower() + ".min.js"));
                    result.Append(Environment.NewLine);
                }
                foreach (var item in partsToDontBundle)
                {
                    result.AppendFormat("<script {1}{2}src=\"{0}\"></script>", urlHelper.Content(item.Src), item.IsAsync ? "async " : "", item.IsDefer ? "defer " : "");
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
                    result.AppendFormat("<script {1}{2}src=\"{0}\"></script>", urlHelper.Content(item.Src), item.IsAsync ? "async " : "", item.IsDefer ? "defer " : "");
                    result.Append(Environment.NewLine);
                }
                return result.ToString();
            }
        }

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

        protected virtual string GetBundleSha256(string[] parts)
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
