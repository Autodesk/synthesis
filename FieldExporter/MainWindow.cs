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
        Inventor.Application inventorApplication;
        Dictionary<string, AssemblyDocument> documents = new Dictionary<string, AssemblyDocument>();
        ScanProgressWindow scanProgressWindow;
        
        /// <summary>
        /// Connects to Inventor, constructs the form, and scans for visible documents.
        /// </summary>
        public MainWindow()
        {
            try
            {
                inventorApplication = (Inventor.Application)Marshal.GetActiveObject("Inventor.Application");
            }
            catch
            {
                MessageBox.Show("Please launch Autodesk Inventor and try again.", "Could not connect to Autodesk Inventor.");
                System.Environment.Exit(1);
            }

            InitializeComponent();

            RefreshVisibleDocuments();
        }

        /// <summary>
        /// Checks for open assembly documents in Inventor.
        /// </summary>
        private void RefreshVisibleDocuments()
        {
            DocumentList.BeginUpdate();
            documents.Clear();

            foreach (AssemblyDocument doc in inventorApplication.Documents.VisibleDocuments)
            {
                documents.Add(doc.DisplayName, doc);
            }

            DocumentList.DataSource = documents.Count > 0 ? new BindingSource(documents, null) : null;
            DocumentList.DisplayMember = "Key";
            DocumentList.ValueMember = "Value";

            DocumentList.EndUpdate();
        }

        /// <summary>
        /// Iterates through all components in the specified collection and returns a TreeNode
        /// containing all of the corresponding Inventor components.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        private TreeNode IterateThroughAssembly(ComponentOccurrences collection, TreeNode parentNode)
        {
            IEnumerator enumerator = collection.GetEnumerator();
            ComponentOccurrence occurrence;

            while (enumerator.MoveNext())
            {
                occurrence = (ComponentOccurrence) enumerator.Current;

                TreeNode currentNode = new TreeNode(occurrence.Name);

                parentNode.Nodes.Add(currentNode);
                
                IterateThroughAssembly((ComponentOccurrences) occurrence.SubOccurrences, currentNode);
            }

            return parentNode;
        }

        /// <summary>
        /// Refreshes the visible documents.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshButton_Click(object sender, EventArgs e)
        {
            RefreshVisibleDocuments();
        }

        /// <summary>
        /// Initializes the Assembly scan process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScanButton_Click(object sender, EventArgs e)
        {
            DocumentView.Nodes.Clear();
            try
            {
                KeyValuePair<string, AssemblyDocument> selectedItem = (KeyValuePair<string, AssemblyDocument>)DocumentList.SelectedItem;
                scanProgressWindow = new ScanProgressWindow();
                scanProgressWindow.Show(this);
                DocumentScanner.RunWorkerAsync(selectedItem);
            }
            catch
            {
                MessageBox.Show("Please select a document.", "No document selected.");
            }
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
                KeyValuePair<string, AssemblyDocument> arg = (KeyValuePair<string, AssemblyDocument>)e.Argument;
                e.Result = IterateThroughAssembly(arg.Value.ComponentDefinition.Occurrences, new TreeNode(arg.Key));
            }
            catch
            {
                e.Result = null;
            }
        }

        /// <summary>
        /// Concludes the scanning process and updates the document view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DocumentScanner_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            scanProgressWindow.Close();
            if (e.Result != null)
            {
                DocumentView.Nodes.Add((TreeNode)e.Result);
            }
            else
            {
                RefreshVisibleDocuments();
                MessageBox.Show("Make sure the document is open in Inventor.", "Document not found.");
            }
        }

    }
}
