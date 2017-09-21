﻿using SprocMapperLibrary.CacheProvider;
using SprocMapperLibrary.Shared.Interface;

namespace SprocMapperLibrary.Interface
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISprocMapperAccess
    {
        /// <summary>
        /// Contains methods for executing database commands.
        /// </summary>
        BaseCommand Command();

        /// <summary>
        /// Contains methods for performing select query on a database.
        /// </summary>
        BaseQuery Query();

        /// <summary>
        /// Register a cache provider that extends AbstractCacheProvider. 
        /// </summary>
        /// <param name="cacheProvider"></param>
        void RegisterCacheProvider(ICacheProvider cacheProvider);

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
