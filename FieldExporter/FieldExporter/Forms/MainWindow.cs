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

namespace FieldExporter
{
    public partial class MainWindow : Form
    {
        /*
         * TODO: First improve ProgressWindow disposition. Then optimize code. Then add features.
         */

        /// <summary>
        /// The events caused by user interaction with Inventor.
        /// </summary>
        private InteractionEvents interactionEvents;

        /// <summary>
        /// The events triggered by object selection in Inventor
        /// </summary>
        private SelectEvents selectEvents;

        /// <summary>
        /// Used to determine if Inventor interaction is enabled.
        /// </summary>
        private bool interactionEnabled = false;

        /// <summary>
        /// The ProgressWindow instance.
        /// </summary>
        private ProgressWindow progressWindow;
        
        /// <summary>
        /// Constructs the form.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Enables interaction events.
        /// </summary>
        private void EnableInteractionEvents()
        {
            try
            {
                interactionEvents = Program.INVENTOR_APPLICATION.CommandManager.CreateInteractionEvents();
                interactionEvents.OnActivate += interactionEvents_OnActivate;
                interactionEvents.Start();

                inventorTreeView.HotTracking = false;

                InventorSelectButton.Text = "Cancel Selection";

                interactionEnabled = true;
            }
            catch
            {
                MessageBox.Show("Cannot enter select mode.", "Document not found.");
            }
        }

        /// <summary>
        /// Disables interaction events.
        /// </summary>
        private void DisableInteractionEvents()
        {
            interactionEvents.Stop();

            Program.INVENTOR_APPLICATION.ActiveDocument.SelectSet.Clear();

            inventorTreeView.HotTracking = true;

            InventorSelectButton.Text = "Select in Inventor";
            AddSelectionButton.Enabled = false;

            interactionEnabled = false;
        }

        /// <summary>
        /// Enables select events when interaction events are activated.
        /// </summary>
        void interactionEvents_OnActivate()
        {
            selectEvents = interactionEvents.SelectEvents;
            selectEvents.AddSelectionFilter(SelectionFilterEnum.kAssemblyOccurrenceFilter);
            selectEvents.OnSelect += selectEvents_OnSelect;
            selectEvents.OnPreSelect += selectEvents_OnPreSelect;
        }

        /// <summary>
        /// Allows the user to see if they have already added a collision component in select mode.
        /// </summary>
        /// <param name="PreSelectEntity"></param>
        /// <param name="DoHighlight"></param>
        /// <param name="MorePreSelectEntities"></param>
        /// <param name="SelectionDevice"></param>
        /// <param name="ModelPosition"></param>
        /// <param name="ViewPosition"></param>
        /// <param name="View"></param>
        void selectEvents_OnPreSelect(ref object PreSelectEntity, out bool DoHighlight, ref ObjectCollection MorePreSelectEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
        {
            DoHighlight = true;

            if (PreSelectEntity is ComponentOccurrence)
            {
                ComponentOccurrence componentOccurrence = (ComponentOccurrence)PreSelectEntity;

                if (inventorTreeView.Nodes.Find(componentOccurrence.Name, true).Length > 0)
                {
                    inventorTreeView.Invoke(new Action(() =>
                        {
                            inventorTreeView.SelectedNode = inventorTreeView.Nodes.Find(componentOccurrence.Name, true)[0];
                            inventorTreeView.SelectedNode.EnsureVisible();
                        }));
                }
            }
        }

        /// <summary>
        /// Enables the "Add Selection" button when an object in Inventor is selected.
        /// </summary>
        /// <param name="JustSelectedEntities"></param>
        /// <param name="SelectionDevice"></param>
        /// <param name="ModelPosition"></param>
        /// <param name="ViewPosition"></param>
        /// <param name="View"></param>
        void selectEvents_OnSelect(ObjectsEnumerator JustSelectedEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
        {
            if (!AddSelectionButton.Enabled)
            {
                AddSelectionButton.Invoke(new Action(() =>
                {
                    AddSelectionButton.Enabled = true;
                }));
            }
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
                inventorTreeView.Reset();
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

            SelectionAdder.Dispose();
            Exporter.Dispose();
        }

        /// <summary>
        /// Allows the user to see what component each tree node references by selecting the component in Inventor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void inventorTreeView_NodeMouseHover(object sender, TreeNodeMouseHoverEventArgs e)
        {
            if (!interactionEnabled)
            {
                Program.INVENTOR_APPLICATION.ActiveDocument.SelectSet.Clear();
                Program.INVENTOR_APPLICATION.ActiveDocument.SelectSet.Select((ComponentOccurrence)e.Node.Tag);
            }
        }

        /// <summary>
        /// Enables or disables Inventor interaction.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InventorSelectButton_Click(object sender, EventArgs e)
        {
            if (interactionEnabled)
            {
                DisableInteractionEvents();
            }
            else
            {
                EnableInteractionEvents();
            }
        }

        /// <summary>
        /// Opens the progess window and starts the selection adding process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddSelectionButton_Click(object sender, EventArgs e)
        {
            Program.INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = true;

            AddSelectionButton.Enabled = false;
            InventorSelectButton.Enabled = false;

            progressWindow = new ProgressWindow();
            progressWindow.Show(this);
            progressWindow.ProcessProgressBar.Minimum = 0;
            progressWindow.ProcessProgressBar.Maximum = selectEvents.SelectedEntities.Count;
            progressWindow.ProcessProgressBar.Value = 0;
            progressWindow.ProcessProgressBar.Step = 1;
            progressWindow.Disposed += progressWindow_Disposed;

            SelectionAdder.RunWorkerAsync();
        }

        private void progressWindow_Disposed(object sender, EventArgs e)
        {
            if (SelectionAdder.IsBusy) SelectionAdder.CancelAsync();
            if (Exporter.IsBusy) SelectionAdder.CancelAsync();
        }

        /// <summary>
        /// Scans the selected objects, updates the scan progress window, and
        /// adds the scanned elements to the collision objects view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectionAdder_DoWork(object sender, DoWorkEventArgs e)
        {
            inventorTreeView.Invoke(new Action<ObjectsEnumerator, Action<int, int>>(inventorTreeView.AddComponents),
                selectEvents.SelectedEntities, new Action<int, int>((int progress, int total) =>
                    {
                        progressWindow.Invoke(new Action(() =>
                            {
                                progressWindow.ProcessInfoLabel.Text = "Processing: " + (Math.Round((progress / (float)total) * 100.0f, 2)).ToString() + "%";
                                progressWindow.ProcessProgressBar.PerformStep();
                            }));
                    }));
        }

        /// <summary>
        /// Closes the progress window, and disables Inventor interaction.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectionAdder_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Program.INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = false;

            progressWindow.Close();
            DisableInteractionEvents();
            InventorSelectButton.Enabled = true;
        }

        /// <summary>
        /// Launches the file browser dialog window and updates the file path text box text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog.ShowDialog();
            FilePathTextBox.Text = FolderBrowserDialog.SelectedPath;
        }

