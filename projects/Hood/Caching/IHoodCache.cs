using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace Hood.Caching
{
    public interface IHoodCache
    {
        IDictionary<string, DateTime> Entries { get; }
        bool TryGetValue<T>(string key, out T cacheItem);

        string Add<T>(string key, T cacheItem, MemoryCacheEntryOptions options = null);

        void ResetCache();
        void Remove(string key);
        void RemoveByType(Type type);
  }
}
