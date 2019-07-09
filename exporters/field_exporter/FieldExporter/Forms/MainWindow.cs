using System;
using System.Windows.Forms;
using System.Collections.Generic;
using FieldExporter.Components;
using System.Diagnostics;

namespace FieldExporter
{
    public partial class MainWindow : Form
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        /// 

        

        public MainWindow()
        {
            InitializeComponent();
            comboForward.SelectedItem = comboForward.Items[0];

            comboUp.SelectedItem = comboForward.Items[2];
            Text = "Synthesis Field Exporter - " + Program.ASSEMBLY_DOCUMENT.DisplayName;
        }

        /// <summary>
        /// Returns the physicsGroupsTabControl instance.
        /// </summary>
        /// <returns></returns>
        public PropertySetsTabControl GetPropertySetsTabControl()
        {
            return propertySetsTabControl;
        }

        /// <summary>
        /// Prepares the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Load(object sender, EventArgs e)
        {
            menuStrip.Renderer = new ToolStripProfessionalRenderer(new SynthesisColorTable());

            Forms.PleaseWaitForm pleaseWait = new Forms.PleaseWaitForm("Loading config...");
            Enabled = false;
            pleaseWait.Show();

            try
            {
                if (Program.ASSEMBLY_DOCUMENT != null)
                {
                    Exporter.FieldProperties fieldProps;
                    List<PropertySet> propSets;
                    Dictionary<string, List<string>> occPropSets;

                    Exporter.SaveManager.Load(Program.ASSEMBLY_DOCUMENT, out fieldProps, out propSets, out occPropSets);

                    fieldMeta.SetSpawnpoints(fieldProps.spawnpoints);
                    GetPropertySetsTabControl().ApplyPropertySets(propSets);
                    GetPropertySetsTabControl().ApplyGamepieces(fieldProps.gamepieces);
                    GetPropertySetsTabControl().ApplyOccurrences(occPropSets);
                }
            }
            catch (Exporter.FailedToLoadException)
            {
                // Failed to load config
            }

            pleaseWait.Close();
            Enabled = true;

            
        }

        /// <summary>
        /// Closes the window when the exitToolStripMenuItem is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Saves the current configuration to the field assembly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Forms.PleaseWaitForm pleaseWait = new Forms.PleaseWaitForm("Saving config...");
            Enabled = false;
            pleaseWait.Show();

            try
            {
                if (Program.ASSEMBLY_DOCUMENT != null)
                {
                    Exporter.FieldProperties fieldProps = new Exporter.FieldProperties(FieldMetaForm.GetSpawnpoints(),
                                                                                       GetPropertySetsTabControl().TranslateToGamepieces());
                    List<PropertySet> propSets = GetPropertySetsTabControl().TranslateToPropertySets();
                    Exporter.SaveManager.Save(Program.ASSEMBLY_DOCUMENT, fieldProps, propSets);
                }

                Enabled = true;
                pleaseWait.Close();
            }
            catch (Exporter.FailedToSaveException er)
            {
                pleaseWait.Close();
                MessageBox.Show("Failed to save field configuration. The following error occurred:\n" + er.InnerException.ToString(), "Error", MessageBoxButtons.OK);
                Enabled = true;
            }
        }

        /// <summary>
        /// Resets the size of the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resetSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
            Size = MinimumSize;
        }

        /// <summary>
        /// Toggles the TopMost property for the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (alwaysOnTopToolStripMenuItem.Checked)
            {
                TopMost = true;
            }
            else
            {
                TopMost = false;
            }
        }

        /// <summary>
        /// Opens the user's browser and brings them to the field exporter tutorial page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tutorialsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://bxd.autodesk.com/synthesis/?page=tutorialFieldExporter");
        }

        /// <summary>
        /// Prevents the user from switching tabs when the exporter is running.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (exportForm.IsExporting)
                e.Cancel = true;
        }

        private void UpdateRotationOffset()
        {
            BXDVector3 rot = new BXDVector3();

     
            // Forward Remap
            switch (comboForward.SelectedIndex)
            {
                case 0: // Forward
                    if (!checkForwardInvert.Checked)
                    {
                        rot.y = 0;
                    }
                    else
                    {
                        rot.y = -180;
                    }
                    break;
                case 1: // Side
                    if (!checkForwardInvert.Checked)
                    {
                        rot.y = 90;
                    }
                    else
                    {
                        rot.y = -90;
                    }
                    break;
                case 2: // Up
                    if (!checkForwardInvert.Checked)
                    {
                        rot.x = 90;
                    }
                    else
                    {
                        rot.x = -90;
                    }
                    break;
            }


            switch (comboUp.SelectedIndex)
            {
                case 0: // Forward
                    if (!checkUpInvert.Checked)
                    {
                        rot.x = -90;
                    }
                    else
                    {
                        rot.x = 90;
                    }
                    break;
                case 1: // Side
                    if (!checkUpInvert.Checked)
                    {
                        rot.z = -90;
                    }
                    else
                    {
                        rot.z = 90;
                    }
                    break;
                case 2: // Up
                    if (!checkUpInvert.Checked)
                    {
                        rot.z = 0;
                    }
                    else
                    {
                        rot.z = 180;
                    }
                    break;
            }

            fieldMeta.SetRotationOffset(rot);
        }

        private void ComboForward_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateRotationOffset();
        }

        private void ComboUp_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateRotationOffset();
        }

        private void CheckForwardInvert_CheckedChanged(object sender, EventArgs e)
        {
            UpdateRotationOffset();
        }

        private void CheckUpInvert_CheckedChanged(object sender, EventArgs e)
        {
            UpdateRotationOffset();
        }
    }
}
