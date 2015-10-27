namespace FieldExporter.Components
{
    partial class MeshColliderPropertiesForm
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
            this.convexCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // convexCheckBox
            // 
            this.convexCheckBox.AutoSize = true;
            this.convexCheckBox.Checked = true;
            this.convexCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.convexCheckBox.Location = new System.Drawing.Point(3, 3);
            this.convexCheckBox.Name = "convexCheckBox";
            this.convexCheckBox.Size = new System.Drawing.Size(76, 21);
            this.convexCheckBox.TabIndex = 0;
            this.convexCheckBox.Text = "Convex";
            this.convexCheckBox.UseVisualStyleBackColor = true;
            // 
            // MeshColliderPropertiesForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.convexCheckBox);
            this.Name = "MeshColliderPropertiesForm";
            this.Size = new System.Drawing.Size(300, 27);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox convexCheckBox;

    }
}
