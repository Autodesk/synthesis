using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using System.Collections.Generic;

using Math = System.Math;

namespace SynthesisCore.Components
{
    public class MotorManager : Component {

        public void AddMotor(MotorController controller)
        {
            AllMotorControllers.Add(controller);
        }

        public void RemoveMotor(int index)
        {
            AllMotorControllers.RemoveAt(index);
        }

        private List<MotorController> _allMotorControllers = null;

        public List<MotorController> AllMotorControllers
        {
            get
            {
                if (_allMotorControllers == null)
                {
                    _allMotorControllers = new List<MotorController>();

                    foreach (var joints in EnvironmentManager.GetComponentsWhere<Joints>(c => IsDescendant(Entity.Value, c.Entity.Value)))
                    {
                        foreach (var j in joints.AllJoints)
                        {
                            if (j is HingeJoint hingeJoint)
                            {
                                _allMotorControllers.Add(new MotorController(hingeJoint));
                            }
                        }
                    }
                }
                return _allMotorControllers;
            }
            set => _allMotorControllers = value;
        }

        private bool IsDescendant(Entity ancestor, Entity test)
        {
            if (test == ancestor)
                return true;
            var parent = test.GetComponent<Parent>().ParentEntity;
            return parent != null && IsDescendant(ancestor, parent);
        }

        public MotorManager()
        {
            Joints.GlobalAddJoint += j =>
            {
                if (j is HingeJoint hingeJoint)
                {
                    AllMotorControllers.Add(new MotorController(hingeJoint));
                }
            };
        }
    }

    public class MotorController {

        public HingeJoint Joint;

        public MotorType MotorType;
        public float Gearing = 1;
        public int MotorCount = 1;

        public bool Locked {
            get => !Joint.Motor.FreeSpin;
            set => Joint.Motor.FreeSpin = !value;
        }

        public MotorController(HingeJoint joint) {
           Joint = joint;
           joint.UseMotor = true;
        }

        public void SetPercent(float percent) {
            if (MotorType != null && MotorCount > 0) {
                percent = Math.Min(Math.Max(-1, percent), 1);
                var initMotor = Joint.Motor;
                initMotor.TargetVelocity = (MotorType.MaxVelocity * Gearing) * percent;
                initMotor.Force = (MotorType.Torque * MotorCount) / Gearing;
                Joint.Motor = initMotor;
            }
        }

        public void SetVelocity(float velocity) {
            if (MotorType != null && MotorCount > 0) {
                var initMotor = Joint.Motor;
                initMotor.TargetVelocity = Math.Abs(velocity) <= (MotorType.MaxVelocity * Gearing) ? velocity : Math.Sign(velocity) * (MotorType.MaxVelocity * Gearing);
                initMotor.Force = (MotorType.Torque * MotorCount) / Gearing;
                Joint.Motor = initMotor;
            }
        }
    }

    public class MotorType {
        public string MotorName;
        public float MaxVelocity;
        public float Torque;
    }

}