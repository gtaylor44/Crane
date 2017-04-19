using System;
using System.Collections.Generic;
using SprocMapperLibrary.Base;

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
        internal static readonly object Padlock = new object();

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        public override void Add<T>(string key, IEnumerable<T> items)
        {
            lock (Padlock)
            {
                MemoryCacheSingleton.Instance.Add(key, items, DateTimeOffset.MaxValue);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Remove(string key)
        {
            lock (Padlock)
            {
                MemoryCacheSingleton.Instance.Remove(key);
            }
        }
    }
}
