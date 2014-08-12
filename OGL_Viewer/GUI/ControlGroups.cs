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
    private List<RigidNode_Base> groupList;
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

    private void UpdateGroupList()
    {
        if (groupList == null)
            return;
        lstGroups.Items.Clear();
        //foreach (BXDAMesh mesh in groupList)
        //{
        //    System.Windows.Forms.ListViewItem item = new System.Windows.Forms.ListViewItem(new string[] {mesh.,
        //        group.grounded?"Yes":"No",group.colorFaces?"Yes":"No", group.highRes?"Yes":"No", group.convex?"Convex":"Concave"});
        //    item.Tag = group;
        //    lstGroups.Items.Add(item);
        //}
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

        tabsMain.Height = newTabHeight;
        tabsMain.Width = newTabWidth;
        lstGroups.Height = newListHeight;
        lstGroups.Width = newListWidth;
        jointPane.Height = newListHeight;
        jointPane.Width = newListWidth;
    }

    private void lstGroups_SelectedIndexChanged(object sender, EventArgs e)
    {
        //if (chkHighlightComponents.Checked && lstGroups.SelectedItems.Count == 1 && lstGroups.SelectedItems[0].Tag is CustomRigidGroup)
        //{
        //    CustomRigidGroup group = (CustomRigidGroup) lstGroups.SelectedItems[0].Tag;
        //    ComponentHighlighter.PrepareHighlight();
        //    ComponentHighlighter.ClearHighlight();
        //    foreach (Inventor.ComponentOccurrence child in group.occurrences)
        //    {
        //        ComponentHighlighter.CHILD_HIGHLIGHT_SET.AddItem(child);
        //    }
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