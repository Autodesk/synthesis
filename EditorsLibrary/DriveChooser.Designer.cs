
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
            this.cmbJointDriver = new System.Windows.Forms.ComboBox();
            this.grpChooseDriver = new System.Windows.Forms.GroupBox();
            this.grpDriveOptions = new System.Windows.Forms.GroupBox();
            this.txtPortB = new System.Windows.Forms.NumericUpDown();
            this.lblPort = new System.Windows.Forms.Label();
            this.txtPortA = new System.Windows.Forms.NumericUpDown();
            this.btnSave = new System.Windows.Forms.Button();
            this.cmbWheelType = new System.Windows.Forms.ComboBox();
            this.cmbPneumaticPressure = new System.Windows.Forms.ComboBox();
            this.tabsMeta = new System.Windows.Forms.TabControl();
            this.metaWheel = new System.Windows.Forms.TabPage();
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
            this.txtHighLimit = new System.Windows.Forms.NumericUpDown();
            this.lblLimits = new System.Windows.Forms.Label();
            this.txtLowLimit = new System.Windows.Forms.NumericUpDown();
            this.lblType = new System.Windows.Forms.Label();
            this.lblFriction = new System.Windows.Forms.Label();
            this.grpChooseDriver.SuspendLayout();
            this.grpDriveOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtPortB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPortA)).BeginInit();
            this.tabsMeta.SuspendLayout();
            this.metaWheel.SuspendLayout();
            this.metaPneumatic.SuspendLayout();
            this.metaGearing.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtHighLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLowLimit)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbJointDriver
            // 
            this.cmbJointDriver.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbJointDriver.FormattingEnabled = true;
            this.cmbJointDriver.Location = new System.Drawing.Point(15, 21);
            this.cmbJointDriver.Name = "cmbJointDriver";
            this.cmbJointDriver.Size = new System.Drawing.Size(287, 24);
            this.cmbJointDriver.TabIndex = 0;
            this.cmbJointDriver.SelectedIndexChanged += new System.EventHandler(this.cmbJointDriver_SelectedIndexChanged);
            // 
            // grpChooseDriver
            // 
            this.grpChooseDriver.Controls.Add(this.cmbJointDriver);
            this.grpChooseDriver.Location = new System.Drawing.Point(12, 12);
            this.grpChooseDriver.Name = "grpChooseDriver";
            this.grpChooseDriver.Size = new System.Drawing.Size(319, 56);
            this.grpChooseDriver.TabIndex = 1;
            this.grpChooseDriver.TabStop = false;
            this.grpChooseDriver.Text = "Joint Driver";
            // 
            // grpDriveOptions
            // 
            this.grpDriveOptions.Controls.Add(this.txtHighLimit);
            this.grpDriveOptions.Controls.Add(this.lblLimits);
            this.grpDriveOptions.Controls.Add(this.txtLowLimit);
            this.grpDriveOptions.Controls.Add(this.txtPortB);
            this.grpDriveOptions.Controls.Add(this.lblPort);
            this.grpDriveOptions.Controls.Add(this.txtPortA);
            this.grpDriveOptions.Location = new System.Drawing.Point(13, 75);
            this.grpDriveOptions.Name = "grpDriveOptions";
            this.grpDriveOptions.Size = new System.Drawing.Size(318, 72);
            this.grpDriveOptions.TabIndex = 2;
            this.grpDriveOptions.TabStop = false;
            this.grpDriveOptions.Text = "Joint Options";
            // 
            // txtPortB
            // 
            this.txtPortB.Location = new System.Drawing.Point(140, 42);
            this.txtPortB.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtPortB.Name = "txtPortB";
            this.txtPortB.Size = new System.Drawing.Size(120, 22);
            this.txtPortB.TabIndex = 2;
            this.txtPortB.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
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
            // txtPortA
            // 
            this.txtPortA.Location = new System.Drawing.Point(14, 42);
            this.txtPortA.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtPortA.Name = "txtPortA";
            this.txtPortA.Size = new System.Drawing.Size(120, 22);
            this.txtPortA.TabIndex = 1;
            this.txtPortA.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(13, 247);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(320, 28);
            this.btnSave.TabIndex = 11;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
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
            this.cmbWheelType.SelectedIndexChanged += new System.EventHandler(this.cmbWheelType_SelectedIndexChanged);
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
            // 
            // tabsMeta
            // 
            this.tabsMeta.Controls.Add(this.metaWheel);
            this.tabsMeta.Controls.Add(this.metaPneumatic);
            this.tabsMeta.Controls.Add(this.metaGearing);
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
            // 
            // txtGearRationDenom
            // 
            this.txtGearRationDenom.Location = new System.Drawing.Point(136, 29);
            this.txtGearRationDenom.Name = "txtGearRationDenom";
            this.txtGearRationDenom.Size = new System.Drawing.Size(120, 22);
            this.txtGearRationDenom.TabIndex = 13;
            this.txtGearRationDenom.Text = "1";
            // 
            // txtHighLimit
            // 
            this.txtHighLimit.DecimalPlaces = 4;
            this.txtHighLimit.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.txtHighLimit.Location = new System.Drawing.Point(140, 92);
            this.txtHighLimit.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
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
            this.txtLowLimit.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.txtLowLimit.Location = new System.Drawing.Point(14, 92);
            this.txtLowLimit.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.txtLowLimit.Name = "txtLowLimit";
            this.txtLowLimit.Size = new System.Drawing.Size(120, 22);
            this.txtLowLimit.TabIndex = 3;
            this.txtLowLimit.Visible = false;
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
            // lblFriction
            // 
            this.lblFriction.AutoSize = true;
            this.lblFriction.Location = new System.Drawing.Point(133, 0);
            this.lblFriction.Name = "lblFriction";
            this.lblFriction.Size = new System.Drawing.Size(92, 17);
            this.lblFriction.TabIndex = 15;
            this.lblFriction.Text = "Friction Level";
            // 
            // DriveChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(340, 282);
            this.Controls.Add(this.tabsMeta);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.grpDriveOptions);
            this.Controls.Add(this.grpChooseDriver);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "DriveChooser";
            this.Text = "Configure Joint";
            this.grpChooseDriver.ResumeLayout(false);
            this.grpDriveOptions.ResumeLayout(false);
            this.grpDriveOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtPortB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPortA)).EndInit();
            this.tabsMeta.ResumeLayout(false);
            this.metaWheel.ResumeLayout(false);
            this.metaWheel.PerformLayout();
            this.metaPneumatic.ResumeLayout(false);
            this.metaPneumatic.PerformLayout();
            this.metaGearing.ResumeLayout(false);
            this.metaGearing.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtHighLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLowLimit)).EndInit();
            this.ResumeLayout(false);

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
}