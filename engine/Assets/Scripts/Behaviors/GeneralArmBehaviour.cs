using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Synthesis.PreferenceManager;
using Synthesis.UI.Dynamic;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

using Logger = SynthesisAPI.Utilities.Logger;

#nullable enable

namespace Synthesis {
    public class GeneralArmBehaviour : SimBehaviour {
        private string _forwardInputKey    = "_forward";
        private string _reverseInputKey    = "_reverse";
        private string _forwardDisplayName = " Forward";
        private string _reverseDisplayName = " Reverse";

        private RotationalDriver _armDriver;
        private float _speedMod = 0.4f;

        private RobotSimObject _robot;

        public GeneralArmBehaviour(string simObjectId, RotationalDriver armDriver)
            : base(simObjectId) { // base(simObjectId, GetInputs(this, armSignal))

            if (armDriver.IsReserved)
                throw new Exception("Rotational Driver already reserved");
            armDriver.Reserve(this);
            _armDriver = armDriver;

            _robot = (SimulationManager.SimulationObjects[SimObjectId] as RobotSimObject)!;

            _forwardInputKey    = _robot.RobotGUID + _armDriver.Signal + _forwardInputKey;
            _reverseInputKey    = _robot.RobotGUID + _armDriver.Signal + _reverseInputKey;
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
        
        public Analog TryLoadInput(string key, Analog defaultInput) {
            Analog input;
            if (InputManager.MappedValueInputs.ContainsKey(key)) {
                input                = InputManager.GetAnalog(key);
                input.ContextBitmask = defaultInput.ContextBitmask;
                return input;
            }
            input = SimulationPreferences.GetRobotInput(_robot.RobotGUID, key);
            if (input == null) {
                SimulationPreferences.SetRobotInput(_robot.RobotGUID, key, defaultInput);
                return defaultInput;
            }
            return input;
        }

        private void OnValueInputAssigned(IEvent tmp) {
            ValueInputAssignedEvent args = tmp as ValueInputAssignedEvent;
            if (args.InputKey.Equals(_forwardInputKey) || args.InputKey.Equals(_reverseInputKey)) {

                if (_robot.RobotGUID != (MainHUD.SelectedRobot?.RobotGUID ?? string.Empty) ||
                    !((DynamicUIManager.ActiveModal as ChangeInputsModal)?.isSave ?? false))
                    return;
                SimulationPreferences.SetRobotInput(_robot.RobotGUID, args.InputKey, args.Input);
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
