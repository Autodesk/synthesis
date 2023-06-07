using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Mirabuf;
using Synthesis.PreferenceManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

namespace Synthesis {
	public class TankDriveBehavior : SimBehaviour {

		internal const string LEFT_FORWARD = "Tank Left-Forward";
		internal const string LEFT_BACKWARD = "Tank Left-Backward";
		internal const string RIGHT_FORWARD = "Tank Right-Forward";
		internal const string RIGHT_BACKWARD = "Tank Right-Backward";

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

		public TankDriveBehavior(string simObjectId, List<WheelDriver> leftWheels, List<WheelDriver> rightWheels, string inputName = "") : base(
			simObjectId) {
			if (inputName == "")
				inputName = simObjectId;

			SimObjectId = simObjectId;
			_leftWheels = leftWheels;
			_rightWheels = rightWheels;

			InitInputs(GetInputs());

			EventBus.NewTypeListener<ValueInputAssignedEvent>(OnValueInputAssigned);
		}

		public (string key, Analog input)[] GetInputs() {
            return new (string key, Analog input)[] {
                (LEFT_FORWARD, TryLoadInput(LEFT_FORWARD, new Digital("W"))),
                (LEFT_BACKWARD, TryLoadInput(LEFT_BACKWARD, new Digital("S"))),
				(RIGHT_FORWARD, TryLoadInput(RIGHT_FORWARD, new Digital("UpArrow"))),
				(RIGHT_BACKWARD, TryLoadInput(RIGHT_BACKWARD, new Digital("DownArrow")))
            };
        }

        public Analog TryLoadInput(string key, Analog defaultInput)
        {
	        if (_robot == null) _robot = SimulationManager.SimulationObjects[SimObjectId] as RobotSimObject;
	        return SimulationPreferences.GetRobotInput(
		               _robot.MiraLive.MiraAssembly.Info.GUID, key)
	               ?? defaultInput;
        }

        private void OnValueInputAssigned(IEvent tmp) {
			ValueInputAssignedEvent args = tmp as ValueInputAssignedEvent;
			switch (args.InputKey) {
				case LEFT_FORWARD:
				case LEFT_BACKWARD:
				case RIGHT_FORWARD:
				case RIGHT_BACKWARD:
					if (base.SimObjectId != RobotSimObject.GetCurrentlyPossessedRobot().MiraGUID) return;
					RobotSimObject robot = SimulationManager.SimulationObjects[base.SimObjectId] as RobotSimObject;
					SimulationPreferences.SetRobotInput(
						_robot.MiraLive.MiraAssembly.Info.GUID,
						args.InputKey,
						args.Input);
					break;
			}
		}

		public override void Update() {
			var leftForwardInput = InputManager.MappedValueInputs[LEFT_FORWARD];
			var leftBackwardInput = InputManager.MappedValueInputs[LEFT_BACKWARD];
			var rightForwardInput = InputManager.MappedValueInputs[RIGHT_FORWARD];
			var rightBackwardInput = InputManager.MappedValueInputs[RIGHT_BACKWARD];

			var leftSpeed = Mathf.Abs(leftForwardInput.Value) - Mathf.Abs(leftBackwardInput.Value);
			var rightSpeed = Mathf.Abs(rightForwardInput.Value) - Mathf.Abs(rightBackwardInput.Value);

			foreach (var wheel in _leftWheels) {
				wheel.MainInput = leftSpeed * speedMult;
			}
			foreach (var wheel in _rightWheels) {
				wheel.MainInput = rightSpeed * speedMult;
			}
		}
	}
}