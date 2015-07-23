using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

        RigidNode_Base.NODE_FACTORY = delegate()
        {
            return new OGL_RigidNode();
        };

        jointEditorPane1.SelectedJoint += robotViewer1.SelectJoint;

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
        });
    }

    public void SetNew()
    {
        SkeletonBase = null;
        Meshes = null;

        ReloadPanels();
    }

    public void LoadFromInventor()
    {
        if (SkeletonBase != null && !WarnUnsaved()) return;

        try
        {
            var exporterThread = new Thread(() =>
            {
                exporter = new ExporterForm();

                exporter.ShowDialog();
            });

            exporterThread.SetApartmentState(ApartmentState.STA);
            exporterThread.Start();

            exporterThread.Join();

        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
            return;
        }

        ReloadPanels();
    }

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

    public bool SaveRobot(bool isSaveAs)
    {
        if (SkeletonBase == null || Meshes == null) return false;

        string dirPath = lastDirPath;

        if (dirPath == null || isSaveAs) dirPath = OpenFolderPath();

        if (File.Exists(dirPath + "\\skeleton.bxdj"))
        {
            if (isSaveAs && !WarnOverwrite()) return false;
        }

        try
        {
            BXDJSkeleton.WriteSkeleton(dirPath + "\\skeleton.bxdj", SkeletonBase);

            for (int i = 0; i < Meshes.Count; i++)
            {
                Meshes[i].WriteToFile(dirPath + "\\node_" + i + ".bxda");
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }

        MessageBox.Show("Saved!");

        lastDirPath = dirPath;

        return true;
    }

    public void ExporterReset()
    {
        exporter.ResetProgress();
    }

    public void ExporterSetProgress(double percentLength)
    {
        exporter.AddProgress((int) Math.Floor(percentLength) - exporter.GetProgress());
    }

    public void ExporterSetSubText(string text)
    {
        exporter.SetProgressText(text);
    }

    private string OpenFolderPath()
    {
        string dirPath = null;

        var dialogThread = new Thread(() =>
        {
            FolderBrowserDialog openDialog = new FolderBrowserDialog();
            openDialog.RootFolder = Environment.SpecialFolder.UserProfile;
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

    private bool WarnOverwrite()
    {
        DialogResult overwriteResult = MessageBox.Show("Really overwrite?", "Overwrite Warning", MessageBoxButtons.YesNo);

        if (overwriteResult == DialogResult.Yes) return true;
        else return false;
    }

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

    private void ReloadPanels()
    {
        jointEditorPane1.SetSkeleton(SkeletonBase);
        bxdaEditorPane1.loadModel(Meshes);
        robotViewer1.LoadModel(SkeletonBase, Meshes);
    }

}
