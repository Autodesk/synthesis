using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Synthesis.PreferenceManager;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;
using Synthesis.Util;
using Synthesis.Physics;

#nullable enable

namespace Synthesis {
    public class RotationalDriver : Driver {
        private const float MIRABUF_TO_UNITY_FORCE = 40f;

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
            get {
                if (PhysicsManager.IsFrozen)
                    return 0f;
                var val = State.GetValue(_inputs[0]);
                return val == null ? 0.0 : val.NumberValue;
            }
            set => State.SetValue(_inputs[0], Value.ForNumber(value));
        }

        public RotationalControlMode ControlMode {
            get {
                var val = State.GetValue(_inputs[1]);
                if (val == null)
                    throw new Exception($"No value with guid '{_inputs[1]}'");

                switch (val.StringValue) {
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
                        State.SetValue(_inputs[1], Value.ForString("Position"));
                        break;
                    case RotationalControlMode.Velocity:
                        State.SetValue(_inputs[1], Value.ForString("Velocity"));
                        break;
                    default:
                        throw new Exception("Unrecognized Rotational Control Mode");
                }
            }
        }

        public new string Name => State.SignalMap[_inputs[0]].Name;

        private JointMotor _motor;
        public JointMotor Motor {
            get => _motor;
            set {
                _motor = value;
                SimulationPreferences.SetRobotJointMotor((_simObject as RobotSimObject)!.MiraGUID, Name, _motor);
            }
        }
        private HingeJoint _jointA;
        public HingeJoint JointA => _jointA;
        private HingeJoint _jointB;
        public HingeJoint JointB => _jointB;

        private Rigidbody? _rbA;
        public Rigidbody RbA {
            get {
                if (_rbA == null)
                    _rbA = _jointA.GetComponent<Rigidbody>();
                return _rbA;
            }
        }
        private Rigidbody? _rbB;
        public Rigidbody RbB {
            get {
                if (_rbB == null)
                    _rbB = _jointB.GetComponent<Rigidbody>();
                return _rbB;
            }
        }

        private float _fakedTheta = 0f;
        private float _fakedOmega = 0f;

        private JointLimits? _rotationalLimits;

        private bool _useFakeMotion = false;
        public bool UseFakeMotion {
            get => _useFakeMotion;
            set {
                if (value != _useFakeMotion) {
                    _useFakeMotion = value;
                    if (_useFakeMotion) {
                        return;
                    }
                    if (_rotationalLimits.HasValue) {
                        _jointA.limits = _rotationalLimits.Value;
                    }
                    EnableMotor();
                }
            }
        }

        private bool _useMotor = true;

        // Is this actually used?
        public bool UseMotor { get => _useMotor; }

        public readonly string MotorRef;

        private float _convertedMotorTargetVel { get => Motor.targetVelocity * Mathf.Rad2Deg; }

        public RotationalDriver(string name, string[] inputs, string[] outputs, SimObject simObject, HingeJoint jointA,
            HingeJoint jointB, bool isWheel, string motorRef = "")
            : base(name, inputs, outputs, simObject) {
            _jointA = jointA;
            _jointB = jointB;

            if (_jointA.useLimits) {
                _rotationalLimits = _jointA.limits;
            }

            UseFakeMotion = jointA.useLimits;
            EnableMotor();

            (simObject as RobotSimObject)!.MiraLive.MiraAssembly.Data.Joints.MotorDefinitions.TryGetValue(
                motorRef, out var motor);
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

            MotorRef = motorRef;

            _isWheel = isWheel;

            State.SetValue(_inputs[1], Value.ForString("Velocity"));
            State.SetValue(_outputs[0], Value.ForNumber(0));
            State.SetValue(_outputs[1], Value.ForNumber(1));
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
        private float _lastUpdate = float.NaN;

        public override void Update() {
            float deltaT = 0f;
            if (!float.IsNaN(_lastUpdate)) {
                deltaT = Time.realtimeSinceStartup - _lastUpdate;
            }
            _lastUpdate = Time.realtimeSinceStartup;

            switch (ControlMode) {
                case RotationalControlMode.Position:
                    PositionControl();
                    break;
                case RotationalControlMode.Velocity:
                    VelocityControl(deltaT);
                    break;
            }

            // Angle loops around so this works for now
            _jointAngle += (_jointA.velocity * Time.deltaTime) / 360f;

            State.SetValue(_outputs[0], Value.ForNumber(_jointAngle));
            State.SetValue(_outputs[1], Value.ForNumber(_jointA.angle));
        }

        private void PositionControl() {
            if (_jointA.useMotor) {
                var targetPosition = MainInput;

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

                _jointA.motor =
                    new JointMotor { force = MIRABUF_TO_UNITY_FORCE * Motor.force * (inertiaA / (inertiaA + inertiaB)),
                        freeSpin = Motor.freeSpin, targetVelocity = _convertedMotorTargetVel * output };
                _jointB.motor =
                    new JointMotor { force = MIRABUF_TO_UNITY_FORCE * Motor.force * (inertiaB / (inertiaA + inertiaB)),
                        freeSpin = Motor.freeSpin, targetVelocity = -_convertedMotorTargetVel * output };
            }
        }

        private void VelocityControl(float deltaT) {
            if (_jointA.useMotor) {
                var val = (float) MainInput;

                var inertiaA =
                    SynthesisUtil.GetInertiaAroundParallelAxis(_jointA.connectedBody, _jointB.anchor, _jointB.axis);

                var inertiaB =
                    SynthesisUtil.GetInertiaAroundParallelAxis(_jointB.connectedBody, _jointA.anchor, _jointA.axis);

                if (_useFakeMotion) {
                    float alpha = val * _convertedMotorTargetVel;

                    _fakedTheta += alpha * deltaT;

                    if (_rotationalLimits.HasValue) {
                        if (_fakedTheta > _rotationalLimits.Value.max) {
                            _fakedTheta = _rotationalLimits.Value.max;
                        } else if (_fakedTheta < _rotationalLimits.Value.min) {
                            _fakedTheta = _rotationalLimits.Value.min;
                        }

                        _fakedTheta = Mathf.Clamp(_fakedTheta, -180, 179);

                        _jointA.limits =
                            new JointLimits { bounceMinVelocity = _rotationalLimits.Value.bounceMinVelocity,
                                bounciness                      = _rotationalLimits.Value.bounciness,
                                contactDistance = _rotationalLimits.Value.contactDistance, max = _fakedTheta + 1,
                                min = _fakedTheta };
                    }

                } else {
                    _jointA.motor = new JointMotor { force = Motor.force * (inertiaA / (inertiaA + inertiaB)),
                        freeSpin = Motor.freeSpin, targetVelocity = _convertedMotorTargetVel * val };
                    _jointB.motor = new JointMotor { force = Motor.force * (inertiaB / (inertiaA + inertiaB)),
                        freeSpin = Motor.freeSpin, targetVelocity = -_convertedMotorTargetVel * val };
                }
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
