using System.Collections.Generic;
using System.Runtime.Caching;

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
                var res = (IEnumerable<T>)MemoryCacheSingleton.Instance[key];

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

            var cachePolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = cacheStrategy.InfiniteExpiration
                    ? ObjectCache.InfiniteAbsoluteExpiration : GetDateTimeOffsetFromTimespan(cacheStrategy.AbsoluteExpiration)
            };

            lock (Padlock)
            {
                MemoryCacheSingleton.Instance.Add(key, items, cachePolicy);
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
        /// Removes a given array of keys from cache.
        /// </summary>
        /// <param name="keys"></param>
        public override void Remove(string[] keys)
        {
            foreach (var key in keys)
            {
                MemoryCacheSingleton.Instance.Remove(key);
            }
        }

        /// <summary>
        /// Removes all keys from cache.
        /// </summary>
        public override void ResetCache()
        {
            foreach (var element in MemoryCacheSingleton.Instance)
            {
                MemoryCacheSingleton.Instance.Remove(element.Key);
            }
        }
    }
}
