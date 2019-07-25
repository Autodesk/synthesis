using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Inventor;

namespace FieldExporter.Components
{
    public partial class ExportForm : UserControl
    {
        public string path;

        public readonly string FIELD_FOLDER = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\Autodesk\\Synthesis\\Fields\\";

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
        /// Starts the export process when the "Export" button is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void exportButton_Click(object sender, EventArgs e)
        {
            if (exportButton.Text.Equals("Export") && !exporter.IsBusy)
            {
                exportButton.Text = "Cancel";
                fieldNameTextBox.Enabled = false;

                Program.LockInventor();

                exporter.RunWorkerAsync();
            }
            else if (exportButton.Text.Equals("Cancel") && exporter.IsBusy)
            {
                exporter.CancelAsync();
            }
        }

        private void fieldNameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (fieldNameTextBox.Text.Length > 0)
            {
                exportButton.Enabled = true;
                statusLabel.Text = "Ready to export.";
            }
            else
            {
                exportButton.Enabled = false;
                statusLabel.Text = "Please specify a field name.";
            }
        }

        private void ExportFieldData(string folder)
        {
            Exporter.FieldProperties fieldProps = new Exporter.FieldProperties(FieldMetaForm.GetSpawnpoints(),
                                                                               Program.MAINWINDOW.GetPropertySetsTabControl().TranslateToGamepieces());

            fieldProps.Write(folder + "\\field_data.xml");
        }

        /// <summary>
        /// Executes the actual exporting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exporter_DoWork(object sender, DoWorkEventArgs e)
        {
            string directory = FIELD_FOLDER + fieldNameTextBox.Text;

            // Create directory if it does not exist
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            // Warn user of overwrite if it does exist
            else if (MessageBox.Show("A field with this name already exists. Continue?", "Overwrite Existing Field", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            FieldDefinition fieldDefinition = FieldDefinition.Factory(Guid.NewGuid(), Program.ASSEMBLY_DOCUMENT.DisplayName);

            foreach (PropertySet ps in Program.MAINWINDOW.GetPropertySetsTabControl().TranslateToPropertySets())
                fieldDefinition.AddPropertySet(ps);

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

                    ComponentPropertiesTabPage componentProperties = Program.MAINWINDOW.GetPropertySetsTabControl().GetParentTabPage(currentOccurrence.Name);

                    if (componentProperties != null)
                    {
                        outputNode.PropertySetID = componentProperties.Name;

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

            fieldDefinition.GetMeshOutput().WriteToFile(directory + "\\mesh.bxda");
            
            // Field data such as spawnpoints and gamepieces
            ExportFieldData(directory);

            // Property sets
            BXDFProperties.WriteProperties(directory + "\\definition.bxdf", fieldDefinition);

            // Open the export directory when done
            if (openFolderCheckBox.Checked)
                Process.Start("explorer.exe", "/select, " + directory);
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

            if (e.Cancelled)
            {
                statusLabel.Text = "Export Canceled.";
            }
            else if (e.Error != null)
            {
                statusLabel.Text = "Export Failed.";
            }

            exportButton.Text = "Export";
            fieldNameTextBox.Enabled = true;
        }
    }
}
