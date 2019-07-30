namespace InventorRobotExporter.GUI.Editors.JointEditor
{
    partial class JointDriverEditor
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
            this.cmbJointDriver = new System.Windows.Forms.ComboBox();
            this.grpChooseDriver = new System.Windows.Forms.GroupBox();
            this.grpDriveOptions = new System.Windows.Forms.GroupBox();
            this.JointOptionsLayout = new System.Windows.Forms.TableLayoutPanel();
            this.txtHighLimit = new System.Windows.Forms.NumericUpDown();
            this.lblPort = new System.Windows.Forms.Label();
            this.txtLowLimit = new System.Windows.Forms.NumericUpDown();
            this.txtPort1 = new System.Windows.Forms.NumericUpDown();
            this.txtPort2 = new System.Windows.Forms.NumericUpDown();
            this.lblLimits = new System.Windows.Forms.Label();
            this.rbCAN = new System.Windows.Forms.RadioButton();
            this.rbPWM = new System.Windows.Forms.RadioButton();
            this.cmbWheelType = new System.Windows.Forms.ComboBox();
            this.cmbPneumaticPressure = new System.Windows.Forms.ComboBox();
            this.tabsMeta = new System.Windows.Forms.TabControl();
            this.metaWheel = new System.Windows.Forms.TabPage();
            this.WheelLayout = new System.Windows.Forms.TableLayoutPanel();
            this.cmbDriveSide = new System.Windows.Forms.ComboBox();
            this.chkBoxDriveWheel = new System.Windows.Forms.CheckBox();
            this.lblFriction = new System.Windows.Forms.Label();
            this.cmbFrictionLevel = new System.Windows.Forms.ComboBox();
            this.lblType = new System.Windows.Forms.Label();
            this.metaPneumatic = new System.Windows.Forms.TabPage();
            this.PneumaticLayout = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.numericUpDownPnuDia = new System.Windows.Forms.NumericUpDown();
            this.PnuDiaUnits = new System.Windows.Forms.Label();
            this.lblPressure = new System.Windows.Forms.Label();
            this.lblDiameter = new System.Windows.Forms.Label();
            this.metaGearing = new System.Windows.Forms.TabPage();
            this.GearLayout = new System.Windows.Forms.TableLayoutPanel();
            this.OutputGeartxt = new System.Windows.Forms.NumericUpDown();
            this.lblOutputGear = new System.Windows.Forms.Label();
            this.lblInputGear = new System.Windows.Forms.Label();
            this.InputGeartxt = new System.Windows.Forms.NumericUpDown();
            this.metaBrake = new System.Windows.Forms.TabPage();
            this.BreakLayout = new System.Windows.Forms.TableLayoutPanel();
            this.chkBoxHasBrake = new System.Windows.Forms.CheckBox();
            this.metaMotorType = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.RobotCompetitionDropDown = new System.Windows.Forms.ComboBox();
            this.MotorTypeDropDown = new System.Windows.Forms.ComboBox();
            this.RobotCompetitonLabel = new System.Windows.Forms.Label();
            this.MotorTypeLabel = new System.Windows.Forms.Label();
            this.ConfigJointLayout = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.CalculatedWeightCheck = new System.Windows.Forms.CheckBox();
            this.WeightBox = new System.Windows.Forms.NumericUpDown();
            this.WeightLabel = new System.Windows.Forms.Label();
            this.UnitBox = new System.Windows.Forms.ComboBox();
            this.grpChooseDriver.SuspendLayout();
            this.grpDriveOptions.SuspendLayout();
            this.JointOptionsLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtHighLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLowLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort2)).BeginInit();
            this.tabsMeta.SuspendLayout();
            this.metaWheel.SuspendLayout();
            this.WheelLayout.SuspendLayout();
            this.metaPneumatic.SuspendLayout();
            this.PneumaticLayout.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPnuDia)).BeginInit();
            this.metaGearing.SuspendLayout();
            this.GearLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OutputGeartxt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.InputGeartxt)).BeginInit();
            this.metaBrake.SuspendLayout();
            this.BreakLayout.SuspendLayout();
            this.metaMotorType.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.ConfigJointLayout.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WeightBox)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbJointDriver
            // 
            this.cmbJointDriver.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbJointDriver.FormattingEnabled = true;
            this.cmbJointDriver.Location = new System.Drawing.Point(5, 21);
            this.cmbJointDriver.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbJointDriver.Name = "cmbJointDriver";
            this.cmbJointDriver.Size = new System.Drawing.Size(508, 24);
            this.cmbJointDriver.TabIndex = 0;
            this.cmbJointDriver.SelectedIndexChanged += new System.EventHandler(this.cmbJointDriver_SelectedIndexChanged);
            // 
            // grpChooseDriver
            // 
            this.grpChooseDriver.Controls.Add(this.cmbJointDriver);
            this.grpChooseDriver.Location = new System.Drawing.Point(3, 90);
            this.grpChooseDriver.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grpChooseDriver.Name = "grpChooseDriver";
            this.grpChooseDriver.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grpChooseDriver.Size = new System.Drawing.Size(516, 54);
            this.grpChooseDriver.TabIndex = 1;
            this.grpChooseDriver.TabStop = false;
            this.grpChooseDriver.Text = "Joint Driver";
            // 
            // grpDriveOptions
            // 
            this.grpDriveOptions.AutoSize = true;
            this.grpDriveOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpDriveOptions.Controls.Add(this.JointOptionsLayout);
            this.grpDriveOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpDriveOptions.Location = new System.Drawing.Point(3, 244);
            this.grpDriveOptions.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grpDriveOptions.Name = "grpDriveOptions";
            this.grpDriveOptions.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grpDriveOptions.Size = new System.Drawing.Size(516, 121);
            this.grpDriveOptions.TabIndex = 2;
            this.grpDriveOptions.TabStop = false;
            this.grpDriveOptions.Text = "Other";
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
            this.JointOptionsLayout.Controls.Add(this.txtPort1, 0, 1);
            this.JointOptionsLayout.Controls.Add(this.txtPort2, 1, 1);
            this.JointOptionsLayout.Controls.Add(this.lblLimits, 0, 2);
            this.JointOptionsLayout.Controls.Add(this.rbCAN, 3, 1);
            this.JointOptionsLayout.Controls.Add(this.rbPWM, 2, 1);
            this.JointOptionsLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.JointOptionsLayout.Location = new System.Drawing.Point(3, 17);
            this.JointOptionsLayout.Margin = new System.Windows.Forms.Padding(4);
            this.JointOptionsLayout.Name = "JointOptionsLayout";
            this.JointOptionsLayout.RowCount = 4;
            this.JointOptionsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.JointOptionsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.JointOptionsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.JointOptionsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.JointOptionsLayout.Size = new System.Drawing.Size(510, 102);
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
            this.txtHighLimit.Location = new System.Drawing.Point(191, 78);
            this.txtHighLimit.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtHighLimit.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.txtHighLimit.Name = "txtHighLimit";
            this.txtHighLimit.Size = new System.Drawing.Size(182, 22);
            this.txtHighLimit.TabIndex = 4;
            this.txtHighLimit.Visible = false;
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(4, 4);
            this.lblPort.Margin = new System.Windows.Forms.Padding(4);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(34, 17);
            this.lblPort.TabIndex = 1;
            this.lblPort.Text = "Port";
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
            this.txtLowLimit.Location = new System.Drawing.Point(3, 78);
            this.txtLowLimit.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtLowLimit.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.txtLowLimit.Name = "txtLowLimit";
            this.txtLowLimit.Size = new System.Drawing.Size(182, 22);
            this.txtLowLimit.TabIndex = 3;
            this.txtLowLimit.Visible = false;
            // 
            // txtPort1
            // 
            this.txtPort1.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtPort1.Location = new System.Drawing.Point(3, 27);
            this.txtPort1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtPort1.Name = "txtPort1";
            this.txtPort1.Size = new System.Drawing.Size(182, 22);
            this.txtPort1.TabIndex = 1;
            this.txtPort1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // txtPort2
            // 
            this.txtPort2.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtPort2.Location = new System.Drawing.Point(191, 27);
            this.txtPort2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtPort2.Name = "txtPort2";
            this.txtPort2.Size = new System.Drawing.Size(182, 22);
            this.txtPort2.TabIndex = 2;
            this.txtPort2.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblLimits
            // 
            this.lblLimits.AutoSize = true;
            this.lblLimits.Location = new System.Drawing.Point(4, 55);
            this.lblLimits.Margin = new System.Windows.Forms.Padding(4);
            this.lblLimits.Name = "lblLimits";
            this.lblLimits.Size = new System.Drawing.Size(78, 17);
            this.lblLimits.TabIndex = 4;
            this.lblLimits.Text = "Joint Limits";
            this.lblLimits.Visible = false;
            // 
            // rbCAN
            // 
            this.rbCAN.AutoSize = true;
            this.rbCAN.Dock = System.Windows.Forms.DockStyle.Right;
            this.rbCAN.Location = new System.Drawing.Point(447, 27);
            this.rbCAN.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rbCAN.Name = "rbCAN";
            this.rbCAN.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.rbCAN.Size = new System.Drawing.Size(60, 22);
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
            this.rbPWM.Location = new System.Drawing.Point(379, 27);
            this.rbPWM.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rbPWM.Name = "rbPWM";
            this.rbPWM.Size = new System.Drawing.Size(62, 22);
            this.rbPWM.TabIndex = 7;
            this.rbPWM.TabStop = true;
            this.rbPWM.Text = "PWM";
            this.rbPWM.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.rbPWM.UseVisualStyleBackColor = true;
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
            this.cmbWheelType.Location = new System.Drawing.Point(3, 27);
            this.cmbWheelType.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbWheelType.Name = "cmbWheelType";
            this.cmbWheelType.Size = new System.Drawing.Size(163, 24);
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
            this.cmbPneumaticPressure.Location = new System.Drawing.Point(257, 27);
            this.cmbPneumaticPressure.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbPneumaticPressure.Name = "cmbPneumaticPressure";
            this.cmbPneumaticPressure.Size = new System.Drawing.Size(248, 24);
            this.cmbPneumaticPressure.TabIndex = 6;
            // 
            // tabsMeta
            // 
            this.tabsMeta.Controls.Add(this.metaWheel);
            this.tabsMeta.Controls.Add(this.metaPneumatic);
            this.tabsMeta.Controls.Add(this.metaGearing);
            this.tabsMeta.Controls.Add(this.metaBrake);
            this.tabsMeta.Controls.Add(this.metaMotorType);
            this.tabsMeta.Location = new System.Drawing.Point(3, 148);
            this.tabsMeta.Margin = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this.tabsMeta.Name = "tabsMeta";
            this.tabsMeta.SelectedIndex = 0;
            this.tabsMeta.Size = new System.Drawing.Size(516, 92);
            this.tabsMeta.TabIndex = 11;
            // 
            // metaWheel
            // 
            this.metaWheel.Controls.Add(this.WheelLayout);
            this.metaWheel.Location = new System.Drawing.Point(4, 25);
            this.metaWheel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.metaWheel.Name = "metaWheel";
            this.metaWheel.Size = new System.Drawing.Size(508, 63);
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
            this.WheelLayout.Controls.Add(this.cmbDriveSide, 2, 1);
            this.WheelLayout.Controls.Add(this.chkBoxDriveWheel, 2, 0);
            this.WheelLayout.Controls.Add(this.lblFriction, 1, 0);
            this.WheelLayout.Controls.Add(this.cmbFrictionLevel, 1, 1);
            this.WheelLayout.Controls.Add(this.lblType, 0, 0);
            this.WheelLayout.Controls.Add(this.cmbWheelType, 0, 1);
            this.WheelLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.WheelLayout.Location = new System.Drawing.Point(0, 0);
            this.WheelLayout.Margin = new System.Windows.Forms.Padding(4);
            this.WheelLayout.Name = "WheelLayout";
            this.WheelLayout.RowCount = 2;
            this.WheelLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.WheelLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.WheelLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.WheelLayout.Size = new System.Drawing.Size(508, 53);
            this.WheelLayout.TabIndex = 13;
            // 
            // cmbDriveSide
            // 
            this.cmbDriveSide.Dock = System.Windows.Forms.DockStyle.Top;
            this.cmbDriveSide.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDriveSide.FormattingEnabled = true;
            this.cmbDriveSide.Items.AddRange(new object[] {
            "High",
            "Medium",
            "Low"});
            this.cmbDriveSide.Location = new System.Drawing.Point(341, 27);
            this.cmbDriveSide.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbDriveSide.Name = "cmbDriveSide";
            this.cmbDriveSide.Size = new System.Drawing.Size(164, 24);
            this.cmbDriveSide.TabIndex = 16;
            this.cmbDriveSide.Visible = false;
            // 
            // chkBoxDriveWheel
            // 
            this.chkBoxDriveWheel.AutoSize = true;
            this.chkBoxDriveWheel.Dock = System.Windows.Forms.DockStyle.Right;
            this.chkBoxDriveWheel.Location = new System.Drawing.Point(394, 2);
            this.chkBoxDriveWheel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chkBoxDriveWheel.Name = "chkBoxDriveWheel";
            this.chkBoxDriveWheel.Size = new System.Drawing.Size(111, 21);
            this.chkBoxDriveWheel.TabIndex = 5;
            this.chkBoxDriveWheel.Text = "Drive Wheel ";
            this.chkBoxDriveWheel.UseVisualStyleBackColor = true;
            this.chkBoxDriveWheel.CheckedChanged += new System.EventHandler(this.chkBoxDriveWheel_CheckedChanged);
            // 
            // lblFriction
            // 
            this.lblFriction.AutoSize = true;
            this.lblFriction.Location = new System.Drawing.Point(173, 4);
            this.lblFriction.Margin = new System.Windows.Forms.Padding(4);
            this.lblFriction.Name = "lblFriction";
            this.lblFriction.Size = new System.Drawing.Size(92, 17);
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
            this.cmbFrictionLevel.Location = new System.Drawing.Point(172, 27);
            this.cmbFrictionLevel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbFrictionLevel.Name = "cmbFrictionLevel";
            this.cmbFrictionLevel.Size = new System.Drawing.Size(163, 24);
            this.cmbFrictionLevel.TabIndex = 13;
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(4, 4);
            this.lblType.Margin = new System.Windows.Forms.Padding(4);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(84, 17);
            this.lblType.TabIndex = 14;
            this.lblType.Text = "Wheel Type";
            // 
            // metaPneumatic
            // 
            this.metaPneumatic.Controls.Add(this.PneumaticLayout);
            this.metaPneumatic.Location = new System.Drawing.Point(4, 25);
            this.metaPneumatic.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.metaPneumatic.Name = "metaPneumatic";
            this.metaPneumatic.Size = new System.Drawing.Size(508, 63);
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
            this.PneumaticLayout.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.PneumaticLayout.Controls.Add(this.cmbPneumaticPressure, 1, 1);
            this.PneumaticLayout.Controls.Add(this.lblPressure, 1, 0);
            this.PneumaticLayout.Controls.Add(this.lblDiameter, 0, 0);
            this.PneumaticLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.PneumaticLayout.Location = new System.Drawing.Point(0, 0);
            this.PneumaticLayout.Margin = new System.Windows.Forms.Padding(4);
            this.PneumaticLayout.Name = "PneumaticLayout";
            this.PneumaticLayout.RowCount = 2;
            this.PneumaticLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PneumaticLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PneumaticLayout.Size = new System.Drawing.Size(508, 63);
            this.PneumaticLayout.TabIndex = 13;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 63F));
            this.tableLayoutPanel2.Controls.Add(this.numericUpDownPnuDia, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.PnuDiaUnits, 1, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 28);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(248, 32);
            this.tableLayoutPanel2.TabIndex = 15;
            // 
            // numericUpDownPnuDia
            // 
            this.numericUpDownPnuDia.DecimalPlaces = 6;
            this.numericUpDownPnuDia.Location = new System.Drawing.Point(3, 3);
            this.numericUpDownPnuDia.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownPnuDia.Name = "numericUpDownPnuDia";
            this.numericUpDownPnuDia.Size = new System.Drawing.Size(179, 22);
            this.numericUpDownPnuDia.TabIndex = 0;
            // 
            // PnuDiaUnits
            // 
            this.PnuDiaUnits.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.PnuDiaUnits.AutoSize = true;
            this.PnuDiaUnits.Location = new System.Drawing.Point(189, 7);
            this.PnuDiaUnits.Margin = new System.Windows.Forms.Padding(4);
            this.PnuDiaUnits.Name = "PnuDiaUnits";
            this.PnuDiaUnits.Size = new System.Drawing.Size(29, 17);
            this.PnuDiaUnits.TabIndex = 15;
            this.PnuDiaUnits.Text = "(in)";
            this.PnuDiaUnits.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPressure
            // 
            this.lblPressure.AutoSize = true;
            this.lblPressure.Location = new System.Drawing.Point(258, 4);
            this.lblPressure.Margin = new System.Windows.Forms.Padding(4);
            this.lblPressure.Name = "lblPressure";
            this.lblPressure.Size = new System.Drawing.Size(65, 17);
            this.lblPressure.TabIndex = 13;
            this.lblPressure.Text = "Pressure";
            this.lblPressure.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
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
            // metaGearing
            // 
            this.metaGearing.Controls.Add(this.GearLayout);
            this.metaGearing.Location = new System.Drawing.Point(4, 25);
            this.metaGearing.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.metaGearing.Name = "metaGearing";
            this.metaGearing.Size = new System.Drawing.Size(508, 63);
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
            this.GearLayout.Controls.Add(this.OutputGeartxt, 0, 1);
            this.GearLayout.Controls.Add(this.lblOutputGear, 1, 0);
            this.GearLayout.Controls.Add(this.lblInputGear, 0, 0);
            this.GearLayout.Controls.Add(this.InputGeartxt, 0, 1);
            this.GearLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.GearLayout.Location = new System.Drawing.Point(0, 0);
            this.GearLayout.Margin = new System.Windows.Forms.Padding(4);
            this.GearLayout.Name = "GearLayout";
            this.GearLayout.RowCount = 2;
            this.GearLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.GearLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.GearLayout.Size = new System.Drawing.Size(508, 56);
            this.GearLayout.TabIndex = 13;
            // 
            // OutputGeartxt
            // 
            this.OutputGeartxt.DecimalPlaces = 5;
            this.OutputGeartxt.Location = new System.Drawing.Point(257, 31);
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
            this.OutputGeartxt.Size = new System.Drawing.Size(206, 22);
            this.OutputGeartxt.TabIndex = 16;
            this.OutputGeartxt.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblOutputGear
            // 
            this.lblOutputGear.AutoSize = true;
            this.lblOutputGear.Location = new System.Drawing.Point(258, 4);
            this.lblOutputGear.Margin = new System.Windows.Forms.Padding(4);
            this.lblOutputGear.Name = "lblOutputGear";
            this.lblOutputGear.Size = new System.Drawing.Size(78, 20);
            this.lblOutputGear.TabIndex = 14;
            this.lblOutputGear.Text = "Output Gear";
            this.lblOutputGear.UseCompatibleTextRendering = true;
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
            this.InputGeartxt.Size = new System.Drawing.Size(206, 22);
            this.InputGeartxt.TabIndex = 15;
            this.InputGeartxt.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // metaBrake
            // 
            this.metaBrake.Controls.Add(this.BreakLayout);
            this.metaBrake.Location = new System.Drawing.Point(4, 25);
            this.metaBrake.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.metaBrake.Name = "metaBrake";
            this.metaBrake.Size = new System.Drawing.Size(508, 63);
            this.metaBrake.TabIndex = 3;
            this.metaBrake.Text = "Brake";
            this.metaBrake.UseVisualStyleBackColor = true;
            // 
            // BreakLayout
            // 
            this.BreakLayout.AutoSize = true;
            this.BreakLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BreakLayout.ColumnCount = 2;
            this.BreakLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.BreakLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.BreakLayout.Controls.Add(this.chkBoxHasBrake, 0, 0);
            this.BreakLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.BreakLayout.Location = new System.Drawing.Point(0, 0);
            this.BreakLayout.Margin = new System.Windows.Forms.Padding(4);
            this.BreakLayout.Name = "BreakLayout";
            this.BreakLayout.RowCount = 2;
            this.BreakLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.BreakLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.BreakLayout.Size = new System.Drawing.Size(508, 29);
            this.BreakLayout.TabIndex = 13;
            // 
            // chkBoxHasBrake
            // 
            this.chkBoxHasBrake.AutoSize = true;
            this.chkBoxHasBrake.Location = new System.Drawing.Point(4, 4);
            this.chkBoxHasBrake.Margin = new System.Windows.Forms.Padding(4);
            this.chkBoxHasBrake.Name = "chkBoxHasBrake";
            this.chkBoxHasBrake.Size = new System.Drawing.Size(96, 21);
            this.chkBoxHasBrake.TabIndex = 0;
            this.chkBoxHasBrake.Text = "Has Brake";
            this.chkBoxHasBrake.UseVisualStyleBackColor = true;
            // 
            // metaMotorType
            // 
            this.metaMotorType.Controls.Add(this.tableLayoutPanel1);
            this.metaMotorType.Location = new System.Drawing.Point(4, 25);
            this.metaMotorType.Name = "metaMotorType";
            this.metaMotorType.Size = new System.Drawing.Size(508, 63);
            this.metaMotorType.TabIndex = 4;
            this.metaMotorType.Text = "Motor Type";
            this.metaMotorType.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38.38583F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 61.61417F));
            this.tableLayoutPanel1.Controls.Add(this.RobotCompetitionDropDown, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.MotorTypeDropDown, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.RobotCompetitonLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.MotorTypeLabel, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(508, 63);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // RobotCompetitionDropDown
            // 
            this.RobotCompetitionDropDown.Dock = System.Windows.Forms.DockStyle.Top;
            this.RobotCompetitionDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RobotCompetitionDropDown.FormattingEnabled = true;
            this.RobotCompetitionDropDown.Items.AddRange(new object[] {
            "GENERIC",
            "FRC",
            "FTC",
            "VEX"});
            this.RobotCompetitionDropDown.Location = new System.Drawing.Point(3, 33);
            this.RobotCompetitionDropDown.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.RobotCompetitionDropDown.Name = "RobotCompetitionDropDown";
            this.RobotCompetitionDropDown.Size = new System.Drawing.Size(189, 24);
            this.RobotCompetitionDropDown.TabIndex = 14;
            this.RobotCompetitionDropDown.SelectedIndexChanged += new System.EventHandler(this.RobotCompetitionDropDown_SelectedIndexChanged);
            // 
            // MotorTypeDropDown
            // 
            this.MotorTypeDropDown.Dock = System.Windows.Forms.DockStyle.Top;
            this.MotorTypeDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.MotorTypeDropDown.Enabled = false;
            this.MotorTypeDropDown.FormattingEnabled = true;
            this.MotorTypeDropDown.Location = new System.Drawing.Point(198, 33);
            this.MotorTypeDropDown.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MotorTypeDropDown.Name = "MotorTypeDropDown";
            this.MotorTypeDropDown.Size = new System.Drawing.Size(307, 24);
            this.MotorTypeDropDown.TabIndex = 13;
            // 
            // RobotCompetitonLabel
            // 
            this.RobotCompetitonLabel.AutoSize = true;
            this.RobotCompetitonLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RobotCompetitonLabel.Location = new System.Drawing.Point(3, 0);
            this.RobotCompetitonLabel.Name = "RobotCompetitonLabel";
            this.RobotCompetitonLabel.Size = new System.Drawing.Size(189, 31);
            this.RobotCompetitonLabel.TabIndex = 15;
            this.RobotCompetitonLabel.Text = "Robot Competition";
            this.RobotCompetitonLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MotorTypeLabel
            // 
            this.MotorTypeLabel.AutoSize = true;
            this.MotorTypeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MotorTypeLabel.Location = new System.Drawing.Point(198, 0);
            this.MotorTypeLabel.Name = "MotorTypeLabel";
            this.MotorTypeLabel.Size = new System.Drawing.Size(307, 31);
            this.MotorTypeLabel.TabIndex = 16;
            this.MotorTypeLabel.Text = "Motor Type";
            this.MotorTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ConfigJointLayout
            // 
            this.ConfigJointLayout.AutoSize = true;
            this.ConfigJointLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ConfigJointLayout.ColumnCount = 1;
            this.ConfigJointLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ConfigJointLayout.Controls.Add(this.grpChooseDriver, 0, 1);
            this.ConfigJointLayout.Controls.Add(this.grpDriveOptions, 0, 3);
            this.ConfigJointLayout.Controls.Add(this.tabsMeta, 0, 2);
            this.ConfigJointLayout.Controls.Add(this.groupBox1, 0, 0);
            this.ConfigJointLayout.Location = new System.Drawing.Point(4, 4);
            this.ConfigJointLayout.Margin = new System.Windows.Forms.Padding(4);
            this.ConfigJointLayout.Name = "ConfigJointLayout";
            this.ConfigJointLayout.RowCount = 4;
            this.ConfigJointLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ConfigJointLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ConfigJointLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ConfigJointLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ConfigJointLayout.Size = new System.Drawing.Size(522, 367);
            this.ConfigJointLayout.TabIndex = 12;
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.tableLayoutPanel3);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(516, 82);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Attatched Component Weight";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.CalculatedWeightCheck, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.WeightBox, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.WeightLabel, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.UnitBox, 2, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 18);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(510, 61);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // CalculatedWeightCheck
            // 
            this.CalculatedWeightCheck.AutoSize = true;
            this.tableLayoutPanel3.SetColumnSpan(this.CalculatedWeightCheck, 3);
            this.CalculatedWeightCheck.Dock = System.Windows.Forms.DockStyle.Left;
            this.CalculatedWeightCheck.Location = new System.Drawing.Point(4, 36);
            this.CalculatedWeightCheck.Margin = new System.Windows.Forms.Padding(4);
            this.CalculatedWeightCheck.Name = "CalculatedWeightCheck";
            this.CalculatedWeightCheck.Size = new System.Drawing.Size(167, 21);
            this.CalculatedWeightCheck.TabIndex = 4;
            this.CalculatedWeightCheck.Text = "Use calculated weight";
            this.CalculatedWeightCheck.UseVisualStyleBackColor = true;
            this.CalculatedWeightCheck.CheckedChanged += new System.EventHandler(this.CalculatedWeightCheck_CheckedChanged);
            // 
            // WeightBox
            // 
            this.WeightBox.DecimalPlaces = 2;
            this.WeightBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.WeightBox.Location = new System.Drawing.Point(65, 5);
            this.WeightBox.Margin = new System.Windows.Forms.Padding(5);
            this.WeightBox.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.WeightBox.Name = "WeightBox";
            this.WeightBox.Size = new System.Drawing.Size(333, 22);
            this.WeightBox.TabIndex = 0;
            // 
            // WeightLabel
            // 
            this.WeightLabel.AutoSize = true;
            this.WeightLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.WeightLabel.Location = new System.Drawing.Point(4, 4);
            this.WeightLabel.Margin = new System.Windows.Forms.Padding(4);
            this.WeightLabel.Name = "WeightLabel";
            this.WeightLabel.Size = new System.Drawing.Size(52, 24);
            this.WeightLabel.TabIndex = 1;
            this.WeightLabel.Text = "Weight";
            this.WeightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // UnitBox
            // 
            this.UnitBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.UnitBox.Items.AddRange(new object[] {
            "Pounds",
            "Kilograms"});
            this.UnitBox.Location = new System.Drawing.Point(407, 4);
            this.UnitBox.Margin = new System.Windows.Forms.Padding(4);
            this.UnitBox.Name = "UnitBox";
            this.UnitBox.Size = new System.Drawing.Size(99, 24);
            this.UnitBox.TabIndex = 2;
            // 
            // JointCardEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.ConfigJointLayout);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "JointDriverEditor";
            this.Size = new System.Drawing.Size(530, 375);
            this.grpChooseDriver.ResumeLayout(false);
            this.grpDriveOptions.ResumeLayout(false);
            this.grpDriveOptions.PerformLayout();
            this.JointOptionsLayout.ResumeLayout(false);
            this.JointOptionsLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtHighLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLowLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort2)).EndInit();
            this.tabsMeta.ResumeLayout(false);
            this.metaWheel.ResumeLayout(false);
            this.metaWheel.PerformLayout();
            this.WheelLayout.ResumeLayout(false);
            this.WheelLayout.PerformLayout();
            this.metaPneumatic.ResumeLayout(false);
            this.metaPneumatic.PerformLayout();
            this.PneumaticLayout.ResumeLayout(false);
            this.PneumaticLayout.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPnuDia)).EndInit();
            this.metaGearing.ResumeLayout(false);
            this.metaGearing.PerformLayout();
            this.GearLayout.ResumeLayout(false);
            this.GearLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OutputGeartxt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.InputGeartxt)).EndInit();
            this.metaBrake.ResumeLayout(false);
            this.metaBrake.PerformLayout();
            this.BreakLayout.ResumeLayout(false);
            this.BreakLayout.PerformLayout();
            this.metaMotorType.ResumeLayout(false);
            this.metaMotorType.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ConfigJointLayout.ResumeLayout(false);
            this.ConfigJointLayout.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WeightBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbJointDriver;
        private System.Windows.Forms.GroupBox grpChooseDriver;
        private System.Windows.Forms.GroupBox grpDriveOptions;
        private System.Windows.Forms.NumericUpDown txtPort2;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.NumericUpDown txtPort1;
        private System.Windows.Forms.ComboBox cmbWheelType;
        private System.Windows.Forms.ComboBox cmbPneumaticPressure;
        private System.Windows.Forms.TabControl tabsMeta;
        private System.Windows.Forms.TabPage metaWheel;
        private System.Windows.Forms.TabPage metaPneumatic;
        private System.Windows.Forms.ComboBox cmbFrictionLevel;
        private System.Windows.Forms.Label lblDiameter;
        private System.Windows.Forms.TabPage metaGearing;
        private System.Windows.Forms.Label lblInputGear;
        private System.Windows.Forms.Label lblPressure;
        private System.Windows.Forms.Label lblOutputGear;
        private System.Windows.Forms.NumericUpDown txtHighLimit;
        private System.Windows.Forms.Label lblLimits;
        private System.Windows.Forms.NumericUpDown txtLowLimit;
        private System.Windows.Forms.Label lblFriction;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.TabPage metaBrake;
        private System.Windows.Forms.CheckBox chkBoxHasBrake;
        private System.Windows.Forms.RadioButton rbPWM;
        private System.Windows.Forms.RadioButton rbCAN;
        private System.Windows.Forms.TableLayoutPanel ConfigJointLayout;
        private System.Windows.Forms.TableLayoutPanel JointOptionsLayout;
        private System.Windows.Forms.TableLayoutPanel WheelLayout;
        private System.Windows.Forms.TableLayoutPanel PneumaticLayout;
        private System.Windows.Forms.TableLayoutPanel GearLayout;
        private System.Windows.Forms.TableLayoutPanel BreakLayout;
        private System.Windows.Forms.NumericUpDown InputGeartxt;
        private System.Windows.Forms.NumericUpDown OutputGeartxt;
        private System.Windows.Forms.TabPage metaMotorType;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label MotorTypeLabel;
        private System.Windows.Forms.ComboBox RobotCompetitionDropDown;
        private System.Windows.Forms.ComboBox MotorTypeDropDown;
        private System.Windows.Forms.Label RobotCompetitonLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.NumericUpDown numericUpDownPnuDia;
        private System.Windows.Forms.Label PnuDiaUnits;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.CheckBox CalculatedWeightCheck;
        private System.Windows.Forms.NumericUpDown WeightBox;
        private System.Windows.Forms.Label WeightLabel;
        private System.Windows.Forms.ComboBox UnitBox;
        private System.Windows.Forms.CheckBox chkBoxDriveWheel;
        private System.Windows.Forms.ComboBox cmbDriveSide;
    }
}