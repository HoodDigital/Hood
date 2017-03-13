using Hood.Services;
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
        private Dictionary<string, Lazy<Dictionary<string, ContentCategory>>>  bySlug;
        private Lazy<ContentCategory[]> topLevel;

        public ContentCategoryCache(IConfiguration config, 
                                    ISettingsRepository settings)
        {
            _config = config;
            _settings = settings;
            EventHandler<EventArgs> resetContentByTypeCache = (sender, eventArgs) =>
            {
                ResetCache();
            };
            Events.ContentChanged += resetContentByTypeCache;
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

        public void ResetCache()
        {
            var options = new DbContextOptionsBuilder<DefaultHoodDbContext>();
            options.UseSqlServer(_config["Data:ConnectionString"]);
            var db = new DefaultHoodDbContext(options.Options);
            byKey = new Lazy<Dictionary<int, ContentCategory>>(() => db.ContentCategories.ToDictionary(c => c.ContentCategoryId));

            ContentSettings contentSettings = _settings.GetContentSettings();
            bySlug = new Dictionary<string, Lazy<Dictionary<string, ContentCategory>>>();
            foreach (var type in contentSettings.Types.Where(t => t.Enabled))
            {
                bySlug.Add(
                    type.Type,
                    new Lazy<Dictionary<string, ContentCategory>>(() => db.ContentCategories.Where(c => c.ContentType == type.Type && c.Slug != null).ToDictionary(c => c.Slug))
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

        public IEnumerable<string> GetSuggestions(string query)
        {
            return byKey.Value.Values.Where(c => c.DisplayName.ToLowerInvariant().Contains(query)).Select(c => c.DisplayName);
        }

    }
}
