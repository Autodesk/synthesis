using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BetterWheelCollider : MonoBehaviour
{
    public float currentTorque;
    public float brakeTorque;
    public float sidewaysGrip = 0.75f;  // 0=slides, 1=doesn't
    public float forwardsGrip = 1;  // 0=no grip, 1=full grip
    public float forceMultiplier = 4.2f; // idkwhy
	public float wheelAngle = 0;
	public int wheelType = (int)WheelType.NORMAL;

    public BetterWheelCollider() {
    }

    public void Start()
    {
        rigidbody.drag = 1f;
        rigidbody.angularDrag = 1f;
    }

	private Vector3 lastNormalDrag = Vector3.zero;

    float color = 0;
    Color[] forceColor = { Color.blue, Color.cyan, Color.green };
	Color[] dragColor = { Color.magenta, Color.red, Color.yellow };
    public void OnCollisionStay(Collision collisionInfo)
    {
        Vector3 normal = Vector3.zero;
        Vector3 point = Vector3.zero;
        #region compute normal and point
        foreach (ContactPoint contact in collisionInfo.contacts)
        {
            normal += contact.normal;
            point += contact.point;
        }
        point /= collisionInfo.contacts.Length;
        normal.Normalize();
        #endregion

        // Get the world rotation specs
        Vector3 axis = transform.localToWorldMatrix * gameObject.GetComponent<HingeJoint>().axis;
        Vector3 basePoint = transform.localToWorldMatrix * gameObject.GetComponent<HingeJoint>().connectedAnchor;
        axis.Normalize();

        Vector3 relativeVelocity = rigidbody.velocity;
        if (collisionInfo.rigidbody != null)
            relativeVelocity -= collisionInfo.rigidbody.velocity;

		Vector3 forceDirection = Vector3.Cross (normal, axis).normalized;

		if (wheelType == (int)WheelType.MECANUM)
			forceDirection = Quaternion.Euler (0, wheelAngle, 0) * forceDirection;

		float appliedRadius = 1;//Vector3.Distance(point, basePoint);
        Vector3 force = Vector3.zero;
        float normalVelocity = Vector3.Dot(relativeVelocity, axis);
		if (Math.Abs(normalVelocity) > 1)
			normalVelocity = Math.Sign(normalVelocity);
		Vector3 normalDrag = -sidewaysGrip * Math.Abs(normalVelocity) * normalVelocity * axis * Init.PHYSICS_MASS_MULTIPLIER;

		if (wheelType == (int)WheelType.MECANUM)
			normalDrag = Quaternion.Euler (0, wheelAngle, 0) * normalDrag;

		if (lastNormalDrag != Vector3.zero)
		{
			Vector3 tmpDrag = normalDrag * 0.5f;
			normalDrag = tmpDrag + lastNormalDrag;
			lastNormalDrag = tmpDrag;
		}

        if (currentTorque != 0)
        {
            force = forwardsGrip * currentTorque / appliedRadius * forceDirection;
        }
        else
        {
            // braking
			float rotationalVelocity = Vector3.Dot(forceDirection, relativeVelocity) * Init.PHYSICS_MASS_MULTIPLIER;
            if (Math.Abs(rotationalVelocity) > 0.01)
                force = forwardsGrip * -(Math.Abs(rotationalVelocity) > brakeTorque ? brakeTorque * Math.Sign(rotationalVelocity) : rotationalVelocity) * forceDirection;
        }
		#region DEBUG
        int colorMain = ((int) color) % forceColor.Length;
		int colorSecond = ((int) (color + 1)) % forceColor.Length;
        float weight = color - (int)color;
		Color showF = Color.Lerp(forceColor[colorMain], forceColor[colorSecond], weight);
		Color showN = Color.Lerp(dragColor[colorMain], dragColor[colorSecond], weight);
        Debug.DrawRay(point, -force / Init.PHYSICS_MASS_MULTIPLIER, showF, 0.5f);
		Debug.DrawRay(point, -normalDrag / Init.PHYSICS_MASS_MULTIPLIER, showN, 0.5f);
        color = color + 0.1f;
		#endregion

		rigidbody.AddForce((force + normalDrag) * forceMultiplier, ForceMode.Impulse);
    }
}
