using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using BxDRobotExporter.Exporter;
using BxDRobotExporter.OGLViewer;
using BxDRobotExporter.RigidAnalyzer;
using Inventor;

namespace BxDRobotExporter.ControlGUI
{
    public partial class LiteExporterForm : Form
    {
        private readonly RobotData robotData;
        public bool Exporting = false;

        public event EventHandler StartExport;
        public void OnStartExport()
        {
            StartExport?.Invoke(this, null);
        }

        public event EventHandler CancelExport;
        public void OnCancelExport()
        {
            CancelExport?.Invoke(this, null);
        }

        public static LiteExporterForm Instance;

        public LiteExporterForm(RobotData robotData)
        {
            this.robotData = robotData;
            InitializeComponent();
            Instance = this;
            LoadingAnimation.WaitOnLoad = true;
            ExporterWorker.WorkerReportsProgress = true;
            ExporterWorker.WorkerSupportsCancellation = true;
            ExporterWorker.DoWork += ExporterWorker_DoWork;
            ExporterWorker.RunWorkerCompleted += ExporterWorker_RunWorkerCompleted;

            Shown += delegate (object sender, EventArgs e)
            {
                if (InventorManager.Instance == null)
                {
                    MessageBox.Show("Couldn't detect a running instance of Inventor.");
                    return;
                }

                InventorManager.Instance.UserInterfaceManager.UserInteractionDisabled = true;

                Exporting = true;
                OnStartExport();
                ExporterWorker.RunWorkerAsync();
            };

            FormClosing += delegate (object sender, FormClosingEventArgs e)
            {
                InventorManager.Instance.UserInterfaceManager.UserInteractionDisabled = false;
            };
        }

