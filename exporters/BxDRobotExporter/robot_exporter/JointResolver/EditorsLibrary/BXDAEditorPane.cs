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

        public delegate void BXDAEditorEvent(BXDAMesh mesh);

        public event BXDAEditorEvent NodeSelected;

        /// <summary>
        /// The root node in the tree
        /// </summary>
        public BXDAEditorNode rootNode;

        private string _units = "lb";
        public string Units
        {
            get
            {
                return _units;
            }
            set
            {
                if (rootNode != null)
                {
                    foreach (BXDAEditorNode meshNode in rootNode.Nodes)
                    {
                        if (meshNode.type == BXDAEditorNode.NodeType.MESH)
                        {
                            meshNode.Nodes[2].Text = String.Format("Physical Properties ({0}, cm)", value);

                            if (value.Equals("lb"))
                            {
                                (meshNode.Nodes[2].Nodes[0] as BXDAEditorNode).data[0] = (meshNode.data[0] as BXDAMesh).physics.mass * 2.205f;
                            }
                            else
                            {
                                (meshNode.Nodes[2].Nodes[0] as BXDAEditorNode).data[0] = (meshNode.data[0] as BXDAMesh).physics.mass;
                            }

                            (meshNode.Nodes[2].Nodes[0] as BXDAEditorNode).updateName();
                        }
                    }
                }

                _units = (value != null) ? value : "lb";
            }
        }

        public int NodeCount
        {
            get
            {
                return rootNode.Nodes.Count - 1;
            }
        }

        /// <summary>
        /// Create a new control and load a <see cref="System.Windows.Forms.TreeView"/> from a directory with .bxda files
        /// </summary>
        public BXDAEditorPane()
        {
            InitializeComponent();
            
            treeView1.NodeMouseClick += treeView1_NodeMouseClick;
        }

        public void loadModel(List<BXDAMesh> meshes)
        {
            treeView1.Nodes.Clear();

            if (meshes == null) return;

            rootNode = new BXDAEditorNode("Model", BXDAEditorNode.NodeType.SECTION_HEADER, false);
            rootNode.Nodes.Add(new BXDAEditorNode("Exported with version", BXDAEditorNode.NodeType.STRING, false, BXDIO.ASSEMBLY_VERSION));
            foreach (BXDAMesh mesh in meshes)
            {
                rootNode.Nodes.Add(GenerateTree(mesh));
            }

            treeView1.Nodes.Add(rootNode);
        }

        public void AddSelection(RigidNode_Base node, bool clearExisting)
        {
            if (rootNode == null) return;

            foreach (BXDAEditorNode treeNode in rootNode.Nodes)
            {
                treeNode.BackColor = Color.White;
            }

            if (node == null)
            {
                treeView1.SelectedNode = null;
                return;
            }

            List<char> IndexChars = new List<char>();
            foreach (char c in node.ModelFileName)
            {
                if (int.TryParse(c.ToString(), out int i))
                    IndexChars.Add(c);
            }
            treeView1.SelectedNode = treeView1.Nodes[0].Nodes[int.Parse(new string(IndexChars.ToArray()))];
        }

        public void SelectJoints(List<RigidNode_Base> nodes)
        {
            if (nodes == null || nodes.Count == 0) AddSelection(null, true);
            else AddSelection(nodes[0], true);
        }

        protected void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            AddSelection(null, true);

            if (((BXDAEditorNode)e.Node).type != BXDAEditorNode.NodeType.MESH) return;

            e.Node.BackColor = Color.LightSteelBlue;

            NodeSelected(((BXDAEditorNode)e.Node).data[0] as BXDAMesh);
        }

        /// <summary>
        /// Generates a tree from a bxda file
        /// </summary>
        /// <param name="meshPath">The path to the .bxda file</param>
        /// <returns>The root node of the mesh tree</returns>
        private BXDAEditorNode GenerateTree(BXDAMesh mesh)
        {
            BXDAEditorNode meshNode = new BXDAEditorNode(BXDAEditorNode.NodeType.MESH, false, mesh);

            BXDAEditorNode visualSectionHeader = new BXDAEditorNode("Visual Sub-meshes", BXDAEditorNode.NodeType.INTEGER, false,
                                                                    mesh.meshes.Count);
            meshNode.Nodes.Add(visualSectionHeader);
            generateSubMeshTree(visualSectionHeader, mesh.meshes);

            BXDAEditorNode collisionSectionHeader = new BXDAEditorNode("Collision Sub-meshes", BXDAEditorNode.NodeType.INTEGER, false, 
                                                                       mesh.colliders.Count);
            meshNode.Nodes.Add(collisionSectionHeader);
            generateSubMeshTree(collisionSectionHeader, mesh.colliders);

            BXDAEditorNode physicsSectionHeader = new BXDAEditorNode(String.Format("Physical Properties ({0}, cm)", _units), 
                                                                     BXDAEditorNode.NodeType.SECTION_HEADER, false);
            meshNode.Nodes.Add(physicsSectionHeader);

            physicsSectionHeader.Nodes.Add(new BXDAEditorNode("Total Mass", BXDAEditorNode.NodeType.FLOAT, true, 
                                                              _units.Equals("lb") ? mesh.physics.mass * 2.205f : mesh.physics.mass));
            physicsSectionHeader.Nodes.Add(new BXDAEditorNode("Center of Mass", BXDAEditorNode.NodeType.VECTOR3, true,
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
                BXDAEditorNode subMeshNode = new BXDAEditorNode(BXDAEditorNode.NodeType.SUBMESH, false, subMesh);
                root.Nodes.Add(subMeshNode);

                //Vertices
                BXDAEditorNode verticesSectionHeader = new BXDAEditorNode("Vertices", BXDAEditorNode.NodeType.INTEGER, false,
                                                                          subMesh.verts.Length);
                subMeshNode.Nodes.Add(verticesSectionHeader);
                
                //Vertex Normals
                if (subMesh.norms != null)
                {
                    BXDAEditorNode normalsSectionHeader = new BXDAEditorNode("Surface Normals", BXDAEditorNode.NodeType.INTEGER, false,
                                                                             subMesh.norms.Length);
                    subMeshNode.Nodes.Add(normalsSectionHeader);
                }

                //Surfaces
                BXDAEditorNode surfacesSectionHeader = new BXDAEditorNode("Surfaces", BXDAEditorNode.NodeType.INTEGER, false,
                                                                          subMesh.surfaces.Count);
                subMeshNode.Nodes.Add(surfacesSectionHeader);

                foreach (BXDAMesh.BXDASurface surface in subMesh.surfaces)
                {
                    BXDAEditorNode surfaceNode = new BXDAEditorNode(BXDAEditorNode.NodeType.SURFACE, false, surface);
                    surfacesSectionHeader.Nodes.Add(surfaceNode);

                    //Material Properties
                    BXDAEditorNode materialSectionHeader = new BXDAEditorNode("Material Properties", BXDAEditorNode.NodeType.SECTION_HEADER,
                                                                              false);
                    surfaceNode.Nodes.Add(materialSectionHeader);

                    materialSectionHeader.Nodes.Add(new BXDAEditorNode("Color", BXDAEditorNode.NodeType.COLOR,
                                                                          true, surface.color, surface.hasColor));
                    materialSectionHeader.Nodes.Add(new BXDAEditorNode("Transparency", BXDAEditorNode.NodeType.FLOAT, true,
                                                                       surface.transparency));
                    materialSectionHeader.Nodes.Add(new BXDAEditorNode("Translucency", BXDAEditorNode.NodeType.FLOAT, true,
                                                                       surface.translucency));
                    materialSectionHeader.Nodes.Add(new BXDAEditorNode("Specular Intensity", BXDAEditorNode.NodeType.FLOAT, true,
                                                                       surface.specular));

                    //Indices
                    BXDAEditorNode indicesSectionHeader = new BXDAEditorNode("Indices", BXDAEditorNode.NodeType.INTEGER, false,
                                                                             surface.indicies.Length);
                    surfaceNode.Nodes.Add(indicesSectionHeader);
                }
            }
        }

        private void reloadModel()
        {
            var meshNodes = from BXDAEditorNode node in rootNode.Nodes
                            where node.type == BXDAEditorNode.NodeType.MESH
                            select node;

            foreach (BXDAEditorNode node in meshNodes)
            {
                reloadMesh(node);
            }
        }

        private void reloadMesh(BXDAEditorNode meshNode)
        {
            foreach (BXDAEditorNode visualSubMeshNode in meshNode.Nodes[0].Nodes) //Visual Sub-meshes
            {
                reloadSubMesh(visualSubMeshNode);
            }

            foreach (BXDAEditorNode collisionSubMeshNode in meshNode.Nodes[1].Nodes) //Collision Sub-meshes
            {
                reloadSubMesh(collisionSubMeshNode);
            }

            BXDAMesh mesh = (BXDAMesh) meshNode.data[0];
            
            //Physical properties
            float mass = (float)((BXDAEditorNode)meshNode.Nodes[2].Nodes[0]).data[0];
            if (_units.Equals("lb")) mesh.physics.mass = mass / 2.205f;
            else mesh.physics.mass = mass;
            mesh.physics.centerOfMass.x = (float) ((BXDAEditorNode) meshNode.Nodes[2].Nodes[1]).data[0];
            mesh.physics.centerOfMass.y = (float) ((BXDAEditorNode) meshNode.Nodes[2].Nodes[1]).data[1];
            mesh.physics.centerOfMass.z = (float) ((BXDAEditorNode) meshNode.Nodes[2].Nodes[1]).data[2];
        }

        private void reloadSubMesh(BXDAEditorNode subMeshNode)
        {
            foreach (BXDAEditorNode surfaceNode in subMeshNode.LastNode.Nodes)
            {
                BXDAMesh.BXDASurface surface = (BXDAMesh.BXDASurface) surfaceNode.data[0];
                BXDAEditorNode materialNode = (BXDAEditorNode) surfaceNode.Nodes[0];

                surface.hasColor = (bool) ((BXDAEditorNode) materialNode.Nodes[0]).data[1];
                surface.color = (uint) ((BXDAEditorNode) materialNode.Nodes[0]).data[0];
                surface.transparency = (float) ((BXDAEditorNode) materialNode.Nodes[1]).data[0];
                surface.translucency = (float) ((BXDAEditorNode) materialNode.Nodes[2]).data[0];
                surface.specular = (float) ((BXDAEditorNode) materialNode.Nodes[3]).data[0];
            }
        }
        
        /// <summary>
        /// Occurs after a node is double clicked
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            BXDAEditorNode selectedNode = (BXDAEditorNode) treeView1.SelectedNode;
            
            if (selectedNode != null && selectedNode.isEditable)
            {
                BXDAEditorForm editForm = new BXDAEditorForm(selectedNode);
                if (editForm.ShowDialog(this) == DialogResult.OK)
                {
                    reloadModel();
                }
            }
        }
        
        /// <summary>
        /// A <see cref="System.Windows.Forms.TreeNode"/> with extra data loaded from .bxda
        /// </summary>
        public class BXDAEditorNode : TreeNode
        {

            public NodeType type;

            public bool isEditable;

            public NodeData data;

            /// <summary>
            /// Create a node without a specific name
            /// </summary>
            /// <param name="t">The type of node</param>
            /// <param name="dat">Extra data</param>
            public BXDAEditorNode(NodeType t, bool editable, params Object[] dat)
                : base(t.ToString())
            {
                type = t;
                isEditable = editable;
                if (isEditable) BackColor = Color.LightGray;
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
            public BXDAEditorNode(string header, NodeType t, bool editable, params Object[] dat)
                : base(header)
            {
                type = t;
                isEditable = editable;
                if (isEditable) BackColor = Color.LightGray;
                data = new NodeData(dat);
                SetText(header);
                Name = header;
            }

            public void updateName()
            {
                SetText(Text.Substring(0, Text.LastIndexOf(":")));
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
                    case NodeType.COLOR:
                        uint color = (uint) data[0];
                        if (!((bool)data[1])) return String.Format("{0}", false);
                        else
                        {
                            string colorString = String.Format("{0:X}", color);
                            return "#" + new string('0', 8 - colorString.Length) + colorString;
                        }
                    case NodeType.INTEGER:
                    case NodeType.FLOAT:
                    case NodeType.STRING:
                        return String.Format("{0}", data[0]);
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
                VECTOR3, // {float, float, float}
                COLOR, // {uint, bool}
                INTEGER, // {int}
                FLOAT, // {float}
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

    }
}
