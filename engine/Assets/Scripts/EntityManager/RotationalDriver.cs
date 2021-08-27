using Google.Protobuf.WellKnownTypes;
using SynthesisAPI.Utilities;
using UnityEngine;

namespace Synthesis.Entity {
    /// <summary>
    /// May separate this out into a driver for arms and a driver for wheels
    /// </summary>
    public class RotationalDriver : Driver {

        public JointMotor Motor;
        private HingeJoint _joint;
        
        public RotationalDriver(string name, string[] inputs, string[] outputs, ControllableState state,
            HingeJoint joint, JointMotor motor)
            : base(name, inputs, outputs, state) {
            _joint = joint;
            Motor = motor;
        }

        public override void Update() {
            if (_joint.useMotor) {
                _joint.motor = new JointMotor {
                    force = Motor.force,
                    freeSpin = Motor.freeSpin,
                    targetVelocity = Motor.targetVelocity * (float)(_state.CurrentSignals.ContainsKey(_inputs[0])
                        ? _state.CurrentSignals[_inputs[0]].Value.NumberValue
                        : 0.0f)
                };
            }

            var updateSignal = new UpdateSignals();
            var key = _outputs[0];
            var current = _state.CurrentSignals[key];
            updateSignal.SignalMap.Add(key, new UpdateSignal() {
                DeviceType = current.DeviceType, Io = current.Io,
                Value = new Value { NumberValue = _joint.angle }
            });
            _state.Update(updateSignal);
        }
    }
}