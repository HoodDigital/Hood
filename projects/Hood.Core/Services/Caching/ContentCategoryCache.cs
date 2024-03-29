﻿using Hood.Contexts;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Html;
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
            if (!Engine.Services.Installed) {
                return;
            }
            var options = new DbContextOptionsBuilder<ContentContext>();
            options.UseSqlServer(_config["ConnectionStrings:DefaultConnection"]);
            var db = new ContentContext(options.Options);
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
        public IHtmlContent ContentCategoryTree(IEnumerable<ContentCategory> startLevel, string contentSlug, string listClass = "list-unstyled", string itemClass = "")
        {
            string htmlOutput = string.Empty;

            if (startLevel != null && startLevel.Count() > 0)
            {
                htmlOutput += $"<ul class='{listClass}'>";
                foreach (var key in startLevel.Select(c => c.Id))
                {
                    // Have to reload from the cache to use the count.
                    var category = FromKey(key);

                    htmlOutput += $"<li class='{itemClass}'>";
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
        public IHtmlContent CategorySelectOptions(IEnumerable<ContentCategory> startLevel, int? selectedValue, int startingLevel = 0)
        {
            string htmlOutput = string.Empty;
            if (startLevel != null && startLevel.Count() > 0)
            {
                foreach (var key in startLevel.Select(c => c.Id))
                {
                    // Have to reload from the cache to use the count.
                    var category = FromKey(key);

                    htmlOutput += "<option value=\"" + category.Id + "\"" + (selectedValue == category.Id ? " selected" : "") + ">";
                    for (int i = 0; i < startingLevel; i++)
                    {
                        htmlOutput += "- ";
                    }
                    htmlOutput += string.Format("{0} ({1})", category.DisplayName, category.Count);
                    htmlOutput += "</option>";
                    htmlOutput += CategorySelectOptions(category.Children, selectedValue, startingLevel + 1);
                }
            }

            var builder = new HtmlString(htmlOutput);
            return builder;
        }
        public IHtmlContent AdminContentCategoryTree(IEnumerable<ContentCategory> startLevel, string contentType, List<string> categoriesSelected, int startingLevel = 0)
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
                        carets += "<i class='fa fa-angle-double-right me-1'></i>";
                    }

                    var currentChecked = categoriesSelected != null && categoriesSelected.Contains(category.Slug) ? "checked" : "";

                    htmlOutput += $@"
                        <div class='list-group-item list-group-item-action d-flex align-items-center p-2'>
                            <div class='col form-check'>
                                <input class='form-check-input submit-on-change'
                                       id='Category-{category.Slug}' name='categories' {currentChecked}
                                       type='checkbox'
                                       value='{category.Slug}' />
                                <label class='form-check-label d-block' for='Category-{category.Slug}'>
                                    {carets}{category.DisplayName} <span>({category.Count})</span>
                                </label>
                            </div>
                            <div class='col-auto'>
                                <a class='btn btn-warning content-categories-edit mr-2' href='/admin/content/categories/edit/{category.Id}?type={category.Slug}' data-complete='$.hood.Content.Categories.Editor'>
                                    <i class='fa fa-edit'></i><span>
                                        Edit
                                    </span>
                                </a>
                                <a class='btn btn-danger content-categories-delete' href='/admin/content/categories/delete/{category.Id}'>
                                    <i class='fa fa-trash'></i>
                                    <span>
                                        Delete
                                    </span>
                                </a>
                            </div>
                        </div>";

                    htmlOutput += AdminContentCategoryTree(category.Children, contentType, categoriesSelected, startingLevel + 1);

                }
            }

            var builder = new HtmlString(htmlOutput);
            return builder;
        }
        public IHtmlContent AddToCategoryTree(IEnumerable<ContentCategory> startLevel, Content content, int startingLevel = 0)
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
                        <div class='list-group-item list-group-item-action d-flex align-items-center p-2'>
                            <div class='col form-check'>
                                <input class='form-check-input content-categories-check'
                                       id='content-categories-check-{category.Id}'
                                       name='content-categories-check-{category.Id}'
                                       type='checkbox'
                                       data-url='/admin/content/{content.Id}/categories/toggle'
                                       value='{category.Id}' {check} />
                                <label class='form-check-label d-block' for='content-categories-check-{category.Id}'>
                                    {carets}{category.DisplayName} <span>({category.Count})</span>
                                </label>
                            </div>
                            <div class='col-auto'>
                                <a class='btn btn-warning content-categories-edit mr-2' href='/admin/content/categories/edit/{category.Id}?type={category.Slug}' data-complete='$.hood.Content.Categories.Editor'>
                                    <i class='fa fa-edit'></i><span>
                                        Edit
                                    </span>
                                </a>
                                <a class='btn btn-danger content-categories-delete' href='/admin/content/categories/delete/{category.Id}'>
                                    <i class='fa fa-trash'></i>
                                    <span>
                                        Delete
                                    </span>
                                </a>
                            </div>
                        </div>";

                    htmlOutput += AddToCategoryTree(category.Children, content, startingLevel + 1);
                }
            }

            var builder = new HtmlString(htmlOutput);
            return builder;
        }

    }
}
