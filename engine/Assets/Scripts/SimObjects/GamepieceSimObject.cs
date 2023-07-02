using System.Collections;
using System.Collections.Generic;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

using Bounds = UnityEngine.Bounds;

public class GamepieceSimObject : SimObject {
    private GameObject _gamepieceObject;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    public GameObject GamepieceObject { get; private set; }
    public Bounds GamepieceBounds { get; private set; }
    public Vector3 InitialPosition { get; set; }
    public Quaternion InitialRotation { get; set; }
    public ShootableGamepiece Shootable { get; private set; }
    public bool IsCurrentlyPossessed { get; internal set; }

    public GamepieceSimObject(string name, GameObject g) : base(name, new ControllableState()) {
        GamepieceObject = g;
        GamepieceBounds = GetBounds(GamepieceObject.transform);
        InitialPosition = g.transform.position;
        InitialRotation = g.transform.rotation;

        foreach (Transform t in g.GetComponentsInChildren<Transform>()) {
            t.tag = "gamepiece";
        }

        Shootable           = GamepieceObject.AddComponent<ShootableGamepiece>();
        Shootable.SimObject = this;
    }

    private static Bounds GetBounds(Transform top) {
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
                max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        top.GetComponentsInChildren<Renderer>().ForEach(x => {
            var b = x.bounds;
            if (min.x > b.min.x)
                min.x = b.min.x;
            if (min.y > b.min.y)
                min.y = b.min.y;
            if (min.z > b.min.z)
                min.z = b.min.z;
            if (max.x < b.max.x)
                max.x = b.max.x;
            if (max.y < b.max.y)
                max.y = b.max.y;
            if (max.z < b.max.z)
                max.z = b.max.z;
        });
        return new Bounds(((max + min) / 2f) - top.position, max - min);
    }

    public void DeleteGamepiece() {
        GameObject.Destroy(_gamepieceObject);
        SimulationManager.RemoveSimObject(this);
    }

    public void Reset() {
        GamepieceObject.transform.position = InitialPosition;
        GamepieceObject.transform.rotation = InitialRotation;
    }
}
