using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class UnityRigidNode : RigidNode_Base
{
    public GameObject unityObject, wheelCollider;

    private Joint joint;
    /// <summary>
    /// Cached physics data from the original BXDA Mesh.
    /// </summary>
    private PhysicalProperties bxdPhysics;
    //private float center, current;

    //The root transform for the whole object model is determined in this constructor passively
    public void CreateTransform(Transform root)
    {
        unityObject = new GameObject();
        unityObject.transform.parent = root;
        unityObject.transform.position = new Vector3(0, 0, 0);
        unityObject.name = base.modelFileName;
    }

    /// <summary>
    /// Creates a joint at the given position, aligned to the given axis, with the given type.
    /// </summary>
    /// <typeparam name="T">The joint type</typeparam>
    /// <param name="pos">The base position</param>
    /// <param name="axis">The axis</param>
    /// <param name="jointType">The joint callback for additional configuration</param>
    /// <returns>The joint that was created</returns>
    private T ConfigJointInternal<T>(Vector3 pos, Vector3 axis, Action<T> jointType) where T : Joint
    {
        GameObject rigid = ((UnityRigidNode) GetParent()).unityObject;
        if (!rigid.gameObject.GetComponent<Rigidbody>())
        {
            rigid.gameObject.AddComponent<Rigidbody>();
        }
        Rigidbody rigidB = rigid.gameObject.GetComponent<Rigidbody>();
        joint = unityObject.gameObject.AddComponent<T>();

        joint.connectedBody = rigidB;

        //configures the joint
        joint.anchor = pos;
        joint.connectedAnchor = pos;

        axis.Normalize();
        joint.axis = axis;


        //joint.secondaryAxis = new Vector3 (0, 0, 1);
        if (joint is ConfigurableJoint)
        {
            ConfigurableJoint cj = (ConfigurableJoint) joint;
            cj.angularXMotion = ConfigurableJointMotion.Locked;
            cj.angularYMotion = ConfigurableJointMotion.Locked;
            cj.angularZMotion = ConfigurableJointMotion.Locked;
            cj.xMotion = ConfigurableJointMotion.Locked;
            cj.yMotion = ConfigurableJointMotion.Locked;
            cj.zMotion = ConfigurableJointMotion.Locked;
        }
        jointType((T) joint);
        return (T) joint;
    }

    /// <summary>
    /// Creates the capsule collider and better wheel collider for this object.
    /// </summary>
    /// <param name="center">The joint to center on</param>
    private void CreateWheel(RotationalJoint_Base center)
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
        unityObject.AddComponent<BetterWheelCollider>().attach(this);
        //I want the grandfather to have a rigidbody

        unityObject.GetComponent<Rigidbody>().useConeFriction = true;
    }

    /// <summary>
    /// Gets the joint for this node as the given joint type, or null if it doesn't exist.
    /// </summary>
    /// <typeparam name="T">The joint type</typeparam>
    /// <returns>The joint, or null if no such joint exists</returns>
    public T GetJoint<T>() where T : Joint
    {
        return joint as T;
    }
}
