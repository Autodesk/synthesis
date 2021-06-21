using System;
using UnityEngine;

namespace Synthesis.ModelManager.Models
{
    public class Motor : MonoBehaviour
    {
        public float MaxSpeed;
        public float Torque;

        private HingeJoint _joint;
        public HingeJoint Joint { get => _joint; set => _joint = value; }

        public (string Name, string Uuid) Meta { get; set; } // => _joint.GetComponent<JointMeta>().Data;

        public static implicit operator HingeJoint(Motor joint) => joint._joint;

        public Motor(HingeJoint joint)
        {
            _joint = joint;
        }

        public override int GetHashCode() => Meta.Uuid.GetHashCode();
        public override bool Equals(object other) {
            if (other == null || !(other is Motor))
                return false;

            return (other as Motor).GetHashCode() == GetHashCode();
        }

        public static bool operator ==(Motor a, Motor b) {
            if (ReferenceEquals(a, null))
                return ReferenceEquals(b, null);
                
            return a.Equals(b);
        }
        public static bool operator !=(Motor a, Motor b) {
            if (ReferenceEquals(a, null))
                return !ReferenceEquals(b, null);
            
            return !a.Equals(b);
        }

    }
}