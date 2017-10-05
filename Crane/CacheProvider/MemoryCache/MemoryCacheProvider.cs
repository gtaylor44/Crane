using System.Collections.Generic;
using System.Runtime.Caching;

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

        /// <inheritdoc />
        public override void Add<T>(string key, IEnumerable<T> items)
        {
            var cacheStrategy = GetCachingStrategy(key);

            var cachePolicy = new CacheItemPolicy();

            if (cacheStrategy.AbsoluteExpiration.HasValue)
            {
                cachePolicy.AbsoluteExpiration = cacheStrategy.InfiniteExpiration
                    ? ObjectCache.InfiniteAbsoluteExpiration : GetDateTimeOffsetFromTimespan(cacheStrategy.AbsoluteExpiration.Value);
            }

            if (cacheStrategy.SlidingExpiration.HasValue)
            {
                cachePolicy.SlidingExpiration = cacheStrategy.SlidingExpiration.Value;
            }

            lock (Padlock)
            {
                MemoryCacheSingleton.Instance.Add(key, items, cachePolicy);
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
                foreach (var element in MemoryCacheSingleton.Instance)
                {
                    MemoryCacheSingleton.Instance.Remove(element.Key);
                }
            }
        }
    }
}
