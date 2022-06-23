using System.Collections;
using System.Collections.Generic;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

public class GamepieceSimObject : SimObject {

    private GameObject _gamepieceObject;
    private Transform initialTransform;
    public GameObject GamepieceObject { get; set; }

    public GamepieceSimObject(string name, GameObject g) : base(name, new ControllableState()) {
        _gamepieceObject = g;
        initialTransform = _gamepieceObject.transform;

        foreach (Transform t in g.GetComponentsInChildren<Transform>()) {
            t.tag = "gamepiece";
        }
    }

    public void Reset()
    {
        _gamepieceObject.transform.position = initialTransform.position;
        _gamepieceObject.transform.rotation = initialTransform.rotation;
    }
}
