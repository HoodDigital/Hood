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
    public class ForumCategoryCache
    {
        private readonly IConfiguration _config;
        private readonly ISettingsRepository _settings;

        private Lazy<Dictionary<int, ForumCategory>> byKey;
        private Lazy<Dictionary<string, ForumCategory>> bySlug;
        private Lazy<ForumCategory[]> topLevel;

        public int Count { get { return byKey.Value.Count; } }

        public ForumCategoryCache(IConfiguration config,
                                    ISettingsRepository settings,
                                    EventsService events)
        {
            _config = config;
            _settings = settings;
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
        public IHtmlContent CategorySelectOptions(IEnumerable<ForumCategory> startLevel, string selectedValue, bool useSlug = false, int startingLevel = 0)
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
        public IHtmlContent AdminForumCategoryTree(IEnumerable<ForumCategory> startLevel, int startingLevel = 0)
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
                    htmlOutput += string.Format("<a href=\"/admin/forums/manage?category={0}/\" class=\"forum-category\">", category.Slug);
                    htmlOutput += string.Format("{0} <span>({1})</span>", category.DisplayName, category.Count);
                    htmlOutput += "</a>";
                    htmlOutput += " <small>[" + category.Slug + "]</small>";
                    htmlOutput += "</td>";
                    htmlOutput += "<td class='text-right'>";
                    htmlOutput += string.Format("<a class=\"btn btn-sm btn-warning m-l-sm edit-forum-category action-button\" data-id=\"{0}\"><i class=\"fa fa-edit\"></i><span>&nbsp;Edit</span></a>", category.Id);
                    htmlOutput += string.Format("<a class=\"btn btn-sm btn-danger m-l-xs delete-forum-category action-button\" data-id=\"{0}\"><i class=\"fa fa-trash\"></i><span>&nbsp;Delete</span></a>", category.Id);
                    htmlOutput += "</td>";
                    htmlOutput += AdminForumCategoryTree(category.Children, startingLevel + 1);
                    htmlOutput += "</tr>";
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

                    htmlOutput += "<div class=\"checkbox\">";
                    for (int i = 0; i < startingLevel; i++)
                    {
                        htmlOutput += "<i class=\"fa fa-caret-right m-r-sm\"></i> ";
                    }
                    htmlOutput += string.Format("<input class=\"styled forum-category-check\" id=\"forum-category-check-{1}\" name=\"forum-category-check-{1}\" type=\"checkbox\" data-id=\"{0}\" value=\"{1}\" {2}>", forum.Id, category.Id, forum.IsInCategory(category.Id) ? "checked" : "");
                    htmlOutput += string.Format("<label for=\"forum-category-check-{1}\">{0}</label>", category.DisplayName, category.Id);
                    htmlOutput += "</div>";
                    htmlOutput += AddToCategoryTree(category.Children, forum, startingLevel + 1);
                }
            }

            var builder = new HtmlString(htmlOutput);
            return builder;
        }

    }
}
