using Hood.Models;
using Hood.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Caching
{
    public class ContentByTypeCache
    {
        private readonly IConfiguration _config;
        private readonly ISettingsRepository _settings;

        private Dictionary<string, Lazy<Dictionary<int, Content>>> bySlug;
        private readonly ContentCategoryCache _categories;
        private readonly IEventsService _events;

        public ContentByTypeCache(IConfiguration config, 
                                  ISettingsRepository settings,
                                  ContentCategoryCache categories,
                                  IEventsService events)
        {
            _config = config;
            _settings = settings;
            _categories = categories;
            _events = events;
            EventHandler<EventArgs> resetContentByTypeCache = (sender, eventArgs) =>
            {
                ResetCache();
            };
            _events.ContentChanged += resetContentByTypeCache;
            ResetCache();
        }

        public Content GetById(string contentType, int id)
        {
            if (!bySlug.ContainsKey(contentType))
                return null;
            if (!bySlug[contentType].Value.ContainsKey(id))
                return null;
            return bySlug[contentType].Value[id];
        }

        public void ResetCache()
        {
            var options = new DbContextOptionsBuilder<HoodDbContext>();
            options.UseSqlServer(_config["ConnectionStrings:DefaultConnection"]);
            var db = new HoodDbContext(options.Options);

            ContentSettings contentSettings = _settings.GetContentSettings();
            bySlug = new Dictionary<string, Lazy<Dictionary<int, Content>>>();
            foreach (var type in contentSettings.Types.Where(t => t.Enabled && t.CachedByType))
            {
                bySlug.Add(
                    type.Type,
                    new Lazy<Dictionary<int, Content>>(() => db.Content.Where(c => c.ContentType == type.Type).ToDictionary(c => c.Id))
                );
            }
        }
    }
}
