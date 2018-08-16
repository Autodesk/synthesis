using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FieldExporter.Exporter
{
    class Gamepiece
    {
        public string id = "";
        public Inventor.ComponentOccurrence component = null;
        public uint holdingLimit = uint.MaxValue;
        public BXDVector3 spawnpoint = new BXDVector3(0, 0, 0);
    }
}
