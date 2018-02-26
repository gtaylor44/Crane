using Crane.CacheProvider;

namespace Crane.Shared.Base
{
    /// <summary>
    /// 
    /// </summary>
    public class QueryOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public AbstractCraneCacheProvider CacheProvider { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool ValidateSelectColumns { get; set; } = false;      
    }
}
