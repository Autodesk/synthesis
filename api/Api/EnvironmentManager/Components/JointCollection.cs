using SynthesisAPI.Modules.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SynthesisAPI.EnvironmentManager.Components
{
    /// <summary>
    /// Bit rough, but the JointCollection is going to manage IJoints on top of the environmentmanager.
    /// Same for the JointCollectionAdapter.
    /// </summary>
    [BuiltinComponent]
    public class JointCollection : Component
    {
        public delegate void JointChange(IJoint index);
        public event JointChange JointAdded;
        public event JointChange JointRemoved;

        internal List<IJoint> _joints = new List<IJoint>();

        public float Count => _joints.Count;

        public void Add(IJoint joint)
        {
            _joints.Add(joint);
            JointAdded?.Invoke(joint);
        }

        public void Remove(IJoint joint)
        {
            _joints.Remove(joint);
            JointRemoved?.Invoke(joint);
        }
    }
}
