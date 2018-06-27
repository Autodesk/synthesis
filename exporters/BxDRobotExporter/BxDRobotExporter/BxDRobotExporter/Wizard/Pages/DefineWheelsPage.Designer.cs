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
            this.LeftWheelPropertiesPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.WarningLabel = new System.Windows.Forms.Label();
            this.RobotInfoGroupBox = new System.Windows.Forms.GroupBox();
            this.MetricCheckBox = new System.Windows.Forms.CheckBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.DriveTrainDropdown = new System.Windows.Forms.ComboBox();
            this.DriveTrainLabel = new System.Windows.Forms.Label();
            this.RightWheelPropertiesPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.WheelNodeGroupBox.SuspendLayout();
            this.LeftWheelPropertiesPanel.SuspendLayout();
            this.RobotInfoGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.RightWheelPropertiesPanel.SuspendLayout();
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
            this.DefineWheelsInstruction1.Location = new System.Drawing.Point(408, 23);
            this.DefineWheelsInstruction1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.DefineWheelsInstruction1.Name = "DefineWheelsInstruction1";
            this.DefineWheelsInstruction1.Size = new System.Drawing.Size(189, 69);
            this.DefineWheelsInstruction1.TabIndex = 2;
            this.DefineWheelsInstruction1.Text = "Check the boxes of all of the nodes which are drive wheels. Select a node to focu" +
    "s on it in Inventor.";
            // 
            // WheelNodeGroupBox
            // 
            this.WheelNodeGroupBox.Controls.Add(this.label4);
            this.WheelNodeGroupBox.Controls.Add(this.NodeListBox);
            this.WheelNodeGroupBox.Controls.Add(this.DefineWheelsInstruction1);
            this.WheelNodeGroupBox.Location = new System.Drawing.Point(0, 110);
            this.WheelNodeGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.WheelNodeGroupBox.Name = "WheelNodeGroupBox";
            this.WheelNodeGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.WheelNodeGroupBox.Size = new System.Drawing.Size(613, 146);
            this.WheelNodeGroupBox.TabIndex = 3;
            this.WheelNodeGroupBox.TabStop = false;
            this.WheelNodeGroupBox.Text = "Select Wheel Joints";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(321, 123);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 17);
            this.label4.TabIndex = 5;
            // 
            // NodeListBox
            // 
            this.NodeListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NodeListBox.FormattingEnabled = true;
            this.NodeListBox.ItemHeight = 22;
            this.NodeListBox.Location = new System.Drawing.Point(12, 22);
            this.NodeListBox.Name = "NodeListBox";
            this.NodeListBox.Size = new System.Drawing.Size(389, 92);
            this.NodeListBox.TabIndex = 4;
            this.NodeListBox.SelectedIndexChanged += new System.EventHandler(this.NodeListBox_SelectedIndexChanged);
            this.NodeListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.NodeListBox_MouseDown);
            // 
            // LeftWheelPropertiesPanel
            // 
            this.LeftWheelPropertiesPanel.AutoScroll = true;
            this.LeftWheelPropertiesPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LeftWheelPropertiesPanel.Controls.Add(this.label1);
            this.LeftWheelPropertiesPanel.Location = new System.Drawing.Point(0, 254);
            this.LeftWheelPropertiesPanel.Margin = new System.Windows.Forms.Padding(4);
            this.LeftWheelPropertiesPanel.Name = "LeftWheelPropertiesPanel";
            this.LeftWheelPropertiesPanel.Size = new System.Drawing.Size(308, 549);
            this.LeftWheelPropertiesPanel.TabIndex = 4;
            this.LeftWheelPropertiesPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.LeftWheelPropertiesPanel_DragDrop);
            this.LeftWheelPropertiesPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.LeftWheelPropertiesPanel_DragEnter);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(256, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Please drag left wheels here";
            // 
            // WarningLabel
            // 
            this.WarningLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WarningLabel.ForeColor = System.Drawing.Color.Red;
            this.WarningLabel.Location = new System.Drawing.Point(5, 25);
            this.WarningLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.WarningLabel.Name = "WarningLabel";
            this.WarningLabel.Size = new System.Drawing.Size(592, 59);
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
            this.RobotInfoGroupBox.Location = new System.Drawing.Point(0, 40);
            this.RobotInfoGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.RobotInfoGroupBox.Name = "RobotInfoGroupBox";
            this.RobotInfoGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.RobotInfoGroupBox.Size = new System.Drawing.Size(613, 62);
            this.RobotInfoGroupBox.TabIndex = 6;
            this.RobotInfoGroupBox.TabStop = false;
            this.RobotInfoGroupBox.Text = "Drive Information";
            // 
            // MetricCheckBox
            // 
            this.MetricCheckBox.AutoSize = true;
            this.MetricCheckBox.Location = new System.Drawing.Point(506, 21);
            this.MetricCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.MetricCheckBox.Name = "MetricCheckBox";
            this.MetricCheckBox.Size = new System.Drawing.Size(104, 21);
            this.MetricCheckBox.TabIndex = 4;
            this.MetricCheckBox.Text = "Metric Units";
            this.MetricCheckBox.UseVisualStyleBackColor = true;
            this.MetricCheckBox.CheckedChanged += new System.EventHandler(this.MetricCheckBox_CheckedChanged);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(366, 22);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(133, 22);
            this.numericUpDown1.TabIndex = 3;
            this.numericUpDown1.TextChanged += new System.EventHandler(this.NumericUpDown1_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(265, 23);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Robo Weight:";
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
            // RightWheelPropertiesPanel
            // 
            this.RightWheelPropertiesPanel.AutoScroll = true;
            this.RightWheelPropertiesPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RightWheelPropertiesPanel.Controls.Add(this.label2);
            this.RightWheelPropertiesPanel.Location = new System.Drawing.Point(316, 254);
            this.RightWheelPropertiesPanel.Margin = new System.Windows.Forms.Padding(4);
            this.RightWheelPropertiesPanel.Name = "RightWheelPropertiesPanel";
            this.RightWheelPropertiesPanel.Size = new System.Drawing.Size(297, 549);
            this.RightWheelPropertiesPanel.TabIndex = 7;
            this.RightWheelPropertiesPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.RightWheelPropertiesPanel_DragDrop);
            this.RightWheelPropertiesPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.RightWheelPropertiesPanel_DragEnter);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(268, 25);
            this.label2.TabIndex = 1;
            this.label2.Text = "Please drag right wheels here";
            // 
            // DefineWheelsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.RightWheelPropertiesPanel);
            this.Controls.Add(this.RobotInfoGroupBox);
            this.Controls.Add(this.WarningLabel);
            this.Controls.Add(this.LeftWheelPropertiesPanel);
            this.Controls.Add(this.WheelNodeGroupBox);
            this.Controls.Add(this.DefineWheelsTitleLabel);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "DefineWheelsPage";
            this.Size = new System.Drawing.Size(613, 804);
            this.WheelNodeGroupBox.ResumeLayout(false);
            this.WheelNodeGroupBox.PerformLayout();
            this.LeftWheelPropertiesPanel.ResumeLayout(false);
            this.LeftWheelPropertiesPanel.PerformLayout();
            this.RobotInfoGroupBox.ResumeLayout(false);
            this.RobotInfoGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.RightWheelPropertiesPanel.ResumeLayout(false);
            this.RightWheelPropertiesPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label DefineWheelsTitleLabel;
        private System.Windows.Forms.Label DefineWheelsInstruction1;
        private System.Windows.Forms.GroupBox WheelNodeGroupBox;
        private System.Windows.Forms.FlowLayoutPanel LeftWheelPropertiesPanel;
        private System.Windows.Forms.Label WarningLabel;
        private System.Windows.Forms.GroupBox RobotInfoGroupBox;
        private System.Windows.Forms.ComboBox DriveTrainDropdown;
        private System.Windows.Forms.Label DriveTrainLabel;
        private System.Windows.Forms.FlowLayoutPanel RightWheelPropertiesPanel;
        private System.Windows.Forms.ListBox NodeListBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox MetricCheckBox;
        private System.Windows.Forms.Label label4;
    }
}
