namespace BxDRobotExporter.Wizard
{
    partial class WheelSetupPanel
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
            this.MainGroupBox = new System.Windows.Forms.GroupBox();
            this.WheelSideLabel = new System.Windows.Forms.Label();
            this.RightRadioButton = new System.Windows.Forms.RadioButton();
            this.LeftRadioButton = new System.Windows.Forms.RadioButton();
            this.FrictionComboBox = new System.Windows.Forms.ComboBox();
            this.FrictionLabel = new System.Windows.Forms.Label();
            this.WheelTypeLabel = new System.Windows.Forms.Label();
            this.WheelTypeComboBox = new System.Windows.Forms.ComboBox();
            this.ViewInventorButton = new System.Windows.Forms.Button();
            this.MainGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainGroupBox
            // 
            this.MainGroupBox.Controls.Add(this.WheelSideLabel);
            this.MainGroupBox.Controls.Add(this.RightRadioButton);
            this.MainGroupBox.Controls.Add(this.LeftRadioButton);
            this.MainGroupBox.Controls.Add(this.FrictionComboBox);
            this.MainGroupBox.Controls.Add(this.FrictionLabel);
            this.MainGroupBox.Controls.Add(this.WheelTypeLabel);
            this.MainGroupBox.Controls.Add(this.WheelTypeComboBox);
            this.MainGroupBox.Location = new System.Drawing.Point(0, 0);
            this.MainGroupBox.Name = "MainGroupBox";
            this.MainGroupBox.Size = new System.Drawing.Size(220, 100);
            this.MainGroupBox.TabIndex = 0;
            this.MainGroupBox.TabStop = false;
            this.MainGroupBox.Text = "node__.bxda";
            // 
            // WheelSideLabel
            // 
            this.WheelSideLabel.AutoSize = true;
            this.WheelSideLabel.Location = new System.Drawing.Point(6, 76);
            this.WheelSideLabel.Name = "WheelSideLabel";
            this.WheelSideLabel.Size = new System.Drawing.Size(65, 13);
            this.WheelSideLabel.TabIndex = 6;
            this.WheelSideLabel.Text = "Wheel Side:";
            // 
            // RightRadioButton
            // 
            this.RightRadioButton.AutoSize = true;
            this.RightRadioButton.Location = new System.Drawing.Point(130, 74);
            this.RightRadioButton.Name = "RightRadioButton";
            this.RightRadioButton.Size = new System.Drawing.Size(50, 17);
            this.RightRadioButton.TabIndex = 5;
            this.RightRadioButton.TabStop = true;
            this.RightRadioButton.Text = "Right";
            this.RightRadioButton.UseVisualStyleBackColor = true;
            this.RightRadioButton.CheckedChanged += new System.EventHandler(this.RightRadioButton_CheckedChanged);
            // 
            // LeftRadioButton
            // 
            this.LeftRadioButton.AutoSize = true;
            this.LeftRadioButton.Location = new System.Drawing.Point(81, 74);
            this.LeftRadioButton.Name = "LeftRadioButton";
            this.LeftRadioButton.Size = new System.Drawing.Size(43, 17);
            this.LeftRadioButton.TabIndex = 4;
            this.LeftRadioButton.TabStop = true;
            this.LeftRadioButton.Text = "Left";
            this.LeftRadioButton.UseVisualStyleBackColor = true;
            this.LeftRadioButton.CheckedChanged += new System.EventHandler(this.LeftRadioButton_CheckedChanged);
            // 
            // FrictionComboBox
            // 
            this.FrictionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FrictionComboBox.FormattingEnabled = true;
            this.FrictionComboBox.Items.AddRange(new object[] {
            "Low",
            "Medium",
            "High"});
            this.FrictionComboBox.Location = new System.Drawing.Point(81, 45);
            this.FrictionComboBox.Name = "FrictionComboBox";
            this.FrictionComboBox.Size = new System.Drawing.Size(116, 21);
            this.FrictionComboBox.TabIndex = 3;
            // 
            // FrictionLabel
            // 
            this.FrictionLabel.AutoSize = true;
            this.FrictionLabel.Location = new System.Drawing.Point(4, 48);
            this.FrictionLabel.Name = "FrictionLabel";
            this.FrictionLabel.Size = new System.Drawing.Size(78, 13);
            this.FrictionLabel.TabIndex = 2;
            this.FrictionLabel.Text = "Wheel Friction:";
            // 
            // WheelTypeLabel
            // 
            this.WheelTypeLabel.AutoSize = true;
            this.WheelTypeLabel.Location = new System.Drawing.Point(4, 20);
            this.WheelTypeLabel.Name = "WheelTypeLabel";
            this.WheelTypeLabel.Size = new System.Drawing.Size(71, 13);
            this.WheelTypeLabel.TabIndex = 1;
            this.WheelTypeLabel.Text = "Wheel Type: ";
            // 
            // WheelTypeComboBox
            // 
            this.WheelTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WheelTypeComboBox.FormattingEnabled = true;
            this.WheelTypeComboBox.Items.AddRange(new object[] {
            "Normal",
            "Omni",
            "Mecanum"});
            this.WheelTypeComboBox.Location = new System.Drawing.Point(81, 17);
            this.WheelTypeComboBox.Name = "WheelTypeComboBox";
            this.WheelTypeComboBox.Size = new System.Drawing.Size(116, 21);
            this.WheelTypeComboBox.TabIndex = 0;
            this.WheelTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.WheelTypeComboBox_SelectedIndexChanged);
            // 
            // ViewInventorButton
            // 
            this.ViewInventorButton.Location = new System.Drawing.Point(3, 106);
            this.ViewInventorButton.Name = "ViewInventorButton";
            this.ViewInventorButton.Size = new System.Drawing.Size(197, 22);
            this.ViewInventorButton.TabIndex = 1;
            this.ViewInventorButton.Text = "View in Inventor";
            this.ViewInventorButton.UseVisualStyleBackColor = true;
            this.ViewInventorButton.Click += new System.EventHandler(this.ViewInventorButton_Click);
            // 
            // WheelSetupPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ViewInventorButton);
            this.Controls.Add(this.MainGroupBox);
            this.Name = "WheelSetupPanel";
            this.Size = new System.Drawing.Size(203, 131);
            this.MainGroupBox.ResumeLayout(false);
            this.MainGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox MainGroupBox;
        private System.Windows.Forms.ComboBox WheelTypeComboBox;
        private System.Windows.Forms.Label WheelTypeLabel;
        private System.Windows.Forms.ComboBox FrictionComboBox;
        private System.Windows.Forms.Label FrictionLabel;
        private System.Windows.Forms.Label WheelSideLabel;
        private System.Windows.Forms.RadioButton RightRadioButton;
        private System.Windows.Forms.RadioButton LeftRadioButton;
        private System.Windows.Forms.Button ViewInventorButton;
    }
}
