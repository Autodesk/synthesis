using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.FEA
{
    public struct ContactDescriptor
    {
        public float AppliedImpulse { get; set; }
        public BulletSharp.Math.Vector3 Position { get; set; }
    }
}
