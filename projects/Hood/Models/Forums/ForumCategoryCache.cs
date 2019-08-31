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
    public class ForumCategoryCache
    {
        private readonly IConfiguration _config;

        private Lazy<Dictionary<int, ForumCategory>> byKey;
        private Lazy<Dictionary<string, ForumCategory>> bySlug;
        private Lazy<ForumCategory[]> topLevel;

        public int Count { get { return byKey.Value.Count; } }

        public ForumCategoryCache(
            IConfiguration config,
            IEventsService events)
        {
            _config = config;
            EventHandler<EventArgs> resetForumCache = (sender, eventArgs) =>
            {
                ResetCache();
            };
            events.ForumChanged += resetForumCache;
            ResetCache();
        }

        public ForumCategory FromKey(int categoryId)
        {
            return byKey.Value[categoryId];
        }
        public ForumCategory FromSlug(string slug)
        {
            return bySlug.Value[slug];
        }

        public void ResetCache()
        {
            var options = new DbContextOptionsBuilder<HoodDbContext>();
            options.UseSqlServer(_config["ConnectionStrings:DefaultConnection"]);
            var db = new HoodDbContext(options.Options);
            byKey = new Lazy<Dictionary<int, ForumCategory>>(() =>
            {
                var q = from d in db.ForumCategories
                        select new ForumCategory
                        {
                            Id = d.Id,
                            DisplayName = d.DisplayName,
                            Slug = d.Slug,
                            ParentCategoryId = d.ParentCategoryId,
                            ParentCategory = d.ParentCategory,
                            Children = d.Children,
                            Count = d.Forum.Where(c => c.Forum.Published).Count(),
                        };
                return q.ToDictionary(c => c.Id);
            });
            bySlug = new Lazy<Dictionary<string, ForumCategory>>(() =>
            {
                var q = from d in db.ForumCategories
                        select new ForumCategory
                        {
                            Id = d.Id,
                            DisplayName = d.DisplayName,
                            Slug = d.Slug,
                            ParentCategoryId = d.ParentCategoryId,
                            ParentCategory = d.ParentCategory,
                            Children = d.Children,
                            Count = d.Forum.Where(f => f.Forum.Published).Count(),
                        };
                return q.ToDictionary(c => c.Slug);
            });
            topLevel = new Lazy<ForumCategory[]>(() => byKey.Value.Values.Where(c => c.ParentCategoryId == null).ToArray());
        }

        public IEnumerable<ForumCategory> TopLevel()
        {
            topLevel = new Lazy<ForumCategory[]>(() => byKey.Value.Values.Where(c => c.ParentCategoryId == null).ToArray());
            return topLevel.Value;
        }

        public IEnumerable<ForumCategory> GetHierarchy(int categoryId)
        {
            var result = new List<ForumCategory>();
            var category = FromKey(categoryId);
            while (category != null)
            {
                result.Insert(0, category);
                if (category.ParentCategoryId.HasValue)
                    category = FromKey(category.ParentCategoryId.Value);
                else
                    category = null;
            }

            return result;
        }

        public IEnumerable<ForumCategory> GetThisAndChildren(int categoryId)
        {
            return GetAllCategoriesIncludingChildren(new ForumCategory[] { FromKey(categoryId) });
        }

        private static IEnumerable<ForumCategory> GetAllCategoriesIncludingChildren(IEnumerable<ForumCategory> categories)
        {
            return categories
                .Union(categories
                    .Where(c => c.Children != null)
                    .SelectMany(c => GetAllCategoriesIncludingChildren(c.Children)));
        }

        public IEnumerable<ForumCategory> GetSuggestions()
        {
            return bySlug.Value.Values;
        }

        // Html Outputs
        public IHtmlContent ForumCategoryTree(IEnumerable<ForumCategory> startLevel)
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
                    htmlOutput += string.Format("<a href=\"/forums?category={0}\" class=\"forum-category\">", category.Slug);
                    htmlOutput += string.Format("{0} <span>{1}</span>", category.DisplayName, category.Count);
                    htmlOutput += "</a>";
                    htmlOutput += ForumCategoryTree(category.Children);
                    htmlOutput += "</li>";

                }
                htmlOutput += "</ul>";
            }
            var builder = new HtmlString(htmlOutput);
            return builder;
        }
        public IHtmlContent CategorySelectOptions(IEnumerable<ForumCategory> startLevel, int? selectedValue, bool useSlug = false, int startingLevel = 0)
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
                    htmlOutput += CategorySelectOptions(category.Children, selectedValue, useSlug, startingLevel + 1);
                }
            }
            var builder = new HtmlString(htmlOutput);
            return builder;
        }
        public IHtmlContent AdminForumCategoryTree(IEnumerable<ForumCategory> startLevel, int startingLevel = 0)
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
                                    <a class='btn-link text-warning hood-modal mr-2' href='/admin/forum/categories/edit/{category.Id}?type={category.Slug}' data-complete='$.hood.Forum.Categories.Editor'>
                                        <i class='fa fa-edit'></i><span>
                                            Edit
                                        </span>
                                    </a>
                                    <a class='btn-link text-danger forum-categories-delete' href='/admin/forum/categories/delete/{category.Id}'>
                                        <i class='fa fa-trash'></i>
                                        <span>
                                            Delete
                                        </span>
                                    </a>
                                </div>
                            </div>
                        </div>";

                    htmlOutput += AdminForumCategoryTree(category.Children, startingLevel + 1);

                }
            }

            var builder = new HtmlString(htmlOutput);
            return builder;
        }
        public IHtmlContent AddToCategoryTree(IEnumerable<ForumCategory> startLevel, Forum forum, int startingLevel = 0)
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

                    string check = forum.Categories.Any(c => c.CategoryId == category.Id) ? "checked" : "";

                    htmlOutput += $@"
                        <div class='list-group-item list-group-item-action p-0'>
                            <div class='custom-control custom-checkbox d-flex'>
                                <input class='custom-control-input forum-categories-check'
                                       id='forum-categories-check-{category.Id}'
                                       name='forum-categories-check-{category.Id}'
                                       type='checkbox'
                                       data-url='/admin/forums/{forum.Id}/categories/toggle'
                                       value='{category.Id}' {check} />
                                <label class='custom-control-label col m-2 mt-1 mb-1' for='forum-categories-check-{category.Id}'>
                                    {carets}{category.DisplayName} <span>({category.Count})</span>
                                </label>
                                <div class='col-auto p-2'>
                                    <a class='btn-link text-warning hood-modal mr-2' href='/admin/forums/categories/edit/{category.Id}?type={category.Slug}' data-complete='$.hood.Forum.Categories.Editor'>
                                        <i class='fa fa-edit'></i><span>
                                            Edit
                                        </span>
                                    </a>
                                    <a class='btn-link text-danger forum-categories-delete' href='/admin/forums/categories/delete/{category.Id}'>
                                        <i class='fa fa-trash'></i>
                                        <span>
                                            Delete
                                        </span>
                                    </a>
                                </div>
                            </div>
                        </div>";

                    htmlOutput += AddToCategoryTree(category.Children, forum, startingLevel + 1);
                }
            }

            var builder = new HtmlString(htmlOutput);
            return builder;
        }

    }
}
