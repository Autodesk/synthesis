using System.Collections;
using System.Collections.Generic;
using SynthesisAPI.Simulation;
using UnityEngine;

namespace Synthesis {
    public class RotationalDriver : Driver {

        public JointMotor Motor;
        private HingeJoint _jointA;
        public HingeJoint JointA => _jointA;
        private HingeJoint _jointB;
        public HingeJoint JointB => _jointB;

        private bool _useMotor;

        public bool UseMotor
        {
            get => _useMotor;
        }

        public RotationalDriver(string name, string[] inputs, string[] outputs, SimObject simObject,
            HingeJoint jointA, HingeJoint jointB, JointMotor motor)
            : base(name, inputs, outputs, simObject) {
            _jointA = jointA;
            _jointB = jointB;
            Motor = motor;
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
            if (_jointA.useMotor) {
                _jointA.motor = new JointMotor {
                    force = Motor.force,
                    freeSpin = Motor.freeSpin,
                    targetVelocity = Motor.targetVelocity * (float)(State.CurrentSignals.ContainsKey(_inputs[0])
                        ? State.CurrentSignals[_inputs[0]].Value.NumberValue
                        : 0.0f)
                };
                _jointB.motor = new JointMotor {
                    force = Motor.force,
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