namespace BxDRobotExporter.Wizard
{
    partial class DefinePartPanel
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
            this.NodeGroupBox = new System.Windows.Forms.GroupBox();
            this.MainTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.DriverLayout = new System.Windows.Forms.TableLayoutPanel();
            this.SelectDriverLabel = new System.Windows.Forms.Label();
            this.DriverComboBox = new System.Windows.Forms.ComboBox();
            this.PortsGroupBox = new System.Windows.Forms.GroupBox();
            this.PortLayout = new System.Windows.Forms.TableLayoutPanel();
            this.rbCAN = new System.Windows.Forms.RadioButton();
            this.rbPWM = new System.Windows.Forms.RadioButton();
            this.PortOneLabel = new System.Windows.Forms.Label();
            this.PortTwoLabel = new System.Windows.Forms.Label();
            this.PortTwoUpDown = new System.Windows.Forms.NumericUpDown();
            this.PortOneUpDown = new System.Windows.Forms.NumericUpDown();
            this.metaBrake = new System.Windows.Forms.TabPage();
            this.BrakeLayout = new System.Windows.Forms.TableLayoutPanel();
            this.chkBoxHasBrake = new System.Windows.Forms.CheckBox();
            this.metaGearing = new System.Windows.Forms.TabPage();
            this.GearLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblInputGear = new System.Windows.Forms.Label();
            this.lblOutputGear = new System.Windows.Forms.Label();
            this.OutputGeartxt = new System.Windows.Forms.NumericUpDown();
            this.InputGeartxt = new System.Windows.Forms.NumericUpDown();
            this.metaPneumatic = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblDiameter = new System.Windows.Forms.Label();
            this.cmbPneumaticDiameter = new System.Windows.Forms.ComboBox();
            this.lblPressure = new System.Windows.Forms.Label();
            this.cmbPneumaticPressure = new System.Windows.Forms.ComboBox();
            this.tabsMeta = new System.Windows.Forms.TabControl();
            this.NodeGroupBox.SuspendLayout();
            this.MainTableLayout.SuspendLayout();
            this.DriverLayout.SuspendLayout();
            this.PortsGroupBox.SuspendLayout();
            this.PortLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PortTwoUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PortOneUpDown)).BeginInit();
            this.metaBrake.SuspendLayout();
            this.BrakeLayout.SuspendLayout();
            this.metaGearing.SuspendLayout();
            this.GearLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OutputGeartxt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.InputGeartxt)).BeginInit();
            this.metaPneumatic.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabsMeta.SuspendLayout();
            this.SuspendLayout();
            // 
            // NodeGroupBox
            // 
            this.NodeGroupBox.AutoSize = true;
            this.NodeGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.NodeGroupBox.Controls.Add(this.MainTableLayout);
            this.NodeGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.NodeGroupBox.Location = new System.Drawing.Point(0, 0);
            this.NodeGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.NodeGroupBox.Name = "NodeGroupBox";
            this.NodeGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.NodeGroupBox.Size = new System.Drawing.Size(533, 242);
            this.NodeGroupBox.TabIndex = 0;
            this.NodeGroupBox.TabStop = false;
            this.NodeGroupBox.Text = "Empty";
            // 
            // MainTableLayout
            // 
            this.MainTableLayout.AutoSize = true;
            this.MainTableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainTableLayout.ColumnCount = 2;
            this.MainTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.MainTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.MainTableLayout.Controls.Add(this.tabsMeta, 0, 2);
            this.MainTableLayout.Controls.Add(this.DriverLayout, 0, 0);
            this.MainTableLayout.Controls.Add(this.PortsGroupBox, 0, 1);
            this.MainTableLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.MainTableLayout.Location = new System.Drawing.Point(4, 19);
            this.MainTableLayout.Margin = new System.Windows.Forms.Padding(4);
            this.MainTableLayout.Name = "MainTableLayout";
            this.MainTableLayout.RowCount = 3;
            this.MainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainTableLayout.Size = new System.Drawing.Size(525, 219);
            this.MainTableLayout.TabIndex = 14;
            // 
            // DriverLayout
            // 
            this.DriverLayout.AutoSize = true;
            this.DriverLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DriverLayout.ColumnCount = 2;
            this.MainTableLayout.SetColumnSpan(this.DriverLayout, 2);
            this.DriverLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.DriverLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.DriverLayout.Controls.Add(this.SelectDriverLabel, 0, 0);
            this.DriverLayout.Controls.Add(this.DriverComboBox, 1, 0);
            this.DriverLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.DriverLayout.Location = new System.Drawing.Point(0, 0);
            this.DriverLayout.Margin = new System.Windows.Forms.Padding(0);
            this.DriverLayout.Name = "DriverLayout";
            this.DriverLayout.RowCount = 1;
            this.DriverLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.DriverLayout.Size = new System.Drawing.Size(525, 32);
            this.DriverLayout.TabIndex = 0;
            // 
            // SelectDriverLabel
            // 
            this.SelectDriverLabel.AutoSize = true;
            this.SelectDriverLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.SelectDriverLabel.Location = new System.Drawing.Point(4, 4);
            this.SelectDriverLabel.Margin = new System.Windows.Forms.Padding(4);
            this.SelectDriverLabel.Name = "SelectDriverLabel";
            this.SelectDriverLabel.Size = new System.Drawing.Size(80, 24);
            this.SelectDriverLabel.TabIndex = 1;
            this.SelectDriverLabel.Text = "Joint Driver";
            this.SelectDriverLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // DriverComboBox
            // 
            this.DriverComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.DriverComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DriverComboBox.FormattingEnabled = true;
            this.DriverComboBox.Items.AddRange(new object[] {
            "No Driver",
            "Motor",
            "Servo",
            "Bumper Pneumatic",
            "Relay Pneumatic",
            "Dual Motor"});
            this.DriverComboBox.Location = new System.Drawing.Point(92, 4);
            this.DriverComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.DriverComboBox.Name = "DriverComboBox";
            this.DriverComboBox.Size = new System.Drawing.Size(429, 24);
            this.DriverComboBox.TabIndex = 2;
            this.DriverComboBox.SelectedIndexChanged += new System.EventHandler(this.DriverComboBox_SelectedIndexChanged);
            // 
            // PortsGroupBox
            // 
            this.PortsGroupBox.AutoSize = true;
            this.PortsGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainTableLayout.SetColumnSpan(this.PortsGroupBox, 2);
            this.PortsGroupBox.Controls.Add(this.PortLayout);
            this.PortsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.PortsGroupBox.Location = new System.Drawing.Point(4, 36);
            this.PortsGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.PortsGroupBox.Name = "PortsGroupBox";
            this.PortsGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.PortsGroupBox.Size = new System.Drawing.Size(517, 83);
            this.PortsGroupBox.TabIndex = 6;
            this.PortsGroupBox.TabStop = false;
            this.PortsGroupBox.Text = "Ports";
            // 
            // PortLayout
            // 
            this.PortLayout.AutoSize = true;
            this.PortLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PortLayout.ColumnCount = 4;
            this.PortLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.PortLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.PortLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.PortLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.PortLayout.Controls.Add(this.rbCAN, 1, 1);
            this.PortLayout.Controls.Add(this.rbPWM, 0, 1);
            this.PortLayout.Controls.Add(this.PortOneLabel, 0, 0);
            this.PortLayout.Controls.Add(this.PortTwoLabel, 2, 0);
            this.PortLayout.Controls.Add(this.PortTwoUpDown, 3, 0);
            this.PortLayout.Controls.Add(this.PortOneUpDown, 1, 0);
            this.PortLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.PortLayout.Location = new System.Drawing.Point(4, 19);
            this.PortLayout.Margin = new System.Windows.Forms.Padding(4);
            this.PortLayout.Name = "PortLayout";
            this.PortLayout.RowCount = 2;
            this.PortLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.PortLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.PortLayout.Size = new System.Drawing.Size(509, 60);
            this.PortLayout.TabIndex = 4;
            // 
            // rbCAN
            // 
            this.rbCAN.AutoSize = true;
            this.rbCAN.Location = new System.Drawing.Point(71, 32);
            this.rbCAN.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rbCAN.Name = "rbCAN";
            this.rbCAN.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.rbCAN.Size = new System.Drawing.Size(60, 21);
            this.rbCAN.TabIndex = 9;
            this.rbCAN.TabStop = true;
            this.rbCAN.Text = "CAN";
            this.rbCAN.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.rbCAN.UseVisualStyleBackColor = true;
            // 
            // rbPWM
            // 
            this.rbPWM.AutoSize = true;
            this.rbPWM.Dock = System.Windows.Forms.DockStyle.Right;
            this.rbPWM.Location = new System.Drawing.Point(3, 32);
            this.rbPWM.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rbPWM.Name = "rbPWM";
            this.rbPWM.Size = new System.Drawing.Size(62, 26);
            this.rbPWM.TabIndex = 8;
            this.rbPWM.TabStop = true;
            this.rbPWM.Text = "PWM";
            this.rbPWM.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.rbPWM.UseVisualStyleBackColor = true;
            // 
            // PortOneLabel
            // 
            this.PortOneLabel.AutoSize = true;
            this.PortOneLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PortOneLabel.Location = new System.Drawing.Point(4, 4);
            this.PortOneLabel.Margin = new System.Windows.Forms.Padding(4);
            this.PortOneLabel.Name = "PortOneLabel";
            this.PortOneLabel.Size = new System.Drawing.Size(60, 22);
            this.PortOneLabel.TabIndex = 1;
            this.PortOneLabel.Text = "Port 1";
            this.PortOneLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PortTwoLabel
            // 
            this.PortTwoLabel.AutoSize = true;
            this.PortTwoLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PortTwoLabel.Location = new System.Drawing.Point(265, 4);
            this.PortTwoLabel.Margin = new System.Windows.Forms.Padding(4);
            this.PortTwoLabel.Name = "PortTwoLabel";
            this.PortTwoLabel.Size = new System.Drawing.Size(46, 22);
            this.PortTwoLabel.TabIndex = 2;
            this.PortTwoLabel.Text = "Port 2";
            this.PortTwoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PortTwoUpDown
            // 
            this.PortTwoUpDown.Dock = System.Windows.Forms.DockStyle.Top;
            this.PortTwoUpDown.Location = new System.Drawing.Point(319, 4);
            this.PortTwoUpDown.Margin = new System.Windows.Forms.Padding(4);
            this.PortTwoUpDown.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.PortTwoUpDown.Name = "PortTwoUpDown";
            this.PortTwoUpDown.Size = new System.Drawing.Size(186, 22);
            this.PortTwoUpDown.TabIndex = 3;
            this.PortTwoUpDown.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // PortOneUpDown
            // 
            this.PortOneUpDown.Dock = System.Windows.Forms.DockStyle.Top;
            this.PortOneUpDown.Location = new System.Drawing.Point(72, 4);
            this.PortOneUpDown.Margin = new System.Windows.Forms.Padding(4);
            this.PortOneUpDown.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.PortOneUpDown.Name = "PortOneUpDown";
            this.PortOneUpDown.Size = new System.Drawing.Size(185, 22);
            this.PortOneUpDown.TabIndex = 0;
            this.PortOneUpDown.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // metaElevatorBrake
            // 
            this.metaBrake.Controls.Add(this.BrakeLayout);
            this.metaBrake.Location = new System.Drawing.Point(4, 25);
            this.metaBrake.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.metaBrake.Name = "metaElevatorBrake";
            this.metaBrake.Size = new System.Drawing.Size(514, 63);
            this.metaBrake.TabIndex = 3;
            this.metaBrake.Text = "Brake";
            this.metaBrake.UseVisualStyleBackColor = true;
            // 
            // BrakeLayout
            // 
            this.BrakeLayout.AutoSize = true;
            this.BrakeLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BrakeLayout.ColumnCount = 2;
            this.BrakeLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.BrakeLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.BrakeLayout.Controls.Add(this.chkBoxHasBrake, 0, 0);
            this.BrakeLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.BrakeLayout.Location = new System.Drawing.Point(0, 0);
            this.BrakeLayout.Margin = new System.Windows.Forms.Padding(4);
            this.BrakeLayout.Name = "BrakeLayout";
            this.BrakeLayout.RowCount = 2;
            this.BrakeLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.BrakeLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.BrakeLayout.Size = new System.Drawing.Size(514, 55);
            this.BrakeLayout.TabIndex = 13;
            // 
            // chkBoxHasBrake
            // 
            this.chkBoxHasBrake.AutoSize = true;
            this.chkBoxHasBrake.Location = new System.Drawing.Point(261, 4);
            this.chkBoxHasBrake.Margin = new System.Windows.Forms.Padding(4);
            this.chkBoxHasBrake.Name = "chkBoxHasBrake";
            this.chkBoxHasBrake.Size = new System.Drawing.Size(96, 21);
            this.chkBoxHasBrake.TabIndex = 0;
            this.chkBoxHasBrake.Text = "Has Brake";
            this.chkBoxHasBrake.UseVisualStyleBackColor = true;
            // 
            // metaGearing
            // 
            this.metaGearing.Controls.Add(this.GearLayout);
            this.metaGearing.Location = new System.Drawing.Point(4, 25);
            this.metaGearing.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.metaGearing.Name = "metaGearing";
            this.metaGearing.Size = new System.Drawing.Size(514, 63);
            this.metaGearing.TabIndex = 2;
            this.metaGearing.Text = "Gear Ratio";
            this.metaGearing.UseVisualStyleBackColor = true;
            // 
            // GearLayout
            // 
            this.GearLayout.AutoSize = true;
            this.GearLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.GearLayout.ColumnCount = 2;
            this.GearLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.GearLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.GearLayout.Controls.Add(this.InputGeartxt, 0, 1);
            this.GearLayout.Controls.Add(this.OutputGeartxt, 0, 1);
            this.GearLayout.Controls.Add(this.lblOutputGear, 1, 0);
            this.GearLayout.Controls.Add(this.lblInputGear, 0, 0);
            this.GearLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.GearLayout.Location = new System.Drawing.Point(0, 0);
            this.GearLayout.Margin = new System.Windows.Forms.Padding(4);
            this.GearLayout.Name = "GearLayout";
            this.GearLayout.RowCount = 2;
            this.GearLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.GearLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.GearLayout.Size = new System.Drawing.Size(514, 56);
            this.GearLayout.TabIndex = 13;
            // 
            // lblInputGear
            // 
            this.lblInputGear.AutoSize = true;
            this.lblInputGear.Location = new System.Drawing.Point(4, 4);
            this.lblInputGear.Margin = new System.Windows.Forms.Padding(4);
            this.lblInputGear.Name = "lblInputGear";
            this.lblInputGear.Size = new System.Drawing.Size(75, 17);
            this.lblInputGear.TabIndex = 11;
            this.lblInputGear.Text = "Input Gear";
            // 
            // lblOutputGear
            // 
            this.lblOutputGear.AutoSize = true;
            this.lblOutputGear.Location = new System.Drawing.Point(261, 4);
            this.lblOutputGear.Margin = new System.Windows.Forms.Padding(4);
            this.lblOutputGear.Name = "lblOutputGear";
            this.lblOutputGear.Size = new System.Drawing.Size(78, 20);
            this.lblOutputGear.TabIndex = 14;
            this.lblOutputGear.Text = "Output Gear";
            this.lblOutputGear.UseCompatibleTextRendering = true;
            // 
            // OutputGeartxt
            // 
            this.OutputGeartxt.DecimalPlaces = 5;
            this.OutputGeartxt.Location = new System.Drawing.Point(260, 31);
            this.OutputGeartxt.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.OutputGeartxt.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.OutputGeartxt.Name = "OutputGeartxt";
            this.OutputGeartxt.Size = new System.Drawing.Size(251, 22);
            this.OutputGeartxt.TabIndex = 16;
            this.OutputGeartxt.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // InputGeartxt
            // 
            this.InputGeartxt.DecimalPlaces = 5;
            this.InputGeartxt.Location = new System.Drawing.Point(3, 31);
            this.InputGeartxt.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.InputGeartxt.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.InputGeartxt.Name = "InputGeartxt";
            this.InputGeartxt.Size = new System.Drawing.Size(251, 22);
            this.InputGeartxt.TabIndex = 17;
            this.InputGeartxt.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // metaPneumatic
            // 
            this.metaPneumatic.Controls.Add(this.tableLayoutPanel1);
            this.metaPneumatic.Location = new System.Drawing.Point(4, 25);
            this.metaPneumatic.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.metaPneumatic.Name = "metaPneumatic";
            this.metaPneumatic.Size = new System.Drawing.Size(514, 63);
            this.metaPneumatic.TabIndex = 1;
            this.metaPneumatic.Text = "Pneumatic";
            this.metaPneumatic.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.cmbPneumaticPressure, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblPressure, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.cmbPneumaticDiameter, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblDiameter, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(514, 53);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // lblDiameter
            // 
            this.lblDiameter.AutoSize = true;
            this.lblDiameter.Location = new System.Drawing.Point(4, 4);
            this.lblDiameter.Margin = new System.Windows.Forms.Padding(4);
            this.lblDiameter.Name = "lblDiameter";
            this.lblDiameter.Size = new System.Drawing.Size(116, 17);
            this.lblDiameter.TabIndex = 9;
            this.lblDiameter.Text = "Internal Diameter";
            this.lblDiameter.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // cmbPneumaticDiameter
            // 
            this.cmbPneumaticDiameter.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmbPneumaticDiameter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPneumaticDiameter.FormattingEnabled = true;
            this.cmbPneumaticDiameter.Items.AddRange(new object[] {
            "1 in",
            ".5 in",
            ".25 in"});
            this.cmbPneumaticDiameter.Location = new System.Drawing.Point(3, 27);
            this.cmbPneumaticDiameter.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbPneumaticDiameter.Name = "cmbPneumaticDiameter";
            this.cmbPneumaticDiameter.Size = new System.Drawing.Size(251, 24);
            this.cmbPneumaticDiameter.TabIndex = 12;
            // 
            // lblPressure
            // 
            this.lblPressure.AutoSize = true;
            this.lblPressure.Location = new System.Drawing.Point(261, 4);
            this.lblPressure.Margin = new System.Windows.Forms.Padding(4);
            this.lblPressure.Name = "lblPressure";
            this.lblPressure.Size = new System.Drawing.Size(65, 17);
            this.lblPressure.TabIndex = 13;
            this.lblPressure.Text = "Pressure";
            this.lblPressure.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // cmbPneumaticPressure
            // 
            this.cmbPneumaticPressure.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmbPneumaticPressure.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPneumaticPressure.FormattingEnabled = true;
            this.cmbPneumaticPressure.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.cmbPneumaticPressure.Items.AddRange(new object[] {
            "60 psi",
            "20 psi",
            "10 psi"});
            this.cmbPneumaticPressure.Location = new System.Drawing.Point(260, 27);
            this.cmbPneumaticPressure.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbPneumaticPressure.Name = "cmbPneumaticPressure";
            this.cmbPneumaticPressure.Size = new System.Drawing.Size(251, 24);
            this.cmbPneumaticPressure.TabIndex = 6;
            // 
            // tabsMeta
            // 
            this.MainTableLayout.SetColumnSpan(this.tabsMeta, 2);
            this.tabsMeta.Controls.Add(this.metaPneumatic);
            this.tabsMeta.Controls.Add(this.metaGearing);
            this.tabsMeta.Controls.Add(this.metaBrake);
            this.tabsMeta.Location = new System.Drawing.Point(3, 125);
            this.tabsMeta.Margin = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this.tabsMeta.Name = "tabsMeta";
            this.tabsMeta.SelectedIndex = 0;
            this.tabsMeta.Size = new System.Drawing.Size(522, 92);
            this.tabsMeta.TabIndex = 13;
            // 
            // DefinePartPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.NodeGroupBox);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(533, 0);
            this.Name = "DefinePartPanel";
            this.Size = new System.Drawing.Size(533, 242);
            this.NodeGroupBox.ResumeLayout(false);
            this.NodeGroupBox.PerformLayout();
            this.MainTableLayout.ResumeLayout(false);
            this.MainTableLayout.PerformLayout();
            this.DriverLayout.ResumeLayout(false);
            this.DriverLayout.PerformLayout();
            this.PortsGroupBox.ResumeLayout(false);
            this.PortsGroupBox.PerformLayout();
            this.PortLayout.ResumeLayout(false);
            this.PortLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PortTwoUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PortOneUpDown)).EndInit();
            this.metaBrake.ResumeLayout(false);
            this.metaBrake.PerformLayout();
            this.BrakeLayout.ResumeLayout(false);
            this.BrakeLayout.PerformLayout();
            this.metaGearing.ResumeLayout(false);
            this.metaGearing.PerformLayout();
            this.GearLayout.ResumeLayout(false);
            this.GearLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OutputGeartxt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.InputGeartxt)).EndInit();
            this.metaPneumatic.ResumeLayout(false);
            this.metaPneumatic.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tabsMeta.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox NodeGroupBox;
        private System.Windows.Forms.Label SelectDriverLabel;
        private System.Windows.Forms.ComboBox DriverComboBox;
        private System.Windows.Forms.GroupBox PortsGroupBox;
        private System.Windows.Forms.Label PortTwoLabel;
        private System.Windows.Forms.Label PortOneLabel;
        private System.Windows.Forms.NumericUpDown PortOneUpDown;
        private System.Windows.Forms.NumericUpDown PortTwoUpDown;
        private System.Windows.Forms.TableLayoutPanel MainTableLayout;
        private System.Windows.Forms.TableLayoutPanel DriverLayout;
        private System.Windows.Forms.TableLayoutPanel PortLayout;
        private System.Windows.Forms.TabPage metaBrake;
        private System.Windows.Forms.TableLayoutPanel BrakeLayout;
        private System.Windows.Forms.CheckBox chkBoxHasBrake;
        private System.Windows.Forms.RadioButton rbPWM;
        private System.Windows.Forms.RadioButton rbCAN;
        private System.Windows.Forms.TabControl tabsMeta;
        private System.Windows.Forms.TabPage metaPneumatic;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox cmbPneumaticPressure;
        private System.Windows.Forms.Label lblPressure;
        private System.Windows.Forms.ComboBox cmbPneumaticDiameter;
        private System.Windows.Forms.Label lblDiameter;
        private System.Windows.Forms.TabPage metaGearing;
        private System.Windows.Forms.TableLayoutPanel GearLayout;
        private System.Windows.Forms.NumericUpDown InputGeartxt;
        private System.Windows.Forms.NumericUpDown OutputGeartxt;
        private System.Windows.Forms.Label lblOutputGear;
        private System.Windows.Forms.Label lblInputGear;
    }
}
