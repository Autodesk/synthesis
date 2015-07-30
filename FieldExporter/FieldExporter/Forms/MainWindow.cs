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

    }
}
