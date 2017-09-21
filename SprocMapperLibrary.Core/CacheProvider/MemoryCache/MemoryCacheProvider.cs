using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace SprocMapperLibrary.CacheProvider.MemoryCache
{  
    /// <inheritdoc />
    public class MemoryCacheProvider : AbstractCacheProvider
    {     
        /// <inheritdoc />
        public override bool TryGet<T>(string key, out IEnumerable<T> items)
        {
            lock (Padlock)
            {
                IEnumerable<T> res;
                MemoryCacheSingleton.Instance.TryGetValue(key, out res);

                if (res == null)
                {
                    items = default(IEnumerable<T>);
                    return false;
                }

                lock (Padlock)
                {
                    items = res;
                    return true;
                }
            }
        }

        /// <inheritdoc />
        public override void Add<T>(string key, IEnumerable<T> items)
        {
            var cacheStrategy = GetCachingStrategy(key);

            var cachePolicy = new MemoryCacheEntryOptions();

            if (cacheStrategy.AbsoluteExpiration.HasValue)
            {
                cachePolicy.AbsoluteExpiration = cacheStrategy.InfiniteExpiration
                    ? DateTimeOffset.MaxValue : GetDateTimeOffsetFromTimespan(cacheStrategy.AbsoluteExpiration.Value);
            }

            if (cacheStrategy.SlidingExpiration.HasValue)
            {
                cachePolicy.SlidingExpiration = cacheStrategy.SlidingExpiration;
            }

            lock (Padlock)
            {         
                MemoryCacheSingleton.Instance.Set(key, items, cachePolicy);
            }
        }

        /// <inheritdoc />
        public override void Remove(string key)
        {
            lock (Padlock)
            {
                MemoryCacheSingleton.Instance.Remove(key);
            }
        }

        /// <inheritdoc />
        public override void ResetCache()
        {
            lock (Padlock)
            {
                MemoryCacheSingleton.Instance.Dispose();
                MemoryCacheSingleton.Instance = new Microsoft.Extensions.Caching.Memory.MemoryCache(new MemoryCacheOptions());
            }
        }
    }
}
