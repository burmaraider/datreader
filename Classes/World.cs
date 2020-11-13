using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVP2.DAT.Reader.Classes
{
    public class WorldObjects
    {
        public List<WorldObject> obj { get; set; }

        public int endingOffset { get; set; }
    }

    public class WorldObject
    {
        public Dictionary<string, object> options { get; set; }
        public string objectType { get; set; }
        public Int16 dataLength { get; set; }
        public Int32 dataOffset { get; set; }
        public Int32 objectEntries { get; set; }

    }
}
