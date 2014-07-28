
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
            this.txtHighLimit = new System.Windows.Forms.NumericUpDown();
            this.lblLimits = new System.Windows.Forms.Label();
            this.txtLowLimit = new System.Windows.Forms.NumericUpDown();
            this.txtPortB = new System.Windows.Forms.NumericUpDown();
            this.lblPort = new System.Windows.Forms.Label();
            this.txtPortA = new System.Windows.Forms.NumericUpDown();
            this.btnSave = new System.Windows.Forms.Button();
            this.grpWheelOptions = new System.Windows.Forms.GroupBox();
            this.cmbFrictionLevel = new System.Windows.Forms.ComboBox();
            this.cmbWheelType = new System.Windows.Forms.ComboBox();
            this.grpPneumaticSpecs = new System.Windows.Forms.GroupBox();
            this.lblPneumaticForceTell = new System.Windows.Forms.Label();
            this.lblPneumaticVelocityTell = new System.Windows.Forms.Label();
            this.txtPneumaticForce = new System.Windows.Forms.TextBox();
            this.lblForce = new System.Windows.Forms.Label();
            this.lblVelocity = new System.Windows.Forms.Label();
            this.txtPneumaticVelocity = new System.Windows.Forms.TextBox();
            this.grpGearRatio = new System.Windows.Forms.GroupBox();
            this.txtGearRationNum = new System.Windows.Forms.TextBox();
            this.txtGearRationDenom = new System.Windows.Forms.TextBox();
            this.lblOver = new System.Windows.Forms.Label();
            this.grpChooseDriver.SuspendLayout();
            this.grpDriveOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtHighLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLowLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPortB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPortA)).BeginInit();
            this.grpWheelOptions.SuspendLayout();
            this.grpPneumaticSpecs.SuspendLayout();
            this.grpGearRatio.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmbJointDriver
            // 
            this.cmbJointDriver.FormattingEnabled = true;
            this.cmbJointDriver.Location = new System.Drawing.Point(15, 21);
            this.cmbJointDriver.Name = "cmbJointDriver";
            this.cmbJointDriver.Size = new System.Drawing.Size(317, 24);
            this.cmbJointDriver.TabIndex = 0;
            this.cmbJointDriver.SelectedIndexChanged += new System.EventHandler(this.cmbJointDriver_SelectedIndexChanged);
            // 
            // grpChooseDriver
            // 
            this.grpChooseDriver.Controls.Add(this.cmbJointDriver);
            this.grpChooseDriver.Location = new System.Drawing.Point(12, 12);
            this.grpChooseDriver.Name = "grpChooseDriver";
            this.grpChooseDriver.Size = new System.Drawing.Size(338, 56);
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
            this.grpDriveOptions.Size = new System.Drawing.Size(337, 128);
            this.grpDriveOptions.TabIndex = 2;
            this.grpDriveOptions.TabStop = false;
            this.grpDriveOptions.Text = "Joint Options";
            this.grpDriveOptions.Enter += new System.EventHandler(this.grpDriveOptions_Enter);
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
            this.txtHighLimit.TabIndex = 5;
            // 
            // lblLimits
            // 
            this.lblLimits.AutoSize = true;
            this.lblLimits.Location = new System.Drawing.Point(11, 72);
            this.lblLimits.Name = "lblLimits";
            this.lblLimits.Size = new System.Drawing.Size(78, 17);
            this.lblLimits.TabIndex = 4;
            this.lblLimits.Text = "Joint Limits";
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
            this.txtLowLimit.ValueChanged += new System.EventHandler(this.txtLowLimit_ValueChanged);
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
            this.txtPortB.ValueChanged += new System.EventHandler(this.txtPortB_ValueChanged);
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(11, 22);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(51, 17);
            this.lblPort.TabIndex = 1;
            this.lblPort.Text = "Port ID";
            this.lblPort.Click += new System.EventHandler(this.lblPort_Click);
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
            this.txtPortA.TabIndex = 0;
            this.txtPortA.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtPortA.ValueChanged += new System.EventHandler(this.txtPortA_ValueChanged);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(11, 334);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(338, 28);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // grpWheelOptions
            // 
            this.grpWheelOptions.Controls.Add(this.cmbFrictionLevel);
            this.grpWheelOptions.Controls.Add(this.cmbWheelType);
            this.grpWheelOptions.Location = new System.Drawing.Point(11, 209);
            this.grpWheelOptions.Name = "grpWheelOptions";
            this.grpWheelOptions.Size = new System.Drawing.Size(339, 63);
            this.grpWheelOptions.TabIndex = 6;
            this.grpWheelOptions.TabStop = false;
            this.grpWheelOptions.Text = "Wheel Type";
            this.grpWheelOptions.Enter += new System.EventHandler(this.grpWheelOptions_Enter);
            // 
            // cmbFrictionLevel
            // 
            this.cmbFrictionLevel.FormattingEnabled = true;
            this.cmbFrictionLevel.Items.AddRange(new object[] {
            "High",
            "Medium",
            "Banana"});
            this.cmbFrictionLevel.Location = new System.Drawing.Point(171, 21);
            this.cmbFrictionLevel.Name = "cmbFrictionLevel";
            this.cmbFrictionLevel.Size = new System.Drawing.Size(149, 24);
            this.cmbFrictionLevel.TabIndex = 1;
            this.cmbFrictionLevel.Text = "Friction Level";
            this.cmbFrictionLevel.SelectedIndexChanged += new System.EventHandler(this.cmbFrictionLevel_SelectedIndexChanged);
            // 
            // cmbWheelType
            // 
            this.cmbWheelType.FormattingEnabled = true;
            this.cmbWheelType.Items.AddRange(new object[] {
            "Not a Wheel",
            "Normal",
            "Omni",
            "Mecanum"});
            this.cmbWheelType.Location = new System.Drawing.Point(16, 21);
            this.cmbWheelType.Name = "cmbWheelType";
            this.cmbWheelType.Size = new System.Drawing.Size(149, 24);
            this.cmbWheelType.TabIndex = 0;
            this.cmbWheelType.Text = "Wheel Type";
            this.cmbWheelType.SelectedIndexChanged += new System.EventHandler(this.cmbWheelType_SelectedIndexChanged);
            // 
            // grpPneumaticSpecs
            // 
            this.grpPneumaticSpecs.Controls.Add(this.lblPneumaticForceTell);
            this.grpPneumaticSpecs.Controls.Add(this.lblPneumaticVelocityTell);
            this.grpPneumaticSpecs.Controls.Add(this.txtPneumaticForce);
            this.grpPneumaticSpecs.Controls.Add(this.lblForce);
            this.grpPneumaticSpecs.Controls.Add(this.lblVelocity);
            this.grpPneumaticSpecs.Controls.Add(this.txtPneumaticVelocity);
            this.grpPneumaticSpecs.Location = new System.Drawing.Point(11, 209);
            this.grpPneumaticSpecs.Name = "grpPneumaticSpecs";
            this.grpPneumaticSpecs.Size = new System.Drawing.Size(339, 63);
            this.grpPneumaticSpecs.TabIndex = 2;
            this.grpPneumaticSpecs.TabStop = false;
            this.grpPneumaticSpecs.Text = "Pneumatic Specifications";
            // 
            // lblPneumaticForceTell
            // 
            this.lblPneumaticForceTell.AutoSize = true;
            this.lblPneumaticForceTell.Location = new System.Drawing.Point(289, 38);
            this.lblPneumaticForceTell.Name = "lblPneumaticForceTell";
            this.lblPneumaticForceTell.Size = new System.Drawing.Size(44, 17);
            this.lblPneumaticForceTell.TabIndex = 5;
            this.lblPneumaticForceTell.Text = "Force";
            // 
            // lblPneumaticVelocityTell
            // 
            this.lblPneumaticVelocityTell.AutoSize = true;
            this.lblPneumaticVelocityTell.Location = new System.Drawing.Point(113, 39);
            this.lblPneumaticVelocityTell.Name = "lblPneumaticVelocityTell";
            this.lblPneumaticVelocityTell.Size = new System.Drawing.Size(57, 17);
            this.lblPneumaticVelocityTell.TabIndex = 4;
            this.lblPneumaticVelocityTell.Text = "Velocity";
            // 
            // txtPneumaticForce
            // 
            this.txtPneumaticForce.Location = new System.Drawing.Point(183, 21);
            this.txtPneumaticForce.Name = "txtPneumaticForce";
            this.txtPneumaticForce.Size = new System.Drawing.Size(100, 22);
            this.txtPneumaticForce.TabIndex = 3;
            this.txtPneumaticForce.TextChanged += new System.EventHandler(this.txtPneumaticForce_TextChanged);
            // 
            // lblForce
            // 
            this.lblForce.AutoSize = true;
            this.lblForce.Location = new System.Drawing.Point(289, 21);
            this.lblForce.Name = "lblForce";
            this.lblForce.Size = new System.Drawing.Size(26, 17);
            this.lblForce.TabIndex = 2;
            this.lblForce.Text = "psi";
            this.lblForce.Click += new System.EventHandler(this.lblForce_Click);
            // 
            // lblVelocity
            // 
            this.lblVelocity.AutoSize = true;
            this.lblVelocity.Location = new System.Drawing.Point(114, 22);
            this.lblVelocity.Name = "lblVelocity";
            this.lblVelocity.Size = new System.Drawing.Size(30, 17);
            this.lblVelocity.TabIndex = 1;
            this.lblVelocity.Text = "m/s";
            // 
            // txtPneumaticVelocity
            // 
            this.txtPneumaticVelocity.Location = new System.Drawing.Point(7, 22);
            this.txtPneumaticVelocity.Name = "txtPneumaticVelocity";
            this.txtPneumaticVelocity.Size = new System.Drawing.Size(100, 22);
            this.txtPneumaticVelocity.TabIndex = 0;
            // 
            // grpGearRatio
            // 
            this.grpGearRatio.Controls.Add(this.lblOver);
            this.grpGearRatio.Controls.Add(this.txtGearRationDenom);
            this.grpGearRatio.Controls.Add(this.txtGearRationNum);
            this.grpGearRatio.Location = new System.Drawing.Point(13, 279);
            this.grpGearRatio.Name = "grpGearRatio";
            this.grpGearRatio.Size = new System.Drawing.Size(331, 49);
            this.grpGearRatio.TabIndex = 7;
            this.grpGearRatio.TabStop = false;
            this.grpGearRatio.Text = "Gear Ratio";
            // 
            // txtGearRationNum
            // 
            this.txtGearRationNum.Location = new System.Drawing.Point(6, 20);
            this.txtGearRationNum.Name = "txtGearRationNum";
            this.txtGearRationNum.Size = new System.Drawing.Size(100, 22);
            this.txtGearRationNum.TabIndex = 0;
            // 
            // txtGearRationDenom
            // 
            this.txtGearRationDenom.Location = new System.Drawing.Point(130, 20);
            this.txtGearRationDenom.Name = "txtGearRationDenom";
            this.txtGearRationDenom.Size = new System.Drawing.Size(100, 22);
            this.txtGearRationDenom.TabIndex = 1;
            // 
            // lblOver
            // 
            this.lblOver.AutoSize = true;
            this.lblOver.Location = new System.Drawing.Point(112, 25);
            this.lblOver.Name = "lblOver";
            this.lblOver.Size = new System.Drawing.Size(12, 17);
            this.lblOver.TabIndex = 2;
            this.lblOver.Text = "/";
            this.lblOver.Click += new System.EventHandler(this.lblOver_Click);
            // 
            // DriveChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(362, 374);
            this.Controls.Add(this.grpGearRatio);
            this.Controls.Add(this.grpPneumaticSpecs);
            this.Controls.Add(this.grpWheelOptions);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.grpDriveOptions);
            this.Controls.Add(this.grpChooseDriver);
            this.Name = "DriveChooser";
            this.Text = "Configure Joint";
            this.grpChooseDriver.ResumeLayout(false);
            this.grpDriveOptions.ResumeLayout(false);
            this.grpDriveOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtHighLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLowLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPortB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPortA)).EndInit();
            this.grpWheelOptions.ResumeLayout(false);
            this.grpPneumaticSpecs.ResumeLayout(false);
            this.grpPneumaticSpecs.PerformLayout();
            this.grpGearRatio.ResumeLayout(false);
            this.grpGearRatio.PerformLayout();
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ComboBox cmbJointDriver;
    private System.Windows.Forms.GroupBox grpChooseDriver;
    private System.Windows.Forms.GroupBox grpDriveOptions;
    private System.Windows.Forms.NumericUpDown txtHighLimit;
    private System.Windows.Forms.Label lblLimits;
    private System.Windows.Forms.NumericUpDown txtLowLimit;
    private System.Windows.Forms.NumericUpDown txtPortB;
    private System.Windows.Forms.Label lblPort;
    private System.Windows.Forms.NumericUpDown txtPortA;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.GroupBox grpWheelOptions;
    private System.Windows.Forms.ComboBox cmbWheelType;
    private System.Windows.Forms.ComboBox cmbFrictionLevel;
    private System.Windows.Forms.GroupBox grpPneumaticSpecs;
    private System.Windows.Forms.TextBox txtPneumaticForce;
    private System.Windows.Forms.Label lblForce;
    private System.Windows.Forms.Label lblVelocity;
    private System.Windows.Forms.TextBox txtPneumaticVelocity;
    private System.Windows.Forms.Label lblPneumaticForceTell;
    private System.Windows.Forms.Label lblPneumaticVelocityTell;
    private System.Windows.Forms.GroupBox grpGearRatio;
    private System.Windows.Forms.TextBox txtGearRationNum;
    private System.Windows.Forms.Label lblOver;
    private System.Windows.Forms.TextBox txtGearRationDenom;
}