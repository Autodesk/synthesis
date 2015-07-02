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
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabsMain = new System.Windows.Forms.TabControl();
            this.tabGroups = new System.Windows.Forms.TabPage();
            this.lstGroups = new System.Windows.Forms.ListView();
            this.groups_chName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groups_chSubmesh = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groups_chSubTris = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groups_chCollider = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groups_chCollTris = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabJoints = new System.Windows.Forms.TabPage();
            this.jointPane = new EditorsLibrary.JointEditorPane();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.bxdaEditorPane1 = new EditorsLibrary.BXDAEditorPane();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.pnlOutput = new System.Windows.Forms.Panel();
            this.btnExportDrivers = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.tabsMain.SuspendLayout();
            this.tabGroups.SuspendLayout();
            this.tabJoints.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.pnlOutput.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.Location = new System.Drawing.Point(12, 687);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(130, 42);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tabsMain
            // 
            this.tabsMain.Controls.Add(this.tabGroups);
            this.tabsMain.Controls.Add(this.tabJoints);
            this.tabsMain.Controls.Add(this.tabPage1);
            this.tabsMain.Location = new System.Drawing.Point(13, 13);
            this.tabsMain.Name = "tabsMain";
            this.tabsMain.SelectedIndex = 0;
            this.tabsMain.Size = new System.Drawing.Size(935, 668);
            this.tabsMain.TabIndex = 4;
            this.tabsMain.SelectedIndexChanged += new System.EventHandler(this.tabsMain_SelectedIndexChanged);
            // 
            // tabGroups
            // 
            this.tabGroups.Controls.Add(this.lstGroups);
            this.tabGroups.Location = new System.Drawing.Point(4, 25);
            this.tabGroups.Name = "tabGroups";
            this.tabGroups.Padding = new System.Windows.Forms.Padding(3);
            this.tabGroups.Size = new System.Drawing.Size(927, 639);
            this.tabGroups.TabIndex = 0;
            this.tabGroups.Text = "Object Groups";
            this.tabGroups.UseVisualStyleBackColor = true;
            // 
            // lstGroups
            // 
            this.lstGroups.AutoArrange = false;
            this.lstGroups.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.groups_chName,
            this.groups_chSubmesh,
            this.groups_chSubTris,
            this.groups_chCollider,
            this.groups_chCollTris});
            this.lstGroups.FullRowSelect = true;
            this.lstGroups.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstGroups.Location = new System.Drawing.Point(0, 0);
            this.lstGroups.MultiSelect = false;
            this.lstGroups.Name = "lstGroups";
            this.lstGroups.ShowGroups = false;
            this.lstGroups.Size = new System.Drawing.Size(924, 633);
            this.lstGroups.TabIndex = 4;
            this.lstGroups.UseCompatibleStateImageBehavior = false;
            this.lstGroups.View = System.Windows.Forms.View.Details;
            this.lstGroups.SelectedIndexChanged += new System.EventHandler(this.lstGroups_SelectedIndexChanged);
            // 
            // groups_chName
            // 
            this.groups_chName.Text = "Name";
            this.groups_chName.Width = 98;
            // 
            // groups_chSubmesh
            // 
            this.groups_chSubmesh.Text = "Submesh Count";
            this.groups_chSubmesh.Width = 115;
            // 
            // groups_chSubTris
            // 
            this.groups_chSubTris.Text = "Submesh Triangles";
            this.groups_chSubTris.Width = 136;
            // 
            // groups_chCollider
            // 
            this.groups_chCollider.Text = "Collider Count";
            this.groups_chCollider.Width = 103;
            // 
            // groups_chCollTris
            // 
            this.groups_chCollTris.Text = "Collider Triangles";
            this.groups_chCollTris.Width = 124;
            // 
            // tabJoints
            // 
            this.tabJoints.Controls.Add(this.jointPane);
            this.tabJoints.Location = new System.Drawing.Point(4, 25);
            this.tabJoints.Name = "tabJoints";
            this.tabJoints.Padding = new System.Windows.Forms.Padding(3);
            this.tabJoints.Size = new System.Drawing.Size(927, 639);
            this.tabJoints.TabIndex = 1;
            this.tabJoints.Text = "Joint Options";
            this.tabJoints.UseVisualStyleBackColor = true;
            // 
            // jointPane
            // 
            this.jointPane.Location = new System.Drawing.Point(0, 0);
            this.jointPane.Name = "jointPane";
            this.jointPane.Size = new System.Drawing.Size(927, 639);
            this.jointPane.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.bxdaEditorPane1);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(927, 639);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "Mesh Properties";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // bxdaEditorPane1
            // 
            this.bxdaEditorPane1.Location = new System.Drawing.Point(0, 0);
            this.bxdaEditorPane1.Name = "bxdaEditorPane1";
            this.bxdaEditorPane1.Size = new System.Drawing.Size(927, 639);
            this.bxdaEditorPane1.TabIndex = 0;
            this.bxdaEditorPane1.Load += new System.EventHandler(this.bxdaEditorPane1_Load);
            // 
            // txtFilePath
            // 
            this.txtFilePath.Enabled = false;
            this.txtFilePath.Location = new System.Drawing.Point(130, 16);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(201, 22);
            this.txtFilePath.TabIndex = 6;
            this.txtFilePath.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(337, 16);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 7;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // pnlOutput
            // 
            this.pnlOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlOutput.Controls.Add(this.btnExportDrivers);
            this.pnlOutput.Controls.Add(this.txtFilePath);
            this.pnlOutput.Controls.Add(this.btnBrowse);
            this.pnlOutput.Location = new System.Drawing.Point(380, 687);
            this.pnlOutput.Name = "pnlOutput";
            this.pnlOutput.Size = new System.Drawing.Size(568, 53);
            this.pnlOutput.TabIndex = 8;
            // 
            // btnExportDrivers
            // 
            this.btnExportDrivers.Location = new System.Drawing.Point(19, 12);
            this.btnExportDrivers.Name = "btnExportDrivers";
            this.btnExportDrivers.Size = new System.Drawing.Size(105, 31);
            this.btnExportDrivers.TabIndex = 9;
            this.btnExportDrivers.Text = "Import Drivers";
            this.btnExportDrivers.UseVisualStyleBackColor = true;
            this.btnExportDrivers.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(816, 687);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(132, 42);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // ControlGroups
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 741);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.tabsMain);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.pnlOutput);
            this.MinimumSize = new System.Drawing.Size(800, 300);
            this.Name = "ControlGroups";
            this.Text = "Control Groups";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ControlGroups_FormClosed);
            this.Load += new System.EventHandler(this.ControlGroups_Load);
            this.SizeChanged += new System.EventHandler(this.window_SizeChanged);
            this.tabsMain.ResumeLayout(false);
            this.tabGroups.ResumeLayout(false);
            this.tabJoints.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.pnlOutput.ResumeLayout(false);
            this.pnlOutput.PerformLayout();
            this.ResumeLayout(false);

    }
    internal System.Windows.Forms.Button btnCancel;

    private System.Windows.Forms.TabControl tabsMain;
    private System.Windows.Forms.TabPage tabGroups;
    private System.Windows.Forms.TabPage tabJoints;
    private System.Windows.Forms.ListView lstGroups;
    private System.Windows.Forms.ColumnHeader groups_chName;
    private System.Windows.Forms.ColumnHeader groups_chCollider;
    private System.Windows.Forms.ColumnHeader groups_chSubmesh;
    public EditorsLibrary.JointEditorPane jointPane;
    private System.Windows.Forms.ColumnHeader groups_chSubTris;
    private System.Windows.Forms.ColumnHeader groups_chCollTris;
    private System.Windows.Forms.TextBox txtFilePath;
    private System.Windows.Forms.Button btnBrowse;
    private System.Windows.Forms.Panel pnlOutput;
    private System.Windows.Forms.Button btnExportDrivers;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.TabPage tabPage1;
    public EditorsLibrary.BXDAEditorPane bxdaEditorPane1;
}