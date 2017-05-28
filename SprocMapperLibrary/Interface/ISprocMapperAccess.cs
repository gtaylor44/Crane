using SprocMapperLibrary.CacheProvider;

namespace SprocMapperLibrary.Interface
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISprocMapperAccess
    {
        /// <summary>
        /// 
        /// </summary>
        BaseSproc Sproc();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cacheProvider"></param>
        void RegisterCacheProvider(AbstractCacheProvider cacheProvider);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        void RemoveFromCache(string key);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keys"></param>
        void RemoveFromCache(string[] keys);

        /// <summary>
        /// 
        /// </summary>
        void ResetCache();
    }
}