        /// <summary>
        /// Updates the progress bar with an unknown state of progress, displaying a specific message.
        /// </summary>
        /// <param name="message">Message to display next to progress bar.</param>
        public void SetProgress(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action<string>)SetProgress, message);
                return;
            }

            ProgressLabel.Text = message;
            ProgressBar.Style = ProgressBarStyle.Marquee;
        }

        /// <summary>
        /// Updates the progress bar with a specific state (i.e. 5/10 complete) and message (i.e. "Building model...").
        /// </summary>
        /// <param name="current">Current progress.</param>
        /// <param name="max">Maximum value for progress (what it will be when the process is complete). Uses previous value if less than 0.</param> 
        /// <param name="message">Message to display next to progress bar. Does not change text if message is null.</param>
        public void SetProgress(int current, int max = -1, string message = null)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action<int, int, string>)SetProgress, current, max, message);
                return;
            }

            if (message != null)
                ProgressLabel.Text = message;

            ProgressBar.Style = ProgressBarStyle.Continuous;

            if (max >= 0)
                ProgressBar.Maximum = max;

            if (current <= ProgressBar.Maximum)
                ProgressBar.Value = current;
            else
                ProgressBar.Value = ProgressBar.Maximum;
        }

        /// <summary>
        /// Updates the progress bar with a specific state (i.e. 50% complete) and message (i.e. "Building model...").
        /// </summary>
        /// <param name="current">Current progress as a percent (0 to 1).</param>
        /// <param name="message">Message to display next to progress bar. Does not change text if message is null.</param>
        public void SetProgress(double current, string message = null)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action<double, string>)SetProgress, current, message);
                return;
            }

            if (message != null)
                ProgressLabel.Text = message;
            ProgressBar.Style = ProgressBarStyle.Continuous;
            ProgressBar.Maximum = 10000;
            ProgressBar.Value = (int) (current * 10000);
        }

        /// <summary>
        /// Executes the functions that export the robot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExporterWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (InventorManager.Instance.ActiveDocument == null || !(InventorManager.Instance.ActiveDocument is AssemblyDocument))
            {
                MessageBox.Show("Couldn't detect an open assembly");
                return;
            }

            if (robotData.SkeletonBase == null)
                return; // Skeleton has not been built

            List<BXDAMesh> meshes = ExportMeshesLite(robotData.SkeletonBase, robotData.Settings.TotalWeightKg);

            robotData.Meshes = meshes;
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            if (ExporterWorker.IsBusy)
                ExporterWorker.CancelAsync();
        
            if (ExporterWorker.CancellationPending)
                Dispose();
        }

        private void ExporterWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Exporting = false;

            if (e.Cancelled)
                ProgressLabel.Text = "Export Cancelled";
            else if (e.Error != null)
            {
                ProgressLabel.Text = "An error occurred.";
#if DEBUG
                MessageBox.Show(e.Error.ToString());
#else
            MessageBox.Show(e.Error.Message);
#endif
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        /// <summary>
        /// The lite equivalent of the 'Start Exporter' <see cref="Button"/> in the <see cref="ExporterForm"/>. Used in <see cref="ExporterWorker_DoWork(Object, "/>
        /// </summary>
        /// <seealso cref="ExporterWorker_DoWork"/>
        /// <param name="baseNode"></param>
        /// <returns></returns>
        public List<BXDAMesh> ExportMeshesLite(RigidNode_Base baseNode, float totalMassKg)
        {
            SurfaceExporter surfs = new SurfaceExporter();
            BXDJSkeleton.SetupFileNames(baseNode);

            List<RigidNode_Base> nodes = new List<RigidNode_Base>();
            baseNode.ListAllNodes(nodes);

            List<BXDAMesh> meshes = new List<BXDAMesh>();

            SetProgress(0, "Exporting Model");

            for (int i = 0; i < nodes.Count; i++)
            {
                RigidNode_Base node = nodes[i];

                if (node is RigidNode && node.GetModel() != null && node.ModelFileName != null && node.GetModel() is CustomRigidGroup)
                {
                    try
                    {
                        CustomRigidGroup group = (CustomRigidGroup)node.GetModel();

                        BXDAMesh output = surfs.ExportAll(group, node.GUID, (long progress, long total) =>
                        {
                            SetProgress(((double)progress / total) / nodes.Count + (double)i / nodes.Count);
                        });
                    
                        output.colliders.Clear();
                        output.colliders.AddRange(ConvexHullCalculator.GetHull(output));

                        meshes.Add(output);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error exporting mesh: " + node.GetModelID(), e);
                    }
                }
            }

            // Apply custom mass to mesh
            if (totalMassKg > 0) // Negative value indicates that default mass should be left alone (TODO: Make default mass more accurate)
            {
                float totalDefaultMass = 0;
                foreach (BXDAMesh mesh in meshes)
                {
                    totalDefaultMass += mesh.physics.mass;
                }
                for (int i = 0; i < meshes.Count; i++)
                {
                    meshes[i].physics.mass = totalMassKg * (float)(meshes[i].physics.mass / totalDefaultMass);
                }
            }

            // Add meshes to all nodes
            for (int i = 0; i < meshes.Count; i++)
            {
                ((OglRigidNode)nodes[i]).LoadMeshes(meshes[i]);
            }

            // Get wheel information (radius, center, etc.) for all wheels
            foreach (RigidNode_Base node in nodes)
            {
                SkeletalJoint_Base joint = node.GetSkeletalJoint();

                // Joint will be null if the node has no connection.
                // cDriver will be null if there is no driver connected to the joint.
                if (joint != null && joint.cDriver != null)
                {
                    WheelDriverMeta wheelDriver = (WheelDriverMeta)joint.cDriver.GetInfo(typeof(WheelDriverMeta));

                    // Drivers without wheel metadata do not need radius, center, or width info.
                    if (wheelDriver != null)
                    {
                        (node as OGLViewer.OglRigidNode).GetWheelInfo(out float radius, out float width, out BXDVector3 center);
                        wheelDriver.radius = radius;
                        wheelDriver.center = center;
                        wheelDriver.width = width;

                        joint.cDriver.AddInfo(wheelDriver);
                    }
                }
            }

            return meshes;
        }
    }
}