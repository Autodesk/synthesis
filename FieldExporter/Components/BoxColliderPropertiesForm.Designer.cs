﻿namespace FieldExporter.Components
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
            this.zScaleUpDown = new System.Windows.Forms.NumericUpDown();
            this.zScaleLabel = new System.Windows.Forms.Label();
            this.yScaleUpDown = new System.Windows.Forms.NumericUpDown();
            this.yScaleLabel = new System.Windows.Forms.Label();
            this.xScaleUpDown = new System.Windows.Forms.NumericUpDown();
            this.xScaleLabel = new System.Windows.Forms.Label();
            this.scaleGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zScaleUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yScaleUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xScaleUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // scaleGroupBox
            // 
            this.scaleGroupBox.Controls.Add(this.zScaleUpDown);
            this.scaleGroupBox.Controls.Add(this.zScaleLabel);
            this.scaleGroupBox.Controls.Add(this.yScaleUpDown);
            this.scaleGroupBox.Controls.Add(this.yScaleLabel);
            this.scaleGroupBox.Controls.Add(this.xScaleUpDown);
            this.scaleGroupBox.Controls.Add(this.xScaleLabel);
            this.scaleGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scaleGroupBox.Location = new System.Drawing.Point(0, 0);
            this.scaleGroupBox.Name = "scaleGroupBox";
            this.scaleGroupBox.Size = new System.Drawing.Size(300, 102);
            this.scaleGroupBox.TabIndex = 0;
            this.scaleGroupBox.TabStop = false;
            this.scaleGroupBox.Text = "Scale";
            // 
            // zScaleUpDown
            // 
            this.zScaleUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.zScaleUpDown.DecimalPlaces = 2;
            this.zScaleUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.zScaleUpDown.Location = new System.Drawing.Point(33, 74);
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
            this.zScaleUpDown.Size = new System.Drawing.Size(261, 22);
            this.zScaleUpDown.TabIndex = 5;
            this.zScaleUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // zScaleLabel
            // 
            this.zScaleLabel.AutoSize = true;
            this.zScaleLabel.Location = new System.Drawing.Point(6, 76);
            this.zScaleLabel.Name = "zScaleLabel";
            this.zScaleLabel.Size = new System.Drawing.Size(21, 17);
            this.zScaleLabel.TabIndex = 4;
            this.zScaleLabel.Text = "Z:";
            // 
            // yScaleUpDown
            // 
            this.yScaleUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.yScaleUpDown.DecimalPlaces = 2;
            this.yScaleUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.yScaleUpDown.Location = new System.Drawing.Point(33, 46);
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
            this.yScaleUpDown.Size = new System.Drawing.Size(261, 22);
            this.yScaleUpDown.TabIndex = 3;
            this.yScaleUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // yScaleLabel
            // 
            this.yScaleLabel.AutoSize = true;
            this.yScaleLabel.Location = new System.Drawing.Point(6, 48);
            this.yScaleLabel.Name = "yScaleLabel";
            this.yScaleLabel.Size = new System.Drawing.Size(21, 17);
            this.yScaleLabel.TabIndex = 2;
            this.yScaleLabel.Text = "Y:";
            // 
            // xScaleUpDown
            // 
            this.xScaleUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.xScaleUpDown.DecimalPlaces = 2;
            this.xScaleUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.xScaleUpDown.Location = new System.Drawing.Point(33, 18);
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
            this.xScaleUpDown.Size = new System.Drawing.Size(261, 22);
            this.xScaleUpDown.TabIndex = 1;
            this.xScaleUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // xScaleLabel
            // 
            this.xScaleLabel.AutoSize = true;
            this.xScaleLabel.Location = new System.Drawing.Point(6, 23);
            this.xScaleLabel.Name = "xScaleLabel";
            this.xScaleLabel.Size = new System.Drawing.Size(21, 17);
            this.xScaleLabel.TabIndex = 0;
            this.xScaleLabel.Text = "X:";
            // 
            // BoxColliderPropertiesForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.scaleGroupBox);
            this.Name = "BoxColliderPropertiesForm";
            this.Size = new System.Drawing.Size(300, 102);
            this.scaleGroupBox.ResumeLayout(false);
            this.scaleGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zScaleUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yScaleUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xScaleUpDown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox scaleGroupBox;
        private System.Windows.Forms.NumericUpDown xScaleUpDown;
        private System.Windows.Forms.Label xScaleLabel;
        private System.Windows.Forms.NumericUpDown zScaleUpDown;
        private System.Windows.Forms.Label zScaleLabel;
        private System.Windows.Forms.NumericUpDown yScaleUpDown;
        private System.Windows.Forms.Label yScaleLabel;

    }
}
