using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace Synthesis.Field
{
    public class Gamepiece
    {
        public string name;
        public int holdingLimit;
        public Vector3 spawnpoint;
        public Gamepiece(XElement e)
        {
            this.name = e.Attribute("id").Value;
            this.holdingLimit = int.Parse(e.Attribute("holdinglimit").Value);
            if (holdingLimit == -1) holdingLimit = int.MaxValue;
            this.spawnpoint = new Vector3(float.Parse(e.Attribute("x").Value), float.Parse(e.Attribute("y").Value), float.Parse(e.Attribute("z").Value));
        }
    }
}
