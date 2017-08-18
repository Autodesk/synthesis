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
            String number = txtNumber.Text;
            String firstDigits = number.Substring(0, number.Length - 2);
            String secondDigits = number.Substring(number.Length - 2, 2);

            String deviceName;

            //get all loopback adapters to see if one already exists
            System.Management.ManagementObjectCollection results = (new System.Management.ManagementObjectSearcher(
                        "SELECT Name, NetConnectionID, PNPDeviceID, ConfigManagerErrorCode, NetConnectionStatus " +
                        "FROM Win32_NetworkAdapter " +
                        "WHERE ServiceName='msloop'")).Get();

            if(results.Count > 0) {
                deviceName = results.GetEnumerator().Current.Properties.Cast<System.Management.PropertyData>()
                    .Single(p => p.Name == "Name").Value.ToString();
            }
            else {

                //enable a loopback adapter
                System.Diagnostics.Process.Start("cmd", "/C devcon install %WINDIR%\\Inf\\NetLoop.inf *MSLOOP")
                    .WaitForExit(0);

                results = (new System.Management.ManagementObjectSearcher(
                            "SELECT Name, NetConnectionID, PNPDeviceID, ConfigManagerErrorCode, NetConnectionStatus " +
                            "FROM Win32_NetworkAdapter " +
                            "WHERE ServiceName='msloop'")).Get();
                deviceName = results.GetEnumerator().Current.Properties.Cast<System.Management.PropertyData>()
                    .Single(p => p.Name == "Name").Value.ToString();
            }

            //set the IP address of the loopback adapter
            System.Diagnostics.Process.Start("netsh", "int ip set address name=\"" + deviceName +
                    "\" static 10." + firstDigits + "." + secondDigits + ".2 255.255.255.0");
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
            Console.WriteLine(entryPoint);

#if DEBUG
            String cygwinPath = "C:\\cygwin64";
            String buildTool = currentDir + "/../../..";
            String makeArgs = "SYNTHESIS_LIBS=" + currentDir + "/../../../../../emulation/hel/build " +
                "SYNTHESIS_INCLUDES=" + currentDir + "/../../../../../emulation/hel " +
                "SYNTHESIS_JARS=" + currentDir + "/../../../../../emulation/hel/build/java " +
                "TEAM_ID_FILE=" + currentDir + "/../../../teamID.cpp " +
                "ENTRY_POINT=\"" + entryPoint + "\"";
#else
            String cygwinPath = "C:\\Program Files (x86)\\Autodesk\\Synthesis\\cygwin64";
            String buildTool = "/cygdrive/c/Program Files (x86)/Autodesk/Synthesis/SynthesisDrive/HELBuildTool";
            String makeArgs = "ENTRY_POINT=\"" + entryPoint + "\"";
#endif
            System.Diagnostics.ProcessStartInfo startInfo;
            if (isJava) {
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
            startInfo.EnvironmentVariables["PATH"] = cygwinPath + "\\bin;" + envPath;
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
    }
}
