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
    /// Gets the joint for this node as the given joint type, or null if it doesn't exist.
    /// </summary>
    /// <typeparam name="T">The joint type</typeparam>
    /// <returns>The joint, or null if no such joint exists</returns>
    public T GetJoint<T>() where T : Joint
    {
        return joint as T;
    }
}
