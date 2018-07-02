namespace BxDRobotExporter.Wizard
{
    partial class DefineWheelsPage
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
            this.DefineWheelsTitleLabel = new System.Windows.Forms.Label();
            this.DefineWheelsInstruction1 = new System.Windows.Forms.Label();
            this.WheelNodeGroupBox = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.NodeListBox = new System.Windows.Forms.ListBox();
            this.LeftWheelsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.LeftWheelsGroup = new System.Windows.Forms.GroupBox();
            this.WarningLabel = new System.Windows.Forms.Label();
            this.RobotInfoGroupBox = new System.Windows.Forms.GroupBox();
            this.MetricCheckBox = new System.Windows.Forms.CheckBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.DriveTrainDropdown = new System.Windows.Forms.ComboBox();
            this.DriveTrainLabel = new System.Windows.Forms.Label();
            this.RightWheelsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.RightWheelsGroup = new System.Windows.Forms.GroupBox();
            this.WheelNodeGroupBox.SuspendLayout();
            this.LeftWheelsGroup.SuspendLayout();
            this.RobotInfoGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.RightWheelsGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // DefineWheelsTitleLabel
            // 
            this.DefineWheelsTitleLabel.AutoSize = true;
            this.DefineWheelsTitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DefineWheelsTitleLabel.Location = new System.Drawing.Point(-4, 0);
            this.DefineWheelsTitleLabel.Name = "DefineWheelsTitleLabel";
            this.DefineWheelsTitleLabel.Size = new System.Drawing.Size(232, 20);
            this.DefineWheelsTitleLabel.TabIndex = 0;
            this.DefineWheelsTitleLabel.Text = "Step 2: Define Your Wheels";
            // 
            // DefineWheelsInstruction1
            // 
            this.DefineWheelsInstruction1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DefineWheelsInstruction1.Location = new System.Drawing.Point(306, 19);
            this.DefineWheelsInstruction1.Name = "DefineWheelsInstruction1";
            this.DefineWheelsInstruction1.Size = new System.Drawing.Size(142, 70);
            this.DefineWheelsInstruction1.TabIndex = 2;
            this.DefineWheelsInstruction1.Text = "Click on items to the left to see the part they correspond to. Drag wheels from t" +
    "he list into their respective columns below.";
            // 
            // WheelNodeGroupBox
            // 
            this.WheelNodeGroupBox.Controls.Add(this.label4);
            this.WheelNodeGroupBox.Controls.Add(this.NodeListBox);
            this.WheelNodeGroupBox.Controls.Add(this.DefineWheelsInstruction1);
            this.WheelNodeGroupBox.Location = new System.Drawing.Point(0, 89);
            this.WheelNodeGroupBox.Name = "WheelNodeGroupBox";
            this.WheelNodeGroupBox.Size = new System.Drawing.Size(460, 119);
            this.WheelNodeGroupBox.TabIndex = 3;
            this.WheelNodeGroupBox.TabStop = false;
            this.WheelNodeGroupBox.Text = "Select Wheel Joints";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(241, 100);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 13);
            this.label4.TabIndex = 5;
            // 
            // NodeListBox
            // 
            this.NodeListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NodeListBox.FormattingEnabled = true;
            this.NodeListBox.Location = new System.Drawing.Point(9, 18);
            this.NodeListBox.Margin = new System.Windows.Forms.Padding(2);
            this.NodeListBox.Name = "NodeListBox";
            this.NodeListBox.Size = new System.Drawing.Size(293, 69);
            this.NodeListBox.TabIndex = 4;
            this.NodeListBox.SelectedIndexChanged += new System.EventHandler(this.NodeListBox_SelectedIndexChanged);
            this.NodeListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.NodeListBox_MouseDown);
            // 
            // LeftWheelsPanel
            // 
            this.LeftWheelsPanel.Location = new System.Drawing.Point(6, 19);
            this.LeftWheelsPanel.Name = "LeftWheelsPanel";
            this.LeftWheelsPanel.Size = new System.Drawing.Size(223, 414);
            this.LeftWheelsPanel.TabIndex = 4;
            this.LeftWheelsPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.LeftWheelsPanel_DragDrop);
            this.LeftWheelsPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.LeftWheelsPanel_DragEnter);
            // 
            // LeftWheelsGroup
            // 
            this.LeftWheelsGroup.Controls.Add(this.LeftWheelsPanel);
            this.LeftWheelsGroup.Location = new System.Drawing.Point(0, 214);
            this.LeftWheelsGroup.Name = "LeftWheelsGroup";
            this.LeftWheelsGroup.Size = new System.Drawing.Size(235, 439);
            this.LeftWheelsGroup.TabIndex = 1;
            this.LeftWheelsGroup.TabStop = false;
            this.LeftWheelsGroup.Text = "Left Wheels";
            // 
            // WarningLabel
            // 
            this.WarningLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WarningLabel.ForeColor = System.Drawing.Color.Red;
            this.WarningLabel.Location = new System.Drawing.Point(4, 20);
            this.WarningLabel.Name = "WarningLabel";
            this.WarningLabel.Size = new System.Drawing.Size(444, 48);
            this.WarningLabel.TabIndex = 5;
            // 
            // RobotInfoGroupBox
            // 
            this.RobotInfoGroupBox.BackColor = System.Drawing.SystemColors.Control;
            this.RobotInfoGroupBox.Controls.Add(this.MetricCheckBox);
            this.RobotInfoGroupBox.Controls.Add(this.numericUpDown1);
            this.RobotInfoGroupBox.Controls.Add(this.label3);
            this.RobotInfoGroupBox.Controls.Add(this.DriveTrainDropdown);
            this.RobotInfoGroupBox.Controls.Add(this.DriveTrainLabel);
            this.RobotInfoGroupBox.Location = new System.Drawing.Point(0, 32);
            this.RobotInfoGroupBox.Name = "RobotInfoGroupBox";
            this.RobotInfoGroupBox.Size = new System.Drawing.Size(460, 50);
            this.RobotInfoGroupBox.TabIndex = 6;
            this.RobotInfoGroupBox.TabStop = false;
            this.RobotInfoGroupBox.Text = "Drive Information";
            // 
            // MetricCheckBox
            // 
            this.MetricCheckBox.AutoSize = true;
            this.MetricCheckBox.Location = new System.Drawing.Point(380, 17);
            this.MetricCheckBox.Name = "MetricCheckBox";
            this.MetricCheckBox.Size = new System.Drawing.Size(82, 17);
            this.MetricCheckBox.TabIndex = 4;
            this.MetricCheckBox.Text = "Metric Units";
            this.MetricCheckBox.UseVisualStyleBackColor = true;
            this.MetricCheckBox.CheckedChanged += new System.EventHandler(this.MetricCheckBox_CheckedChanged);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(274, 18);
            this.numericUpDown1.Margin = new System.Windows.Forms.Padding(2);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(100, 20);
            this.numericUpDown1.TabIndex = 3;
            this.numericUpDown1.TextChanged += new System.EventHandler(this.NumericUpDown1_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(199, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Robot Weight:";
            // 
            // DriveTrainDropdown
            // 
            this.DriveTrainDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DriveTrainDropdown.FormattingEnabled = true;
            this.DriveTrainDropdown.Items.AddRange(new object[] {
            "Select Drive Train...",
            "Tank",
            "Mecanum",
            "Swerve",
            "H-Drive",
            "Other/Custom"});
            this.DriveTrainDropdown.Location = new System.Drawing.Point(71, 17);
            this.DriveTrainDropdown.Name = "DriveTrainDropdown";
            this.DriveTrainDropdown.Size = new System.Drawing.Size(121, 21);
            this.DriveTrainDropdown.TabIndex = 1;
            this.DriveTrainDropdown.SelectionChangeCommitted += new System.EventHandler(this.DriveTrainDropdown_SelectedIndexChanged);
            // 
            // DriveTrainLabel
            // 
            this.DriveTrainLabel.AutoSize = true;
            this.DriveTrainLabel.Location = new System.Drawing.Point(7, 20);
            this.DriveTrainLabel.Name = "DriveTrainLabel";
            this.DriveTrainLabel.Size = new System.Drawing.Size(62, 13);
            this.DriveTrainLabel.TabIndex = 0;
            this.DriveTrainLabel.Text = "Drive Train:";
            // 
            // RightWheelsPanel
            // 
            this.RightWheelsPanel.Location = new System.Drawing.Point(6, 19);
            this.RightWheelsPanel.Name = "RightWheelsPanel";
            this.RightWheelsPanel.Size = new System.Drawing.Size(209, 414);
            this.RightWheelsPanel.TabIndex = 7;
            this.RightWheelsPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.RightWheelsPanel_DragDrop);
            this.RightWheelsPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.RightWheelsPanel_DragEnter);
            // 
            // RightWheelsGroup
            // 
            this.RightWheelsGroup.Controls.Add(this.RightWheelsPanel);
            this.RightWheelsGroup.Location = new System.Drawing.Point(241, 214);
            this.RightWheelsGroup.Name = "RightWheelsGroup";
            this.RightWheelsGroup.Size = new System.Drawing.Size(221, 439);
            this.RightWheelsGroup.TabIndex = 2;
            this.RightWheelsGroup.TabStop = false;
            this.RightWheelsGroup.Text = "Right Wheels";
            // 
            // DefineWheelsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.RightWheelsGroup);
            this.Controls.Add(this.LeftWheelsGroup);
            this.Controls.Add(this.RobotInfoGroupBox);
            this.Controls.Add(this.WarningLabel);
            this.Controls.Add(this.WheelNodeGroupBox);
            this.Controls.Add(this.DefineWheelsTitleLabel);
            this.Name = "DefineWheelsPage";
            this.Size = new System.Drawing.Size(460, 653);
            this.WheelNodeGroupBox.ResumeLayout(false);
            this.WheelNodeGroupBox.PerformLayout();
            this.LeftWheelsGroup.ResumeLayout(false);
            this.RobotInfoGroupBox.ResumeLayout(false);
            this.RobotInfoGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.RightWheelsGroup.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label DefineWheelsTitleLabel;
        private System.Windows.Forms.Label DefineWheelsInstruction1;
        private System.Windows.Forms.GroupBox WheelNodeGroupBox;
        private System.Windows.Forms.FlowLayoutPanel LeftWheelsPanel;
        private System.Windows.Forms.Label WarningLabel;
        private System.Windows.Forms.GroupBox RobotInfoGroupBox;
        private System.Windows.Forms.ComboBox DriveTrainDropdown;
        private System.Windows.Forms.Label DriveTrainLabel;
        private System.Windows.Forms.FlowLayoutPanel RightWheelsPanel;
        private System.Windows.Forms.ListBox NodeListBox;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox MetricCheckBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox LeftWheelsGroup;
        private System.Windows.Forms.GroupBox RightWheelsGroup;
    }
}
