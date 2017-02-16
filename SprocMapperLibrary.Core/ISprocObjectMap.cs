using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastMember;

namespace SprocMapperLibrary.Core
{
    public interface ISprocObjectMap
    {
        Type Type { get; set; }
        HashSet<string> Columns { get; set; }
        Dictionary<string, string> CustomColumnMappings { get; set; }
        Dictionary<string, Member> MemberInfoCache { get; set; }
        TypeAccessor TypeAccessor { get; set; }
        Dictionary<string, int> ColumnOrdinalDic { get; set; }
    }
}
