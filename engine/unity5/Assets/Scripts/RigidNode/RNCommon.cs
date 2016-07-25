using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BulletUnity;

public partial class RigidNode : RigidNode_Base
{
    private GameObject gameObject;
    private Component joint;
    private PhysicalProperties physicalProperties;

    public RigidNode(Guid guid)
        : base(guid)
    {
    }

    public void CreateTransform(Transform root)
    {
        gameObject = new GameObject();
        gameObject.transform.parent = root;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.name = ModelFileName;
    }
}
