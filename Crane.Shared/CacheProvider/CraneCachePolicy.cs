using System;

namespace SprocMapperLibrary.CacheProvider
{
    /// <summary>
    /// 
    /// </summary>
    public class CraneCachePolicy
    {
        /// <summary>
        /// Apply the policy only to the keys in a given regular expression.
        /// </summary>
        internal string CacheKeyRegExp { get; set; }

        /// <summary>
        /// Sets expiration to maximum value. 
        /// </summary>
        public bool InfiniteExpiration { get; set; }

        /// <summary>
        /// Set an absolute expiration time
        /// </summary>
        public TimeSpan? AbsoluteExpiration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan? SlidingExpiration { get; set; }
    }
}
