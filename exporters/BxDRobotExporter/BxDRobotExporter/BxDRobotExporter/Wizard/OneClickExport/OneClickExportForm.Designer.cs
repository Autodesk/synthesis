namespace BxDRobotExporter.Wizard
{
    partial class OneClickExportForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OneClickExportForm));
            this.WheelCountLabel = new System.Windows.Forms.Label();
            this.WheelCountUpDown = new System.Windows.Forms.NumericUpDown();
            this.DriveTrainLabel = new System.Windows.Forms.Label();
            this.DriveTrainComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.OneClickInfoLabel = new System.Windows.Forms.Label();
            this.ExportParametersGroupBox = new System.Windows.Forms.GroupBox();
            this.MergeNodesCheckBox = new System.Windows.Forms.CheckBox();
            this.FieldSelectComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.LaunchSynthesisCheckBox = new System.Windows.Forms.CheckBox();
            this.ExportButton = new System.Windows.Forms.Button();
            this.CancelExportButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.WheelCountUpDown)).BeginInit();
            this.ExportParametersGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // WheelCountLabel
            // 
            this.WheelCountLabel.AutoSize = true;
            this.WheelCountLabel.Location = new System.Drawing.Point(6, 20);
            this.WheelCountLabel.Name = "WheelCountLabel";
            this.WheelCountLabel.Size = new System.Drawing.Size(72, 13);
            this.WheelCountLabel.TabIndex = 0;
            this.WheelCountLabel.Text = "Wheel Count:";
            // 
            // WheelCountUpDown
            // 
            this.WheelCountUpDown.Location = new System.Drawing.Point(84, 17);
            this.WheelCountUpDown.Name = "WheelCountUpDown";
            this.WheelCountUpDown.Size = new System.Drawing.Size(35, 20);
            this.WheelCountUpDown.TabIndex = 1;
            // 
            // DriveTrainLabel
            // 
            this.DriveTrainLabel.AutoSize = true;
            this.DriveTrainLabel.Location = new System.Drawing.Point(125, 19);
            this.DriveTrainLabel.Name = "DriveTrainLabel";
            this.DriveTrainLabel.Size = new System.Drawing.Size(62, 13);
            this.DriveTrainLabel.TabIndex = 2;
            this.DriveTrainLabel.Text = "Drive Train:";
            // 
            // DriveTrainComboBox
            // 
            this.DriveTrainComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DriveTrainComboBox.FormattingEnabled = true;
            this.DriveTrainComboBox.Items.AddRange(new object[] {
            "Western",
            "Mecanum"});
            this.DriveTrainComboBox.Location = new System.Drawing.Point(193, 16);
            this.DriveTrainComboBox.Name = "DriveTrainComboBox";
            this.DriveTrainComboBox.Size = new System.Drawing.Size(96, 21);
            this.DriveTrainComboBox.TabIndex = 3;
            this.DriveTrainComboBox.SelectedIndexChanged += new System.EventHandler(this.DriveTrainComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(197, 20);
            this.label1.TabIndex = 4;
            this.label1.Text = "One Click Export (Beta)";
            // 
            // OneClickInfoLabel
            // 
            this.OneClickInfoLabel.Location = new System.Drawing.Point(17, 37);
            this.OneClickInfoLabel.Name = "OneClickInfoLabel";
            this.OneClickInfoLabel.Size = new System.Drawing.Size(425, 53);
            this.OneClickInfoLabel.TabIndex = 5;
            this.OneClickInfoLabel.Text = resources.GetString("OneClickInfoLabel.Text");
            // 
            // ExportParametersGroupBox
            // 
            this.ExportParametersGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ExportParametersGroupBox.Controls.Add(this.MergeNodesCheckBox);
            this.ExportParametersGroupBox.Controls.Add(this.FieldSelectComboBox);
            this.ExportParametersGroupBox.Controls.Add(this.label2);
            this.ExportParametersGroupBox.Controls.Add(this.LaunchSynthesisCheckBox);
            this.ExportParametersGroupBox.Controls.Add(this.DriveTrainComboBox);
            this.ExportParametersGroupBox.Controls.Add(this.DriveTrainLabel);
            this.ExportParametersGroupBox.Controls.Add(this.WheelCountUpDown);
            this.ExportParametersGroupBox.Controls.Add(this.WheelCountLabel);
            this.ExportParametersGroupBox.Location = new System.Drawing.Point(12, 93);
            this.ExportParametersGroupBox.Name = "ExportParametersGroupBox";
            this.ExportParametersGroupBox.Size = new System.Drawing.Size(430, 71);
            this.ExportParametersGroupBox.TabIndex = 6;
            this.ExportParametersGroupBox.TabStop = false;
            this.ExportParametersGroupBox.Text = "Export Settings";
            // 
            // MergeNodesCheckBox
            // 
            this.MergeNodesCheckBox.AutoSize = true;
            this.MergeNodesCheckBox.Location = new System.Drawing.Point(295, 19);
            this.MergeNodesCheckBox.Name = "MergeNodesCheckBox";
            this.MergeNodesCheckBox.Size = new System.Drawing.Size(130, 17);
            this.MergeNodesCheckBox.TabIndex = 7;
            this.MergeNodesCheckBox.Text = "Merge Unused Nodes";
            this.MergeNodesCheckBox.UseVisualStyleBackColor = true;
            // 
            // FieldSelectComboBox
            // 
            this.FieldSelectComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FieldSelectComboBox.Enabled = false;
            this.FieldSelectComboBox.FormattingEnabled = true;
            this.FieldSelectComboBox.Location = new System.Drawing.Point(196, 46);
            this.FieldSelectComboBox.Name = "FieldSelectComboBox";
            this.FieldSelectComboBox.Size = new System.Drawing.Size(228, 21);
            this.FieldSelectComboBox.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(125, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Select Field:";
            // 
            // LaunchSynthesisCheckBox
            // 
            this.LaunchSynthesisCheckBox.AutoSize = true;
            this.LaunchSynthesisCheckBox.Location = new System.Drawing.Point(9, 48);
            this.LaunchSynthesisCheckBox.Name = "LaunchSynthesisCheckBox";
            this.LaunchSynthesisCheckBox.Size = new System.Drawing.Size(110, 17);
            this.LaunchSynthesisCheckBox.TabIndex = 4;
            this.LaunchSynthesisCheckBox.Text = "Launch Synthesis";
            this.LaunchSynthesisCheckBox.UseVisualStyleBackColor = true;
            this.LaunchSynthesisCheckBox.CheckedChanged += new System.EventHandler(this.LaunchSynthesisCheckBox_CheckedChanged);
            // 
            // ExportButton
            // 
            this.ExportButton.Location = new System.Drawing.Point(230, 170);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(212, 23);
            this.ExportButton.TabIndex = 7;
            this.ExportButton.Text = "Export";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // CancelExportButton
            // 
            this.CancelExportButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelExportButton.Location = new System.Drawing.Point(12, 170);
            this.CancelExportButton.Name = "CancelExportButton";
            this.CancelExportButton.Size = new System.Drawing.Size(212, 23);
            this.CancelExportButton.TabIndex = 8;
            this.CancelExportButton.Text = "Cancel";
            this.CancelExportButton.UseVisualStyleBackColor = true;
            this.CancelExportButton.Click += new System.EventHandler(this.CancelExportButton_Click);
            // 
            // OneClickExportForm
            // 
            this.AcceptButton = this.CancelExportButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelExportButton;
            this.ClientSize = new System.Drawing.Size(454, 204);
            this.Controls.Add(this.CancelExportButton);
            this.Controls.Add(this.ExportButton);
            this.Controls.Add(this.ExportParametersGroupBox);
            this.Controls.Add(this.OneClickInfoLabel);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(470, 243);
            this.MinimumSize = new System.Drawing.Size(470, 243);
            this.Name = "OneClickExportForm";
            this.Text = "One Click Export (Beta)";
            this.Load += new System.EventHandler(this.OneClickExportForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.WheelCountUpDown)).EndInit();
            this.ExportParametersGroupBox.ResumeLayout(false);
            this.ExportParametersGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label WheelCountLabel;
        private System.Windows.Forms.NumericUpDown WheelCountUpDown;
        private System.Windows.Forms.Label DriveTrainLabel;
        private System.Windows.Forms.ComboBox DriveTrainComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label OneClickInfoLabel;
        private System.Windows.Forms.GroupBox ExportParametersGroupBox;
        private System.Windows.Forms.ComboBox FieldSelectComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox LaunchSynthesisCheckBox;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.Button CancelExportButton;
        private System.Windows.Forms.CheckBox MergeNodesCheckBox;
    }
}