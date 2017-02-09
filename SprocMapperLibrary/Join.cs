using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprocMapperLibrary
{
    public class Join
    {
        private Type Type { get; set; }
        public string ParentKey { get; set; }
        public string JoinObject { get; set; }
        public HashSet<string> Columns { get; set; }
        public Dictionary<string, string> CustomColumnMappings { get; set; }
    }
}
