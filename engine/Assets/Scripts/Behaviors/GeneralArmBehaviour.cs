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

        internal string FORWARD = " Forward"; // TODO
        internal string REVERSE = " Reverse"; // TODO

        private RotationalDriver _armDriver;
        private float _speedMod = 0.4f;

        public GeneralArmBehaviour(string simObjectId, RotationalDriver armDriver) : base(simObjectId) {// base(simObjectId, GetInputs(this, armSignal))

            if (armDriver.IsReserved)
                throw new Exception("Rotational Driver already reserved");
            armDriver.Reserve(this);
            _armDriver = armDriver;

            // var name = SimulationManager.SimulationObjects[simObjectId].State.CurrentSignalLayout.SignalMap[armSignal].Info.Name;
            // FORWARD = name + FORWARD;
            // REVERSE = name + REVERSE;

            // if (RobotSimObject.ControllableJointCounter > 9)
            //     Logger.Log("Too Many Arm Joints. Need to come up with a better plan to generate keys", LogLevel.Debug);

            // var key = ((RobotSimObject.ControllableJointCounter + 1) % 10).ToString();
            // SetupInput(FORWARD, new Digital("Alpha" + key));
            // SetupInput(REVERSE, new Digital("Alpha" + key, (int)ModKey.LeftShift));
            // RobotSimObject.ControllableJointCounter++;

            InitInputs(GetInputs());

            EventBus.NewTypeListener<ValueInputAssignedEvent>(OnValueInputAssigned);
        }

        public (string key, Analog input)[] GetInputs() {
            var name = _armDriver.Name;
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
            => SimulationPreferences.GetRobotInput((SimulationManager.SimulationObjects[SimObjectId] as RobotSimObject).MiraLive.MiraAssembly.Info.GUID, key)
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
        }

        public override void Update() {

            var forw = InputManager.MappedValueInputs[FORWARD];
            var rev = InputManager.MappedValueInputs[REVERSE];
            float val = Mathf.Abs(forw.Value) - Mathf.Abs(rev.Value);

            _armDriver.MainInput = val * _speedMod;
        }

        public override void OnRemove() {
            _armDriver.MainInput = 0f;
            _armDriver.Unreserve();
        }
    }
}
