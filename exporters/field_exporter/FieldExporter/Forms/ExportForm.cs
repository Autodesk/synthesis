using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Inventor;

namespace FieldExporter.Components
{
    public partial class ExportForm : Form
    {
        public string path;

        /// <summary>
        /// Used for determining if the exporter is running.
        /// </summary>
        public bool IsExporting
        {
            get
            {
                return exporter.IsBusy;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ExportForm class.
        /// </summary>
        /// <param name="parent"></param>
        public ExportForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Launches the file browser dialog window and updates the file path text box text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browseButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                filePathTextBox.Text = folderBrowserDialog.SelectedPath;
                exportButton.Enabled = true;
                statusLabel.Text = "Ready to export.";
            }
        }

        /// <summary>
        /// Starts the export process when the "Export" button is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void exportButton_Click(object sender, EventArgs e)
        {
            if (exportButton.Text.Equals("Export") && !exporter.IsBusy)
            {
                exportButton.Text = "Cancel";
                browseButton.Enabled = false;

                Program.LockInventor();

                exporter.RunWorkerAsync();
            }
            else if (exportButton.Text.Equals("Cancel") && exporter.IsBusy)
            {
                exporter.CancelAsync();
            }
        }

        /// <summary>
        /// Executes the actual exporting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exporter_DoWork(object sender, DoWorkEventArgs e)
        {
            FieldDefinition fieldDefinition = FieldDefinition.Factory(Guid.NewGuid(), Program.ASSEMBLY_DOCUMENT.DisplayName);

            foreach (PropertySet ps in LegacyInterchange.PropSets/*Program.MAINWINDOW.GetPropertySetsTabControl().TranslateToPropertySets()*/)
            {
                fieldDefinition.AddPropertySet(ps);
            }

            SurfaceExporter surfaceExporter = new SurfaceExporter();
            List<string> exportedMeshes = new List<string>();
            List<string> exportedColliders = new List<string>();
            StringBuilder pathBuilder = new StringBuilder();

            int numOccurrences = Program.ASSEMBLY_DOCUMENT.ComponentDefinition.Occurrences.AllLeafOccurrences.Count;
            int progressPercent = 0;
            int currentOccurrenceID = 0;

            foreach (ComponentOccurrence currentOccurrence in Program.ASSEMBLY_DOCUMENT.ComponentDefinition.Occurrences.AllLeafOccurrences)
            {
                if (exporter.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                progressPercent = (int)Math.Floor((currentOccurrenceID / (double)numOccurrences) * 100.0);
                exporter.ReportProgress(progressPercent, "Exporting... " + progressPercent + "%");

                if (currentOccurrence.Visible &&
                    currentOccurrence.ReferencedDocumentDescriptor != null &&
                    currentOccurrence.ReferencedDocumentDescriptor.ReferencedDocumentType == DocumentTypeEnum.kPartDocumentObject &&
                    currentOccurrence.SurfaceBodies.Count > 0)
                {
                    FieldNode outputNode = new FieldNode(currentOccurrence.Name);

                    outputNode.Position = Utilities.ToBXDVector(currentOccurrence.Transformation.Translation);
                    outputNode.Rotation = Utilities.QuaternionFromMatrix(currentOccurrence.Transformation);

                    if (!exportedMeshes.Contains(currentOccurrence.ReferencedDocumentDescriptor.FullDocumentName))
                    {
                        surfaceExporter.Reset();
                        surfaceExporter.Export(((PartDocument)currentOccurrence.ReferencedDocumentDescriptor.ReferencedDocument).ComponentDefinition, false, true);

                        BXDAMesh.BXDASubMesh outputMesh = surfaceExporter.GetOutput().meshes.First();

                        exportedMeshes.Add(currentOccurrence.ReferencedDocumentDescriptor.FullDocumentName);
                        fieldDefinition.AddSubMesh(outputMesh);
                    }

                    outputNode.SubMeshID = exportedMeshes.IndexOf(currentOccurrence.ReferencedDocumentDescriptor.FullDocumentName);

                    //ComponentPropertiesTabPage componentProperties = Program.MAINWINDOW.GetPropertySetsTabControl().GetParentTabPage(currentOccurrence.Name);
                    string componentProperties = LegacyInterchange.GetCompFromDictionary(currentOccurrence.Name);

                    if (componentProperties != null)
                    {
                        outputNode.PropertySetID = componentProperties/*.Name*/;

                        PropertySet propertySet = fieldDefinition.GetPropertySets()[outputNode.PropertySetID];

                        if (propertySet.Collider.CollisionType == PropertySet.PropertySetCollider.PropertySetCollisionType.MESH &&
                            ((PropertySet.MeshCollider)propertySet.Collider).Convex)
                        {
                            if (!exportedColliders.Contains(currentOccurrence.ReferencedDocumentDescriptor.FullDocumentName))
                            {
                                exportedColliders.Add(currentOccurrence.ReferencedDocumentDescriptor.FullDocumentName);
                                var test = fieldDefinition.GetSubMesh(outputNode.SubMeshID);
                                fieldDefinition.AddCollisionMesh(ConvexHullCalculator.GetHull(fieldDefinition.GetSubMesh(outputNode.SubMeshID)));
                            }
                            outputNode.CollisionMeshID = exportedColliders.IndexOf(currentOccurrence.ReferencedDocumentDescriptor.FullDocumentName);
                        }
                    }

                    pathBuilder.Clear();

                    foreach (ComponentOccurrence co in currentOccurrence.OccurrencePath)
                    {
                        pathBuilder.Append(co.Name + "/");
                    }

                    pathBuilder.Length--;

                    fieldDefinition.NodeGroup[pathBuilder.ToString()] = outputNode;
                }

                currentOccurrenceID++;
            }

            exporter.ReportProgress(100, "Export Successful!");

            fieldDefinition.GetMeshOutput().WriteToFile(filePathTextBox.Text + "\\mesh.bxda");

            BXDFProperties.WriteProperties(filePathTextBox.Text + "\\definition.bxdf", fieldDefinition);

            // Use the commented code below for debugging.

            /** /
            string result;
            FieldDefinition readDefinition = BXDFProperties.ReadProperties(filePathTextBox.Text + "\\definition.bxdf", out result);
            MessageBox.Show(result);
            /**/
        }

        /// <summary>
        /// Updates the status label and progress bar when progress is made by the exporter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exporter_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            statusLabel.Text = (string)e.UserState;
            exportProgressBar.Value = e.ProgressPercentage;
        }

        /// <summary>
        /// Resumes normality when the export process completes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exporter_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Program.UnlockInventor();

            if (e.Cancelled || e.Error != null)
            {
                statusLabel.Text = "Export Failed.";
                //exportProgressBar.Value = 0;
            }

            exportButton.Text = "Export";
            browseButton.Enabled = true;
        }

        private void ExportForm_Load(object sender, EventArgs e)
        {

        }
    }
}
