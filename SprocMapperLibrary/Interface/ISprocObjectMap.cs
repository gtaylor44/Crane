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
    internal interface ISprocObjectMap
    {
        Type Type { get; set; }
        HashSet<string> Columns { get; set; }
        Dictionary<string, string> CustomColumnMappings { get; set; }
        Dictionary<string, Member> MemberInfoCache { get; set; }
        Dictionary<string, object> DefaultValueDic { get; set; }
        TypeAccessor TypeAccessor { get; set; }
        Dictionary<string, int> ColumnOrdinalDic { get; set; }
    }
}
