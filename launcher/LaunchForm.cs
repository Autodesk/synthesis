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
        const String buildCurrent = "3.2.0.0";
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
            try
            {
                Process.Start(exePath + "\\Synthesis\\Synthesis.exe");
            }
            catch
            {
                MessageBox.Show(this, "Couldn't start Synthesis!");
            }

            Focus();
        }

        private void synthesisBulletButton_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(exePath + "\\Synthesis\\Synthesis_Bullet.exe");
            }
            catch
            {
                MessageBox.Show(this, "Couldn't start Synthesis with Bullet Physics!");
            }
        }

        public void updateStream()
        {
            string streamResults = "Changes in " + buildCurrent +
                ": \n -Added joystick support with new input manager" +
                ": \n -Reworked simulator user interface to improve useability" +
                ": \n -Added robot camera and indicator" +
                ": \n -Added toolkit features with ruler and stopwatch";
            liveUpdater.Text = streamResults;
            string build = Page_Load();

            buildLabel.Text = buildCurrent;

            if (build == null)
                return;

            if (buildCurrent.Equals(build))
            {
                //write loop with stream reader to read all of the info from the text changelog
                buildLabel.Text = buildCurrent;
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show("There is an update for this product, would you like to download it?", "Update avaliable", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    //do something
                    Process.Start("http://bxd.autodesk.com/Downloadables/Synthesis%20Installer.exe");
                    System.Environment.Exit(1);
                }
            }
        }

        private void rExporter_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(exePath + "\\RobotExporter\\Inventor_Exporter.exe");
            }
            catch
            {
                MessageBox.Show(this, "Failed to start the Robot exporter!");
            }
        }

        private void fExporter_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(exePath + "\\FieldExporter\\FieldExporter.exe");
            }
            catch
            {
                MessageBox.Show(this, "Failed to start the field exporter!");
            }
        }

        private void codeViewer_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(exePath + "\\SynthesisDrive\\HELBuildTool\\HELBuildTool.exe");
            }
            catch
            {
                MessageBox.Show(this, "Failed to start the Code Viewer!");
            }
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
                    return words[2];
                }
            }
            catch
            {
                MessageBox.Show(this, "Connection to the remote server for a update was not successful!");
                return null;
            }
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


        private void LaunchForm_Load(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void liveUpdater_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
