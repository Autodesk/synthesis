using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Inventor;

public class RigidNode : RigidNode_Base
{
    public CustomRigidGroup group;

    public RigidNode()
        : this(null)
    {
    }
    public RigidNode(CustomRigidGroup grp)
    {
        this.group = grp;
    }

    public override object GetModel()
    {
        return group;
    }
}