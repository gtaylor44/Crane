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
