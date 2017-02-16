using System;
using System.Collections.Generic;
using FastMember;

namespace SprocMapperLibrary.Core
{
    public class SprocObjectMap<T> : ISprocObjectMap
    {
        public SprocObjectMap()
        {
            MemberInfoCache = new Dictionary<string, Member>();
            CustomColumnMappings = new Dictionary<string, string>();
            ColumnOrdinalDic = new Dictionary<string, int>();
            Columns = new HashSet<string>();
            Type = typeof(T);
        }
        public Type Type { get; set; }
        public HashSet<string> Columns { get; set; }
        public Dictionary<string, string> CustomColumnMappings { get; set; }
        public Dictionary<string, Member> MemberInfoCache { get; set; }
        public TypeAccessor TypeAccessor { get; set; }
        public Dictionary<string, int> ColumnOrdinalDic { get; set; }
    }

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
