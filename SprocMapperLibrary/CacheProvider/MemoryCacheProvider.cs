using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using SprocMapperLibrary.Base;

namespace SprocMapperLibrary.CacheProvider
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
        internal static readonly MemoryCache Cache = new MemoryCache("CachingProvider");

        /// <summary>
        /// 
        /// </summary>
        public override bool TryGet<T>(string key, out IEnumerable<T> items)
        {
            lock (Padlock)
            {
                var res = (IEnumerable<T>)Cache[key];

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
                Cache.Add(key, items, DateTimeOffset.MaxValue);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Remove(string key)
        {
            lock (Padlock)
            {
                Cache.Remove(key);
            }
        }
    }
}
