using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EditorsLibrary;
using JointResolver.ControlGUI;
using OGLViewer;

public delegate bool ValidationAction(RigidNode_Base baseNode, out string message);

[Guid("ec18f8d4-c13e-4c86-8148-7414efb6e1e2")]
public partial class SynthesisGUI : Form
{
    //public event Action ExportFinished;
    //public void OnExportFinished()
    //{
    //    ExportFinished.Invoke();
    //}

    public class RuntimeMeta
    {
        public bool UseSettingsDir;
        public string ActiveDir;
        public string ActiveRobotName;

        public static RuntimeMeta CreateRuntimeMeta()
        {
            return new RuntimeMeta
            {
                UseSettingsDir = true,
                ActiveDir = null,
                ActiveRobotName = null
            };
        }
    }

    public RuntimeMeta RMeta = RuntimeMeta.CreateRuntimeMeta();

    public static SynthesisGUI Instance;

    public static PluginSettingsForm.PluginSettingsValues PluginSettings;

    public Form BXDAViewerPaneForm = new Form
    {
        FormBorderStyle = FormBorderStyle.None
    };
    public Form JointPaneForm = new Form
    {
        FormBorderStyle = FormBorderStyle.None
    };

    public RigidNode_Base SkeletonBase = null;
    public List<BXDAMesh> Meshes = null;

    private LiteExporterForm liteExporter;

    static SynthesisGUI()
    {
    }

    public SynthesisGUI(bool MakeOwners = false)
    {
        InitializeComponent();

        Instance = this;

        bxdaEditorPane1.Units = "lbs";
        BXDAViewerPaneForm.Controls.Add(bxdaEditorPane1);
        if (MakeOwners) BXDAViewerPaneForm.Owner = this;
        BXDAViewerPaneForm.FormClosing += Generic_FormClosing;

        JointPaneForm.Controls.Add(jointEditorPane1);
        if (MakeOwners) JointPaneForm.Owner = this;
        JointPaneForm.FormClosing += Generic_FormClosing;


        RigidNode_Base.NODE_FACTORY = delegate (Guid guid)
        {
            return new OGL_RigidNode(guid);
        };

        settingsExporter.Click += SettingsExporter_OnClick;

        Shown += SynthesisGUI_Shown;

        FormClosing += new FormClosingEventHandler(delegate (object sender, FormClosingEventArgs e)
        {
            if (SkeletonBase != null && !WarnUnsaved()) e.Cancel = true;
            InventorManager.ReleaseInventor();
        });

        jointEditorPane1.ModifiedJoint += delegate (List<RigidNode_Base> nodes)
        {

            if (nodes == null || nodes.Count == 0) return;

            foreach (RigidNode_Base node in nodes)
            {
                if (node.GetSkeletalJoint() != null && node.GetSkeletalJoint().cDriver != null &&
                    node.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>() != null &&
                    node.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>().radius == 0 &&
                    node is OGL_RigidNode)
                {
                    (node as OGL_RigidNode).GetWheelInfo(out float radius, out float width, out BXDVector3 center);

                    WheelDriverMeta wheelDriver = node.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>();
                    wheelDriver.center = center;
                    wheelDriver.radius = radius;
                    wheelDriver.width = width;
                    node.GetSkeletalJoint().cDriver.AddInfo(wheelDriver);

                }
            }
        };

        jointEditorPane1.SelectedJoint += bxdaEditorPane1.SelectJoints;

        bxdaEditorPane1.NodeSelected += (BXDAMesh mesh) =>
            {
                List<RigidNode_Base> nodes = new List<RigidNode_Base>();
                SkeletonBase.ListAllNodes(nodes);

                jointEditorPane1.AddSelection(nodes[Meshes.IndexOf(mesh)], true);
            };
    }

    private void Generic_FormClosing(object sender, FormClosingEventArgs e)
    {
        foreach (Form f in OwnedForms)
        {
            if(f.Visible)
                f.Close();
        }
        Close();
    }

    private void SynthesisGUI_Shown(object sender, EventArgs e)
    {
        Hide();
        BXDAViewerPaneForm.Show();
        JointPaneForm.Show();
    }

    public void SetNew()
    {
        if (SkeletonBase == null || !WarnUnsaved()) return;

        SkeletonBase = null;
        Meshes = null;
        ReloadPanels();
    }

