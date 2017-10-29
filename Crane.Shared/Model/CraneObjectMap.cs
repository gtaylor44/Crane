using System;
using System.Collections.Generic;
using System.Reflection;

namespace Crane
{
    internal class CraneObjectMap<T> : ICraneObjectMap
    {
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
        public Type Type { get; set; }
        public HashSet<string> Columns { get; set; }
        public Dictionary<string, string> CustomColumnMappings { get; set; }
        public Dictionary<string, PropertyInfo> PropertyInfoCache { get; set; }
        public Dictionary<string, TypeInfo> TypeInfoCache { get; set; }
        public Dictionary<string, object> DefaultValueDic { get; set; }
        public Dictionary<string, int> ColumnOrdinalDic { get; set; }
    }
}
