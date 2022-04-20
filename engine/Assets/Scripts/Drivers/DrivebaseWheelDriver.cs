using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SynthesisAPI.Simulation;

public class DrivebaseWheelDriver : Driver {

    private float force;
    private float speed;
    private Vector3 _axis;

    private GameObject _parentBody;
    private GameObject _wheelBody;

    public DrivebaseWheelDriver(string name, string[] inputs, string[] outputs, SimObject simObject, GameObject parentBody, GameObject wheelBody, Vector3 axis, float force, float speed) : base(name, inputs, outputs, simObject) {
        _parentBody = parentBody;
        _wheelBody = wheelBody;
    }

    public override void Update() {
        var percent = base._simObject.State.CurrentSignals[_inputs[0]].Value.NumberValue;
    }
}

public class DrivebaseWheelContactProvider : MonoBehaviour {

    public Dictionary<GameObject, Collision> Collisions;

    public void OnCollisionEnter(Collision collision) {
        Collisions[collision.gameObject] = collision;
    }

    public void OnCollisionExit(Collision collision) {
        if (Collisions.ContainsKey(collision.gameObject)) {
            Collisions.Remove(collision.gameObject);
        }
    }
}
