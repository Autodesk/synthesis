using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Synthesis.PreferenceManager;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;
using Synthesis.Util;

#nullable enable

namespace Synthesis {
    public class RotationalDriver : Driver {
        // clang-format off
        private bool _isWheel = false;
        // clang-format on
        public bool IsWheel => _isWheel;

        public string Signal => _inputs[0];

        private SimBehaviour? _reservee;
        public SimBehaviour? Reservee => _reservee;
        public bool IsReserved        => _reservee != null;

        /// <summary>
        /// Global Coordinate Anchor Point for Joint
        /// </summary>
        public Vector3 Anchor { get => _jointA.transform.localToWorldMatrix.MultiplyPoint3x4(_jointA.anchor); }

        /// <summary>
        /// Global Coordinate Axis for Joint
        /// </summary>
        public Vector3 Axis {
            get => _jointA.transform.localToWorldMatrix.MultiplyVector(_jointA.axis);
            set => SetAxis(value);
        }

        public enum RotationalControlMode {
            Position,
            Velocity
        }

        public double MainInput {
            get => State.CurrentSignals[_inputs[0]].Value.NumberValue;
            set { State.CurrentSignals[_inputs[0]].Value = Value.ForNumber(value); }
        }

        public RotationalControlMode ControlMode {
            get {
                switch (State.CurrentSignals[_inputs[1]].Value.StringValue) {
                    case "Velocity":
                        return RotationalControlMode.Velocity;
                    case "Position":
                        return RotationalControlMode.Position;
                    default:
                        throw new Exception("No valid control mode");
                }
            }
            set {
                switch (value) {
                    case RotationalControlMode.Position:
                        State.CurrentSignals[_inputs[1]].Value = Value.ForString("Position");
                        break;
                    case RotationalControlMode.Velocity:
                        State.CurrentSignals[_inputs[1]].Value = Value.ForString("Velocity");
                        break;
                    default:
                        throw new Exception("Unrecognized Rotational Control Mode");
                }
            }
        }

        public string Name => State.CurrentSignalLayout.SignalMap[_inputs[0]].Info.Name;

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
        public bool UseMotor { get => _useMotor; }

        public RotationalDriver(string name, string[] inputs, string[] outputs, SimObject simObject, HingeJoint jointA,
            HingeJoint jointB, bool isWheel, Mirabuf.Motor.Motor? motor = null)
            : base(name, inputs, outputs, simObject) {
            _jointA = jointA;
            _jointB = jointB;
            if (motor != null && motor.MotorTypeCase == Mirabuf.Motor.Motor.MotorTypeOneofCase.SimpleMotor) {
                _motor = motor!.SimpleMotor.UnityMotor;
            } else {
                Motor = new JointMotor() {
                    // Default Motor. Slow but powerful enough. Also uses Motor to save it
                    force          = 2000,
                    freeSpin       = false,
                    targetVelocity = 500,
                };
            }

            _isWheel = isWheel;

            State.CurrentSignals[_inputs[1]]  = new UpdateSignal() { DeviceType = "Mode", Io = UpdateIOType.Input,
                 Value = Google.Protobuf.WellKnownTypes.Value.ForString("Velocity") };
            State.CurrentSignals[_outputs[0]] = new UpdateSignal() { DeviceType = "PWM", Io = UpdateIOType.Output,
                Value = Google.Protobuf.WellKnownTypes.Value.ForNumber(0) };
            State.CurrentSignals[_outputs[1]] = new UpdateSignal() { DeviceType = "Range", Io = UpdateIOType.Output,
                Value = Google.Protobuf.WellKnownTypes.Value.ForNumber(0) };
        }

        void EnableMotor() {
            _useMotor        = true;
            _jointA.useMotor = true;
        }

        void DisableMotor() {
            _useMotor        = false;
            _jointA.useMotor = false;
        }

        public bool Reserve(SimBehaviour? behaviour) {
            if (behaviour == null || _reservee != null)
                return false;

            _reservee = behaviour;
            return true;
        }

        public void Unreserve() {
            _reservee = null;
        }

        private float _jointAngle = 0.0f;

        public override void Update() {
            switch (ControlMode) {
                case RotationalControlMode.Position:
                    PositionControl();
                    break;
                case RotationalControlMode.Velocity:
                    VelocityControl();
                    break;
            }

            // Angle loops around so this works for now
            _jointAngle += (_jointA.velocity * Time.deltaTime) / 360f;
            State.CurrentSignals[_outputs[0]].Value = Google.Protobuf.WellKnownTypes.Value.ForNumber(_jointAngle);
            State.CurrentSignals[_outputs[1]].Value = Google.Protobuf.WellKnownTypes.Value.ForNumber(_jointA.angle);
        }

        private void PositionControl() {
            if (_jointA.useMotor) {
                var targetPosition = (float) (State.CurrentSignals.ContainsKey(_inputs[0])
                                                  ? State.CurrentSignals[_inputs[0]].Value.NumberValue
                                                  : 0.0f);

                var inertiaA =
                    SynthesisUtil.GetInertiaAroundParallelAxis(_jointA.connectedBody, _jointB.anchor, _jointB.axis);
                var inertiaB =
                    SynthesisUtil.GetInertiaAroundParallelAxis(_jointB.connectedBody, _jointA.anchor, _jointA.axis);

                float error = (float) (targetPosition - _jointA.angle);
                while (error < -180) {
                    error += 360;
                }
                while (error > 180) {
                    error -= 360;
                }

                float output = error * 0.1f;

                _jointA.motor = new JointMotor { force = Motor.force * (inertiaA / (inertiaA + inertiaB)),
                    freeSpin = Motor.freeSpin, targetVelocity = (Motor.targetVelocity) * output };
                _jointB.motor = new JointMotor { force = Motor.force * (inertiaB / (inertiaA + inertiaB)),
                    freeSpin = Motor.freeSpin, targetVelocity = (-Motor.targetVelocity) * output };
            }
        }

        private void VelocityControl() {
            if (_jointA.useMotor) {
                var val = (float) (State.CurrentSignals.ContainsKey(_inputs[0])
                                       ? State.CurrentSignals[_inputs[0]].Value.NumberValue
                                       : 0.0f);

                var inertiaA =
                    SynthesisUtil.GetInertiaAroundParallelAxis(_jointA.connectedBody, _jointB.anchor, _jointB.axis);
                var inertiaB =
                    SynthesisUtil.GetInertiaAroundParallelAxis(_jointB.connectedBody, _jointA.anchor, _jointA.axis);
                _jointA.motor = new JointMotor { force = Motor.force * (inertiaA / (inertiaA + inertiaB)),
                    freeSpin = Motor.freeSpin, targetVelocity = (Motor.targetVelocity) * val };
                _jointB.motor = new JointMotor { force = Motor.force * (inertiaB / (inertiaA + inertiaB)),
                    freeSpin = Motor.freeSpin, targetVelocity = (-Motor.targetVelocity) * val };
            }
        }

        /// <summary>
        /// Set a new axis for the joints
        /// </summary>
        /// <param name="newAxis">Global vector</param>
        public void SetAxis(Vector3 newAxis) {
            _jointA.axis = _jointA.transform.worldToLocalMatrix.MultiplyVector(newAxis).normalized;
            _jointB.axis = _jointB.transform.worldToLocalMatrix.MultiplyVector(newAxis).normalized;
        }
    }
}
