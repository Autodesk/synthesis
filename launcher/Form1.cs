using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Resources;
using System.Globalization;
using System.Threading;
using System.Reflection;
using System.ComponentModel.Design;
using System.Collections;
using System.Diagnostics;

namespace SynthesisLauncher
{
    public partial class LaunchForm : Form
    {
        const String buildCurrent = "3.1.0.0";
        string exePath = Application.StartupPath;
        public LaunchForm()
        {
            InitializeComponent();
            updateStream();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(1);
        }

        private void synthesis_Click(object sender, EventArgs e)
        {
            //Do button stuff
            String exc = null;
            logText("Starting Synthesis...");

            try {
                Process.Start(exePath + "\\Synthesis\\Synthesis.exe");
                logText("Synthesis Started!");
            }
            catch (Exception f)
            {
                exc = f.ToString();
                logText("Failed to start Synthesis!" + "\n Please report this: \n" + exc);
            }

            this.Focus();
        }
        public void updateStream()
        {
            string streamResults = "Changes in " + buildCurrent +": \n -Improved how PhysX collisions Interact in Simulator \n -Added a Driver Practice mode \n -Greatly Improved the UI \n -Added a New Launcher";
            liveUpdater.Text = streamResults;
            string currentBuild = getCurrentBuild();
            string build = Page_Load();


            buildLabel.Text = currentBuild;
           

            if ((currentBuild == build) && currentBuild != null)
            {
                //write loop with stream reader to read all of the info from the text changelog
                buildLabel.Text = currentBuild;
            }
            else if(((currentBuild != build) && currentBuild != null))
            {
                DialogResult dialogResult = MessageBox.Show("There is an update for this product, would you like to download it?", "Update avaliable", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    //do something
                    Process.Start("http://bxd.autodesk.com/Downloadables/SynthesisBetaInstaller.exe");
                    System.Environment.Exit(1);
                    
                }
                else if (dialogResult == DialogResult.No)
                {
                    //do something else
                }
            }
            else if(((currentBuild != build) && currentBuild == null))
            {
                //User needs a new manifest file because their's is missing or missplaced
                liveUpdater.Text = "Cannot locate your local manifest file which details information about the current build version of Autodesk Synthesis. \nPlease report this to BXD@autodesk.com";
            }
            else
            {
                MessageBox.Show("An error has occured while trying to connect to Autodesk Servers!");
            }
            
        }

        public void logText(string text)
        {
            //for debug purposes
            //TextDisplay.Text = text + "\n" + TextDisplay.Text;
        }

        private void rExporter_Click(object sender, EventArgs e)
        {
            String exc = null;
            logText("Starting Robot Exporter...");

            try
            {
                Process.Start(exePath + "\\RobotExporter\\Inventor_Exporter.exe");
                logText("Robot Exporter Started!");
            }
            catch (Exception f)
            {
                exc = f.ToString();
                logText("Failed to start the Robot exporter!" + "\n Please report this: \n" + exc);
            }
        }

        private void fExporter_Click(object sender, EventArgs e)
        {
            String exc = null;
            logText("Starting Field Exporter...");

            try
            {
                Process.Start(exePath + "\\FieldExporter\\FieldExporter.exe");
                logText("Field Exporter Started!");
            }
            catch (Exception f)
            {
                exc = f.ToString();
                logText("Failed to start the field exporter!" + "\n Please report this: \n" + exc);
            }
        }

        private void codeViewer_Click(object sender, EventArgs e)
        {
            String exc = null;
            logText("Starting Code Viewer...");
            //logText(exePath);
            try
            {
                Process.Start(exePath + "\\SynthesisDrive\\SynthesisDriver.exe");
                //logText(exePath);
                logText("Code Viewer Started!");
            }
            catch (Exception f)
            {
                exc = f.ToString();
                //logText("Failed to start the Code Viewer!" + "\n Please report this: \n" + exc);
            }
        }

        public string getCurrentBuild()
        {
            return buildCurrent;
            /*
            try {
                string line = File.ReadAllText(@"../../Resources/Manifest.txt");
                string[] words = line.Split(' ');
                string buildVersion = words[2];
                //ResXResourceWriter resx = new ResXResourceWriter(@".\CarResources.resx");
                //ResXResourceReader resx = new ResXResourceReader("Resources.resx");
                Console.WriteLine(buildVersion);
                return buildVersion;
            }catch (Exception e)
            {
                logText("Could not read Manifest file!");
                return "error";
            }
            */
        }

        public string Page_Load()
        {
            try {
                WebClient web = new WebClient();
                System.IO.Stream stream = web.OpenRead("http://bxd.autodesk.com/ChangeLog.txt");
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                {
                    String line = reader.ReadToEnd();
                    string[] words = line.Split(' ');
                    logText("Connection to update server successful!" + "\n" + "Current Release Build Number is " + words[2]);
                    return words[2];
                }
            }catch(Exception e)
            {
                logText("Connection to the remote server for a update was not successful!");
                return null;
            }
            //string userscore = doc.DocumentNode.SelectNodes("//*[@id=\"main\"]/div[3]/div/div[2]/div[1]/div[2]/div[1]/div/div[2]/a/span[1]")[0].InnerText;
            //string summary = doc.DocumentNode.SelectNodes("//*[@id=\"main\"]/div[3]/div/div[2]/div[2]/div[1]/ul/li/span[2]/span/span[1]")[0].InnerText;
        }

        private void robotExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://bxd.autodesk.com/?page=tutorialRobotExporter");
        }

        private void fieldExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://bxd.autodesk.com/?page=tutorialFieldExporter");
        }

        private void jointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://bxd.autodesk.com/?page=tutorialJoints");
        }

        private void javaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://bxd.autodesk.com/?page=tutorialCompileJava");
        }

        private void driverstationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://bxd.autodesk.com/?page=tutorialDriverStation");
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
