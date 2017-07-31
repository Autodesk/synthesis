using System.Collections.Generic;
using GopherAPI.Nodes.Colliders;

namespace GopherAPI.Nodes
{
    public class GopherField : GopherNodeTree_Base
    {
        /// <summary>
        /// A list containing all of the colliders. The List is initialized with an empty NoCollider object which will be referenced for all of the nodes without colliders
        /// </summary>
        public List<GopherCollider_Base> Colliders { get; internal set; } = new List<GopherCollider_Base> { new NoCollider() };

        /// <summary>
        /// Gets a list of all of the nodes in the node tree
        /// </summary>
        /// <param name="list">the list to add all of the nodes to</param>
        public void ListAllNodes(List<GopherFieldNode> list)
        {
            foreach(var node in RootNodes)
            {
                list.AddRange(node.ListAllNodes());
            }
        }

        /// <summary>
        /// Gets a list of all the nodes in the tree
        /// </summary>
        /// <returns>
        /// A list of all the nodes in the node tree
        /// </returns>
        public List<GopherFieldNode> ListAllNodes()
        {
            List<GopherFieldNode> ret = new List<GopherFieldNode>();
            ListAllNodes(ret);
            return ret;
        }

        /// <summary>
        /// A list containing all the root nodes of the node tree
        /// </summary>
        /// <remarks>
        /// Though you can use the 'Add' method, it won't do anything as this returns a copy of GopherNodeTree_Base.rootNodes. The proper way to add root nodes is at creation.
        /// </remarks>
        public List<GopherFieldNode> RootNodes
        {
            get
            {
                return rootNodes.ConvertAll(x => (GopherFieldNode)x);
            }
            internal set
            {
                rootNodes = value.ConvertAll(x => (GopherNode_Base)x);
            }
        }
    }
}
