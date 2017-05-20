using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace Hood.Caching
{
    public interface IHoodCache
    {
        IList<string> Keys { get; }
        bool TryGetValue<T>(string key, out T cacheItem);

        string Add<T>(string key, T cacheItem, MemoryCacheEntryOptions options = null);

        void ResetCache();
        void Remove(string key);
        void RemoveWhere(Func<string, bool> predicate);
        void RemoveByType(Type type);
  }
}
