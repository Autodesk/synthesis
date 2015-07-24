using Inventor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using FieldExporter.Components;
using FieldExporter.Controls;

namespace FieldExporter
{
    public partial class MainWindow : Form
    {   
        /// <summary>
        /// Constructs the form.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Checks to see if there is an active document in Inventor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Activated(object sender, EventArgs e)
        {
            if (Program.INVENTOR_APPLICATION.ActiveDocument is AssemblyDocument)
            {
                Text = "Field Exporter - " + Program.INVENTOR_APPLICATION.ActiveDocument.DisplayName;
            }
            else
            {
                Text = "Field Exporter - No Document Found";
                //inventorTreeView.Reset();
            }
        }

        /// <summary>
        /// Disposes any background processes to ensure safe closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = false;
        }

        /// <summary>
        /// Launches the file browser dialog window and updates the file path text box text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseButton_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.ShowDialog();
            FilePathTextBox.Text = folderBrowserDialog.SelectedPath;
        }

        /// <summary>
        /// Shows the progress window and starts the export process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (FilePathTextBox.Text.Length == 0)
            {
                MessageBox.Show("Invalid Export Parameters.");
                return;
            }

            Program.INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = true;

            ExportButton.Enabled = false;
            BrowseButton.Enabled = false;

            Program.progressWindow = new ProgressWindow(this, "Exporting...", "Exporting...",
                0, ((AssemblyDocument)Program.INVENTOR_APPLICATION.ActiveDocument).ComponentDefinition.Occurrences.AllLeafOccurrences.Count,
                new Action(() =>
                    {
                        FieldDefinition fieldDefinition = new FieldDefinition("definition");
                        SurfaceExporter exporter = new SurfaceExporter();

                        ComponentOccurrencesEnumerator componentOccurrences = ((AssemblyDocument)Program.INVENTOR_APPLICATION.ActiveDocument).ComponentDefinition.Occurrences.AllLeafOccurrences;

                        for (int i = 0; i < componentOccurrences.Count; i++)
                        {
                            if (Program.progressWindow.currentState.Equals(ProgressWindow.ProcessState.CANCELLED))
                                return;

                            Program.progressWindow.SetProgress(i, "Exporting: " + (Math.Round((i / (float)componentOccurrences.Count) * 100.0f, 2)).ToString() + "%");

                            if (componentOccurrences[i + 1].Visible)
                            {
                                exporter.Reset();
                                exporter.Export(componentOccurrences[i + 1], false, true); // Index starts at 1?

                                BXDAMesh output = exporter.GetOutput();

                                FieldNode outputNode = new FieldNode(componentOccurrences[i + 1].Name);

                                PhysicsGroupsTabControl tabControl = (PhysicsGroupsTabControl)physicsTab.Controls["physicsGroupsTabControl"];
                                if (tabControl != null)
                                {
                                    ComponentPropertiesTabPage tabPage = tabControl.GetParentTabPage(componentOccurrences[i + 1].Name);
                                    if (tabPage != null)
                                    {
                                        tabPage.childForm.Invoke(new Action(() =>
                                            {
                                                outputNode = new FieldNode(
                                                    componentOccurrences[i + 1].Name,
                                                    tabPage.childForm.GetCollisionType(),
                                                    tabPage.childForm.IsConvex(),
                                                    tabPage.childForm.GetFriction());
                                            }));
                                    }
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
                        MessageBox.Show(new Form() { TopMost = true },
                            Program.progressWindow.currentState.Equals(ProgressWindow.ProcessState.SUCCEEDED) ? "Export Successful!" : "Export Failed.");

                        Program.INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = false;
                        ExportButton.Enabled = true;
                        BrowseButton.Enabled = true;
                    }));

            Program.progressWindow.StartProcess();
        }

    }
}
