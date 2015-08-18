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
            this.buttonNew = new System.Windows.Forms.Button();
            this.panelJoints = new System.Windows.Forms.Panel();
            this.buttonShow = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonNew
            // 
            this.buttonNew.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonNew.Location = new System.Drawing.Point(421, 381);
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.Size = new System.Drawing.Size(344, 32);
            this.buttonNew.TabIndex = 0;
            this.buttonNew.Text = "Create New Group";
            this.buttonNew.UseVisualStyleBackColor = true;
            this.buttonNew.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // panelJoints
            // 
            this.panelJoints.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelJoints.Location = new System.Drawing.Point(4, 4);
            this.panelJoints.Name = "panelJoints";
            this.panelJoints.Size = new System.Drawing.Size(761, 371);
            this.panelJoints.TabIndex = 4;
            // 
            // buttonShow
            // 
            this.buttonShow.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonShow.Location = new System.Drawing.Point(3, 381);
            this.buttonShow.Name = "buttonShow";
            this.buttonShow.Size = new System.Drawing.Size(344, 32);
            this.buttonShow.TabIndex = 5;
            this.buttonShow.Text = "Show Unassigned Joints";
            this.buttonShow.UseVisualStyleBackColor = true;
            this.buttonShow.Click += new System.EventHandler(this.buttonShow_Click);
            // 
            // JointGroupPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonShow);
            this.Controls.Add(this.buttonNew);
            this.Controls.Add(this.panelJoints);
            this.Name = "JointGroupPane";
            this.Size = new System.Drawing.Size(768, 416);
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button buttonNew;
    private System.Windows.Forms.Panel panelJoints;
    private System.Windows.Forms.Button buttonShow;


}
