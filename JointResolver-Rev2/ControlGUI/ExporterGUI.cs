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
using OGLViewer;

public partial class ExporterGUI : Form
{

    private RigidNode_Base skeletonBase;
    private List<BXDAMesh> meshes;

    public ExporterGUI()
    {
        InitializeComponent();

        RigidNode_Base.NODE_FACTORY = delegate()
        {
            return new OGL_RigidNode();
        };

        this.robotViewer1.loadInventor.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            LoadFromInventor();
        });
        this.robotViewer1.openExisting.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            OpenExisting();
        });
        this.robotViewer1.saveButton.Click += new System.EventHandler(delegate(object sender, System.EventArgs e)
        {
            SaveRobot();
        });

        this.FormClosing += new FormClosingEventHandler(delegate(object sender, FormClosingEventArgs e)
        {
            if (skeletonBase != null && !WarnUnsaved()) e.Cancel = true;
        });
    }

    public void LoadFromInventor()
    {
        if (skeletonBase != null && !WarnUnsaved()) return;

        Visible = false;

        try
        {
            Exporter.LoadInventorInstance();
            skeletonBase = Exporter.ExportSkeleton();
            meshes = Exporter.ExportMeshes(skeletonBase);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }

        Visible = true;

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

        ReloadPanels();
    }

    public bool SaveRobot()
    {
        if (skeletonBase == null || meshes == null) return false;

        string dirPath = OpenFolderPath();

        if (dirPath == null || (File.Exists(dirPath + "\\skeleton.bxdj") && !WarnOverwrite())) return false;

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

        return true;
    }

    private string OpenFolderPath()
    {
        string dirPath = null;

        var dialogThread = new Thread(() =>
        {
            FolderBrowserDialog openDialog = new FolderBrowserDialog();
            openDialog.RootFolder = Environment.SpecialFolder.UserProfile;
            openDialog.ShowNewFolderButton = false;
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
            return SaveRobot();
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
        robotViewer1.loadModel(skeletonBase, meshes);
    }

}
