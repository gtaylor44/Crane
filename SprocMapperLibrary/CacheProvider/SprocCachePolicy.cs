using System;

namespace SprocMapperLibrary.CacheProvider
{
    /// <summary>
    /// 
    /// </summary>
    public class SprocCachePolicy
    {
        /// <summary>
        /// 
        /// </summary>
        internal string CacheKeyRegExp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool InfiniteExpiration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan AbsoluteExpiration { get; set; }
    }
}
