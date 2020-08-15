using System;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.AssetManager;
using SynthesisAPI.Utilities;
using System.Collections.Generic;

namespace SynthesisCore.Systems {

    public static class MotorManager {

        public static void AddMotor(MotorController controller) {
            Instance.AllMotorControllers.Add(controller);
        }

        public static void RemoveMotor(int index) {
            Instance.AllMotorControllers.RemoveAt(index);
        }

        public static List<MotorController> AllMotorControllers {
            get => Instance.AllMotorControllers;
        }

        private class Inner {

            public List<MotorController> AllMotorControllers;

            private Inner() {
                AllMotorControllers = new List<MotorController>();

                foreach (var j in Joints.GlobalAllJoints) {
                    if (j.GetType() == typeof(HingeJoint)) {
                        AllMotorControllers.Add(new MotorController((HingeJoint)j));
                    }
                }

                Joints.GlobalAddJoint += j => {
                    if (j.GetType() == typeof(HingeJoint)) {
                        AllMotorControllers.Add(new MotorController((HingeJoint)j));
                    }
                };
            }

            private static Inner _instance = null;
            public static Inner InnerInstance {
                get {
                    if (_instance == null)
                        _instance = new Inner();
                    return _instance;
                }
            }
        }

        private static Inner Instance => Inner.InnerInstance;
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
                initMotor.TargetVelocity = Math.Abs(velocity) <= (MotorType.MaxVelocity * Gearing) ? velocity : (Math.Abs(velocity) / velocity) * (MotorType.MaxVelocity * Gearing);
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