using Hood.Enums;
using Hood.Services;
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
                            Count = d.Forum.Where(c => c.Forum.Published).Count(),
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
                category = category.ParentCategory;
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
    }
}
