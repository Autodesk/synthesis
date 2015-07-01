using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditorsLibrary
{

    /// <summary>
    /// A control to edit .bxda files
    /// </summary>
    public partial class BXDAEditorPane : UserControl
    {

        /// <summary>
        /// The tree representation of the mesh node
        /// </summary>
        private TreeView meshTree;

        /// <summary>
        /// The node containing file version information
        /// </summary>
        private BXDAEditorNode versionNode;

        /// <summary>
        /// The node holding the tree representing the mesh
        /// </summary>
        private BXDAEditorNode meshNode;

        /// <summary>
        /// Create a new control and load a <see cref="System.Windows.Forms.TreeView"/> from a .bxda file
        /// </summary>
        /// <param name="meshPath">The path to the .bxda file</param>
        public BXDAEditorPane(string meshPath = null)
        {
            InitializeComponent();
            GenerateTree((meshPath != null) ? meshPath : BXDSettings.Instance.LastSkeletonDirectory + "node_0.bxda");
        }

        /// <summary>
        /// Generate the <see cref="System.Windows.Forms.TreeView"/>
        /// </summary>
        /// <param name="meshPath">The path to the .bxda file</param>
        private void GenerateTree(string meshPath)
        {
            BinaryReader reader = new BinaryReader(new FileStream(meshPath, FileMode.Open, FileAccess.Read));

            versionNode = new BXDAEditorNode(String.Format("Synthesis version {0}", BXDIO.VersionToString(reader.ReadUInt32())),
                                             BXDAEditorNode.NodeType.SECTION_HEADER);
            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            BXDAMesh mesh = new BXDAMesh();
            mesh.ReadData(reader);

            reader.Close();

            meshNode = new BXDAEditorNode(BXDAEditorNode.NodeType.MESH, mesh);

            BXDAEditorNode visualSubMeshSectionHeader = generateSubMeshTree(mesh.meshes);
            visualSubMeshSectionHeader.Name = "Visual Sub-meshes";

            BXDAEditorNode collisionSubMeshSectionHeader = generateSubMeshTree(mesh.colliders);
            collisionSubMeshSectionHeader.Name = "Collision Sub-meshes";

            BXDAEditorNode physicsSectionHeader = new BXDAEditorNode("Physical Properties", BXDAEditorNode.NodeType.SECTION_HEADER);
            meshNode.Nodes.Add(physicsSectionHeader);

            physicsSectionHeader.Nodes.Add(new BXDAEditorNode("Total Mass", BXDAEditorNode.NodeType.NUMBER, mesh.physics.mass));
            physicsSectionHeader.Nodes.Add(new BXDAEditorNode("Center of Mass", BXDAEditorNode.NodeType.VECTOR3,
                                                       mesh.physics.centerOfMass.x, mesh.physics.centerOfMass.y, mesh.physics.centerOfMass.z));

            meshTree = new TreeView();
            meshTree.Nodes.Add(versionNode);
            meshTree.Nodes.Add(meshNode);

            Controls.Add(meshTree);
        }

        /// <summary>
        /// Generate a tree of Sub-meshes from a list (Mostly for code niceness)
        /// </summary>
        /// <param name="subMeshes">A list of submeshes (Either visual or collision)</param>
        /// <returns>A blank node with the generated tree of Sub-meshes under it</returns>
        private BXDAEditorNode generateSubMeshTree(List<BXDAMesh.BXDASubMesh> subMeshes)
        {
            BXDAEditorNode subMeshSectionHeader = new BXDAEditorNode(BXDAEditorNode.NodeType.SECTION_HEADER);
            meshNode.Nodes.Add(subMeshSectionHeader);

            //Sub-meshes
            foreach (BXDAMesh.BXDASubMesh subMesh in subMeshes)
            {
                BXDAEditorNode subMeshNode = new BXDAEditorNode(BXDAEditorNode.NodeType.SUBMESH, subMesh);
                subMeshSectionHeader.Nodes.Add(subMeshNode);

                //Vertices
                BXDAEditorNode verticesSectionHeader = new BXDAEditorNode("Vertices", BXDAEditorNode.NodeType.SECTION_HEADER);
                subMeshNode.Nodes.Add(verticesSectionHeader);

                for (int vertIndex = 0; vertIndex < subMesh.verts.Length; vertIndex += 3)
                {
                    verticesSectionHeader.Nodes.Add(new BXDAEditorNode(BXDAEditorNode.NodeType.VECTOR3,
                                                    subMesh.verts[vertIndex], subMesh.verts[vertIndex + 1], subMesh.verts[vertIndex + 2]));
                }

                //Vertex Normals
                if (subMesh.norms != null)
                {
                    BXDAEditorNode normalsSectionHeader = new BXDAEditorNode("Surface Normals", BXDAEditorNode.NodeType.SECTION_HEADER);
                    subMeshNode.Nodes.Add(normalsSectionHeader);

                    for (int normIndex = 0; normIndex < subMesh.norms.Length; normIndex += 3)
                    {
                        normalsSectionHeader.Nodes.Add(new BXDAEditorNode(BXDAEditorNode.NodeType.VECTOR3,
                                                        subMesh.norms[normIndex], subMesh.norms[normIndex + 1], subMesh.norms[normIndex + 2]));
                    }
                }

                //Surfaces
                BXDAEditorNode surfacesSectionHeader = new BXDAEditorNode("Surfaces", BXDAEditorNode.NodeType.SECTION_HEADER);
                subMeshNode.Nodes.Add(surfacesSectionHeader);

                foreach (BXDAMesh.BXDASurface surface in subMesh.surfaces)
                {
                    BXDAEditorNode surfaceNode = new BXDAEditorNode(BXDAEditorNode.NodeType.SURFACE, surface);
                    surfacesSectionHeader.Nodes.Add(surfaceNode);

                    //Material Properties
                    BXDAEditorNode materialSectionHeader = new BXDAEditorNode("Material Properties", BXDAEditorNode.NodeType.SECTION_HEADER);
                    surfaceNode.Nodes.Add(materialSectionHeader);

                    if (surface.hasColor) materialSectionHeader.Nodes.Add(new BXDAEditorNode("Surface Color", BXDAEditorNode.NodeType.NUMBER,
                                                                          surface.color));
                    materialSectionHeader.Nodes.Add(new BXDAEditorNode("Transparency", BXDAEditorNode.NodeType.NUMBER, surface.transparency));
                    materialSectionHeader.Nodes.Add(new BXDAEditorNode("Translucency", BXDAEditorNode.NodeType.NUMBER, surface.translucency));
                    materialSectionHeader.Nodes.Add(new BXDAEditorNode("Specular Intensity", BXDAEditorNode.NodeType.NUMBER, surface.specular));

                    //Indices
                    BXDAEditorNode indicesSectionHeader = new BXDAEditorNode("Indices", BXDAEditorNode.NodeType.SECTION_HEADER);
                    surfaceNode.Nodes.Add(indicesSectionHeader);

                    for (int indexIndex = 0; indexIndex < surface.indicies.Length; indexIndex += 3)
                    {
                        indicesSectionHeader.Nodes.Add(new BXDAEditorNode(BXDAEditorNode.NodeType.VECTOR3,
                                            surface.indicies[indexIndex], surface.indicies[indexIndex + 1], surface.indicies[indexIndex + 2]));
                    }
                }
            }

            return subMeshSectionHeader;
        }

        /// <summary>
        /// A <see cref="System.Windows.Forms.TreeNode"/> with extra data loaded from .bxda
        /// </summary>
        private class BXDAEditorNode : TreeNode
        {

            public NodeType type;

            public NodeData data;

            /// <summary>
            /// Create a node without a specific name
            /// </summary>
            /// <param name="t">The type of node</param>
            /// <param name="dat">Extra data</param>
            public BXDAEditorNode(NodeType t, params Object[] dat)
                : base(t.ToString())
            {
                type = t;
                data = new NodeData(dat);
            }

            /// <summary>
            /// Create a node with a special name
            /// </summary>
            /// <param name="header">The node's name</param>
            /// <param name="t">The type of node</param>
            /// <param name="dat">Extra data</param>
            public BXDAEditorNode(string header, NodeType t, params Object[] dat)
                : base(header)
            {
                type = t;
                data = new NodeData(dat);
            }

            /// <summary>
            /// Turns number data to specific format strings
            /// </summary>
            /// <returns>The string representation of the node</returns>
            public override string ToString()
            {
                switch (type)
                {
                    case NodeType.VECTOR3:
                        return String.Format("<{0}, {1}, {2}>", data);
                    case NodeType.NUMBER:
                        return String.Format("{0}", data);
                    default:
                        return "";
                }
            }

            /// <summary>
            /// The enumeration for node type
            /// </summary>
            public enum NodeType
            {
                SECTION_HEADER, // No data (Blank node that can be used to group other nodes)
                MESH, // {BXDAMesh}
                SUBMESH, // {BXDAMesh.BXDASubMesh}
                SURFACE, // {BXDAMesh.BXDASurface}
                VECTOR3, // {float, float ,float}
                NUMBER // {byte or float}
            }

            /// <summary>
            /// The container for extra node data
            /// </summary>
            public class NodeData
            {

                /// <summary>
                /// Extra data
                /// </summary>
                private Object[] _data;

                /// <summary>
                /// The indexer
                /// </summary>
                /// <param name="i">Index</param>
                /// <returns>The data at the specified index</returns>
                public Object this[int i]
                {
                    get
                    {
                        return _data[i];
                    }
                    set
                    {
                        _data[i] = value;
                    }
                }

                /// <summary>
                /// Create a new NodeData object
                /// </summary>
                /// <param name="data">Extra data to store</param>
                public NodeData(Object[] data)
                {
                    _data = new Object[data.Length];
                    if (data.Length > 0) data.CopyTo(_data, 0);
                }

            }

        }

    }
}
