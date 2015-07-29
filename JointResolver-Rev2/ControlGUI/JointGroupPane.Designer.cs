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
            this.panelJoints = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panelJoints
            // 
            this.panelJoints.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelJoints.Location = new System.Drawing.Point(0, 0);
            this.panelJoints.Name = "panelJoints";
            this.panelJoints.Size = new System.Drawing.Size(774, 422);
            this.panelJoints.TabIndex = 1;
            // 
            // JointGroupPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelJoints);
            this.Name = "JointGroupPane";
            this.Size = new System.Drawing.Size(774, 422);
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panelJoints;


}
