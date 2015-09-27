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
            folderBrowserDialog.ShowDialog();
            FilePathTextBox.Text = folderBrowserDialog.SelectedPath;
        }

        /// <summary>
        /// Starts the export process when the "Export" button is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exportButton_Click(object sender, EventArgs e)
        {
            if (FilePathTextBox.Text.Length == 0)
            {
                MessageBox.Show("Invalid Export Parameters.");
                return;
            }

            Program.LockInventor();

            exportButton.Enabled = false;
            browseButton.Enabled = false;

            Program.PROCESSWINDOW = new ProcessWindow(this, "Exporting...", "Exporting...",
                0, Program.ASSEMBLY_DOCUMENT.ComponentDefinition.Occurrences.AllLeafOccurrences.Count,
                new Action(() =>
                { 
                    FieldDefinition fieldDefinition = FieldDefinition.Factory(Guid.NewGuid(), Program.ASSEMBLY_DOCUMENT.DisplayName);
                    SurfaceExporter exporter = new SurfaceExporter();

                    foreach (PhysicsGroup g in Program.MAINWINDOW.GetPhysicsGroupsTabControl().TranslateToPhysicsGroups())
                    {
                        fieldDefinition.AddPhysicsGroup(g);
                    }

                    ComponentOccurrencesEnumerator componentOccurrences = Program.ASSEMBLY_DOCUMENT.ComponentDefinition.Occurrences.AllLeafOccurrences;
                    ComponentOccurrence currentOccurrence;

                    StringBuilder pathBuilder = new StringBuilder();

                    for (int i = 0; i < componentOccurrences.Count; i++)
                    {
                        if (Program.PROCESSWINDOW.currentState.Equals(ProcessWindow.ProcessState.CANCELLED))
                            return;

                        Program.PROCESSWINDOW.SetProgress(i + 1, "Exporting... " + (Math.Round(((i + 1) / (float)componentOccurrences.Count) * 100.0f, 2)).ToString() + "%");

                        currentOccurrence = componentOccurrences[i + 1];

                        if (currentOccurrence.Visible)
                        {
                            exporter.Reset();
                            exporter.Export(currentOccurrence, false, true);

                            BXDAMesh output = exporter.GetOutput();

                            FieldNode outputNode = new FieldNode(currentOccurrence.Name);

                            ComponentPropertiesTabPage tabPage = Program.MAINWINDOW.GetPhysicsGroupsTabControl().GetParentTabPage(currentOccurrence.Name);

                            if (tabPage != null)
                                outputNode.PhysicsGroupID = tabPage.Name;

                            outputNode.SubMesh = output.meshes.First();

                            pathBuilder.Clear();

                            foreach (ComponentOccurrence co in currentOccurrence.OccurrencePath)
                            {
                                pathBuilder.Append(co.Name + "/");
                            }

                            pathBuilder.Length--;

                            fieldDefinition.NodeGroup[pathBuilder.ToString()] = outputNode;
                        }
                    }

                    fieldDefinition.CreateMesh();
                    fieldDefinition.GetMeshOutput().WriteToFile(FilePathTextBox.Text + "\\mesh.bxda");

                    Guid guid = Guid.NewGuid();
                    BXDFProperties.WriteProperties(FilePathTextBox.Text + "\\definition.bxdf", fieldDefinition);

                    FieldDefinition readDefinition = BXDFProperties.ReadProperties(FilePathTextBox.Text + "\\definition.bxdf");

                    //MessageBox.Show("GUID: " + readDefinition.GUID.ToString());

                    //foreach (KeyValuePair<string, PhysicsGroup> physicsGroup in readDefinition.GetPhysicsGroups())
                    //{
                    //    MessageBox.Show("PhysicsGroupID: " + physicsGroup.Value.PhysicsGroupID);
                    //    MessageBox.Show("CollisionType: " + physicsGroup.Value.CollisionType.ToString());
                    //    MessageBox.Show("Friction: " + physicsGroup.Value.Friction.ToString());
                    //    MessageBox.Show("Mass: " + physicsGroup.Value.Mass.ToString());
                    //}

                    //foreach (FieldNode node in readDefinition.NodeGroup.EnumerateAllLeafFieldNodes())
                    //{
                    //    MessageBox.Show("NodeID: " + node.NodeID);
                    //    MessageBox.Show("MeshID: " + node.MeshID);
                    //    MessageBox.Show("PhysicsGroupID: " + node.PhysicsGroupID);
                    //}
                }),
                new Action(() =>
                {
                    MessageBox.Show(Program.PROCESSWINDOW.currentState.Equals(ProcessWindow.ProcessState.SUCCEEDED) ? "Export Successful :D" : "Export Failed :(");
                    
                    Program.UnlockInventor();

                    exportButton.Enabled = true;
                    browseButton.Enabled = true;
                }));

            Program.PROCESSWINDOW.StartProcess();
        }
    }
}
