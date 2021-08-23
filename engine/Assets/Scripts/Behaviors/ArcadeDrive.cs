using System;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using SynthesisAPI.Simulation;
using UnityEditor;
using UnityEngine;
using Math = SynthesisAPI.Utilities.Math;

namespace Assets.Scripts.Behaviors
{
	public class ArcadeDrive : SimBehaviour
	{

		private List<string> _leftSignals;
		private List<string> _rightSignals;

		private double _leftSpeed;
		private double _rightSpeed;

		private double _xSpeed;
		private double _zRot;

		private bool _squareInputs; // TODO: Add ability to modify this

		private bool _didUpdate;

		private byte _keyMask;

		public ArcadeDrive(string simObjectId, List<string> leftSignals, List<string> rightSignals, string inputName = "") : base(
			simObjectId)
		{
			if (inputName == "")
				inputName = simObjectId;

			SimObjectId = simObjectId;
			_leftSignals = leftSignals;
			_rightSignals = rightSignals;


		}

		public override void Update()
		{
			_didUpdate = true;
			if (Input.GetKeyDown(KeyCode.W) && !Input.GetKeyDown(KeyCode.S))
			{
				if (Input.GetKeyDown(KeyCode.A))
				{
					_xSpeed = System.Math.Sqrt(2)/2;
					_zRot = -1*System.Math.Sqrt(2)/2;
				} else if (Input.GetKeyDown(KeyCode.D))
				{
					_xSpeed = System.Math.Sqrt(2)/2;
					_zRot = System.Math.Sqrt(2)/2;
				}
				else
				{
					_xSpeed = 1;
					_zRot = 0;
				}
			} else if (Input.GetKeyDown(KeyCode.S))
			{
				if (Input.GetKeyDown(KeyCode.A))
				{
					_xSpeed = -1*System.Math.Sqrt(2)/2;
					_zRot = -1*System.Math.Sqrt(2)/2;
				} else if (Input.GetKeyDown(KeyCode.D))
				{
					_xSpeed = -1*System.Math.Sqrt(2)/2;
					_zRot = System.Math.Sqrt(2)/2;
				}
				else
				{
					_xSpeed = -1;
					_zRot = 0;
				}
			} else if (Input.GetKeyDown(KeyCode.A) && !Input.GetKeyDown(KeyCode.D))
			{
				_zRot = -1;
			}  else if (Input.GetKeyDown(KeyCode.D))
			{
				_zRot = 1;
			}
			else
			{
				//_xSpeed = 0;
				//_zRot = 0;
			}

			if (_didUpdate)
			{
				_didUpdate = false;
				_keyMask = 0b0000000;
				(_leftSpeed, _rightSpeed) = SolveSpeed(_xSpeed, _zRot, _squareInputs);
				foreach (var sig in _leftSignals)
				{
					SimulationManager.SimulationObjects[SimObjectId].State.CurrentSignals[sig].Value = Value.ForNumber(_leftSpeed);
				}
				foreach (var sig in _rightSignals)
            	{
            		SimulationManager.SimulationObjects[SimObjectId].State.CurrentSignals[sig].Value = Value.ForNumber(-_rightSpeed);
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