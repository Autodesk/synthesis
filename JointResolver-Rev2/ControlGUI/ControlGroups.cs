using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading;

public partial class ControlGroups
{
    public FormState formState;

    private RigidNode_Base skeleton;
    private List<CustomRigidGroup> groupList;
    private DriveChooser driveChooser = new DriveChooser();

    public ControlGroups()
    {
        InitializeComponent();  // Remove for death
        jointPane.ModifiedJoint += jointPane_ModifiedJoint;
        jointPane.SelectedJoint += jointPane_SelectedJoint;
    }

    void jointPane_SelectedJoint(RigidNode_Base node)
    {
        if (node == null)
        {
            ComponentHighlighter.ClearHighlight();
        }
        else if (node is RigidNode)
        {
            foreach (Inventor.ComponentOccurrence occ in ((RigidNode) node).group.occurrences)
            {
                ComponentHighlighter.PrepareHighlight();
                ComponentHighlighter.CHILD_HIGHLIGHT_SET.AddItem(occ);
            }
        }
    }

    private void jointPane_ModifiedJoint(RigidNode_Base node)
    {
        if (node==null || !(node is RigidNode)) return;
        if (node.GetSkeletalJoint() != null && node.GetSkeletalJoint().cDriver != null && node.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>() != null)
        {
            ((RigidNode) node).RegisterDeferredCalculation("wheel-driver", WheelAnalyzer.StartCalculations);
        }
        else
        {
            ((RigidNode) node).UnregisterDeferredCalculation("wheel-driver");
        }
    }

    private void btnExport_Click(object sender, EventArgs e)
    {
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
        foreach (CustomRigidGroup group in groupList)
        {
            System.Windows.Forms.ListViewItem item = new System.Windows.Forms.ListViewItem(new string[] {group.ToString(),
                group.grounded?"Yes":"No",group.colorFaces?"Yes":"No", group.highRes?"Yes":"No", group.convex?"Convex":"Concave"});
            item.Tag = group;
            lstGroups.Items.Add(item);
        }
    }

    private void ControlGroups_Load(object sender, EventArgs e)
    {
    }


    public void SetGroupList(List<CustomRigidGroup> groupList)
    {
        this.groupList = groupList;
        UpdateGroupList();
    }

    public void SetSkeleton(RigidNode_Base root)
    {
        this.skeleton = root;
        this.jointPane.SetSkeleton(root);
    }

    public void Cleanup()
    {
        ComponentHighlighter.CleanupHighlighter();
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
        groups_chGrounded.Width = this.lstGroups.Width / 8;
        groups_chFaceColor.Width = this.lstGroups.Width / (800 / 120);
        groups_chHighRes.Width = this.lstGroups.Width / (800 / 140);
        groups_chConcavity.Width = this.lstGroups.Width / 10;



        tabsMain.Height = newTabHeight;
        tabsMain.Width = newTabWidth;
        lstGroups.Height = newListHeight;
        lstGroups.Width = newListWidth;
        jointPane.Height = newListHeight;
        jointPane.Width = newListWidth;
    }

    private void lstGroups_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (chkHighlightComponents.Checked && lstGroups.SelectedItems.Count == 1 && lstGroups.SelectedItems[0].Tag is CustomRigidGroup)
        {
            CustomRigidGroup group = (CustomRigidGroup) lstGroups.SelectedItems[0].Tag;
            ComponentHighlighter.PrepareHighlight();
            ComponentHighlighter.ClearHighlight();
            foreach (Inventor.ComponentOccurrence child in group.occurrences)
            {
                ComponentHighlighter.CHILD_HIGHLIGHT_SET.AddItem(child);
            }
        }
    }

    private void lstGroups_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
    {
        System.Windows.Forms.ListViewItem item = lstGroups.GetItemAt(e.X, e.Y);
        if (item != null && item.Tag != null && item.Tag is CustomRigidGroup)
        {
            System.Drawing.Rectangle clicked = item.Bounds;
            int rightPos = 0;
            int column;
            for (column = 0; column <= lstGroups.Columns.Count; column++)
            {
                int leftPos = rightPos;
                rightPos += lstGroups.Columns[column].Width;
                if (clicked.Left + rightPos > e.X && clicked.Left + leftPos < e.X)
                {
                    break;
                }
            }
            if (column == 2)    // Multicolor
            {
                bool cVal = ((CustomRigidGroup) item.Tag).colorFaces;
                ((CustomRigidGroup) item.Tag).colorFaces = !cVal;
                item.SubItems[2].Text = !cVal ? "Yes" : "No";
            }
            else if (column == 3)   // Highres
            {
                bool cVal = ((CustomRigidGroup) item.Tag).highRes;
                ((CustomRigidGroup) item.Tag).highRes = !cVal;
                item.SubItems[3].Text = !cVal ? "Yes" : "No";
            }
            else if (column == 4)
            {
                bool cVal = ((CustomRigidGroup) item.Tag).convex;
                ((CustomRigidGroup) item.Tag).convex = !cVal;
                item.SubItems[4].Text = !cVal ? "Convex" : "Concave";
            }
        }
    }

    private void btnCalculate_Click_1(object sender, EventArgs e)
    {
        //if (lstJoints.SelectedItems.Count == 1 && lstJoints.SelectedItems[0].Tag is RigidNode)
        //{
        //    InventorSkeletalJoint joint = (InventorSkeletalJoint) ((RigidNode) lstJoints.SelectedItems[0].Tag).GetSkeletalJoint();
        //    joint.DetermineLimits();
        //}
    }

    private void tabsMain_SelectedIndexChanged(object sender, EventArgs e)
    {
    }
}

public enum FormState
{
    SUBMIT,
    CANCEL,
    CLOSE
}