using System;
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
    public class GeneralArmBehaviour : SimBehaviour {
        private string _forwardInputKey    = "_forward";
        private string _reverseInputKey    = "_reverse";
        private string _forwardDisplayName = " Forward";
        private string _reverseDisplayName = " Reverse";

        private RotationalDriver _armDriver;
        private float _speedMod = 0.4f;

        public GeneralArmBehaviour(string simObjectId, RotationalDriver armDriver)
            : base(simObjectId) { // base(simObjectId, GetInputs(this, armSignal))

            if (armDriver.IsReserved)
                throw new Exception("Rotational Driver already reserved");
            armDriver.Reserve(this);
            _armDriver = armDriver;

            _forwardInputKey    = _armDriver.Signal + _forwardInputKey;
            _reverseInputKey    = _armDriver.Signal + _reverseInputKey;
            var name            = _armDriver.Name;
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
                    TryLoadInput(_reverseInputKey, new Digital("Alpha" + key, (int) ModKey.LeftShift)))
            };
        }

        public Analog TryLoadInput(string key, Analog defaultInput) =>
            SimulationPreferences.GetRobotInput(
                (SimulationManager.SimulationObjects[SimObjectId] as RobotSimObject).MiraLiveFiles[0].MiraAssembly.Info.GUID,
                key) ??
            defaultInput;

        private void OnValueInputAssigned(IEvent tmp) {
            ValueInputAssignedEvent args = tmp as ValueInputAssignedEvent;
            if (args.InputKey.Equals(_forwardInputKey) || args.InputKey.Equals(_reverseInputKey)) {
                if (base.SimObjectId != RobotSimObject.GetCurrentlyPossessedRobot().MiraGUID)
                    return;
                RobotSimObject robot = SimulationManager.SimulationObjects[base.SimObjectId] as RobotSimObject;
                SimulationPreferences.SetRobotInput(robot.MiraGUID, args.InputKey, args.Input);
            }
        }

        public override void Update() {
            var forw  = InputManager.MappedValueInputs[_forwardInputKey];
            var rev   = InputManager.MappedValueInputs[_reverseInputKey];
            float val = Mathf.Abs(forw.Value) - Mathf.Abs(rev.Value);

            _armDriver.MainInput = val * _speedMod;
        }

        public override void OnRemove() {
            _armDriver.MainInput = 0f;
            _armDriver.Unreserve();
        }

        protected override void OnDisable() {
            _armDriver.MainInput = 0f;
        }
    }
}
