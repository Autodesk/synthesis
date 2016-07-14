using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventorAddInBasicGUI2
{
    public partial class UserControl1 : Form
    {
        public UserControl1()
        {
            InitializeComponent();
        }
        public void saveFile()
        {
            saveFileDialog1.ShowDialog();
            String pathToSave = saveFileDialog1.FileName;
        }
    }
}
