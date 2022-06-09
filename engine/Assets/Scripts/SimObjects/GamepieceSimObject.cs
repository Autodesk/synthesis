using System.Collections;
using System.Collections.Generic;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

public class GamepieceSimObject : SimObject {

    private GameObject _gamepieceObject;

    public GamepieceSimObject(string name, GameObject g) : base(name, new ControllableState()) {
        _gamepieceObject = g;

        foreach (Transform t in g.GetComponentsInChildren<Transform>()) {
            t.tag = "gamepiece";
        }
    }
}
