using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hood.Caching
{
    public class HoodCache : IHoodCache
    {
        private readonly ConcurrentDictionary<string, DateTime> _entryKeys;
        private readonly IMemoryCache _cache;
        public HoodCache(IMemoryCache cache)
        {
            _entryKeys = new ConcurrentDictionary<string, DateTime>();
            _cache = cache;
        }

        public ConcurrentDictionary<string, DateTime> Keys 
        { 
            get
            {
                return _entryKeys;
            }                
        }

        public bool Exists(string key)
        {
            return _entryKeys.ContainsKey(key);
        }

        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(_entryKeys.ContainsKey(key));
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

        public void Add<T>(string key, T cacheItem, TimeSpan? expiry = null)
        {
            if (!expiry.HasValue)
                _cache.Set(key, cacheItem);
            else
                _cache.Set(key, cacheItem, expiry.Value);

            AddToKeyStore(key);
        }

        public Task AddAsync<T>(string key, T cacheItem, TimeSpan? expiry = null)
        {
            Add(key, cacheItem, expiry);
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            if (!key.IsSet())
                return;
            RemoveKey(key);
            _cache.Remove(key);
        }

        public Task RemoveAsync(string key)
        {            
            Remove(key);
            return Task.CompletedTask;
        }

        public void RemoveByType(Type type)
        {
            if (type == null)
                return;
            var toRemove = _entryKeys.Where(e => e.Key.StartsWith(type.ToString())).ToList();
            foreach (var entry in toRemove)
                Remove(entry.Key);
        }

        public Task RemoveByTypeAsync(Type type)
        {
            RemoveByType(type);
            return Task.CompletedTask;
        }

        public void ResetCache()
        {
            var keys = _entryKeys.Select(e => e.Key);
            foreach (var key in keys)
                Remove(key);
            _entryKeys.Clear();
        }

        public Task ResetCacheAsync()
        {
            ResetCache();
            return Task.CompletedTask;
        }

        #region "Helpers"
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

        #endregion
    }
}
