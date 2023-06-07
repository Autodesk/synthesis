using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Synthesis;
using UnityEngine;

public class CustomWheel : MonoBehaviour {

    public static bool UseKineticFriction = false;

    private Rigidbody _rb = null;
    public Rigidbody Rb {
        get {
            if (_rb == null) {
                _rb = GetComponent<Rigidbody>();
            }
            return _rb;
        }
    }

    public float ImpulseMax = 100f;

    public Vector3 LocalAxis;
    private Vector3 Axis => Rb.transform.localToWorldMatrix.MultiplyVector(LocalAxis);

    public Vector3 LocalAnchor;
    private Vector3 Anchor => Rb.transform.localToWorldMatrix.MultiplyPoint3x4(LocalAnchor);

    // Friction Constants
    public float SlidingStaticFriction = 1.3f;
    public float SlidingKineticFriction = 0.95f;

    // Motor Stats
    private MotorStats Motor = new MotorStats {
        RadsPerSecondMax = Mathf.PI, // Radians / Second
        StallTorque = 10, // Newton * Meters
        BrakingConstant = 0.1f
    };

    private float _motorPercent = 0f;
    public float MotorPercent {
        get => _motorPercent;
        set {
            _motorPercent = Mathf.Clamp(value, -1f, 1f);
        }
    }

    // Wheel States
    public float RotationSpeed = 0f; // rads/s TODO: Make private eventually
    public float Radius = 0.05f;

    private float _startTime;
    
    private Vector3 _lastImpulseTotal;
    // private bool _lastIsRollingStatic;
    // private bool _lastIsSlidingStatic;

    public bool HasContacts => _pairings.Count > 0;

    public float Inertia => WheelDriver.GetInertiaFromAxisVector(Rb, LocalAxis);
    
    public void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Anchor, 0.05f);

        var offset = new Vector3(0, 0.2f, 0);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(Anchor, Anchor + Axis);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(Anchor, Anchor + _lastImpulseTotal);

        // if (_lastIsRollingStatic)
        //     Gizmos.color = Color.green;
        // else
        //     Gizmos.color = Color.red;
        // Gizmos.DrawSphere(Anchor + offset * 4, 0.05f);
        //
        // if (_lastIsSlidingStatic)
        //     Gizmos.color = Color.green;
        // else
        //     Gizmos.color = Color.red;
        // Gizmos.DrawSphere(Anchor + offset * 4.5f, 0.05f);
    }

    // void FixedUpdate() {
    //     GetFrictionForces();
    // }

    public void GetFrictionForces(float mod) {
        if (_pairings.Count > 0) {
            CalculateFriction();

            _lastImpulseTotal = (_staticImpulseVecAccum + _rollingImpulseVecAccum);

            Rb.velocity += _lastImpulseTotal * mod;// / Rb.mass;

            _staticImpulseVecAccum = new Vector3();
            _rollingImpulseVecAccum = new Vector3();
        }
    }

    private Vector3 _staticImpulseVecAccum = new Vector3();
    private Vector3 _rollingImpulseVecAccum = new Vector3();
    public void OnCollisionStay(Collision collision) {
        // _collisionCalls++;
        _pairings.Add((collision.impulse, collision.relativeVelocity));
    }

    private List<(Vector3 impulse, Vector3 velocity)> _pairings = new List<(Vector3, Vector3)>();
    public void CalculateFriction() {
        Vector3 impulse = Vector3.zero;
        Vector3 velocity = Vector3.zero;
        _pairings.ForEach(x => { impulse += x.impulse; velocity += x.velocity; });
        velocity /= _pairings.Count; // The velocities are different and I don't know why

        impulse = ClampMag(impulse, 0, ImpulseMax);
        
        CalculateSlidingFriction(impulse, velocity);
        CalculateRollingFriction(impulse, velocity);

        _pairings.Clear();
    }

    public void CalculateSlidingFriction(Vector3 impulse, Vector3 velocity) {
        var dirVelocity = Vector3.Dot(Axis, velocity);
        var dirMomentum = dirVelocity * Rb.mass;
        var staticImpulse = SlidingStaticFriction * impulse.magnitude;

        if (UseKineticFriction && dirMomentum > staticImpulse) {
            var dynamicImpulse = ClampMag((SlidingKineticFriction * impulse.magnitude * Axis) / Rb.mass, 0f, dirVelocity);
            _staticImpulseVecAccum += dynamicImpulse;
        } else {
            var staticImpulseVec = dirVelocity * Axis;
            _staticImpulseVecAccum += staticImpulseVec;
        }
    }

    public void CalculateRollingFriction(Vector3 impulse, Vector3 velocity) {
        // var torque = Torque(RotationSpeed, percentInput);
        
        var direction = Vector3.Cross(impulse.normalized, Axis).normalized;
        var wheelSurfaceVelocity = Vector3.Cross(impulse.normalized * Radius, Axis * RotationSpeed);
        var groundSurfaceVelocity = Vector3.Dot(direction, wheelSurfaceVelocity) + Vector3.Dot(-direction, velocity);

        Vector3 frictionImpulse;

        if (UseKineticFriction && SlidingStaticFriction * impulse.magnitude < Mathf.Abs(groundSurfaceVelocity) * Rb.mass) {
            frictionImpulse = ClampMag(
                (SlidingKineticFriction * impulse.magnitude * -direction * Mathf.Sign(groundSurfaceVelocity)) / Rb.mass,
                0f,
                Mathf.Abs(groundSurfaceVelocity)
                );
        } else {
            frictionImpulse = groundSurfaceVelocity * -direction;
        }

        _rollingImpulseVecAccum += frictionImpulse;
    }

    public Vector3 ClampMag(Vector3 v, float min, float max) {
        if (v.magnitude > max)
            return v.normalized * max;
        if (v.magnitude < min)
            return v.normalized * min;
        return v;
    }

    public float Torque(float speed, float percentVoltage) {
        float targetTorque = Motor.StallTorque * Mathf.Clamp((Motor.RadsPerSecondMax - speed) / Motor.RadsPerSecondMax, -0.3f, 1);
        return targetTorque;
        // return targetTorque - (Motor.StallTorque * Motor.BrakingConstant);
    }

    public string Str(Vector3 v) => $"({v.x},{v.y},{v.z})";

    public struct MotorStats {
        public float RadsPerSecondMax;
        public float StallTorque;
        public float BrakingConstant;
    }
    
}
