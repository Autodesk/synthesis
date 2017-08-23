using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StandaloneViewer
{
    public partial class StandaloneViewerForm : Form
    {
        public StandaloneViewerForm()
        {
            InitializeComponent();

            Size = LaunchParams.WindowSize;

            Shown += StandaloneViewerForm_Shown;
            robotViewer1.NodeSelected += RobotViewer1_NodeSelected;
            robotViewer1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            robotViewer1.AutoSize = true;
            Resize += StandaloneViewerForm_Resize;
        }

        private void StandaloneViewerForm_Resize(object sender, EventArgs e)
        {
            robotViewer1.SELECT_BUFFER_WIDTH = Size.Width;
            robotViewer1.SELECT_BUFFER_HEIGHT = Size.Height;
        }

        private void RobotViewer1_NodeSelected(RigidNode_Base node, bool clearExisting)
        {

        }

        private void StandaloneViewerForm_Shown(object sender, EventArgs e)
        {
            Text = "Robot Viewer";
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            robotViewer1 = new RobotViewer();
            FolderBrowserDialog browser = new FolderBrowserDialog();
            if (browser.ShowDialog() == DialogResult.OK && browser.SelectedPath != null)
            {
                RigidNode_Base node = BXDJSkeleton.ReadSkeleton(browser.SelectedPath + @"\skeleton.bxdj");

                List<RigidNode_Base> nodes = node.ListAllNodes();

                List<BXDAMesh> meshes = new List<BXDAMesh>();

                foreach (RigidNode_Base n in nodes)
                {
                    BXDAMesh mesh = new BXDAMesh();
                    mesh.ReadFromFile(browser.SelectedPath + "\\" + n.ModelFileName);

                    if (!n.GUID.Equals(mesh.GUID))
                    {
                        MessageBox.Show(n.ModelFileName + " has been modified.", "Could not load mesh.");
                    }
                    meshes.Add(mesh);
                }
                robotViewer1.LoadModel(node, meshes);
                robotViewer1.FixLimits();
                robotViewer1.HighlightAll();

                Text = "Robot Viewer: " + browser.SelectedPath.Split(new char[] { '\\' }, System.StringSplitOptions.RemoveEmptyEntries)[browser.SelectedPath.Split(new char[] { '\\' }, System.StringSplitOptions.RemoveEmptyEntries).Length - 1];
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tutorialsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://bxd.autodesk.com/tutorials.html");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutDialog().ShowDialog();
        }
    }
}
