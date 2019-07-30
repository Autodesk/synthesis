using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventorRobotExporter.GUI.Editors.SimpleJointEditor
{
    public partial class AdvancedJointSettings : Form
    {
        public AdvancedJointSettings()
        {
            InitializeComponent();

            this.MaximizeBox = false;

            FillInfo();
        }

        private void FillInfo()
        {
            // To be removed later, just for concept
            ListViewItem item = new ListViewItem("Encoder");
            item.SubItems.Add("3");
            item.SubItems.Add("4");

            sensorsTable.Items.Add(item);

            portTypeInput.SelectedIndex = 0;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            Close();
            // TODO: Add save functionality
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
