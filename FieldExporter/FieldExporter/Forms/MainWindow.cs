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
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Text = "Synthesis Field Exporter - " + Program.ASSEMBLY_DOCUMENT.DisplayName;
        }

        /// <summary>
        /// Returns the physicsGroupsTabControl instance.
        /// </summary>
        /// <returns></returns>
        public PhysicsGroupsTabControl GetPhysicsGroupsTabControl()
        {
            return physicsGroupsTabControl;
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
            Size = new Size(960, 720);
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
        /// Prevents the user from switching tabs when the progress window is open.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (System.Windows.Forms.Application.OpenForms.OfType<ProcessWindow>().Any())
            {
                e.Cancel = true;
            }
        }

    }
}
