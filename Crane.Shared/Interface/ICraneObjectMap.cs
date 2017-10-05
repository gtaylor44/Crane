using System;
using System.Collections.Generic;
using FastMember;

// ReSharper disable once CheckNamespace
namespace SprocMapperLibrary
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
        Dictionary<string, Member> MemberInfoCache { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Dictionary<string, object> DefaultValueDic { get; set; }
        /// <summary>
        /// 
        /// </summary>
        TypeAccessor TypeAccessor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Dictionary<string, int> ColumnOrdinalDic { get; set; }
    }
}
