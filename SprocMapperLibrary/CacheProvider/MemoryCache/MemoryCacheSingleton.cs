using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;

namespace SprocMapperLibrary.CacheProvider.MemoryCache
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class MemoryCacheSingleton
    {
        private MemoryCacheSingleton() { }
        /// <summary>
        /// 
        /// </summary>
        public static System.Runtime.Caching.MemoryCache Instance { get; } = new System.Runtime.Caching.MemoryCache("CachingProvider");
    }
}
