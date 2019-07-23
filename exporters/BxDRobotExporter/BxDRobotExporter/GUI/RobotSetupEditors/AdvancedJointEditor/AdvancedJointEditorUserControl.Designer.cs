using System.Windows.Forms;

namespace BxDRobotExporter
{
    public partial class AdvancedJointEditorUserControl
    {

        private System.ComponentModel.IContainer components;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lstJoints = new System.Windows.Forms.ListView();
            this.item_chType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.item_chChild = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.item_chDrive = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.item_chWheel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.item_chSensors = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.jointContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SuspendLayout();
            // 
            // lstJoints
            // 
            this.lstJoints.AutoArrange = false;
            this.lstJoints.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.item_chType,
            this.item_chChild,
            this.item_chDrive,
            this.item_chWheel,
            this.item_chSensors});
            this.lstJoints.ContextMenuStrip = this.jointContextMenu;
            this.lstJoints.Dock = System.Windows.Forms.DockStyle.Top;
            this.lstJoints.FullRowSelect = true;
            this.lstJoints.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstJoints.Location = new System.Drawing.Point(0, 0);
            this.lstJoints.Name = "lstJoints";
            this.lstJoints.ShowGroups = false;
            this.lstJoints.Size = new System.Drawing.Size(800, 548);
            this.lstJoints.TabIndex = 3;
            this.lstJoints.UseCompatibleStateImageBehavior = false;
            this.lstJoints.View = System.Windows.Forms.View.Details;
            this.lstJoints.SelectedIndexChanged += new System.EventHandler(this.LstJoints_SelectedIndexChanged);
            // 
            // item_chType
            // 
            this.item_chType.Text = "Joint Type";
            this.item_chType.Width = 100;
            // 
            // item_chChild
            // 
            this.item_chChild.Text = "Joint Name";
            this.item_chChild.Width = 80;
            // 
            // item_chDrive
            // 
            this.item_chDrive.Text = "Driver";
            this.item_chDrive.Width = 160;
            // 
            // item_chWheel
            // 
            this.item_chWheel.Text = "Wheel Type";
            this.item_chWheel.Width = 100;
            // 
            // item_chSensors
            // 
            this.item_chSensors.Text = "Sensor Count";
            this.item_chSensors.Width = 95;
            // 
            // jointContextMenu
            // 
            this.jointContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.jointContextMenu.Name = "jointContextMenu";
            this.jointContextMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.jointContextMenu.ShowImageMargin = false;
            this.jointContextMenu.Size = new System.Drawing.Size(36, 4);
            // 
            // JointEditorPane
            // 
            this.Controls.Add(this.lstJoints);
            this.Name = "AdvancedJointEditorUserControl";
            this.Size = new System.Drawing.Size(800, 551);
            this.ResumeLayout(false);
        }

        private ListView lstJoints;
        private ColumnHeader item_chType;
        private ColumnHeader item_chChild;
        private ColumnHeader item_chDrive;
        private ColumnHeader item_chWheel;
        private ColumnHeader item_chSensors;
        private ContextMenuStrip jointContextMenu;

    }
}