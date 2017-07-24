using System;
using System.Collections.Generic;
using System.Linq;
using GopherAPI.Nodes.Joint;


namespace GopherAPI.Nodes
{
    /// <summary>
    /// The node object used for GopherRobots.
    /// </summary>
    /// <remarks>
    /// So much casting...
    /// </remarks>
    public class GopherRobotNode : GopherNode_Base
    {
        /// <summary>
        /// Gets a bool that states whether or not this is a drive wheel
        /// </summary>
        /// <returns></returns>
        public bool IsDriveWheel()
        {
            try
            {
                if (GetParentJoint().Driver.GetIsDriveWheel())
                    return true;
            }
            catch (NullReferenceException)
            {
                return false;
            }
            return false;
        }

        private Dictionary<GopherJoint_Base, GopherRobotNode> children;

        /// <summary>
        /// Dictionary containing all of the children of this node
        /// </summary>
        public Dictionary<GopherJoint_Base, GopherRobotNode> Children;

        /// <summary>
        /// Adds the given node as a child of this node
        /// </summary>
        /// <param name="joint">The joint connecting this node to the child</param>
        /// <param name="child">The child node</param>
        public void AddChild(GopherJoint_Base joint, GopherRobotNode child)
        {
            children.Add(joint, child);
            child.parentConnection = joint;
            child.parent = this;
            child.Level = (ushort)(Level + 1);
            joint.Child = child;
            joint.Parent = parent;
        }

        /// <summary>
        /// A list of all the GopherJoints with this node as the parent
        /// </summary>
        /// <remarks>
        /// This list is generated from the Children object
        /// </remarks>
        public List<GopherJoint_Base> Joints => Children.Keys.ToList();

        /// <summary>
        /// Gets the parent node of this node. Returns null if this is a root node
        /// </summary>
        /// <returns></returns>
        public GopherRobotNode GetParent()
        {
            return (GopherRobotNode)parent;
        }

        /// <summary>
        /// Gets a list including this node and of the nodes below it (children, grandchildren, etc.)
        /// </summary>
        /// <returns>
        /// A list of all nodes at and below this node
        /// </returns>
        public List<GopherRobotNode> ListAllNodes()
        {
            var list = new List<GopherRobotNode>();
            list.Add(this);
            foreach (var child in Children.Values)
            {
                list.AddRange(child.ListAllNodes());
            }
            return list;
        }
    }
}