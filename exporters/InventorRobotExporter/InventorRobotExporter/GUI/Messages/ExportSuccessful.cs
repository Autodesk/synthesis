using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventorRobotExporter.GUI.Messages
{
    public partial class ExportSuccessful : Form
    {
        public ExportSuccessful(string path)
        {
            InitializeComponent();

            description.Text = description.Text.Replace("%path%", path);
            this.MaximizeBox = false;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
