using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventorMirabufExporter.GUI
{
    public partial class ConfigUI : Form
    {
        public ConfigUI()
        {
            InitializeComponent();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (Convert.ToBoolean(checkBox1.CheckState))
                groupBox1.Enabled = true;
            else
                groupBox1.Enabled = false;
        }
    }
}
