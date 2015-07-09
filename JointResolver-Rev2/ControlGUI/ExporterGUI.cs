using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
    }

    public void LoadFromInventor()
    {
        if (skeletonBase != null && !WarnOverwrite()) return;

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

        ReloadPanels();
    }

    public void OpenExisting()
    {
        if (skeletonBase != null && !WarnOverwrite()) return;

        string dirPath = BXDSettings.Instance.LastSkeletonDirectory;

        try
        {
            skeletonBase = BXDJSkeleton.ReadSkeleton(dirPath + "\\skeleton.bxdj");
            meshes = new List<BXDAMesh>();

            var meshFiles = Directory.GetFiles(dirPath).Where(name => !name.EndsWith("j"));
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

    private bool WarnOverwrite()
    {
        DialogResult overwriteResult = MessageBox.Show("Really overwrite?", "Overwrite Warning", MessageBoxButtons.YesNo);

        if (overwriteResult == DialogResult.Yes) return true;
        else return false;
    }

    public void SaveRobot()
    {
        if (skeletonBase == null || meshes == null) return;

        string dirPath = BXDSettings.Instance.LastSkeletonDirectory;

        if (File.Exists(dirPath + "\\skeleton.bxdj") && !WarnOverwrite()) return;

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
    }

    private void ReloadPanels()
    {
        jointEditorPane1.SetSkeleton(skeletonBase);
        bxdaEditorPane1.loadModel(meshes);
        robotViewer1.loadModel(skeletonBase, meshes);
    }

}
