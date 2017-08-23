using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace BxDRobotExporter.Wizard
{
    public partial class OneClickExportForm : Form
    {
        private Dictionary<string, string> fields = new Dictionary<string, string>();

        public OneClickExportForm()
        {
            InitializeComponent();
        }

        private void OneClickExportForm_Load(object sender, EventArgs e)
        {
            var dirs = Directory.GetDirectories(@"C:\users\t_howab\documents\synthesis\fields");

            foreach (var dir in dirs)
            {
                fields.Add(dir.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last(), dir);
                FieldSelectComboBox.Items.Add(dir.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last());
            }


        }

        private void LaunchSynthesisCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            FieldSelectComboBox.Enabled = LaunchSynthesisCheckBox.Checked;
        }

        private void CancelExportButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            Hide();

        }
    }
}
