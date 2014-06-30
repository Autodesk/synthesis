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
        this.lstItems = new System.Windows.Forms.ListBox();
        this.btnExport = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // lstItems
        // 
        this.lstItems.FormattingEnabled = true;
        this.lstItems.ItemHeight = 16;
        this.lstItems.Location = new System.Drawing.Point(12, 12);
        this.lstItems.Name = "lstItems";
        this.lstItems.Size = new System.Drawing.Size(936, 420);
        this.lstItems.TabIndex = 0;
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
        // ControlGroups
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(960, 493);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnExport);
        this.Controls.Add(this.lstItems);
        this.Name = "ControlGroups";
        this.Text = "Control Groups";
        this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ControlGroups_FormClosed);
        this.Load += new System.EventHandler(this.ControlGroups_Load);
        this.ResumeLayout(false);

    }
    internal System.Windows.Forms.ListBox lstItems;
    internal System.Windows.Forms.Button btnExport;
    internal System.Windows.Forms.Button btnCancel;
    public ControlGroups()
    {
        InitializeComponent();
    }
}