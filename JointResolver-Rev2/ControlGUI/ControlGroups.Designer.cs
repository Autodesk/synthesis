using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

partial class ControlGroups : System.Windows.Forms.Form
{

    //Form overrides dispose to clean up the component list.
    [System.Diagnostics.DebuggerNonUserCode()]
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    //NOTE: The following procedure is required by the Windows Form Designer
    //It can be modified using the Windows Form Designer.  
    //Do not modify it using the code editor.
    [System.Diagnostics.DebuggerStepThrough()]
    private void InitializeComponent()
    {
            this.btnExport = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lstItemView = new System.Windows.Forms.ListView();
            this.item_chType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.item_chParent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.item_chChild = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.item_chDrive = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabsMain = new System.Windows.Forms.TabControl();
            this.tabGroups = new System.Windows.Forms.TabPage();
            this.tabJoints = new System.Windows.Forms.TabPage();
            this.tabsMain.SuspendLayout();
            this.tabJoints.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(818, 439);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(130, 42);
            this.btnExport.TabIndex = 1;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(12, 439);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(130, 42);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lstItemView
            // 
            this.lstItemView.AutoArrange = false;
            this.lstItemView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.item_chType,
            this.item_chParent,
            this.item_chChild,
            this.item_chDrive});
            this.lstItemView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstItemView.Location = new System.Drawing.Point(6, 6);
            this.lstItemView.MultiSelect = false;
            this.lstItemView.Name = "lstItemView";
            this.lstItemView.ShowGroups = false;
            this.lstItemView.Size = new System.Drawing.Size(915, 378);
            this.lstItemView.TabIndex = 3;
            this.lstItemView.UseCompatibleStateImageBehavior = false;
            this.lstItemView.View = System.Windows.Forms.View.Details;
            this.lstItemView.SelectedIndexChanged += new System.EventHandler(this.lstItemView_SelectedIndexChanged);
            this.lstItemView.DoubleClick += new System.EventHandler(this.lstItemView_DoubleClick);
            // 
            // item_chType
            // 
            this.item_chType.Text = "Joint Type";
            // 
            // item_chParent
            // 
            this.item_chParent.Text = "Fixed";
            // 
            // item_chChild
            // 
            this.item_chChild.Text = "Child";
            // 
            // item_chDrive
            // 
            this.item_chDrive.Text = "Driver";
            // 
            // tabsMain
            // 
            this.tabsMain.Controls.Add(this.tabGroups);
            this.tabsMain.Controls.Add(this.tabJoints);
            this.tabsMain.Location = new System.Drawing.Point(13, 13);
            this.tabsMain.Name = "tabsMain";
            this.tabsMain.SelectedIndex = 0;
            this.tabsMain.Size = new System.Drawing.Size(935, 419);
            this.tabsMain.TabIndex = 4;
            // 
            // tabGroups
            // 
            this.tabGroups.Location = new System.Drawing.Point(4, 25);
            this.tabGroups.Name = "tabGroups";
            this.tabGroups.Padding = new System.Windows.Forms.Padding(3);
            this.tabGroups.Size = new System.Drawing.Size(842, 307);
            this.tabGroups.TabIndex = 0;
            this.tabGroups.Text = "Object Groups";
            this.tabGroups.UseVisualStyleBackColor = true;
            // 
            // tabJoints
            // 
            this.tabJoints.Controls.Add(this.lstItemView);
            this.tabJoints.Location = new System.Drawing.Point(4, 25);
            this.tabJoints.Name = "tabJoints";
            this.tabJoints.Padding = new System.Windows.Forms.Padding(3);
            this.tabJoints.Size = new System.Drawing.Size(927, 390);
            this.tabJoints.TabIndex = 1;
            this.tabJoints.Text = "Joint Drivers";
            this.tabJoints.UseVisualStyleBackColor = true;
            // 
            // ControlGroups
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 493);
            this.Controls.Add(this.tabsMain);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnExport);
            this.Name = "ControlGroups";
            this.Text = "Control Groups";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ControlGroups_FormClosed);
            this.Load += new System.EventHandler(this.ControlGroups_Load);
            this.tabsMain.ResumeLayout(false);
            this.tabJoints.ResumeLayout(false);
            this.ResumeLayout(false);

    }
    internal System.Windows.Forms.Button btnExport;
    internal System.Windows.Forms.Button btnCancel;
    public ControlGroups()
    {
        InitializeComponent();
    }

    private System.Windows.Forms.ListView lstItemView;
    private System.Windows.Forms.ColumnHeader item_chType;
    private System.Windows.Forms.ColumnHeader item_chParent;
    private System.Windows.Forms.ColumnHeader item_chChild;
    private System.Windows.Forms.ColumnHeader item_chDrive;
    private System.Windows.Forms.TabControl tabsMain;
    private System.Windows.Forms.TabPage tabGroups;
    private System.Windows.Forms.TabPage tabJoints;
}