using System;
using Crane.CacheProvider;
using Crane.Shared.Base;

namespace Crane.Base
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
        protected QueryOptions QueryOptions;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public BaseAccess(QueryOptions options)
        {
            QueryOptions = options ?? CraneHelper.GetDefaultQueryOptions();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cacheProvider"></param>
        public void RegisterCacheProvider(AbstractCraneCacheProvider cacheProvider)
        {
            if (QueryOptions.CacheProvider != null)
                throw new InvalidOperationException(CacheAlreadyRegisteredMsg);

            QueryOptions.CacheProvider = cacheProvider;
        }

        /// <summary>
        /// Removes a cached result. The next time the sproc with the given key is called, it will be a fresh copy. 
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void RemoveKeyFromCache(string key)
        {
            if (QueryOptions.CacheProvider == null)            
                throw new InvalidOperationException(NoCacheRegisteredMsg);

            QueryOptions.CacheProvider.Remove(key);
        }

        /// <summary>
        /// Resets all cached items. Anything that is utilising cache will get a new copy next time query is executed. 
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void ResetCache()
        {
            if (QueryOptions.CacheProvider == null)
            {
                throw new InvalidOperationException(NoCacheRegisteredMsg);
            }

            QueryOptions.CacheProvider.ResetCache();
        }

        /// <summary>
        /// Set a custom policy on all cached items.
        /// </summary>
        /// <param name="policy">The custom policy.</param>
        public void AddGlobalPolicy(CraneCachePolicy policy)
        {
            if (QueryOptions.CacheProvider == null)
            {
                throw new InvalidOperationException(NoCacheRegisteredMsg);
            }

            QueryOptions.CacheProvider.AddGlobalPolicy(policy);
        }

        /// <summary>
        /// Set a custom policy for a regular expression. If the regular expression matches, this policy will take precedence over the global policy (if one is set) and default policy. 
        /// </summary>
        /// <param name="regularExpression">The regular express pattern to match.</param>
        /// <param name="policy">The custom policy.</param>
        public void AddPolicy(string regularExpression, CraneCachePolicy policy)
        {
            if (QueryOptions.CacheProvider == null)
            {
                throw new InvalidOperationException(NoCacheRegisteredMsg);
            }

            QueryOptions.CacheProvider.AddPolicy(regularExpression, policy);
        }
    }
}
