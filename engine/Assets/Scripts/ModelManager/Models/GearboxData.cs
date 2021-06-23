using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.ModelManager.Models
{
    public struct GearboxData
    {
        public string Name;
        public IList<string> MotorUuids;
        public float MaxSpeed;
        public float Torque;
    }
}
