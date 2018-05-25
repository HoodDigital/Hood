using Hood.Enums;
using Hood.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;

namespace Hood.Extensions
{
    public static class HtmlHelperExtensions
    {
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
