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

        private void btnClean_Click(object sender, EventArgs e)
        {
            //get the values for the various fields
            String path = txtBrowse.Text;

            //don't do anything if the user hasn't entered a path yet
            if(path == "")
            {
                return;
            }

            //current directory used in debug mode for getting the build
            //directory relative to the HELBuildTool directory
            String currentDir = System.IO.Directory.GetCurrentDirectory();

            String windowsCurrentDir = currentDir;
            //convert currentDir to a cygwin path
            //this pattern will be used a few times for converting between
            //windows style paths and cygwin paths. It's not perfect, but
            //it works pretty well. The one situation in which this could
            //become a problem is if your code is not in `C:\`. This is
            //something that we should look into fixing in the future.
            currentDir = currentDir
                .Replace("C:\\", "/cygdrive/c/").Replace("\\", "/");
            System.IO.Directory.SetCurrentDirectory(path);

            //in debug mode, use the built files relative to the HELBuildTool
            //directory. In release mode, use the ones from the installer.
#if DEBUG
            //path to the cygwin installation
            String cygwinPath = "C:\\cygwin64";
            //path to the build tool directory. `currentDir` is actually the
            //build directory of the build tool, so we need to back up a bit to
            //get to the actual build tool
            String buildTool = currentDir + "/../../../..";
#else
            //path to the cygwin installation
            String cygwinPath =
                "C:\\Program Files (x86)\\Autodesk\\Synthesis\\cygwin64";
            //path to the build tool directory
            String buildTool =
                "/cygdrive/c/Program Files (x86)/Autodesk/Synthesis/" +
                "SynthesisDrive/HELBuildTool";
#endif
            //launch cygwin bash to clean
            System.Diagnostics.ProcessStartInfo startInfo;
            startInfo = new System.Diagnostics.ProcessStartInfo(
                    cygwinPath + "\\bin\\mintty.exe",
                    "'" + cygwinPath + "\\bin\\bash.exe' " +
                    "-c \"mount -c /cygdrive && make -f " +
                    "'" + buildTool + "/Makefile' " +
                    "clean " +
                    "|| read -p 'Press enter to exit'\"");

            //make sure that the cygwin directory is in the PATH
            String envPath = System.Environment.GetEnvironmentVariable("PATH");
            startInfo.EnvironmentVariables["PATH"] =
                cygwinPath + "\\bin;" + envPath;

            startInfo.UseShellExecute = false;

            System.Diagnostics.Process.Start(startInfo);

#if DEBUG
            System.IO.Directory.SetCurrentDirectory(windowsCurrentDir);
#endif
        }

        private void btnRunCode_Click(object sender, EventArgs e)
        {
            //get the values for the various fields
            String number = txtNumber.Text;
            String entryPoint = txtBrowseJava.Text;
            String path = txtBrowse.Text;

            //don't do anything if the user hasn't entered a path yet
            if(path == "" || number == "") {
                return;
            }

            bool isJava = javaButton.Checked;

            //current directory used in debug mode for getting the build
            //directory relative to the HELBuildTool directory
            String currentDir = System.IO.Directory.GetCurrentDirectory();

            String windowsCurrentDir = currentDir;
            //convert currentDir to a cygwin path
            //this pattern will be used a few times for converting between
            //windows style paths and cygwin paths. It's not perfect, but
            //it works pretty well. The one situation in which this could
            //become a problem is if your code is not in `C:\`. This is
            //something that we should look into fixing in the future.
            currentDir = currentDir
                .Replace("C:\\", "/cygdrive/c/").Replace("\\", "/");
            System.IO.Directory.SetCurrentDirectory(path);

            //in debug mode, use the built files relative to the HELBuildTool
            //directory. In release mode, use the ones from the installer.
#if DEBUG
            //path to the cygwin installation
            String cygwinPath = "C:\\cygwin64";
            //path to the build tool directory. `currentDir` is actually the
            //build directory of the build tool, so we need to back up a bit to
            //get to the actual build tool
            String buildTool = currentDir + "/../../../..";
            //java built directory
            String jarDir = currentDir +
                "/../../../../../../emulation/hel/build/java";
            //arguments to pass to make
            String makeArgs =
                "SYNTHESIS_LIBS=" + currentDir +
                    "/../../../../../../emulation/hel/build " +
                "SYNTHESIS_INCLUDES=" + currentDir +
                    "/../../../../../../emulation/hel " +
                "SYNTHESIS_JARS=" + jarDir + " " +
                "TEAM_ID_FILE=" + currentDir +
                    "/../../../../teamID.cpp ";
            
            jarDir = jarDir
                .Replace("/cygdrive/c/", "C:\\").Replace("\\", "/");
#else
            //path to the cygwin installation
            String cygwinPath =
                "C:\\Program Files (x86)\\Autodesk\\Synthesis\\cygwin64";
            //path to the build tool directory
            String buildTool =
                "/cygdrive/c/Program Files (x86)/Autodesk/Synthesis/" +
                "SynthesisDrive/HELBuildTool";
            //arguments to pass to make (there are none in release mode)
            String makeArgs = "";
            //path to the java jars
            String jarDir =
                "/cygdrive/c/Program Files (x86)/Autodesk/Synthesis/" +
                "SynthesisDrive/jars";

            jarDir = jarDir.Replace("/cygdrive/c/", "C:\\").Replace("\\", "/");
#endif
            //launch cygwin bash to compile and run
            System.Diagnostics.ProcessStartInfo startInfo;
            if (isJava) {
                //in java, the makefile also needs to know what the java class
                //that contains the main robot file is
                makeArgs += "ENTRY_POINT=\"" + entryPoint + "\"";

                startInfo = new System.Diagnostics.ProcessStartInfo(
                        //launch the terminal
                        cygwinPath + "\\bin\\mintty.exe",
                        //with bash
                        "'" + cygwinPath + "\\bin\\bash.exe' " +
                        //mount the cygwin directories
                        "-c \"mount -c /cygdrive && " +
                        //run make using the java makefile
                        "make -f " + "'" + buildTool + "/Makefile.java' " +
                        //set the team number env variable
                        "TEAM_ID=" + number + " " +
                        //run the java at the end (java is special, so
                        //the code for this gets moved into the makefile)
                        makeArgs + " run " +
                        //if there is an error, don't close the window
                        "|| read -p 'Press enter to exit'\"");
            }
            else {
                //pretty much the same
                startInfo = new System.Diagnostics.ProcessStartInfo(
                        cygwinPath + "\\bin\\mintty.exe",
                        "'" + cygwinPath + "\\bin\\bash.exe' " +
                        "-c \"mount -c /cygdrive && make -f " +
                        "'" + buildTool + "/Makefile' " +
                        "TEAM_ID=" + number + " " +
                        makeArgs + " " +
                        //C++ is a lot simpler, so we can just run the compiled
                        //executable directly
                        "&& echo 'Starting robot code' &&" +
                        "./build/FRC_UserProgram " +
                        "|| read -p 'Press enter to exit'\"");
            }

            //make sure that the cygwin directory is in the PATH
            String envPath = System.Environment.GetEnvironmentVariable("PATH");
            startInfo.EnvironmentVariables["PATH"] =
                cygwinPath + "\\bin;" + jarDir + envPath;
            //set the team number env variable
            startInfo.EnvironmentVariables["TEAM_ID"] = "" + number;

            startInfo.UseShellExecute = false;

            System.Diagnostics.Process.Start(startInfo);

#if DEBUG
            System.IO.Directory.SetCurrentDirectory(windowsCurrentDir);
#endif
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            //set the user code directory
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
