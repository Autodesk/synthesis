using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FolderSelect;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        FolderSelect.FolderSelectDialog folderSelectDialog;
        OpenFileDialog fileSelectDialog;

        public Form1()
        {
            folderSelectDialog = new FolderSelectDialog();
            fileSelectDialog = new OpenFileDialog();
            InitializeComponent();

            txtBrowseJava.Enabled = false;
        }

        private void btnSetup_Click(object sender, EventArgs e)
        {
            String path = txtBrowse.Text;
            Console.WriteLine(path);
            String currentDir = System.IO.Directory.GetCurrentDirectory();
            String windowsCurrentDir = currentDir;
            //convert currentDir to a cygwin path
            currentDir = currentDir.Replace("C:\\", "/cygdrive/c/").Replace("\\", "/");
            System.IO.Directory.SetCurrentDirectory(path);
#if DEBUG
            String cygwinPath = "C:\\cygwin64";
            String buildTool = currentDir + "/../../../..";
#else
            String cygwinPath = "C:\\Program Files (x86)\\Autodesk\\Synthesis\\cygwin64";
            String buildTool = "/cygdrive/c/Program Files (x86)/Autodesk/Synthesis/SynthesisDrive/HELBuildTool";
#endif
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(
                        cygwinPath + "\\bin\\mintty.exe",
                        "'" + cygwinPath + "\\bin\\bash.exe' " +
                        "-c \"mount -c /cygdrive && make -f " +
                        "'" + buildTool + "/Makefile' clean " +
                        "|| read -p 'Press enter to exit'\"");
            startInfo.UseShellExecute = false;

            String envPath = System.Environment.GetEnvironmentVariable("PATH");
            startInfo.EnvironmentVariables["PATH"] = cygwinPath + "\\bin;" + envPath;
            System.Diagnostics.Process.Start(startInfo);
#if DEBUG
            System.IO.Directory.SetCurrentDirectory(windowsCurrentDir);
#endif
        }

        private void btnRunCode_Click(object sender, EventArgs e)
        {
            String number = txtNumber.Text;
            String entryPoint = txtBrowseJava.Text;
            String path = txtBrowse.Text;

            bool isJava = javaButton.Checked;

            Console.WriteLine(path);
            String currentDir = System.IO.Directory.GetCurrentDirectory();
            String windowsCurrentDir = currentDir;
            //convert currentDir to a cygwin path
            currentDir = currentDir.Replace("C:\\", "/cygdrive/c/").Replace("\\", "/");
            System.IO.Directory.SetCurrentDirectory(path);

#if DEBUG
            String cygwinPath = "C:\\cygwin64";
            String buildTool = currentDir + "/../../../..";
            String jarDir = currentDir + "/../../../../../../emulation/hel/build/java";
            String makeArgs = "SYNTHESIS_LIBS=" + currentDir + "/../../../../../../emulation/hel/build " +
                "SYNTHESIS_INCLUDES=" + currentDir + "/../../../../../../emulation/hel " +
                "SYNTHESIS_JARS=" + jarDir + " " +
                "TEAM_ID_FILE=" + currentDir + "/../../../../teamID.cpp ";
            jarDir = jarDir.Replace("/cygdrive/c/", "C:\\").Replace("\\", "/");
#else
            String cygwinPath = "C:\\Program Files (x86)\\Autodesk\\Synthesis\\cygwin64";
            String buildTool = "/cygdrive/c/Program Files (x86)/Autodesk/Synthesis/SynthesisDrive/HELBuildTool";
            String makeArgs = "";
            String jarDir = "/cygdrive/c/Program\ Files\ \(x86\)/Autodesk/Synthesis/SynthesisDrive/jar";
            jarDir = jarDir.Replace("/cygdrive/c/", "C:\\").Replace("\\", "/");
#endif
            System.Diagnostics.ProcessStartInfo startInfo;
            if (isJava) {
                Console.WriteLine(entryPoint);
                makeArgs += "ENTRY_POINT=\"" + entryPoint + "\"";
                startInfo = new System.Diagnostics.ProcessStartInfo(
                        cygwinPath + "\\bin\\mintty.exe",
                        "'" + cygwinPath + "\\bin\\bash.exe' " +
                        "-c \"mount -c /cygdrive && make -f " +
                        "'" + buildTool + "/Makefile.java' " +
                        "TEAM_ID=" + number + " " +
                        makeArgs + " run " +
                        "|| read -p 'Press enter to exit'\"");
            }
            else {
                startInfo = new System.Diagnostics.ProcessStartInfo(
                        cygwinPath + "\\bin\\mintty.exe",
                        "'" + cygwinPath + "\\bin\\bash.exe' " +
                        "-c \"mount -c /cygdrive && make -f " +
                        "'" + buildTool + "/Makefile' " +
                        "TEAM_ID=" + number + " " +
                        makeArgs + " " +
                        "&& echo 'Starting robot code' && ./build/FRC_UserProgram " +
                        "|| read -p 'Press enter to exit'\"");
            }
            String envPath = System.Environment.GetEnvironmentVariable("PATH");
            startInfo.EnvironmentVariables["PATH"] = cygwinPath + "\\bin;" + jarDir + envPath;
            startInfo.EnvironmentVariables["TEAM_ID"] = "" + number;
            startInfo.UseShellExecute = false;

            System.Diagnostics.Process.Start(startInfo);
#if DEBUG
            System.IO.Directory.SetCurrentDirectory(windowsCurrentDir);
#endif
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            //openFileDialog.ShowDialog();
            if (folderSelectDialog.ShowDialog())
            {
                txtBrowse.Text = folderSelectDialog.FileName;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void cppButton_CheckedChanged(object sender, EventArgs e)
        {
            if (javaButton.Checked)
            {
                txtBrowseJava.Enabled = true;
            }
            else
            {
                txtBrowseJava.Enabled = false;
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void txtNumber_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void lbBrowse_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
