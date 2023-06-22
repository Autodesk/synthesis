using Google.Protobuf.WellKnownTypes;
using Synthesis.WS;
using Synthesis.WS.Translation;
using SynthesisAPI.RoboRIO;
using SynthesisAPI.Simulation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSSimBehavior : SimBehaviour {
    private RioTranslationLayer _translation;

    public RioTranslationLayer Translation {
        get => _translation;
        set { _translation = value; }
    }

    public WSSimBehavior(string simObjectId, RioTranslationLayer translation) : base(simObjectId) {
        _translation = translation;

        WebSocketManager.Init();
    }

    public override void Update() {
        var signalState = SimulationManager.SimulationObjects[SimObjectId].State;

        // Motor Groups
        _translation.PWMGroups.ForEach(group => group.Update(WebSocketManager.RioState, signalState));

        // Encoders
        _translation.Encoders.ForEach(x => x.Update(signalState));
    }
}
