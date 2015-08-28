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
            //Program.INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = true;

            exportButton.Enabled = false;
            browseButton.Enabled = false;

            Program.PROCESSWINDOW = new ProcessWindow(this, "Exporting...", "Exporting...",
                0, Program.ASSEMBLY_DOCUMENT.ComponentDefinition.Occurrences.AllLeafOccurrences.Count,
                new Action(() =>
                {
                    FieldDefinition fieldDefinition = new FieldDefinition("definition");
                    SurfaceExporter exporter = new SurfaceExporter();

                    foreach (PhysicsGroup g in Program.MAINWINDOW.GetPhysicsGroupsTabControl().TranslateToPhysicsGroups())
                    {
                        fieldDefinition.AddPhysicsGroup(g);
                    }

                    ComponentOccurrencesEnumerator componentOccurrences = Program.ASSEMBLY_DOCUMENT.ComponentDefinition.Occurrences.AllLeafOccurrences;

                    for (int i = 0; i < componentOccurrences.Count; i++)
                    {
                        if (Program.PROCESSWINDOW.currentState.Equals(ProcessWindow.ProcessState.CANCELLED))
                            return;

                        Program.PROCESSWINDOW.SetProgress(i, "Exporting... " + (Math.Round((i / (float)componentOccurrences.Count) * 100.0f, 2)).ToString() + "%");

                        if (componentOccurrences[i + 1].Visible)
                        {
                            exporter.Reset();
                            exporter.Export(componentOccurrences[i + 1], false, true); // Index starts at 1?

                            BXDAMesh output = exporter.GetOutput();

                            FieldNode outputNode = new FieldNode(componentOccurrences[i + 1].Name);

                            ComponentPropertiesTabPage tabPage = Program.MAINWINDOW.GetPhysicsGroupsTabControl().GetParentTabPage(componentOccurrences[i + 1].Name);
                            if (tabPage != null)
                            {
                                outputNode.physicsGroupID = tabPage.Name;
                            }

                            outputNode.AddSubMeshes(output);

                            fieldDefinition.AddChild(outputNode);
                        }
                    }

                    BXDFProperties.WriteProperties(FilePathTextBox.Text + "\\definition.bxdf", fieldDefinition);

                    fieldDefinition.CreateMesh();
                    fieldDefinition.GetMeshOutput().WriteToFile(FilePathTextBox.Text + "\\mesh.bxda");
                }),
                new Action(() =>
                {
                    MessageBox.Show(Program.PROCESSWINDOW.currentState.Equals(ProcessWindow.ProcessState.SUCCEEDED) ? "Export Successful :D" : "Export Failed :(");
                    
                    Program.UnlockInventor();
                    //Program.INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = false;
                    exportButton.Enabled = true;
                    browseButton.Enabled = true;
                }));

            Program.PROCESSWINDOW.StartProcess();
        }
    }
}
