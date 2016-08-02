using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using OpenTK;

namespace Simulation_RD.Extensions
{
    public static class BXDConversions
    {
        public static Vector3 Convert(this BXDVector3 val)
        {
            return new Vector3(val.x, val.y, val.z);
        }
    }
}
