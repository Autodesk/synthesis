namespace FieldExporter.Components
{
    partial class SphereColliderPropertiesForm
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
            this.scaleLabel = new System.Windows.Forms.Label();
            this.scaleUpDown = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.scaleUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // scaleLabel
            // 
            this.scaleLabel.AutoSize = true;
            this.scaleLabel.Location = new System.Drawing.Point(3, 5);
            this.scaleLabel.Name = "scaleLabel";
            this.scaleLabel.Size = new System.Drawing.Size(47, 17);
            this.scaleLabel.TabIndex = 0;
            this.scaleLabel.Text = "Scale:";
            // 
            // scaleUpDown
            // 
            this.scaleUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scaleUpDown.DecimalPlaces = 2;
            this.scaleUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.scaleUpDown.Location = new System.Drawing.Point(56, 3);
            this.scaleUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.scaleUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.scaleUpDown.Name = "scaleUpDown";
            this.scaleUpDown.Size = new System.Drawing.Size(244, 22);
            this.scaleUpDown.TabIndex = 1;
            this.scaleUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // SphereColliderPropertiesForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.scaleUpDown);
            this.Controls.Add(this.scaleLabel);
            this.Name = "SphereColliderPropertiesForm";
            this.Size = new System.Drawing.Size(300, 28);
            ((System.ComponentModel.ISupportInitialize)(this.scaleUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label scaleLabel;
        private System.Windows.Forms.NumericUpDown scaleUpDown;
    }
}
