using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using System.Collections.Generic;

namespace SynthesisCore.Simulation
{
    public class MotorAssemblyManager : Component
    {
        public void AddMotor(MotorAssembly controller)
        {
            AllGearBoxes.Add(controller);
        }

        public void RemoveMotor(int index)
        {
            AllGearBoxes.RemoveAt(index);
        }

        private List<MotorAssembly> _allGearBoxes = null;

        public List<MotorAssembly> AllGearBoxes
        {
            get
            {
                if (_allGearBoxes == null)
                {
                    _allGearBoxes = new List<MotorAssembly>();

                    foreach (var entity in EnvironmentManager.GetEntitiesWhere(
                        e => IsDescendant(Entity.Value, e) &&
                        e.GetComponent<Joints>() != null))
                    {
                        foreach (var j in entity.GetComponent<Joints>().AllJoints)
                        {
                            if (j is HingeJoint hingeJoint)
                            {
                                _allGearBoxes.Add(new MotorAssembly(entity, hingeJoint));
                            }
                        }
                    }
                }
                return _allGearBoxes;
            }
            set => _allGearBoxes = value;
        }

        private bool IsDescendant(Entity ancestor, Entity test)
        {
            if (test == ancestor)
                return true;
            var parent = test.GetComponent<Parent>()?.ParentEntity;
            return parent != null && IsDescendant(ancestor, parent.Value);
        }

        public MotorAssemblyManager()
        {
            /*
            Joints.GlobalAddJoint += j =>
            {
                if (j is HingeJoint hingeJoint)
                {
                    AllGearBoxes.Add(new MotorAssembly(hingeJoint, MotorFactory.CIMMotor(), 9.29, 1));
                }
            };
            */
        }
    }
}