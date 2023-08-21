using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.Simulation;
using Synthesis.PreferenceManager;
using Google.Protobuf.WellKnownTypes;
using Synthesis.Physics;

namespace Synthesis {
    public class LinearDriver : Driver {
        public const float LINEAR_TO_MOTOR_VELOCITY = 100f;

        public string Signal => _inputs[0];

        public ConfigurableJoint JointA { get; private set; }
        public ConfigurableJoint JointB { get; private set; }
        private float _maxSpeed;
        private float _lastSpeed = 0;
        public float MaxSpeed {
            get => _maxSpeed;
            set {
                _maxSpeed = value;
                SimulationPreferences.SetRobotJointSpeed((_simObject as RobotSimObject).MiraGUID, Name, _maxSpeed);
            }
        }
        public float _position = 0f;
        public float Position {
            get => _position;
        private
            set {
                var newPos             = Mathf.Clamp(value, Limits.Lower, Limits.Upper);
                JointA.connectedAnchor = JointA.anchor + (JointA.axis * newPos);
                _position              = newPos;
            }
        }
        // clang-format off
        private float _targetVelocity = 0f;
        // clang-format on
        public float Velocity => _targetVelocity;
        // Note: only used to save between sessions
        private JointMotor _motor;
        public JointMotor Motor {
            get => _motor;
            set {
                _motor = value;
                SimulationPreferences.SetRobotJointMotor((_simObject as RobotSimObject)!.MiraGUID, Name, _motor);
            }
        }
        public (float Upper, float Lower) Limits { get; private set; }

        public double MainInput {
            get {
                if (PhysicsManager.IsFrozen)
                    return 0f;
                var val = State.GetValue(_inputs[0]);
                return val == null ? 0.0 : val.NumberValue;
            }
            set => State.SetValue(_inputs[0], Value.ForNumber(value));
        }

        public new string Name => State.SignalMap[_inputs[0]].Name;

        public readonly string MotorRef;

        public LinearDriver(string name, string[] inputs, string[] outputs, SimObject simObject,
            ConfigurableJoint jointA, ConfigurableJoint jointB, float maxSpeed, (float, float) limits,
            string motorRef = "")
            : base(name, inputs, outputs, simObject) {
            // Takeover joint configuration and make it more suited to control rather than passive
            var l              = jointA.linearLimit;
            l.limit            = 0f;
            jointA.linearLimit = l;

            JointA   = jointA;
            JointB   = jointB;
            MaxSpeed = maxSpeed;
            Position = 0f;
            Limits   = limits;
            MotorRef = motorRef;

            (simObject as RobotSimObject)!.MiraLive.MiraAssembly.Data.Joints.MotorDefinitions.TryGetValue(
                motorRef, out var motor);

            if (motor != null && motor.MotorTypeCase == Mirabuf.Motor.Motor.MotorTypeOneofCase.SimpleMotor) {
                _motor = motor!.SimpleMotor.UnityMotor;
            } else {
                Motor = new JointMotor() {
                    force          = 1, // About a Neo 550. Max is Falcon 550 at 4.67
                    freeSpin       = false,
                    targetVelocity = 20,
                };
            }

            _targetVelocity = MaxSpeed;
        }

        public override void Update() {
            // TODO: Velocity?

            float value = (float) MainInput;

            _targetVelocity = value * MaxSpeed;

            var delta         = _targetVelocity - _lastSpeed;
            var possibleDelta = _motor.force * Time.deltaTime / JointB.connectedBody.mass * 100;

            if (delta > 0.001)
                Debug.Log($"tarVel {_targetVelocity} last {_lastSpeed} delta {delta} pos {possibleDelta}");
            
            if (Mathf.Abs(delta) > possibleDelta)
                delta = possibleDelta * Mathf.Sign(delta);
            _lastSpeed += Time.deltaTime * delta;
            if (Mathf.Abs(_lastSpeed) > MaxSpeed)
                _lastSpeed = MaxSpeed * Mathf.Sign(_lastSpeed);
            Position += _lastSpeed;
        }
    }
}
