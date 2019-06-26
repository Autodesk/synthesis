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
            this.components = new System.ComponentModel.Container();
            this.DefineWheelsInstruction = new System.Windows.Forms.Label();
            this.WheelNodeGroupBox = new System.Windows.Forms.GroupBox();
            this.WheelJointsLayout = new System.Windows.Forms.TableLayoutPanel();
            this.RemoveWheelsButton = new System.Windows.Forms.Button();
            this.NodeListBox = new System.Windows.Forms.ListBox();
            this.AutoFillButton = new System.Windows.Forms.Button();
            this.LeftWheelsGroup = new System.Windows.Forms.GroupBox();
            this.LeftWheelsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.RobotInfoGroupBox = new System.Windows.Forms.GroupBox();
            this.RobotInfoLayout = new System.Windows.Forms.TableLayoutPanel();
            this.DriveTrainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.DriveTrainLabel = new System.Windows.Forms.Label();
            this.DriveTrainDropdown = new System.Windows.Forms.ComboBox();
            this.WeightLayout = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.WeightUnitSelector = new System.Windows.Forms.ComboBox();
            this.WeightBox = new System.Windows.Forms.NumericUpDown();
            this.RightWheelsGroup = new System.Windows.Forms.GroupBox();
            this.RightWheelsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.MainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.MiddleWheelsGroup = new System.Windows.Forms.GroupBox();
            this.MiddleWheelsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.RightBackWheelsGroup = new System.Windows.Forms.GroupBox();
            this.RightBackWheelsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.LeftBackWheelsGroup = new System.Windows.Forms.GroupBox();
            this.LeftBackWheelsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.AutoFillToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.WheelNodeGroupBox.SuspendLayout();
            this.WheelJointsLayout.SuspendLayout();
            this.LeftWheelsGroup.SuspendLayout();
            this.RobotInfoGroupBox.SuspendLayout();
            this.RobotInfoLayout.SuspendLayout();
            this.DriveTrainLayout.SuspendLayout();
            this.WeightLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.WeightBox)).BeginInit();
            this.RightWheelsGroup.SuspendLayout();
            this.MainLayout.SuspendLayout();
            this.MiddleWheelsGroup.SuspendLayout();
            this.RightBackWheelsGroup.SuspendLayout();
            this.LeftBackWheelsGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // DefineWheelsInstruction
            // 
            this.DefineWheelsInstruction.AutoSize = true;
            this.DefineWheelsInstruction.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DefineWheelsInstruction.Location = new System.Drawing.Point(302, 5);
            this.DefineWheelsInstruction.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.DefineWheelsInstruction.Name = "DefineWheelsInstruction";
            this.DefineWheelsInstruction.Size = new System.Drawing.Size(291, 40);
            this.DefineWheelsInstruction.TabIndex = 2;
            this.DefineWheelsInstruction.Text = "Drag wheel parts from the list to the left into the appropriate column below.";
            // 
            // WheelNodeGroupBox
            // 
            this.WheelNodeGroupBox.AutoSize = true;
            this.WheelNodeGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainLayout.SetColumnSpan(this.WheelNodeGroupBox, 2);
            this.WheelNodeGroupBox.Controls.Add(this.WheelJointsLayout);
            this.WheelNodeGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.WheelNodeGroupBox.Location = new System.Drawing.Point(4, 83);
            this.WheelNodeGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.WheelNodeGroupBox.Name = "WheelNodeGroupBox";
            this.WheelNodeGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.WheelNodeGroupBox.Size = new System.Drawing.Size(605, 165);
            this.WheelNodeGroupBox.TabIndex = 3;
            this.WheelNodeGroupBox.TabStop = false;
            this.WheelNodeGroupBox.Text = "Select Wheels";
            // 
            // WheelJointsLayout
            // 
            this.WheelJointsLayout.AutoSize = true;
            this.WheelJointsLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.WheelJointsLayout.ColumnCount = 2;
            this.WheelJointsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.WheelJointsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.WheelJointsLayout.Controls.Add(this.RemoveWheelsButton, 1, 2);
            this.WheelJointsLayout.Controls.Add(this.NodeListBox, 0, 0);
            this.WheelJointsLayout.Controls.Add(this.AutoFillButton, 1, 1);
            this.WheelJointsLayout.Controls.Add(this.DefineWheelsInstruction, 1, 0);
            this.WheelJointsLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.WheelJointsLayout.Location = new System.Drawing.Point(4, 25);
            this.WheelJointsLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.WheelJointsLayout.Name = "WheelJointsLayout";
            this.WheelJointsLayout.RowCount = 3;
            this.WheelJointsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.WheelJointsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.WheelJointsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.WheelJointsLayout.Size = new System.Drawing.Size(597, 140);
            this.WheelJointsLayout.TabIndex = 8;
            // 
            // RemoveWheelsButton
            // 
            this.RemoveWheelsButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.RemoveWheelsButton.Location = new System.Drawing.Point(302, 100);
            this.RemoveWheelsButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RemoveWheelsButton.Name = "RemoveWheelsButton";
            this.RemoveWheelsButton.Size = new System.Drawing.Size(291, 35);
            this.RemoveWheelsButton.TabIndex = 8;
            this.RemoveWheelsButton.Text = "Remove All Wheels";
            this.RemoveWheelsButton.UseVisualStyleBackColor = true;
            this.RemoveWheelsButton.Click += new System.EventHandler(this.RemoveWheelsButton_Click);
            // 
            // NodeListBox
            // 
            this.NodeListBox.AllowDrop = true;
            this.NodeListBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.NodeListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.NodeListBox.FormattingEnabled = true;
            this.NodeListBox.ItemHeight = 17;
            this.NodeListBox.Location = new System.Drawing.Point(4, 6);
            this.NodeListBox.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.NodeListBox.Name = "NodeListBox";
            this.WheelJointsLayout.SetRowSpan(this.NodeListBox, 3);
            this.NodeListBox.Size = new System.Drawing.Size(290, 123);
            this.NodeListBox.TabIndex = 4;
            this.NodeListBox.SelectedIndexChanged += new System.EventHandler(this.NodeListBox_SelectedIndexChanged);
            this.NodeListBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.NodeListBox_DragDrop);
            this.NodeListBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.Field_DragEnter);
            this.NodeListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.NodeListBox_MouseDown);
            // 
            // AutoFillButton
            // 
            this.AutoFillButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.AutoFillButton.Location = new System.Drawing.Point(302, 55);
            this.AutoFillButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.AutoFillButton.Name = "AutoFillButton";
            this.AutoFillButton.Size = new System.Drawing.Size(291, 35);
            this.AutoFillButton.TabIndex = 7;
            this.AutoFillButton.Text = "Attempt Auto Fill";
            this.AutoFillButton.UseVisualStyleBackColor = true;
            this.AutoFillButton.Click += new System.EventHandler(this.AutoFill_Click);
            // 
            // LeftWheelsGroup
            // 
            this.LeftWheelsGroup.AllowDrop = true;
            this.LeftWheelsGroup.Controls.Add(this.LeftWheelsPanel);
            this.LeftWheelsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LeftWheelsGroup.Location = new System.Drawing.Point(4, 258);
            this.LeftWheelsGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.LeftWheelsGroup.Name = "LeftWheelsGroup";
            this.LeftWheelsGroup.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.LeftWheelsGroup.Size = new System.Drawing.Size(298, 518);
            this.LeftWheelsGroup.TabIndex = 1;
            this.LeftWheelsGroup.TabStop = false;
            this.LeftWheelsGroup.Text = "Left Wheels";
            this.LeftWheelsGroup.DragDrop += new System.Windows.Forms.DragEventHandler(this.LeftWheelsPanel_DragDrop);
            this.LeftWheelsGroup.DragEnter += new System.Windows.Forms.DragEventHandler(this.Field_DragEnter);
            // 
            // LeftWheelsPanel
            // 
            this.LeftWheelsPanel.AllowDrop = true;
            this.LeftWheelsPanel.AutoScroll = true;
            this.LeftWheelsPanel.ColumnCount = 1;
            this.LeftWheelsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LeftWheelsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.LeftWheelsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LeftWheelsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.LeftWheelsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LeftWheelsPanel.Location = new System.Drawing.Point(4, 25);
            this.LeftWheelsPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.LeftWheelsPanel.Name = "LeftWheelsPanel";
            this.LeftWheelsPanel.RowCount = 2;
            this.LeftWheelsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 709F));
            this.LeftWheelsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 709F));
            this.LeftWheelsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.LeftWheelsPanel.Size = new System.Drawing.Size(290, 488);
            this.LeftWheelsPanel.TabIndex = 1;
            this.LeftWheelsPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.LeftWheelsPanel_DragDrop);
            this.LeftWheelsPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.Field_DragEnter);
            // 
            // RobotInfoGroupBox
            // 
            this.RobotInfoGroupBox.AutoSize = true;
            this.RobotInfoGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainLayout.SetColumnSpan(this.RobotInfoGroupBox, 2);
            this.RobotInfoGroupBox.Controls.Add(this.RobotInfoLayout);
            this.RobotInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.RobotInfoGroupBox.Location = new System.Drawing.Point(4, 5);
            this.RobotInfoGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RobotInfoGroupBox.Name = "RobotInfoGroupBox";
            this.RobotInfoGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RobotInfoGroupBox.Size = new System.Drawing.Size(605, 68);
            this.RobotInfoGroupBox.TabIndex = 6;
            this.RobotInfoGroupBox.TabStop = false;
            this.RobotInfoGroupBox.Text = "General Information";
            // 
            // RobotInfoLayout
            // 
            this.RobotInfoLayout.AutoSize = true;
            this.RobotInfoLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.RobotInfoLayout.ColumnCount = 2;
            this.RobotInfoLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.RobotInfoLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.RobotInfoLayout.Controls.Add(this.DriveTrainLayout, 0, 0);
            this.RobotInfoLayout.Controls.Add(this.WeightLayout, 1, 0);
            this.RobotInfoLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.RobotInfoLayout.Location = new System.Drawing.Point(4, 25);
            this.RobotInfoLayout.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RobotInfoLayout.Name = "RobotInfoLayout";
            this.RobotInfoLayout.RowCount = 1;
            this.RobotInfoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.RobotInfoLayout.Size = new System.Drawing.Size(597, 38);
            this.RobotInfoLayout.TabIndex = 5;
            // 
            // DriveTrainLayout
            // 
            this.DriveTrainLayout.AutoSize = true;
            this.DriveTrainLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DriveTrainLayout.ColumnCount = 2;
            this.DriveTrainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.DriveTrainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.DriveTrainLayout.Controls.Add(this.DriveTrainLabel, 0, 0);
            this.DriveTrainLayout.Controls.Add(this.DriveTrainDropdown, 1, 0);
            this.DriveTrainLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.DriveTrainLayout.Location = new System.Drawing.Point(0, 0);
            this.DriveTrainLayout.Margin = new System.Windows.Forms.Padding(0);
            this.DriveTrainLayout.Name = "DriveTrainLayout";
            this.DriveTrainLayout.RowCount = 1;
            this.DriveTrainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.DriveTrainLayout.Size = new System.Drawing.Size(298, 38);
            this.DriveTrainLayout.TabIndex = 0;
            // 
            // DriveTrainLabel
            // 
            this.DriveTrainLabel.AutoSize = true;
            this.DriveTrainLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.DriveTrainLabel.Location = new System.Drawing.Point(4, 5);
            this.DriveTrainLabel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.DriveTrainLabel.Name = "DriveTrainLabel";
            this.DriveTrainLabel.Size = new System.Drawing.Size(80, 28);
            this.DriveTrainLabel.TabIndex = 0;
            this.DriveTrainLabel.Text = "Drive Train";
            this.DriveTrainLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // DriveTrainDropdown
            // 
            this.DriveTrainDropdown.Dock = System.Windows.Forms.DockStyle.Top;
            this.DriveTrainDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DriveTrainDropdown.FormattingEnabled = true;
            this.DriveTrainDropdown.Items.AddRange(new object[] {"Select drive train...", "Tank", "H-Drive", "Other/Custom"});
            this.DriveTrainDropdown.Location = new System.Drawing.Point(92, 5);
            this.DriveTrainDropdown.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.DriveTrainDropdown.Name = "DriveTrainDropdown";
            this.DriveTrainDropdown.Size = new System.Drawing.Size(209, 28);
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
            this.WeightLayout.Location = new System.Drawing.Point(298, 0);
            this.WeightLayout.Margin = new System.Windows.Forms.Padding(0);
            this.WeightLayout.Name = "WeightLayout";
            this.WeightLayout.RowCount = 1;
            this.WeightLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.WeightLayout.Size = new System.Drawing.Size(299, 38);
            this.WeightLayout.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Left;
            this.label3.Location = new System.Drawing.Point(4, 5);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 28);
            this.label3.TabIndex = 2;
            this.label3.Text = "Weight";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // WeightUnitSelector
            // 
            this.WeightUnitSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WeightUnitSelector.DropDownWidth = 60;
            this.WeightUnitSelector.FormattingEnabled = true;
            this.WeightUnitSelector.Items.AddRange(new object[] {"Pounds", "Kilograms"});
            this.WeightUnitSelector.Location = new System.Drawing.Point(151, 5);
            this.WeightUnitSelector.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.WeightUnitSelector.Name = "WeightUnitSelector";
            this.WeightUnitSelector.Size = new System.Drawing.Size(144, 28);
            this.WeightUnitSelector.TabIndex = 4;
            // 
            // WeightBox
            // 
            this.WeightBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WeightBox.Location = new System.Drawing.Point(68, 5);
            this.WeightBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.WeightBox.Maximum = new decimal(new int[] {150, 0, 0, 0});
            this.WeightBox.Name = "WeightBox";
            this.WeightBox.Size = new System.Drawing.Size(75, 27);
            this.WeightBox.TabIndex = 3;
            // 
            // RightWheelsGroup
            // 
            this.RightWheelsGroup.AllowDrop = true;
            this.RightWheelsGroup.Controls.Add(this.RightWheelsPanel);
            this.RightWheelsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RightWheelsGroup.Location = new System.Drawing.Point(310, 258);
            this.RightWheelsGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RightWheelsGroup.Name = "RightWheelsGroup";
            this.RightWheelsGroup.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RightWheelsGroup.Size = new System.Drawing.Size(299, 518);
            this.RightWheelsGroup.TabIndex = 2;
            this.RightWheelsGroup.TabStop = false;
            this.RightWheelsGroup.Text = "Right Wheels";
            this.RightWheelsGroup.DragDrop += new System.Windows.Forms.DragEventHandler(this.RightWheelsPanel_DragDrop);
            this.RightWheelsGroup.DragEnter += new System.Windows.Forms.DragEventHandler(this.Field_DragEnter);
            // 
            // RightWheelsPanel
            // 
            this.RightWheelsPanel.AllowDrop = true;
            this.RightWheelsPanel.AutoScroll = true;
            this.RightWheelsPanel.ColumnCount = 1;
            this.RightWheelsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.RightWheelsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.RightWheelsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.RightWheelsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.RightWheelsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RightWheelsPanel.Location = new System.Drawing.Point(4, 25);
            this.RightWheelsPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RightWheelsPanel.Name = "RightWheelsPanel";
            this.RightWheelsPanel.RowCount = 2;
            this.RightWheelsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 709F));
            this.RightWheelsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 709F));
            this.RightWheelsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.RightWheelsPanel.Size = new System.Drawing.Size(291, 488);
            this.RightWheelsPanel.TabIndex = 1;
            this.RightWheelsPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.RightWheelsPanel_DragDrop);
            this.RightWheelsPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.Field_DragEnter);
            // 
            // MainLayout
            // 
            this.MainLayout.ColumnCount = 2;
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.MainLayout.Controls.Add(this.RobotInfoGroupBox, 0, 0);
            this.MainLayout.Controls.Add(this.WheelNodeGroupBox, 0, 1);
            this.MainLayout.Controls.Add(this.MiddleWheelsGroup, 0, 3);
            this.MainLayout.Controls.Add(this.RightBackWheelsGroup, 1, 3);
            this.MainLayout.Controls.Add(this.LeftBackWheelsGroup, 0, 3);
            this.MainLayout.Controls.Add(this.LeftWheelsGroup, 0, 2);
            this.MainLayout.Controls.Add(this.RightWheelsGroup, 1, 2);
            this.MainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainLayout.Location = new System.Drawing.Point(0, 0);
            this.MainLayout.Margin = new System.Windows.Forms.Padding(0);
            this.MainLayout.Name = "MainLayout";
            this.MainLayout.RowCount = 4;
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 72.72727F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 27.27273F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.MainLayout.Size = new System.Drawing.Size(613, 1005);
            this.MainLayout.TabIndex = 7;
            // 
            // MiddleWheelsGroup
            // 
            this.MiddleWheelsGroup.AllowDrop = true;
            this.MainLayout.SetColumnSpan(this.MiddleWheelsGroup, 2);
            this.MiddleWheelsGroup.Controls.Add(this.MiddleWheelsPanel);
            this.MiddleWheelsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MiddleWheelsGroup.Location = new System.Drawing.Point(4, 786);
            this.MiddleWheelsGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MiddleWheelsGroup.Name = "MiddleWheelsGroup";
            this.MiddleWheelsGroup.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MiddleWheelsGroup.Size = new System.Drawing.Size(605, 188);
            this.MiddleWheelsGroup.TabIndex = 7;
            this.MiddleWheelsGroup.TabStop = false;
            this.MiddleWheelsGroup.Text = "Middle Wheels (H-Drive only)";
            this.MiddleWheelsGroup.DragDrop += new System.Windows.Forms.DragEventHandler(this.MiddleWheelsPanel_DragDrop);
            this.MiddleWheelsGroup.DragEnter += new System.Windows.Forms.DragEventHandler(this.Field_DragEnter);
            // 
            // MiddleWheelsPanel
            // 
            this.MiddleWheelsPanel.AllowDrop = true;
            this.MiddleWheelsPanel.AutoScroll = true;
            this.MiddleWheelsPanel.ColumnCount = 2;
            this.MiddleWheelsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MiddleWheelsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.MiddleWheelsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MiddleWheelsPanel.Location = new System.Drawing.Point(4, 25);
            this.MiddleWheelsPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MiddleWheelsPanel.Name = "MiddleWheelsPanel";
            this.MiddleWheelsPanel.RowCount = 2;
            this.MiddleWheelsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 709F));
            this.MiddleWheelsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.MiddleWheelsPanel.Size = new System.Drawing.Size(597, 158);
            this.MiddleWheelsPanel.TabIndex = 1;
            this.MiddleWheelsPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.MiddleWheelsPanel_DragDrop);
            this.MiddleWheelsPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.Field_DragEnter);
            // 
            // RightBackWheelsGroup
            // 
            this.RightBackWheelsGroup.AllowDrop = true;
            this.RightBackWheelsGroup.Controls.Add(this.RightBackWheelsPanel);
            this.RightBackWheelsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RightBackWheelsGroup.Location = new System.Drawing.Point(310, 984);
            this.RightBackWheelsGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RightBackWheelsGroup.Name = "RightBackWheelsGroup";
            this.RightBackWheelsGroup.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RightBackWheelsGroup.Size = new System.Drawing.Size(299, 16);
            this.RightBackWheelsGroup.TabIndex = 7;
            this.RightBackWheelsGroup.TabStop = false;
            this.RightBackWheelsGroup.Text = "Right Back Wheels (Mecanum only)";
            this.RightBackWheelsGroup.DragDrop += new System.Windows.Forms.DragEventHandler(this.RightBackWheelsPanel_DragDrop);
            this.RightBackWheelsGroup.DragEnter += new System.Windows.Forms.DragEventHandler(this.Field_DragEnter);
            // 
            // RightBackWheelsPanel
            // 
            this.RightBackWheelsPanel.AllowDrop = true;
            this.RightBackWheelsPanel.AutoScroll = true;
            this.RightBackWheelsPanel.ColumnCount = 1;
            this.RightBackWheelsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.RightBackWheelsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.RightBackWheelsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.RightBackWheelsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.RightBackWheelsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RightBackWheelsPanel.Location = new System.Drawing.Point(4, 25);
            this.RightBackWheelsPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.RightBackWheelsPanel.Name = "RightBackWheelsPanel";
            this.RightBackWheelsPanel.RowCount = 2;
            this.RightBackWheelsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 709F));
            this.RightBackWheelsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 709F));
            this.RightBackWheelsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.RightBackWheelsPanel.Size = new System.Drawing.Size(291, 0);
            this.RightBackWheelsPanel.TabIndex = 1;
            this.RightBackWheelsPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.RightBackWheelsPanel_DragDrop);
            this.RightBackWheelsPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.Field_DragEnter);
            // 
            // LeftBackWheelsGroup
            // 
            this.LeftBackWheelsGroup.AllowDrop = true;
            this.LeftBackWheelsGroup.Controls.Add(this.LeftBackWheelsPanel);
            this.LeftBackWheelsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LeftBackWheelsGroup.Location = new System.Drawing.Point(4, 984);
            this.LeftBackWheelsGroup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.LeftBackWheelsGroup.Name = "LeftBackWheelsGroup";
            this.LeftBackWheelsGroup.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.LeftBackWheelsGroup.Size = new System.Drawing.Size(298, 16);
            this.LeftBackWheelsGroup.TabIndex = 7;
            this.LeftBackWheelsGroup.TabStop = false;
            this.LeftBackWheelsGroup.Text = "Left Back Wheels (Mecanum only)";
            this.LeftBackWheelsGroup.DragDrop += new System.Windows.Forms.DragEventHandler(this.LeftBackWheelsPanel_DragDrop);
            this.LeftBackWheelsGroup.DragEnter += new System.Windows.Forms.DragEventHandler(this.Field_DragEnter);
            // 
            // LeftBackWheelsPanel
            // 
            this.LeftBackWheelsPanel.AllowDrop = true;
            this.LeftBackWheelsPanel.AutoScroll = true;
            this.LeftBackWheelsPanel.ColumnCount = 1;
            this.LeftBackWheelsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LeftBackWheelsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.LeftBackWheelsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LeftBackWheelsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.LeftBackWheelsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LeftBackWheelsPanel.Location = new System.Drawing.Point(4, 25);
            this.LeftBackWheelsPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.LeftBackWheelsPanel.Name = "LeftBackWheelsPanel";
            this.LeftBackWheelsPanel.RowCount = 2;
            this.LeftBackWheelsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 709F));
            this.LeftBackWheelsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 709F));
            this.LeftBackWheelsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.LeftBackWheelsPanel.Size = new System.Drawing.Size(290, 0);
            this.LeftBackWheelsPanel.TabIndex = 1;
            this.LeftBackWheelsPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.LeftBackWheelsPanel_DragDrop);
            this.LeftBackWheelsPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.Field_DragEnter);
            // 
            // DefineWheelsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.MainLayout);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "DefineWheelsPage";
            this.Size = new System.Drawing.Size(613, 1005);
            this.WheelNodeGroupBox.ResumeLayout(false);
            this.WheelNodeGroupBox.PerformLayout();
            this.WheelJointsLayout.ResumeLayout(false);
            this.WheelJointsLayout.PerformLayout();
            this.LeftWheelsGroup.ResumeLayout(false);
            this.RobotInfoGroupBox.ResumeLayout(false);
            this.RobotInfoGroupBox.PerformLayout();
            this.RobotInfoLayout.ResumeLayout(false);
            this.RobotInfoLayout.PerformLayout();
            this.DriveTrainLayout.ResumeLayout(false);
            this.DriveTrainLayout.PerformLayout();
            this.WeightLayout.ResumeLayout(false);
            this.WeightLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize) (this.WeightBox)).EndInit();
            this.RightWheelsGroup.ResumeLayout(false);
            this.MainLayout.ResumeLayout(false);
            this.MainLayout.PerformLayout();
            this.MiddleWheelsGroup.ResumeLayout(false);
            this.RightBackWheelsGroup.ResumeLayout(false);
            this.LeftBackWheelsGroup.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label DefineWheelsInstruction;
        private System.Windows.Forms.GroupBox WheelNodeGroupBox;
        private System.Windows.Forms.GroupBox RobotInfoGroupBox;
        private System.Windows.Forms.ComboBox DriveTrainDropdown;
        private System.Windows.Forms.Label DriveTrainLabel;
        private System.Windows.Forms.Button AutoFillButton;
        private System.Windows.Forms.ListBox NodeListBox;
        private System.Windows.Forms.NumericUpDown WeightBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox LeftWheelsGroup;
        private System.Windows.Forms.GroupBox RightWheelsGroup;
        private System.Windows.Forms.ComboBox WeightUnitSelector;
        private System.Windows.Forms.TableLayoutPanel MainLayout;
        private System.Windows.Forms.TableLayoutPanel WheelJointsLayout;
        private System.Windows.Forms.TableLayoutPanel RobotInfoLayout;
        private System.Windows.Forms.TableLayoutPanel DriveTrainLayout;
        private System.Windows.Forms.TableLayoutPanel WeightLayout;
        private System.Windows.Forms.TableLayoutPanel LeftWheelsPanel;
        private System.Windows.Forms.TableLayoutPanel RightWheelsPanel;
        private System.Windows.Forms.ToolTip AutoFillToolTip;
        private System.Windows.Forms.GroupBox MiddleWheelsGroup;
        private System.Windows.Forms.TableLayoutPanel MiddleWheelsPanel;
        private System.Windows.Forms.GroupBox LeftBackWheelsGroup;
        private System.Windows.Forms.TableLayoutPanel LeftBackWheelsPanel;
        private System.Windows.Forms.GroupBox RightBackWheelsGroup;
        private System.Windows.Forms.TableLayoutPanel RightBackWheelsPanel;
        private System.Windows.Forms.Button RemoveWheelsButton;
    }
}