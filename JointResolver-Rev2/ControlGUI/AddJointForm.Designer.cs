partial class AddJointForm
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
        this.comboboxChooseJoint = new System.Windows.Forms.ComboBox();
        this.buttonCancel = new System.Windows.Forms.Button();
        this.buttonOK = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // comboboxChooseJoint
        // 
        this.comboboxChooseJoint.Font = new System.Drawing.Font("Lucida Sans Typewriter", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.comboboxChooseJoint.FormattingEnabled = true;
        this.comboboxChooseJoint.Items.AddRange(new object[] {
            "Rotational",
            "Linear",
            "Planar",
            "Cylindrical",
            "Ball"});
        this.comboboxChooseJoint.Location = new System.Drawing.Point(60, 40);
        this.comboboxChooseJoint.Name = "comboboxChooseJoint";
        this.comboboxChooseJoint.Size = new System.Drawing.Size(163, 23);
        this.comboboxChooseJoint.TabIndex = 0;
        this.comboboxChooseJoint.Text = "Choose joint type";
        // 
        // buttonCancel
        // 
        this.buttonCancel.Location = new System.Drawing.Point(12, 114);
        this.buttonCancel.Name = "buttonCancel";
        this.buttonCancel.Size = new System.Drawing.Size(97, 32);
        this.buttonCancel.TabIndex = 2;
        this.buttonCancel.Text = "Cancel";
        this.buttonCancel.UseVisualStyleBackColor = true;
        this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
        // 
        // buttonOK
        // 
        this.buttonOK.Location = new System.Drawing.Point(173, 114);
        this.buttonOK.Name = "buttonOK";
        this.buttonOK.Size = new System.Drawing.Size(97, 32);
        this.buttonOK.TabIndex = 3;
        this.buttonOK.Text = "OK";
        this.buttonOK.UseVisualStyleBackColor = true;
        this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
        // 
        // AddJointForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(282, 155);
        this.Controls.Add(this.buttonOK);
        this.Controls.Add(this.buttonCancel);
        this.Controls.Add(this.comboboxChooseJoint);
        this.MaximumSize = new System.Drawing.Size(300, 200);
        this.MinimumSize = new System.Drawing.Size(300, 200);
        this.Name = "AddJointForm";
        this.ShowIcon = false;
        this.ShowInTaskbar = false;
        this.Text = "Add Joint";
        this.TopMost = true;
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ComboBox comboboxChooseJoint;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.Button buttonOK;
}
