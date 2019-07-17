using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Caching
{
    public class ContentCategoryCache
    {
        private readonly IConfiguration _config;
        private Lazy<Dictionary<int, ContentCategory>> byKey;
        private Dictionary<string, Lazy<Dictionary<string, ContentCategory>>> bySlug;
        private Lazy<ContentCategory[]> topLevel;

        public ContentCategoryCache(
            IConfiguration config,
            IEventsService events)
        {
            _config = config;
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

        public int Count(string type)
        {
            if (type.IsSet())
                return bySlug[type].Value.Count;
            else
                return byKey.Value.Count;
        }

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
                            Count = d.Content.Where(c => c.Content.Status == ContentStatus.Published).Count(),
                        };
                return q.ToDictionary(c => c.Id);
            });

            ContentSettings contentSettings = Engine.Settings.Content;
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
                                    Count = d.Content.Where(c => c.Content.Status == ContentStatus.Published).Count(),
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
                    htmlOutput += string.Format("<a href=\"/{0}/category/{1}/\" class=\"content-categories\">", contentSlug, category.Slug);
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

                    string carets = "";
                    for (int i = 0; i < startingLevel; i++)
                    {
                        carets += "<i class='fa fa-caret-right mr-1'></i>";
                    }

                    htmlOutput += $@"
                        <div class='list-group-item list-group-item-action p-0'>
                            <div class='custom-control custom-checkbox d-flex'>
                                <input class='custom-control-input refresh-on-change'
                                       id='Category-{category.Slug}' name='categories'
                                       type='checkbox'
                                       value='{category.Slug}' />
                                <label class='custom-control-label col m-2 mt-1 mb-1' for='Category-{category.Slug}'>
                                    {carets}{category.DisplayName} <span>({category.Count})</span>
                                </label>
                                <div class='col-auto p-2'>
                                    <a class='btn-link text-warning hood-modal mr-2' href='/admin/content/categories/edit/{category.Id}?type={category.Slug}' data-complete='$.hood.Content.Categories.Editor'>
                                        <i class='fa fa-edit'></i><span>
                                            Edit
                                        </span>
                                    </a>
                                    <a class='btn-link text-danger content-categories-delete' href='/admin/content/categories/delete/{category.Id}'>
                                        <i class='fa fa-trash'></i>
                                        <span>
                                            Delete
                                        </span>
                                    </a>
                                </div>
                            </div>
                        </div>";

                    htmlOutput += AdminContentCategoryTree(category.Children, contentType, startingLevel + 1);

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

                    string carets = "";
                    for (int i = 0; i < startingLevel; i++)
                    {
                        carets += "<i class='fa fa-caret-right mr-1'></i>";
                    }

                    string check = content.Categories.Any(c => c.CategoryId == category.Id) ? "checked" : "";

                    htmlOutput += $@"
                        <div class='list-group-item list-group-item-action p-0'>
                            <div class='custom-control custom-checkbox d-flex'>
                                <input class='custom-control-input content-categories-check'
                                       id='content-categories-check-{category.Id}'
                                       name='content-categories-check-{category.Id}'
                                       type='checkbox'
                                       data-url='/admin/content/{content.Id}/categories/toggle'
                                       value='{category.Id}' {check} />
                                <label class='custom-control-label col m-2 mt-1 mb-1' for='content-categories-check-{category.Id}'>
                                    {carets}{category.DisplayName} <span>({category.Count})</span>
                                </label>
                                <div class='col-auto p-2'>
                                    <a class='btn-link text-warning hood-modal mr-2' href='/admin/content/categories/edit/{category.Id}?type={category.Slug}' data-complete='$.hood.Content.Categories.Editor'>
                                        <i class='fa fa-edit'></i><span>
                                            Edit
                                        </span>
                                    </a>
                                    <a class='btn-link text-danger content-categories-delete' href='/admin/content/categories/delete/{category.Id}'>
                                        <i class='fa fa-trash'></i>
                                        <span>
                                            Delete
                                        </span>
                                    </a>
                                </div>
                            </div>
                        </div>";

                    htmlOutput += AddToCategoryTree(category.Children, content, contentSlug, startingLevel + 1);
                }
            }

            var builder = new HtmlString(htmlOutput);
            return builder;
        }

    }
}
