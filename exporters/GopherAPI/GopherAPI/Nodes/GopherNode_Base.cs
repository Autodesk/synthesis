using System.Collections.Generic;
using System.Text;
using GopherAPI.STL;

using GopherAPI.Other;
using GopherAPI.Nodes.Joint;


namespace GopherAPI.Nodes
{
    /// <summary>
    /// The base class from which GopherRobotNode and GopherFieldNode all derive from. Its probably best not to touch this as most of the methods are only implemented in the derived classes.
    /// </summary>
    public class GopherNode_Base
    {
        /// <summary>
        /// The mesh that Unity will render.
        /// </summary>
        public STLMesh Mesh { get; internal set; }

        /// <summary>
        /// True if the node is a leaf (if it has no children)
        /// </summary>
        public bool IsLeaf { get; internal set; } = false;

        /// <summary>
        /// The level of the node in the tree.
        /// </summary>
        /// <remarks>
        /// For root nodes, this value will be zero
        /// </remarks>
        public ushort Level { get; set; }

        protected GopherNode_Base parent = null;
        protected GopherJoint_Base parentConnection = null;

        /// <summary>
        /// Gets the parent node
        /// </summary>
        /// <remarks>
        /// This returns null if the node is a root node
        /// </remarks>
        /// <returns>The parent node</returns>
        protected GopherNode_Base GetParent_Base()
        {
            return parent;
        }

        /// <summary>
        /// Gets the parent joint
        /// </summary>
        /// <remarks>
        /// This returns null if this node is a root node
        /// </remarks>
        /// <returns>The parent Joint</returns>
        public GopherJoint_Base GetParentJoint()
        {
            return parentConnection;
        }
    }
}