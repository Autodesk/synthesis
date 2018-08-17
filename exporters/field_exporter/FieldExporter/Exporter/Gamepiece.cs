using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FieldExporter.Exporter
{
    public class Gamepiece
    {
        public Gamepiece(string id, BXDVector3 spawnpoint, ushort holdingLimit = ushort.MaxValue)
        {
            this.id = id;
            this.spawnpoint = spawnpoint;
            this.holdingLimit = holdingLimit;
        }

        public string id = "";
        public BXDVector3 spawnpoint = new BXDVector3(0, 0, 0);
        public ushort holdingLimit = ushort.MaxValue;
    }
}
