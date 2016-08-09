using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BulletUnity;

public partial class RigidNode : RigidNode_Base
{
    public GameObject MainObject;
    private Transform root;
    private Component joint;
    private PhysicalProperties physicalProperties;
    private Vector3 comOffset;

    public RigidNode(Guid guid)
        : base(guid)
    {
    }

    public void CreateTransform(Transform root)
    {
        MainObject = new GameObject(ModelFileName);
        MainObject.transform.parent = root;

        this.root = root;

        comOffset = Vector3.zero;
    }
}
