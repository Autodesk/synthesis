using System.Collections.Generic;
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
    private const float SlidingStaticFriction  = 1.3f;
    private const float SlidingKineticFriction = 0.95f;
    // Wheel States
    public float RotationSpeed = 0f;    // radians/second
    public float Radius        = 0.05f; // meters

    private float _startTime;

    public bool HasContacts => _collisionDataThisFrame.numCollisions > 0;

    public float Inertia => WheelDriver.GetInertiaFromAxisVector(Rb, LocalAxis);

    private Vector3 _lastImpulseTotal;

    public void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Anchor, 0.05f);

        /*Gizmos.color = Color.green;
        Gizmos.DrawLine(Anchor, Anchor + Axis);*/

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(Anchor, Anchor + _lastImpulseTotal);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Anchor, Anchor + (Axis * RotationSpeed));
    }

    /// <summary>
    /// Calculates friction forces for the wheel and applies them. Should be run every FixedUpdate
    /// </summary>
    /// <param name="mod">Modifier to account for janky Unity joints</param>
    public void CalculateAndApplyFriction(float mod) {
        if (!HasContacts)
            return;

        _lastImpulseTotal = CalculateNetFriction();

        Rb.velocity += _lastImpulseTotal * mod; // / Rb.mass;
    }

    public void OnCollisionStay(Collision collision) {
        Vector3 impulse = collision.impulse;

        // If impulse vector is suspected of being backwards (happens with mean machine), calculate it manually
        if (impulse.normalized.y < 0.01) {
            impulse = Vector3.zero;
            collision.contacts.ForEach(contact => {
                impulse += (Rb.worldCenterOfMass - contact.point).normalized * contact.impulse.magnitude;
            });
        }

        _collisionDataThisFrame.impulse += impulse;
        _collisionDataThisFrame.velocity += collision.relativeVelocity;
        _collisionDataThisFrame.numCollisions++;
    }

    private (Vector3 impulse, Vector3 velocity, int numCollisions) _collisionDataThisFrame;

    /// <summary>
    /// Compiles contacts and calculates friction forces
    /// </summary>
    /// <returns>The net friction force acting on the wheel from collisions this frame</returns>
    private Vector3 CalculateNetFriction() {
        Vector3 netImpulse  = _collisionDataThisFrame.impulse;
        Vector3 netVelocity = _collisionDataThisFrame.velocity;

        netVelocity /= _collisionDataThisFrame.numCollisions; // The velocities are different and I don't know why

        netImpulse = ClampMag(netImpulse, 0, ImpulseMax);

        _collisionDataThisFrame = new(Vector3.zero, Vector3.zero, 0);

        return CalculateSlidingFriction(netImpulse, netVelocity) + CalculateRollingFriction(netImpulse, netVelocity);
    }

    /// <summary>
    /// Calculates friction forces parallel to the axis of rotation
    /// </summary>
    /// <param name="impulse">Impulse of the collision data</param>
    /// <param name="velocity">Relative velocity of the object to the contacted object</param>
    /// <returns>A vector for sliding friction between the wheel and the ground</returns>
    private Vector3 CalculateSlidingFriction(Vector3 impulse, Vector3 velocity) {
        var dirVelocity   = Vector3.Dot(Axis, velocity);
        var dirMomentum   = dirVelocity * Rb.mass;
        var staticImpulse = SlidingStaticFriction * impulse.magnitude;

        if (UseKineticFriction && dirMomentum > staticImpulse) {
            var dynamicImpulse =
                ClampMag((SlidingKineticFriction * impulse.magnitude * Axis) / Rb.mass, 0f, dirVelocity);
            return dynamicImpulse;
        } else {
            var staticImpulseVec = dirVelocity * Axis;
            return staticImpulseVec;
        }
    }

    /// <summary>
    /// Calculates friction forces perpendicular to the axis of rotation
    /// </summary>
    /// <param name="impulse">Impulse of the collision data</param>
    /// <param name="velocity">Relative velocity of the object to the contacted object</param>
    /// <returns>A vector for rolling friction between the wheel and the ground</returns>
    private Vector3 CalculateRollingFriction(Vector3 impulse, Vector3 velocity) {
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

        return frictionImpulse;
    }

    /// <summary>
    /// Clamp function for the magnitude of a vector
    /// </summary>
    /// <param name="v">Vector to clamp</param>
    /// <param name="min">Minimum magnitude</param>
    /// <param name="max">Maximum magnitude</param>
    /// <returns>Clamped Vector</returns>
    public static Vector3 ClampMag(Vector3 v, float min, float max) {
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
    public static string Str(Vector3 v) => $"({v.x},{v.y},{v.z})";
}
