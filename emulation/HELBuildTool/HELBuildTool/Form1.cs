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
        OpenFileDialog openFileDialog;

        public Form1()
        {
            openFileDialog = new OpenFileDialog();
            InitializeComponent();
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
            String path = txtBrowse.Text;

            Console.WriteLine(path);
            String currentDir = System.IO.Directory.GetCurrentDirectory();
            String windowsCurrentDir = currentDir;
            //convert currentDir to a cygwin path
            currentDir = currentDir.Replace("C:\\", "/cygdrive/c/").Replace("\\", "/");
            System.IO.Directory.SetCurrentDirectory(path);
            Console.WriteLine(currentDir);

#if DEBUG
            String cygwinPath = "C:\\cygwin64";
            String buildTool = currentDir + "/../../..";
            String makeArgs = "SYNTHESIS_LIBS=" + currentDir + "/../../../../../emulation/hel/build " +
              "SYNTHESIS_INCLUDES=" + currentDir + "/../../../../../emulation/hel/ " +
              "TEAM_ID_FILE=" + currentDir + "/../../../teamID.cpp";
#else
            String cygwinPath = "C:\\Program Files (x86)\\Autodesk\\Synthesis\\cygwin64";
            String buildTool = "/cygdrive/c/Program Files (x86)/Autodesk/Synthesis/SynthesisDrive/HELBuildTool";
            String makeArgs = "";
#endif
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(
                cygwinPath + "\\bin\\mintty.exe",
                "'" + cygwinPath + "\\bin\\bash.exe' " +
                "-c \"mount -c /cygdrive && make -f " +
                "'" + buildTool + "/Makefile' " +
                "TEAM_ID=" + number + " " +
                makeArgs + " " +
                "&& echo 'Starting robot code' && ./build/FRC_UserProgram " +
                "|| read -p 'Press enter to continue'\"");
            startInfo.EnvironmentVariables["PATH"] = cygwinPath + "\\bin";
            startInfo.UseShellExecute = false;

            System.Diagnostics.Process.Start(startInfo);
            System.IO.Directory.SetCurrentDirectory(windowsCurrentDir);
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {
            //openFileDialog.ShowDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
              txtBrowse.Text = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
