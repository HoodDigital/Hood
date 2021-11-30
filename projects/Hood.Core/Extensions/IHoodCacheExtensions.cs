using Hood.Caching;
using System;

namespace Hood.Extensions
{
    public static class IHoodCacheExtensions
    {
        public static T GetFromCache<T>(this IHoodCache cache, string key, Func<T> loadAction)
        {
            if (cache.TryGetValue(key, out T cachedObject))
            {
                return cachedObject;
            }
            cachedObject = loadAction.Invoke();
            if (cachedObject != null)
            {
                cache.Add(key, cachedObject);
            }
            return cachedObject;
        }
    }
}
