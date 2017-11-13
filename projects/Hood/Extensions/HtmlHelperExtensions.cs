using Hood.Enums;
using Hood.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlContent ContentCategoryTree(this IHtmlHelper html, IEnumerable<ContentCategory> categories, string contentSlug)
        {
            string htmlOutput = string.Empty;

            if (categories != null && categories.Count() > 0)
            {
                htmlOutput += "<ul>";
                foreach (var category in categories)
                {
                    htmlOutput += "<li>";
                    htmlOutput += string.Format("<a href=\"/{0}/category/{1}/\" class=\"content-category\">", contentSlug, category.Slug);
                    htmlOutput += string.Format("{0} <span>{1}</span>", category.DisplayName, category.Count);
                    htmlOutput += "</a>";
                    htmlOutput += html.ContentCategoryTree(category.Children, contentSlug);
                    htmlOutput += "</li>";
                }
                htmlOutput += "</ul>";
            }

            return html.Raw(htmlOutput);
        }

        public static IHtmlContent CategorySelectOptions(this IHtmlHelper html, IEnumerable<ContentCategory> categories, bool useSlug = false, int startingLevel = 0)
        {
            string htmlOutput = string.Empty;
            if (categories != null && categories.Count() > 0)
            {
                foreach (var category in categories)
                {
                    if (useSlug)
                        htmlOutput += "<option value=\"" + category.Slug + "\">";
                    else
                        htmlOutput += "<option value=\"" + category.Id + "\">";
                    for (int i = 0; i < startingLevel; i++)
                    {
                        htmlOutput += "- ";
                    }
                    htmlOutput += string.Format("{0} ({1})", category.DisplayName, category.Count);
                    htmlOutput += "</option>";
                    htmlOutput += html.CategorySelectOptions(category.Children, useSlug, startingLevel + 1);
                }
            }
            return html.Raw(htmlOutput);
        }

        public static IHtmlContent AdminContentCategoryTree(this IHtmlHelper html, IEnumerable<ContentCategory> categories, string contentSlug, int startingLevel = 0)
        {
            string htmlOutput = string.Empty;

            if (categories != null && categories.Count() > 0)
            {
                foreach (var category in categories)
                {
                    htmlOutput += "<tr>";
                    htmlOutput += "<td>";
                    for (int i = 0; i < startingLevel; i++)
                    {
                        htmlOutput += "<i class=\"fa fa-caret-right m-r-sm\"></i> ";
                    }
                    htmlOutput += string.Format("<a href=\"/{0}/category/{1}/\" class=\"content-category\">", contentSlug, category.Slug);
                    htmlOutput += string.Format("{0} <span>({1})</span>", category.DisplayName, category.Count);
                    htmlOutput += "</a>";
                    htmlOutput += " <small>[" + category.Slug + "]</small>";
                    htmlOutput += "</td>";
                    htmlOutput += "<td class='text-right'>";
                    htmlOutput += string.Format("<a class=\"btn btn-sm btn-warning m-l-sm edit-category action-button\" data-id=\"{0}\" data-type=\"{1}\"><i class=\"fa fa-edit\"></i><span>&nbsp;Edit</span></a>", category.Id, category.ContentType);
                    htmlOutput += string.Format("<a class=\"btn btn-sm btn-danger m-l-xs delete-category action-button\" data-id=\"{0}\"><i class=\"fa fa-trash\"></i><span>&nbsp;Delete</span></a>", category.Id);
                    htmlOutput += "</td>";
                    htmlOutput += html.AdminContentCategoryTree(category.Children, contentSlug, startingLevel + 1);
                    htmlOutput += "</tr>";
                }
            }

            return html.Raw(htmlOutput);
        }

        public static IHtmlContent BootstrapAlert(this IHtmlHelper html, string message, AlertType type)
        {
            if (!message.IsSet())
                return null;

            switch (type)
            {
                case AlertType.Success:
                    return html.Raw(string.Format("<div class='alert alert-success'><i class='fa fa-thumbs-up m-r-sm'></i>{0}</div>", message));
                case AlertType.Warning:
                    return html.Raw(string.Format("<div class='alert alert-warning'><i class='fa fa-exclamation-triangle m-r-sm'></i>{0}</div>", message));
                case AlertType.Danger:
                    return html.Raw(string.Format("<div class='alert alert-danger'><i class='fa fa-exclamation-triangle m-r-sm'></i>{0}</div>", message));
                case AlertType.Info:
                default:
                    return html.Raw(string.Format("<div class='alert alert-info'><i class='fa fa-info-circle m-r-sm'></i>{0}</div>", message));
            }

        }

    }
}
