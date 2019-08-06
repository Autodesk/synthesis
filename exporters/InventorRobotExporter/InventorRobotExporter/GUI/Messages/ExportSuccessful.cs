using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventorRobotExporter.GUI.Messages
{
    public partial class ExportSuccessful : Form
    {
        private readonly string path;

        public ExportSuccessful(string path)
        {
            this.path = path;
            InitializeComponent();
            description.Text = description.Text.Replace("%path%", path);
        }

        private void OpenClick(object sender, EventArgs e)
        {
            Process.Start(path);
            Close();
        }

        private void OkClick(object sender, EventArgs e)
        {
            Close();
        }
    }
}
