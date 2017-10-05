using Microsoft.Extensions.Caching.Memory;
namespace SprocMapperLibrary.CacheProvider.MemoryCache
{
    /// <summary>
    /// 
    /// </summary>
    public static class MemoryCacheSingleton
    {
        /// <summary>
        /// 
        /// </summary>
        public static IMemoryCache Instance { get; set; } = new Microsoft.Extensions.Caching.Memory.MemoryCache(new MemoryCacheOptions());
    }
}
