using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using IdentityServer.Core.Extensions;

namespace IdentityServer.Core
{
    public static class CacheService
    {
        public const string UserPrefix = "Users";

        private static int _cacheExpirationMs;

        private static MemoryCache _instance;

        public static IMemoryCache GetInstance()
        {
            return _instance ?? (_instance = new MemoryCache(new MemoryCacheOptions()));
        }

        public static void Init(int cacheTimeMinutes)
        {
            _cacheExpirationMs = cacheTimeMinutes * 60 * 1000;
        }

        public static async Task<T> GetDataFromCacheAsync<T>(string key, Func<Task<T>> getEntityFunc)
            where T : class
        {
            var item = await GetInstance().GetOrCreateExclusiveAsync(key, async cacheEntry =>
            {
                var cts = new CancellationTokenSource();
                cacheEntry.AddExpirationToken(new CancellationChangeToken(cts.Token));
                var value = await getEntityFunc().ConfigureAwait(false);
                cts.CancelAfter(_cacheExpirationMs);

                return value;
            });

            return item;
        }

        public static async Task<IEnumerable<T>> GetDataFromCacheAsync<T>(string key, Func<Task<IEnumerable<T>>> getEntitiesFunc)
            where T : class
        {
            var list = await GetInstance().GetOrCreateExclusiveAsync(key, async cacheEntry =>
            {
                var cts = new CancellationTokenSource();
                cacheEntry.AddExpirationToken(new CancellationChangeToken(cts.Token));
                var items = await getEntitiesFunc().ConfigureAwait(false);
                cts.CancelAfter(_cacheExpirationMs);

                return items;
            });

            return list;
        }
    }
}
