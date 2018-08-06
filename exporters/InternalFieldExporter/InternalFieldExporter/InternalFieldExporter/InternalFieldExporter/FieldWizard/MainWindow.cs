using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        /// Prepares the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Load(object sender, EventArgs e)
        {
            menuStrip.Renderer = new ToolStripProfessionalRenderer(new SynthesisColorTable());
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void resetSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void tutorialsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
