namespace InternalFieldExporter.FieldWizard.Components
{
    partial class BoxColliderPropertiesForm
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
            this.scaleGroupBox = new System.Windows.Forms.GroupBox();
            this.scaleTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.xScaleLabel = new System.Windows.Forms.Label();
            this.zScaleUpDown = new System.Windows.Forms.NumericUpDown();
            this.yScaleLabel = new System.Windows.Forms.Label();
            this.yScaleUpDown = new System.Windows.Forms.NumericUpDown();
            this.zScaleLabel = new System.Windows.Forms.Label();
            this.xScaleUpDown = new System.Windows.Forms.NumericUpDown();
            this.scaleGroupBox.SuspendLayout();
            this.scaleTableLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zScaleUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yScaleUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xScaleUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // scaleGroupBox
            // 
            this.scaleGroupBox.AutoSize = true;
            this.scaleGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.scaleGroupBox.Controls.Add(this.scaleTableLayout);
            this.scaleGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.scaleGroupBox.Location = new System.Drawing.Point(0, 0);
            this.scaleGroupBox.Margin = new System.Windows.Forms.Padding(2);
            this.scaleGroupBox.Name = "scaleGroupBox";
            this.scaleGroupBox.Padding = new System.Windows.Forms.Padding(2);
            this.scaleGroupBox.Size = new System.Drawing.Size(100, 89);
            this.scaleGroupBox.TabIndex = 0;
            this.scaleGroupBox.TabStop = false;
            this.scaleGroupBox.Text = "Scale";
            // 
            // scaleTableLayout
            // 
            this.scaleTableLayout.AutoSize = true;
            this.scaleTableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.scaleTableLayout.ColumnCount = 2;
            this.scaleTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.scaleTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.scaleTableLayout.Controls.Add(this.xScaleLabel, 0, 0);
            this.scaleTableLayout.Controls.Add(this.zScaleUpDown, 1, 2);
            this.scaleTableLayout.Controls.Add(this.yScaleLabel, 0, 1);
            this.scaleTableLayout.Controls.Add(this.yScaleUpDown, 1, 1);
            this.scaleTableLayout.Controls.Add(this.zScaleLabel, 0, 2);
            this.scaleTableLayout.Controls.Add(this.xScaleUpDown, 1, 0);
            this.scaleTableLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.scaleTableLayout.Location = new System.Drawing.Point(2, 15);
            this.scaleTableLayout.Name = "scaleTableLayout";
            this.scaleTableLayout.RowCount = 3;
            this.scaleTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.scaleTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.scaleTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.scaleTableLayout.Size = new System.Drawing.Size(96, 72);
            this.scaleTableLayout.TabIndex = 6;
            // 
            // xScaleLabel
            // 
            this.xScaleLabel.AutoSize = true;
            this.xScaleLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.xScaleLabel.Location = new System.Drawing.Point(2, 0);
            this.xScaleLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.xScaleLabel.Name = "xScaleLabel";
            this.xScaleLabel.Size = new System.Drawing.Size(17, 24);
            this.xScaleLabel.TabIndex = 0;
            this.xScaleLabel.Text = "X:";
            this.xScaleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // zScaleUpDown
            // 
            this.zScaleUpDown.DecimalPlaces = 2;
            this.zScaleUpDown.Dock = System.Windows.Forms.DockStyle.Top;
            this.zScaleUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.zScaleUpDown.Location = new System.Drawing.Point(23, 50);
            this.zScaleUpDown.Margin = new System.Windows.Forms.Padding(2);
            this.zScaleUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.zScaleUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.zScaleUpDown.Name = "zScaleUpDown";
            this.zScaleUpDown.Size = new System.Drawing.Size(71, 20);
            this.zScaleUpDown.TabIndex = 5;
            this.zScaleUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // yScaleLabel
            // 
            this.yScaleLabel.AutoSize = true;
            this.yScaleLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.yScaleLabel.Location = new System.Drawing.Point(2, 24);
            this.yScaleLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.yScaleLabel.Name = "yScaleLabel";
            this.yScaleLabel.Size = new System.Drawing.Size(17, 24);
            this.yScaleLabel.TabIndex = 1;
            this.yScaleLabel.Text = "Y:";
            this.yScaleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // yScaleUpDown
            // 
            this.yScaleUpDown.DecimalPlaces = 2;
            this.yScaleUpDown.Dock = System.Windows.Forms.DockStyle.Top;
            this.yScaleUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.yScaleUpDown.Location = new System.Drawing.Point(23, 26);
            this.yScaleUpDown.Margin = new System.Windows.Forms.Padding(2);
            this.yScaleUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.yScaleUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.yScaleUpDown.Name = "yScaleUpDown";
            this.yScaleUpDown.Size = new System.Drawing.Size(71, 20);
            this.yScaleUpDown.TabIndex = 4;
            this.yScaleUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // zScaleLabel
            // 
            this.zScaleLabel.AutoSize = true;
            this.zScaleLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.zScaleLabel.Location = new System.Drawing.Point(2, 48);
            this.zScaleLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.zScaleLabel.Name = "zScaleLabel";
            this.zScaleLabel.Size = new System.Drawing.Size(17, 24);
            this.zScaleLabel.TabIndex = 2;
            this.zScaleLabel.Text = "Z:";
            this.zScaleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // xScaleUpDown
            // 
            this.xScaleUpDown.DecimalPlaces = 2;
            this.xScaleUpDown.Dock = System.Windows.Forms.DockStyle.Top;
            this.xScaleUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.xScaleUpDown.Location = new System.Drawing.Point(23, 2);
            this.xScaleUpDown.Margin = new System.Windows.Forms.Padding(2);
            this.xScaleUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.xScaleUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.xScaleUpDown.Name = "xScaleUpDown";
            this.xScaleUpDown.Size = new System.Drawing.Size(71, 20);
            this.xScaleUpDown.TabIndex = 3;
            this.xScaleUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // BoxColliderPropertiesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.scaleGroupBox);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(100, 0);
            this.Name = "BoxColliderPropertiesForm";
            this.Size = new System.Drawing.Size(100, 89);
            this.scaleGroupBox.ResumeLayout(false);
            this.scaleGroupBox.PerformLayout();
            this.scaleTableLayout.ResumeLayout(false);
            this.scaleTableLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zScaleUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yScaleUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xScaleUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox scaleGroupBox;
        private System.Windows.Forms.Label xScaleLabel;
        private System.Windows.Forms.Label yScaleLabel;
        private System.Windows.Forms.NumericUpDown zScaleUpDown;
        private System.Windows.Forms.NumericUpDown yScaleUpDown;
        private System.Windows.Forms.NumericUpDown xScaleUpDown;
        private System.Windows.Forms.Label zScaleLabel;
        private System.Windows.Forms.TableLayoutPanel scaleTableLayout;
    }
}
