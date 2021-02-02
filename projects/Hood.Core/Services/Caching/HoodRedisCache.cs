using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Caching
{
    public class HoodRedisCache : IHoodCache
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public HoodRedisCache(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        public IDatabase Database => _connectionMultiplexer.GetDatabase();

        public bool Exists(string key)
        {
            return Database.KeyExists(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await Database.KeyExistsAsync(key);
        }

        public bool TryGetValue<T>(string key, out T cacheItem)
        {
            if (!Exists(key))
            {
                cacheItem = default;
                return false;
            }
            var json = Database.StringGet(key);
            try
            {
                cacheItem = JsonConvert.DeserializeObject<T>(json);
                return true;
            }
            catch (JsonSerializationException)
            {
                cacheItem = default;
                return false;
            }
        }

        public void Add<T>(string key, T cacheItem, TimeSpan? expiry = null)
        {
            var json = JsonConvert.SerializeObject(cacheItem);
            Database.StringSet(key, json, expiry);
        }

        public async Task AddAsync<T>(string key, T cacheItem, TimeSpan? expiry = null)
        {
            var json = JsonConvert.SerializeObject(cacheItem);
            await Database.StringSetAsync(key, json, expiry);
        }

        public void Remove(string key)
        {
            Database.KeyDelete(key);
        }

        public async Task RemoveAsync(string key)
        {
            await Database.KeyDeleteAsync(key);
        }

        public void ResetCache()
        {
            throw new NotImplementedException();
        }

        public Task ResetCacheAsync()
        {
            throw new NotImplementedException();
        }
    }
}
