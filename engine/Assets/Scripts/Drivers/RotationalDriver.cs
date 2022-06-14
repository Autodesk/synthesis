using System.Collections;
using System.Collections.Generic;
using Synthesis.PreferenceManager;
using SynthesisAPI.Simulation;
using UnityEngine;

namespace Synthesis {
    public class RotationalDriver : Driver {

        public string InputSignal => _inputs[0];

        private JointMotor _motor;
        public JointMotor Motor {
            get => _motor;
            set {
                _motor = value;
                SimulationPreferences.SetRobotJointMotor((_simObject as RobotSimObject).MiraGUID, Name, _motor);
            }
        }
        private HingeJoint _jointA;
        public HingeJoint JointA => _jointA;
        private HingeJoint _jointB;
        public HingeJoint JointB => _jointB;

        private Rigidbody _rbA = null;
        public Rigidbody RbA {
            get {
                if (_rbA == null)
                    _rbA = _jointA.GetComponent<Rigidbody>();
                return _rbA;
            }
        }
        private Rigidbody _rbB = null;
        public Rigidbody RbB {
            get {
                if (_rbB == null)
                    _rbB = _jointB.GetComponent<Rigidbody>();
                return _rbB;
            }
        }

        private bool _useMotor = true;

        // Is this actually used?
        public bool UseMotor {
            get => _useMotor;
        }

        public RotationalDriver(string name, string[] inputs, string[] outputs, SimObject simObject,
            HingeJoint jointA, HingeJoint jointB, JointMotor? motor = null)
            : base(name, inputs, outputs, simObject) {
            _jointA = jointA;
            _jointB = jointB;
            if (motor.HasValue) {
                _motor = motor.Value;
            } else {
                var m = SimulationPreferences.GetRobotJointMotor((_simObject as RobotSimObject).MiraGUID, name);
                if (m.HasValue)
                    _motor = m.Value;
                else
                    Motor = new JointMotor() // Default Motor. Slow but powerful enough. Also uses Motor to save it
                        {
                            force = 2000.0f,
                            freeSpin = false,
                            targetVelocity = 500,
                        };
            }
        }

        void EnableMotor()
        {
            _useMotor = true;
            _jointA.useMotor = true;
        }

        void DisableMotor()
        {
            _useMotor = false;
            _jointA.useMotor = false;
        }

        public override void Update() {

            // if (_useMotor) {
            //     var val = (float)(State.CurrentSignals.ContainsKey(_inputs[0])
            //         ? State.CurrentSignals[_inputs[0]].Value.NumberValue
            //         : 0.0f);

            //     var rbA = _jointB.connectedBody;
            //     var rbB = _jointA.connectedBody;

            //     rbA.AddRelativeTorque(val * _jointA.axis.normalized * Motor.force * Time.deltaTime, ForceMode.Impulse);
            //     rbB.AddRelativeTorque(val * _jointB.axis.normalized * -Motor.force * Time.deltaTime, ForceMode.Impulse);
            // }

            if (_jointA.useMotor) {
                _jointA.motor = new JointMotor {
                    force = Motor.force * (RbA.mass / (RbA.mass + RbB.mass)),
                    freeSpin = Motor.freeSpin,
                    targetVelocity = Motor.targetVelocity * (float)(State.CurrentSignals.ContainsKey(_inputs[0])
                        ? State.CurrentSignals[_inputs[0]].Value.NumberValue
                        : 0.0f)
                };
                _jointB.motor = new JointMotor {
                    force = Motor.force * (RbB.mass / (RbA.mass + RbB.mass)),
                    freeSpin = Motor.freeSpin,
                    targetVelocity = -Motor.targetVelocity * (float)(State.CurrentSignals.ContainsKey(_inputs[0])
                        ? State.CurrentSignals[_inputs[0]].Value.NumberValue
                        : 0.0f)
                };
            }

            // var updateSignal = new UpdateSignals();
            // var key = _outputs[0];
            // var current = _state.CurrentSignals[key];
            // updateSignal.SignalMap.Add(key, new UpdateSignal() {
            //     Class = current.Class, Io = current.Io,
            //     Value = new Value { NumberValue = _jointA.angle }
            // });
            // _state.Update(updateSignal);
        }
    }
}