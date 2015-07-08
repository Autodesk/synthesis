using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class UnityRigidNode : RigidNode_Base
{
    /// <summary>
    /// Creates the capsule collider and better wheel collider for this object.
    /// </summary>
    private void CreateWheel()
    {
        WheelDriverMeta wheel = this.GetDriverMeta<WheelDriverMeta>();
        if (wheel == null)
            return;

        wheelCollider = new GameObject(unityObject.name + " Collider");

        wheelCollider.transform.parent = unityObject.transform;
        Vector3 anchorBase = joint.connectedAnchor;
        float centerMod = Vector3.Dot(wheel.center.AsV3() - anchorBase, joint.axis);
        wheelCollider.transform.localPosition = centerMod * joint.axis + anchorBase;
        wheelCollider.transform.rotation = Quaternion.FromToRotation(new Vector3(1, 0, 0), new Vector3(joint.axis.x, joint.axis.y, joint.axis.z));

        wheelCollider.AddComponent<CapsuleCollider>().radius = (wheel.radius * 1.10f) * 0.01f;
        wheelCollider.GetComponent<CapsuleCollider>().height = wheelCollider.GetComponent<CapsuleCollider>().radius / 4f + wheel.width * 0.01f;
        wheelCollider.GetComponent<CapsuleCollider>().center = new Vector3(0, 0, 0);
        wheelCollider.GetComponent<CapsuleCollider>().direction = 0;
        unityObject.AddComponent<BetterWheelCollider>();
        //I want the grandfather to have a rigidbody

        unityObject.GetComponent<Rigidbody>().useConeFriction = true;
    }

    /// <summary>
    /// Orients drive wheel normals so they face away from the center of mass.
    /// </summary>
    /// <remarks>
    /// Implemented so that the joint's axis is negated when the angle between the joint's axis and the vector from
    /// the wheel to the center of mass is greater than 90 degrees.
    /// </remarks>
    private void OrientWheelNormals()
    {
        if (GetSkeletalJoint() is RotationalJoint_Base && this.HasDriverMeta<WheelDriverMeta>())
        {
            Vector3 com = ((UnityRigidNode) GetParent()).bxdPhysics.centerOfMass.AsV3();
            RotationalJoint_Base rJoint = (RotationalJoint_Base) GetSkeletalJoint();
            Vector3 diff = rJoint.basePoint.AsV3() - com;
            double dot = Vector3.Dot(diff, rJoint.axis.AsV3());
            if (dot < 0)
            {
                rJoint.axis = rJoint.axis.Multiply(-1);
            }
        }
    }
}