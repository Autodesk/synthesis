using System;
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
            HingeJoint jointA, HingeJoint jointB, Mirabuf.Motor.Motor? motor = null)
            : base(name, inputs, outputs, simObject) {
            _jointA = jointA;
            _jointB = jointB;
            if (motor != null && motor.MotorTypeCase == Mirabuf.Motor.Motor.MotorTypeOneofCase.SimpleMotor) {
                _motor = motor!.SimpleMotor.UnityMotor;
            } else {
                // var m = SimulationPreferences.GetRobotJointMotor((_simObject as RobotSimObject).MiraGUID, name);
                Motor = new JointMotor() { // Default Motor. Slow but powerful enough. Also uses Motor to save it
                    force = 2000,
                    freeSpin = false,
                    targetVelocity = 500,
                };
            }

            // Debug.Log($"Speed: {_motor.targetVelocity}\nForce: {_motor.force}");
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

                var val = (float)(State.CurrentSignals.ContainsKey(_inputs[0])
                    ? State.CurrentSignals[_inputs[0]].Value.NumberValue
                    : 0.0f);

                var inertiaA = GetInertiaAroundParallelAxis(_jointA.connectedBody, _jointB.anchor, _jointB.axis);
                // var angAccelA = (Motor.force * val) / inertiaA;
                // _jointA.connectedBody.AddTorque(_jointB.axis.normalized * angAccelA * Mathf.Rad2Deg, ForceMode.Acceleration);
                // Debug.Log($"{angAccelA} to {_jointA.connectedBody.name}");

                var inertiaB = GetInertiaAroundParallelAxis(_jointB.connectedBody, _jointA.anchor, _jointA.axis);
                // var angAccelB = (-Motor.force * val) / inertiaB;
                // _jointB.connectedBody.AddTorque(_jointA.axis.normalized * angAccelB * Mathf.Rad2Deg, ForceMode.Acceleration);
                // Debug.Log($"{angAccelB} to {_jointB.connectedBody.name}");

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

            // var updateSignal = new UpdateSignals();
            // var key = _outputs[0];
            // var current = _state.CurrentSignals[key];
            // updateSignal.SignalMap.Add(key, new UpdateSignal() {
            //     Class = current.Class, Io = current.Io,
            //     Value = new Value { NumberValue = _jointA.angle }
            // });
            // _state.Update(updateSignal);
        }

        public float GetInertiaAroundParallelAxis(Rigidbody rb, Vector3 localAnchor, Vector3 localAxis) {
            var comInertia = GetInertiaFromAxisVector(rb, localAxis);
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
            var sph = CartesianToSphericalCoordinate(localAxis);
            var inertia = rb.inertiaTensorRotation * rb.inertiaTensor;
            return EllipsoidRadiusFromSphericalCoordinate(sph, inertia);
        }

        public static float EllipsoidRadiusFromSphericalCoordinate(Vector3 sph, Vector3 ellipsoidRadi) {
            var cartEllip = new Vector3(
                ellipsoidRadi.x * Mathf.Sin(sph.y) * Mathf.Cos(sph.z),
                ellipsoidRadi.y * Mathf.Cos(sph.y),
                ellipsoidRadi.z * Mathf.Sin(sph.y) * Mathf.Sin(sph.z)
            );
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
            return new Vector3(
                cart.magnitude,
                Mathf.Acos(cart.y / 1),
                Mathf.Asin(cart.z / 1)
            );
        }
    }
}