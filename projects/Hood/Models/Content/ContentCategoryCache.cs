using Hood.Enums;
using Hood.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Models
{
    public class ContentCategoryCache
    {
        private readonly IConfiguration _config;
        private readonly ISettingsRepository _settings;

        private Lazy<Dictionary<int, ContentCategory>> byKey;
        private Dictionary<string, Lazy<Dictionary<string, ContentCategory>>> bySlug;
        private Lazy<ContentCategory[]> topLevel;

        public ContentCategoryCache(IConfiguration config,
                                    ISettingsRepository settings,
                                    IEventsService events)
        {
            _config = config;
            _settings = settings;
            EventHandler<EventArgs> resetContentByTypeCache = (sender, eventArgs) =>
            {
                ResetCache();
            };
            events.ContentChanged += resetContentByTypeCache;
            ResetCache();
        }

        public ContentCategory FromKey(int categoryId)
        {
            return byKey.Value[categoryId];
        }
        public ContentCategory FromSlug(string contentType, string slug)
        {
            if (!bySlug.ContainsKey(contentType))
                return null;
            if (!bySlug[contentType].Value.ContainsKey(slug))
                return null;
            return bySlug[contentType].Value[slug];
        }

        public int Count { get { return byKey.Value.Count; } }

        public void ResetCache()
        {
            var options = new DbContextOptionsBuilder<HoodDbContext>();
            options.UseSqlServer(_config["ConnectionStrings:DefaultConnection"]);
            var db = new HoodDbContext(options.Options);
            byKey = new Lazy<Dictionary<int, ContentCategory>>(() =>
            {
                var q = from d in db.ContentCategories
                        select new ContentCategory
                        {
                            Id = d.Id,
                            DisplayName = d.DisplayName,
                            Slug = d.Slug,
                            ContentType = d.ContentType,
                            ParentCategoryId = d.ParentCategoryId,
                            ParentCategory = d.ParentCategory,
                            Children = d.Children,
                            Count = d.Content.Where(c => c.Content.Status == (int)Status.Published).Count(),
                        };
                return q.ToDictionary(c => c.Id);
            });

            ContentSettings contentSettings = _settings.GetContentSettings();
            bySlug = new Dictionary<string, Lazy<Dictionary<string, ContentCategory>>>();
            foreach (var type in contentSettings.Types.Where(t => t.Enabled))
            {
                bySlug.Add(
                    type.Type,
                    new Lazy<Dictionary<string, ContentCategory>>(() =>
                    {
                        var q = from d in db.ContentCategories
                                where d.ContentType == type.Type
                                select new ContentCategory
                                {
                                    Id = d.Id,
                                    DisplayName = d.DisplayName,
                                    Slug = d.Slug,
                                    ContentType = d.ContentType,
                                    ParentCategoryId = d.ParentCategoryId,
                                    ParentCategory = d.ParentCategory,
                                    Children = d.Children,
                                    Count = d.Content.Where(c => c.Content.Status == (int)Status.Published).Count(),
                                };
                        return q.ToDictionary(c => c.Slug);
                    })
                );
            }
            topLevel = new Lazy<ContentCategory[]>(() => byKey.Value.Values.Where(c => c.ParentCategoryId == null).ToArray());
        }

        public IEnumerable<ContentCategory> TopLevel(string type)
        {
            topLevel = new Lazy<ContentCategory[]>(() => byKey.Value.Values.Where(c => c.ParentCategoryId == null && c.ContentType == type).ToArray());
            return topLevel.Value;
        }

        public IEnumerable<ContentCategory> GetHierarchy(int categoryId)
        {
            var result = new List<ContentCategory>();
            var category = FromKey(categoryId);
            while (category != null)
            {
                result.Insert(0, category);
                category = category.ParentCategory;
            }

            return result;
        }

        public IEnumerable<ContentCategory> GetThisAndChildren(int categoryId)
        {
            return GetAllCategoriesIncludingChildren(new ContentCategory[] { FromKey(categoryId) });
        }

        private static IEnumerable<ContentCategory> GetAllCategoriesIncludingChildren(IEnumerable<ContentCategory> categories)
        {
            return categories
                .Union(categories
                    .Where(c => c.Children != null)
                    .SelectMany(c => GetAllCategoriesIncludingChildren(c.Children)));
        }

        public IEnumerable<ContentCategory> GetSuggestions(string type)
        {
            return bySlug[type].Value.Values;
        }

        // Html
        public IHtmlContent ContentCategoryTree(IEnumerable<ContentCategory> startLevel, string contentSlug)
        {
            string htmlOutput = string.Empty;

            if (startLevel != null && startLevel.Count() > 0)
            {
                htmlOutput += "<ul>";
                foreach (var key in startLevel.Select(c => c.Id))
                {
                    // Have to reload from the cache to use the count.
                    var category = FromKey(key);

                    htmlOutput += "<li>";
                    htmlOutput += string.Format("<a href=\"/{0}/category/{1}/\" class=\"content-category\">", contentSlug, category.Slug);
                    htmlOutput += string.Format("{0} <span>{1}</span>", category.DisplayName, category.Count);
                    htmlOutput += "</a>";
                    htmlOutput += ContentCategoryTree(category.Children, contentSlug);
                    htmlOutput += "</li>";
                }
                htmlOutput += "</ul>";
            }

            var builder = new HtmlString(htmlOutput);
            return builder;
        }
        public IHtmlContent CategorySelectOptions(IEnumerable<ContentCategory> startLevel, string selectedValue, bool useSlug = false, int startingLevel = 0)
        {
            string htmlOutput = string.Empty;
            if (startLevel != null && startLevel.Count() > 0)
            {
                foreach (var key in startLevel.Select(c => c.Id))
                {
                    // Have to reload from the cache to use the count.
                    var category = FromKey(key);

                    if (useSlug)
                    {
                        htmlOutput += "<option value=\"" + category.Slug + "\"" + (selectedValue == category.Slug ? " selected" : "") + ">";
                    }
                    else
                    {
                        htmlOutput += "<option value=\"" + category.Id + "\"" + (selectedValue == category.Id.ToString() ? " selected" : "") + ">";
                    }
                    for (int i = 0; i < startingLevel; i++)
                    {
                        htmlOutput += "- ";
                    }
                    htmlOutput += string.Format("{0} ({1})", category.DisplayName, category.Count);
                    htmlOutput += "</option>";
                    htmlOutput += CategorySelectOptions(category.Children, selectedValue, useSlug, startingLevel + 1);
                }
            }

            var builder = new HtmlString(htmlOutput);
            return builder;
        }
        public IHtmlContent AdminContentCategoryTree(IEnumerable<ContentCategory> startLevel, string contentType, int startingLevel = 0)
        {
            string htmlOutput = string.Empty;

            if (startLevel != null && startLevel.Count() > 0)
            {
                foreach (var key in startLevel.Select(c => c.Id))
                {
                    // Have to reload from the cache to use the count.
                    var category = FromKey(key);

                    htmlOutput += "<tr>";
                    htmlOutput += "<td>";
                    for (int i = 0; i < startingLevel; i++)
                    {
                        htmlOutput += "<i class=\"fa fa-caret-right m-r-sm\"></i> ";
                    }
                    htmlOutput += string.Format("<a href=\"/admin/content/{0}/manage?category={1}\" class=\"content-category\">", contentType, category.Slug);
                    htmlOutput += string.Format("{0} <span>({1})</span>", category.DisplayName, category.Count);
                    htmlOutput += "</a>";
                    htmlOutput += " <small>[" + category.Slug + "]</small>";
                    htmlOutput += "</td>";
                    htmlOutput += "<td class='text-right'>";
                    htmlOutput += string.Format("<a class=\"btn btn-sm btn-warning m-l-sm edit-content-category action-button\" data-id=\"{0}\" data-type=\"{1}\"><i class=\"fa fa-edit\"></i><span>&nbsp;Edit</span></a>", category.Id, category.ContentType);
                    htmlOutput += string.Format("<a class=\"btn btn-sm btn-danger m-l-xs delete-content-category action-button\" data-id=\"{0}\"><i class=\"fa fa-trash\"></i><span>&nbsp;Delete</span></a>", category.Id);
                    htmlOutput += "</td>";
                    htmlOutput += AdminContentCategoryTree(category.Children, contentType, startingLevel + 1);
                    htmlOutput += "</tr>";
                }
            }

            var builder = new HtmlString(htmlOutput);
            return builder;
        }
        public IHtmlContent AddToCategoryTree(IEnumerable<ContentCategory> startLevel, Content content, string contentSlug, int startingLevel = 0)
        {
            string htmlOutput = string.Empty;

            if (startLevel != null && startLevel.Count() > 0)
            {
                foreach (var key in startLevel.Select(c => c.Id))
                {
                    // Have to reload from the cache to use the count.
                    var category = FromKey(key);

                    htmlOutput += "<div class=\"checkbox\">";
                    for (int i = 0; i < startingLevel; i++)
                    {
                        htmlOutput += "<i class=\"fa fa-caret-right m-r-sm\"></i> ";
                    }
                    htmlOutput += string.Format("<input class=\"styled content-category-check\" id=\"content-category-check-{1}\" name=\"content-category-check-{1}\" type=\"checkbox\" data-id=\"{0}\" value=\"{1}\" {2}>", content.Id, category.Id, content.IsInCategory(category.Id) ? "checked" : "");
                    htmlOutput += string.Format("<label for=\"content-category-check-{1}\">{0}</label>", category.DisplayName, category.Id);
                    htmlOutput += "</div>";
                    htmlOutput += AddToCategoryTree(category.Children, content, contentSlug, startingLevel + 1);
                }
            }

            var builder = new HtmlString(htmlOutput);
            return builder;
        }

    }
}
