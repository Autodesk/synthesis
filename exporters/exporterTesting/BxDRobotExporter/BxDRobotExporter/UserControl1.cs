using System;
using System.Windows.Forms;

namespace BxDRobotExporter
{
    public partial class UserControl1 : Form
    {
        public UserControl1()
        {
            InitializeComponent();
        }
        // runs the save of the files
        public void saveFile()
        {
            saveFileDialog1.ShowDialog();
            String pathToSave = saveFileDialog1.FileName;
        }
    }
}
