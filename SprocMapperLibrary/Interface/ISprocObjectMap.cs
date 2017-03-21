using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FastMember;

[assembly: InternalsVisibleTo("UnitTest")]
[assembly: InternalsVisibleTo("IntegrationTest")]
namespace SprocMapperLibrary.Interface
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISprocObjectMap
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
