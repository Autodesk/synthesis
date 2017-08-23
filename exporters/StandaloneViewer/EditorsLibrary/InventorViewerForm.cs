using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StandaloneViewer
{
    public partial class InventorViewerForm : Form
    {
        public InventorViewerForm()
        {
            InitializeComponent();

            Shown += StandaloneViewerForm_Shown;
            robotViewer1.NodeSelected += RobotViewer1_NodeSelected;
        }

        private void RobotViewer1_NodeSelected(RigidNode_Base node, bool clearExisting)
        {

        }

        private void StandaloneViewerForm_Shown(object sender, EventArgs e)
        {
            RigidNode_Base node = BXDJSkeleton.ReadSkeleton(LaunchParams.Path + @"\skeleton.bxdj");

            List<RigidNode_Base> nodes = node.ListAllNodes();

            List<BXDAMesh> meshes = new List<BXDAMesh>();

            foreach (RigidNode_Base n in nodes)
            {
                BXDAMesh mesh = new BXDAMesh();
                mesh.ReadFromFile(LaunchParams.Path + "\\" + n.ModelFileName);

                if (!n.GUID.Equals(mesh.GUID))
                {
                    MessageBox.Show(n.ModelFileName + " has been modified.", "Could not load mesh.");
                }
                meshes.Add(mesh);
            }
            robotViewer1.LoadModel(node, meshes);
            robotViewer1.FixLimits();
            robotViewer1.HighlightAll();
        }
    }
}
