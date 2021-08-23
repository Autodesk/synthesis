using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Mirabuf;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

namespace Assets.Scripts.Behaviors
{
	public class TankDriveBehavior : SimBehaviour
	{

		private List<string> _leftSignals;
		private List<string> _rightSignals;
		private bool         _didUpdate;
		private double       _leftSpeed;
		private double       _rightSpeed;
		private double       _inL;
		private double       _inR;
		private object       _squareInputs;

		public TankDriveBehavior(string simObjectId, List<string> leftSignals, List<string> rightSignals) : base(simObjectId)
		{
			SimObjectId = simObjectId;
			_leftSignals = leftSignals;
			_rightSignals = rightSignals;
		}

		public override void Update()
		{

			if (_didUpdate)
			{
				_didUpdate = false;
				(_leftSpeed, _rightSpeed) = SolveSpeed(_inL, _inR, false);
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

		protected static (double, double) SolveSpeed(double lSpeed, double rSpeed, bool squareInputs)
		{
			return (0, 0);
		}
	}
}