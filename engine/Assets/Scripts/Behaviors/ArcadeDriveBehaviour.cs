using System;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Synthesis.PreferenceManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Simulation;
using UnityEditor;
using UnityEngine;
using Input = UnityEngine.Input;
using Logger = SynthesisAPI.Utilities.Logger;
using Math = SynthesisAPI.Utilities.Math;

namespace Synthesis {
	public class ArcadeDriveBehaviour : SimBehaviour
	{

		internal const string FORWARD = "Arcade Forward";
		internal const string BACKWARD = "Arcade Backward";
		internal const string LEFT = "Arcade Left";
		internal const string RIGHT = "Arcade Right";

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

		public ArcadeDriveBehaviour(string simObjectId, List<WheelDriver> leftWheels, List<WheelDriver> rightWheels, string inputName = "") : base(
			simObjectId) {
			if (inputName == "")
				inputName = simObjectId;

			SimObjectId = simObjectId;
			_leftWheels = leftWheels;
			_rightWheels = rightWheels;

			InitInputs(GetInputs());

			EventBus.NewTypeListener<ValueInputAssignedEvent>(OnValueInputAssigned);
		}

		public (string key, string displayName, Analog input)[] GetInputs() {
            return new (string key, string displayName, Analog input)[] {
                (FORWARD, FORWARD, TryLoadInput(FORWARD, new Digital("W"))),
                (BACKWARD, BACKWARD, TryLoadInput(BACKWARD, new Digital("S"))),
				(LEFT, LEFT, TryLoadInput(LEFT, new Digital("A"))),
				(RIGHT, RIGHT, TryLoadInput(RIGHT, new Digital("D")))
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
				case FORWARD:
				case BACKWARD:
				case LEFT:
				case RIGHT:
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
			var forwardInput = InputManager.MappedValueInputs[FORWARD];
			var backwardInput = InputManager.MappedValueInputs[BACKWARD];
			var leftInput = InputManager.MappedValueInputs[LEFT];
			var rightInput = InputManager.MappedValueInputs[RIGHT];

			_xSpeed = Mathf.Abs(forwardInput.Value) - Mathf.Abs(backwardInput.Value);
			_zRot = Mathf.Abs(rightInput.Value) - Mathf.Abs(leftInput.Value);

			// Deadbanding
			_xSpeed = Math.Abs(_xSpeed) > DEADBAND ? _xSpeed : 0;
			_zRot = Math.Abs(_zRot) > DEADBAND ? _zRot : 0;

			(_leftSpeed, _rightSpeed) = SolveSpeed(_xSpeed, _zRot, _squareInputs);
			foreach (var wheel in _leftWheels) {
				wheel.MainInput = _leftSpeed * speedMult;
			}
			foreach (var wheel in _rightWheels) {
				wheel.MainInput = _rightSpeed * speedMult;
			}
		}

		// Implementation derived from https://github.com/wpilibsuite/allwpilib/blob/362066a9b77f38a2862e306b8119e753b199d4ae/wpilibc/src/main/native/cpp/drive/DifferentialDrive.cpp
		protected static (float lSpeed, float rSpeed) SolveSpeed(float xSpeed, float zRot, bool squareInputs) {
			if (xSpeed == 0 && zRot == 0) {
				return (0, 0);
			}
			xSpeed = Math.Clamp(xSpeed, -1, 1);
			zRot = Math.Clamp(zRot, -1, 1);

			if (squareInputs) {
				xSpeed = xSpeed * xSpeed * (xSpeed < 0 ? -1 : 1);
				zRot = zRot * zRot * (zRot < 0 ? -1 : 1);
			}

			float lSpeed = Mathf.Clamp(xSpeed + zRot, -1f, 1f), rSpeed = Mathf.Clamp(xSpeed - zRot, -1f, 1f);

			return (lSpeed, rSpeed);

		}

	}
}