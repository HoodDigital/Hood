using Hood.Core;
using Hood.Enums;
using Hood.Services;
using Hood.ViewModels;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Text.Encodings.Web;

namespace Hood.Extensions
{
    public static class HtmlHelperExtensions
    {
        #region Scripts
        public static string RenderHtmlContent(this IHtmlContent htmlContent)
        {
            using (var writer = new StringWriter())
            {
                htmlContent.WriteTo(writer, HtmlEncoder.Default);
                var htmlOutput = writer.ToString();
                return htmlOutput;
            }
        }

        public static void AddInlineScript(this IHtmlHelper html, ResourceLocation location, string script)
        {
            var pageBuilder = Engine.Services.Resolve<IPageBuilder>();
            pageBuilder.AddInlineScript(location, script);
        }
        public static void AddScript(this IHtmlHelper html, ResourceLocation location, string src, bool bundle = false, bool isAsync = false, bool isDefer = false)
        {
            var pageBuilder = Engine.Services.Resolve<IPageBuilder>();
            pageBuilder.AddScript(location, src, bundle, isAsync, isDefer);
        }
        public static IHtmlContent RenderScripts(this IHtmlHelper html, IUrlHelper urlHelper, ResourceLocation location, bool bundleFiles = true)
        {
            var pageBuilder = Engine.Services.Resolve<IPageBuilder>();
            return new HtmlString(pageBuilder.GenerateScripts(urlHelper, location, bundleFiles));
        }
        public static IHtmlContent RenderInlineScripts(this IHtmlHelper html, IUrlHelper urlHelper, ResourceLocation location)
        {
            var pageBuilder = Engine.Services.Resolve<IPageBuilder>();
            return new HtmlString(pageBuilder.GenerateInlineScripts(urlHelper, location));
        }

        #endregion

        public static IHtmlContent RenderHoneypotField<TModel>(this IHtmlHelper<TModel> html)
            where TModel : HoneyPotFormModel
        {
            var keygen = new KeyGenerator(false, true, true, false);
            var hp_key = keygen.Generate(4);
            var output = $@"<div class='comments_or_notes hidden d-none'><label for='comments_or_notes_{hp_key}'>Comments:</label><input name='comments_or_notes_{hp_key}' id='comments_or_notes_{hp_key}' type='text' tabindex='-1' autocomplete='off' /></div>";
            return new HtmlString(output);
        }

    }
}
