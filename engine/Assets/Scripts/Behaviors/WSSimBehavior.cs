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
        if (!WebSocketManager.RioState.GetData<DriverStationData>("").Enabled)
            return; // TODO: This should zero out the signals because they aren't "pulled" towards a center point

        var signalState = SimulationManager.SimulationObjects[SimObjectId].State;

        // Motor Groups
        _translation.PWMGroups.ForEach(group => group.Update(WebSocketManager.RioState, signalState));
        
        // Encoders
        _translation.Encoders.ForEach(x => x.Update(WebSocketManager.RioState, signalState));
    }
}
