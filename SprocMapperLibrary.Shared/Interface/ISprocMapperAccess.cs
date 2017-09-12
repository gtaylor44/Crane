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
        void RemoveKeyFromCache(string key);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regex"></param>
        void RemoveMatchingKeysFromCache(string regex);

        /// <summary>
        /// 
        /// </summary>
        void ResetCache();
    }
}
