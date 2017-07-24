using System;
using System.Collections.Generic;
using System.Text;
using GopherAPI.Other;
using GopherAPI.Nodes.Colliders;
using GopherAPI.Nodes.Joint;
using GopherAPI.STL;

namespace GopherAPI.Nodes
{
    /// <summary>
    /// Class used to populate the GopherField object from a RawField Format
    /// </summary>
    internal class FieldNodeGenerator
    {
        private RawField field = new RawField();

        /// <summary>
        /// Finds all of the nodes without a parent joint
        /// </summary>
        /// <returns></returns>
        private List<STLMesh> FindRoots()
        {
            var ret = new List<STLMesh>();
            foreach (var mesh in field.Meshes)
            {
                if (field.GetIsRoot(mesh.MeshID))
                    ret.Add(mesh);
            }
            return ret;
        }

        /// <summary>
        /// Populates the referenced node one level down
        /// </summary>
        /// <param name="node"></param>
        /// <returns>
        /// Returns true if the node is a 'leaf' (if it has no children)
        /// </returns>
        private bool PopulateNode(GopherFieldNode node)
        {
            if (node.Mesh == null)
                throw new NullReferenceException("ERROR: The Mesh member of node was null");
            var tempJoints = field.GetChildJoints(node.Mesh.MeshID);
            if(tempJoints.Count == 0)
            {
                node.IsLeaf = true;
                return true;
            }
            else
            {
                node.IsLeaf = false;
                foreach(var joint in tempJoints)
                {
                    node.AddChild(joint, new GopherFieldNode { Mesh = field.GetMesh(joint.Meta.ChildID) });
                }
            }
            return false;
        }

        /// <summary>
        /// Populates the node tree from roots to leaves
        /// </summary>
        /// <param name="nodes"></param>
        private bool PopulateNodes(List<GopherFieldNode> nodes, out List<GopherFieldNode> NextLevel)
        {
            NextLevel = new List<GopherFieldNode>();

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

        /// <summary>
        /// Generates a GopherField from a RawField class.
        /// </summary>
        /// <returns></returns>
        private GopherField GenerateField()
        {
            Gopher.ProgressCallback("Generating field node tree...");
            var rootMeshes = FindRoots();
            var rootNodes = new List<GopherFieldNode>();
            
            //Turn meshes into nodes
            foreach(var mesh in rootMeshes)
            {
                rootNodes.Add(new GopherFieldNode
                {
                    Mesh = mesh,
                    Level = 0
                });
            }

            //Prepare GopherField object for population
            GopherField ret = new GopherField
            {
                RootNodes = rootNodes
            };

            //Population objects
            List<GopherFieldNode> ThisLevel = ret.RootNodes;
            List<GopherFieldNode> NextLevel = new List<GopherFieldNode>();
            bool DoContinue = true;
            //Populating nodes
            do
            {
                DoContinue = PopulateNodes(ThisLevel, out NextLevel);
                ThisLevel = new List<GopherFieldNode>();
                ThisLevel = NextLevel;
                NextLevel = new List<GopherFieldNode>();
            }
            while (DoContinue);

            //Add colliders, generate top level collider list, and remove redundencies
            foreach (var node in ret.ListAllNodes())
            {
                if (node.Mesh.AttributeID == uint.MaxValue)
                    node.Collider = ret.Colliders[0];
                else
                {
                    var col = field.GetCollider(node.Mesh.AttributeID);
                    col.Nephews.Add(node);
                    node.Collider = col;
                    ret.Colliders.Add(col);
                }
            }
            Gopher.ProgressCallback("Generating field node tree...DONE");
            return ret;
        }

        internal delegate GopherField GopherFieldFactory(RawField raw);

        /// <summary>
        /// Generates a GopherField from a RawField
        /// </summary>
        internal static GopherFieldFactory FieldFactory = delegate (RawField raw)
        {
            return new FieldNodeGenerator
            {
                field = raw
            }.GenerateField();
        };
    }
}
