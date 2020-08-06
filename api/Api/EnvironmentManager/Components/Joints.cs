using SynthesisAPI.Modules.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SynthesisAPI.EnvironmentManager.Components
{
    /// <summary>
    /// A container for Joints stored in an entity
    /// </summary>
    [BuiltinComponent]
    public class Joints : Component
    {
        public delegate void JointAdded(IJoint joint);
        public delegate void JointRemoved(int index);
        public event JointAdded AddJoint;
        public event JointRemoved RemoveJoint;

        internal List<IJoint> _joints = new List<IJoint>();

        public float Count => _joints.Count;

        public void Add(IJoint joint)
        {
            _joints.Add(joint);
            AddJoint?.Invoke(joint);
        }

        public void Remove(IJoint joint)
        {
            int jointIndex = _joints.IndexOf(joint);
            _joints.RemoveAt(jointIndex);
            RemoveJoint?.Invoke(jointIndex);
        }

        public IEnumerable<IJoint> GetJointsWhere(Func<IJoint, bool> predicate)
        {
            return _joints.Where(predicate);
        }
    }
}
