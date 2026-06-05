using System;
using System.Collections.Generic;
using System.Linq;
using Hood.Contexts;
using Hood.Core;
using Hood.Models;
using Hood.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Hood.Caching
{
    public class ContentByTypeCache
    {
        private readonly IConfiguration _config;

        private Dictionary<string, Lazy<Dictionary<int, Content>>> bySlug;
        private readonly IEventsService _events;

        public ContentByTypeCache(IConfiguration config, IEventsService events)
        {
            _config = config;
            _events = events;
            EventHandler<EventArgs> resetContentByTypeCache = (_, _) =>
            {
                ResetCache();
            };
            _events.ContentChanged += resetContentByTypeCache;
            // Settings saves must rebuild the type-keyed cache too (HOOD-82).
            _events.OptionsChanged += resetContentByTypeCache;
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
            var options = new DbContextOptionsBuilder<ContentContext>();
            options.UseSqlServer(_config["ConnectionStrings:DefaultConnection"]);
            var db = new ContentContext(options.Options);

            ContentSettings contentSettings = Engine.Settings.Content;
            bySlug = new Dictionary<string, Lazy<Dictionary<int, Content>>>();
            if (contentSettings?.Types == null)
            {
                // Unseeded database — empty cache; install gate handles routing (HOOD-81).
                return;
            }
            foreach (var type in contentSettings.Types.Where(t => t.Enabled && t.CachedByType))
            {
                bySlug.Add(
                    type.Type,
                    new Lazy<Dictionary<int, Content>>(() =>
                        db.Content.Where(c => c.ContentType == type.Type).ToDictionary(c => c.Id)
                    )
                );
            }
        }
    }
}
