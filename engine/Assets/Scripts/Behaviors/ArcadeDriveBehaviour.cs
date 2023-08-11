using System;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Synthesis.PreferenceManager;
using Synthesis.UI.Dynamic;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Simulation;
using UnityEditor;
using UnityEngine;
using Input  = UnityEngine.Input;
using Logger = SynthesisAPI.Utilities.Logger;
using Math   = SynthesisAPI.Utilities.Math;

namespace Synthesis {
    public class ArcadeDriveBehaviour : SimBehaviour {
        internal const string FORWARD    = "Arcade Forward";
        internal const string BACKWARD   = "Arcade Backward";
        internal const string LEFT       = "Arcade Left";
        internal const string RIGHT      = "Arcade Right";
        private readonly string forward  = FORWARD;
        private readonly string backward = BACKWARD;
        private readonly string left     = LEFT;
        private readonly string right    = RIGHT;

        private readonly List<WheelDriver> _leftWheels;
        private readonly List<WheelDriver> _rightWheels;

        private double _leftSpeed;
        private double _rightSpeed;

        private float _xSpeed;
        private float _zRot;

        private bool _squareInputs; // TODO: Add ability to modify this

        private bool _didUpdate;

        private byte _keyMask;

        private const double DEADBAND = 0.1;

        public double speedMult = 1.0f;

        public ArcadeDriveBehaviour(
            string simObjectId, List<WheelDriver> leftWheels, List<WheelDriver> rightWheels, string inputName = "")
            : base(simObjectId) {
            if (inputName == "")
                inputName = simObjectId;

            _leftWheels  = leftWheels;
            _rightWheels = rightWheels;

            forward  = MiraId + FORWARD;
            backward = MiraId + BACKWARD;
            left     = MiraId + LEFT;
            right    = MiraId + RIGHT;

            InitInputs(GetInputs());

            EventBus.NewTypeListener<ValueInputAssignedEvent>(OnValueInputAssigned);
        }

        public (string key, string displayName, Analog input)[] GetInputs() {
            return new(string key, string displayName,
                Analog input)[] { (forward, FORWARD, TryLoadInput(forward, new Digital("W"))),
                (backward, BACKWARD, TryLoadInput(backward, new Digital("S"))),
                (left, LEFT, TryLoadInput(left, new Digital("A"))),
                (right, RIGHT, TryLoadInput(right, new Digital("D"))) };
        }

        public Analog TryLoadInput(string key, Analog defaultInput) {
            Analog input = SimulationPreferences.GetRobotInput(MiraId, key);
            if (input == null) {
                SimulationPreferences.SetRobotInput(MiraId, key, defaultInput);
                return defaultInput;
            }
            return input;
        }

        private void OnValueInputAssigned(IEvent tmp) {
            ValueInputAssignedEvent args = tmp as ValueInputAssignedEvent;
            if (args.InputKey.Length > MiraId.Length) {
                string s = args.InputKey.Remove(0, MiraId.Length);
                switch (s) {
                    case FORWARD:
                    case BACKWARD:
                    case LEFT:
                    case RIGHT:
                        if (base.MiraId != MainHUD.SelectedRobot.MiraGUID ||
                            !(DynamicUIManager.ActiveModal as ChangeInputsModal).isSave)
                            return;
                        SimulationPreferences.SetRobotInput(MiraId, args.InputKey, args.Input);
                        break;
                }
            }
        }

        public override void Update() {
            var forwardInput  = InputManager.MappedValueInputs[forward];
            var backwardInput = InputManager.MappedValueInputs[backward];
            var leftInput     = InputManager.MappedValueInputs[left];
            var rightInput    = InputManager.MappedValueInputs[right];

            _xSpeed = Mathf.Abs(forwardInput.Value) - Mathf.Abs(backwardInput.Value);
            _zRot   = Mathf.Abs(rightInput.Value) - Mathf.Abs(leftInput.Value);

            // Deadbanding
            _xSpeed = Math.Abs(_xSpeed) > DEADBAND ? _xSpeed : 0;
            _zRot   = Math.Abs(_zRot) > DEADBAND ? _zRot : 0;

            (_leftSpeed, _rightSpeed) = SolveSpeed(_xSpeed, _zRot, _squareInputs);
            foreach (var wheel in _leftWheels) {
                wheel.MainInput = _leftSpeed * speedMult;
            }
            foreach (var wheel in _rightWheels) {
                wheel.MainInput = _rightSpeed * speedMult;
            }
        }

        protected override void OnDisable() {
            _leftWheels.ForEach(w => w.MainInput = 0);
            _rightWheels.ForEach(w => w.MainInput = 0);
        }

        // Implementation derived from
        // https://github.com/wpilibsuite/allwpilib/blob/362066a9b77f38a2862e306b8119e753b199d4ae/wpilibc/src/main/native/cpp/drive/DifferentialDrive.cpp
        protected static (float lSpeed, float rSpeed) SolveSpeed(float xSpeed, float zRot, bool squareInputs) {
            if (xSpeed == 0 && zRot == 0) {
                return (0, 0);
            }
            xSpeed = Math.Clamp(xSpeed, -1, 1);
            zRot   = Math.Clamp(zRot, -1, 1);

            if (squareInputs) {
                xSpeed = xSpeed * xSpeed * (xSpeed < 0 ? -1 : 1);
                zRot   = zRot * zRot * (zRot < 0 ? -1 : 1);
            }

            float lSpeed = Mathf.Clamp(xSpeed + zRot, -1f, 1f), rSpeed = Mathf.Clamp(xSpeed - zRot, -1f, 1f);

            return (lSpeed, rSpeed);
        }
    }
}