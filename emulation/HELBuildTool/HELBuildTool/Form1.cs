using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            String number = txtNumber.Text;
            String path = txtBrowse.Text;

            System.IO.Directory.SetCurrentDirectory(path);

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo("C:\\cygwin64\\bin\\mintty.exe", "C:\\cygwin64\\bin\\bash.exe -c \"mount -c /cygdrive; make -f /cygdrive/c/cygwin64/home/t_leeb/synthesis/emulation/HELBuildTool/Makefile && ./build/FRC_UserProgram || read -p \"Press enter to continue\"\"");
            startInfo.EnvironmentVariables["PATH"] = "C:\\cygwin64\\bin";
            startInfo.UseShellExecute = false;

            System.Diagnostics.Process.Start(startInfo);
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtBrowse.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
