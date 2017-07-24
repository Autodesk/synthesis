using System;
using System.Collections.Generic;
using System.Linq;
using GopherAPI.STL;

namespace GopherAPI.Nodes
{
    internal class RobotNodeGenerator
    {
        private RawRobot robot = new RawRobot();

        /// <summary>
        /// Finds all the root nodes of the robot
        /// </summary>
        /// <returns></returns>
        private List<STLMesh> FindRoots()
        {
            var ret = new List<STLMesh>();
            foreach (var mesh in robot.Meshes)
            {
                if (robot.GetIsRoot(mesh.MeshID))
                    ret.Add(mesh);
            }
            return ret;
        }

        /// <summary>
        /// Populates the given node one level down
        /// </summary>
        /// <param name="node"></param>
        /// <returns>
        /// Returns true if the node is a 'leaf' (if it has no children)
        /// </returns>
        private bool PopulateNode(GopherRobotNode node)
        {
            if (node.Mesh == null)
                throw new NullReferenceException("ERROR: The Mesh member of node was null");
            var tempJoints = robot.GetChildJoints(node.Mesh.MeshID);
            if (tempJoints.Count == 0)
            {
                node.IsLeaf = true;
                return true;
            }
            else
            {
                node.IsLeaf = false;
                foreach (var joint in tempJoints)
                {
                    node.AddChild(joint, new GopherRobotNode { Mesh = robot.GetMesh(joint.Meta.ChildID) });
                }
            }
            return false;
        }

        /// <summary>
        /// Populates the node tree from roots to leaves
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="NextLevel">the next list that will be passed to Populate nodes</param>
        private bool PopulateNodes(List<GopherRobotNode> nodes, out List<GopherRobotNode> NextLevel)
        {
            NextLevel = new List<GopherRobotNode>();

            for (int i = 0; i < nodes.Count; i++)
            {
                PopulateNode(nodes[i]);
                NextLevel.AddRange(nodes[i].Children.Values);
            }

            if (NextLevel.Count == 0)
                return false;
            else
                return true;
        }

        private GopherRobot GenerateRobot()
        {
            Gopher.ProgressCallback("Generating robot node tree...");
            var rootMeshes = FindRoots();
            var rootNodes = new List<GopherRobotNode>();
            
            //Turn meshes into nodes
            foreach(var mesh in rootMeshes)
            {
                rootNodes.Add(new GopherRobotNode
                {
                    Mesh = mesh,
                    Level = 0
                });
            }

            //Prepare GopherRobot object for population
            var ret = new GopherRobot
            {
                RootNodes = rootNodes
            };

            //Population objects
            List<GopherRobotNode> ThisLevel = ret.RootNodes;
            List<GopherRobotNode> NextLevel = new List<GopherRobotNode>();
            bool DoContinue = true;
            //Populating nodes
            do
            {
                DoContinue = PopulateNodes(ThisLevel, out NextLevel);
                ThisLevel = new List<GopherRobotNode>();
                ThisLevel = NextLevel;
                NextLevel = new List<GopherRobotNode>();
            }
            while (DoContinue);
            Gopher.ProgressCallback("Generating robot node tree...DONE");
            return ret;
        }

        internal delegate GopherRobot GopherRobotFactory(RawRobot raw);

        /// <summary>
        /// Generates a GopherRobot from a RawRobot
        /// </summary>
        internal static GopherRobotFactory RobotFactory = delegate (RawRobot raw)
        {
            return new RobotNodeGenerator
            {
                robot = raw
            }.GenerateRobot();
        };

    }
}
