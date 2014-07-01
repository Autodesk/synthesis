using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading;

public partial class ControlGroups
{
    public FormState formState;

    private List<RigidNode> nodeList;
    private List<CustomRigidGroup> groupList;
    private DriveChooser driveChooser = new DriveChooser();

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

    private void updateJointList()
    {
        if ((nodeList == null))
            return;
        lstJoints.Items.Clear();
        foreach (RigidNode node in nodeList)
        {
            if (!((node.parentConnection == null) || (node.parent == null)))
            {
                SkeletalJoint joint = node.getSkeletalJoint();
                if (joint != null)
                {

                    System.Windows.Forms.ListViewItem item = new System.Windows.Forms.ListViewItem(new string[] { 
                joint.getJointType(),
                joint.GetParent().ToString(),
                joint.GetChild().ToString(), joint.cDriver!=null?joint.cDriver.ToString():"No driver" });
                    item.Tag = joint;
                    lstJoints.Items.Add(item);
                }
            }
        }
    }
    private void updateGroupList()
    {
        if (groupList == null) return;
        lstGroups.Items.Clear();
        foreach (CustomRigidGroup group in groupList)
        {
            System.Windows.Forms.ListViewItem item = new System.Windows.Forms.ListViewItem(new string[] {group.ToString(),
            group.grounded?"Yes":"No",group.colorFaces?"Yes":"No", group.highRes?"Yes":"No"});
            item.Tag = group;
            lstGroups.Items.Add(item);
        }
    }

    private void ControlGroups_Load(object sender, EventArgs e)
    {
        updateJointList();
    }

    public void setNodeList(List<RigidNode> nodeList)
    {
        this.nodeList = nodeList;
        updateJointList();
    }

    public void setGroupList(List<CustomRigidGroup> groupList)
    {
        this.groupList = groupList;
        updateGroupList();
    }

    public void Cleanup()
    {
        ComponentHighlighter.cleanupHS();
    }

    private void ControlGroups_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
    {
        formState = FormState.CLOSE;
        Hide();
    }

    private void lstJoints_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lstJoints.SelectedItems.Count == 1 && lstJoints.SelectedItems[0].Tag is SkeletalJoint)
        {
            SkeletalJoint joint = (SkeletalJoint)lstJoints.SelectedItems[0].Tag;
            joint.DoHighlight();
        }
        else
        {
            ComponentHighlighter.clearHighlight();
        }
    }

    private void lstJoints_DoubleClick(object sender, EventArgs e)
    {
        if (lstJoints.SelectedItems.Count == 1 && lstJoints.SelectedItems[0].Tag is SkeletalJoint)
        {
            SkeletalJoint joint = (SkeletalJoint)lstJoints.SelectedItems[0].Tag;
            driveChooser.ShowDialog(joint);
            updateJointList();
        }
    }

    private void lstGroups_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lstGroups.SelectedItems.Count == 1 && lstGroups.SelectedItems[0].Tag is CustomRigidGroup)
        {
            CustomRigidGroup group = (CustomRigidGroup)lstGroups.SelectedItems[0].Tag;
            ComponentHighlighter.prepareHighlight();
            ComponentHighlighter.clearHighlight();
            foreach (Inventor.ComponentOccurrence child in group.occurrences)
            {
                ComponentHighlighter.childHS.AddItem(child);
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
                bool cVal = ((CustomRigidGroup)item.Tag).colorFaces;
                ((CustomRigidGroup)item.Tag).colorFaces = !cVal;
                item.SubItems[2].Text = !cVal ? "Yes" : "No";
            }
            else if (column == 3)   // Highres
            {
                bool cVal = ((CustomRigidGroup)item.Tag).highRes;
                ((CustomRigidGroup)item.Tag).highRes = !cVal;
                item.SubItems[3].Text = !cVal ? "Yes" : "No";
            }
        }
    }
}

public enum FormState
{
    SUBMIT,
    CANCEL,
    CLOSE
}