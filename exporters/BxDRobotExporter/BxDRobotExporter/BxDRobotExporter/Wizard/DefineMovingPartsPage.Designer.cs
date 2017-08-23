namespace BxDRobotExporter.Wizard
{
    partial class DefineMovingPartsPage
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
            this.DefineMovingParts = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // DefineMovingParts
            // 
            this.DefineMovingParts.AutoSize = true;
            this.DefineMovingParts.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DefineMovingParts.Location = new System.Drawing.Point(-4, 0);
            this.DefineMovingParts.Name = "DefineMovingParts";
            this.DefineMovingParts.Size = new System.Drawing.Size(283, 20);
            this.DefineMovingParts.TabIndex = 0;
            this.DefineMovingParts.Text = "Step 3: Define Other Moving Parts";
            // 
            // DefineMovingPartsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DefineMovingParts);
            this.Name = "DefineMovingPartsPage";
            this.Size = new System.Drawing.Size(460, 653);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label DefineMovingParts;
    }
}
