using System;

namespace Crane.CacheProvider
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
        /// Set an absolute expiration
        /// </summary>
        public TimeSpan? AbsoluteExpiration { get; set; }

        /// <summary>
        /// Set a sliding expiration
        /// </summary>
        public TimeSpan? SlidingExpiration { get; set; }
    }
}
