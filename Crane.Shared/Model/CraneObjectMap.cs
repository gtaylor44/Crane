using System;
using System.Collections.Generic;
using System.Reflection;

namespace Crane
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CraneObjectMap<T> : ICraneObjectMap
    {
        /// <summary>
        /// 
        /// </summary>
        internal CraneObjectMap()
        {
            PropertyInfoCache = new Dictionary<string, PropertyInfo>();
            TypeInfoCache = new Dictionary<string, TypeInfo>();
            CustomColumnMappings = new Dictionary<string, string>();
            ColumnOrdinalDic = new Dictionary<string, int>();
            Columns = new HashSet<string>();
            DefaultValueDic = new Dictionary<string, object>();
            Type = typeof(T);
        }
        /// <summary>
        /// 
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public HashSet<string> Columns { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string> CustomColumnMappings { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, PropertyInfo> PropertyInfoCache { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, TypeInfo> TypeInfoCache { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, object> DefaultValueDic { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, int> ColumnOrdinalDic { get; set; }
    }
}
