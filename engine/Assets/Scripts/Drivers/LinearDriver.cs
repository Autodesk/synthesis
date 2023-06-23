using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.Simulation;
using Synthesis.PreferenceManager;

namespace Synthesis {
    public class LinearDriver : Driver {
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
        private float _velocity  = 0f;
        public float Velocity   => _velocity;
        public (float Upper, float Lower) Limits { get; private set; }

        public LinearDriver(string name, string[] inputs, string[] outputs, SimObject simObject,
            ConfigurableJoint jointA, ConfigurableJoint jointB, float maxSpeed, (float, float) limits)
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

            _velocity = MaxSpeed;
        }

        public override void Update() {
            // TODO: Velocity?

            float value = State.CurrentSignals.ContainsKey(_inputs[0])
                              ? (float) State.CurrentSignals[_inputs[0]].Value.NumberValue
                              : 0f;

            _velocity = value * MaxSpeed;
            Position += Time.deltaTime * _velocity;
        }
    }
}
