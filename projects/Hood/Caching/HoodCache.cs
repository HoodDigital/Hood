using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Caching
{
    public class HoodCache : IHoodCache
    {
        private readonly ConcurrentDictionary<string, DateTime> _entryKeys;
        private readonly IMemoryCache _cache;
        public HoodCache(IMemoryCache cache)
        {
            // Memory Cache stuff
            _entryKeys = new ConcurrentDictionary<string, DateTime>();
            _cache = cache;

            Engine.Events.ContentChanged += OnContentChanged;
            Engine.Events.PropertiesChanged += OnPropertiesChanged;
            Engine.Events.OptionsChanged += OnOptionsChanged;
        }

        public void Add<T>(string key, T cacheItem, MemoryCacheEntryOptions options = null)
        {
            if (options == null)
                _cache.Set(key, cacheItem);
            else
                _cache.Set(key, cacheItem, options);

            AddToKeyStore(key);
        }

        private void AddToKeyStore(string key)
        {
            if (_entryKeys.TryGetValue(key, out DateTime priorEntry))
            {
                // Try to update with the new entry if a previous entries exist.
                bool entryAdded = _entryKeys.TryUpdate(key, DateTime.Now, priorEntry);

                if (!entryAdded)
                {
                    // The update will fail if the previous entry was removed after retrival.
                    // Adding the new entry will succeed only if no entry has been added since.
                    // This guarantees removing an old entry does not prevent adding a new entry.
                    _entryKeys.TryAdd(key, DateTime.Now);
                }
            }
            else
            {
                // Try to add the new entry if no previous entries exist.
                _entryKeys.TryAdd(key, DateTime.Now);
            }
        }
        private void RemoveKey(string key)
        {
            if (!_entryKeys.ContainsKey(key))
                return;
            if (_entryKeys.TryRemove(key, out _))
            {
            }
        }

        /// <summary>
        /// A key/value list of all Keys in the Cache, along with the DateTime they were added to it.
        /// </summary>
        public IDictionary<string, DateTime> Entries
        {
            get
            {
                if (_entryKeys == null)
                    return null;
                return _entryKeys;
            }
        }

        public bool TryGetValue<T>(string key, out T cacheItem)
        {
            try
            {
                if (_cache.TryGetValue(key, out cacheItem))
                {
                    if (!_entryKeys.ContainsKey(key))
                        AddToKeyStore(key);
                    return true;
                }
                RemoveKey(key);
                return false;
            }
            catch (Exception)
            {
                cacheItem = default;
                return false;
            }
        }

        public void Remove(string key)
        {
            if (!key.IsSet())
                return;
            RemoveKey(key);
            _cache.Remove(key);
        }

        public void RemoveByType(Type type)
        {
            if (type == null)
                return;
            var toRemove = _entryKeys.Where(e => e.Key.StartsWith(type.ToString())).ToList();
            foreach (var entry in toRemove)
                Remove(entry.Key);
        }

        public void ResetCache()
        {
            var keys = _entryKeys.Select(e => e.Key);
            foreach (var key in keys)
                Remove(key);
            _entryKeys.Clear();
        }

        #region "Events"
        private void OnContentChanged(object sender, EventArgs eventArgs)
        {
            RemoveByType(typeof(Content));
        }

        private void OnPropertiesChanged(object sender, EventArgs e)
        {
            RemoveByType(typeof(PropertyListing));
        }

        private void OnOptionsChanged(object sender, EventArgs e)
        {
            RemoveByType(typeof(Option));
        }
        #endregion
    }
}
