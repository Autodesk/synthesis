using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

public interface RigidNodeFactory
{
    RigidNode_Base create();
}

public class BaseRigidNodeFactory : RigidNodeFactory
{
    public RigidNode_Base create()
    {
        return new RigidNode_Base();
    }
}

public class RigidNode_Base
{
    public static RigidNodeFactory NODE_FACTORY = new BaseRigidNodeFactory();

    protected int level;
    private RigidNode_Base parent;
    private SkeletalJoint_Base parentConnection;
    public string modelName;

    public Dictionary<SkeletalJoint_Base, RigidNode_Base> children = new Dictionary<SkeletalJoint_Base, RigidNode_Base>();

    public void addChild(SkeletalJoint_Base joint, RigidNode_Base child)
    {
        children.Add(joint, child);
        child.parentConnection = joint;
        child.parent = this;
        child.level = this.level + 1;
    }

    public RigidNode_Base getParent()
    {
        return parent;
    }

    public SkeletalJoint_Base getSkeletalJoint()
    {
        return parentConnection;
    }

    public virtual object getModel() { return null; }

    public override string ToString()
    {
        string result = new string(' ', 3 * level) + "Rigid Node" + System.Environment.NewLine + new string(' ', 3 * level) + "Name: " + getModel() + System.Environment.NewLine;
        if (children.Count > 0)
        {
            result += new string(' ', 3 * level) + "Children: ";
            foreach (KeyValuePair<SkeletalJoint_Base, RigidNode_Base> pair in children)
            {
                result += System.Environment.NewLine + new string(' ', 3 * level + 1) + "- " + pair.Key.ToString();
                result += System.Environment.NewLine + pair.Value.ToString();
            }
        }
        return result;
    }

    public void listAllNodes(List<RigidNode_Base> list)
    {
        list.Add(this);
        foreach (KeyValuePair<SkeletalJoint_Base, RigidNode_Base> pair in children)
        {
            pair.Value.listAllNodes(list);
        }
    }
}