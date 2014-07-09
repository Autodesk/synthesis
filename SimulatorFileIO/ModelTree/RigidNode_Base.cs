using System.Collections.Generic;

public interface RigidNodeFactory
{
    RigidNode_Base Create();
}

public class BaseRigidNodeFactory : RigidNodeFactory
{
    public RigidNode_Base Create()
    {
        return new RigidNode_Base();
    }
}

public class RigidNode_Base
{
    /// <summary>
    /// By setting this to a custom value skeletons that are read using <see cref="BXDJSkeleton.ReadSkeleton(string)"/> can 
    /// be composed of a custom rigid node type.
    /// </summary>
    public static RigidNodeFactory NODE_FACTORY = new BaseRigidNodeFactory();

    protected int level;
    private RigidNode_Base parent;
    private SkeletalJoint_Base parentConnection;
    protected string modelFileName;
    protected string modelFullID;

    public Dictionary<SkeletalJoint_Base, RigidNode_Base> children = new Dictionary<SkeletalJoint_Base, RigidNode_Base>();

    public void AddChild(SkeletalJoint_Base joint, RigidNode_Base child)
    {
        children.Add(joint, child);
        child.parentConnection = joint;
        child.parent = this;
        child.level = this.level + 1;
    }

    public RigidNode_Base GetParent()
    {
        return parent;
    }

    public SkeletalJoint_Base GetSkeletalJoint()
    {
        return parentConnection;
    }

    public string GetModelFileName()
    {
        return modelFileName;
    }

    public virtual string GetModelID()
    {
        return modelFullID;
    }

    public void SetModelFileName(string s)
    {
        modelFileName = s;
    }

    public void SetModelID(string s)
    {
        modelFullID = s;
    }

    public virtual object GetModel()
    {
        return null;
    }

    public override string ToString()
    {
        string result = new string('\t', level) + "Rigid Node" + System.Environment.NewLine;
        result += new string('\t', level) + "Name: " + GetModel() + System.Environment.NewLine;
        if (parentConnection != null && parentConnection.cDriver != null)
        {
            result += new string('\t', level) + "Driver: " + ("\n" + parentConnection.cDriver.ToString()).Replace("\n", "\n" + new string('\t', level + 1));
        }
        if (children.Count > 0)
        {
            result += new string('\t', level) + "Children: ";
            foreach (KeyValuePair<SkeletalJoint_Base, RigidNode_Base> pair in children)
            {
                result += System.Environment.NewLine + new string('\t', level) + " - " + pair.Key.ToString();
                result += System.Environment.NewLine + pair.Value.ToString();
                result += "\n";
            }
        }
        return result;
    }

    public void ListAllNodes(List<RigidNode_Base> list)
    {
        list.Add(this);
        foreach (KeyValuePair<SkeletalJoint_Base, RigidNode_Base> pair in children)
        {
            pair.Value.ListAllNodes(list);
        }
    }
}