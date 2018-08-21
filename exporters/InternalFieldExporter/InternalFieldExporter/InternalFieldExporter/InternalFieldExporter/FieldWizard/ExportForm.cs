using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inventor;


namespace InternalFieldExporter.FieldWizard
{
    public partial class ExportForm : Form
    {
        public string path;

        /// <summary>
        /// Used for determining if the exporter is running.
        /// </summary>
        //public bool IsExporting
        //{
        //    get
        //    {
        //        return Exporter.IsBusy;
        //    }
        //}

        /// <summary>
        /// Initializes a new instance of the ExportForm class.
        /// </summary>
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
        /// Starts the export process when the "Export" button is clicked.
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
            
            //Adds propertysets to FieldDefinition
            foreach (PropertySet ps in Program.MAINWINDOW.GetPropertySetsTabControl().TranslateToPropertySets())
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

            //Gets meshes and colliders and applies them to the property sets and FieldNode
            foreach (ComponentOccurrence currentOccurrence in Program.ASSEMBLY_DOCUMENT.ComponentDefinition.Occurrences.AllLeafOccurrences)
            {
                //Stops the export process
                if (exporter.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                //The progress bar % calculator and label
                progressPercent = (int)Math.Floor((currentOccurrenceID / (double)numOccurrences) * 100.0);
                exporter.ReportProgress(progressPercent, "Exporting... " + progressPercent + "%");
                
                if (currentOccurrence.Visible &&
                    currentOccurrence.ReferencedDocumentDescriptor != null &&
                    currentOccurrence.ReferencedDocumentDescriptor.ReferencedDocumentType == DocumentTypeEnum.kPartDocumentObject &&
                    currentOccurrence.SurfaceBodies.Count > 0)
                {
                    FieldNode outputNode = new FieldNode(currentOccurrence.Name);

                    //Sets the starting Position and Rotation of the FieldNode
                    outputNode.Position = Utilities.ToBXDVector(currentOccurrence.Transformation.Translation);
                    outputNode.Rotation = Utilities.QuaternionFromMatrix(currentOccurrence.Transformation);

                    //Sets meshes
                    if (!exportedMeshes.Contains(currentOccurrence.ReferencedDocumentDescriptor.FullDocumentName))
                    {
                        List<BXDAMesh.BXDASubMesh> outputMeshes = surfaceExporter.Export(currentOccurrence);

                        //Adds submeshes to the fieldDefinition
                        if (outputMeshes.Count > 0)
                        {
                            exportedMeshes.Add(currentOccurrence.ReferencedDocumentDescriptor.FullDocumentName);
                            fieldDefinition.AddSubMesh(outputMeshes[0]);
                        }
                    }
                    outputNode.SubMeshID = exportedMeshes.IndexOf(currentOccurrence.ReferencedDocumentDescriptor.FullDocumentName);

                    ComponentPropertiesTabPage componentProperties = Program.MAINWINDOW.GetPropertySetsTabControl().GetParentTabPage(currentOccurrence.Name);
                    
                    //Sets the colliders
                    if (componentProperties != null)
                    {
                        outputNode.PropertySetID = componentProperties.Name;

                        PropertySet propertySet = fieldDefinition.GetPropertySets()[outputNode.PropertySetID];

                        if (propertySet.Collider.CollisionType == PropertySet.PropertySetCollider.PropertySetCollisionType.MESH &&
                            ((PropertySet.MeshCollider)propertySet.Collider).Convex)
                        {
                            if (!exportedColliders.Contains(currentOccurrence.ReferencedDocumentDescriptor.FullDocumentName))
                            {
                                var tempMesh = new BXDAMesh();
                                tempMesh.meshes.Add(fieldDefinition.GetSubMesh(outputNode.SubMeshID));
                                var colliders = ConvexHullCalculator.GetHull(tempMesh);

                                //Adds colliders to the fieldDefinition
                                if (colliders.Count > 0)
                                {
                                    exportedColliders.Add(currentOccurrence.ReferencedDocumentDescriptor.FullDocumentName);
                                    fieldDefinition.AddCollisionMesh(colliders[0]);
                                }
                            }
                            outputNode.CollisionMeshID = exportedColliders.IndexOf(currentOccurrence.ReferencedDocumentDescriptor.FullDocumentName);
                        }
                    }

                    //Clears the old StringBuilder instance
                    pathBuilder.Clear();

                    //Builds a new path for StringBuilder
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


    }
}
