using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Synthesis.PreferenceManager;
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

            var name = SimulationManager.SimulationObjects[simObjectId].State.CurrentSignalLayout.SignalMap[_sliderSignal].Info.Name;
            FORWARD = name + FORWARD;
            REVERSE = name + REVERSE;

            if (RobotSimObject.ControllableJointCounter > 9)
                Logger.Log("Too Many Slider Joints. Need to come up with a better plan to generate keys", LogLevel.Debug);

            var key = ((RobotSimObject.ControllableJointCounter + 1) % 10).ToString();
            InputManager.AssignValueInput(FORWARD, SimulationPreferences.GetRobotInput(
            (SimulationManager.SimulationObjects[simObjectId] as RobotSimObject).MiraAssembly.Info.GUID, FORWARD) ?? new Digital("Alpha" + key));
            InputManager.AssignValueInput(REVERSE, SimulationPreferences.GetRobotInput(
            (SimulationManager.SimulationObjects[simObjectId] as RobotSimObject).MiraAssembly.Info.GUID, REVERSE) ?? new Digital("Alpha" + key, (int)ModKey.LeftShift));
            RobotSimObject.ControllableJointCounter++;
        }

        public override void Update() {
            var forw = InputManager.MappedValueInputs[FORWARD];
            var rev = InputManager.MappedValueInputs[REVERSE];
            float val = Mathf.Abs(forw.Value) - Mathf.Abs(rev.Value);

            SimulationManager.SimulationObjects[SimObjectId].State.CurrentSignals[_sliderSignal].Value = Value.ForNumber(val);
        }

    }
}
