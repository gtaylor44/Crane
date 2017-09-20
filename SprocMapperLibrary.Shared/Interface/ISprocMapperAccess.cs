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
        BaseCommand Command();

        /// <summary>
        /// 
        /// </summary>
        BaseQuery Query();

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
        void ResetCache();
    }
}
