using System;
using System.Collections.Generic;

/// <summary>
/// Represents a node inside the hierarchy representing how a robot moves.
/// </summary>
public class RigidNode_Base
{
    /// <summary>
    /// Generic delegate for creating rigid node instances
    /// </summary>
    public delegate RigidNode_Base RigidNodeFactory(Guid guid);

    /// <summary>
    /// By setting this to a custom value skeletons that are read using <see cref="BXDJSkeleton.ReadBinarySkeleton(string)"/> can 
    /// be composed of a custom rigid node type.
    /// </summary>
    public static RigidNodeFactory NODE_FACTORY = delegate(Guid guid)
    {
        return new RigidNode_Base(guid);
    };

    /// <summary>
    /// How far down in the hierarchy this element is.  The higher it is the farther from the root node.
    /// </summary>
    protected int level;

    /// <summary>
    /// The node that represents the parent of this node.  If this is null then this is a root node.
    /// </summary>
    private RigidNode_Base parent;

    /// <summary>
    /// The joint that connects this node to its parent.
    /// </summary>
    private SkeletalJoint_Base parentConnection;

    /// <summary>
    /// The name of the file holding this node's model.
    /// </summary>
    public string ModelFileName;

    /// <summary>
    /// The globally unique identifier.
    /// </summary>
    public Guid GUID
    {
        get;
        private set;
    }

    /// <summary>
    /// A very verbose identifier that represents the element this node is in the overall structure.
    /// </summary>
    public string ModelFullID;

    /// <summary>
    /// A mapping between each child node of this node and the joint connection between the two.
    /// </summary>
    public Dictionary<SkeletalJoint_Base, RigidNode_Base> Children = new Dictionary<SkeletalJoint_Base, RigidNode_Base>();

    /// <summary>
    /// Initializes a new instance of the RigidNode_Base class.
    /// </summary>
    /// <param name="guid"></param>
    public RigidNode_Base(Guid guid)
    {
        GUID = guid;
    }

    /// <summary>
    /// Adds the given node as a child of this node.
    /// </summary>
    /// <param name="joint">The joint connecting this node to the child</param>
    /// <param name="child">The child node</param>
    public void AddChild(SkeletalJoint_Base joint, RigidNode_Base child)
    {
        Children.Add(joint, child);
        child.parentConnection = joint;
        child.parent = this;
        child.level = level + 1;
    }

    /// <summary>
    /// Gets the parent node for this node.
    /// </summary>
    /// <returns>The parent node, or null if this node is a root node</returns>
    public RigidNode_Base GetParent()
    {
        return parent;
    }

    /// <summary>
    /// Gets the skeletal joint connecting this node to its parent.
    /// </summary>
    /// <remarks>
    /// This should always be non-null when the current node isn't a root node.
    /// </remarks>
    /// <returns>The joint connection, or null if no connection exists.</returns>
    public SkeletalJoint_Base GetSkeletalJoint()
    {
        return parentConnection;
    }

    /// <summary>
    /// Gets the actual object visually representing this rigid node if such an item exists.
    /// </summary>
    /// <returns>The representation, or null</returns>
    public virtual object GetModel()
    {
        return null;
    }

    /// <summary>
    /// Gets a very verbose identifier that represents the element this node is in the overall structure.
    /// </summary>
    /// <returns>The model identifier</returns>
    public virtual string GetModelID()
    {
        return ModelFullID;
    }

    public override string ToString()
    {
        string result = new string('\t', level) + "Rigid Node" + System.Environment.NewLine;
        result += new string('\t', level) + "ID: " + ModelFullID + System.Environment.NewLine;
        if (parentConnection != null && parentConnection.cDriver != null)
        {
            result += new string('\t', level) + "Driver: " + ("\n" + parentConnection.cDriver.ToString()).Replace("\n", "\n" + new string('\t', level + 1));
        }
        if (Children.Count > 0)
        {
            result += new string('\t', level) + "Children: ";
            foreach (KeyValuePair<SkeletalJoint_Base, RigidNode_Base> pair in Children)
            {
                result += System.Environment.NewLine + new string('\t', level) + " - " + pair.Key.ToString();
                result += System.Environment.NewLine + pair.Value.ToString();
                result += "\n";
            }
        }
        return result;
    }

    /// <summary>
    /// Gets a list of all the rigid nodes at or below this item in the tree.
    /// </summary>
    /// <returns>The list of nodes</returns>
    public List<RigidNode_Base> ListAllNodes()
    {
        List<RigidNode_Base> list = new List<RigidNode_Base>();
        ListAllNodes(list);
        return list;
    }

    /// <summary>
    /// Gets a list of all the rigid nodes at or below this item in the tree.
    /// </summary>
    /// <param name="list">The list to write the nodes to</param>
    public void ListAllNodes(List<RigidNode_Base> list)
    {
        list.Add(this);
        foreach (KeyValuePair<SkeletalJoint_Base, RigidNode_Base> pair in Children)
        {
            pair.Value.ListAllNodes(list);
        }
    }
}