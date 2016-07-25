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
using System.Windows.Forms;
using EditorsLibrary;
using OGLViewer;

public partial class SynthesisGUI : Form
{

    public static SynthesisGUI Instance;

    public static ExporterSettingsForm.ExporterSettingsValues ExporterSettings;
    public static ViewerSettingsForm.ViewerSettingsValues ViewerSettings;

    public RigidNode_Base SkeletonBase = null;
    public List<BXDAMesh> Meshes = null;

    private ExporterForm exporter;

    /// <summary>
    /// The last path that was saved to/opened from
    /// </summary>
    private string lastDirPath = null;

    static SynthesisGUI()
    {
        BXDSettings.Load();
        object exportSettings = BXDSettings.Instance.GetSettingsObject("Exporter Settings");
        object viewSettings = BXDSettings.Instance.GetSettingsObject("Viewer Settings");

        ExporterSettings = (exportSettings != null) ?
                           (ExporterSettingsForm.ExporterSettingsValues)exportSettings : ExporterSettingsForm.GetDefaultSettings();
        ViewerSettings = (viewSettings != null) ? (ViewerSettingsForm.ViewerSettingsValues)viewSettings : ViewerSettingsForm.GetDefaultSettings();
    }

    public SynthesisGUI()
    {
        InitializeComponent();

        Instance = this;

        robotViewer1.LoadSettings(ViewerSettings);
        bxdaEditorPane1.Units = ViewerSettings.modelUnits;

        RigidNode_Base.NODE_FACTORY = delegate(Guid guid)
        {
            return new OGL_RigidNode(guid);
        };

        fileNew.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            SetNew();
        });
        fileLoad.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            LoadFromInventor();
        });
        fileOpen.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            OpenExisting();
        });
        fileSave.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            SaveRobot(false);
        });
        fileSaveAs.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            SaveRobot(true);
        });
        fileExit.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            Close();
        });

        settingsExporter.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            var defaultValues = BXDSettings.Instance.GetSettingsObject("Exporter Settings");

            ExporterSettingsForm eSettingsForm = new ExporterSettingsForm((defaultValues != null) ? (ExporterSettingsForm.ExporterSettingsValues) defaultValues :
                                                                                             ExporterSettingsForm.GetDefaultSettings());

            eSettingsForm.ShowDialog(this);

            BXDSettings.Instance.AddSettingsObject("Exporter Settings", eSettingsForm.values);
            ExporterSettings = eSettingsForm.values;
        });
        settingsViewer.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            var defaultValues = BXDSettings.Instance.GetSettingsObject("Viewer Settings");

            ViewerSettingsForm vSettingsForm = new ViewerSettingsForm((defaultValues != null) ? (ViewerSettingsForm.ViewerSettingsValues) defaultValues : 
                                                                                    ViewerSettingsForm.GetDefaultSettings());

            vSettingsForm.ShowDialog(this);

            BXDSettings.Instance.AddSettingsObject("Viewer Settings", vSettingsForm.values);
            ViewerSettings = vSettingsForm.values;

            robotViewer1.LoadSettings(ViewerSettings);
            bxdaEditorPane1.Units = ViewerSettings.modelUnits;
        });

        helpAbout.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            AboutDialog about = new AboutDialog();
            about.ShowDialog(this);
        });

        this.FormClosing += new FormClosingEventHandler(delegate(object sender, FormClosingEventArgs e)
        {
            if (SkeletonBase != null && !WarnUnsaved()) e.Cancel = true;
            else BXDSettings.Save();

            InventorManager.ReleaseInventor();
        });

        jointEditorPane1.ModifiedJoint += delegate(List<RigidNode_Base> nodes)
        {

            if (nodes == null || nodes.Count == 0) return;

            foreach (RigidNode_Base node in nodes)
            {
                if (node.GetSkeletalJoint() != null && node.GetSkeletalJoint().cDriver != null &&
                    node.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>() != null &&
                    node.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>().radius == 0 &&
                    node is OGL_RigidNode)
                {
                    float radius, width;
                    BXDVector3 center;

                    (node as OGL_RigidNode).GetWheelInfo(out radius, out width, out center);

                    WheelDriverMeta wheelDriver = node.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>();
                    wheelDriver.center = center;
                    wheelDriver.radius = radius;
                    wheelDriver.width = width;
                    node.GetSkeletalJoint().cDriver.AddInfo(wheelDriver);

                }
            }
        };


        jointEditorPane1.SelectedJoint += robotViewer1.SelectJoints;
        jointEditorPane1.SelectedJoint += bxdaEditorPane1.SelectJoints;

        robotViewer1.NodeSelected += jointEditorPane1.AddSelection;
        robotViewer1.NodeSelected += bxdaEditorPane1.AddSelection;

        bxdaEditorPane1.NodeSelected += (BXDAMesh mesh) =>
            {
                List<RigidNode_Base> nodes = new List<RigidNode_Base>();
                SkeletonBase.ListAllNodes(nodes);

                jointEditorPane1.AddSelection(nodes[Meshes.IndexOf(mesh)], true);
            };

        bxdaEditorPane1.NodeSelected += (BXDAMesh mesh) =>
        {
            List<RigidNode_Base> nodes = new List<RigidNode_Base>();
            SkeletonBase.ListAllNodes(nodes);

            robotViewer1.SelectJoints(nodes.GetRange(Meshes.IndexOf(mesh), 1));
        };
    }

    ~SynthesisGUI()
    {

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
    public void LoadFromInventor()
    {
        if (SkeletonBase != null && !WarnUnsaved()) return;

        try
        {
            var exporterThread = new Thread(() =>
            {
                exporter = new ExporterForm(ExporterSettings);

                exporter.ShowDialog();
            });

            exporterThread.SetApartmentState(ApartmentState.STA);
            exporterThread.Start();

            exporterThread.Join();

            GC.Collect();
        }
        catch (System.Runtime.InteropServices.InvalidComObjectException)
        {
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
            return;
        }

        ReloadPanels();
    }

    /// <summary>
    /// Open a previously exported robot
    /// </summary>
    public void OpenExisting()
    {
        if (SkeletonBase != null && !WarnUnsaved()) return;

        string dirPath = OpenFolderPath();

        if (dirPath == null) return;

        try
        {
            SkeletonBase = BXDJSkeleton.ReadSkeleton(dirPath + "\\skeleton.bxdj");
            Meshes = new List<BXDAMesh>();
            
            var meshFiles = Directory.GetFiles(dirPath).Where(name => name.EndsWith(".bxda"));
            
            foreach (string fileName in meshFiles)
            {
                BXDAMesh mesh = new BXDAMesh();
                mesh.ReadFromFile(fileName);

                foreach (RigidNode_Base node in SkeletonBase.ListAllNodes())
                {
                    if (node.ModelFileName.Equals(fileName.Substring(fileName.LastIndexOf('\\') + 1)) &&
                        !node.GUID.Equals(mesh.GUID))
                    {
                        MessageBox.Show(fileName + " has been modified.", "Could not load mesh.");
                        return;
                    }
                }

                Meshes.Add(mesh);
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }

        lastDirPath = dirPath;

        ReloadPanels();
    }

    /// <summary>
    /// Save all changes to an open robot
    /// </summary>
    /// <param name="isSaveAs">If this is a "Save As" operation</param>
    /// <returns>If the save operation succeeded</returns>
    public bool SaveRobot(bool isSaveAs)
    {
        if (SkeletonBase == null || Meshes == null) return false;

        string dirPath = lastDirPath;

        if (dirPath == null || isSaveAs) dirPath = OpenFolderPath();
        if (dirPath == null) return false;

        if (File.Exists(dirPath + "\\skeleton.bxdj"))
        {
            if (dirPath != lastDirPath && !WarnOverwrite()) return false;
        }

        try
        {
            BXDJSkeleton.WriteSkeleton(dirPath + "\\skeleton.bxdj", SkeletonBase);

            for (int i = 0; i < Meshes.Count; i++)
            {
                Meshes[i].WriteToFile(dirPath + "\\node_" + i + ".bxda");
            }

            /*
             * The commented code below is for testing purposes.
             * To determine if the reading/writing process runs
             * without loss of data, compare the text of skeleton.bxdj
             * and skeleton2.bxdj. If they are equal, no data was lost.
             */

            /** /
            RigidNode_Base testRigidNode = BXDJSkeleton.ReadSkeleton(dirPath + "\\skeleton.bxdj");

            BXDJSkeleton.WriteSkeleton(dirPath + "\\skeleton2.bxdj", testRigidNode);
            /**/
        }
        catch (Exception e)
        {
            MessageBox.Show("Couldn't save robot \n" + e.Message);
            return false;
        }

        MessageBox.Show("Saved!");

        lastDirPath = dirPath;

        return true;
    }

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
        exporter.AddProgress((int) Math.Floor(percentLength) - exporter.GetProgress());
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

    /// <summary>
    /// Get the desired folder to open from or save to
    /// </summary>
    /// <returns>The full path of the selected folder</returns>
    private string OpenFolderPath()
    {
        string dirPath = null;

        var dialogThread = new Thread(() =>
        {
            FolderBrowserDialog openDialog = new FolderBrowserDialog();
            openDialog.ShowNewFolderButton = true;
            openDialog.Description = "Choose Robot Folder";
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
    private bool WarnUnsaved()
    {
        DialogResult saveResult = MessageBox.Show("Do you want to save your work?", "Save", MessageBoxButtons.YesNoCancel);

        if (saveResult == DialogResult.Yes)
        {
            return SaveRobot(false);
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
    private void ReloadPanels()
    {
        jointEditorPane1.SetSkeleton(SkeletonBase);
        bxdaEditorPane1.loadModel(Meshes);
        robotViewer1.LoadModel(SkeletonBase, Meshes);
    }

    protected override void OnResize(EventArgs e)
    {
        SuspendLayout();

        base.OnResize(e);
        splitContainer1.Height = ClientSize.Height - 27;

        ResumeLayout();
    }

    private void helpTutorials_Click(object sender, EventArgs e)
    {
        Process.Start("http://bxd.autodesk.com/synthesis/?page=Tutorials");
    }

}
