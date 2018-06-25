using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Inventor;
using System.Threading;

public partial class LiteExporterForm : Form
{
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

    public LiteExporterForm()
    {
        InitializeComponent();
        LoadingAnimation.Image = JointResolver.Properties.Resources.LoadAnimation;
        Instance = this;
        LoadingAnimation.WaitOnLoad = true;
        ExporterWorker.WorkerReportsProgress = true;
        ExporterWorker.WorkerSupportsCancellation = true;
        ExporterWorker.DoWork += ExporterWorker_DoWork;
        ExporterWorker.RunWorkerCompleted += ExporterWorker_RunWorkerCompleted;

        FormClosing += delegate (object sender, FormClosingEventArgs e)
        {
            InventorManager.Instance.UserInterfaceManager.UserInteractionDisabled = false;
        };
        Shown += delegate (object sender, EventArgs e)
        {
            Exporting = true;
            OnStartExport();
            ExporterWorker.RunWorkerAsync();
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
        
        RigidNode_Base Skeleton = ExportSkeleteonLite(InventorManager.Instance.ComponentOccurrences.OfType<ComponentOccurrence>().ToList());

        List<BXDAMesh> Meshes = ExportMeshesLite(Skeleton);

        SynthesisGUI.Instance.Meshes = Meshes;
        SynthesisGUI.Instance.SkeletonBase = Skeleton;
    }

    private void ExitButton_Click(object sender, EventArgs e)
    {
        if (ExporterWorker.IsBusy)
            ExporterWorker.CancelAsync();
        Close();
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
            #region DEBUG SWITCH
#if DEBUG
            MessageBox.Show(e.Error.ToString());
#else
            MessageBox.Show(e.Error.Message);
#endif
        } 
        #endregion
        else
        {
            ProgressLabel.Text = "Export Completed Successfully";
            Close();
        }
    }

    /// <summary>
    /// The lightweight equivalent of the 'Add From Inventor' button in the <see cref="ExporterForm"/>. Used in <see cref="ExportMeshesLite(RigidNode_Base)"/>
    /// </summary>
    /// <param name="occurrences"></param>
    /// <returns></returns>
    public RigidNode_Base ExportSkeleteonLite(List<ComponentOccurrence> occurrences)
    {
        if (occurrences.Count == 0)
        {
            throw new ArgumentException("ERROR: 0 Occurrences passed to ExportSkeletonLite", "occurrences");
        }

        #region CenterJoints
        int NumCentered = 0;

        LiteExporterForm.Instance.SetProgress(NumCentered, occurrences.Count, "Centering Joints");
        foreach (ComponentOccurrence component in occurrences)
        {
            Exporter.CenterAllJoints(component);
            NumCentered++;
            LiteExporterForm.Instance.SetProgress(NumCentered, occurrences.Count);
        }
        #endregion

        #region Build Models
        //Getting Rigid Body Info...
        LiteExporterForm.Instance.SetProgress("Getting Rigid Body Info...");
        NameValueMap RigidGetOptions = InventorManager.Instance.TransientObjects.CreateNameValueMap();

        RigidGetOptions.Add("DoubleBearing", false);
        RigidBodyResults RawRigidResults = InventorManager.Instance.AssemblyDocument.ComponentDefinition.RigidBodyAnalysis(RigidGetOptions);
        
        CustomRigidResults RigidResults = new CustomRigidResults(RawRigidResults);


        //Building Model...
        LiteExporterForm.Instance.SetProgress("Building Model...");
        RigidBodyCleaner.CleanGroundedBodies(RigidResults);
        RigidNode baseNode = RigidBodyCleaner.BuildAndCleanDijkstra(RigidResults);
#endregion

        #region Cleaning Up
        //Cleaning Up...
        LiteExporterForm.Instance.SetProgress("Cleaning Up...");
        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        baseNode.ListAllNodes(nodes);

        foreach (RigidNode_Base node in nodes)
        {
            node.ModelFileName = ((RigidNode)node).group.ToString();
            node.ModelFullID = node.GetModelID();
        }
#endregion
        return baseNode;
    }

    /// <summary>
    /// The lite equivalent of the 'Start Exporter' <see cref="Button"/> in the <see cref="ExporterForm"/>. Used in <see cref="ExporterWorker_DoWork(Object, "/>
    /// </summary>
    /// <seealso cref="ExporterWorker_DoWork"/>
    /// <param name="baseNode"></param>
    /// <returns></returns>
    public List<BXDAMesh> ExportMeshesLite(RigidNode_Base baseNode)
    {
        SurfaceExporter surfs = new SurfaceExporter();
        BXDJSkeleton.SetupFileNames(baseNode, true);

        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        baseNode.ListAllNodes(nodes);

        List<BXDAMesh> meshes = new List<BXDAMesh>();

        SetProgress(0, "Exporting Parts");

        for (int i = 0; i < nodes.Count; i++)
        {
            RigidNode_Base node = nodes[i];

            if (node is RigidNode && node.GetModel() != null && node.ModelFileName != null && node.GetModel() is CustomRigidGroup)
            {
                try
                {
                    CustomRigidGroup group = (CustomRigidGroup)node.GetModel();
                    surfs.Reset(node.GUID);
                    surfs.ExportAll(group, (long progress, long total) =>
                    {
                        SetProgress((double) progress / total / nodes.Count + (double) i / nodes.Count);
                    });
                    BXDAMesh output = surfs.GetOutput();
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

        return meshes;
    }
}