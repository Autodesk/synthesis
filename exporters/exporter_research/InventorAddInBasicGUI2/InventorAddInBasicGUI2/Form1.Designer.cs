using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventorAddInBasicGUI2
{
    partial class Form1
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
        /// Required method for Design er support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grpChooseDriver = new System.Windows.Forms.GroupBox();
            this.grpDriveOptions = new System.Windows.Forms.GroupBox();
            this.rbPWM = new System.Windows.Forms.RadioButton();
            this.rbCAN = new System.Windows.Forms.RadioButton();
            this.chkBoxDriveWheel = new System.Windows.Forms.CheckBox();
            this.txtHighLimit = new System.Windows.Forms.NumericUpDown();
            this.lblLimits = new System.Windows.Forms.Label();
            this.txtLowLimit = new System.Windows.Forms.NumericUpDown();
            this.Solenoid1txtPort = new System.Windows.Forms.NumericUpDown();
            this.Solenoid2txtPort = new System.Windows.Forms.NumericUpDown();
            this.PWM1txtPort = new System.Windows.Forms.NumericUpDown();
            this.PWM2txtPort = new System.Windows.Forms.NumericUpDown();
            this.CANtxtPort1 = new System.Windows.Forms.NumericUpDown();
            this.CANtxtPort2 = new System.Windows.Forms.NumericUpDown();
            this.RelaytxtPort = new System.Windows.Forms.NumericUpDown();
            this.lblPort = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.cmbWheelType = new System.Windows.Forms.ComboBox();
            this.cmbPneumaticPressure = new System.Windows.Forms.ComboBox();
            this.tabsMeta = new System.Windows.Forms.TabControl();
            this.metaWheel = new System.Windows.Forms.TabPage();
            this.lblFriction = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.cmbFrictionLevel = new System.Windows.Forms.ComboBox();
            this.metaPneumatic = new System.Windows.Forms.TabPage();
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
            this.grpChooseDriver.SuspendLayout();
            this.grpDriveOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtHighLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLowLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Solenoid1txtPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Solenoid2txtPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PWM1txtPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PWM2txtPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CANtxtPort1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CANtxtPort2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RelaytxtPort)).BeginInit();
            this.tabsMeta.SuspendLayout();
            this.metaWheel.SuspendLayout();
            this.metaPneumatic.SuspendLayout();
            this.metaGearing.SuspendLayout();
            this.metaElevatorBrake.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.brakePortB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.brakePortA)).BeginInit();
            this.metaElevatorStages.SuspendLayout();
            this.SuspendLayout();
            //
            // grpChooseDriver
            // 
            this.grpChooseDriver.Location = new System.Drawing.Point(12, 12);
            this.grpChooseDriver.Name = "grpChooseDriver";
            this.grpChooseDriver.Size = new System.Drawing.Size(319, 56);
            this.grpChooseDriver.TabIndex = 1;
            this.grpChooseDriver.TabStop = false;
            this.grpChooseDriver.Text = "Joint Driver";
            // 
            // grpDriveOptions
            // 
            this.grpDriveOptions.Controls.Add(this.rbPWM);
            this.grpDriveOptions.Controls.Add(this.rbCAN);
            this.grpDriveOptions.Controls.Add(this.chkBoxDriveWheel);
            this.grpDriveOptions.Controls.Add(this.txtHighLimit);
            this.grpDriveOptions.Controls.Add(this.lblLimits);
            this.grpDriveOptions.Controls.Add(this.txtLowLimit);
            this.grpDriveOptions.Controls.Add(this.Solenoid1txtPort);
            this.grpDriveOptions.Controls.Add(this.Solenoid2txtPort);
            this.grpDriveOptions.Controls.Add(this.PWM1txtPort);
            this.grpDriveOptions.Controls.Add(this.PWM2txtPort);
            this.grpDriveOptions.Controls.Add(this.CANtxtPort1);
            this.grpDriveOptions.Controls.Add(this.CANtxtPort2);
            this.grpDriveOptions.Controls.Add(this.RelaytxtPort);
            this.grpDriveOptions.Controls.Add(this.lblPort);
            this.grpDriveOptions.Location = new System.Drawing.Point(13, 75);
            this.grpDriveOptions.Name = "grpDriveOptions";
            this.grpDriveOptions.Size = new System.Drawing.Size(318, 72);
            this.grpDriveOptions.TabIndex = 2;
            this.grpDriveOptions.TabStop = false;
            this.grpDriveOptions.Text = "Joint Options";
            this.grpChooseDriver.Hide();
            // 
            // rbPWM
            // 
            this.rbPWM.AutoSize = true;
            this.rbPWM.Location = new System.Drawing.Point(244, 43);
            this.rbPWM.Name = "rbPWM";
            this.rbPWM.Size = new System.Drawing.Size(62, 21);
            this.rbPWM.TabIndex = 7;
            this.rbPWM.TabStop = true;
            this.rbPWM.Text = "PWM";
            this.rbPWM.UseVisualStyleBackColor = true;
            this.rbPWM.Click += new System.EventHandler(this.btnPWM_Click);
            // 
            // rbCAN
            // 
            this.rbCAN.AutoSize = true;
            this.rbCAN.Location = new System.Drawing.Point(244, 22);
            this.rbCAN.Name = "rbCAN";
            this.rbCAN.Size = new System.Drawing.Size(57, 21);
            this.rbCAN.TabIndex = 6;
            this.rbCAN.TabStop = true;
            this.rbCAN.Text = "CAN";
            this.rbCAN.UseVisualStyleBackColor = true;
            this.rbCAN.Click += new System.EventHandler(this.btnCAN_Click);
            // 
            // chkBoxDriveWheel
            // 
            this.chkBoxDriveWheel.AutoSize = true;
            this.chkBoxDriveWheel.Location = new System.Drawing.Point(219, 0);
            this.chkBoxDriveWheel.Name = "chkBoxDriveWheel";
            this.chkBoxDriveWheel.Size = new System.Drawing.Size(111, 21);
            this.chkBoxDriveWheel.TabIndex = 5;
            this.chkBoxDriveWheel.Text = "Drive Wheel ";
            this.chkBoxDriveWheel.UseVisualStyleBackColor = true;
            // 
            // txtHighLimit
            // 
            this.txtHighLimit.DecimalPlaces = 4;
            this.txtHighLimit.Increment = new decimal(new int[] { 1, 0, 0, 65536});
            this.txtHighLimit.Location = new System.Drawing.Point(140, 92);
            this.txtHighLimit.Minimum = new decimal(new int[] { 100, 0, 0, -2147483648});
            this.txtHighLimit.Name = "txtHighLimit";
            this.txtHighLimit.Size = new System.Drawing.Size(120, 22);
            this.txtHighLimit.TabIndex = 4;
            this.txtHighLimit.Visible = false;
            // 
            // lblLimits
            // 
            this.lblLimits.AutoSize = true;
            this.lblLimits.Location = new System.Drawing.Point(11, 72);
            this.lblLimits.Name = "lblLimits";
            this.lblLimits.Size = new System.Drawing.Size(78, 17);
            this.lblLimits.TabIndex = 4;
            this.lblLimits.Text = "Joint Limits";
            this.lblLimits.Visible = false;
            // 
            // txtLowLimit
            // 
            this.txtLowLimit.DecimalPlaces = 4;
            this.txtLowLimit.Increment = new decimal(new int[] { 1, 0, 0, 65536});
            this.txtLowLimit.Location = new System.Drawing.Point(14, 92);
            this.txtLowLimit.Minimum = new decimal(new int[] { 100, 0, 0, -2147483648});
            this.txtLowLimit.Name = "txtLowLimit";
            this.txtLowLimit.Size = new System.Drawing.Size(120, 22);
            this.txtLowLimit.TabIndex = 3;
            this.txtLowLimit.Visible = false;
            // 
            // Solenoid1txtPort
            // 
            this.Solenoid1txtPort.Location = new System.Drawing.Point(2, 42);
            this.Solenoid1txtPort.Minimum = new decimal(new int[] { 1, 0, 0, 0});
            this.Solenoid1txtPort.Name = "SolenoidPort1";
            this.Solenoid1txtPort.Size = new System.Drawing.Size(120, 22);
            this.Solenoid1txtPort.TabIndex = 2;
            this.Solenoid1txtPort.Value = new decimal(new int[] { 1, 0, 0, 0});
            this.Solenoid1txtPort.ValueChanged += new System.EventHandler(Solenoid1Change);
            // 
            // Solenoid2txtPort
            // 
            this.Solenoid2txtPort.Location = new System.Drawing.Point(123, 42);
            this.Solenoid2txtPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.Solenoid2txtPort.Name = "SolenoidPort2";
            this.Solenoid2txtPort.Size = new System.Drawing.Size(120, 22);
            this.Solenoid2txtPort.TabIndex = 1;
            this.Solenoid2txtPort.Value = new decimal(new int[] { 1, 0, 0, 0 });
            this.Solenoid2txtPort.Maximum = new decimal(new int[] { 21, 0, 0, 0 });
            this.Solenoid2txtPort.ValueChanged += new System.EventHandler(Solenoid2Change);
            // 
            // PWM1txtPort
            // 
            this.PWM1txtPort.Location = new System.Drawing.Point(2, 42);
            this.PWM1txtPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.PWM1txtPort.Maximum = new decimal(new int[] { 21, 0, 0, 0 });
            this.PWM1txtPort.Name = "PWMPort1";
            this.PWM1txtPort.Size = new System.Drawing.Size(120, 22);
            this.PWM1txtPort.TabIndex = 2;
            this.PWM1txtPort.Value = new decimal(new int[] { 1, 0, 0, 0 });
            this.PWM1txtPort.ValueChanged += new System.EventHandler(PWM1Change);
            // 
            // PWM2txtPort
            // 
            this.PWM2txtPort.Location = new System.Drawing.Point(123, 42);
            this.PWM2txtPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.PWM2txtPort.Maximum = new decimal(new int[] { 21, 0, 0, 0 });
            this.PWM2txtPort.Name = "PWMPort2";
            this.PWM2txtPort.Size = new System.Drawing.Size(120, 22);
            this.PWM2txtPort.TabIndex = 2;
            this.PWM2txtPort.Value = new decimal(new int[] { 1, 0, 0, 0 });
            this.PWM2txtPort.ValueChanged += new System.EventHandler(PWM2Change);
            // 
            // CANtxtPort1
            // 
            this.CANtxtPort1.Location = new System.Drawing.Point(2, 42);
            this.CANtxtPort1.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.CANtxtPort1.Maximum = new decimal(new int[] { 101, 0, 0, 0 });
            this.CANtxtPort1.Name = "CANPort1";
            this.CANtxtPort1.Size = new System.Drawing.Size(120, 22);
            this.CANtxtPort1.TabIndex = 2;
            this.CANtxtPort1.Value = new decimal(new int[] { 1, 0, 0, 0 });
            this.CANtxtPort1.ValueChanged += new System.EventHandler(CAN1Change);
            // 
            // CANtxtPort2
            // 
            this.CANtxtPort2.Location = new System.Drawing.Point(123, 42);
            this.CANtxtPort2.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.CANtxtPort2.Maximum = new decimal(new int[] { 101, 0, 0, 0 });
            this.CANtxtPort2.Name = "CANPort2";
            this.CANtxtPort2.Size = new System.Drawing.Size(120, 22);
            this.CANtxtPort2.TabIndex = 2;
            this.CANtxtPort2.Value = new decimal(new int[] { 1, 0, 0, 0 });
            this.CANtxtPort2.ValueChanged += new System.EventHandler(CAN2Change);
            // 
            // RelaytxtPort
            // 
            this.RelaytxtPort.Location = new System.Drawing.Point(2, 42);
            this.RelaytxtPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.RelaytxtPort.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            this.RelaytxtPort.Name = "RelayPort";
            this.RelaytxtPort.Size = new System.Drawing.Size(120, 22);
            this.RelaytxtPort.TabIndex = 2;
            this.RelaytxtPort.Value = new decimal(new int[] { 1, 0, 0, 0 });
            this.RelaytxtPort.ValueChanged += new System.EventHandler(RelayChanged);
            //
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(11, 22);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(51, 17);
            this.lblPort.TabIndex = 1;
            this.lblPort.Text = "Port ID";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(13, 247);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(320, 28);
            this.btnSave.TabIndex = 11;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(SaveButtonPressed);
            // 
            // cmbWheelType
            // 
            this.cmbWheelType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbWheelType.FormattingEnabled = true;
            this.cmbWheelType.Items.AddRange(new object[] {
            "Not a Wheel",
            "Normal",
            "Omni",
            "Mecanum"});
            this.cmbWheelType.Location = new System.Drawing.Point(10, 23);
            this.cmbWheelType.Name = "cmbWheelType";
            this.cmbWheelType.Size = new System.Drawing.Size(120, 24);
            this.cmbWheelType.TabIndex = 7;
            this.cmbWheelType.SelectedIndex = 0;
            this.cmbWheelType.SelectedIndexChanged += new System.EventHandler(driveWheelChoice);
            // 
            // cmbPneumaticPressure
            // 
            this.cmbPneumaticPressure.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPneumaticPressure.FormattingEnabled = true;
            this.cmbPneumaticPressure.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.cmbPneumaticPressure.Items.AddRange(new object[] {
            "60 psi",
            "20 psi",
            "10 psi"});
            this.cmbPneumaticPressure.Location = new System.Drawing.Point(136, 27);
            this.cmbPneumaticPressure.Name = "cmbPneumaticPressure";
            this.cmbPneumaticPressure.Size = new System.Drawing.Size(120, 24);
            this.cmbPneumaticPressure.TabIndex = 6;
            this.cmbPneumaticPressure.SelectedItem = "60 psi";
            // 
            // tabsMeta
            // 
            this.tabsMeta.Controls.Add(this.metaWheel);
            this.tabsMeta.Controls.Add(this.metaPneumatic);
            this.tabsMeta.Controls.Add(this.metaGearing);
            this.tabsMeta.Controls.Add(this.metaElevatorBrake);
            this.tabsMeta.Controls.Add(this.metaElevatorStages);
            this.tabsMeta.Location = new System.Drawing.Point(13, 153);
            this.tabsMeta.Name = "tabsMeta";
            this.tabsMeta.SelectedIndex = 0;
            this.tabsMeta.Size = new System.Drawing.Size(318, 88);
            this.tabsMeta.TabIndex = 11;
            // 
            // metaWheel
            // 
            this.metaWheel.Controls.Add(this.lblFriction);
            this.metaWheel.Controls.Add(this.lblType);
            this.metaWheel.Controls.Add(this.cmbFrictionLevel);
            this.metaWheel.Controls.Add(this.cmbWheelType);
            this.metaWheel.Location = new System.Drawing.Point(4, 25);
            this.metaWheel.Name = "metaWheel";
            this.metaWheel.Size = new System.Drawing.Size(310, 59);
            this.metaWheel.TabIndex = 0;
            this.metaWheel.Text = "Wheel";
            this.metaWheel.UseVisualStyleBackColor = true;
            // 
            // lblFriction
            // 
            this.lblFriction.AutoSize = true;
            this.lblFriction.Location = new System.Drawing.Point(133, 0);
            this.lblFriction.Name = "lblFriction";
            this.lblFriction.Size = new System.Drawing.Size(92, 17);
            this.lblFriction.TabIndex = 15;
            this.lblFriction.Text = "Friction Level";
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(7, 0);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(84, 17);
            this.lblType.TabIndex = 14;
            this.lblType.Text = "Wheel Type";
            // 
            // cmbFrictionLevel
            // 
            this.cmbFrictionLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFrictionLevel.FormattingEnabled = true;
            this.cmbFrictionLevel.Items.AddRange(new object[] {
            "High",
            "Medium",
            "Low"});
            this.cmbFrictionLevel.Location = new System.Drawing.Point(136, 23);
            this.cmbFrictionLevel.Name = "cmbFrictionLevel";
            this.cmbFrictionLevel.Size = new System.Drawing.Size(120, 24);
            this.cmbFrictionLevel.TabIndex = 13;
            this.cmbFrictionLevel.SelectedIndex = 1;
            this.cmbFrictionLevel.SelectedIndexChanged += new System.EventHandler(FrictionLevelChanged);
            // 
            // metaPneumatic
            // 
            this.metaPneumatic.Controls.Add(this.lblPressure);
            this.metaPneumatic.Controls.Add(this.cmbPneumaticPressure);
            this.metaPneumatic.Controls.Add(this.cmbPneumaticDiameter);
            this.metaPneumatic.Controls.Add(this.lblDiameter);
            this.metaPneumatic.Location = new System.Drawing.Point(4, 25);
            this.metaPneumatic.Name = "metaPneumatic";
            this.metaPneumatic.Size = new System.Drawing.Size(310, 59);
            this.metaPneumatic.TabIndex = 1;
            this.metaPneumatic.Text = "Pneumatic";
            this.metaPneumatic.UseVisualStyleBackColor = true;
            // 
            // lblPressure
            // 
            this.lblPressure.AutoSize = true;
            this.lblPressure.Location = new System.Drawing.Point(133, 7);
            this.lblPressure.Name = "lblPressure";
            this.lblPressure.Size = new System.Drawing.Size(65, 17);
            this.lblPressure.TabIndex = 13;
            this.lblPressure.Text = "Pressure";
            // 
            // cmbPneumaticDiameter
            // 
            this.cmbPneumaticDiameter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPneumaticDiameter.FormattingEnabled = true;
            this.cmbPneumaticDiameter.Items.AddRange(new object[] {
            "1 in",
            ".5 in",
            ".25 in"});
            this.cmbPneumaticDiameter.Location = new System.Drawing.Point(6, 27);
            this.cmbPneumaticDiameter.Name = "cmbPneumaticDiameter";
            this.cmbPneumaticDiameter.Size = new System.Drawing.Size(124, 24);
            this.cmbPneumaticDiameter.TabIndex = 12;
            this.cmbPneumaticDiameter.SelectedItem = ".5 in";
            this.cmbPneumaticDiameter.SelectedIndexChanged += new System.EventHandler(InternalDiameterChanged);
            // 
            // lblDiameter
            // 
            this.lblDiameter.AutoSize = true;
            this.lblDiameter.Location = new System.Drawing.Point(3, 7);
            this.lblDiameter.Name = "lblDiameter";
            this.lblDiameter.Size = new System.Drawing.Size(116, 17);
            this.lblDiameter.TabIndex = 9;
            this.lblDiameter.Text = "Internal Diameter";
            // 
            // metaGearing
            // 
            this.metaGearing.Controls.Add(this.lblOutputGear);
            this.metaGearing.Controls.Add(this.lblInputGear);
            this.metaGearing.Controls.Add(this.txtGearRationNum);
            this.metaGearing.Controls.Add(this.txtGearRationDenom);
            this.metaGearing.Location = new System.Drawing.Point(4, 25);
            this.metaGearing.Name = "metaGearing";
            this.metaGearing.Size = new System.Drawing.Size(310, 59);
            this.metaGearing.TabIndex = 2;
            this.metaGearing.Text = "Gear Ratio";
            this.metaGearing.UseVisualStyleBackColor = true;
            // 
            // lblOutputGear
            // 
            this.lblOutputGear.AutoSize = true;
            this.lblOutputGear.Location = new System.Drawing.Point(133, 9);
            this.lblOutputGear.Name = "lblOutputGear";
            this.lblOutputGear.Size = new System.Drawing.Size(78, 20);
            this.lblOutputGear.TabIndex = 14;
            this.lblOutputGear.Text = "Output Gear";
            this.lblOutputGear.UseCompatibleTextRendering = true;
            // 
            // lblInputGear
            // 
            this.lblInputGear.AutoSize = true;
            this.lblInputGear.Location = new System.Drawing.Point(7, 9);
            this.lblInputGear.Name = "lblInputGear";
            this.lblInputGear.Size = new System.Drawing.Size(75, 17);
            this.lblInputGear.TabIndex = 11;
            this.lblInputGear.Text = "Input Gear";
            // 
            // txtGearRationNum
            // 
            this.txtGearRationNum.Location = new System.Drawing.Point(10, 29);
            this.txtGearRationNum.Name = "txtGearRationNum";
            this.txtGearRationNum.Size = new System.Drawing.Size(120, 22);
            this.txtGearRationNum.TabIndex = 12;
            this.txtGearRationNum.Text = "1";
            this.txtGearRationNum.TextChanged += new System.EventHandler(InputGearChanged);
            // 
            // txtGearRationDenom
            // 
            this.txtGearRationDenom.Location = new System.Drawing.Point(136, 29);
            this.txtGearRationDenom.Name = "txtGearRationDenom";
            this.txtGearRationDenom.Size = new System.Drawing.Size(120, 22);
            this.txtGearRationDenom.TabIndex = 13;
            this.txtGearRationDenom.Text = "1";
            this.txtGearRationDenom.TextChanged += new System.EventHandler(OutputGearChanged);
            // 
            // metaElevatorBrake
            // 
            this.metaElevatorBrake.Controls.Add(this.brakePortB);
            this.metaElevatorBrake.Controls.Add(this.brakePortA);
            this.metaElevatorBrake.Controls.Add(this.lblBrakePort);
            this.metaElevatorBrake.Controls.Add(this.chkBoxHasBrake);
            this.metaElevatorBrake.Location = new System.Drawing.Point(4, 25);
            this.metaElevatorBrake.Name = "metaElevatorBrake";
            this.metaElevatorBrake.Size = new System.Drawing.Size(310, 59);
            this.metaElevatorBrake.TabIndex = 3;
            this.metaElevatorBrake.Text = "Break Info";
            this.metaElevatorBrake.UseVisualStyleBackColor = true;
            // 
            // brakePortB
            // 
            this.brakePortB.Location = new System.Drawing.Point(136, 27);
            this.brakePortB.Name = "brakePortB";
            this.brakePortB.Size = new System.Drawing.Size(120, 22);
            this.brakePortB.TabIndex = 3;
            this.brakePortB.ValueChanged += new System.EventHandler(Brake2Changed);
            // 
            // brakePortA
            // 
            this.brakePortA.Location = new System.Drawing.Point(10, 27);
            this.brakePortA.Name = "brakePortA";
            this.brakePortA.Size = new System.Drawing.Size(120, 22);
            this.brakePortA.TabIndex = 2;
            this.brakePortA.ValueChanged += new System.EventHandler(Brake1Changed);
            // 
            // lblBrakePort
            // 
            this.lblBrakePort.AutoSize = true;
            this.lblBrakePort.Location = new System.Drawing.Point(10, 3);
            this.lblBrakePort.Name = "lblBrakePort";
            this.lblBrakePort.Size = new System.Drawing.Size(75, 17);
            this.lblBrakePort.TabIndex = 1;
            this.lblBrakePort.Text = "Brake Port";
            // 
            // chkBoxHasBrake
            // 
            this.chkBoxHasBrake.AutoSize = true;
            this.chkBoxHasBrake.Location = new System.Drawing.Point(160, 3);
            this.chkBoxHasBrake.Name = "chkBoxHasBrake";
            this.chkBoxHasBrake.Size = new System.Drawing.Size(96, 21);
            this.chkBoxHasBrake.TabIndex = 0;
            this.chkBoxHasBrake.Text = "Has Brake";
            this.chkBoxHasBrake.UseVisualStyleBackColor = true;
            this.chkBoxHasBrake.Click += new System.EventHandler(btnHasBrake_Click);
            // 
            // metaElevatorStages
            // 
            this.metaElevatorStages.Controls.Add(this.cmbStages);
            this.metaElevatorStages.Location = new System.Drawing.Point(4, 25);
            this.metaElevatorStages.Name = "metaElevatorStages";
            this.metaElevatorStages.Size = new System.Drawing.Size(310, 59);
            this.metaElevatorStages.TabIndex = 4;
            this.metaElevatorStages.Text = "Stages";
            this.metaElevatorStages.UseVisualStyleBackColor = true;
            // 
            // cmbStages
            // 
            this.cmbStages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStages.FormattingEnabled = true;
            this.cmbStages.Items.AddRange(new object[] {
            "single stage elevator",
            "cascading stage one",
            "cascading stage two",
            "continuous stage one",
            "continuous stage two"});
            this.cmbStages.Location = new System.Drawing.Point(39, 17);
            this.cmbStages.Name = "cmbStages";
            this.cmbStages.Size = new System.Drawing.Size(217, 24);
            this.cmbStages.TabIndex = 0;
            this.cmbStages.SelectedIndexChanged += new System.EventHandler(StagesChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(381, 317);
            this.Controls.Add(this.tabsMeta);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.grpDriveOptions);
            this.Controls.Add(this.grpChooseDriver);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form1";
            this.Text = "Configure Joint";
            this.grpChooseDriver.ResumeLayout(false);
            this.grpDriveOptions.ResumeLayout(false);
            this.grpDriveOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtHighLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLowLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Solenoid2txtPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Solenoid1txtPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PWM1txtPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PWM2txtPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CANtxtPort1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CANtxtPort2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RelaytxtPort)).EndInit();
            this.tabsMeta.ResumeLayout(false);
            this.metaWheel.ResumeLayout(false);
            this.metaWheel.PerformLayout();
            this.metaPneumatic.ResumeLayout(false);
            this.metaPneumatic.PerformLayout();
            this.metaGearing.ResumeLayout(false);
            this.metaGearing.PerformLayout();
            this.metaElevatorBrake.ResumeLayout(false);
            this.metaElevatorBrake.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.brakePortB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.brakePortA)).EndInit();
            this.metaElevatorStages.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion
        
        private System.Windows.Forms.GroupBox grpChooseDriver;
        private System.Windows.Forms.GroupBox grpDriveOptions;
        private System.Windows.Forms.NumericUpDown Solenoid1txtPort;
        private System.Windows.Forms.NumericUpDown Solenoid2txtPort;
        private System.Windows.Forms.NumericUpDown PWM1txtPort;
        private System.Windows.Forms.NumericUpDown PWM2txtPort;
        private System.Windows.Forms.NumericUpDown CANtxtPort1;
        private System.Windows.Forms.NumericUpDown CANtxtPort2;
        private System.Windows.Forms.NumericUpDown RelaytxtPort;
        private System.Windows.Forms.Label lblPort;
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
    }
}