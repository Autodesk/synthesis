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
            this.DefineMovingPartsLabel = new System.Windows.Forms.Label();
            this.Step3InfoLabel = new System.Windows.Forms.Label();
            this.DefinePartsPanelLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // DefineMovingPartsLabel
            // 
            this.DefineMovingPartsLabel.AutoSize = true;
            this.DefineMovingPartsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DefineMovingPartsLabel.Location = new System.Drawing.Point(-4, 0);
            this.DefineMovingPartsLabel.Name = "DefineMovingPartsLabel";
            this.DefineMovingPartsLabel.Size = new System.Drawing.Size(283, 20);
            this.DefineMovingPartsLabel.TabIndex = 0;
            this.DefineMovingPartsLabel.Text = "Step 3: Define Other Moving Parts";
            // 
            // Step3InfoLabel
            // 
            this.Step3InfoLabel.Location = new System.Drawing.Point(3, 34);
            this.Step3InfoLabel.Name = "Step3InfoLabel";
            this.Step3InfoLabel.Size = new System.Drawing.Size(454, 33);
            this.Step3InfoLabel.TabIndex = 1;
            this.Step3InfoLabel.Text = "If you have any other moving parts on your robot that you believe would be useful" +
    " to have in the simulation, set them up here by checking any of the nodes on the" +
    " list.\r\n";
            // 
            // DefinePartsPanelLayout
            // 
            this.DefinePartsPanelLayout.AutoScroll = true;
            this.DefinePartsPanelLayout.Location = new System.Drawing.Point(0, 70);
            this.DefinePartsPanelLayout.Name = "DefinePartsPanelLayout";
            this.DefinePartsPanelLayout.Size = new System.Drawing.Size(460, 583);
            this.DefinePartsPanelLayout.TabIndex = 2;
            // 
            // DefineMovingPartsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DefinePartsPanelLayout);
            this.Controls.Add(this.Step3InfoLabel);
            this.Controls.Add(this.DefineMovingPartsLabel);
            this.Name = "DefineMovingPartsPage";
            this.Size = new System.Drawing.Size(460, 653);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label DefineMovingPartsLabel;
        private System.Windows.Forms.Label Step3InfoLabel;
        private System.Windows.Forms.FlowLayoutPanel DefinePartsPanelLayout;
    }
}
