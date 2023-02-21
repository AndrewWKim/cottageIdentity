using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityServer.Core.Extensions
{
    public static class MemoryCacheExtensions
    {
        public static async Task<TItem> GetOrCreateExclusiveAsync<TItem>(this IMemoryCache cache, object key, Func<ICacheEntry, Task<TItem>> factory)
        {
            if (!cache.TryGetValue(key, out var result))
            {
                SemaphoreSlim exclusiveLock;

                lock (cache)
                {
                    exclusiveLock = cache.GetOrCreate("__exclusive__", entry =>
                    {
                        entry.Priority = CacheItemPriority.NeverRemove;
                        return new SemaphoreSlim(1);
                    });
                }

                exclusiveLock.Wait();

                try
                {
                    if (cache.TryGetValue(key, out result))
                    {
                        return (TItem)result;
                    }

                    var entry = cache.CreateEntry(key);
                    result = await factory(entry).ConfigureAwait(false);
                    entry.SetValue(result);
                    entry.Dispose();
                }
                finally
                {
                    exclusiveLock.Release();
                }
            }

            return (TItem)result;
        }
    }
}
