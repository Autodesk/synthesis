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

    public void OnCollisionStay(Collision collisionInfo)
    {
        Vector3 norm = Vector3.zero;
        Vector3 point = Vector3.zero;
        foreach (ContactPoint contact in collisionInfo.contacts)
        {
            norm += contact.normal;
            point += contact.point;
        }
        point /= collisionInfo.contacts.Length;
        norm.Normalize();
        Vector3 axis = transform.localToWorldMatrix * gameObject.GetComponent<ConfigurableJoint>().axis;
        axis.Normalize();
        Vector3 basePoint = transform.localToWorldMatrix * gameObject.GetComponent<ConfigurableJoint>().connectedAnchor;
        Vector3 crossed = Vector3.Cross(norm, axis).normalized;
        float appliedRadius = Vector3.Distance(point, basePoint);
        Vector3 force = Vector3.zero;
        Vector3 relVel = rigidbody.velocity - collisionInfo.rigidbody.velocity;
        float normalVel = Vector3.Dot(relVel, axis);
        Vector3 normalDrag = -sidewaysGrip * normalVel * axis * forceMultiplier;
        if (currentTorque != 0)
        {
            force = forwardsGrip * currentTorque / appliedRadius * crossed;
        }
        else
        {
            // braking
            float rotVel = Vector3.Dot(crossed, relVel) / 10f;
            if (Math.Abs(rotVel) > 0.01)
                force = forwardsGrip * -(Math.Abs(rotVel) > brakeTorque ? brakeTorque * Math.Sign(rotVel) : rotVel) * crossed;
        }
        Debug.DrawRay(point, -force);
        Debug.DrawRay(point, -normalDrag);
        rigidbody.AddForce(force * forceMultiplier, ForceMode.Impulse);
        // Sideways friction bro
        rigidbody.AddForce(normalDrag, ForceMode.Impulse);
    }
}
