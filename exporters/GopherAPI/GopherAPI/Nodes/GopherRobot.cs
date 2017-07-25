using System.Collections.Generic;
using GopherAPI.Nodes.Joint;
using GopherAPI.Nodes.Joint.Driver;

namespace GopherAPI.Nodes
{
    /// <summary>
    /// A fully noded robot model. This is the class that will be read by Unity
    /// </summary>
    public class GopherRobot : GopherNodeTree_Base
    {
        /// <summary>
        /// A list containing all the root nodes of the node tree
        /// </summary>
        /// <remarks>
        /// Though you can use the 'Add' method, it won't do anything as this returns a copy of GopherNodeTree_Base.rootNodes. The proper way to add root nodes is at creation.
        /// </remarks>
        public List<GopherRobotNode> RootNodes
        {
            get
            {
                return rootNodes.ConvertAll(x => (GopherRobotNode)x);
            }
            internal set
            {
                rootNodes = value.ConvertAll(x => (GopherNode_Base)x);
            }
        }

        /// <summary>
        /// Gets a list of all of the nodes in the node tree
        /// </summary>
        /// <param name="list">the list to add all of the nodes to</param>
        public void ListAllNodes(List<GopherRobotNode> list)
        {
            foreach (var node in RootNodes)
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
        public List<GopherRobotNode> ListAllNodes()
        {
            List<GopherRobotNode> ret = new List<GopherRobotNode>();
            ListAllNodes(ret);
            return ret;
        }
    }
}
