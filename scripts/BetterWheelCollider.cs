using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BetterWheelCollider : MonoBehaviour
{
    private UnityRigidNode node;
    public float currentTorque;
    public float brakeTorque;
    public float sidewaysGrip = 0.5f;  // 0=slides, 1=doesn't
    public float forwardsGrip = 1;  // 0=no grip, 1=full grip
    public float forceMultiplier = 0.035f; // idkwhy

    public BetterWheelCollider() {
    }

    public void attach(UnityRigidNode node)
    {
        this.node = node;
        rigidbody.drag = 1f;
        rigidbody.angularDrag = 1f;
    }

    float color = 0;
    Color[] colors = { Color.blue, Color.cyan, Color.green, Color.magenta, Color.red, Color.yellow };
    public void OnCollisionStay(Collision collisionInfo)
    {
        Vector3 normal = Vector3.zero;
        Vector3 point = Vector3.zero;
        foreach (ContactPoint contact in collisionInfo.contacts)
        {
            normal += contact.normal;
            point += contact.point;
        }
        point /= collisionInfo.contacts.Length;
        normal.Normalize();

        // Get the world rotation specs
        Vector3 axis = transform.localToWorldMatrix * gameObject.GetComponent<HingeJoint>().axis;
        Vector3 basePoint = transform.localToWorldMatrix * gameObject.GetComponent<HingeJoint>().connectedAnchor;
        axis.Normalize();

        Vector3 relativeVelocity = rigidbody.velocity - collisionInfo.rigidbody.velocity;

        Vector3 forceDirection = Vector3.Cross(normal, axis).normalized;
        float appliedRadius = Vector3.Distance(point, basePoint);
        Vector3 force = Vector3.zero;
        float normalVelocity = Vector3.Dot(relativeVelocity, axis);
        Vector3 normalDrag = -sidewaysGrip * normalVelocity * axis * forceMultiplier;
        if (currentTorque != 0)
        {
            force = forwardsGrip * currentTorque / appliedRadius * forceDirection;
        }
        else
        {
            // braking
            float rotationalVelocity = Vector3.Dot(forceDirection, relativeVelocity) / 10f;
            if (Math.Abs(rotationalVelocity) > 0.01)
                force = forwardsGrip * -(Math.Abs(rotationalVelocity) > brakeTorque ? brakeTorque * Math.Sign(rotationalVelocity) : rotationalVelocity) * forceDirection;
        }
        int colorMain = ((int) color) % colors.Length;
        int colorSecond = ((int) (color + 1)) % colors.Length;
        float weight = color - (int)color;
        Color show = Color.Lerp(colors[colorMain], colors[colorSecond], weight);
        Debug.DrawRay(point, -force * 10, show, 0.5f);
        Debug.DrawRay(point, -normalDrag * 10, show, 0.5f);
        color = color + 0.1f;
        rigidbody.AddForce(force * forceMultiplier, ForceMode.Impulse);
        // Sideways friction bro
        rigidbody.AddForce(normalDrag, ForceMode.Impulse);
    }
}