        /// <summary>
        /// Shows the progress window and starts the export process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportButton_Click(object sender, EventArgs e)
        {
            Program.INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = true;

            ExportButton.Enabled = false;

            progressWindow = new ProgressWindow();
            progressWindow.Show(this);
            progressWindow.Text = "Exporting...";
            progressWindow.ProcessInfoLabel.Text = "Exporting...";
            progressWindow.ProcessProgressBar.Minimum = 0;
            progressWindow.ProcessProgressBar.Maximum = ((AssemblyDocument)Program.INVENTOR_APPLICATION.ActiveDocument).ComponentDefinition.Occurrences.AllLeafOccurrences.Count;
            progressWindow.ProcessProgressBar.Value = 0;
            progressWindow.ProcessProgressBar.Step = 1;

            Exporter.RunWorkerAsync();
        }

        /// <summary>
        /// Exports the BXDF and BXDA files.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exporter_DoWork(object sender, DoWorkEventArgs e)
        {
            if (FilePathTextBox.Text.Length == 0 || FileNameTextBox.Text.Length == 0 || FileNameTextBox.Text.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            {
                e.Result = "Invalid Export Parameters.";
                return;
            }

            FieldDefinition fieldDefinition = new FieldDefinition(FileNameTextBox.Text);
            SurfaceExporter exporter = new SurfaceExporter();

            ComponentOccurrencesEnumerator componentOccurrences = ((AssemblyDocument)Program.INVENTOR_APPLICATION.ActiveDocument).ComponentDefinition.Occurrences.AllLeafOccurrences;

            for (int i = 0; i < componentOccurrences.Count; i++)
            {
                if (!progressWindow.IsDisposed)
                {
                    progressWindow.Invoke(new Action(() =>
                        {
                            progressWindow.ProcessInfoLabel.Text = "Exporting: " + (Math.Round((i / (float)componentOccurrences.Count) * 100.0f, 2)).ToString() + "%";
                            progressWindow.ProcessProgressBar.Value = i;
                        }));
                }

                if (componentOccurrences[i + 1].Visible)
                {
                    exporter.Reset();
                    exporter.Export(componentOccurrences[i + 1], false, true); // Index starts at 1?

                    BXDAMesh output = exporter.GetOutput();

                    FieldNode outputNode = new FieldNode(componentOccurrences[i + 1].Name,
                        inventorTreeView.Nodes.Find(componentOccurrences[i + 1].Name, true).Length > 0 ?
                        FieldNodeCollisionType.MESH : FieldNodeCollisionType.NONE);

                    outputNode.AddSubMeshes(output);

                    fieldDefinition.AddChild(outputNode);
                }
            }

            BXDFProperties.WriteProperties(FilePathTextBox.Text + "\\" + FileNameTextBox.Text + ".bxdf", fieldDefinition);

            fieldDefinition.CreateMesh();
            fieldDefinition.GetMeshOutput().WriteToFile(FilePathTextBox.Text + "\\" + FileNameTextBox.Text + ".bxda");

            FieldDefinition_Base copyDefinition = BXDFProperties.ReadProperties(FilePathTextBox.Text + "\\" + FileNameTextBox.Text + ".bxdf");
            
            e.Result = "Export Successful!";
        }

        /// <summary>
        /// Shows the completion message and closes the progress window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exporter_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Program.INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = false;

            MessageBox.Show(e.Result.ToString());
            progressWindow.Close();
            ExportButton.Enabled = true;
        }

    }
}
