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

public enum ProgressTextType { Normal, ShortTaskBegin, ShortTaskEnd }

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

    public void SetProgressText(string text, ProgressTextType ProgressType = ProgressTextType.Normal)
    {
        if(InvokeRequired)
        {
            BeginInvoke((Action<string, ProgressTextType>)SetProgressText, text, ProgressType);
            return;
        }
        switch (ProgressType)
        {
            case ProgressTextType.Normal:
                text = text ?? "";
                ProgressLabel.Text = "Progress: " + text;
                break;
            case ProgressTextType.ShortTaskBegin:
                ProgressLabel.Text = "Progress: " + text;
                break;
            case ProgressTextType.ShortTaskEnd:
                text = text ?? "Done";
                ProgressLabel.Text += text;
                break;
        }

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

        SetProgressText(string.Format("Centering Joints {0} / {1}", NumCentered, occurrences.Count));
        foreach (ComponentOccurrence component in occurrences)
        {
            Exporter.CenterAllJoints(component);
            NumCentered++;
            SetProgressText(string.Format("Centering Joints {0} / {1}", NumCentered, occurrences.Count));
        }
#endregion

        #region Build Models
        //Getting Rigid Body Info...
        SetProgressText("Getting Rigid Body Info...", ProgressTextType.ShortTaskBegin);
        NameValueMap RigidGetOptions = InventorManager.Instance.TransientObjects.CreateNameValueMap();

        RigidGetOptions.Add("DoubleBearing", false);
        RigidBodyResults RawRigidResults = InventorManager.Instance.AssemblyDocument.ComponentDefinition.RigidBodyAnalysis(RigidGetOptions);

        //Getting Rigid Body Info...Done
        SetProgressText(null, ProgressTextType.ShortTaskEnd);
        CustomRigidResults RigidResults = new CustomRigidResults(RawRigidResults);


        //Building Model...
        SetProgressText("Building Model...", ProgressTextType.ShortTaskBegin);
        RigidBodyCleaner.CleanGroundedBodies(RigidResults);
        RigidNode baseNode = RigidBodyCleaner.BuildAndCleanDijkstra(RigidResults);

        //Building Model...Done
        SetProgressText(null, ProgressTextType.ShortTaskEnd);
#endregion

        #region Cleaning Up
        //Cleaning Up...
        LiteExporterForm.Instance.SetProgressText("Cleaning Up...", ProgressTextType.ShortTaskBegin);
        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        baseNode.ListAllNodes(nodes);

        foreach (RigidNode_Base node in nodes)
        {
            node.ModelFileName = ((RigidNode)node).group.ToString();
            node.ModelFullID = node.GetModelID();
        }
        //Cleaning Up...Done
        LiteExporterForm.Instance.SetProgressText(null, ProgressTextType.ShortTaskEnd);
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

        foreach (RigidNode_Base node in nodes)
        {
            SetProgressText("Exporting " + node.ModelFileName);

            if (node is RigidNode && node.GetModel() != null && node.ModelFileName != null && node.GetModel() is CustomRigidGroup)
            {
                try
                {
                    CustomRigidGroup group = (CustomRigidGroup)node.GetModel();
                    surfs.Reset(node.GUID);
                    surfs.ExportAll(group, (long progress, long total) =>
                    {
                        SetProgressText(String.Format("Export {0} / {1}", progress, total));
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