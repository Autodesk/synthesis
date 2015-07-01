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
        /// The root node in the tree
        /// </summary>
        private BXDAEditorNode rootNode;

        /// <summary>
        /// Create a new control and load a <see cref="System.Windows.Forms.TreeView"/> from a directory with .bxda files
        /// </summary>
        public BXDAEditorPane()
        {
            InitializeComponent();

            loadModel(BXDSettings.Instance.LastSkeletonDirectory);

            //BXDAEditorNode testNode = new BXDAEditorNode("TEST", BXDAEditorNode.NodeType.SECTION_HEADER);
            //treeView1.Nodes.Add(testNode);
        }

        public void loadModel(string modelPath)
        {
            List<String> fileNames = new List<String>(Directory.GetFiles(modelPath));
            var bxdaFiles = from file in fileNames
                            where file.Substring(file.Length - 4).Equals("bxda")
                            select file;

            if (bxdaFiles == null) throw new FileNotFoundException("Could not find .bxda files in specified directory");

            rootNode = new BXDAEditorNode("Model", BXDAEditorNode.NodeType.SECTION_HEADER);
            rootNode.Nodes.Add(new BXDAEditorNode("Version", BXDAEditorNode.NodeType.STRING, BXDIO.ASSEMBLY_VERSION));
            foreach (string fileName in bxdaFiles)
            {
                rootNode.Nodes.Add(GenerateTree(fileName));
            }

            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(rootNode);
        }

        /// <summary>
        /// Generates a tree from a bxda file
        /// </summary>
        /// <param name="meshPath">The path to the .bxda file</param>
        /// <returns>The root node of the mesh tree</returns>
        private BXDAEditorNode GenerateTree(string meshPath)
        {
            BinaryReader reader = new BinaryReader(new FileStream(meshPath, FileMode.Open, FileAccess.Read));

            BXDAMesh mesh = new BXDAMesh();
            mesh.ReadData(reader);

            reader.Close();

            BXDAEditorNode meshNode = new BXDAEditorNode(BXDAEditorNode.NodeType.MESH, mesh, meshPath);

            BXDAEditorNode visualSectionHeader = new BXDAEditorNode("Visual Sub-meshes", BXDAEditorNode.NodeType.NUMBER, mesh.meshes.Count);
            meshNode.Nodes.Add(visualSectionHeader);
            generateSubMeshTree(visualSectionHeader, mesh.meshes);

            BXDAEditorNode collisionSectionHeader = new BXDAEditorNode("Collision Sub-meshes", BXDAEditorNode.NodeType.NUMBER, mesh.colliders.Count);
            meshNode.Nodes.Add(collisionSectionHeader);
            generateSubMeshTree(collisionSectionHeader, mesh.colliders);

            BXDAEditorNode physicsSectionHeader = new BXDAEditorNode("Physical Properties", BXDAEditorNode.NodeType.SECTION_HEADER);
            meshNode.Nodes.Add(physicsSectionHeader);

            physicsSectionHeader.Nodes.Add(new BXDAEditorNode("Total Mass", BXDAEditorNode.NodeType.NUMBER, mesh.physics.mass));
            physicsSectionHeader.Nodes.Add(new BXDAEditorNode("Center of Mass", BXDAEditorNode.NodeType.VECTOR3,
                                                       mesh.physics.centerOfMass.x, mesh.physics.centerOfMass.y, mesh.physics.centerOfMass.z));

            return meshNode;
        }

        /// <summary>
        /// Generate a tree of Sub-meshes from a list (Mostly for code niceness)
        /// </summary>
        /// <param name="subMeshes">A list of submeshes (Either visual or collision)</param>
        /// <returns>A blank node with the generated tree of Sub-meshes under it</returns>
        private void generateSubMeshTree(BXDAEditorNode root, List<BXDAMesh.BXDASubMesh> subMeshes)
        {
            //Sub-meshes
            foreach (BXDAMesh.BXDASubMesh subMesh in subMeshes)
            {
                BXDAEditorNode subMeshNode = new BXDAEditorNode(BXDAEditorNode.NodeType.SUBMESH, subMesh);
                root.Nodes.Add(subMeshNode);

                //Vertices
                BXDAEditorNode verticesSectionHeader = new BXDAEditorNode("Vertices", BXDAEditorNode.NodeType.NUMBER, subMesh.verts.Length);
                subMeshNode.Nodes.Add(verticesSectionHeader);

                //Vertex Normals
                if (subMesh.norms != null)
                {
                    BXDAEditorNode normalsSectionHeader = new BXDAEditorNode("Surface Normals", BXDAEditorNode.NodeType.NUMBER, subMesh.norms.Length);
                    subMeshNode.Nodes.Add(normalsSectionHeader);
                }

                //Surfaces
                BXDAEditorNode surfacesSectionHeader = new BXDAEditorNode("Surfaces", BXDAEditorNode.NodeType.NUMBER, subMesh.surfaces.Count);
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
                    BXDAEditorNode indicesSectionHeader = new BXDAEditorNode("Indices", BXDAEditorNode.NodeType.NUMBER, surface.indicies.Length);
                    surfaceNode.Nodes.Add(indicesSectionHeader);
                }
            }
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
                SetText(t.ToString());
                Name = t.ToString();
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
                SetText(header);
                Name = header;
            }

            private void SetText(string textRoot)
            {
                Text = String.Format("{0}: {1}", textRoot, ToString());
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
                        return String.Format("<{0}, {1}, {2}>", data[0], data[1], data[2]);
                    case NodeType.NUMBER:
                        return String.Format("{0}", data[0]);
                    case NodeType.STRING:
                        return String.Format("{0}", data[0]);
                    case NodeType.MESH:
                        return String.Format("{0}", data[1]);
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
                MESH, // {BXDAMesh, string}
                SUBMESH, // {BXDAMesh.BXDASubMesh}
                SURFACE, // {BXDAMesh.BXDASurface}
                VECTOR3, // {float, float ,float}
                NUMBER, // {byte or float}
                STRING // {string}
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

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

    }
}
