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
            this.WheelJointsLayout = new System.Windows.Forms.TableLayoutPanel();
            this.NodeListBox = new System.Windows.Forms.ListBox();
            this.AutoFill = new System.Windows.Forms.Button();
            this.LeftWheelsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.LeftWheelsGroup = new System.Windows.Forms.GroupBox();
            this.RobotInfoGroupBox = new System.Windows.Forms.GroupBox();
            this.DriveInfoLayout = new System.Windows.Forms.TableLayoutPanel();
            this.DrivetrainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.DriveTrainLabel = new System.Windows.Forms.Label();
            this.DriveTrainDropdown = new System.Windows.Forms.ComboBox();
            this.WeightLayout = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.WeightUnitSelector = new System.Windows.Forms.ComboBox();
            this.WeightBox = new System.Windows.Forms.NumericUpDown();
            this.RightWheelsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.RightWheelsGroup = new System.Windows.Forms.GroupBox();
            this.MainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.WheelNodeGroupBox.SuspendLayout();
            this.WheelJointsLayout.SuspendLayout();
            this.LeftWheelsGroup.SuspendLayout();
            this.RobotInfoGroupBox.SuspendLayout();
            this.DriveInfoLayout.SuspendLayout();
            this.DrivetrainLayout.SuspendLayout();
            this.WeightLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WeightBox)).BeginInit();
            this.RightWheelsGroup.SuspendLayout();
            this.MainLayout.SuspendLayout();
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
            this.DefineWheelsInstruction1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DefineWheelsInstruction1.Location = new System.Drawing.Point(227, 3);
            this.DefineWheelsInstruction1.Margin = new System.Windows.Forms.Padding(3);
            this.DefineWheelsInstruction1.Name = "DefineWheelsInstruction1";
            this.DefineWheelsInstruction1.Size = new System.Drawing.Size(218, 76);
            this.DefineWheelsInstruction1.TabIndex = 2;
            this.DefineWheelsInstruction1.Text = "Drag the appropriate parts from the list to the left into their respective column" +
    "s below.";
            this.DefineWheelsInstruction1.Click += new System.EventHandler(this.DefineWheelsInstruction1_Click);
            // 
            // WheelNodeGroupBox
            // 
            this.MainLayout.SetColumnSpan(this.WheelNodeGroupBox, 2);
            this.WheelNodeGroupBox.Controls.Add(this.WheelJointsLayout);
            this.WheelNodeGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.WheelNodeGroupBox.Location = new System.Drawing.Point(3, 55);
            this.WheelNodeGroupBox.Name = "WheelNodeGroupBox";
            this.WheelNodeGroupBox.Size = new System.Drawing.Size(454, 130);
            this.WheelNodeGroupBox.TabIndex = 3;
            this.WheelNodeGroupBox.TabStop = false;
            this.WheelNodeGroupBox.Text = "Select Wheels";
            // 
            // WheelJointsLayout
            // 
            this.WheelJointsLayout.ColumnCount = 2;
            this.WheelJointsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.WheelJointsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.WheelJointsLayout.Controls.Add(this.NodeListBox, 0, 0);
            this.WheelJointsLayout.Controls.Add(this.AutoFill, 1, 1);
            this.WheelJointsLayout.Controls.Add(this.DefineWheelsInstruction1, 1, 0);
            this.WheelJointsLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WheelJointsLayout.Location = new System.Drawing.Point(3, 16);
            this.WheelJointsLayout.Name = "WheelJointsLayout";
            this.WheelJointsLayout.RowCount = 2;
            this.WheelJointsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.WheelJointsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.WheelJointsLayout.Size = new System.Drawing.Size(448, 111);
            this.WheelJointsLayout.TabIndex = 8;
            // 
            // NodeListBox
            // 
            this.NodeListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NodeListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NodeListBox.FormattingEnabled = true;
            this.NodeListBox.Location = new System.Drawing.Point(3, 3);
            this.NodeListBox.Name = "NodeListBox";
            this.WheelJointsLayout.SetRowSpan(this.NodeListBox, 2);
            this.NodeListBox.Size = new System.Drawing.Size(218, 105);
            this.NodeListBox.TabIndex = 4;
            this.NodeListBox.SelectedIndexChanged += new System.EventHandler(this.NodeListBox_SelectedIndexChanged);
            this.NodeListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.NodeListBox_MouseDown);
            // 
            // AutoFill
            // 
            this.AutoFill.Dock = System.Windows.Forms.DockStyle.Top;
            this.AutoFill.Location = new System.Drawing.Point(227, 85);
            this.AutoFill.Name = "AutoFill";
            this.AutoFill.Size = new System.Drawing.Size(218, 23);
            this.AutoFill.TabIndex = 7;
            this.AutoFill.Text = "AutoFill";
            this.AutoFill.UseVisualStyleBackColor = true;
            // 
            // LeftWheelsPanel
            // 
            this.LeftWheelsPanel.AutoScroll = true;
            this.LeftWheelsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LeftWheelsPanel.Location = new System.Drawing.Point(3, 16);
            this.LeftWheelsPanel.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.LeftWheelsPanel.Name = "LeftWheelsPanel";
            this.LeftWheelsPanel.Size = new System.Drawing.Size(218, 417);
            this.LeftWheelsPanel.TabIndex = 4;
            this.LeftWheelsPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.LeftWheelsPanel_DragDrop);
            this.LeftWheelsPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.Panel_DragEnter);
            // 
            // LeftWheelsGroup
            // 
            this.LeftWheelsGroup.AllowDrop = true;
            this.LeftWheelsGroup.Controls.Add(this.LeftWheelsPanel);
            this.LeftWheelsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LeftWheelsGroup.Location = new System.Drawing.Point(3, 191);
            this.LeftWheelsGroup.Name = "LeftWheelsGroup";
            this.LeftWheelsGroup.Size = new System.Drawing.Size(224, 436);
            this.LeftWheelsGroup.TabIndex = 1;
            this.LeftWheelsGroup.TabStop = false;
            this.LeftWheelsGroup.Text = "Left Wheels";
            this.LeftWheelsGroup.DragDrop += new System.Windows.Forms.DragEventHandler(this.LeftWheelsPanel_DragDrop);
            this.LeftWheelsGroup.DragEnter += new System.Windows.Forms.DragEventHandler(this.Panel_DragEnter);
            // 
            // RobotInfoGroupBox
            // 
            this.RobotInfoGroupBox.AutoSize = true;
            this.RobotInfoGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.RobotInfoGroupBox.BackColor = System.Drawing.SystemColors.Control;
            this.MainLayout.SetColumnSpan(this.RobotInfoGroupBox, 2);
            this.RobotInfoGroupBox.Controls.Add(this.DriveInfoLayout);
            this.RobotInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.RobotInfoGroupBox.Location = new System.Drawing.Point(3, 3);
            this.RobotInfoGroupBox.Name = "RobotInfoGroupBox";
            this.RobotInfoGroupBox.Size = new System.Drawing.Size(454, 46);
            this.RobotInfoGroupBox.TabIndex = 6;
            this.RobotInfoGroupBox.TabStop = false;
            this.RobotInfoGroupBox.Text = "General Information";
            // 
            // DriveInfoLayout
            // 
            this.DriveInfoLayout.AutoSize = true;
            this.DriveInfoLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DriveInfoLayout.ColumnCount = 2;
            this.DriveInfoLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.DriveInfoLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.DriveInfoLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.DriveInfoLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.DriveInfoLayout.Controls.Add(this.DrivetrainLayout, 0, 0);
            this.DriveInfoLayout.Controls.Add(this.WeightLayout, 1, 0);
            this.DriveInfoLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.DriveInfoLayout.Location = new System.Drawing.Point(3, 16);
            this.DriveInfoLayout.Name = "DriveInfoLayout";
            this.DriveInfoLayout.RowCount = 1;
            this.DriveInfoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.DriveInfoLayout.Size = new System.Drawing.Size(448, 27);
            this.DriveInfoLayout.TabIndex = 5;
            // 
            // DrivetrainLayout
            // 
            this.DrivetrainLayout.AutoSize = true;
            this.DrivetrainLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DrivetrainLayout.ColumnCount = 2;
            this.DrivetrainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.DrivetrainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.DrivetrainLayout.Controls.Add(this.DriveTrainLabel, 0, 0);
            this.DrivetrainLayout.Controls.Add(this.DriveTrainDropdown, 1, 0);
            this.DrivetrainLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.DrivetrainLayout.Location = new System.Drawing.Point(0, 0);
            this.DrivetrainLayout.Margin = new System.Windows.Forms.Padding(0);
            this.DrivetrainLayout.Name = "DrivetrainLayout";
            this.DrivetrainLayout.RowCount = 1;
            this.DrivetrainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.DrivetrainLayout.Size = new System.Drawing.Size(224, 27);
            this.DrivetrainLayout.TabIndex = 0;
            // 
            // DriveTrainLabel
            // 
            this.DriveTrainLabel.AutoSize = true;
            this.DriveTrainLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.DriveTrainLabel.Location = new System.Drawing.Point(3, 3);
            this.DriveTrainLabel.Margin = new System.Windows.Forms.Padding(3);
            this.DriveTrainLabel.Name = "DriveTrainLabel";
            this.DriveTrainLabel.Size = new System.Drawing.Size(59, 21);
            this.DriveTrainLabel.TabIndex = 0;
            this.DriveTrainLabel.Text = "Drive Train";
            this.DriveTrainLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // DriveTrainDropdown
            // 
            this.DriveTrainDropdown.Dock = System.Windows.Forms.DockStyle.Top;
            this.DriveTrainDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DriveTrainDropdown.FormattingEnabled = true;
            this.DriveTrainDropdown.Items.AddRange(new object[] {
            "Select Drive Train...",
            "Tank",
            "Mecanum",
            "Swerve",
            "H-Drive",
            "Other/Custom"});
            this.DriveTrainDropdown.Location = new System.Drawing.Point(68, 3);
            this.DriveTrainDropdown.Name = "DriveTrainDropdown";
            this.DriveTrainDropdown.Size = new System.Drawing.Size(153, 21);
            this.DriveTrainDropdown.TabIndex = 1;
            this.DriveTrainDropdown.SelectionChangeCommitted += new System.EventHandler(this.DriveTrainDropdown_SelectedIndexChanged);
            // 
            // WeightLayout
            // 
            this.WeightLayout.AutoSize = true;
            this.WeightLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.WeightLayout.ColumnCount = 3;
            this.WeightLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.WeightLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.WeightLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.WeightLayout.Controls.Add(this.label3, 0, 0);
            this.WeightLayout.Controls.Add(this.WeightUnitSelector, 2, 0);
            this.WeightLayout.Controls.Add(this.WeightBox, 1, 0);
            this.WeightLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.WeightLayout.Location = new System.Drawing.Point(224, 0);
            this.WeightLayout.Margin = new System.Windows.Forms.Padding(0);
            this.WeightLayout.Name = "WeightLayout";
            this.WeightLayout.RowCount = 1;
            this.WeightLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.WeightLayout.Size = new System.Drawing.Size(224, 27);
            this.WeightLayout.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Left;
            this.label3.Location = new System.Drawing.Point(3, 3);
            this.label3.Margin = new System.Windows.Forms.Padding(3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 21);
            this.label3.TabIndex = 2;
            this.label3.Text = "Robot Weight";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // WeightUnitSelector
            // 
            this.WeightUnitSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WeightUnitSelector.DropDownWidth = 60;
            this.WeightUnitSelector.FormattingEnabled = true;
            this.WeightUnitSelector.Items.AddRange(new object[] {
            "Pounds",
            "Kilograms"});
            this.WeightUnitSelector.Location = new System.Drawing.Point(169, 3);
            this.WeightUnitSelector.Name = "WeightUnitSelector";
            this.WeightUnitSelector.Size = new System.Drawing.Size(52, 21);
            this.WeightUnitSelector.TabIndex = 4;
            // 
            // WeightBox
            // 
            this.WeightBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WeightBox.Location = new System.Drawing.Point(82, 3);
            this.WeightBox.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.WeightBox.Name = "WeightBox";
            this.WeightBox.Size = new System.Drawing.Size(81, 20);
            this.WeightBox.TabIndex = 3;
            // 
            // RightWheelsPanel
            // 
            this.RightWheelsPanel.AutoScroll = true;
            this.RightWheelsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RightWheelsPanel.Location = new System.Drawing.Point(3, 16);
            this.RightWheelsPanel.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.RightWheelsPanel.Name = "RightWheelsPanel";
            this.RightWheelsPanel.Size = new System.Drawing.Size(218, 417);
            this.RightWheelsPanel.TabIndex = 7;
            this.RightWheelsPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.RightWheelsPanel_DragDrop);
            this.RightWheelsPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.Panel_DragEnter);
            // 
            // RightWheelsGroup
            // 
            this.RightWheelsGroup.AllowDrop = true;
            this.RightWheelsGroup.Controls.Add(this.RightWheelsPanel);
            this.RightWheelsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RightWheelsGroup.Location = new System.Drawing.Point(233, 191);
            this.RightWheelsGroup.Name = "RightWheelsGroup";
            this.RightWheelsGroup.Size = new System.Drawing.Size(224, 436);
            this.RightWheelsGroup.TabIndex = 2;
            this.RightWheelsGroup.TabStop = false;
            this.RightWheelsGroup.Text = "Right Wheels";
            this.RightWheelsGroup.DragDrop += new System.Windows.Forms.DragEventHandler(this.RightWheelsPanel_DragDrop);
            this.RightWheelsGroup.DragEnter += new System.Windows.Forms.DragEventHandler(this.Panel_DragEnter);
            // 
            // MainLayout
            // 
            this.MainLayout.ColumnCount = 2;
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.MainLayout.Controls.Add(this.RobotInfoGroupBox, 0, 0);
            this.MainLayout.Controls.Add(this.RightWheelsGroup, 1, 2);
            this.MainLayout.Controls.Add(this.WheelNodeGroupBox, 0, 1);
            this.MainLayout.Controls.Add(this.LeftWheelsGroup, 0, 2);
            this.MainLayout.Location = new System.Drawing.Point(0, 23);
            this.MainLayout.Name = "MainLayout";
            this.MainLayout.RowCount = 3;
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayout.Size = new System.Drawing.Size(460, 630);
            this.MainLayout.TabIndex = 7;
            // 
            // DefineWheelsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.MainLayout);
            this.Controls.Add(this.DefineWheelsTitleLabel);
            this.Name = "DefineWheelsPage";
            this.Size = new System.Drawing.Size(460, 653);
            this.WheelNodeGroupBox.ResumeLayout(false);
            this.WheelJointsLayout.ResumeLayout(false);
            this.LeftWheelsGroup.ResumeLayout(false);
            this.RobotInfoGroupBox.ResumeLayout(false);
            this.RobotInfoGroupBox.PerformLayout();
            this.DriveInfoLayout.ResumeLayout(false);
            this.DriveInfoLayout.PerformLayout();
            this.DrivetrainLayout.ResumeLayout(false);
            this.DrivetrainLayout.PerformLayout();
            this.WeightLayout.ResumeLayout(false);
            this.WeightLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WeightBox)).EndInit();
            this.RightWheelsGroup.ResumeLayout(false);
            this.MainLayout.ResumeLayout(false);
            this.MainLayout.PerformLayout();
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
        private System.Windows.Forms.TableLayoutPanel MainLayout;
        private System.Windows.Forms.TableLayoutPanel WheelJointsLayout;
        private System.Windows.Forms.TableLayoutPanel DriveInfoLayout;
        private System.Windows.Forms.TableLayoutPanel DrivetrainLayout;
        private System.Windows.Forms.TableLayoutPanel WeightLayout;
    }
}
