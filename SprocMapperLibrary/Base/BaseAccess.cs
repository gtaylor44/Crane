using System;

namespace SprocMapperLibrary.Base
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BaseAccess
    {
        private const string InvalidCacheMsg = "Cache provider already registered.";

        /// <summary>
        /// 
        /// </summary>
        protected AbstractCacheProvider CacheProvider;

        /// <summary>
        /// 
        /// </summary>
        protected BaseAccess()
        {
            CacheProvider = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cacheProvider"></param>
        public void RegisterCacheProvider(AbstractCacheProvider cacheProvider)
        {
            if (CacheProvider != null)
                throw new InvalidOperationException(InvalidCacheMsg);

            CacheProvider = cacheProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void RemoveFromCache(string key)
        {
            if (CacheProvider == null)
            {
                throw new InvalidOperationException("No cache provider has been registered. Use 'RegisterCacheProvider' to register a cache provider.");
            }

            CacheProvider.Remove(key);
        }
    }
}
