using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GopherAPI.STL;

using GopherAPI.Other;
using GopherAPI.Nodes.Joint;
using GopherAPI.Nodes.Colliders;

namespace GopherAPI.Nodes
{
    /// <summary>
    /// The node object used for GopherRobots.
    /// </summary>
    /// <remarks>
    /// So much casting...
    /// </remarks>
    public class GopherFieldNode : GopherNode_Base
    {
        /// <summary>
        /// Dictionary containing all of the children of this node
        /// </summary>
        public Dictionary<GopherJoint_Base, GopherFieldNode> Children;

        /// <summary>
        /// Adds the given node as a child of this node
        /// </summary>
        /// <param name="joint">The joint connecting this node to the child</param>
        /// <param name="child">The child node</param>
        public void AddChild(GopherJoint_Base joint, GopherFieldNode child)
        {
            Children.Add(joint, child);
            child.parentConnection = joint;
            child.parent = this;
            joint.Child = child;
            joint.Parent = parent;
            child.Level = (ushort)(Level + 1);
        }

        /// <summary>
        /// Gets the parent node of the field
        /// </summary>
        /// <returns>The parent node of the field or null if it is a root node</returns>
        public GopherFieldNode GetParent()
        {
            return (GopherFieldNode)parent;
        }

        /// <summary>
        /// The collider object for the node.
        /// </summary>
        /// <remarks>
        /// If there is no collider, Collider will return a 'NoDriver' object.
        /// </remarks>
        public GopherCollider_Base Collider { get; internal set; }

        /// <summary>
        /// A list of all the GopherJoints with this node as the parent
        /// </summary>
        /// <remarks>
        /// This list is generated from the Children object
        /// </remarks>
        public List<GopherJoint_Base> Joints => Children.Keys.ToList();

        /// <summary>
        /// A list containing all of the elements attached to the Collider object (aka, all the elements in the attribute group) 
        /// </summary>
        /// <remarks>
        /// If there is no collider, Cousins will return the 'Nephews' object of the 'NoDriver' object. This is a list that contains all the nodes without colliders
        /// </remarks>
        public List<GopherFieldNode> Cousins => Collider.Nephews;

        /// <summary>
        /// Gets a list including this node and of the nodes below it (children, grandchildren, etc.)
        /// </summary>
        /// <returns>
        /// A list of all nodes at and below this node
        /// </returns>
        public List<GopherFieldNode> ListAllNodes()
        {
            var list = new List<GopherFieldNode>();
            list.Add(this);
            foreach(var child in Children.Values)
            {
                list.AddRange(child.ListAllNodes());
            }
            return list;
        }
    }
}
