using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inventor;

namespace FieldExporter.Components
{
    public partial class ExportForm : UserControl
    {
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
        private void exportButton_Click(object sender, EventArgs e)
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
            SurfaceExporter surfaceExporter = new SurfaceExporter();

            foreach (PropertySet g in Program.MAINWINDOW.GetPropertySetsTabControl().TranslateToPropertySets())
            {
                fieldDefinition.AddPropertySet(g);
            }

            ComponentOccurrencesEnumerator componentOccurrences = Program.ASSEMBLY_DOCUMENT.ComponentDefinition.Occurrences.AllLeafOccurrences;
            ComponentOccurrence currentOccurrence;

            StringBuilder pathBuilder = new StringBuilder();

            for (int i = 0; i < componentOccurrences.Count; i++)
            {
                if (exporter.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                exporter.ReportProgress((int)Math.Round(((i + 1) / (float)componentOccurrences.Count) * 100.0f));

                currentOccurrence = componentOccurrences[i + 1];

                if (currentOccurrence.Visible && currentOccurrence.SurfaceBodies.Count > 0) // If the part has a mesh.
                {
                    FieldNode outputNode = new FieldNode(currentOccurrence.Name);

                    surfaceExporter.Reset();
                    surfaceExporter.Export(currentOccurrence, false, true);

                    BXDAMesh output = surfaceExporter.GetOutput();

                    fieldDefinition.AddSubMesh(output.meshes.First(), outputNode);

                    ComponentPropertiesTabPage tabPage = Program.MAINWINDOW.GetPropertySetsTabControl().GetParentTabPage(currentOccurrence.Name);

                    if (tabPage != null)
                    {
                        outputNode.PropertySetID = tabPage.Name;

                        if (fieldDefinition.GetPropertySet()[outputNode.PropertySetID].Collider.CollisionType == PropertySet.PropertySetCollider.PropertySetCollisionType.MESH)
                        {
                            fieldDefinition.AddCollisionMesh(ConvexHullCalculator.GetHull(fieldDefinition.GetSubMesh(outputNode.SubMeshID),
                                ((PropertySet.MeshCollider)fieldDefinition.GetPropertySet()[outputNode.PropertySetID].Collider).Convex),
                                outputNode);
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
            }

            fieldDefinition.GetMeshOutput().WriteToFile(filePathTextBox.Text + "\\mesh.bxda");

            BXDFProperties.WriteProperties(filePathTextBox.Text + "\\definition.bxdf", fieldDefinition);
        }

        /// <summary>
        /// Updates the status label and progress bar when progress is made by the exporter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exporter_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            statusLabel.Text = "Exporting... " + e.ProgressPercentage.ToString() + "%";
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
                MessageBox.Show(e.Error.Message);
                exportProgressBar.Value = 0;
            }
            else
            {
                statusLabel.Text = "Export Successful!";
            }

            exportButton.Text = "Export";
            browseButton.Enabled = true;
        }
    }
}
