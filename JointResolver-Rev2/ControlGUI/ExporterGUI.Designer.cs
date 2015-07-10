using EditorsLibrary;

partial class ExporterGUI
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.splitContainer1 = new System.Windows.Forms.SplitContainer();
        this.splitContainer2 = new System.Windows.Forms.SplitContainer();
        this.bxdaEditorPane1 = new EditorsLibrary.BXDAEditorPane();
        this.robotViewer1 = new EditorsLibrary.RobotViewer();
        this.jointEditorPane1 = new EditorsLibrary.JointEditorPane();
        ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
        this.splitContainer1.Panel1.SuspendLayout();
        this.splitContainer1.Panel2.SuspendLayout();
        this.splitContainer1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
        this.splitContainer2.Panel1.SuspendLayout();
        this.splitContainer2.Panel2.SuspendLayout();
        this.splitContainer2.SuspendLayout();
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
        this.splitContainer1.Panel1.Controls.Add(this.bxdaEditorPane1);
        // 
        // splitContainer1.Panel2
        // 
        this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
        this.splitContainer1.Size = new System.Drawing.Size(1182, 723);
        this.splitContainer1.SplitterDistance = 414;
        this.splitContainer1.TabIndex = 0;
        // 
        // splitContainer2
        // 
        this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitContainer2.Location = new System.Drawing.Point(0, 0);
        this.splitContainer2.Name = "splitContainer2";
        this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
        // 
        // splitContainer2.Panel1
        // 
        this.splitContainer2.Panel1.Controls.Add(this.robotViewer1);
        this.splitContainer2.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
        // 
        // splitContainer2.Panel2
        // 
        this.splitContainer2.Panel2.Controls.Add(this.jointEditorPane1);
        this.splitContainer2.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.splitContainer2.RightToLeft = System.Windows.Forms.RightToLeft.No;
        this.splitContainer2.Size = new System.Drawing.Size(764, 723);
        this.splitContainer2.SplitterDistance = 500;
        this.splitContainer2.TabIndex = 0;
        // 
        // bxdaEditorPane1
        // 
        this.bxdaEditorPane1.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
        this.bxdaEditorPane1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.bxdaEditorPane1.Location = new System.Drawing.Point(0, 0);
        this.bxdaEditorPane1.Name = "bxdaEditorPane1";
        this.bxdaEditorPane1.Size = new System.Drawing.Size(414, 723);
        this.bxdaEditorPane1.TabIndex = 0;
        // 
        // robotViewer1
        // 
        this.robotViewer1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.robotViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.robotViewer1.Location = new System.Drawing.Point(0, 0);
        this.robotViewer1.Name = "robotViewer1";
        this.robotViewer1.Size = new System.Drawing.Size(764, 500);
        this.robotViewer1.TabIndex = 0;
        // 
        // jointEditorPane1
        // 
        this.jointEditorPane1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.jointEditorPane1.Location = new System.Drawing.Point(0, 0);
        this.jointEditorPane1.Name = "jointEditorPane1";
        this.jointEditorPane1.Size = new System.Drawing.Size(764, 219);
        this.jointEditorPane1.TabIndex = 0;
        // 
        // ExporterGUI
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1182, 723);
        this.Controls.Add(this.splitContainer1);
        this.MinimumSize = new System.Drawing.Size(1200, 768);
        this.Name = "ExporterGUI";
        this.Text = "ExporterGUI";
        this.splitContainer1.Panel1.ResumeLayout(false);
        this.splitContainer1.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
        this.splitContainer1.ResumeLayout(false);
        this.splitContainer2.Panel1.ResumeLayout(false);
        this.splitContainer2.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
        this.splitContainer2.ResumeLayout(false);
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private RobotViewer robotViewer1;
    private BXDAEditorPane bxdaEditorPane1;
    private JointEditorPane jointEditorPane1;
}
