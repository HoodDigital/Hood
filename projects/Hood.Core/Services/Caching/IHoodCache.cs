using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hood.Caching
{
    public interface IHoodCache
    {
        ConcurrentDictionary<string, DateTime> Keys { get; }

        bool Exists(string key);
        Task<bool> ExistsAsync(string key);

        bool TryGetValue<T>(string key, out T cacheItem);

        void Add<T>(string key, T cacheItem, TimeSpan? expiry = null);
        Task AddAsync<T>(string key, T cacheItem, TimeSpan? expiry = null);

        void Remove(string key);
        Task RemoveAsync(string key);

        void ResetCache();
        Task ResetCacheAsync();
    }
}
