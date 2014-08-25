using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

public partial class ControlGroups
{
    public FormState formState;

    private RigidNode_Base skeleton;
    private List<RigidNode_Base> groupList;
    private DriveChooser driveChooser = new DriveChooser();

    public string ExportPath
    {
        get
        {
            return txtFilePath.Text;
        }
    }

    public ControlGroups()
    {
        InitializeComponent();// Don't remove
        txtFilePath.Text = BXDSettings.Instance.LastSkeletonDirectory != null ? BXDSettings.Instance.LastSkeletonDirectory : "";
    }

    private void btnExport_Click(object sender, EventArgs e)
    {
        if (txtFilePath.Text.IndexOfAny(System.IO.Path.GetInvalidPathChars()) != -1)
        {
            System.Windows.Forms.MessageBox.Show("\"" + txtFilePath.Text + "\" is not a valid path!");
            return;
        }
        if (System.IO.File.Exists(txtFilePath.Text) && !System.IO.Directory.Exists(txtFilePath.Text))
        {
            System.Windows.Forms.MessageBox.Show("\"" + txtFilePath.Text + "\" exists as a file!");
            return;
        }

        formState = FormState.SUBMIT;
        Hide();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        formState = FormState.CANCEL;
        Hide();
    }

    private void UpdateGroupList()
    {
        if (groupList == null)
            return;
        lstGroups.Items.Clear();
        foreach (RigidNode_Base mesh in groupList)
        {
            OGL_RigidNode node = (OGL_RigidNode) mesh;
            System.Windows.Forms.ListViewItem item = new System.Windows.Forms.ListViewItem(new string[] {mesh.modelFileName,
            Convert.ToString(node.meshCount), Convert.ToString(node.meshTriangleCount), Convert.ToString(node.colliderCount), Convert.ToString(node.colliderTriangleCount) });
            item.Tag = mesh;
            lstGroups.Items.Add(item);
        }
    }

    private void ControlGroups_Load(object sender, EventArgs e)
    {
    }


    public void SetGroupList(List<RigidNode_Base> groupList)
    {
        this.groupList = groupList;
        UpdateGroupList();
    }

    public void SetSkeleton(RigidNode_Base root)
    {
        this.skeleton = root;
        this.jointPane.SetSkeleton(root);
    }

    private void ControlGroups_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
    {
        formState = FormState.CLOSE;
        Hide();
    }

    private void btnCalculate_Click(object sender, EventArgs e)
    {

    }

    private void window_SizeChanged(object sender, EventArgs e)
    {
        int newTabHeight = this.Height - 120;
        int newTabWidth = this.Width - 43;
        int newListHeight = this.Height - 120;
        int newListWidth = this.Width - 63;

        groups_chName.Width = this.lstGroups.Width / 3;

        tabsMain.Height = newTabHeight;
        tabsMain.Width = newTabWidth;
        lstGroups.Height = newListHeight;
        lstGroups.Width = newListWidth;
        jointPane.Height = newListHeight;
        jointPane.Width = newListWidth;
    }

    private void lstGroups_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lstGroups.SelectedItems.Count == 1 && lstGroups.SelectedItems[0].Tag is OGL_RigidNode)
        {
            OGL_RigidNode node = (OGL_RigidNode) lstGroups.SelectedItems[0].Tag;
            foreach (RigidNode_Base ns in groupList)
            {
                ((OGL_RigidNode) ns).highlight &= ~OGL_RigidNode.HighlightState.ACTIVE;
            }
            if (node is OGL_RigidNode)
            {
                ((OGL_RigidNode) node).highlight |= OGL_RigidNode.HighlightState.ACTIVE;
            }
        }
    }

    private void tabsMain_SelectedIndexChanged(object sender, EventArgs e)
    {
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
        string selectedPath = "";
        var t = new Thread((ThreadStart) (() =>
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.UserProfile;
            if (BXDSettings.Instance.LastSkeletonDirectory != null)
            {
                fbd.SelectedPath = BXDSettings.Instance.LastSkeletonDirectory;
            }
            fbd.ShowNewFolderButton = true;
            if (fbd.ShowDialog() == DialogResult.Cancel)
                return;

            selectedPath = fbd.SelectedPath;
        }));

        t.SetApartmentState(ApartmentState.STA);
        t.Start();
        t.Join();
        if (selectedPath.Length > 0 && (System.IO.Directory.Exists(selectedPath) || !System.IO.File.Exists(selectedPath)))
        {
            txtFilePath.Text = selectedPath;
            //loadFromExisting();
        }
    }

    private void loadFromExisting()
    {
        if (skeleton == null)
            return;
        try
        {
            // Merge with existing values
            if (System.IO.File.Exists(txtFilePath.Text + "\\skeleton.bxdj"))
            {
                RigidNode_Base loadedBase = BXDJSkeleton.ReadSkeleton(txtFilePath.Text + "\\skeleton.bxdj");

                BXDJSkeleton.CloneDriversFromTo(loadedBase, skeleton, DialogResult.Yes == MessageBox.Show(
                    "Do you want to overwrite existing drivers/sensors?",
                    "Overwrite Warning", MessageBoxButtons.YesNo));
            }
            jointPane.SetSkeleton(skeleton);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error loading existing skeleton: " + e.ToString());
        }
    }

    private void button1_Click(object sender, EventArgs e)
    {
        loadFromExisting();
    }
}

public enum FormState
{
    SUBMIT,
    CANCEL,
    CLOSE
}