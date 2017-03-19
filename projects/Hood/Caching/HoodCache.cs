using Hood.Models;
using Hood.Services;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Hood.Caching
{
    public class HoodCache : IHoodCache
    {
        private readonly IMemoryCache _cache;

        private IList<string> _entries { get; set; }
        private readonly EventsService _events;

        public HoodCache(IMemoryCache cache,
                         EventsService events)
        {
            _cache = cache;
            _entries = new List<string>();
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
            if (!_entries.Contains(key))
                _entries.Add(key);
            return key;
        }

        public IList<string> Keys
        {
            get
            {
                return _entries;
            }
        }


        public bool TryGetValue<T>(string key, out T cacheItem)
        {
            if (_cache.TryGetValue(key, out cacheItem)) // Get the value from cache.
            {
                if (!_entries.Contains(key)) // Item is in cache, but not entry dictionary. Add it.
                    _entries.Add(key);
                return true;
            }
            if (_entries.Contains(key)) // Item has fallen out of cache, remove from entries.
                _entries.Remove(key);
            return false;

        }

        public void Remove(string key)
        {
            _entries.Remove(key);
            _cache.Remove(key);
        }

        public void RemoveWhere(Func<string, bool> predicate)
        {
            var toRemove = _entries.Where(predicate).ToList();
            foreach (string key in toRemove)
                Remove(key);
        }

        public void RemoveByType(Type type)
        {
            var toRemove = _entries.Where(e => e.StartsWith(type.ToString())).ToList();
            foreach (string key in toRemove)
                Remove(key);
        }

        public void ResetCache()
        {
            var toRemove = _entries.ToList();
            foreach (string key in toRemove)
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
