using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Mirabuf;
using Synthesis.PreferenceManager;
using Synthesis.UI.Dynamic;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

namespace Synthesis {
    public class TankDriveBehavior : SimBehaviour {
        private string left_forward  = "Tank Left-Forward";
        private string left_reverse  = "Tank Left-Reverse";
        private string right_forward = "Tank Right-Forward";
        private string right_reverse = "Tank Right-Reverse";

        internal const string LEFT_FORWARD  = "Tank Left-Forward";
        internal const string LEFT_REVERSE  = "Tank Left-Reverse";
        internal const string RIGHT_FORWARD = "Tank Right-Forward";
        internal const string RIGHT_REVERSE = "Tank Right-Reverse";

        private List<WheelDriver> _leftWheels;
        private List<WheelDriver> _rightWheels;

        private double _leftSpeed;
        private double _rightSpeed;

        private float _xSpeed;
        private float _zRot;

        private bool _squareInputs; // TODO: Add ability to modify this

        private bool _didUpdate;

        private byte _keyMask;

        private const double DEADBAND = 0.1;

        public double speedMult = 1.0f;

        private RobotSimObject _robot;

        public TankDriveBehavior(
            string simObjectId, List<WheelDriver> leftWheels, List<WheelDriver> rightWheels, string inputName = "")
            : base(simObjectId) {
            if (inputName == "")
                inputName = simObjectId;

            SimObjectId  = simObjectId;
            _leftWheels  = leftWheels;
            _rightWheels = rightWheels;

            InitInputs(GetInputs());

            EventBus.NewTypeListener<ValueInputAssignedEvent>(OnValueInputAssigned);
        }

        public (string key, string displayName, Analog input)[] GetInputs() {
            return new(string key, string displayName,
                Analog input)[] { (left_forward, LEFT_FORWARD, TryLoadInput(left_forward, new Digital("W"))),
                (left_reverse, LEFT_REVERSE, TryLoadInput(left_reverse, new Digital("S"))),
                (right_forward, RIGHT_FORWARD, TryLoadInput(right_forward, new Digital("I"))),
                (right_reverse, RIGHT_REVERSE, TryLoadInput(right_reverse, new Digital("K"))) };
        }

        public Analog TryLoadInput(string key, Analog defaultInput) {
            if (_robot == null)
                _robot = SimulationManager.SimulationObjects[SimObjectId] as RobotSimObject;
            return SimulationPreferences.GetRobotInput(_robot.MiraLive.MiraAssembly.Info.GUID, key) ?? defaultInput;
        }

        private void OnValueInputAssigned(IEvent tmp) {
            ValueInputAssignedEvent args = tmp as ValueInputAssignedEvent;
            switch (args.InputKey) {
                case LEFT_FORWARD:
                case LEFT_REVERSE:
                case RIGHT_FORWARD:
                case RIGHT_REVERSE:
                    if (base.MiraId != MainHUD.SelectedRobot.MiraGUID ||
                        !(DynamicUIManager.ActiveModal as ChangeInputsModal).isSave)
                        return;
                    RobotSimObject robot = SimulationManager.SimulationObjects[base.SimObjectId] as RobotSimObject;
                    SimulationPreferences.SetRobotInput(
                        _robot.MiraLive.MiraAssembly.Info.GUID, args.InputKey, args.Input);
                    break;
            }
        }

        public override void Update() {
            var leftForwardInput   = InputManager.MappedValueInputs[left_forward];
            var leftBackwardInput  = InputManager.MappedValueInputs[left_reverse];
            var rightForwardInput  = InputManager.MappedValueInputs[right_forward];
            var rightBackwardInput = InputManager.MappedValueInputs[right_reverse];

            var leftSpeed  = Mathf.Abs(leftForwardInput.Value) - Mathf.Abs(leftBackwardInput.Value);
            var rightSpeed = Mathf.Abs(rightForwardInput.Value) - Mathf.Abs(rightBackwardInput.Value);

            foreach (var wheel in _leftWheels) {
                wheel.MainInput = leftSpeed * speedMult;
            }
            foreach (var wheel in _rightWheels) {
                wheel.MainInput = rightSpeed * speedMult;
            }
        }

        protected override void OnDisable() {
            _leftWheels.ForEach(w => w.MainInput = 0);
            _rightWheels.ForEach(w => w.MainInput = 0);
        }
    }
}