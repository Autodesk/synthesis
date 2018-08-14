namespace InternalFieldExporter.FieldWizard.Components
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
            this.scaleLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.scaleUpDown)).BeginInit();
            this.scaleLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // scaleLabel
            // 
            this.scaleLabel.AutoSize = true;
            this.scaleLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.scaleLabel.Location = new System.Drawing.Point(3, 3);
            this.scaleLabel.Margin = new System.Windows.Forms.Padding(3);
            this.scaleLabel.Name = "scaleLabel";
            this.scaleLabel.Size = new System.Drawing.Size(37, 20);
            this.scaleLabel.TabIndex = 0;
            this.scaleLabel.Text = "Scale:";
            this.scaleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // scaleUpDown
            // 
            this.scaleUpDown.DecimalPlaces = 2;
            this.scaleUpDown.Dock = System.Windows.Forms.DockStyle.Top;
            this.scaleUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.scaleUpDown.Location = new System.Drawing.Point(46, 3);
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
            this.scaleUpDown.Size = new System.Drawing.Size(101, 20);
            this.scaleUpDown.TabIndex = 1;
            this.scaleUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // scaleLayoutPanel
            // 
            this.scaleLayoutPanel.AutoSize = true;
            this.scaleLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.scaleLayoutPanel.ColumnCount = 2;
            this.scaleLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.scaleLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.scaleLayoutPanel.Controls.Add(this.scaleLabel, 0, 0);
            this.scaleLayoutPanel.Controls.Add(this.scaleUpDown, 1, 0);
            this.scaleLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.scaleLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.scaleLayoutPanel.Name = "scaleLayoutPanel";
            this.scaleLayoutPanel.RowCount = 1;
            this.scaleLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.scaleLayoutPanel.Size = new System.Drawing.Size(150, 26);
            this.scaleLayoutPanel.TabIndex = 2;
            // 
            // SphereColliderPropertiesForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.scaleLayoutPanel);
            this.MinimumSize = new System.Drawing.Size(150, 0);
            this.Name = "SphereColliderPropertiesForm";
            this.Size = new System.Drawing.Size(150, 26);
            ((System.ComponentModel.ISupportInitialize)(this.scaleUpDown)).EndInit();
            this.scaleLayoutPanel.ResumeLayout(false);
            this.scaleLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label scaleLabel;
        private System.Windows.Forms.NumericUpDown scaleUpDown;
        private System.Windows.Forms.TableLayoutPanel scaleLayoutPanel;
    }
}
