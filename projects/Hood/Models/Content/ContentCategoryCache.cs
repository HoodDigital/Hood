﻿using Hood.Enums;
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
        private Dictionary<string, Lazy<Dictionary<string, ContentCategory>>> bySlug;
        private Lazy<ContentCategory[]> topLevel;

        public ContentCategoryCache(IConfiguration config,
                                    ISettingsRepository settings,
                                    EventsService events)
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

    }
}
