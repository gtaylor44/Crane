using System;
using System.Collections.Generic;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Crane
{
    /// <summary>
    /// 
    /// </summary>
    internal interface ICraneObjectMap
    {
        /// <summary>
        /// 
        /// </summary>
        Type Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        HashSet<string> Columns { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Dictionary<string, string> CustomColumnMappings { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Dictionary<string, PropertyInfo> PropertyInfoCache { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Dictionary<string, TypeInfo> TypeInfoCache { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Dictionary<string, object> DefaultValueDic { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Dictionary<string, int> ColumnOrdinalDic { get; set; }
    }
}
