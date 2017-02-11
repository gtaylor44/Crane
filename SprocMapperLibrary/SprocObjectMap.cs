using System;
using System.Collections.Generic;

namespace SprocMapperLibrary
{
    public class SprocObjectMap<T> : ISprocObjectMap
    {
        public SprocObjectMap()
        {
            Type = typeof(T);
        }
        public Type Type { get; set; }
        public HashSet<string> Columns { get; set; }
        public Dictionary<string, string> CustomColumnMappings { get; set; }
    }

    public interface ISprocObjectMap
    {
        Type Type { get; set; }
        HashSet<string> Columns { get; set; }
        Dictionary<string, string> CustomColumnMappings { get; set; }
    }
}
