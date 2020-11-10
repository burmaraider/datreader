using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVP2.DAT.Reader.Classes
{
    public static class World
    {
        public struct WorldObjects
        {
            public List<WorldObject> obj { get; set; }
            public int endingOffset { get; set; }

        }

        public struct WorldObject
        {
            public List<byte[]> data { get; set; }
            public Dictionary<string, object> options { get; set; }
            public string objectType { get; set; }
            public Int16 dataLength { get; set; }
            public Int32 dataOffset { get; set; }

            public string name { get; set; }
            public Int32 objectEntries { get; set; }

            public LTTypes.LTRotation rotation { get; set; }

            public LTTypes.LTVector position { get; set; }
        }
    }
}
