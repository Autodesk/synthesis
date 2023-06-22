using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Synthesis;
using UnityEngine;

public class CustomWheel : MonoBehaviour {

    // When enabled, you get weird priority effects. Leave disabled for now.
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
    public float SlidingStaticFriction  = 1.3f;
    public float SlidingKineticFriction = 0.95f;

    // Wheel States
    public float RotationSpeed = 0f;    // radians/second
    public float Radius        = 0.05f; // meters

    private float _startTime;

    public bool HasContacts => _pairings.Count > 0;

    public float Inertia => WheelDriver.GetInertiaFromAxisVector(Rb, LocalAxis);

    // Debugging Information
    private Vector3 _lastImpulseTotal;

    public void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Anchor, 0.05f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(Anchor, Anchor + Axis);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(Anchor, Anchor + _lastImpulseTotal);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Anchor, Anchor + (Axis * RotationSpeed));
    }

    /// <summary>
    /// Calculates friction forces for the wheel and applies them. Should be run every FixedUpdate
    /// </summary>
    /// <param name="mod">Modifier to account for janky Unity joints</param>
    public void GetFrictionForces(float mod) {
        if (_pairings.Count > 0) {
            CalculateFriction();

            _lastImpulseTotal = (_staticImpulseVecAccum + _rollingImpulseVecAccum);

            Rb.velocity += _lastImpulseTotal * mod; // / Rb.mass;

            _staticImpulseVecAccum  = new Vector3();
            _rollingImpulseVecAccum = new Vector3();
        }
    }

    // Tbh these variables are relics but I don't want to mess with anything
    private Vector3 _staticImpulseVecAccum  = new Vector3();
    private Vector3 _rollingImpulseVecAccum = new Vector3();
    public void OnCollisionStay(Collision collision) {
        // _collisionCalls++;
        _pairings.Add((collision.impulse, collision.relativeVelocity));
    }

    private List<(Vector3 impulse, Vector3 velocity)> _pairings = new List<(Vector3, Vector3)>();
    /// <summary>
    /// Compiles contacts and calculates friction forces
    /// </summary>
    public void CalculateFriction() {
        Vector3 impulse  = Vector3.zero;
        Vector3 velocity = Vector3.zero;
        _pairings.ForEach(x => {
            impulse += x.impulse;
            velocity += x.velocity;
        });
        velocity /= _pairings.Count; // The velocities are different and I don't know why

        impulse = ClampMag(impulse, 0, ImpulseMax);

        CalculateSlidingFriction(impulse, velocity);
        CalculateRollingFriction(impulse, velocity);

        _pairings.Clear();
    }

    /// <summary>
    /// Calculates friction forces parallel to the axis of rotation
    /// </summary>
    /// <param name="impulse">Impulse of the collision data</param>
    /// <param name="velocity">Relative velocity of the object to the contacted object</param>
    public void CalculateSlidingFriction(Vector3 impulse, Vector3 velocity) {
        var dirVelocity   = Vector3.Dot(Axis, velocity);
        var dirMomentum   = dirVelocity * Rb.mass;
        var staticImpulse = SlidingStaticFriction * impulse.magnitude;

        if (UseKineticFriction && dirMomentum > staticImpulse) {
            var dynamicImpulse =
                ClampMag((SlidingKineticFriction * impulse.magnitude * Axis) / Rb.mass, 0f, dirVelocity);
            _staticImpulseVecAccum += dynamicImpulse;
        } else {
            var staticImpulseVec = dirVelocity * Axis;
            _staticImpulseVecAccum += staticImpulseVec;
        }
    }

    /// <summary>
    /// Calculates friction forces perpendicular to the axis of rotation
    /// </summary>
    /// <param name="impulse">Impulse of the collision data</param>
    /// <param name="velocity">Relative velocity of the object to the contacted object</param>
    public void CalculateRollingFriction(Vector3 impulse, Vector3 velocity) {
        // var torque = Torque(RotationSpeed, percentInput);

        var direction             = Vector3.Cross(impulse.normalized, Axis).normalized;
        var wheelSurfaceVelocity  = Vector3.Cross(impulse.normalized * Radius, Axis * RotationSpeed);
        var groundSurfaceVelocity = Vector3.Dot(direction, wheelSurfaceVelocity) + Vector3.Dot(-direction, velocity);

        Vector3 frictionImpulse;

        if (UseKineticFriction &&
            SlidingStaticFriction * impulse.magnitude < Mathf.Abs(groundSurfaceVelocity) * Rb.mass) {
            frictionImpulse = ClampMag(
                (SlidingKineticFriction * impulse.magnitude * -direction * Mathf.Sign(groundSurfaceVelocity)) / Rb.mass,
                0f, Mathf.Abs(groundSurfaceVelocity));
        } else {
            frictionImpulse = groundSurfaceVelocity * -direction;
        }

        _rollingImpulseVecAccum += frictionImpulse;
    }

    /// <summary>
    /// Clamp function for the magnitude of a vector
    /// </summary>
    /// <param name="v">Vector to clamp</param>
    /// <param name="min">Minimum magnitude</param>
    /// <param name="max">Maximum magnitude</param>
    /// <returns>Clamped Vector</returns>
    public Vector3 ClampMag(Vector3 v, float min, float max) {
        if (v.magnitude > max)
            return v.normalized * max;
        if (v.magnitude < min)
            return v.normalized * min;
        return v;
    }

    /// <summary>
    /// Utility function to print Vectors. Shows more detail than Vector.ToString().
    /// </summary>
    /// <param name="v">Vector to print</param>
    /// <returns>Stringified vector</returns>
    public string Str(Vector3 v) => $"({v.x},{v.y},{v.z})";
}
