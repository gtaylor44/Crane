using System.Collections.Generic;

namespace SprocMapperLibrary
{
    public class SprocObjectMap<T> : ISprocObjectMap
    {
        public HashSet<string> Columns { get; set; }
        public Dictionary<string, string> CustomColumnMappings { get; set; }
    }

    public interface ISprocObjectMap
    {
        HashSet<string> Columns { get; set; }
        Dictionary<string, string> CustomColumnMappings { get; set; }
    }
}
