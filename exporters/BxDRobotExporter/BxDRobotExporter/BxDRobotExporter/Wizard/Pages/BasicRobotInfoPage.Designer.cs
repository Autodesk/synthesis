namespace BxDRobotExporter.Wizard
{
    partial class BasicRobotInfoPage
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
            this.components = new System.ComponentModel.Container();
            this.BasicInfoTitleLabel = new System.Windows.Forms.Label();
            this.GeneralInfoGroupBox = new System.Windows.Forms.GroupBox();
            this.RoboticsLeagueLabel = new System.Windows.Forms.Label();
            this.FTCRadioButton = new System.Windows.Forms.RadioButton();
            this.FRCRadioButton = new System.Windows.Forms.RadioButton();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.AnalyticsToggleCheckBox = new System.Windows.Forms.CheckBox();
            this.TeamNumberTextBox = new System.Windows.Forms.MaskedTextBox();
            this.TeamNumberLabel = new System.Windows.Forms.Label();
            this.RobotNameTextBox = new System.Windows.Forms.TextBox();
            this.RobotNameLabel = new System.Windows.Forms.Label();
            this.RobotNameToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.DriveTrainLabel = new System.Windows.Forms.Label();
            this.WheelCountLabel = new System.Windows.Forms.Label();
            this.RobotInfoGroupBox = new System.Windows.Forms.GroupBox();
            this.WheelCountUpDown = new System.Windows.Forms.NumericUpDown();
            this.DriveTrainDropdown = new System.Windows.Forms.ComboBox();
            this.RobotMassGroupBox = new System.Windows.Forms.GroupBox();
            this.TotalMassLabel = new System.Windows.Forms.Label();
            this.MassPropertyInfoLabel = new System.Windows.Forms.Label();
            this.MassPropertyTitleLabel = new System.Windows.Forms.Label();
            this.MassPanel = new System.Windows.Forms.Panel();
            this.MetricCheckBox = new System.Windows.Forms.CheckBox();
            this.MassModeLabel = new System.Windows.Forms.Label();
            this.MassModeDropdown = new System.Windows.Forms.ComboBox();
            this.GeneralInfoGroupBox.SuspendLayout();
            this.RobotInfoGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WheelCountUpDown)).BeginInit();
            this.RobotMassGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BasicInfoTitleLabel
            // 
            this.BasicInfoTitleLabel.AutoSize = true;
            this.BasicInfoTitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BasicInfoTitleLabel.Location = new System.Drawing.Point(-5, 0);
            this.BasicInfoTitleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.BasicInfoTitleLabel.Name = "BasicInfoTitleLabel";
            this.BasicInfoTitleLabel.Size = new System.Drawing.Size(345, 25);
            this.BasicInfoTitleLabel.TabIndex = 1;
            this.BasicInfoTitleLabel.Text = "Step One: Basic Robot Information";
            // 
            // GeneralInfoGroupBox
            // 
            this.GeneralInfoGroupBox.BackColor = System.Drawing.SystemColors.Control;
            this.GeneralInfoGroupBox.Controls.Add(this.RoboticsLeagueLabel);
            this.GeneralInfoGroupBox.Controls.Add(this.FTCRadioButton);
            this.GeneralInfoGroupBox.Controls.Add(this.FRCRadioButton);
            this.GeneralInfoGroupBox.Controls.Add(this.linkLabel1);
            this.GeneralInfoGroupBox.Controls.Add(this.AnalyticsToggleCheckBox);
            this.GeneralInfoGroupBox.Controls.Add(this.TeamNumberTextBox);
            this.GeneralInfoGroupBox.Controls.Add(this.TeamNumberLabel);
            this.GeneralInfoGroupBox.Controls.Add(this.RobotNameTextBox);
            this.GeneralInfoGroupBox.Controls.Add(this.RobotNameLabel);
            this.GeneralInfoGroupBox.Location = new System.Drawing.Point(0, 91);
            this.GeneralInfoGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.GeneralInfoGroupBox.Name = "GeneralInfoGroupBox";
            this.GeneralInfoGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.GeneralInfoGroupBox.Size = new System.Drawing.Size(613, 80);
            this.GeneralInfoGroupBox.TabIndex = 2;
            this.GeneralInfoGroupBox.TabStop = false;
            this.GeneralInfoGroupBox.Text = "General Information";
            // 
            // RoboticsLeagueLabel
            // 
            this.RoboticsLeagueLabel.AutoSize = true;
            this.RoboticsLeagueLabel.Location = new System.Drawing.Point(400, 54);
            this.RoboticsLeagueLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.RoboticsLeagueLabel.Name = "RoboticsLeagueLabel";
            this.RoboticsLeagueLabel.Size = new System.Drawing.Size(60, 17);
            this.RoboticsLeagueLabel.TabIndex = 8;
            this.RoboticsLeagueLabel.Text = "League:";
            // 
            // FTCRadioButton
            // 
            this.FTCRadioButton.AutoSize = true;
            this.FTCRadioButton.Location = new System.Drawing.Point(539, 52);
            this.FTCRadioButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.FTCRadioButton.Name = "FTCRadioButton";
            this.FTCRadioButton.Size = new System.Drawing.Size(55, 21);
            this.FTCRadioButton.TabIndex = 7;
            this.FTCRadioButton.TabStop = true;
            this.FTCRadioButton.Text = "FTC";
            this.FTCRadioButton.UseVisualStyleBackColor = true;
            this.FTCRadioButton.CheckedChanged += new System.EventHandler(this.FTCRadioButton_CheckedChanged);
            // 
            // FRCRadioButton
            // 
            this.FRCRadioButton.AutoSize = true;
            this.FRCRadioButton.Location = new System.Drawing.Point(469, 52);
            this.FRCRadioButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.FRCRadioButton.Name = "FRCRadioButton";
            this.FRCRadioButton.Size = new System.Drawing.Size(56, 21);
            this.FRCRadioButton.TabIndex = 6;
            this.FRCRadioButton.TabStop = true;
            this.FRCRadioButton.Text = "FRC";
            this.FRCRadioButton.UseVisualStyleBackColor = true;
            this.FRCRadioButton.CheckedChanged += new System.EventHandler(this.FRCRadioButton_CheckedChanged);
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.BackColor = System.Drawing.Color.Transparent;
            this.linkLabel1.Location = new System.Drawing.Point(229, 54);
            this.linkLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(81, 17);
            this.linkLabel1.TabIndex = 5;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Learn More";
            // 
            // AnalyticsToggleCheckBox
            // 
            this.AnalyticsToggleCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AnalyticsToggleCheckBox.AutoSize = true;
            this.AnalyticsToggleCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.AnalyticsToggleCheckBox.Location = new System.Drawing.Point(9, 53);
            this.AnalyticsToggleCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.AnalyticsToggleCheckBox.Name = "AnalyticsToggleCheckBox";
            this.AnalyticsToggleCheckBox.Size = new System.Drawing.Size(224, 21);
            this.AnalyticsToggleCheckBox.TabIndex = 4;
            this.AnalyticsToggleCheckBox.Text = "Send Anonymous Usage Data.";
            this.AnalyticsToggleCheckBox.UseVisualStyleBackColor = false;
            this.AnalyticsToggleCheckBox.CheckedChanged += new System.EventHandler(this.AnalyticsCheckbox_CheckedChanged);
            // 
            // TeamNumberTextBox
            // 
            this.TeamNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TeamNumberTextBox.Location = new System.Drawing.Point(519, 21);
            this.TeamNumberTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TeamNumberTextBox.Mask = "0000";
            this.TeamNumberTextBox.Name = "TeamNumberTextBox";
            this.TeamNumberTextBox.PromptChar = ' ';
            this.TeamNumberTextBox.Size = new System.Drawing.Size(79, 22);
            this.TeamNumberTextBox.TabIndex = 3;
            // 
            // TeamNumberLabel
            // 
            this.TeamNumberLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TeamNumberLabel.AutoSize = true;
            this.TeamNumberLabel.Location = new System.Drawing.Point(411, 25);
            this.TeamNumberLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.TeamNumberLabel.Name = "TeamNumberLabel";
            this.TeamNumberLabel.Size = new System.Drawing.Size(102, 17);
            this.TeamNumberLabel.TabIndex = 2;
            this.TeamNumberLabel.Text = "Team Number:";
            this.RobotNameToolTip.SetToolTip(this.TeamNumberLabel, "Your FRC Team number (optional)");
            // 
            // RobotNameTextBox
            // 
            this.RobotNameTextBox.Location = new System.Drawing.Point(101, 21);
            this.RobotNameTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.RobotNameTextBox.Name = "RobotNameTextBox";
            this.RobotNameTextBox.ReadOnly = true;
            this.RobotNameTextBox.Size = new System.Drawing.Size(297, 22);
            this.RobotNameTextBox.TabIndex = 1;
            // 
            // RobotNameLabel
            // 
            this.RobotNameLabel.AutoSize = true;
            this.RobotNameLabel.Location = new System.Drawing.Point(5, 25);
            this.RobotNameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.RobotNameLabel.Name = "RobotNameLabel";
            this.RobotNameLabel.Size = new System.Drawing.Size(91, 17);
            this.RobotNameLabel.TabIndex = 0;
            this.RobotNameLabel.Text = "Robot Name:";
            this.RobotNameToolTip.SetToolTip(this.RobotNameLabel, "The name of your robot that will show up in the simulator.");
            // 
            // DriveTrainLabel
            // 
            this.DriveTrainLabel.AutoSize = true;
            this.DriveTrainLabel.Location = new System.Drawing.Point(9, 25);
            this.DriveTrainLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.DriveTrainLabel.Name = "DriveTrainLabel";
            this.DriveTrainLabel.Size = new System.Drawing.Size(82, 17);
            this.DriveTrainLabel.TabIndex = 0;
            this.DriveTrainLabel.Text = "Drive Train:";
            this.RobotNameToolTip.SetToolTip(this.DriveTrainLabel, "Your robot\'s drive train. Select Other / Custom if you don\'t know.");
            // 
            // WheelCountLabel
            // 
            this.WheelCountLabel.AutoSize = true;
            this.WheelCountLabel.Location = new System.Drawing.Point(269, 25);
            this.WheelCountLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.WheelCountLabel.Name = "WheelCountLabel";
            this.WheelCountLabel.Size = new System.Drawing.Size(93, 17);
            this.WheelCountLabel.TabIndex = 3;
            this.WheelCountLabel.Text = "Wheel Count:";
            this.RobotNameToolTip.SetToolTip(this.WheelCountLabel, "The number of wheels on your robot\'s drive base.");
            // 
            // RobotInfoGroupBox
            // 
            this.RobotInfoGroupBox.BackColor = System.Drawing.SystemColors.Control;
            this.RobotInfoGroupBox.Controls.Add(this.WheelCountLabel);
            this.RobotInfoGroupBox.Controls.Add(this.WheelCountUpDown);
            this.RobotInfoGroupBox.Controls.Add(this.DriveTrainDropdown);
            this.RobotInfoGroupBox.Controls.Add(this.DriveTrainLabel);
            this.RobotInfoGroupBox.Location = new System.Drawing.Point(0, 180);
            this.RobotInfoGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.RobotInfoGroupBox.Name = "RobotInfoGroupBox";
            this.RobotInfoGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.RobotInfoGroupBox.Size = new System.Drawing.Size(613, 62);
            this.RobotInfoGroupBox.TabIndex = 3;
            this.RobotInfoGroupBox.TabStop = false;
            this.RobotInfoGroupBox.Text = "Drive Information";
            // 
            // WheelCountUpDown
            // 
            this.WheelCountUpDown.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.WheelCountUpDown.Location = new System.Drawing.Point(368, 22);
            this.WheelCountUpDown.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.WheelCountUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.WheelCountUpDown.Name = "WheelCountUpDown";
            this.WheelCountUpDown.Size = new System.Drawing.Size(56, 22);
            this.WheelCountUpDown.TabIndex = 2;
            this.WheelCountUpDown.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // DriveTrainDropdown
            // 
            this.DriveTrainDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DriveTrainDropdown.FormattingEnabled = true;
            this.DriveTrainDropdown.Items.AddRange(new object[] {
            "Select Drive Train...",
            "Western",
            "Mecanum",
            "Swerve",
            "H-Drive",
            "Other/Custom"});
            this.DriveTrainDropdown.Location = new System.Drawing.Point(95, 21);
            this.DriveTrainDropdown.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DriveTrainDropdown.Name = "DriveTrainDropdown";
            this.DriveTrainDropdown.Size = new System.Drawing.Size(160, 24);
            this.DriveTrainDropdown.TabIndex = 1;
            this.DriveTrainDropdown.SelectedIndexChanged += new System.EventHandler(this.DriveTrainDropdown_SelectedIndexChanged);
            // 
            // RobotMassGroupBox
            // 
            this.RobotMassGroupBox.BackColor = System.Drawing.SystemColors.Control;
            this.RobotMassGroupBox.Controls.Add(this.TotalMassLabel);
            this.RobotMassGroupBox.Controls.Add(this.MassPropertyInfoLabel);
            this.RobotMassGroupBox.Controls.Add(this.MassPropertyTitleLabel);
            this.RobotMassGroupBox.Controls.Add(this.MassPanel);
            this.RobotMassGroupBox.Controls.Add(this.MetricCheckBox);
            this.RobotMassGroupBox.Controls.Add(this.MassModeLabel);
            this.RobotMassGroupBox.Controls.Add(this.MassModeDropdown);
            this.RobotMassGroupBox.Location = new System.Drawing.Point(0, 250);
            this.RobotMassGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.RobotMassGroupBox.Name = "RobotMassGroupBox";
            this.RobotMassGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.RobotMassGroupBox.Size = new System.Drawing.Size(613, 322);
            this.RobotMassGroupBox.TabIndex = 4;
            this.RobotMassGroupBox.TabStop = false;
            this.RobotMassGroupBox.Text = "Robot Mass";
            // 
            // TotalMassLabel
            // 
            this.TotalMassLabel.AutoSize = true;
            this.TotalMassLabel.Location = new System.Drawing.Point(433, 176);
            this.TotalMassLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.TotalMassLabel.Name = "TotalMassLabel";
            this.TotalMassLabel.Size = new System.Drawing.Size(127, 17);
            this.TotalMassLabel.TabIndex = 7;
            this.TotalMassLabel.Text = "Total Mass: 0.0 lbs";
            // 
            // MassPropertyInfoLabel
            // 
            this.MassPropertyInfoLabel.Location = new System.Drawing.Point(4, 223);
            this.MassPropertyInfoLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.MassPropertyInfoLabel.Name = "MassPropertyInfoLabel";
            this.MassPropertyInfoLabel.Size = new System.Drawing.Size(589, 96);
            this.MassPropertyInfoLabel.TabIndex = 6;
            this.MassPropertyInfoLabel.Text = "N/A";
            // 
            // MassPropertyTitleLabel
            // 
            this.MassPropertyTitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MassPropertyTitleLabel.Location = new System.Drawing.Point(4, 204);
            this.MassPropertyTitleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.MassPropertyTitleLabel.Name = "MassPropertyTitleLabel";
            this.MassPropertyTitleLabel.Size = new System.Drawing.Size(585, 18);
            this.MassPropertyTitleLabel.TabIndex = 5;
            this.MassPropertyTitleLabel.Text = "Selected Mode: ";
            // 
            // MassPanel
            // 
            this.MassPanel.AutoScroll = true;
            this.MassPanel.BackColor = System.Drawing.Color.White;
            this.MassPanel.Location = new System.Drawing.Point(9, 78);
            this.MassPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MassPanel.Name = "MassPanel";
            this.MassPanel.Size = new System.Drawing.Size(589, 95);
            this.MassPanel.TabIndex = 4;
            // 
            // MetricCheckBox
            // 
            this.MetricCheckBox.AutoSize = true;
            this.MetricCheckBox.Location = new System.Drawing.Point(452, 26);
            this.MetricCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MetricCheckBox.Name = "MetricCheckBox";
            this.MetricCheckBox.Size = new System.Drawing.Size(104, 21);
            this.MetricCheckBox.TabIndex = 3;
            this.MetricCheckBox.Text = "Metric Units";
            this.RobotNameToolTip.SetToolTip(this.MetricCheckBox, "Toggles the usage of the superior system of measurement,");
            this.MetricCheckBox.UseVisualStyleBackColor = true;
            this.MetricCheckBox.CheckedChanged += new System.EventHandler(this.MetricCheckBox_CheckedChanged);
            // 
            // MassModeLabel
            // 
            this.MassModeLabel.AutoSize = true;
            this.MassModeLabel.Location = new System.Drawing.Point(8, 27);
            this.MassModeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.MassModeLabel.Name = "MassModeLabel";
            this.MassModeLabel.Size = new System.Drawing.Size(157, 17);
            this.MassModeLabel.TabIndex = 1;
            this.MassModeLabel.Text = "Mass Calculation Mode:";
            this.RobotNameToolTip.SetToolTip(this.MassModeLabel, "The method by which the mass of each robot is calculated.");
            // 
            // MassModeDropdown
            // 
            this.MassModeDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.MassModeDropdown.FormattingEnabled = true;
            this.MassModeDropdown.Items.AddRange(new object[] {
            "Simple User Defined (Recommended)",
            "Complex User Defined",
            "Materials Based"});
            this.MassModeDropdown.Location = new System.Drawing.Point(176, 23);
            this.MassModeDropdown.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MassModeDropdown.Name = "MassModeDropdown";
            this.MassModeDropdown.Size = new System.Drawing.Size(267, 24);
            this.MassModeDropdown.TabIndex = 0;
            this.MassModeDropdown.SelectedIndexChanged += new System.EventHandler(this.MassModeDropdown_SelectedIndexChanged);
            // 
            // BasicRobotInfoPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.RobotMassGroupBox);
            this.Controls.Add(this.RobotInfoGroupBox);
            this.Controls.Add(this.GeneralInfoGroupBox);
            this.Controls.Add(this.BasicInfoTitleLabel);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "BasicRobotInfoPage";
            this.Size = new System.Drawing.Size(613, 804);
            this.GeneralInfoGroupBox.ResumeLayout(false);
            this.GeneralInfoGroupBox.PerformLayout();
            this.RobotInfoGroupBox.ResumeLayout(false);
            this.RobotInfoGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WheelCountUpDown)).EndInit();
            this.RobotMassGroupBox.ResumeLayout(false);
            this.RobotMassGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label BasicInfoTitleLabel;
        private System.Windows.Forms.GroupBox GeneralInfoGroupBox;
        private System.Windows.Forms.Label RobotNameLabel;
        private System.Windows.Forms.Label TeamNumberLabel;
        private System.Windows.Forms.TextBox RobotNameTextBox;
        private System.Windows.Forms.MaskedTextBox TeamNumberTextBox;
        private System.Windows.Forms.ToolTip RobotNameToolTip;
        private System.Windows.Forms.GroupBox RobotInfoGroupBox;
        private System.Windows.Forms.ComboBox DriveTrainDropdown;
        private System.Windows.Forms.Label DriveTrainLabel;
        private System.Windows.Forms.Label WheelCountLabel;
        private System.Windows.Forms.NumericUpDown WheelCountUpDown;
        private System.Windows.Forms.Label RoboticsLeagueLabel;
        private System.Windows.Forms.RadioButton FTCRadioButton;
        private System.Windows.Forms.RadioButton FRCRadioButton;
        private System.Windows.Forms.GroupBox RobotMassGroupBox;
        private System.Windows.Forms.Label MassModeLabel;
        private System.Windows.Forms.ComboBox MassModeDropdown;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.CheckBox AnalyticsToggleCheckBox;
        private System.Windows.Forms.CheckBox MetricCheckBox;
        private System.Windows.Forms.Panel MassPanel;
        private System.Windows.Forms.Label MassPropertyTitleLabel;
        private System.Windows.Forms.Label MassPropertyInfoLabel;
        private System.Windows.Forms.Label TotalMassLabel;
    }
}
