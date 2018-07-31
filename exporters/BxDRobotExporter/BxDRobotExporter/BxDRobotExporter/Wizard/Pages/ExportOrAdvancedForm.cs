using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BxDRobotExporter.Wizard
{
    public partial class ExportOrAdvancedForm : Form
    {
        public ExportOrAdvancedForm()
        {
            InitializeComponent();
            this.FormClosing += ExportOrAdvancedForm_FormClosing;
        }

        private void ExportOrAdvancedForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.ShowExportOrAdvancedForm = !this.showAgainCheckBox.Checked;
        }

        private void AdvancedExportButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void exportRobotButton_Click(object sender, EventArgs e)
        {
            Close();
            StandardAddInServer.Instance.ForceExport();
        }

        private void MainLayout_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}