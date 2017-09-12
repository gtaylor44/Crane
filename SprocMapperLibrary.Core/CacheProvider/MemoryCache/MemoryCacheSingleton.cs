using Microsoft.Extensions.Caching.Memory;
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
        public static IMemoryCache Instance { get; set; } = new Microsoft.Extensions.Caching.Memory.MemoryCache(new MemoryCacheOptions());
    }
}
