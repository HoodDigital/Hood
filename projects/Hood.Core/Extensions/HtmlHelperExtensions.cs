using Hood.Core;
using Hood.Enums;
using Hood.Services;
using Hood.ViewModels;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
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
        public static void AddScriptParts(this IHtmlHelper html, string src, bool excludeFromBundle = false, bool isAsync = false, bool isDefer = false)
        {
            AddScriptParts(html, ResourceLocation.AfterScripts, src, excludeFromBundle, isAsync, isDefer);
        }
        public static void AddScriptParts(this IHtmlHelper html, ResourceLocation location, string src, bool excludeFromBundle = false, bool isAsync = false, bool isDefer = false)
        {
            var pageBuilder = Engine.Services.Resolve<IPageBuilder>();
            pageBuilder.AddScriptParts(location, src, excludeFromBundle, isAsync, isDefer);
        }
        public static void AppendScriptParts(this IHtmlHelper html, string src, bool excludeFromBundle = false, bool isAsync = false, bool isDefer = false)
        {
            AppendScriptParts(html, ResourceLocation.AfterScripts, src, excludeFromBundle, isAsync, isDefer);
        }
        public static void AppendScriptParts(this IHtmlHelper html, ResourceLocation location, string src, bool excludeFromBundle = false, bool isAsync = false, bool isDefer = false)
        {
            var pageBuilder = Engine.Services.Resolve<IPageBuilder>();
            pageBuilder.AppendScriptParts(location, src, excludeFromBundle, isAsync, isDefer);
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

        #endregion

        public static IHtmlContent RenderHoneypotField<TModel>(this IHtmlHelper<TModel> html)
            where TModel : HoneyPotFormModel
        {
            var keygen = new KeyGenerator(false, true, true, false);
            var hp_key = keygen.Generate(4);
            var output = $@"<div class='comments_or_notes hidden d-none'><label for='comments_or_notes_{hp_key}'>Comments:</label><input name='comments_or_notes_{hp_key}' id='comments_or_notes_{hp_key}' type='text' tabindex='-1' autocomplete='off' /></div>";
            return new HtmlString(output);
        }

        public static IHtmlContent DescriptionFor<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var modelExplorer = ExpressionMetadataProvider.FromLambdaExpression(expression, html.ViewData, html.MetadataProvider);
            if (modelExplorer == null) throw new InvalidOperationException($"Failed to get model explorer for {ExpressionHelper.GetExpressionText(expression)}");
            return new HtmlString(string.Format("<a class='text-info description' data-toggle='popover' data-placement='left' data-content='{0}' title='{1}'><i class='fa fa-question-circle'></i></a>", WebUtility.HtmlEncode(modelExplorer.Metadata.Description), WebUtility.HtmlEncode(modelExplorer.Metadata.DisplayName)));
        }
        public static IHtmlContent DescriptionTextFor<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var modelExplorer = ExpressionMetadataProvider.FromLambdaExpression(expression, html.ViewData, html.MetadataProvider);
            if (modelExplorer == null) throw new InvalidOperationException($"Failed to get model explorer for {ExpressionHelper.GetExpressionText(expression)}");
            return new HtmlString(WebUtility.HtmlEncode(modelExplorer.Metadata.Description));
        }
        public static IHtmlContent BootstrapAlert(this IHtmlHelper html, string message, AlertType type, bool autoDismiss = false, string cssClass = "")
        {
            if (!message.IsSet())
                return null;

            switch (type)
            {
                case AlertType.Success:
                    return html.Raw(string.Format("<div class='alert alert-success {1} {2}'><i class='fa fa-thumbs-up m-r-sm'></i>{0}</div>", message, autoDismiss ? "auto-dismiss" : "", cssClass));
                case AlertType.Warning:
                    return html.Raw(string.Format("<div class='alert alert-warning {1} {2}'><i class='fa fa-exclamation-triangle m-r-sm'></i>{0}</div>", message, autoDismiss ? "auto-dismiss" : "", cssClass));
                case AlertType.Danger:
                    return html.Raw(string.Format("<div class='alert alert-danger {1} {2}'><i class='fa fa-exclamation-triangle m-r-sm'></i>{0}</div>", message, autoDismiss ? "auto-dismiss" : "", cssClass));
                case AlertType.Info:
                default:
                    return html.Raw(string.Format("<div class='alert alert-info {1} {2}'><i class='fa fa-info-circle m-r-sm'></i>{0}</div>", message, autoDismiss ? "auto-dismiss" : "", cssClass));
            }

        }
        public static IHtmlContent ContainedBootstrapAlert(this IHtmlHelper html, string message, AlertType type, bool autoDismiss = false, string cssClass = "")
        {
            if (!message.IsSet())
                return null;

            switch (type)
            {
                case AlertType.Success:
                    return html.Raw(string.Format("<div class='alert alert-success {1} {2}'><div class='container'><i class='fa fa-thumbs-up m-r-sm'></i>{0}</div></div>", message, autoDismiss ? "auto-dismiss" : "", cssClass));
                case AlertType.Warning:
                    return html.Raw(string.Format("<div class='alert alert-warning {1} {2}'><div class='container'><i class='fa fa-exclamation-triangle m-r-sm'></i>{0}</div></div>", message, autoDismiss ? "auto-dismiss" : "", cssClass));
                case AlertType.Danger:
                    return html.Raw(string.Format("<div class='alert alert-danger {1} {2}'><div class='container'><i class='fa fa-exclamation-triangle m-r-sm'></i>{0}</div></div>", message, autoDismiss ? "auto-dismiss" : "", cssClass));
                case AlertType.Info:
                default:
                    return html.Raw(string.Format("<div class='alert alert-info {1} {2}'><div class='container'><i class='fa fa-info-circle m-r-sm'></i>{0}</div></div>", message, autoDismiss ? "auto-dismiss" : "", cssClass));
            }

        }
    }
}
