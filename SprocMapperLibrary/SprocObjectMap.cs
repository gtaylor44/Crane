using System;
using System.Collections.Generic;
using System.Reflection;

namespace SprocMapperLibrary
{
    public class SprocObjectMap<T> : ISprocObjectMap
    {
        public SprocObjectMap()
        {
            PropertyInfoCache = new Dictionary<string, PropertyInfo>();
            CustomColumnMappings = new Dictionary<string, string>();
            Columns = new HashSet<string>();
            Type = typeof(T);
        }
        public Type Type { get; set; }
        public HashSet<string> Columns { get; set; }
        public Dictionary<string, string> CustomColumnMappings { get; set; }
        public Dictionary<string, PropertyInfo> PropertyInfoCache { get; set; }
    }

    public interface ISprocObjectMap
    {
        Type Type { get; set; }
        HashSet<string> Columns { get; set; }
        Dictionary<string, string> CustomColumnMappings { get; set; }
        Dictionary<string, PropertyInfo> PropertyInfoCache { get; set; }
    }
}
