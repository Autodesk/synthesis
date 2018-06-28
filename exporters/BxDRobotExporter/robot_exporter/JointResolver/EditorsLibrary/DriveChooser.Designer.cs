
partial class DriveChooser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DriveChooser));
            this.cmbJointDriver = new System.Windows.Forms.ComboBox();
            this.grpChooseDriver = new System.Windows.Forms.GroupBox();
            this.grpDriveOptions = new System.Windows.Forms.GroupBox();
            this.JointOptionsLayout = new System.Windows.Forms.TableLayoutPanel();
            this.txtHighLimit = new System.Windows.Forms.NumericUpDown();
            this.lblPort = new System.Windows.Forms.Label();
            this.txtLowLimit = new System.Windows.Forms.NumericUpDown();
            this.txtPortA = new System.Windows.Forms.NumericUpDown();
            this.txtPortB = new System.Windows.Forms.NumericUpDown();
            this.lblLimits = new System.Windows.Forms.Label();
            this.rbCAN = new System.Windows.Forms.RadioButton();
            this.rbPWM = new System.Windows.Forms.RadioButton();
            this.chkBoxDriveWheel = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.cmbWheelType = new System.Windows.Forms.ComboBox();
            this.cmbPneumaticPressure = new System.Windows.Forms.ComboBox();
            this.tabsMeta = new System.Windows.Forms.TabControl();
            this.metaWheel = new System.Windows.Forms.TabPage();
            this.WheelLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblFriction = new System.Windows.Forms.Label();
            this.cmbFrictionLevel = new System.Windows.Forms.ComboBox();
            this.lblType = new System.Windows.Forms.Label();
            this.metaPneumatic = new System.Windows.Forms.TabPage();
            this.PneumaticLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblPressure = new System.Windows.Forms.Label();
            this.cmbPneumaticDiameter = new System.Windows.Forms.ComboBox();
            this.lblDiameter = new System.Windows.Forms.Label();
            this.metaGearing = new System.Windows.Forms.TabPage();
            this.lblOutputGear = new System.Windows.Forms.Label();
            this.lblInputGear = new System.Windows.Forms.Label();
            this.txtGearRationNum = new System.Windows.Forms.TextBox();
            this.txtGearRationDenom = new System.Windows.Forms.TextBox();
            this.metaElevatorBrake = new System.Windows.Forms.TabPage();
            this.brakePortB = new System.Windows.Forms.NumericUpDown();
            this.brakePortA = new System.Windows.Forms.NumericUpDown();
            this.lblBrakePort = new System.Windows.Forms.Label();
            this.chkBoxHasBrake = new System.Windows.Forms.CheckBox();
            this.metaElevatorStages = new System.Windows.Forms.TabPage();
            this.cmbStages = new System.Windows.Forms.ComboBox();
            this.ConfigJointLayout = new System.Windows.Forms.TableLayoutPanel();
            this.GearLayout = new System.Windows.Forms.TableLayoutPanel();
            this.BreakLayout = new System.Windows.Forms.TableLayoutPanel();
            this.grpChooseDriver.SuspendLayout();
            this.grpDriveOptions.SuspendLayout();
            this.JointOptionsLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtHighLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLowLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPortA)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPortB)).BeginInit();
            this.tabsMeta.SuspendLayout();
            this.metaWheel.SuspendLayout();
            this.WheelLayout.SuspendLayout();
            this.metaPneumatic.SuspendLayout();
            this.PneumaticLayout.SuspendLayout();
            this.metaGearing.SuspendLayout();
            this.metaElevatorBrake.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.brakePortB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.brakePortA)).BeginInit();
            this.metaElevatorStages.SuspendLayout();
            this.ConfigJointLayout.SuspendLayout();
            this.GearLayout.SuspendLayout();
            this.BreakLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmbJointDriver
            // 
            this.cmbJointDriver.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbJointDriver.FormattingEnabled = true;
            this.cmbJointDriver.Location = new System.Drawing.Point(4, 17);
            this.cmbJointDriver.Margin = new System.Windows.Forms.Padding(2);
            this.cmbJointDriver.Name = "cmbJointDriver";
            this.cmbJointDriver.Size = new System.Drawing.Size(316, 21);
            this.cmbJointDriver.TabIndex = 0;
            this.cmbJointDriver.SelectedIndexChanged += new System.EventHandler(this.cmbJointDriver_SelectedIndexChanged);
            // 
            // grpChooseDriver
            // 
            this.grpChooseDriver.Controls.Add(this.cmbJointDriver);
            this.grpChooseDriver.Location = new System.Drawing.Point(2, 2);
            this.grpChooseDriver.Margin = new System.Windows.Forms.Padding(2);
            this.grpChooseDriver.Name = "grpChooseDriver";
            this.grpChooseDriver.Padding = new System.Windows.Forms.Padding(2);
            this.grpChooseDriver.Size = new System.Drawing.Size(324, 44);
            this.grpChooseDriver.TabIndex = 1;
            this.grpChooseDriver.TabStop = false;
            this.grpChooseDriver.Text = "Joint Driver";
            this.grpChooseDriver.Enter += new System.EventHandler(this.grpChooseDriver_Enter);
            // 
            // grpDriveOptions
            // 
            this.grpDriveOptions.AutoSize = true;
            this.grpDriveOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpDriveOptions.Controls.Add(this.JointOptionsLayout);
            this.grpDriveOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpDriveOptions.Location = new System.Drawing.Point(2, 129);
            this.grpDriveOptions.Margin = new System.Windows.Forms.Padding(2);
            this.grpDriveOptions.Name = "grpDriveOptions";
            this.grpDriveOptions.Padding = new System.Windows.Forms.Padding(2);
            this.grpDriveOptions.Size = new System.Drawing.Size(324, 103);
            this.grpDriveOptions.TabIndex = 2;
            this.grpDriveOptions.TabStop = false;
            this.grpDriveOptions.Text = "Joint Options";
            // 
            // JointOptionsLayout
            // 
            this.JointOptionsLayout.AutoSize = true;
            this.JointOptionsLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.JointOptionsLayout.ColumnCount = 4;
            this.JointOptionsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.JointOptionsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.JointOptionsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.JointOptionsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.JointOptionsLayout.Controls.Add(this.txtHighLimit, 1, 3);
            this.JointOptionsLayout.Controls.Add(this.lblPort, 0, 0);
            this.JointOptionsLayout.Controls.Add(this.txtLowLimit, 0, 3);
            this.JointOptionsLayout.Controls.Add(this.txtPortA, 0, 1);
            this.JointOptionsLayout.Controls.Add(this.txtPortB, 1, 1);
            this.JointOptionsLayout.Controls.Add(this.lblLimits, 0, 2);
            this.JointOptionsLayout.Controls.Add(this.rbCAN, 2, 1);
            this.JointOptionsLayout.Controls.Add(this.rbPWM, 3, 1);
            this.JointOptionsLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.JointOptionsLayout.Location = new System.Drawing.Point(2, 15);
            this.JointOptionsLayout.Name = "JointOptionsLayout";
            this.JointOptionsLayout.RowCount = 4;
            this.JointOptionsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.JointOptionsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.JointOptionsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.JointOptionsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.JointOptionsLayout.Size = new System.Drawing.Size(320, 86);
            this.JointOptionsLayout.TabIndex = 0;
            // 
            // txtHighLimit
            // 
            this.txtHighLimit.DecimalPlaces = 4;
            this.txtHighLimit.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtHighLimit.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.txtHighLimit.Location = new System.Drawing.Point(107, 64);
            this.txtHighLimit.Margin = new System.Windows.Forms.Padding(2);
            this.txtHighLimit.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.txtHighLimit.Name = "txtHighLimit";
            this.txtHighLimit.Size = new System.Drawing.Size(101, 20);
            this.txtHighLimit.TabIndex = 4;
            this.txtHighLimit.Visible = false;
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(3, 3);
            this.lblPort.Margin = new System.Windows.Forms.Padding(3);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(40, 13);
            this.lblPort.TabIndex = 1;
            this.lblPort.Text = "Port ID";
            // 
            // txtLowLimit
            // 
            this.txtLowLimit.DecimalPlaces = 4;
            this.txtLowLimit.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtLowLimit.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.txtLowLimit.Location = new System.Drawing.Point(2, 64);
            this.txtLowLimit.Margin = new System.Windows.Forms.Padding(2);
            this.txtLowLimit.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.txtLowLimit.Name = "txtLowLimit";
            this.txtLowLimit.Size = new System.Drawing.Size(101, 20);
            this.txtLowLimit.TabIndex = 3;
            this.txtLowLimit.Visible = false;
            // 
            // txtPortA
            // 
            this.txtPortA.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtPortA.Location = new System.Drawing.Point(2, 21);
            this.txtPortA.Margin = new System.Windows.Forms.Padding(2);
            this.txtPortA.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtPortA.Name = "txtPortA";
            this.txtPortA.Size = new System.Drawing.Size(101, 20);
            this.txtPortA.TabIndex = 1;
            this.txtPortA.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // txtPortB
            // 
            this.txtPortB.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtPortB.Location = new System.Drawing.Point(107, 21);
            this.txtPortB.Margin = new System.Windows.Forms.Padding(2);
            this.txtPortB.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtPortB.Name = "txtPortB";
            this.txtPortB.Size = new System.Drawing.Size(101, 20);
            this.txtPortB.TabIndex = 2;
            this.txtPortB.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblLimits
            // 
            this.lblLimits.AutoSize = true;
            this.lblLimits.Location = new System.Drawing.Point(3, 46);
            this.lblLimits.Margin = new System.Windows.Forms.Padding(3);
            this.lblLimits.Name = "lblLimits";
            this.lblLimits.Size = new System.Drawing.Size(58, 13);
            this.lblLimits.TabIndex = 4;
            this.lblLimits.Text = "Joint Limits";
            this.lblLimits.Visible = false;
            // 
            // rbCAN
            // 
            this.rbCAN.AutoSize = true;
            this.rbCAN.Dock = System.Windows.Forms.DockStyle.Right;
            this.rbCAN.Location = new System.Drawing.Point(212, 21);
            this.rbCAN.Margin = new System.Windows.Forms.Padding(2);
            this.rbCAN.Name = "rbCAN";
            this.rbCAN.Padding = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.rbCAN.Size = new System.Drawing.Size(49, 20);
            this.rbCAN.TabIndex = 6;
            this.rbCAN.TabStop = true;
            this.rbCAN.Text = "CAN";
            this.rbCAN.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.rbCAN.UseVisualStyleBackColor = true;
            this.rbCAN.CheckedChanged += new System.EventHandler(this.rbCAN_CheckedChanged);
            // 
            // rbPWM
            // 
            this.rbPWM.AutoSize = true;
            this.rbPWM.Dock = System.Windows.Forms.DockStyle.Right;
            this.rbPWM.Location = new System.Drawing.Point(266, 21);
            this.rbPWM.Margin = new System.Windows.Forms.Padding(2);
            this.rbPWM.Name = "rbPWM";
            this.rbPWM.Size = new System.Drawing.Size(52, 20);
            this.rbPWM.TabIndex = 7;
            this.rbPWM.TabStop = true;
            this.rbPWM.Text = "PWM";
            this.rbPWM.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.rbPWM.UseVisualStyleBackColor = true;
            // 
            // chkBoxDriveWheel
            // 
            this.chkBoxDriveWheel.AutoSize = true;
            this.chkBoxDriveWheel.Dock = System.Windows.Forms.DockStyle.Right;
            this.chkBoxDriveWheel.Location = new System.Drawing.Point(226, 21);
            this.chkBoxDriveWheel.Margin = new System.Windows.Forms.Padding(2);
            this.chkBoxDriveWheel.Name = "chkBoxDriveWheel";
            this.chkBoxDriveWheel.Size = new System.Drawing.Size(88, 21);
            this.chkBoxDriveWheel.TabIndex = 5;
            this.chkBoxDriveWheel.Text = "Drive Wheel ";
            this.chkBoxDriveWheel.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(2, 236);
            this.btnSave.Margin = new System.Windows.Forms.Padding(2);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(324, 22);
            this.btnSave.TabIndex = 11;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // cmbWheelType
            // 
            this.cmbWheelType.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmbWheelType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbWheelType.FormattingEnabled = true;
            this.cmbWheelType.Items.AddRange(new object[] {
            "Not a Wheel",
            "Normal",
            "Omni",
            "Mecanum"});
            this.cmbWheelType.Location = new System.Drawing.Point(2, 21);
            this.cmbWheelType.Margin = new System.Windows.Forms.Padding(2);
            this.cmbWheelType.Name = "cmbWheelType";
            this.cmbWheelType.Size = new System.Drawing.Size(101, 21);
            this.cmbWheelType.TabIndex = 7;
            this.cmbWheelType.SelectedIndexChanged += new System.EventHandler(this.cmbWheelType_SelectedIndexChanged);
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
            this.cmbPneumaticPressure.Location = new System.Drawing.Point(160, 21);
            this.cmbPneumaticPressure.Margin = new System.Windows.Forms.Padding(2);
            this.cmbPneumaticPressure.Name = "cmbPneumaticPressure";
            this.cmbPneumaticPressure.Size = new System.Drawing.Size(154, 21);
            this.cmbPneumaticPressure.TabIndex = 6;
            // 
            // tabsMeta
            // 
            this.tabsMeta.Controls.Add(this.metaWheel);
            this.tabsMeta.Controls.Add(this.metaPneumatic);
            this.tabsMeta.Controls.Add(this.metaGearing);
            this.tabsMeta.Controls.Add(this.metaElevatorBrake);
            this.tabsMeta.Controls.Add(this.metaElevatorStages);
            this.tabsMeta.Location = new System.Drawing.Point(2, 50);
            this.tabsMeta.Margin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.tabsMeta.Name = "tabsMeta";
            this.tabsMeta.SelectedIndex = 0;
            this.tabsMeta.Size = new System.Drawing.Size(324, 75);
            this.tabsMeta.TabIndex = 11;
            // 
            // metaWheel
            // 
            this.metaWheel.Controls.Add(this.WheelLayout);
            this.metaWheel.Location = new System.Drawing.Point(4, 22);
            this.metaWheel.Margin = new System.Windows.Forms.Padding(2);
            this.metaWheel.Name = "metaWheel";
            this.metaWheel.Size = new System.Drawing.Size(316, 49);
            this.metaWheel.TabIndex = 0;
            this.metaWheel.Text = "Wheel";
            this.metaWheel.UseVisualStyleBackColor = true;
            // 
            // WheelLayout
            // 
            this.WheelLayout.AutoSize = true;
            this.WheelLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.WheelLayout.ColumnCount = 3;
            this.WheelLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.WheelLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.WheelLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.WheelLayout.Controls.Add(this.chkBoxDriveWheel, 2, 1);
            this.WheelLayout.Controls.Add(this.lblFriction, 1, 0);
            this.WheelLayout.Controls.Add(this.cmbFrictionLevel, 1, 1);
            this.WheelLayout.Controls.Add(this.lblType, 0, 0);
            this.WheelLayout.Controls.Add(this.cmbWheelType, 0, 1);
            this.WheelLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.WheelLayout.Location = new System.Drawing.Point(0, 0);
            this.WheelLayout.Name = "WheelLayout";
            this.WheelLayout.RowCount = 2;
            this.WheelLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.WheelLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.WheelLayout.Size = new System.Drawing.Size(316, 44);
            this.WheelLayout.TabIndex = 13;
            // 
            // lblFriction
            // 
            this.lblFriction.AutoSize = true;
            this.lblFriction.Location = new System.Drawing.Point(108, 3);
            this.lblFriction.Margin = new System.Windows.Forms.Padding(3);
            this.lblFriction.Name = "lblFriction";
            this.lblFriction.Size = new System.Drawing.Size(70, 13);
            this.lblFriction.TabIndex = 15;
            this.lblFriction.Text = "Friction Level";
            // 
            // cmbFrictionLevel
            // 
            this.cmbFrictionLevel.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmbFrictionLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFrictionLevel.FormattingEnabled = true;
            this.cmbFrictionLevel.Items.AddRange(new object[] {
            "High",
            "Medium",
            "Low"});
            this.cmbFrictionLevel.Location = new System.Drawing.Point(107, 21);
            this.cmbFrictionLevel.Margin = new System.Windows.Forms.Padding(2);
            this.cmbFrictionLevel.Name = "cmbFrictionLevel";
            this.cmbFrictionLevel.Size = new System.Drawing.Size(101, 21);
            this.cmbFrictionLevel.TabIndex = 13;
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(3, 3);
            this.lblType.Margin = new System.Windows.Forms.Padding(3);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(65, 13);
            this.lblType.TabIndex = 14;
            this.lblType.Text = "Wheel Type";
            // 
            // metaPneumatic
            // 
            this.metaPneumatic.Controls.Add(this.PneumaticLayout);
            this.metaPneumatic.Location = new System.Drawing.Point(4, 22);
            this.metaPneumatic.Margin = new System.Windows.Forms.Padding(2);
            this.metaPneumatic.Name = "metaPneumatic";
            this.metaPneumatic.Size = new System.Drawing.Size(316, 49);
            this.metaPneumatic.TabIndex = 1;
            this.metaPneumatic.Text = "Pneumatic";
            this.metaPneumatic.UseVisualStyleBackColor = true;
            // 
            // PneumaticLayout
            // 
            this.PneumaticLayout.AutoSize = true;
            this.PneumaticLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PneumaticLayout.ColumnCount = 2;
            this.PneumaticLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.PneumaticLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.PneumaticLayout.Controls.Add(this.cmbPneumaticPressure, 1, 1);
            this.PneumaticLayout.Controls.Add(this.lblPressure, 1, 0);
            this.PneumaticLayout.Controls.Add(this.cmbPneumaticDiameter, 0, 1);
            this.PneumaticLayout.Controls.Add(this.lblDiameter, 0, 0);
            this.PneumaticLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.PneumaticLayout.Location = new System.Drawing.Point(0, 0);
            this.PneumaticLayout.Name = "PneumaticLayout";
            this.PneumaticLayout.RowCount = 2;
            this.PneumaticLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PneumaticLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PneumaticLayout.Size = new System.Drawing.Size(316, 44);
            this.PneumaticLayout.TabIndex = 13;
            // 
            // lblPressure
            // 
            this.lblPressure.AutoSize = true;
            this.lblPressure.Location = new System.Drawing.Point(161, 3);
            this.lblPressure.Margin = new System.Windows.Forms.Padding(3);
            this.lblPressure.Name = "lblPressure";
            this.lblPressure.Size = new System.Drawing.Size(48, 13);
            this.lblPressure.TabIndex = 13;
            this.lblPressure.Text = "Pressure";
            this.lblPressure.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
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
            this.cmbPneumaticDiameter.Location = new System.Drawing.Point(2, 21);
            this.cmbPneumaticDiameter.Margin = new System.Windows.Forms.Padding(2);
            this.cmbPneumaticDiameter.Name = "cmbPneumaticDiameter";
            this.cmbPneumaticDiameter.Size = new System.Drawing.Size(154, 21);
            this.cmbPneumaticDiameter.TabIndex = 12;
            this.cmbPneumaticDiameter.SelectedIndexChanged += new System.EventHandler(this.cmbPneumaticDiameter_SelectedIndexChanged);
            // 
            // lblDiameter
            // 
            this.lblDiameter.AutoSize = true;
            this.lblDiameter.Location = new System.Drawing.Point(3, 3);
            this.lblDiameter.Margin = new System.Windows.Forms.Padding(3);
            this.lblDiameter.Name = "lblDiameter";
            this.lblDiameter.Size = new System.Drawing.Size(87, 13);
            this.lblDiameter.TabIndex = 9;
            this.lblDiameter.Text = "Internal Diameter";
            this.lblDiameter.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // metaGearing
            // 
            this.metaGearing.Controls.Add(this.GearLayout);
            this.metaGearing.Location = new System.Drawing.Point(4, 22);
            this.metaGearing.Margin = new System.Windows.Forms.Padding(2);
            this.metaGearing.Name = "metaGearing";
            this.metaGearing.Size = new System.Drawing.Size(316, 49);
            this.metaGearing.TabIndex = 2;
            this.metaGearing.Text = "Gear Ratio";
            this.metaGearing.UseVisualStyleBackColor = true;
            // 
            // lblOutputGear
            // 
            this.lblOutputGear.AutoSize = true;
            this.lblOutputGear.Location = new System.Drawing.Point(161, 3);
            this.lblOutputGear.Margin = new System.Windows.Forms.Padding(3);
            this.lblOutputGear.Name = "lblOutputGear";
            this.lblOutputGear.Size = new System.Drawing.Size(66, 17);
            this.lblOutputGear.TabIndex = 14;
            this.lblOutputGear.Text = "Output Gear";
            this.lblOutputGear.UseCompatibleTextRendering = true;
            // 
            // lblInputGear
            // 
            this.lblInputGear.AutoSize = true;
            this.lblInputGear.Location = new System.Drawing.Point(3, 3);
            this.lblInputGear.Margin = new System.Windows.Forms.Padding(3);
            this.lblInputGear.Name = "lblInputGear";
            this.lblInputGear.Size = new System.Drawing.Size(57, 13);
            this.lblInputGear.TabIndex = 11;
            this.lblInputGear.Text = "Input Gear";
            // 
            // txtGearRationNum
            // 
            this.txtGearRationNum.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtGearRationNum.Location = new System.Drawing.Point(2, 25);
            this.txtGearRationNum.Margin = new System.Windows.Forms.Padding(2);
            this.txtGearRationNum.Name = "txtGearRationNum";
            this.txtGearRationNum.Size = new System.Drawing.Size(154, 20);
            this.txtGearRationNum.TabIndex = 12;
            this.txtGearRationNum.Text = "1";
            // 
            // txtGearRationDenom
            // 
            this.txtGearRationDenom.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtGearRationDenom.Location = new System.Drawing.Point(160, 25);
            this.txtGearRationDenom.Margin = new System.Windows.Forms.Padding(2);
            this.txtGearRationDenom.Name = "txtGearRationDenom";
            this.txtGearRationDenom.Size = new System.Drawing.Size(154, 20);
            this.txtGearRationDenom.TabIndex = 13;
            this.txtGearRationDenom.Text = "1";
            // 
            // metaElevatorBrake
            // 
            this.metaElevatorBrake.Controls.Add(this.BreakLayout);
            this.metaElevatorBrake.Location = new System.Drawing.Point(4, 22);
            this.metaElevatorBrake.Margin = new System.Windows.Forms.Padding(2);
            this.metaElevatorBrake.Name = "metaElevatorBrake";
            this.metaElevatorBrake.Size = new System.Drawing.Size(316, 49);
            this.metaElevatorBrake.TabIndex = 3;
            this.metaElevatorBrake.Text = "Break Info";
            this.metaElevatorBrake.UseVisualStyleBackColor = true;
            // 
            // brakePortB
            // 
            this.brakePortB.Dock = System.Windows.Forms.DockStyle.Top;
            this.brakePortB.Location = new System.Drawing.Point(160, 25);
            this.brakePortB.Margin = new System.Windows.Forms.Padding(2);
            this.brakePortB.Name = "brakePortB";
            this.brakePortB.Size = new System.Drawing.Size(154, 20);
            this.brakePortB.TabIndex = 3;
            // 
            // brakePortA
            // 
            this.brakePortA.Dock = System.Windows.Forms.DockStyle.Top;
            this.brakePortA.Location = new System.Drawing.Point(2, 25);
            this.brakePortA.Margin = new System.Windows.Forms.Padding(2);
            this.brakePortA.Name = "brakePortA";
            this.brakePortA.Size = new System.Drawing.Size(154, 20);
            this.brakePortA.TabIndex = 2;
            this.brakePortA.ValueChanged += new System.EventHandler(this.brakePortA_ValueChanged);
            // 
            // lblBrakePort
            // 
            this.lblBrakePort.AutoSize = true;
            this.lblBrakePort.Location = new System.Drawing.Point(3, 3);
            this.lblBrakePort.Margin = new System.Windows.Forms.Padding(3);
            this.lblBrakePort.Name = "lblBrakePort";
            this.lblBrakePort.Size = new System.Drawing.Size(57, 13);
            this.lblBrakePort.TabIndex = 1;
            this.lblBrakePort.Text = "Brake Port";
            this.lblBrakePort.Click += new System.EventHandler(this.label1_Click);
            // 
            // chkBoxHasBrake
            // 
            this.chkBoxHasBrake.AutoSize = true;
            this.chkBoxHasBrake.Location = new System.Drawing.Point(161, 3);
            this.chkBoxHasBrake.Name = "chkBoxHasBrake";
            this.chkBoxHasBrake.Size = new System.Drawing.Size(76, 17);
            this.chkBoxHasBrake.TabIndex = 0;
            this.chkBoxHasBrake.Text = "Has Brake";
            this.chkBoxHasBrake.UseVisualStyleBackColor = true;
            this.chkBoxHasBrake.CheckedChanged += new System.EventHandler(this.chkBoxHasBrake_CheckedChanged);
            // 
            // metaElevatorStages
            // 
            this.metaElevatorStages.Controls.Add(this.cmbStages);
            this.metaElevatorStages.Location = new System.Drawing.Point(4, 22);
            this.metaElevatorStages.Margin = new System.Windows.Forms.Padding(2);
            this.metaElevatorStages.Name = "metaElevatorStages";
            this.metaElevatorStages.Size = new System.Drawing.Size(316, 49);
            this.metaElevatorStages.TabIndex = 4;
            this.metaElevatorStages.Text = "Stages";
            this.metaElevatorStages.UseVisualStyleBackColor = true;
            // 
            // cmbStages
            // 
            this.cmbStages.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmbStages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStages.FormattingEnabled = true;
            this.cmbStages.Items.AddRange(new object[] {
            "Single Stage Elevator",
            "Cascading Stage One",
            "Cascading Stage Two",
            "Continuous Stage One",
            "Continuos Stage Two"});
            this.cmbStages.Location = new System.Drawing.Point(0, 0);
            this.cmbStages.Margin = new System.Windows.Forms.Padding(2);
            this.cmbStages.Name = "cmbStages";
            this.cmbStages.Size = new System.Drawing.Size(316, 21);
            this.cmbStages.TabIndex = 0;
            // 
            // ConfigJointLayout
            // 
            this.ConfigJointLayout.AutoSize = true;
            this.ConfigJointLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ConfigJointLayout.ColumnCount = 1;
            this.ConfigJointLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ConfigJointLayout.Controls.Add(this.grpChooseDriver, 0, 0);
            this.ConfigJointLayout.Controls.Add(this.grpDriveOptions, 0, 2);
            this.ConfigJointLayout.Controls.Add(this.btnSave, 0, 3);
            this.ConfigJointLayout.Controls.Add(this.tabsMeta, 0, 1);
            this.ConfigJointLayout.Location = new System.Drawing.Point(3, 3);
            this.ConfigJointLayout.Name = "ConfigJointLayout";
            this.ConfigJointLayout.RowCount = 4;
            this.ConfigJointLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ConfigJointLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ConfigJointLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ConfigJointLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ConfigJointLayout.Size = new System.Drawing.Size(328, 260);
            this.ConfigJointLayout.TabIndex = 12;
            // 
            // GearLayout
            // 
            this.GearLayout.AutoSize = true;
            this.GearLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.GearLayout.ColumnCount = 2;
            this.GearLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.GearLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.GearLayout.Controls.Add(this.txtGearRationDenom, 1, 1);
            this.GearLayout.Controls.Add(this.txtGearRationNum, 0, 1);
            this.GearLayout.Controls.Add(this.lblOutputGear, 1, 0);
            this.GearLayout.Controls.Add(this.lblInputGear, 0, 0);
            this.GearLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.GearLayout.Location = new System.Drawing.Point(0, 0);
            this.GearLayout.Name = "GearLayout";
            this.GearLayout.RowCount = 2;
            this.GearLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.GearLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.GearLayout.Size = new System.Drawing.Size(316, 47);
            this.GearLayout.TabIndex = 13;
            // 
            // BreakLayout
            // 
            this.BreakLayout.AutoSize = true;
            this.BreakLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BreakLayout.ColumnCount = 2;
            this.BreakLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.BreakLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.BreakLayout.Controls.Add(this.brakePortB, 1, 1);
            this.BreakLayout.Controls.Add(this.lblBrakePort, 0, 0);
            this.BreakLayout.Controls.Add(this.brakePortA, 0, 1);
            this.BreakLayout.Controls.Add(this.chkBoxHasBrake, 1, 0);
            this.BreakLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.BreakLayout.Location = new System.Drawing.Point(0, 0);
            this.BreakLayout.Name = "BreakLayout";
            this.BreakLayout.RowCount = 2;
            this.BreakLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.BreakLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.BreakLayout.Size = new System.Drawing.Size(316, 47);
            this.BreakLayout.TabIndex = 13;
            // 
            // DriveChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(410, 385);
            this.Controls.Add(this.ConfigJointLayout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DriveChooser";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Joint Configuration";
            this.Load += new System.EventHandler(this.DriveChooser_Load);
            this.grpChooseDriver.ResumeLayout(false);
            this.grpDriveOptions.ResumeLayout(false);
            this.grpDriveOptions.PerformLayout();
            this.JointOptionsLayout.ResumeLayout(false);
            this.JointOptionsLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtHighLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLowLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPortA)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPortB)).EndInit();
            this.tabsMeta.ResumeLayout(false);
            this.metaWheel.ResumeLayout(false);
            this.metaWheel.PerformLayout();
            this.WheelLayout.ResumeLayout(false);
            this.WheelLayout.PerformLayout();
            this.metaPneumatic.ResumeLayout(false);
            this.metaPneumatic.PerformLayout();
            this.PneumaticLayout.ResumeLayout(false);
            this.PneumaticLayout.PerformLayout();
            this.metaGearing.ResumeLayout(false);
            this.metaGearing.PerformLayout();
            this.metaElevatorBrake.ResumeLayout(false);
            this.metaElevatorBrake.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.brakePortB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.brakePortA)).EndInit();
            this.metaElevatorStages.ResumeLayout(false);
            this.ConfigJointLayout.ResumeLayout(false);
            this.ConfigJointLayout.PerformLayout();
            this.GearLayout.ResumeLayout(false);
            this.GearLayout.PerformLayout();
            this.BreakLayout.ResumeLayout(false);
            this.BreakLayout.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ComboBox cmbJointDriver;
    private System.Windows.Forms.GroupBox grpChooseDriver;
    private System.Windows.Forms.GroupBox grpDriveOptions;
    private System.Windows.Forms.NumericUpDown txtPortB;
    private System.Windows.Forms.Label lblPort;
    private System.Windows.Forms.NumericUpDown txtPortA;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.ComboBox cmbWheelType;
    private System.Windows.Forms.ComboBox cmbPneumaticPressure;
    private System.Windows.Forms.TabControl tabsMeta;
    private System.Windows.Forms.TabPage metaWheel;
    private System.Windows.Forms.TabPage metaPneumatic;
    private System.Windows.Forms.ComboBox cmbFrictionLevel;
    private System.Windows.Forms.ComboBox cmbPneumaticDiameter;
    private System.Windows.Forms.Label lblDiameter;
    private System.Windows.Forms.TabPage metaGearing;
    private System.Windows.Forms.Label lblInputGear;
    private System.Windows.Forms.TextBox txtGearRationNum;
    private System.Windows.Forms.TextBox txtGearRationDenom;
    private System.Windows.Forms.Label lblPressure;
    private System.Windows.Forms.Label lblOutputGear;
    private System.Windows.Forms.NumericUpDown txtHighLimit;
    private System.Windows.Forms.Label lblLimits;
    private System.Windows.Forms.NumericUpDown txtLowLimit;
    private System.Windows.Forms.Label lblFriction;
    private System.Windows.Forms.Label lblType;
    private System.Windows.Forms.TabPage metaElevatorBrake;
    private System.Windows.Forms.CheckBox chkBoxHasBrake;
    private System.Windows.Forms.CheckBox chkBoxDriveWheel;
    private System.Windows.Forms.Label lblBrakePort;
    private System.Windows.Forms.NumericUpDown brakePortB;
    private System.Windows.Forms.NumericUpDown brakePortA;
    private System.Windows.Forms.TabPage metaElevatorStages;
    private System.Windows.Forms.ComboBox cmbStages;
    private System.Windows.Forms.RadioButton rbPWM;
    private System.Windows.Forms.RadioButton rbCAN;
    private System.Windows.Forms.TableLayoutPanel ConfigJointLayout;
    private System.Windows.Forms.TableLayoutPanel JointOptionsLayout;
    private System.Windows.Forms.TableLayoutPanel WheelLayout;
    private System.Windows.Forms.TableLayoutPanel PneumaticLayout;
    private System.Windows.Forms.TableLayoutPanel GearLayout;
    private System.Windows.Forms.TableLayoutPanel BreakLayout;
}