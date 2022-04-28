using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

using Logger = SynthesisAPI.Utilities.Logger;

public class GeneralArmBehaviour : SimBehaviour {

    internal const string FORWARD = "Joint Forward"; // TODO
    internal const string REVERSE = "Joint Reverse"; // TODO

    private string _armSignal;
    private float _speedMod = 1.0f;

    public GeneralArmBehaviour(string simObjectId, string armSignal) : base(simObjectId) {
        _armSignal = armSignal;

        InputManager.AssignValueInput(FORWARD, new Digital("G"));
        InputManager.AssignValueInput(REVERSE, new Digital("H"));
    }

    public override void Update() {

        var forw = InputManager.MappedValueInputs[FORWARD];
        var rev = InputManager.MappedValueInputs[REVERSE];
        float val = Mathf.Abs(forw.Value) - Mathf.Abs(rev.Value);

        SimulationManager.SimulationObjects[SimObjectId].State.CurrentSignals[_armSignal].Value = Value.ForNumber(val * _speedMod);
        if (forw.Value != 0 || rev.Value != 0)
            Logger.Log($"Forward: {forw.Value}\nReverse: {rev.Value}\nValue: {val}", LogLevel.Debug);
    }
}
