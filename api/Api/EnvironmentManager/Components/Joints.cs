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
    public class Joints : Component
    {
        private Guid _guid;
        private static List<Joints> _allJointContainers = new List<Joints>();

        public delegate void JointChanged(IJoint joint);
        public static event JointChanged GlobalAddJoint;
        internal event JointChanged AddJoint;
        public static event JointChanged GlobalRemoveJoint;
        internal event JointChanged RemoveJoint;

        private List<IJoint> _joints = new List<IJoint>();
        public List<IJoint> AllJoints {
            get => _joints;
        }
        public static List<IJoint> GlobalAllJoints {
            get {
                var allJoints = new List<IJoint>();
                _allJointContainers.ForEach(x => allJoints.AddRange(x._joints));
                return allJoints;
            }
        }

        public float Count => _joints.Count;

        public Joints() {
            _guid = Guid.NewGuid();
            _allJointContainers.Add(this);
        }

        ~Joints() {
            _allJointContainers.RemoveAll(x => Equals(x));
        }

        public void Add(IJoint joint)
        {
            _joints.Add(joint);
            AddJoint?.Invoke(joint);
            GlobalAddJoint?.Invoke(joint);
        }

        public void Remove(IJoint joint)
        {
            _joints.RemoveAt(_joints.IndexOf(joint)); // Unsure if this will work because it doesn't have a HashCode or Equals function
            RemoveJoint?.Invoke(joint);
            GlobalRemoveJoint?.Invoke(joint);
        }

        public IEnumerable<IJoint> GetJointsWhere(Func<IJoint, bool> predicate)
        {
            return _joints.Where(predicate);
        }

        public override bool Equals(object obj) {
            if (!(obj is Joints))
                return false;

            return (obj as Joints).GetHashCode() == GetHashCode();
        }
        public override int GetHashCode() => _guid.GetHashCode();
    }
}
