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
            this.NodeListBox = new System.Windows.Forms.ListBox();
            this.AutoFill = new System.Windows.Forms.Button();
            this.LeftWheelsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.LeftWheelsGroup = new System.Windows.Forms.GroupBox();
            this.RobotInfoGroupBox = new System.Windows.Forms.GroupBox();
            this.WeightUnitSelector = new System.Windows.Forms.ComboBox();
            this.WeightBox = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.DriveTrainDropdown = new System.Windows.Forms.ComboBox();
            this.DriveTrainLabel = new System.Windows.Forms.Label();
            this.RightWheelsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.RightWheelsGroup = new System.Windows.Forms.GroupBox();
            this.WheelNodeGroupBox.SuspendLayout();
            this.LeftWheelsGroup.SuspendLayout();
            this.RobotInfoGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WeightBox)).BeginInit();
            this.RightWheelsGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // DefineWheelsTitleLabel
            // 
            this.DefineWheelsTitleLabel.AutoSize = true;
            this.DefineWheelsTitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DefineWheelsTitleLabel.Location = new System.Drawing.Point(-5, 0);
            this.DefineWheelsTitleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.DefineWheelsTitleLabel.Name = "DefineWheelsTitleLabel";
            this.DefineWheelsTitleLabel.Size = new System.Drawing.Size(280, 25);
            this.DefineWheelsTitleLabel.TabIndex = 0;
            this.DefineWheelsTitleLabel.Text = "Step 2: Define Your Wheels";
            // 
            // DefineWheelsInstruction1
            // 
            this.DefineWheelsInstruction1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DefineWheelsInstruction1.Location = new System.Drawing.Point(408, 10);
            this.DefineWheelsInstruction1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.DefineWheelsInstruction1.Name = "DefineWheelsInstruction1";
            this.DefineWheelsInstruction1.Size = new System.Drawing.Size(189, 86);
            this.DefineWheelsInstruction1.TabIndex = 2;
            this.DefineWheelsInstruction1.Text = "Click on items to the left to see the part they correspond to. Drag wheels from t" +
    "he list into their respective columns below.";
            this.DefineWheelsInstruction1.Click += new System.EventHandler(this.DefineWheelsInstruction1_Click);
            // 
            // WheelNodeGroupBox
            // 
            this.WheelNodeGroupBox.Controls.Add(this.NodeListBox);
            this.WheelNodeGroupBox.Controls.Add(this.DefineWheelsInstruction1);
            this.WheelNodeGroupBox.Controls.Add(this.AutoFill);
            this.WheelNodeGroupBox.Location = new System.Drawing.Point(0, 110);
            this.WheelNodeGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.WheelNodeGroupBox.Name = "WheelNodeGroupBox";
            this.WheelNodeGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.WheelNodeGroupBox.Size = new System.Drawing.Size(613, 120);
            this.WheelNodeGroupBox.TabIndex = 3;
            this.WheelNodeGroupBox.TabStop = false;
            this.WheelNodeGroupBox.Text = "Select Wheel Joints";
            // 
            // NodeListBox
            // 
            this.NodeListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NodeListBox.FormattingEnabled = true;
            this.NodeListBox.ItemHeight = 17;
            this.NodeListBox.Location = new System.Drawing.Point(8, 21);
            this.NodeListBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.NodeListBox.Name = "NodeListBox";
            this.NodeListBox.Size = new System.Drawing.Size(389, 72);
            this.NodeListBox.TabIndex = 4;
            this.NodeListBox.SelectedIndexChanged += new System.EventHandler(this.NodeListBox_SelectedIndexChanged);
            this.NodeListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.NodeListBox_MouseDown);
            // 
            // AutoFill
            // 
            this.AutoFill.Location = new System.Drawing.Point(12, 110);
            this.AutoFill.Margin = new System.Windows.Forms.Padding(4);
            this.AutoFill.Name = "AutoFill";
            this.AutoFill.Size = new System.Drawing.Size(283, 28);
            this.AutoFill.TabIndex = 7;
            this.AutoFill.Text = "AutoFill";
            this.AutoFill.UseVisualStyleBackColor = true;
            // 
            // LeftWheelsPanel
            // 
            this.LeftWheelsPanel.AutoScroll = true;
            this.LeftWheelsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LeftWheelsPanel.Location = new System.Drawing.Point(4, 19);
            this.LeftWheelsPanel.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.LeftWheelsPanel.Name = "LeftWheelsPanel";
            this.LeftWheelsPanel.Size = new System.Drawing.Size(305, 547);
            this.LeftWheelsPanel.TabIndex = 4;
            this.LeftWheelsPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.LeftWheelsPanel_DragDrop);
            this.LeftWheelsPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.LeftWheelsPanel_DragEnter);
            // 
            // LeftWheelsGroup
            // 
            this.LeftWheelsGroup.AllowDrop = true;
            this.LeftWheelsGroup.Controls.Add(this.LeftWheelsPanel);
            this.LeftWheelsGroup.Location = new System.Drawing.Point(0, 233);
            this.LeftWheelsGroup.Margin = new System.Windows.Forms.Padding(4);
            this.LeftWheelsGroup.Name = "LeftWheelsGroup";
            this.LeftWheelsGroup.Padding = new System.Windows.Forms.Padding(4);
            this.LeftWheelsGroup.Size = new System.Drawing.Size(313, 570);
            this.LeftWheelsGroup.TabIndex = 1;
            this.LeftWheelsGroup.TabStop = false;
            this.LeftWheelsGroup.Text = "Left Wheels";
            this.LeftWheelsGroup.DragDrop += new System.Windows.Forms.DragEventHandler(this.LeftWheelsPanel_DragDrop);
            this.LeftWheelsGroup.DragEnter += new System.Windows.Forms.DragEventHandler(this.LeftWheelsPanel_DragEnter);
            // 
            // RobotInfoGroupBox
            // 
            this.RobotInfoGroupBox.BackColor = System.Drawing.SystemColors.Control;
            this.RobotInfoGroupBox.Controls.Add(this.WeightUnitSelector);
            this.RobotInfoGroupBox.Controls.Add(this.WeightBox);
            this.RobotInfoGroupBox.Controls.Add(this.label3);
            this.RobotInfoGroupBox.Controls.Add(this.DriveTrainDropdown);
            this.RobotInfoGroupBox.Controls.Add(this.DriveTrainLabel);
            this.RobotInfoGroupBox.Location = new System.Drawing.Point(0, 39);
            this.RobotInfoGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.RobotInfoGroupBox.Name = "RobotInfoGroupBox";
            this.RobotInfoGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.RobotInfoGroupBox.Size = new System.Drawing.Size(613, 62);
            this.RobotInfoGroupBox.TabIndex = 6;
            this.RobotInfoGroupBox.TabStop = false;
            this.RobotInfoGroupBox.Text = "Drive Information";
            // 
            // WeightUnitSelector
            // 
            this.WeightUnitSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WeightUnitSelector.DropDownWidth = 60;
            this.WeightUnitSelector.FormattingEnabled = true;
            this.WeightUnitSelector.Items.AddRange(new object[] {
            "Pounds",
            "Kilograms"});
            this.WeightUnitSelector.Location = new System.Drawing.Point(510, 19);
            this.WeightUnitSelector.Margin = new System.Windows.Forms.Padding(4);
            this.WeightUnitSelector.Name = "WeightUnitSelector";
            this.WeightUnitSelector.Size = new System.Drawing.Size(90, 24);
            this.WeightUnitSelector.TabIndex = 4;
            // 
            // WeightBox
            // 
            this.WeightBox.Location = new System.Drawing.Point(365, 22);
            this.WeightBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.WeightBox.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.WeightBox.Name = "WeightBox";
            this.WeightBox.Size = new System.Drawing.Size(133, 22);
            this.WeightBox.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(265, 23);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 17);
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
            this.DriveTrainDropdown.Location = new System.Drawing.Point(95, 21);
            this.DriveTrainDropdown.Margin = new System.Windows.Forms.Padding(4);
            this.DriveTrainDropdown.Name = "DriveTrainDropdown";
            this.DriveTrainDropdown.Size = new System.Drawing.Size(160, 24);
            this.DriveTrainDropdown.TabIndex = 1;
            this.DriveTrainDropdown.SelectionChangeCommitted += new System.EventHandler(this.DriveTrainDropdown_SelectedIndexChanged);
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
            // 
            // RightWheelsPanel
            // 
            this.RightWheelsPanel.AutoScroll = true;
            this.RightWheelsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RightWheelsPanel.Location = new System.Drawing.Point(4, 19);
            this.RightWheelsPanel.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.RightWheelsPanel.Name = "RightWheelsPanel";
            this.RightWheelsPanel.Size = new System.Drawing.Size(284, 547);
            this.RightWheelsPanel.TabIndex = 7;
            this.RightWheelsPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.RightWheelsPanel_DragDrop);
            this.RightWheelsPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.RightWheelsPanel_DragEnter);
            // 
            // RightWheelsGroup
            // 
            this.RightWheelsGroup.AllowDrop = true;
            this.RightWheelsGroup.Controls.Add(this.RightWheelsPanel);
            this.RightWheelsGroup.Location = new System.Drawing.Point(321, 233);
            this.RightWheelsGroup.Margin = new System.Windows.Forms.Padding(4);
            this.RightWheelsGroup.Name = "RightWheelsGroup";
            this.RightWheelsGroup.Padding = new System.Windows.Forms.Padding(4);
            this.RightWheelsGroup.Size = new System.Drawing.Size(292, 570);
            this.RightWheelsGroup.TabIndex = 2;
            this.RightWheelsGroup.TabStop = false;
            this.RightWheelsGroup.Text = "Right Wheels";
            this.RightWheelsGroup.DragDrop += new System.Windows.Forms.DragEventHandler(this.RightWheelsPanel_DragDrop);
            this.RightWheelsGroup.DragEnter += new System.Windows.Forms.DragEventHandler(this.RightWheelsPanel_DragEnter);
            // 
            // DefineWheelsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.RightWheelsGroup);
            this.Controls.Add(this.LeftWheelsGroup);
            this.Controls.Add(this.RobotInfoGroupBox);
            this.Controls.Add(this.WheelNodeGroupBox);
            this.Controls.Add(this.DefineWheelsTitleLabel);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "DefineWheelsPage";
            this.Size = new System.Drawing.Size(613, 804);
            this.WheelNodeGroupBox.ResumeLayout(false);
            this.LeftWheelsGroup.ResumeLayout(false);
            this.RobotInfoGroupBox.ResumeLayout(false);
            this.RobotInfoGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WeightBox)).EndInit();
            this.RightWheelsGroup.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label DefineWheelsTitleLabel;
        private System.Windows.Forms.Label DefineWheelsInstruction1;
        private System.Windows.Forms.GroupBox WheelNodeGroupBox;
        private System.Windows.Forms.FlowLayoutPanel LeftWheelsPanel;
        private System.Windows.Forms.GroupBox RobotInfoGroupBox;
        private System.Windows.Forms.ComboBox DriveTrainDropdown;
        private System.Windows.Forms.Label DriveTrainLabel;
        private System.Windows.Forms.Button AutoFill; 
        private System.Windows.Forms.FlowLayoutPanel RightWheelsPanel;
        private System.Windows.Forms.ListBox NodeListBox;
        private System.Windows.Forms.NumericUpDown WeightBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox LeftWheelsGroup;
        private System.Windows.Forms.GroupBox RightWheelsGroup;
        private System.Windows.Forms.ComboBox WeightUnitSelector;
    }
}
