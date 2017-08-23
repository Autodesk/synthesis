namespace BxDRobotExporter.Wizard
{
    partial class MassDataRow
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
            this.SplitContainer = new System.Windows.Forms.SplitContainer();
            this.NodeLabel = new System.Windows.Forms.Label();
            this.MassControl = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
            this.SplitContainer.Panel1.SuspendLayout();
            this.SplitContainer.Panel2.SuspendLayout();
            this.SplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MassControl)).BeginInit();
            this.SuspendLayout();
            // 
            // SplitContainer
            // 
            this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitContainer.IsSplitterFixed = true;
            this.SplitContainer.Location = new System.Drawing.Point(0, 0);
            this.SplitContainer.Name = "SplitContainer";
            // 
            // SplitContainer.Panel1
            // 
            this.SplitContainer.Panel1.BackColor = System.Drawing.Color.White;
            this.SplitContainer.Panel1.Controls.Add(this.NodeLabel);
            // 
            // SplitContainer.Panel2
            // 
            this.SplitContainer.Panel2.Controls.Add(this.MassControl);
            this.SplitContainer.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.SplitContainer.Size = new System.Drawing.Size(327, 20);
            this.SplitContainer.SplitterDistance = 79;
            this.SplitContainer.TabIndex = 0;
            // 
            // NodeLabel
            // 
            this.NodeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NodeLabel.Location = new System.Drawing.Point(0, 0);
            this.NodeLabel.Name = "NodeLabel";
            this.NodeLabel.Size = new System.Drawing.Size(79, 20);
            this.NodeLabel.TabIndex = 0;
            this.NodeLabel.Text = "node__.bxda";
            this.NodeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MassControl
            // 
            this.MassControl.DecimalPlaces = 1;
            this.MassControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MassControl.Location = new System.Drawing.Point(0, 0);
            this.MassControl.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            0});
            this.MassControl.Name = "MassControl";
            this.MassControl.Size = new System.Drawing.Size(244, 20);
            this.MassControl.TabIndex = 0;
            // 
            // MassDataRow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SplitContainer);
            this.Name = "MassDataRow";
            this.Size = new System.Drawing.Size(327, 20);
            this.SplitContainer.Panel1.ResumeLayout(false);
            this.SplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
            this.SplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MassControl)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label NodeLabel;
        public System.Windows.Forms.SplitContainer SplitContainer;
        private System.Windows.Forms.NumericUpDown MassControl;
    }
}
