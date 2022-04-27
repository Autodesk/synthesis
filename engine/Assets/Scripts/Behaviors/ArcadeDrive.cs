using System;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Simulation;
using UnityEditor;
using UnityEngine;
using Input = UnityEngine.Input;
using Logger = SynthesisAPI.Utilities.Logger;
using Math = SynthesisAPI.Utilities.Math;

namespace Synthesis
{
	public class ArcadeDrive : SimBehaviour
	{

		internal const string FORWARD = "Arcade 1 Forward";
		internal const string BACKWARD = "Arcade 1 Backward";
		internal const string LEFT = "Arcade 1 Left";
		internal const string RIGHT = "Arcade 1 Right";

		private List<string> _leftSignals;
		private List<string> _rightSignals;

		private double _leftSpeed;
		private double _rightSpeed;

		private double _xSpeed;
		private double _zRot;

		private bool _squareInputs; // TODO: Add ability to modify this

		private bool _didUpdate;

		private byte _keyMask;

		private const double DEADBAND = 0.1;

		public double speedMult = 1.0f;

		private bool _reversedSideJoints;

		public ArcadeDrive(string simObjectId, List<string> leftSignals, List<string> rightSignals, string inputName = "", bool reversedSideJoints = false) : base(
			simObjectId)
		{
			if (inputName == "")
				inputName = simObjectId;

			SimObjectId = simObjectId;
			_leftSignals = leftSignals;
			_rightSignals = rightSignals;

			_reversedSideJoints = reversedSideJoints;

			InputManager.AssignValueInput(FORWARD, new Digital("W"));
			InputManager.AssignValueInput(BACKWARD, new Digital("S"));
			InputManager.AssignValueInput(LEFT, new Digital("A"));
			InputManager.AssignValueInput(RIGHT, new Digital("D"));


		}

		public override void Update()
		{
			_didUpdate = true;

			var forwardInput = InputManager.MappedValueInputs[FORWARD];
			var backwardInput = InputManager.MappedValueInputs[BACKWARD];
			var leftInput = InputManager.MappedValueInputs[LEFT];
			var rightInput = InputManager.MappedValueInputs[RIGHT];

			if (backwardInput is Digital)
				_xSpeed = forwardInput.Value - backwardInput.Value;
			else
				_xSpeed = forwardInput.Value + backwardInput.Value;
			if (leftInput is Digital)
				_zRot = rightInput.Value - leftInput.Value;
			else
				_zRot = rightInput.Value + leftInput.Value;

			// Deadbanding
			_xSpeed = Math.Abs(_xSpeed) > DEADBAND ? _xSpeed : 0;
			_zRot = Math.Abs(_zRot) > DEADBAND ? _zRot : 0;

			if (_didUpdate)
			{
				_didUpdate = false;
				_keyMask = 0b0000000;
				(_leftSpeed, _rightSpeed) = SolveSpeed(_xSpeed, _zRot, _squareInputs);
				foreach (var sig in _leftSignals)
				{
					SimulationManager.SimulationObjects[SimObjectId].State.CurrentSignals[sig].Value = Value.ForNumber(_leftSpeed*speedMult);
				}
				foreach (var sig in _rightSignals)
            	{
            		SimulationManager.SimulationObjects[SimObjectId].State.CurrentSignals[sig].Value = Value.ForNumber(_rightSpeed*speedMult * (!_reversedSideJoints ? -1 : 1));
            	}
			}

		}

		// Implementation derived from https://github.com/wpilibsuite/allwpilib/blob/362066a9b77f38a2862e306b8119e753b199d4ae/wpilibc/src/main/native/cpp/drive/DifferentialDrive.cpp
		protected static (double lSpeed, double rSpeed) SolveSpeed(double xSpeed, double zRot, bool squareInputs)
		{
			if (xSpeed == 0 && zRot == 0)
			{
				return (0, 0);
			}
			xSpeed = Math.Clamp(xSpeed, -1, 1);
			zRot = Math.Clamp(zRot, -1, 1);

			if (squareInputs)
			{
				xSpeed = xSpeed * xSpeed * (xSpeed < 0 ? -1 : 1);
				zRot = zRot * zRot * (zRot < 0 ? -1 : 1);
			}

			double lSpeed, rSpeed;

			double maxInput = Math.Max(Math.Abs(xSpeed), Math.Abs(zRot)) * (xSpeed < 0 ? -1 : 1);

			if (xSpeed >= 0)
			{
				if (zRot >= 0)
				{
					lSpeed = maxInput;
					rSpeed = xSpeed - zRot;
				}
				else
				{
					lSpeed = xSpeed + zRot;
					rSpeed = maxInput;
				}
			}
			else
			{
				if (zRot >= 0)
				{
					lSpeed = xSpeed + zRot;
					rSpeed = maxInput;
				}
				else
				{
					lSpeed = maxInput;
					rSpeed = xSpeed - zRot;
				}
			}

			// Normalize speeds
			double maxMag = Math.Max(Math.Abs(lSpeed), Math.Abs(rSpeed));
			if (maxMag >= 0)
			{
				lSpeed /= maxMag;
				rSpeed /= maxMag;
			}

			return (lSpeed, rSpeed);

		}

	}
}