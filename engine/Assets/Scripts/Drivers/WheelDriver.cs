using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Mirabuf.Joint;
using Synthesis.PreferenceManager;
using SynthesisAPI.Simulation;
using UnityEngine;

#nullable enable

namespace Synthesis {
    public class WheelDriver : Driver {
        private CustomWheel _customWheel;

        private JointInstance _jointInstance;
        public JointInstance JointInstance => _jointInstance;

        private Vector3 _localAnchor = Vector3.zero;
        public Vector3 Anchor {
            get => _customWheel.Rb.transform.localToWorldMatrix.MultiplyPoint3x4(_localAnchor);
            set {
                _localAnchor             = _customWheel.Rb.transform.worldToLocalMatrix.MultiplyPoint3x4(value);
                _customWheel.LocalAnchor = _localAnchor;
            }
        }
        public Vector3 LocalAnchor {
            get => _localAnchor;
            set {
                _localAnchor             = value;
                _customWheel.LocalAnchor = _localAnchor;
            }
        }

        private Vector3 _localAxis = Vector3.right;
        public Vector3 Axis {
            get => _customWheel.Rb.transform.localToWorldMatrix.MultiplyVector(_localAxis);
            set {
                _localAxis             = _customWheel.Rb.transform.worldToLocalMatrix.MultiplyVector(value);
                _customWheel.LocalAxis = _localAxis;
            }
        }
        public Vector3 LocalAxis {
            get => _localAxis;
            set {
                _localAxis             = value;
                _customWheel.LocalAxis = _localAxis;
            }
        }

        private float _radius = 0.05f;
        public float Radius {
            get => _radius;
            set {
                _radius             = value;
                _customWheel.Radius = _radius;
            }
        }

        public float ImpulseMax {
            get => _customWheel.ImpulseMax;
            set => _customWheel.ImpulseMax = value;
        }

        public enum RotationalControlMode {
            Position,
            Velocity
        }

        public double MainInput {
            get => State.CurrentSignals[_inputs[0]].Value.NumberValue;
            set { State.CurrentSignals[_inputs[0]].Value = Value.ForNumber(value); }
        }

        public bool HasContacts => _customWheel.HasContacts;

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

        private JointMotor _motor;
        public JointMotor Motor {
            get => _motor;
            set {
                _motor = value;
                SimulationPreferences.SetRobotJointMotor((_simObject as RobotSimObject)!.MiraGUID, Name, _motor);
            }
        }

        private bool _useMotor = true;

        // Is this actually used?
        public bool UseMotor { get => _useMotor; }

        private float _targetRotationalSpeed = 0f;

        /// <summary>
        /// Creates a WheelDriver
        /// </summary>
        /// <param name="name">Name of signal</param>
        /// <param name="inputs">Input Signals</param>
        /// <param name="outputs">Output Signals</param>
        /// <param name="simObject">SimObject of which the driver belongs</param>
        /// <param name="jointInstance">Mirabuf Instance of the Joint constructing this wheel relation</param>
        /// <param name="customWheel">Custom Physics Wheel</param>
        /// <param name="anchor">Anchor of the Rotational Joint</param>
        /// <param name="axis">Axis of the Rotational Joint</param>
        /// <param name="radius">Radius of the wheel. Automatically calculated if set to NaN</param>
        /// <param name="motor">Motor settings for the wheel</param>
        public WheelDriver(string name, string[] inputs, string[] outputs, SimObject simObject,
            JointInstance jointInstance, CustomWheel customWheel, Vector3 anchor, Vector3 axis, float radius,
            Mirabuf.Motor.Motor? motor = null)
            : base(name, inputs, outputs, simObject) {
            _jointInstance = jointInstance;
            _customWheel   = customWheel;

            Anchor = _customWheel.Rb.transform.localToWorldMatrix.MultiplyPoint3x4(anchor);
            Axis   = _customWheel.Rb.transform.localToWorldMatrix.MultiplyVector(axis);

            if (float.IsNaN(radius)) {
                Radius = _customWheel.Rb.transform.GetBounds().extents.y;
            } else {
                Radius = radius;
            }

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

            State.CurrentSignals[_inputs[1]]  = new UpdateSignal() { DeviceType = "Mode", Io = UpdateIOType.Input,
                 Value = Google.Protobuf.WellKnownTypes.Value.ForString("Velocity") };
            State.CurrentSignals[_outputs[0]] = new UpdateSignal() { DeviceType = "PWM", Io = UpdateIOType.Output,
                Value = Google.Protobuf.WellKnownTypes.Value.ForNumber(0) };
            State.CurrentSignals[_outputs[1]] = new UpdateSignal() { DeviceType = "Range", Io = UpdateIOType.Output,
                Value = Google.Protobuf.WellKnownTypes.Value.ForNumber(0) };

            // Debug.Log($"Speed: {_motor.targetVelocity}\nForce: {_motor.force}");
        }

        void EnableMotor() {
            _useMotor = true;
        }

        void DisableMotor() {
            _useMotor = false;
        }

        private float _jointAngle = 0.0f;
        private float _lastUpdate = float.NaN;

        public override void Update() {
            switch (ControlMode) {
                case RotationalControlMode.Velocity:
                    VelocityControl();
                    break;
            }

            _lastUpdate = Time.realtimeSinceStartup;

            // I think these work?
            State.CurrentSignals[_outputs[0]].Value =
                Google.Protobuf.WellKnownTypes.Value.ForNumber(_jointAngle / (Mathf.PI * 2f));
            State.CurrentSignals[_outputs[1]].Value =
                Google.Protobuf.WellKnownTypes.Value.ForNumber(PositiveMod(_jointAngle, Mathf.PI));
        }

        public float PositiveMod(float val, float mod) {
            var res = val % mod;
            if (res < 0)
                res += mod;
            return res;
        }

        public void WheelsPhysicsUpdate(float mod) {
            _customWheel.GetFrictionForces(mod);
        }

        private void VelocityControl() {
            if (!_useMotor)
                return;

            var val = (float) (State.CurrentSignals.ContainsKey(_inputs[0])
                                   ? State.CurrentSignals[_inputs[0]].Value.NumberValue
                                   : 0.0f);

            _targetRotationalSpeed = val * Mathf.Deg2Rad * _motor.targetVelocity;

            var delta         = _targetRotationalSpeed - _customWheel.RotationSpeed;
            var possibleDelta = (_motor.force * Time.deltaTime) / _customWheel.Inertia;
            if (Mathf.Abs(delta) > possibleDelta)
                delta = possibleDelta * Mathf.Sign(delta);

            var lastRotSpeed = _customWheel.RotationSpeed;
            _customWheel.RotationSpeed += delta;

            if (!float.IsNaN(_lastUpdate)) {
                var deltaT = Time.realtimeSinceStartup - _lastUpdate;

                if (deltaT == 0f)
                    return;

                var alpha = (_customWheel.RotationSpeed - lastRotSpeed) / deltaT;
                _jointAngle += 0.5f * alpha * deltaT * deltaT + lastRotSpeed * deltaT;
            }
        }

#region Rotational Inertia stuff that isn't used

        public static float GetInertiaAroundParallelAxis(Rigidbody rb, Vector3 localAnchor, Vector3 localAxis) {
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
        public static float GetInertiaFromAxisVector(Rigidbody rb, Vector3 localAxis) {
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
