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
using System.Text.RegularExpressions;

namespace SynthesisLauncher
{
    public partial class LaunchForm : Form
    {
        readonly string asmVersion = Assembly.GetEntryAssembly().GetName().Version.ToString(4);
        string webVersion;

        string exePath = Application.StartupPath;
        public LaunchForm()
        {
            InitializeComponent();
            GetChanges();

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
            catch(Exception ex)
            {
                #region DEBUG SWITCH
#if DEBUG
                MessageBox.Show(ex.ToString());
#else
                MessageBox.Show(this, "Couldn't start Synthesis with Bullet Physics!\n" + ex.Message);
#endif 
                #endregion
            }
        }

        /// <summary>
        /// Queries http://bxd.autodesk.com/ChangeLog.txt to get the current changelog
        /// </summary>
        public void GetChanges()
        {
            try
            {
                WebClient client = new WebClient { BaseAddress = "http://bxd.autodesk.com/Downloadables/" };
                buildLabel.Text = AssemblyName.GetAssemblyName("SynthesisLauncher.exe").Version.ToString(4);

                string update = "";

                using (var reader = new StreamReader(new MemoryStream(client.DownloadData("ChangeLog.txt"))))
                {
                    string first = reader.ReadLine();
                    update = first + "\n";
                    Regex versionRegex = new Regex("[0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+");
                    var match = versionRegex.Match(first);
                    if (match.Success)
                    {
                        webVersion = match.Value;
                    }
                    if (webVersion != asmVersion)
                    {
                        if (MessageBox.Show("There is an update for this product, would you like to download it?",
                            "Update avaliable", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            //Get the installer from the server and run it.
                            Process.Start("http://bxd.autodesk.com/Downloadables/Synthesis%20Installer.exe");
                            Environment.Exit(1);
                        }
                    }
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            update += line + "\n";
                        }
                        else
                            break;
                    }
                }
                updateLabel.Text = update;
                int lineCount = update.Count(x => x == '\n');
                Size = new Size(Size.Width, Size.Height + (13 * (lineCount - 1)));
            }
            catch { }
        }

        public void updateStream()
        {
            try
            {
                string streamResults = "Changes in " + //BuildCurrent +
              ": \n -Added support for local multiplayer" +
              ": \n -Greatly improved driving physics" +
              ": \n -Added new exporter Inventor plugins" +
              ": \n -Revamped UI" +
              ": \n -New field and robot deliver system" +
              ": \n -Added sensor support";
                buildLabel.Text = streamResults;
                string build = Page_Load();


                if (build == null)
                    return;

                if (asmVersion.Equals(build))
                {
                    //Write loop with stream reader to read all of the info from the text changelog.
                    buildLabel.Text = asmVersion;
                }
                else
                {
                    DialogResult dialogResult = MessageBox.Show("There is an update for this product, would you like to download it?", "Update avaliable", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        //Get the installer from the server and run it.
                        Process.Start("http://bxd.autodesk.com/Downloadables/Synthesis%20Installer.exe");
                        Environment.Exit(1);
                    }
                }
            }
            catch { }
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
            Process.Start("http://bxd.autodesk.com/tutorial-robot.html");
        }

        private void fieldExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://bxd.autodesk.com/tutorial-field.html");
        }

        private void jointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://bxd.autodesk.com/tutorial-sim.html");
        }

        private void emulatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://bxd.autodesk.com/tutorial-driverstation.html");
        }

        private void driverstationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://bxd.autodesk.com/?page=tutorialDriverStation");
        }

        private void changesLabel_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
