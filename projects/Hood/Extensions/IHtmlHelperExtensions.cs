using Hood.Core;
using Hood.Enums;
using Hood.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using System.Text.Encodings.Web;

namespace Hood.Extensions
{
    public static class IHtmlHelperExtensions
    {
        public static string RenderHtmlContent(this IHtmlContent htmlContent)
        {
            using (var writer = new StringWriter())
            {
                htmlContent.WriteTo(writer, HtmlEncoder.Default);
                var htmlOutput = writer.ToString();
                return htmlOutput;
            }
        }

        public static void AddInlineScriptParts(this IHtmlHelper html, ResourceLocation location, string script)
        {
            var pageBuilder = Engine.Services.Resolve<IPageBuilder>();
            pageBuilder.AddInlineScriptParts(location, script);
        }
        public static void AppendInlineScriptParts(this IHtmlHelper html, ResourceLocation location, string script)
        {
            var pageBuilder = Engine.Services.Resolve<IPageBuilder>();
            pageBuilder.AppendInlineScriptParts(location, script);
        }
        public static void AddScriptParts(this IHtmlHelper html, string src, string debugSrc = "", bool excludeFromBundle = false, bool isAsync = false)
        {
            AddScriptParts(html, ResourceLocation.AfterScripts, src, debugSrc, excludeFromBundle, isAsync);
        }
        public static void AddScriptParts(this IHtmlHelper html, ResourceLocation location, string src, string debugSrc = "", bool excludeFromBundle = false, bool isAsync = false)
        {
            var pageBuilder = Engine.Services.Resolve<IPageBuilder>();
            pageBuilder.AddScriptParts(location, src, debugSrc, excludeFromBundle, isAsync);
        }
        public static void AppendScriptParts(this IHtmlHelper html, string src, string debugSrc = "", bool excludeFromBundle = false, bool isAsync = false)
        {
            AppendScriptParts(html, ResourceLocation.AfterScripts, src, debugSrc, excludeFromBundle, isAsync);
        }
        public static void AppendScriptParts(this IHtmlHelper html, ResourceLocation location, string src, string debugSrc = "", bool excludeFromBundle = false, bool isAsync = false)
        {
            var pageBuilder = Engine.Services.Resolve<IPageBuilder>();
            pageBuilder.AppendScriptParts(location, src, debugSrc, excludeFromBundle, isAsync);
        }
        public static IHtmlContent RenderScripts(this IHtmlHelper html, IUrlHelper urlHelper, ResourceLocation location, bool bundleFiles = false)
        {
            var pageBuilder = Engine.Services.Resolve<IPageBuilder>();
            return new HtmlString(pageBuilder.GenerateScripts(urlHelper, location, bundleFiles));
        }
        public static IHtmlContent RenderInlineScripts(this IHtmlHelper html, IUrlHelper urlHelper, ResourceLocation location)
        {
            var pageBuilder = Engine.Services.Resolve<IPageBuilder>();
            return new HtmlString(pageBuilder.GenerateInlineScripts(urlHelper, location));
        }

    }


}
