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
            var pageHeadBuilder = Engine.Current.Resolve<IPageBuilder>();
            pageHeadBuilder.AddInlineScriptParts(location, script);
        }
        public static void AppendInlineScriptParts(this IHtmlHelper html, ResourceLocation location, string script)
        {
            var pageHeadBuilder = Engine.Current.Resolve<IPageBuilder>();
            pageHeadBuilder.AppendInlineScriptParts(location, script);
        }
        public static void AddScriptParts(this IHtmlHelper html, string src, string debugSrc = "", bool excludeFromBundle = false, bool isAsync = false)
        {
            AddScriptParts(html, ResourceLocation.AfterScripts, src, debugSrc, excludeFromBundle, isAsync);
        }
        public static void AddScriptParts(this IHtmlHelper html, ResourceLocation location, string src, string debugSrc = "", bool excludeFromBundle = false, bool isAsync = false)
        {
            var pageHeadBuilder = Engine.Current.Resolve<IPageBuilder>();
            pageHeadBuilder.AddScriptParts(location, src, debugSrc, excludeFromBundle, isAsync);
        }
        public static void AppendScriptParts(this IHtmlHelper html, string src, string debugSrc = "", bool excludeFromBundle = false, bool isAsync = false)
        {
            AppendScriptParts(html, ResourceLocation.AfterScripts, src, debugSrc, excludeFromBundle, isAsync);
        }
        public static void AppendScriptParts(this IHtmlHelper html, ResourceLocation location, string src, string debugSrc = "", bool excludeFromBundle = false, bool isAsync = false)
        {
            var pageHeadBuilder = Engine.Current.Resolve<IPageBuilder>();
            pageHeadBuilder.AppendScriptParts(location, src, debugSrc, excludeFromBundle, isAsync);
        }
        public static IHtmlContent RenderScripts(this IHtmlHelper html, IUrlHelper urlHelper, ResourceLocation location, bool bundleFiles = false)
        {
            var pageHeadBuilder = Engine.Current.Resolve<IPageBuilder>();
            return new HtmlString(pageHeadBuilder.GenerateScripts(urlHelper, location, bundleFiles));
        }
        public static IHtmlContent RenderInlineScripts(this IHtmlHelper html, IUrlHelper urlHelper, ResourceLocation location)
        {
            var pageHeadBuilder = Engine.Current.Resolve<IPageBuilder>();
            return new HtmlString(pageHeadBuilder.GenerateInlineScripts(urlHelper, location));
        }

    }


}
