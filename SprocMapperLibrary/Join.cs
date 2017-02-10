using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprocMapperLibrary
{
    public class Join
    {
        public string Key { get; set; }
        public Type Type { get; set; }
        public string JoinToList { get; set; }
        public HashSet<string> Columns { get; set; }
        public Dictionary<string, string> CustomColumnMappings { get; set; }
    }
}
