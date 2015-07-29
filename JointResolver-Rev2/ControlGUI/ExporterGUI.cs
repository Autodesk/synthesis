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

public partial class ExporterGUI : Form
{

    public static ExporterGUI Instance;

    private RigidNode_Base skeletonBase = null;
    private List<BXDAMesh> meshes = null;

    private ExporterSettings.ExporterSettingsValues exporterSettings;
    private ViewerSettings.ViewerSettingsValues viewerSettings;

    private ExporterProgressForm exporterProgress;

    private string lastDirPath = null;

    public ExporterGUI()
    {
        InitializeComponent();

        Instance = this;
        BXDSettings.Load();
        object exportSettings = BXDSettings.Instance.GetSettingsObject("Exporter Settings");
        object viewSettings = BXDSettings.Instance.GetSettingsObject("Viewer Settings");

        exporterSettings = (exportSettings != null) ? (ExporterSettings.ExporterSettingsValues) exportSettings : ExporterSettings.GetDefaultSettings();
        viewerSettings = (viewSettings != null) ? (ViewerSettings.ViewerSettingsValues) viewSettings : ViewerSettings.GetDefaultSettings();
        robotViewer1.LoadSettings(viewerSettings);

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

            ExporterSettings eSettingsForm = new ExporterSettings((defaultValues != null) ? (ExporterSettings.ExporterSettingsValues) defaultValues :
                                                                                             ExporterSettings.GetDefaultSettings());

            eSettingsForm.ShowDialog(this);

            BXDSettings.Instance.AddSettingsObject("Exporter Settings", eSettingsForm.values);
            exporterSettings = eSettingsForm.values;
        });
        settingsViewer.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            var defaultValues = BXDSettings.Instance.GetSettingsObject("Viewer Settings");

            ViewerSettings vSettingsForm = new ViewerSettings((defaultValues != null) ? (ViewerSettings.ViewerSettingsValues) defaultValues : 
                                                                                    ViewerSettings.GetDefaultSettings());

            vSettingsForm.ShowDialog(this);

            BXDSettings.Instance.AddSettingsObject("Viewer Settings", vSettingsForm.values);
            viewerSettings = vSettingsForm.values;

            robotViewer1.LoadSettings(viewerSettings);
        });

        helpAbout.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            AboutDialog about = new AboutDialog();
            about.ShowDialog(this);
        });

        this.FormClosing += new FormClosingEventHandler(delegate(object sender, FormClosingEventArgs e)
        {
            if (skeletonBase != null && !WarnUnsaved()) e.Cancel = true;
            else BXDSettings.Save();

            if (Exporter.INVENTOR_APPLICATION != null) Exporter.INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = false;
        });
    }

    public void SetNew()
    {
        skeletonBase = null;
        meshes = null;

        ReloadPanels();
    }

    public void LoadFromInventor()
    {
        if (skeletonBase != null && !WarnUnsaved()) return;

        RigidNode_Base tmpBase = null;
        List<BXDAMesh> tmpMeshes = null;

        try
        {
            AutoResetEvent startEvent = new AutoResetEvent(false);

            var exporterProgressThread = new Thread(() =>
            {
                exporterProgress = new ExporterProgressForm(startEvent, 
                                Color.FromArgb((int) exporterSettings.generalTextColor), Color.FromArgb((int) exporterSettings.generalBackgroundColor));
                exporterProgress.ShowDialog();
            });

            exporterProgressThread.SetApartmentState(ApartmentState.STA);
            exporterProgressThread.Start();

            startEvent.WaitOne();

            var exporterThread = new Thread(() =>
            {
                try
                {
                    try
                    {
                        Exporter.LoadInventorInstance();
                    }
                    catch
                    {
                        MessageBox.Show("Could not get a running instance of Inventor. Open Inventor and try again");
                        return;
                    }

                    Exporter.INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = true;

                    tmpBase = Exporter.ExportSkeleton();
                    tmpMeshes = Exporter.ExportMeshes(tmpBase, exporterSettings.meshResolutionValue > 0, exporterSettings.meshFancyColors);

                    Exporter.INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = false;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            exporterThread.SetApartmentState(ApartmentState.MTA);
            exporterThread.Start();

            while (!exporterProgressThread.Join(0))
            {
                if (!exporterThread.IsAlive && !exporterProgress.finished)
                {
                    Console.WriteLine("Finished!");
                    if (exporterSettings.generalSaveLog)
                        exporterProgress.Finish(exporterSettings.generalSaveLogLocation + "\\log_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + ".txt");
                    else exporterProgress.Finish();
                }
            }

            if (exporterThread.IsAlive)
            {
                if (Exporter.INVENTOR_APPLICATION != null) 
                    Exporter.INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = false;
                exporterThread.Abort();
            }
            else
            {
                exporterThread.Join();
                if (tmpBase != null && tmpMeshes != null)
                {
                    skeletonBase = new OGL_RigidNode(tmpBase);
                    meshes = tmpMeshes;
                }
            }
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
        if (skeletonBase != null && !WarnUnsaved()) return;

        string dirPath = OpenFolderPath();

        if (dirPath == null) return;

        try
        {
            skeletonBase = BXDJSkeleton.ReadSkeleton(dirPath + "\\skeleton.bxdj");
            meshes = new List<BXDAMesh>();

            var meshFiles = Directory.GetFiles(dirPath).Where(name => name.EndsWith(".bxda"));
            foreach (string fileName in meshFiles)
            {
                BXDAMesh mesh = new BXDAMesh();
                mesh.ReadFromFile(fileName);
                meshes.Add(mesh);
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
        if (skeletonBase == null || meshes == null) return false;

        string dirPath = lastDirPath;

        if (dirPath == null || isSaveAs) dirPath = OpenFolderPath();

        if (File.Exists(dirPath + "\\skeleton.bxdj"))
        {
            if (isSaveAs && !WarnOverwrite()) return false;
        }

        try
        {
            BXDJSkeleton.WriteSkeleton(dirPath + "\\skeleton.bxdj", skeletonBase);

            for (int i = 0; i < meshes.Count; i++)
            {
                meshes[i].WriteToFile(dirPath + "\\node_" + i + ".bxda");
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
        exporterProgress.ResetProgress();
    }

    public void ExporterSetProgress(double percentLength)
    {
        exporterProgress.AddProgress((int) Math.Floor(percentLength) - exporterProgress.GetProgress());
    }

    public void ExporterSetSubText(string text)
    {
        exporterProgress.SetProgressText(text);
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
        jointEditorPane1.SetSkeleton(skeletonBase);
        bxdaEditorPane1.loadModel(meshes);
        robotViewer1.LoadModel(skeletonBase, meshes);
    }

}
