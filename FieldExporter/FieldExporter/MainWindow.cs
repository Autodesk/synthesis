using Inventor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace FieldExporter
{
    public partial class MainWindow : Form
    {
        /// <summary>
        /// The Inventor application instance.
        /// </summary>
        Inventor.Application application;

        /// <summary>
        /// The ScanProgressWindow instance.
        /// </summary>
        ScanProgressWindow progressWindow;
        
        /// <summary>
        /// Connects to Inventor, constructs the form, and scans for visible documents.
        /// </summary>
        public MainWindow()
        {
            try
            {
                application = (Inventor.Application)Marshal.GetActiveObject("Inventor.Application");
            }
            catch
            {
                MessageBox.Show("Please launch Autodesk Inventor and try again.", "Could not connect to Autodesk Inventor.");
                System.Environment.Exit(1);
            }

            InitializeComponent();
        }

        /// <summary>
        /// Scans through all components in the specified collection and returns a TreeNode
        /// containing all of the corresponding Inventor components.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="parentNode"></param>
        /// <returns>The parent node</returns>
        private TreeNode ScanAssembly(ComponentOccurrences collection, TreeNode parentNode)
        {
            IEnumerator enumerator = collection.GetEnumerator();
            ComponentOccurrence occurrence;

            while (enumerator.MoveNext())
            {
                occurrence = (ComponentOccurrence)enumerator.Current;

                TreeNode currentNode = new TreeNode(occurrence.Name);
                currentNode.Name = currentNode.Text;

                parentNode.Nodes.Add(currentNode);

                ScanAssembly((ComponentOccurrences)occurrence.SubOccurrences, currentNode);
            }

            return parentNode;
        }

        /// <summary>
        /// Scans through the given TreeNode and returns a list containing its nodes.
        /// </summary>
        /// <param name="treeNode"></param>
        /// <returns></returns>
        private List<TreeNode> ScanTreeNode(TreeNode treeNode)
        {
            List<TreeNode> nodes = new List<TreeNode>();

            foreach (TreeNode n in treeNode.Nodes)
            {
                nodes.Add(n);
                nodes.AddRange(ScanTreeNode(n));
            }

            return nodes;
        }

        /// <summary>
        /// Scans through the given TreeView and returns a list containing its nodes.
        /// </summary>
        /// <param name="treeView"></param>
        /// <returns></returns>
        private List<TreeNode> ScanTreeView(TreeView treeView)
        {
            List<TreeNode> nodes = new List<TreeNode>();

            foreach (TreeNode n in treeView.Nodes)
            {
                nodes.Add(n);
                nodes.AddRange(ScanTreeNode(n));
            }

            return nodes;
        }

        /// <summary>
        /// Checks to see if there is an active document in Inventor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Activated(object sender, EventArgs e)
        {
            if (application.ActiveDocument != null)
            {
                Text = "Field Exporter - " + application.ActiveDocument.DisplayName;
                if (!DocumentScanner.IsBusy)
                    ScanButton.Enabled = true;
            }
            else
            {
                Text = "Field Exporter - No Document Found";
                DocumentView.Nodes.Clear();
                CollisionObjectsView.Nodes.Clear();
                ScanButton.Enabled = false;
            }
        }

        /// <summary>
        /// Initializes the Assembly scan process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScanButton_Click(object sender, EventArgs e)
        {
            ScanButton.Enabled = false;
            progressWindow = new ScanProgressWindow();
            progressWindow.Show(this);
            DocumentScanner.RunWorkerAsync((AssemblyDocument)application.ActiveDocument);
        }
        
        /// <summary>
        /// Scans the selected Assembly and creates a TreeNode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DocumentScanner_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                AssemblyDocument arg = (AssemblyDocument)e.Argument;
                e.Result = ScanAssembly(arg.ComponentDefinition.Occurrences, new TreeNode(arg.DisplayName));
            }
            catch
            {
                e.Result = null;
            }
        }

        /// <summary>
        /// Concludes the scanning process, updates the document view, and updates the
        /// collision objects view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DocumentScanner_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ScanButton.Enabled = true;

            progressWindow.Close();
            DocumentView.Nodes.Clear();

            if (e.Result != null)
            {
                DocumentView.Nodes.Add((TreeNode)e.Result);
                DocumentView.Nodes[0].Expand();
            }
            else
            {
                MessageBox.Show(this, "Lost connection with the assembly document.", "Document not found.");
                CollisionObjectsView.Nodes.Clear();
                return;
            }

            List<TreeNode> documentViewNodes = ScanTreeView(DocumentView);

            foreach (TreeNode n in ScanTreeView(CollisionObjectsView))
            {
                bool containsNode = false;

                foreach (TreeNode n2 in documentViewNodes)
                {
                    if (n.Name.Equals(n2.Name))
                    {
                        containsNode = true;
                        break;
                    }
                }

                if (!containsNode)
                {
                    CollisionObjectsView.Nodes.RemoveByKey(n.Name);
                }
            }
        }

        /// <summary>
        /// Initializes the drag event when an node is clicked and dragged.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DocumentView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            TreeNode treeNode = (TreeNode)e.Item;
            DoDragDrop(treeNode.Clone(), DragDropEffects.Copy);
        }

        /// <summary>
        /// Adds the node to the CollisionObjectsView if it doesn't exist and removes any child duplicates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollisionObjectsView_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode treeNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            foreach (TreeNode n in ScanTreeView(CollisionObjectsView))
            {
                if (n.Name.Equals(treeNode.Name))
                {
                    MessageBox.Show("Component has already been added.", "A conflict has occurred.");
                    return;
                }
            }

            foreach (TreeNode n in ScanTreeNode(treeNode))
            {
                foreach (TreeNode n2 in ScanTreeView(CollisionObjectsView))
                {
                    if (n2.Name.Equals(n.Name))
                    {
                        CollisionObjectsView.Nodes.RemoveByKey(n2.Name);
                    }
                }
            }

            CollisionObjectsView.Nodes.Add(treeNode);
        }

        /// <summary>
        /// Changes the mouse icon to show that the dragged item can be dropped.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollisionObjectsView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        /// <summary>
        /// Allows the user to delete items with the [delete] key.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollisionObjectsView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
            {
                if (CollisionObjectsView.SelectedNode != null)
                {
                    CollisionObjectsView.Nodes.Remove(CollisionObjectsView.SelectedNode);
                }
            }
        }
    }
}
