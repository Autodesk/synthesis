using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BxDRobotExporter.Exporter;
using BxDRobotExporter.Managers;
using BxDRobotExporter.RigidAnalyzer;
using Inventor;

namespace BxDRobotExporter.ControlGUI
{
    public partial class LoadingSkeletonForm : Form
    {
        private readonly RobotDataManager robotDataManager;

        public LoadingSkeletonForm(RobotDataManager robotDataManager)
        {
            this.robotDataManager = robotDataManager;
            InitializeComponent();

            FormClosing += delegate (object sender, FormClosingEventArgs e)
            {
                InventorManager.Instance.UserInterfaceManager.UserInteractionDisabled = false;
            };
            Shown += delegate (object sender, EventArgs e)
            {
                ExporterWorker.RunWorkerAsync();
            };
        }

        private void SetProgress(string message, int current, int max)
        {
            // Allows function to be called by other threads
            if (InvokeRequired)
            {
                BeginInvoke((Action<string, int, int>)SetProgress, message, current, max);
                return;
            }

            ProgressLabel.Text = message;
            ProgressBar.Maximum = max;
            ProgressBar.Value = current;
        }
        
        // Hide function cannot be run by worker
        private void SetProgressWindowVisisble(bool v)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action<bool>)SetProgressWindowVisisble, v);
                return;
            }

            Visible = v;
        }

        private void ExporterWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            if (InventorManager.Instance == null)
            {
                MessageBox.Show("Couldn't detect a running instance of Inventor.");
                return;
            }

            if (InventorManager.Instance.ActiveDocument == null || !(InventorManager.Instance.ActiveDocument is AssemblyDocument))
            {
                MessageBox.Show("Couldn't detect an open assembly");
                return;
            }

            InventorManager.Instance.UserInterfaceManager.UserInteractionDisabled = true;

            RigidNode_Base skeleton = null;

            try
            {
                skeleton = ExportSkeleton(InventorManager.Instance.ComponentOccurrences.OfType<ComponentOccurrence>().ToList());
            }
            catch (Exporter.Exporter.EmptyAssemblyException)
            {
                SetProgressWindowVisisble(false);

                string caption = "Empty Assembly";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult r = MessageBox.Show("Assembly has no parts to export.", caption, buttons);
            }
            catch (Exporter.Exporter.InvalidJointException ex)
            {
                SetProgressWindowVisisble(false);

                string caption = "Invalid Joint";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult r = MessageBox.Show(ex.Message, caption, buttons);
            }
            catch (Exporter.Exporter.NoGroundException)
            {
                SetProgressWindowVisisble(false);

                string caption = "No Ground";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult r = MessageBox.Show("Please ground a part in your assembly to export your robot.", caption, buttons);
            }
            finally
            {
                robotDataManager.RobotBaseNode = skeleton;
            }
        }

        /// <summary>
        /// The lightweight equivalent of the 'Add From Inventor' button in the <see cref="ExporterForm"/>. Used in <see cref="ExportMeshesLite(RigidNode_Base)"/>
        /// </summary>
        /// <param name="occurrences"></param>
        /// <returns></returns>
        public RigidNode_Base ExportSkeleton(List<ComponentOccurrence> occurrences)
        {
            if (occurrences.Count == 0)
            {
                throw new Exporter.Exporter.EmptyAssemblyException();
            }

            //Getting Rigid Body Info...
            SetProgress("Getting physics info...", occurrences.Count, occurrences.Count + 3);
            NameValueMap rigidGetOptions = InventorManager.Instance.TransientObjects.CreateNameValueMap();

            rigidGetOptions.Add("DoubleBearing", false);
            RigidBodyResults rawRigidResults = InventorManager.Instance.AssemblyDocument.ComponentDefinition.RigidBodyAnalysis(rigidGetOptions);

            //Getting Rigid Body Info...Done
            CustomRigidResults rigidResults = new CustomRigidResults(rawRigidResults);


            //Building Model...
            SetProgress("Building model...", occurrences.Count + 1, occurrences.Count + 3);
            RigidBodyCleaner.CleanGroundedBodies(rigidResults);
            RigidNode baseNode = RigidBodyCleaner.BuildAndCleanDijkstra(rigidResults);

            //Building Model...Done

            //Cleaning Up...
            SetProgress("Cleaning up...", occurrences.Count + 2, occurrences.Count + 3);
            List<RigidNode_Base> nodes = new List<RigidNode_Base>();
            baseNode.ListAllNodes(nodes);

            foreach (RigidNode_Base node in nodes)
            {
                node.ModelFileName = ((RigidNode)node).group.ToString();
                node.ModelFullID = node.GetModelID();
            }
            //Cleaning Up...Done
            SetProgress("Done", occurrences.Count + 3, occurrences.Count + 3);
            return baseNode;
        }

        private void ExporterWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Close();
        }
    }
}
