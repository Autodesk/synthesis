using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CustomWheel : MonoBehaviour {

    public (float staticF, float dynamicF) WheelFriction;
    public (float staticF, float dynamicF) RollingFriction;
    public Func<Vector3> ForwardDir;
    public Func<Vector3> RightDir;

    private Rigidbody _rb;
    private FileStream _fs;
    private StreamWriter _writer;

    void Start() {
        _rb = GetComponent<Rigidbody>();
    }

    void Update() {

    }

    void FixedUpdate() {
        // Debug.Log("Physics Update");
    }

    public void OnCollisionStay(Collision collision) {
        ApplyFriction(collision, RightDir(), WheelFriction.staticF, WheelFriction.dynamicF); // Static
        ApplyFriction(collision, ForwardDir(), RollingFriction.staticF, RollingFriction.dynamicF); // Rolling
    }

    public void ApplyFriction(Collision collision, Vector3 direction, float staticFriction, float kineticFriction) {
        var dirVelocity = Vector3.Dot(direction, collision.relativeVelocity);
        var dirMomentum = dirVelocity * _rb.mass;
        var staticImpulse = staticFriction * collision.impulse.magnitude;
        if (dirMomentum > staticImpulse) {
            var dynamicImpulse = ClampMag((kineticFriction * collision.impulse.magnitude * direction) / _rb.mass, 0f, dirVelocity);
            // Debug.Log($"Dynamic\n{dynamicImpulse.magnitude}");
            _rb.velocity += dynamicImpulse;
        } else {
            var staticImpulseVec = dirVelocity * direction;
            // Debug.Log($"Static\n{staticImpulseVec.magnitude}");
            _rb.velocity += staticImpulseVec;
        }
    }

    public Vector3 ClampMag(Vector3 v, float min, float max) {
        if (v.magnitude > max)
            return v.normalized * max;
        else if (v.magnitude < min)
            return v.normalized * min;
        return v;
    }
}
