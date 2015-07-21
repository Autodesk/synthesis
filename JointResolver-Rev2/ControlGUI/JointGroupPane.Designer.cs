partial class JointGroupPane
{
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.splitContainer1 = new System.Windows.Forms.SplitContainer();
        this.panelJoints = new System.Windows.Forms.Panel();
        this.treeviewInventor = new InventorTreeView();
        ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
        this.splitContainer1.Panel1.SuspendLayout();
        this.splitContainer1.Panel2.SuspendLayout();
        this.splitContainer1.SuspendLayout();
        this.SuspendLayout();
        // 
        // splitContainer1
        // 
        this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitContainer1.Location = new System.Drawing.Point(0, 0);
        this.splitContainer1.Name = "splitContainer1";
        // 
        // splitContainer1.Panel1
        // 
        this.splitContainer1.Panel1.Controls.Add(this.treeviewInventor);
        // 
        // splitContainer1.Panel2
        // 
        this.splitContainer1.Panel2.Controls.Add(this.panelJoints);
        this.splitContainer1.Size = new System.Drawing.Size(774, 422);
        this.splitContainer1.SplitterDistance = 251;
        this.splitContainer1.TabIndex = 0;
        // 
        // panelJoints
        // 
        this.panelJoints.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panelJoints.Location = new System.Drawing.Point(0, 0);
        this.panelJoints.Name = "panelJoints";
        this.panelJoints.Size = new System.Drawing.Size(519, 422);
        this.panelJoints.TabIndex = 0;
        // 
        // treeviewInventor
        // 
        this.treeviewInventor.Dock = System.Windows.Forms.DockStyle.Fill;
        this.treeviewInventor.Location = new System.Drawing.Point(0, 0);
        this.treeviewInventor.Name = "treeviewInventor";
        this.treeviewInventor.Size = new System.Drawing.Size(251, 422);
        this.treeviewInventor.TabIndex = 0;
        // 
        // JointGroupPane
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.splitContainer1);
        this.Name = "JointGroupPane";
        this.Size = new System.Drawing.Size(774, 422);
        this.splitContainer1.Panel1.ResumeLayout(false);
        this.splitContainer1.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
        this.splitContainer1.ResumeLayout(false);
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.Panel panelJoints;
    private InventorTreeView treeviewInventor;

}
