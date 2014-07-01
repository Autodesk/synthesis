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

    private void updateList()
    {
        if ((nodeList == null))
            return;
        lstItemView.Items.Clear();
        foreach (RigidNode node in nodeList)
        {
            if (!((node.parentConnection == null) || (node.parent == null)))
            {
                if ((node.parentConnection.skeletalJoint == null))
                {
                    node.parentConnection.skeletalJoint = SkeletalJoint.create(node.parentConnection, node.parent.@group);
                }
                SkeletalJoint joint = node.parentConnection.skeletalJoint;

                System.Windows.Forms.ListViewItem item = new System.Windows.Forms.ListViewItem(new string[] { 
                joint.getJointType(),
                joint.GetParent().ToString(),
                joint.GetChild().ToString(), joint.cDriver!=null?joint.cDriver.ToString():"No driver" });
                item.Tag = joint;
                lstItemView.Items.Add(item);
            }
        }
    }

    private void ControlGroups_Load(object sender, EventArgs e)
    {
        updateList();
    }

    public void setNodeList(List<RigidNode> nodeList)
    {
        this.nodeList = nodeList;
        updateList();
    }

    public void Cleanup()
    {
        SkeletalJoint.cleanupHS();
    }

    private void ControlGroups_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
    {
        formState = FormState.CLOSE;
        Hide();
    }

    private void lstItemView_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lstItemView.SelectedItems.Count == 1 && lstItemView.SelectedItems[0].Tag is SkeletalJoint)
        {
            SkeletalJoint joint = (SkeletalJoint)lstItemView.SelectedItems[0].Tag;
            joint.DoHighlight();
        }
        else
        {
            SkeletalJoint.clearHighlight();
        }
    }

    private void lstItemView_DoubleClick(object sender, EventArgs e)
    {
        if (lstItemView.SelectedItems.Count == 1 && lstItemView.SelectedItems[0].Tag is SkeletalJoint)
        {
            SkeletalJoint joint = (SkeletalJoint)lstItemView.SelectedItems[0].Tag;
            driveChooser.ShowDialog(joint);
            updateList();
        }
    }
}

public enum FormState
{
    SUBMIT,
    CANCEL,
    CLOSE
}