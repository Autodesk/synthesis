using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Synthesis.PreferenceManager;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

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
            get {
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
                        // DisableMotor();
                    } else {
                        if (_rotationalLimits.HasValue) {
                            _jointA.limits = _rotationalLimits.Value;
                        }
                        EnableMotor();
                    }
                }
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

            if (_jointA.useLimits) {
                _rotationalLimits = _jointA.limits;
            }

            UseFakeMotion = jointA.useLimits;

            if (motor != null && motor.MotorTypeCase == Mirabuf.Motor.Motor.MotorTypeOneofCase.SimpleMotor) {
                _motor = motor!.SimpleMotor.UnityMotor;
            } else {
                // var m = SimulationPreferences.GetRobotJointMotor((_simObject as RobotSimObject).MiraGUID, name);
                Motor = new JointMotor() {
                    // Default Motor. Slow but powerful enough. Also uses Motor to save it
                    force          = 2000,
                    freeSpin       = false,
                    targetVelocity = 500,
                };
            }

            _isWheel = isWheel;

            State.SetValue(_inputs[1], Value.ForString("Velocity"));
            State.SetValue(_outputs[0], Value.ForNumber(0));
            State.SetValue(_outputs[1], Value.ForNumber(1));

            // Debug.Log($"Speed: {_motor.targetVelocity}\nForce: {_motor.force}");
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

            // var updateSignal = new UpdateSignals();
            // var key = _outputs[0];
            // var current = _state.CurrentSignals[key];
            // updateSignal.SignalMap.Add(key, new UpdateSignal() {
            //     Class = current.Class, Io = current.Io,
            //     Value = new Value { NumberValue = _jointA.angle }
            // });
            // _state.Update(updateSignal);
        }

        private void PositionControl() {
            if (_jointA.useMotor) {
                var targetPosition = MainInput;

                // Debug.Log($"D: {_inputs[0]}");
                // Debug.Log($"Target: {targetPosition}");

                var inertiaA = GetInertiaAroundParallelAxis(_jointA.connectedBody, _jointB.anchor, _jointB.axis);
                // var angAccelA = (Motor.force * val) / inertiaA;
                // _jointA.connectedBody.AddTorque(_jointB.axis.normalized * angAccelA * Mathf.Rad2Deg,
                // ForceMode.Acceleration); Debug.Log($"{angAccelA} to {_jointA.connectedBody.name}");

                var inertiaB = GetInertiaAroundParallelAxis(_jointB.connectedBody, _jointA.anchor, _jointA.axis);
                // var angAccelB = (-Motor.force * val) / inertiaB;
                // _jointB.connectedBody.AddTorque(_jointA.axis.normalized * angAccelB * Mathf.Rad2Deg,
                // ForceMode.Acceleration); Debug.Log($"{angAccelB} to {_jointB.connectedBody.name}");

                float error = (float) (targetPosition - _jointA.angle);
                while (error < -180) {
                    error += 360;
                }
                while (error > 180) {
                    error -= 360;
                }

                // Debug.Log($"Error: {error}");

                float output = error * 0.1f;

                _jointA.motor = new JointMotor { force = Motor.force * (inertiaA / (inertiaA + inertiaB)),
                    freeSpin = Motor.freeSpin, targetVelocity = (Motor.targetVelocity) * output };
                _jointB.motor = new JointMotor { force = Motor.force * (inertiaB / (inertiaA + inertiaB)),
                    freeSpin = Motor.freeSpin, targetVelocity = (-Motor.targetVelocity) * output };
            }
        }

        private void VelocityControl(float deltaT) {
            if (_jointA.useMotor) {
                var val = (float) MainInput;

                var inertiaA = GetInertiaAroundParallelAxis(_jointA.connectedBody, _jointB.anchor, _jointB.axis);
                // var angAccelA = (Motor.force * val) / inertiaA;
                // _jointA.connectedBody.AddTorque(_jointB.axis.normalized * angAccelA * Mathf.Rad2Deg,
                // ForceMode.Acceleration); Debug.Log($"{angAccelA} to {_jointA.connectedBody.name}");

                var inertiaB = GetInertiaAroundParallelAxis(_jointB.connectedBody, _jointA.anchor, _jointA.axis);
                // var angAccelB = (-Motor.force * val) / inertiaB;
                // _jointB.connectedBody.AddTorque(_jointA.axis.normalized * angAccelB * Mathf.Rad2Deg,
                // ForceMode.Acceleration); Debug.Log($"{angAccelB} to {_jointB.connectedBody.name}");

                if (_useFakeMotion) {

                    float alpha = val * Motor.targetVelocity;

                    _fakedTheta += alpha * deltaT;

                    if (_rotationalLimits.HasValue) {
                        if (_fakedTheta > _rotationalLimits.Value.max) {
                            _fakedTheta = _rotationalLimits.Value.max;
                        } else if (_fakedTheta < _rotationalLimits.Value.min) {
                            _fakedTheta = _rotationalLimits.Value.min;
                        }

                        _fakedTheta = Mathf.Clamp(_fakedTheta, -180, 179);

						_jointA.limits = new JointLimits {
                            bounceMinVelocity = _rotationalLimits.Value.bounceMinVelocity,
                            bounciness = _rotationalLimits.Value.bounciness,
                            contactDistance = _rotationalLimits.Value.contactDistance,
                            max = _fakedTheta + 1,
                            min = _fakedTheta
                        };

                        // Debug.Log($"Theta: {_fakedTheta}");
                    }
                    
                } else {
                    _jointA.motor = new JointMotor {
                        force = Motor.force * (inertiaA / (inertiaA + inertiaB)),
                        freeSpin = Motor.freeSpin,
                        targetVelocity = (Motor.targetVelocity) * val
                    };
                    _jointB.motor = new JointMotor {
                        force = Motor.force * (inertiaB / (inertiaA + inertiaB)),
                        freeSpin = Motor.freeSpin,
                        targetVelocity = (-Motor.targetVelocity) * val
                    };
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

#region Rotational Inertia stuff that isn't used

        public float GetInertiaAroundParallelAxis(Rigidbody rb, Vector3 localAnchor, Vector3 localAxis) {
            var comInertia       = GetInertiaFromAxisVector(rb, localAxis);
            var pointMassInertia = rb.mass * Mathf.Pow(Vector3.Distance(rb.centerOfMass, localAnchor), 2f);
            return comInertia + pointMassInertia;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="rb"></param>
        /// <param name="axis">Use local axis</param>
        /// <returns></returns>
        public float GetInertiaFromAxisVector(Rigidbody rb, Vector3 localAxis) {
            var sph     = CartesianToSphericalCoordinate(localAxis);
            var inertia = rb.inertiaTensorRotation * rb.inertiaTensor;
            return EllipsoidRadiusFromSphericalCoordinate(sph, inertia);
        }

        public static float EllipsoidRadiusFromSphericalCoordinate(Vector3 sph, Vector3 ellipsoidRadi) {
            var cartEllip = new Vector3(ellipsoidRadi.x * Mathf.Sin(sph.y) * Mathf.Cos(sph.z),
                ellipsoidRadi.y * Mathf.Cos(sph.y), ellipsoidRadi.z * Mathf.Sin(sph.y) * Mathf.Sin(sph.z));
            return cartEllip.magnitude;
        }

        public static void TestSphericalCoordinate() {
            var a = CartesianToSphericalCoordinate(new Vector3(0, -1, 0).normalized);
            var b = CartesianToSphericalCoordinate(new Vector3(0, 0, -1).normalized);
            var c = CartesianToSphericalCoordinate(new Vector3(1, 0, 1).normalized);
            var d = CartesianToSphericalCoordinate(new Vector3(-1, 0, 0));

            PrintSphericalCoordinate(a);
            PrintSphericalCoordinate(b);
            PrintSphericalCoordinate(c);
            PrintSphericalCoordinate(d);

            var radi = new Vector3(1, 2, 3);

            Debug.Log($"Radius: {EllipsoidRadiusFromSphericalCoordinate(a, radi)}");
            Debug.Log($"Radius: {EllipsoidRadiusFromSphericalCoordinate(b, radi)}");
            Debug.Log($"Radius: {EllipsoidRadiusFromSphericalCoordinate(c, radi)}");
            Debug.Log($"Radius: {EllipsoidRadiusFromSphericalCoordinate(d, radi)}");
        }

        public static void PrintSphericalCoordinate(Vector3 a) {
            Debug.Log($"Radius: {a.x}\nTheta: {a.y}\nPhi: {a.z}");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cart"></param>
        /// <returns>X is radius, Y is theta, Z is phi</returns>
        public static Vector3 CartesianToSphericalCoordinate(Vector3 cart) {
            cart = cart.normalized;
            return new Vector3(cart.magnitude, Mathf.Acos(cart.y / 1), Mathf.Asin(cart.z / 1));
        }

#endregion
    }
}