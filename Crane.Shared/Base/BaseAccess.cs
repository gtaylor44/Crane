using System;
using SprocMapperLibrary.CacheProvider;
using SprocMapperLibrary.Shared.Interface;

namespace SprocMapperLibrary.Base
{
    /// <summary>
    /// Contains common behaviours that access object should have. 
    /// </summary>
    public abstract class BaseAccess
    {
        private const string CacheAlreadyRegisteredMsg = "Cache provider already registered.";
        private const string NoCacheRegisteredMsg = "No cache provider has been registered. Use 'RegisterCacheProvider' to register a cache provider.";

        /// <summary>
        /// 
        /// </summary>
        protected AbstractCraneCacheProvider CacheProvider;

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
        public void RegisterCacheProvider(ICraneCacheProvider cacheProvider)
        {
            if (CacheProvider != null)
                throw new InvalidOperationException(CacheAlreadyRegisteredMsg);

            CacheProvider = cacheProvider as AbstractCraneCacheProvider;
        }

        /// <summary>
        /// Removes a cached result. The next time the sproc with the given key is called, it will be a fresh copy. 
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void RemoveKeyFromCache(string key)
        {
            if (CacheProvider == null)            
                throw new InvalidOperationException(NoCacheRegisteredMsg);
            
            CacheProvider.Remove(key);
        }

        /// <summary>
        /// Resets all cached items. Anything that is utilising cache will get a new copy next time query is executed. 
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void ResetCache()
        {
            if (CacheProvider == null)
            {
                throw new InvalidOperationException(NoCacheRegisteredMsg);
            }

            CacheProvider.ResetCache();
        }
    }
}
