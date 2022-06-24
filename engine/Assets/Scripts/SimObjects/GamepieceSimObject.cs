using System.Collections;
using System.Collections.Generic;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

public class GamepieceSimObject : SimObject {

    private GameObject _gamepieceObject;
    private Transform _initialTransform;
    public GameObject GamepieceObject { get; private set; }
    public Transform InitialTransform { get; set; }

    public GamepieceSimObject(string name, GameObject g) : base(name, new ControllableState()) {
        GamepieceObject = g;
        InitialTransform = GamepieceObject.transform;

        foreach (Transform t in g.GetComponentsInChildren<Transform>()) {
            t.tag = "gamepiece";
        }
    }

    public void Reset()
    {
        GamepieceObject.transform.position = InitialTransform.position;
        GamepieceObject.transform.rotation = InitialTransform.rotation;
    }
}
