using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BxDRobotExporter.Forms
{
    public partial class ChooseExportModeForm : Form
    {
        public ChooseExportModeForm()
        {
            InitializeComponent();

            OneClickExportButton.MouseHover += OneClickExportButton_MouseHover;
            GuidedExportButton.MouseHover += GuidedExportButton_MouseHover;
            AdvancedExportButton.MouseHover += AdvancedExportButton_MouseHover;

            OneClickExportButton.MouseLeave += _MouseLeave;
            GuidedExportButton.MouseLeave += _MouseLeave;
            AdvancedExportButton.MouseLeave += _MouseLeave;
        }

        private void OneClickExportButton_Click(object sender, EventArgs e)
        {
            Hide();
            StandardAddInServer.Instance.BeginOneClickExport_OnExecute(null);
            Close();
        }

        private void OneClickExportButton_MouseHover(object sender, EventArgs e)
        {
            this.ExportModeInfoLabel.Text = "One Click Export: Simply define how many wheels and what type of drive train your robot has," +
                " and the exporter will do the work from there. Use this if you want to start driving your robot in Synthesis right away." +
                "\nCurrently only Western and Mecanum drives are supported, but more types are coming soon.";
        }
        
        private void GuidedExportButton_Click(object sender, EventArgs e)
        {
            Hide();
            StandardAddInServer.Instance.BeginWizardExport_OnExecute(null);
            Close();
        }

        private void GuidedExportButton_MouseHover(object sender, EventArgs e)
        {
            this.ExportModeInfoLabel.Text = "Guided Export: Helps you through the export process by giving you a structured GUI to define wheels and joints. " +
                "This is designed to be both easy to use and fully functional.";
        }
        
        private void AdvancedExportButton_Click(object sender, EventArgs e)
        {
            Hide();
            StandardAddInServer.Instance.BeginAdvancedExport_OnExecute(null);
            Close();
        }

        private void AdvancedExportButton_MouseHover(object sender, EventArgs e)
        {
            this.ExportModeInfoLabel.Text = "Advanced Export: Simply exports the meshes and leaves everything else up to the user. " +
                "Recommended if you like the old robot exporter, or if you intend to emulate code.";
        }


        private void _MouseLeave(object sender, EventArgs e)
        {
            ExportModeInfoLabel.Text = "Roll over the buttons to see information about each export mode.";
        }
    }
}
