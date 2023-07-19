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
        private float _velocity = 0f;
        // clang-format on
        public float Velocity => _velocity;
        //Note: only used to save between sessions
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
                if (PhysicsManager.IsFrozen) return 0f;
                var val = State.GetValue(_inputs[0]);
                return val == null ? 0.0 : val.NumberValue;
            }
            set => State.SetValue(_inputs[0], Value.ForNumber(value));
        }

        public new string Name => State.SignalMap[_inputs[0]].Name;
        
        public readonly string MotorRef;

        public LinearDriver(string name, string[] inputs, string[] outputs, SimObject simObject,
            ConfigurableJoint jointA, ConfigurableJoint jointB, float maxSpeed, (float, float) limits, string motorRef = "")
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
                    force          = 2000,
                    freeSpin       = false,
                    targetVelocity = 100,
                };
            }

            _velocity = MaxSpeed;
        }

        public override void Update() {
            // TODO: Velocity?

            float value = (float) MainInput;

            _velocity = value * MaxSpeed;
            Position += Time.deltaTime * _velocity;
        }
    }
}
