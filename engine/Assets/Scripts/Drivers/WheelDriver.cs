using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Mirabuf.Joint;
using Synthesis.PreferenceManager;
using SynthesisAPI.Simulation;
using UnityEngine;
using Synthesis.Util;
using Synthesis.Physics;

#nullable enable

namespace Synthesis {
    public class WheelDriver : Driver {
        private const float MIRABUF_TO_UNITY_FORCE = 40f;

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
            get {
                if (PhysicsManager.IsFrozen)
                    return 0f;
                var val = State.GetValue(_inputs[0]);
                return val == null ? 0.0 : val.NumberValue;
            }
            set => State.SetValue(_inputs[0], Value.ForNumber(value));
        }

        public bool HasContacts => _customWheel.HasContacts;

        private JointMotor _motor;
        public JointMotor Motor {
            get => _motor;
            set {
                _motor = value;
                SimulationPreferences.SetRobotJointMotor((_simObject as RobotSimObject)!.RobotGUID, MotorRef, _motor);
            }
        }

        private bool _useMotor = true;

        // Is this actually used?
        public bool UseMotor { get => _useMotor; }

        private float _targetRotationalSpeed = 0f;

        public readonly string MotorRef;

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
        public WheelDriver(string name, string[] inputs, string[] outputs, SimObject simObject,
            JointInstance jointInstance, CustomWheel customWheel, Vector3 anchor, Vector3 axis, float radius,
            string motorRef)
            : base(name, inputs, outputs, simObject) {
            _jointInstance = jointInstance;
            _customWheel   = customWheel;

            Anchor = _customWheel.Rb.transform.localToWorldMatrix.MultiplyPoint3x4(anchor);
            Axis   = _customWheel.Rb.transform.localToWorldMatrix.MultiplyVector(axis);

            MotorRef = motorRef;

            if (float.IsNaN(radius)) {
                Radius = _customWheel.Rb.transform.GetBounds().extents.y;
            } else {
                Radius = radius;
            }

            var motor = SimulationPreferences.GetRobotJointMotor((simObject as RobotSimObject)!.RobotGUID, motorRef);

            if (motor != null) {
                _motor = motor.Value;
            } else {
                Motor = new JointMotor() {
                    // Default Motor. Slow but powerful enough. Also uses Motor to save it
                    force          = 2000,
                    freeSpin       = false,
                    targetVelocity = 30,
                };
            }

            State.SetValue(_outputs[0], Value.ForNumber(0));
            State.SetValue(_outputs[1], Value.ForNumber(1));
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
            VelocityControl();

            _lastUpdate = Time.realtimeSinceStartup;

            // I think these work?
            State.SetValue(_outputs[0], Value.ForNumber(_jointAngle / (Mathf.PI * 2f)));
            State.SetValue(_outputs[1], Value.ForNumber(PositiveMod(_jointAngle, Mathf.PI)));
        }

        public float PositiveMod(float val, float mod) {
            var res = val % mod;
            if (res < 0)
                res += mod;
            return res;
        }

        public void WheelsPhysicsUpdate(float mod) {
            _customWheel.CalculateAndApplyFriction(mod);
        }

        private void VelocityControl() {
            if (!_useMotor)
                return;

            var val = (float) MainInput;

            _targetRotationalSpeed = val * _motor.targetVelocity;

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
    }
}
