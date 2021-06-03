using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
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
        protected IDatabase Database => _connectionMultiplexer.GetDatabase();

        public ConcurrentDictionary<string, DateTime> Keys { get; }

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
            RedisValue json = Database.StringGet(key);
            try
            {
                cacheItem = JsonConvert.DeserializeObject<T>(json.ToString());
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
            try
            {
                string json = JsonConvert.SerializeObject(cacheItem);
                Database.StringSet(key, json, expiry);
            }
            catch (JsonSerializationException)
            {
                return;
            }
        }

        public async Task AddAsync<T>(string key, T cacheItem, TimeSpan? expiry = null)
        {
            try
            {
                string json = JsonConvert.SerializeObject(cacheItem);
                await Database.StringSetAsync(key, json, expiry);
            }
            catch (JsonSerializationException)
            {
                return;
            }
        }

        public void Remove(string key)
        {
            Database.KeyDelete(key);
        }

        public async Task RemoveAsync(string key)
        {
            await Database.KeyDeleteAsync(key);
        }

        public void RemoveByType(Type type)
        {
            throw new NotImplementedException();
        }

        public Task RemoveByTypeAsync(Type type)
        {
            RemoveByType(type);
            return Task.CompletedTask;
        }

        public void ResetCache()
        {
            Database.Execute("FLUSHALL");
        }

        public async Task ResetCacheAsync()
        {
            await Database.ExecuteAsync("FLUSHDB");
        }
    }
}
