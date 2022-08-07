using Hood.Contexts;
using Hood.Core;
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

        private Dictionary<string, Lazy<Dictionary<int, Content>>> bySlug;
        private readonly ContentCategoryCache _categories;
        private readonly IEventsService _events;

        public ContentByTypeCache(IConfiguration config, 
                                  ContentCategoryCache categories,
                                  IEventsService events)
        {
            _config = config;
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
            var options = new DbContextOptionsBuilder();
            options.UseSqlServer(_config["ConnectionStrings:DefaultConnection"]);
            var db = new ContentContext(options.Options);

            ContentSettings contentSettings = Engine.Settings.Content;
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
