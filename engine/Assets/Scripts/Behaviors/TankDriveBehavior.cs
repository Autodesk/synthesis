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

		internal const string LEFT_FORWARD = "Tank Left Forward";
		internal const string LEFT_REVERSE = "Tank Left Reverse";
		internal const string RIGHT_FORWARD = "Tank Right Forward";
		internal const string RIGHT_REVERSE = "Tank Right Reverse";

		private List<string> _leftSignals;
		private List<string> _rightSignals;
		private double       _speed;

		public TankDriveBehavior(string simObjectId, List<string> leftSignals, List<string> rightSignals) : base(simObjectId) {
			SimObjectId = simObjectId;
			_leftSignals = leftSignals;
			_rightSignals = rightSignals;

			InitInputs(GetInputs());

			EventBus.NewTypeListener<ValueInputAssignedEvent>(OnValueInputAssigned);
		}

		public (string key, string displayName, Analog input)[] GetInputs() {
            return new (string key, string displayName, Analog input)[] {
                (LEFT_FORWARD, LEFT_FORWARD, TryLoadInput(LEFT_FORWARD, new Digital("W"))),
                (LEFT_REVERSE, LEFT_REVERSE, TryLoadInput(LEFT_REVERSE, new Digital("S"))),
				(RIGHT_FORWARD, RIGHT_FORWARD, TryLoadInput(RIGHT_FORWARD, new Digital("I"))),
				(RIGHT_REVERSE, RIGHT_REVERSE, TryLoadInput(RIGHT_REVERSE, new Digital("K")))
            };
        }

        public Analog TryLoadInput(string key, Analog defaultInput)
            => SimulationPreferences.GetRobotInput((SimulationManager.SimulationObjects[SimObjectId] as RobotSimObject).MiraLive.MiraAssembly.Info.GUID, key)
                ?? defaultInput;

		private void OnValueInputAssigned(IEvent tmp) {
			ValueInputAssignedEvent args = tmp as ValueInputAssignedEvent;
			switch (args.InputKey) {
				case LEFT_FORWARD:
				case LEFT_REVERSE:
				case RIGHT_FORWARD:
				case RIGHT_REVERSE:
					if (base.SimObjectId != RobotSimObject.GetCurrentlyPossessedRobot().MiraGUID) return;
					RobotSimObject robot = SimulationManager.SimulationObjects[base.SimObjectId] as RobotSimObject;
					SimulationPreferences.SetRobotInput(
						robot.MiraGUID,
						args.InputKey,
						args.Input);
					break;
			}
		}

		public override void Update() {
			var lf = InputManager.MappedValueInputs[LEFT_FORWARD];
			var lr = InputManager.MappedValueInputs[LEFT_REVERSE];
			var rf = InputManager.MappedValueInputs[RIGHT_FORWARD];
			var rr = InputManager.MappedValueInputs[RIGHT_REVERSE];

			float leftIn = 0f;
			float rightIn = 0f;

			if (lr is Digital)
				leftIn = lf.Value - lr.Value;
			else
				leftIn = lf.Value + lr.Value;
			if (rr is Digital)
				rightIn = rf.Value - rr.Value;
			else
				rightIn = rf.Value + rr.Value;

			foreach (var sig in _leftSignals) {
				SimulationManager.SimulationObjects[SimObjectId].State.CurrentSignals[sig].Value = Value.ForNumber(leftIn*_speed);
			}
			foreach (var sig in _rightSignals) {
				SimulationManager.SimulationObjects[SimObjectId].State.CurrentSignals[sig].Value = Value.ForNumber(rightIn*_speed);
			}
		}
	}
}