using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading;

public partial class ControlGroups
{
    public FormState formState;

    private List<RigidNode_Base> nodeList;
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

    private void UpdateJointList()
    {
        WheelDriverMeta wheelData = null;

        if ((nodeList == null))
            return;
        lstJoints.Items.Clear();
        foreach (RigidNode_Base node in nodeList)
        {
            if (node.GetSkeletalJoint() != null)
            {
                SkeletalJoint_Base joint = node.GetSkeletalJoint();
                if (joint != null)
                {
                    SkeletalJoint wrapped = (joint is InventorSkeletalJoint ? ((InventorSkeletalJoint) joint).GetWrapped() : null);

                    if (joint is RotationalJoint && joint.cDriver != null)
                    {
                        wheelData = ((RotationalJoint) joint).cDriver.GetInfo<WheelDriverMeta>();
                    }
                    else
                    {
                        wheelData = null;
                    }

                    System.Windows.Forms.ListViewItem item = new System.Windows.Forms.ListViewItem(new string[] { 
                    Enum.GetName(typeof(SkeletalJointType),joint.GetJointType()).ToLowerInvariant(),
                        wrapped!=null?wrapped.parentGroup.ToString():"from-file",
                        wrapped!=null?wrapped.childGroup.ToString():"from-file", joint.cDriver!=null?joint.cDriver.ToString():"No driver",
                        wheelData!=null?wheelData.GetTypeString():"No Wheel",
                        joint.attachedSensors.Count.ToString()});
                    item.Tag = node;
                    lstJoints.Items.Add(item);
                }
            }
        }
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
        UpdateJointList();
    }

    public void SetNodeList(List<RigidNode_Base> nodeList)
    {
        this.nodeList = nodeList;
        UpdateJointList();
    }

    public void SetGroupList(List<CustomRigidGroup> groupList)
    {
        this.groupList = groupList;
        UpdateGroupList();
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

    private void lstJoints_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (chkHighlightComponents.Checked && lstJoints.SelectedItems.Count == 1 && lstJoints.SelectedItems[0].Tag is RigidNode)
        {
            InventorSkeletalJoint joint = ((InventorSkeletalJoint) ((RigidNode) lstJoints.SelectedItems[0].Tag).GetSkeletalJoint());
            joint.GetWrapped().DoHighlight();
        }
        else
        {
            ComponentHighlighter.ClearHighlight();
        }
    }

    private void lstJoints_DoubleClick(object sender, EventArgs e)
    {
        if (lstJoints.SelectedItems.Count == 1 && lstJoints.SelectedItems[0].Tag is RigidNode)
        {
            SkeletalJoint_Base joint = ((RigidNode) lstJoints.SelectedItems[0].Tag).GetSkeletalJoint();
            driveChooser.ShowDialog(joint, (RigidNode) lstJoints.SelectedItems[0].Tag);
            UpdateJointList();
        }
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
        if (lstJoints.SelectedItems.Count == 1 && lstJoints.SelectedItems[0].Tag is RigidNode)
        {
            InventorSkeletalJoint joint = (InventorSkeletalJoint) ((RigidNode) lstJoints.SelectedItems[0].Tag).GetSkeletalJoint();
            joint.DetermineLimits();
        }
    }

    private void tabsMain_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (tabsMain.SelectedTab.Name == "tabJoints")
        {
            btnCalculate.Visible = true;
            addSensorButton.Visible = true;
            deleteSensorsButton.Visible = true;
        }
        else
        {
            btnCalculate.Visible = false;
            addSensorButton.Visible = false;
            deleteSensorsButton.Visible = false;
        }
    }

    private void SensorButton_Click(object sender, EventArgs e)
    {
        JointResolver.ControlGUI.AddSensorForm sensorForm = new JointResolver.ControlGUI.AddSensorForm(this, ((RigidNode) lstJoints.SelectedItems[0].Tag).GetSkeletalJoint());
        sensorForm.ShowDialog();
        this.UpdateJointList();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        JointResolver.ControlGUI.SensorListForm listForm = new JointResolver.ControlGUI.SensorListForm(((RigidNode)lstJoints.SelectedItems[0].Tag).GetSkeletalJoint());
        listForm.ShowDialog();
        this.UpdateJointList();
    }
}

public enum FormState
{
    SUBMIT,
    CANCEL,
    CLOSE
}