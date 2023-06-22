using Google.Protobuf.WellKnownTypes;
using Synthesis.PreferenceManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Logger = SynthesisAPI.Utilities.Logger;

namespace Synthesis {
    public class GeneralSliderBehaviour : SimBehaviour {

        private string _forwardInputKey    = "_forward";
        private string _reverseInputKey    = "_reverse";
        private string _forwardDisplayName = " Forward";
        private string _reverseDisplayName = " Reverse";

        private string _sliderSignal;

        public GeneralSliderBehaviour(string simObjectId, string sliderSignal) : base(simObjectId) {
            _sliderSignal = sliderSignal;

            var name = SimulationManager.SimulationObjects[SimObjectId]
                           .State.CurrentSignalLayout.SignalMap[_sliderSignal]
                           .Info.Name;
            _forwardDisplayName = name + _forwardDisplayName;
            _reverseDisplayName = name + _reverseDisplayName;

            InitInputs(GetInputs());

            EventBus.NewTypeListener<ValueInputAssignedEvent>(OnValueInputAssigned);
        }

        public (string key, string displayName, Analog input)[] GetInputs() {
            var key = ((RobotSimObject.ControllableJointCounter + 1) % 10).ToString();
            RobotSimObject.ControllableJointCounter++;
            return new(string key, string displayName, Analog input)[] {
                (_forwardInputKey, _forwardDisplayName, TryLoadInput(_forwardInputKey, new Digital("Alpha" + key))),
                (_reverseInputKey, _reverseDisplayName,
                    TryLoadInput(_reverseInputKey, new Digital("Alpha" + key, (int)ModKey.LeftShift)))
            };
        }

        public Analog TryLoadInput(string key, Analog defaultInput) =>
            SimulationPreferences.GetRobotInput(
                (SimulationManager.SimulationObjects[SimObjectId] as RobotSimObject).MiraLive.MiraAssembly.Info.GUID,
                key) ??
            defaultInput;

        private void OnValueInputAssigned(IEvent tmp) {
            ValueInputAssignedEvent args = tmp as ValueInputAssignedEvent;
            if (args.InputKey.Equals(_forwardInputKey) || args.InputKey.Equals(_reverseInputKey)) {
                if (base.SimObjectId != RobotSimObject.GetCurrentlyPossessedRobot().MiraGUID) {
                    return;
                }

                RobotSimObject robot = SimulationManager.SimulationObjects[base.SimObjectId] as RobotSimObject;
                SimulationPreferences.SetRobotInput(robot.MiraGUID, args.InputKey, args.Input);
            }

            PreferenceManager.PreferenceManager.Save();
        }

        public override void Update() {
            var forw  = InputManager.MappedValueInputs[_forwardInputKey];
            var rev   = InputManager.MappedValueInputs[_reverseInputKey];
            float val = Mathf.Abs(forw.Value) - Mathf.Abs(rev.Value);

            SimulationManager.SimulationObjects[SimObjectId].State.CurrentSignals[_sliderSignal].Value =
                Value.ForNumber(val);
        }
    }
}
