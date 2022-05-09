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

public class GeneralArmBehaviour : SimBehaviour {

    internal string FORWARD = " Forward"; // TODO
    internal string REVERSE = " Reverse"; // TODO

    private string _armSignal;
    private float _speedMod = 0.4f;

    public GeneralArmBehaviour(string simObjectId, string armSignal) : base(simObjectId) {
        _armSignal = armSignal;

        var name = SimulationManager.SimulationObjects[simObjectId].State.CurrentSignalLayout.SignalMap[armSignal].Info.Name;
        FORWARD = name + FORWARD;
        REVERSE = name + REVERSE;

        if (RobotSimObject.ControllableJointCounter > 9)
            Logger.Log("Too Many Arm Joints. Need to come up with a better plan to generate keys", LogLevel.Debug);

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

        SimulationManager.SimulationObjects[SimObjectId].State.CurrentSignals[_armSignal].Value = Value.ForNumber(val * _speedMod);
    }
}
