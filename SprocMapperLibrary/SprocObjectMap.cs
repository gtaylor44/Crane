using System.Collections.Generic;

namespace SprocMapperLibrary
{
    public class SprocObjectMap<T>
    {
        public HashSet<string> Columns;
        public Dictionary<string, string> CustomColumnMappings;
    }
}