    /// <summary>
    /// Export a robot from Inventor
    /// </summary>
    public bool ExportMeshes(bool warnUnsaved = false)
    {
        if (SkeletonBase != null && warnUnsaved && !WarnUnsaved()) return false;

        try
        {
            var exporterThread = new Thread(() =>
            {
#if LITEMODE
                liteExporter = new LiteExporterForm();
                liteExporter.ShowDialog();
#else
                exporter = new ExporterForm(PluginSettings);
                exporter.ShowDialog();
#endif
            });

            exporterThread.SetApartmentState(ApartmentState.STA);
            exporterThread.Start();

            exporterThread.Join();

            GC.Collect();
        }
        catch (InvalidComObjectException)
        {
        }
        catch (TaskCanceledException)
        {
            return true;
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
            return false;
        }

        List<RigidNode_Base> nodes = SkeletonBase.ListAllNodes();
        for (int i = 0; i < Meshes.Count; i++)
        {
            ((OGL_RigidNode)nodes[i]).loadMeshes(Meshes[i]);
        }
        RobotSaveAs(NameRobotForm.NameMode.Initial);

        ReloadPanels();
        return true;
    }

    /// <summary>
    /// Open a previously exported robot. 
    /// </summary>
    /// <param name="validate">If it is not null, this will validate the open inventor assembly.</param>
    public void OpenExisting()
    {
        if (SkeletonBase != null && !WarnUnsaved()) return;

        string dirPath = OpenFolderPath();

        if (dirPath == null) return;

        try
        {
            List<RigidNode_Base> nodes = new List<RigidNode_Base>();
            SkeletonBase = BXDJSkeleton.ReadSkeleton(dirPath + "\\skeleton.bxdj");

            SkeletonBase.ListAllNodes(nodes);

            Meshes = new List<BXDAMesh>();

            foreach (RigidNode_Base n in nodes)
            {
                BXDAMesh mesh = new BXDAMesh();
                mesh.ReadFromFile(dirPath + "\\" + n.ModelFileName);

                if (!n.GUID.Equals(mesh.GUID))
                {
                    MessageBox.Show(n.ModelFileName + " has been modified.", "Could not load mesh.");
                    return;
                }

                Meshes.Add(mesh);
            }
            for (int i = 0; i < Meshes.Count; i++)
            {
                ((OGL_RigidNode)nodes[i]).loadMeshes(Meshes[i]);
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }


        ReloadPanels();
    }

    /// <summary>
    /// Open a previously exported robot. 
    /// </summary>
    /// <param name="validate">If it is not null, this will validate the open inventor assembly.</param>
    public bool OpenExisting(ValidationAction validate, bool warnUnsaved = false)
    {

        if (SkeletonBase != null && warnUnsaved && !WarnUnsaved()) return false;

        string dirPath = OpenFolderPath();

        if (dirPath == null) return false;

        try
        {
            List<RigidNode_Base> nodes = new List<RigidNode_Base>();
            SkeletonBase = BXDJSkeleton.ReadSkeleton(dirPath + "\\skeleton.bxdj");

            if (validate != null)
            {
                if (!validate(SkeletonBase, out string message))
                {
                    while (true)
                    {
                        DialogResult result = MessageBox.Show(message, "Assembly Validation", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Retry)
                            continue;
                        if (result == DialogResult.Abort)
                        {
                            return false;
                        }
                        break;
                    }
                }
                #region DEBUG
#if DEBUG
                else
                {
                    MessageBox.Show(message);
                }
#endif 
                #endregion
            }

            SkeletonBase.ListAllNodes(nodes);

            Meshes = new List<BXDAMesh>();

            foreach (RigidNode_Base n in nodes)
            {
                BXDAMesh mesh = new BXDAMesh();
                mesh.ReadFromFile(dirPath + "\\" + n.ModelFileName);

                if (!n.GUID.Equals(mesh.GUID))
                {
                    MessageBox.Show(n.ModelFileName + " has been modified.", "Could not load mesh.");
                    return false;
                }

                Meshes.Add(mesh);
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }

        RMeta.UseSettingsDir = false;
        RMeta.ActiveDir = dirPath;
        RMeta.ActiveRobotName = dirPath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last();

        ReloadPanels();
        return true;
    }

    /// <summary>
    /// Saves the robot to the directory it was loaded from or the default directory
    /// </summary>
    /// <returns></returns>
    public bool RobotSave(bool silent = false)
    {
        try
        {
            if (!Directory.Exists(PluginSettings.GeneralSaveLocation + "\\" + RMeta.ActiveRobotName))
                Directory.CreateDirectory(PluginSettings.GeneralSaveLocation + "\\" + RMeta.ActiveRobotName);
            BXDJSkeleton.SetupFileNames(SkeletonBase);
            BXDJSkeleton.WriteSkeleton((RMeta.UseSettingsDir && RMeta.ActiveDir != null) ? RMeta.ActiveDir : PluginSettings.GeneralSaveLocation + "\\" + RMeta.ActiveRobotName + "\\skeleton.bxdj", SkeletonBase);
            for (int i = 0; i < Meshes.Count; i++)
            {
                Meshes[i].WriteToFile((RMeta.UseSettingsDir && RMeta.ActiveDir != null) ? RMeta.ActiveDir : PluginSettings.GeneralSaveLocation + "\\" + RMeta.ActiveRobotName + "\\node_" + i + ".bxda");
            }

            for (int i = 0; i < Meshes.Count; i++)
            {
                Meshes[i].WriteToFile((RMeta.UseSettingsDir && RMeta.ActiveDir != null) ? RMeta.ActiveDir : PluginSettings.GeneralSaveLocation + "\\" + RMeta.ActiveRobotName + "\\node_" + i + ".bxda");
            }
            if(!silent)
                MessageBox.Show("Saved");
            return true;
        }
        catch (Exception e)
        {
            //TODO: Create a form that displays a simple error message with an option to expand it and view the exception info
            MessageBox.Show("Error saving robot: " + e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    /// <summary>
    /// Saves the robot to the currently set robot directory.
    /// </summary>
    /// <param name="robotName"></param>
    public bool RobotSaveAs(NameRobotForm.NameMode mode = NameRobotForm.NameMode.SaveAs)
    {
        if (NameRobotForm.NameRobot(out string robotName, mode) == DialogResult.OK)
        {
            try
            {
                if (!Directory.Exists(PluginSettings.GeneralSaveLocation + "\\" + robotName))
                    Directory.CreateDirectory(PluginSettings.GeneralSaveLocation + "\\" + robotName);

                BXDJSkeleton.WriteSkeleton(PluginSettings.GeneralSaveLocation + "\\" + robotName + "\\skeleton.bxdj", SkeletonBase);

                for (int i = 0; i < Meshes.Count; i++)
                {
                    Meshes[i].WriteToFile(PluginSettings.GeneralSaveLocation + "\\" + robotName + "\\node_" + i + ".bxda");
                }

                MessageBox.Show("Saved");

                RMeta.UseSettingsDir = true;
                RMeta.ActiveDir = null;
                RMeta.ActiveRobotName = robotName;

                return true;
            }
            catch (Exception e)
            {
                //TODO: Create a form that displays a simple error message with an option to expand it and view the exception info
                MessageBox.Show("Error saving robot: " + e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// Get the desired folder to open from or save to
    /// </summary>
    /// <returns>The full path of the selected folder</returns>
    private string OpenFolderPath()
    {
        string dirPath = null;

        var dialogThread = new Thread(() =>
        {
            FolderBrowserDialog openDialog = new FolderBrowserDialog()
            {
                Description = "Select a Robot Folder"
            };
            DialogResult openResult = openDialog.ShowDialog();

            if (openResult == DialogResult.OK) dirPath = openDialog.SelectedPath;
        });

        dialogThread.SetApartmentState(ApartmentState.STA);
        dialogThread.Start();
        dialogThread.Join();

        return dirPath;
    }

    /// <summary>
    /// Warn the user that they are about to overwrite existing data
    /// </summary>
    /// <returns>Whether the user wishes to overwrite the data</returns>
    private bool WarnOverwrite()
    {
        DialogResult overwriteResult = MessageBox.Show("Really overwrite existing robot?", "Overwrite Warning", MessageBoxButtons.YesNo);

        if (overwriteResult == DialogResult.Yes) return true;
        else return false;
    }

    /// <summary>
    /// Warn the user that they are about to exit without unsaved work
    /// </summary>
    /// <returns>Whether the user wishes to continue without saving</returns>
    public bool WarnUnsaved()
    {
        DialogResult saveResult = MessageBox.Show("Do you want to save your work?", "Save", MessageBoxButtons.YesNoCancel);

        if (saveResult == DialogResult.Yes)
        {
            return RobotSave();
        }
        else if (saveResult == DialogResult.No)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Reload all panels with newly loaded robot data
    /// </summary>
    public void ReloadPanels()
    {
        jointEditorPane1.SetSkeleton(SkeletonBase);
        bxdaEditorPane1.loadModel(Meshes);
    }

    protected override void OnResize(EventArgs e)
    {
        SuspendLayout();

        base.OnResize(e);
        splitContainer1.Height = ClientSize.Height - 27;

        ResumeLayout();
    }

    /// <summary>
    /// Opens the <see cref="PluginSettingsForm"/> form
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void SettingsExporter_OnClick(object sender, System.EventArgs e)
    {
        try
        {
            //TODO: Implement Value saving and loading
            PluginSettingsForm eSettingsForm = new PluginSettingsForm();
    
            eSettingsForm.ShowDialog();
    
            //BXDSettings.Instance.AddSettingsObject("Plugin Settings", ExporterSettingsForm.values);
            PluginSettings = PluginSettingsForm.Values;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
            throw;
        }
    }

    /// <summary>
    /// Runs the standalone Robot Viewer with and tells it to view the current robot
    /// </summary>
    /// <param name="settingsDir"></param>
    public void PreviewRobot(string settingsDir = null)
    {
        if(RMeta.ActiveDir != null)
        {
            Process.Start(Utilities.VIEWER_PATH, "-path \"" + RMeta.ActiveDir + "\"");
        }
        else
        {
            Process.Start(Utilities.VIEWER_PATH, "-path \"" + settingsDir + "\\" + RMeta.ActiveRobotName + "\"");
        }
    }

    /// <summary>
    /// Used to load <see cref="BXDAMesh"/>es into their corresponding <see cref="OGL_RigidNode"/>s
    /// </summary>
    public void LoadMeshes()
    {
        List<RigidNode_Base> nodes = SkeletonBase.ListAllNodes();
        for (int i = 0; i < Meshes.Count; i++)
        {
            ((OGL_RigidNode)nodes[i]).loadMeshes(Meshes[i]);
        }
    }

    /// <summary>
    /// Merges a node into the parent. Used during the one click export and the wizard.
    /// </summary>
    /// <param name="node"></param>
    public void MergeNodeIntoParent(RigidNode_Base node)
    {
        if (node.GetParent() == null)
            throw new ArgumentException("ERROR: Root node passed to MergeNodeIntoParent(RigidNode_Base)", "node");

        node.GetParent().ModelFullID += node.ModelFullID;

        //Get meshes for each node
        var childMesh = GetMesh(node);
        var parentMesh = GetMesh(node.GetParent());

        //Merge submeshes and colliders
        parentMesh.meshes.AddRange(childMesh.meshes);
        parentMesh.colliders.AddRange(childMesh.colliders);

        //Merge physics
        parentMesh.physics.Add(childMesh.physics.mass, childMesh.physics.centerOfMass);
        
        //Remove node from the children of its parent
        node.GetParent().Children.Remove(node.GetSkeletalJoint());
        Meshes.Remove(childMesh);
    }

    private BXDAMesh GetMesh(RigidNode_Base node)
    {
        return Meshes[SkeletonBase.ListAllNodes().IndexOf(node)];
    }

    #region OUTDATED EXPORTER METHODS
    /// <summary>
    /// Reset the <see cref="ExporterProgressWindow"/> progress bar
    /// </summary>
    public void ExporterReset()
    {
        exporter.ResetProgress();
    }

    public void ExporterOverallReset()
    {
        exporter.ResetOverall();
    }

    /// <summary>
    /// Set the length of the <see cref="ExporterProgressWindow"/> progress bar
    /// </summary>
    /// <param name="percentLength">The length of the bar in percentage points (0%-100%)</param>
    public void ExporterSetProgress(double percentLength)
    {
        exporter.AddProgress((int)Math.Floor(percentLength) - exporter.GetProgress());
    }

    /// <summary>
    /// Set the <see cref="ExporterProgressWindow"/> text after "Progress:"
    /// </summary>
    /// <param name="text">The text to add</param>
    public void ExporterSetSubText(string text)
    {
        exporter.SetProgressText(text);
    }

    public void ExporterSetMeshes(int num)
    {
        exporter.SetNumMeshes(num);
    }

    public void ExporterStepOverall()
    {
        exporter.AddOverallStep();
    }

    public void ExporterSetOverallText(string text)
    {
        exporter.SetOverallText(text);
    }

    private ExporterForm exporter = null;

    private void HelpTutorials_Click(object sender, EventArgs e)
    {
        Process.Start("http://bxd.autodesk.com/tutorial-robot.html");
    }

    #endregion

}