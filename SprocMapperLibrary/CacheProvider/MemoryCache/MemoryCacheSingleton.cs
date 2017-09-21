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
        public static System.Runtime.Caching.MemoryCache Instance { get; } = new System.Runtime.Caching.MemoryCache("CachingProvider");
    }
}
