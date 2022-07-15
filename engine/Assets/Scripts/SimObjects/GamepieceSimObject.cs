using System.Collections;
using System.Collections.Generic;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

public class GamepieceSimObject : SimObject {

    private GameObject _gamepieceObject;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    public GameObject GamepieceObject { get; private set; }
    public Vector3 InitialPosition { get; set; }
    public Quaternion InitialRotation { get; set; }

    public GamepieceSimObject(string name, GameObject g) : base(name, new ControllableState()) {
        GamepieceObject = g;
        InitialPosition = g.transform.position;
        InitialRotation = g.transform.rotation;

        foreach (Transform t in g.GetComponentsInChildren<Transform>()) {
            t.tag = "gamepiece";
        }
    }

    public void Reset()
    {
        GamepieceObject.transform.position = InitialPosition;
        GamepieceObject.transform.rotation = InitialRotation;
    }
}
