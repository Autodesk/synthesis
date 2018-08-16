using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FieldExporter.Exporter
{
    public class Gamepiece
    {
        public Gamepiece(string id, BXDVector3 spawnpoint)
        {
            this.id = id;
            this.spawnpoint = spawnpoint;
        }

        public string id = "";
        public uint holdingLimit = 1; // uint.MaxValue;
        public BXDVector3 spawnpoint = new BXDVector3(0, 0, 0);
    }
}
