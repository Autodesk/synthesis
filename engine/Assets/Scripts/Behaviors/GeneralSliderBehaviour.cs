using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Synthesis.PreferenceManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

using Logger = SynthesisAPI.Utilities.Logger;

namespace Synthesis {
    public class GeneralSliderBehaviour : SimBehaviour {

        internal string FORWARD = " Forward";
        internal string REVERSE = " Reverse";

        private string _sliderSignal;

        public GeneralSliderBehaviour(string simObjectId, string sliderSignal) : base(simObjectId) {
            _sliderSignal = sliderSignal;

            InitInputs(GetInputs());

            EventBus.NewTypeListener<ValueInputAssignedEvent>(OnValueInputAssigned);
        }

        public (string key, Analog input)[] GetInputs() {
            var name = SimulationManager.SimulationObjects[SimObjectId].State.CurrentSignalLayout.SignalMap[_sliderSignal].Info.Name;
            FORWARD = name + FORWARD;
            REVERSE = name + REVERSE;

            var key = ((RobotSimObject.ControllableJointCounter + 1) % 10).ToString();
            RobotSimObject.ControllableJointCounter++;
            return new (string key, Analog input)[] {
                (FORWARD, TryLoadInput(FORWARD, new Digital("Alpha" + key))),
                (REVERSE, TryLoadInput(REVERSE, new Digital("Alpha" + key, (int)ModKey.LeftShift)))
            };
        }

        public Analog TryLoadInput(string key, Analog defaultInput)
            => SimulationPreferences.GetRobotInput((SimulationManager.SimulationObjects[SimObjectId] as RobotSimObject).MiraAssembly.Info.GUID, key)
                ?? defaultInput;

        private void OnValueInputAssigned(IEvent tmp) {
            ValueInputAssignedEvent args = tmp as ValueInputAssignedEvent;
            if (args.InputKey.Equals(FORWARD) || args.InputKey.Equals(REVERSE)) {
                if (base.SimObjectId != RobotSimObject.GetCurrentlyPossessedRobot().MiraGUID) return;
                RobotSimObject robot = SimulationManager.SimulationObjects[base.SimObjectId] as RobotSimObject;
                SimulationPreferences.SetRobotInput(
                    robot.MiraGUID,
                    args.InputKey,
                    args.Input);
            }

            PreferenceManager.PreferenceManager.Save();
        }

        public override void Update() {
            var forw = InputManager.MappedValueInputs[FORWARD];
            var rev = InputManager.MappedValueInputs[REVERSE];
            float val = Mathf.Abs(forw.Value) - Mathf.Abs(rev.Value);

            SimulationManager.SimulationObjects[SimObjectId].State.CurrentSignals[_sliderSignal].Value = Value.ForNumber(val);
        }

    }
}
