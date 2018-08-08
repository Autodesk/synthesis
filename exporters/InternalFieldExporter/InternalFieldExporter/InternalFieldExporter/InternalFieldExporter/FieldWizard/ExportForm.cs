using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InternalFieldExporter.FieldWizard
{
    public partial class ExportForm : Form
    {
        public string path;

        /// <summary>
        /// Used for determining if the exporter is running.
        /// </summary>
        public bool IsExporting
        {
            get
            {
                return Exporter.IsBusy;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ExportForm class.
        /// </summary>
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
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                filePathTextBox.Text = folderBrowserDialog.SelectedPath;
                exportButton.Enabled = true;
                statusLabel.Text = "Ready to export.";
            }
        }
    }
}
