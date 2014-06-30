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

    private void lstItems_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lstItems.SelectedItem is SkeletalJoint)
        {
            SkeletalJoint joint = (SkeletalJoint) lstItems.SelectedItem;
            joint.DoHighlight();
        }
    }

    private void updateList()
    {
        if ((nodeList == null))
            return;
        lstItems.Items.Clear();
        foreach (RigidNode node in nodeList)
        {
            if (!((node.parentConnection == null) || (node.parent == null)))
            {
                if ((node.parentConnection.skeletalJoint == null))
                {
                    node.parentConnection.skeletalJoint = SkeletalJoint.create(node.parentConnection, node.parent.@group);
                }
                lstItems.Items.Add(node.parentConnection.skeletalJoint);
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
}

public enum FormState
{
    SUBMIT,
    CANCEL,
    CLOSE
}