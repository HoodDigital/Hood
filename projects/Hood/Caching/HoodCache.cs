using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Caching
{
    public class HoodCache : IHoodCache
    {
        private readonly IMemoryCache _cache;

        private IDictionary<string, DateTime> _entries { get; set; }

        public HoodCache(IMemoryCache cache,
                         IEventsService events)
        {
            _cache = cache;
            _entries = new Dictionary<string, DateTime>();
            events.ContentChanged += onContentChanged;
            events.PropertiesChanged += onPropertiesChanged;
            events.OptionsChanged += onOptionsChanged;
        }

        public string Add<T>(string key, T cacheItem, MemoryCacheEntryOptions options = null)
        {
            if (options == null)
                _cache.Set(key, cacheItem);
            else
                _cache.Set(key, cacheItem, options);
            if (_entries == null)
                _entries = new Dictionary<string, DateTime>();
            if (!_entries.Keys.Contains(key))
                _entries.Add(key, DateTime.Now);
            else
                _entries[key] = DateTime.Now;
            return key;
        }

        /// <summary>
        /// A key/value list of all Keys in the Cache, along with the DateTime they were added to it.
        /// </summary>
        public IDictionary<string, DateTime> Entries
        {
            get
            {
                if (_entries == null)
                    _entries = new Dictionary<string, DateTime>();
                return _entries;
            }
        }

        public bool TryGetValue<T>(string key, out T cacheItem)
        {
            if (_entries == null)
                _entries = new Dictionary<string, DateTime>();
            if (_cache.TryGetValue(key, out cacheItem)) // Get the value from cache.
            {
                if (!_entries.Keys.Contains(key)) // Item is in cache, but not entry dictionary. Add it.
                    _entries.Add(key, DateTime.Now);
                return true;
            }
            if (_entries.Keys.Contains(key)) // Item has fallen out of cache, remove from entries.
                _entries.Remove(key);
            return false;

        }

        public void Remove(string key)
        {
            if (_entries == null)
                _entries = new Dictionary<string, DateTime>();
            if (!key.IsSet())
                return;
            if (_entries.Keys.Contains(key)) // Item has fallen out of cache, remove from entries.
                _entries.Remove(key);
            _cache.Remove(key);
        }

        public void RemoveByType(Type type)
        {
            if (_entries == null)
                _entries = new Dictionary<string, DateTime>();
            if (type == null)
                return;
            var toRemove = _entries.Where(e => e.Key.StartsWith(type.ToString())).ToList();
            foreach (var entry in toRemove)
                Remove(entry.Key);
        }

        public void ResetCache()
        {
            if (_entries == null)
                _entries = new Dictionary<string, DateTime>();
            var keys = _entries.Select(e => e.Key);
            foreach (var key in keys)
                Remove(key);
        }

        #region "Events"
        private void onContentChanged(object sender, EventArgs eventArgs)
        {
            RemoveByType(typeof(Content));
        }

        private void onPropertiesChanged(object sender, EventArgs e)
        {
            RemoveByType(typeof(PropertyListing));
        }

        private void onOptionsChanged(object sender, EventArgs e)
        {
            RemoveByType(typeof(Option));
        }
        #endregion
    }
}
