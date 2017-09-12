using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SprocMapperLibrary.CacheProvider.MemoryCache
{  
    /// <summary>
    /// 
    /// </summary>
    public class MemoryCacheProvider : AbstractCacheProvider
    {
        /// <summary>
        /// 
        /// </summary>
        public MemoryCacheProvider() : base(){}
        
        /// <summary>
        /// Returns true and initialises output param items if exists in cache, otherwise returns false and default collection. 
        /// </summary>
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

        /// <summary>
        /// Adds an IEnumerable to cache with given key. 
        /// </summary>
        public override void Add<T>(string key, IEnumerable<T> items)
        {
            SprocCachePolicy cacheStrategy = GetCachingStrategy(key);

            var cachePolicy = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = cacheStrategy.InfiniteExpiration
                    ? DateTimeOffset.MaxValue : GetDateTimeOffsetFromTimespan(cacheStrategy.AbsoluteExpiration)
            };

            lock (Padlock)
            {         
                MemoryCacheSingleton.Instance.Set(key, items, cachePolicy);
            }
        }

        /// <summary>
        /// Remove an item in the cache at a specified key.
        /// </summary>
        public override void Remove(string key)
        {
            lock (Padlock)
            {
                MemoryCacheSingleton.Instance.Remove(key);
            }
        }

        /// <summary>
        /// Removes all keys from cache.
        /// </summary>
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
