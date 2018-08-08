using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InternalFieldExporter.FieldWizard;
using System.Diagnostics;

namespace InternalFieldExporter
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();

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
        /// Opens the export form when users click the 'Export' menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        /// <summary>
        /// Prevents the user from switching tabs when the exporter is running.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            //if (ExportForm.IsExporting)
            //    e.Cancel = true;
        }


    }
}
