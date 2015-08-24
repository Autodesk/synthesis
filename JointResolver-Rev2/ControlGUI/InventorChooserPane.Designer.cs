partial class InventorChooserPane
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
        this.buttonSelect = new System.Windows.Forms.Button();
        this.buttonAdd = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // buttonSelect
        // 
        this.buttonSelect.Location = new System.Drawing.Point(3, 381);
        this.buttonSelect.Name = "buttonSelect";
        this.buttonSelect.Size = new System.Drawing.Size(344, 32);
        this.buttonSelect.TabIndex = 0;
        this.buttonSelect.Text = "Select in Inventor";
        this.buttonSelect.UseVisualStyleBackColor = true;
        this.buttonSelect.Click += new System.EventHandler(this.buttonSelect_Click);
        // 
        // buttonAdd
        // 
        this.buttonAdd.Enabled = false;
        this.buttonAdd.Location = new System.Drawing.Point(421, 381);
        this.buttonAdd.Name = "buttonAdd";
        this.buttonAdd.Size = new System.Drawing.Size(344, 32);
        this.buttonAdd.TabIndex = 1;
        this.buttonAdd.Text = "Add selection";
        this.buttonAdd.UseVisualStyleBackColor = true;
        this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
        // 
        // InventorChooserPane
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.buttonAdd);
        this.Controls.Add(this.buttonSelect);
        this.Name = "InventorChooserPane";
        this.Size = new System.Drawing.Size(768, 416);
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button buttonSelect;
    private System.Windows.Forms.Button buttonAdd;
}
