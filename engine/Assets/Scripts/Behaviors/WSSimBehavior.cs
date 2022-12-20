using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Synthesis.WS;
using Synthesis.WS.Translation;
using SynthesisAPI.RoboRIO;
using SynthesisAPI.Simulation;
using UnityEngine;

public class WSSimBehavior : SimBehaviour {

    private RioTranslationLayer _translation;
    public RioTranslationLayer Translation {
        get => _translation;
        set {
            _translation = value;
        }
    }

    public WSSimBehavior(string simObjectId, RioTranslationLayer translation) : base(simObjectId) {
        _translation = translation;

        WebSocketManager.Init();
    }

    public override void Update() {
        if (!WebSocketManager.RioState.GetData<DriverStationData>("DriverStation", "").Enabled)
            return; // TODO: This should zero out the signals because they aren't "pulled" towards a center point

        _translation.MotorGroups.ForEach(group => group.Update(WebSocketManager.RioState, SimulationManager.SimulationObjects[SimObjectId].State));
    }
}
