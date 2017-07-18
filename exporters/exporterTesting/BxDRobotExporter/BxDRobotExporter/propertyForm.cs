using System;
using System.Windows.Forms;

namespace BxDRobotExporter {
    public partial class PropertyForm : Form {

        JointData joint;
        bool twoChoices;
        public PropertyForm() {
            InitializeComponent();
        }
        public void MotorChosen() {
            twoChoices = false;
            Solenoid1txtPort.Hide();
            Solenoid2txtPort.Hide();
            RelaytxtPort.Hide();
            if (rbPWM.Checked) {
                lblPort.Text = "PWM Port";
                CANtxtPort1.Hide();
                CANtxtPort2.Hide();
                PWM1txtPort.Show();
                PWM2txtPort.Show();
            }
            else {
                CANtxtPort1.Show();
                CANtxtPort2.Hide();
                PWM1txtPort.Hide();
                PWM2txtPort.Hide();
                lblPort.Text = "CAN Port";
            }
            grpChooseDriver.Hide();
            tabsMeta.Visible = true;
            tabsMeta.TabPages.Clear();
            tabsMeta.TabPages.Add(metaJointFriction);
            tabsMeta.TabPages.Add(metaWheel);
            tabsMeta.TabPages.Add(metaGearing);
            chkBoxDriveWheel.Show();
            btnSave.Show();
            rbCAN.Show();
            if (joint.Wheel != WheelType.NotAWheel) {
                cmbFrictionLevel.Show();
            }
            else {
                cmbFrictionLevel.Hide();
            }
            rbPWM.Show();

            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
            btnSave.Location = new System.Drawing.Point(9, 190);
            ClientSize = new System.Drawing.Size(340, 225);
        }
        public void ServoChosen() {
            twoChoices = false;
            grpChooseDriver.Hide();
            tabsMeta.TabPages.Clear();
            rbPWM.Show();
            chkBoxDriveWheel.Hide();
            btnSave.Show();
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
            btnSave.Location = new System.Drawing.Point(9, 190);
            ClientSize = new System.Drawing.Size(340, 225);
            rbCAN.Hide();
            rbPWM.Hide();
            tabsMeta.Visible = true;
            tabsMeta.TabPages.Add(metaJointFriction);
            lblPort.Text = "PWM Port";
            RelaytxtPort.Hide();
            Solenoid1txtPort.Hide();
            Solenoid2txtPort.Hide();
            CANtxtPort1.Hide();
            CANtxtPort2.Hide();
            PWM1txtPort.Show();
            PWM2txtPort.Hide();
        }
        public void BumperPneumaticChosen() {
            twoChoices = false;
            grpChooseDriver.Hide();
            tabsMeta.Visible = true;
            tabsMeta.TabPages.Clear();
            tabsMeta.TabPages.Add(metaJointFriction);
            tabsMeta.TabPages.Add(metaPneumatic);
            rbPWM.Hide();
            chkBoxDriveWheel.Hide();
            btnSave.Show();
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
            btnSave.Location = new System.Drawing.Point(9, 190);
            ClientSize = new System.Drawing.Size(340, 225);
            rbCAN.Hide();
            rbPWM.Hide();
            RelaytxtPort.Hide();
            Solenoid1txtPort.Show();
            Solenoid2txtPort.Show();
            CANtxtPort1.Hide();
            CANtxtPort2.Hide();
            PWM1txtPort.Hide();
            PWM2txtPort.Hide();
            lblPort.Text = "Solenoid Port";
        }
        public void RelayPneumaticChosen() {
            twoChoices = false;
            grpChooseDriver.Hide();
            tabsMeta.Visible = true;
            tabsMeta.TabPages.Clear();
            tabsMeta.TabPages.Add(metaJointFriction);
            tabsMeta.TabPages.Add(metaPneumatic);
            rbPWM.Hide();
            chkBoxDriveWheel.Hide();
            btnSave.Show();
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
            btnSave.Location = new System.Drawing.Point(9, 190);
            ClientSize = new System.Drawing.Size(340, 225);
            rbCAN.Hide();
            rbPWM.Hide();
            lblPort.Text = "Relay Port";
            RelaytxtPort.Show();
            Solenoid1txtPort.Hide();
            Solenoid2txtPort.Hide();
            CANtxtPort1.Hide();
            CANtxtPort2.Hide();
            PWM1txtPort.Hide();
            PWM2txtPort.Hide();
        }
        public void WormScrewChosen() {
            twoChoices = false;
            Solenoid1txtPort.Hide();
            Solenoid2txtPort.Hide();
            RelaytxtPort.Hide();
            if (rbPWM.Checked) {
                lblPort.Text = "PWM Port";
                CANtxtPort1.Hide();
                CANtxtPort2.Hide();
                PWM1txtPort.Show();
                PWM2txtPort.Hide();
            }
            else {
                CANtxtPort1.Show();
                CANtxtPort2.Hide();
                PWM1txtPort.Hide();
                PWM2txtPort.Hide();
                lblPort.Text = "CAN Port";
            }
            grpChooseDriver.Hide();
            tabsMeta.TabPages.Clear();
            tabsMeta.Visible = true;
            tabsMeta.TabPages.Add(metaJointFriction);
            rbPWM.Show();
            rbCAN.Show();
            chkBoxDriveWheel.Hide();
            btnSave.Show();
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
            btnSave.Location = new System.Drawing.Point(9, 190);
            ClientSize = new System.Drawing.Size(340, 225);
        }
        public void DualMotorChosen() {
            twoChoices = true;
            Solenoid1txtPort.Hide();
            Solenoid2txtPort.Hide();
            RelaytxtPort.Hide();
            if (rbPWM.Checked) {
                lblPort.Text = "PWM Port";
                CANtxtPort1.Hide();
                CANtxtPort2.Hide();
                PWM1txtPort.Show();
                PWM2txtPort.Show();
            }
            else {
                CANtxtPort1.Show();
                CANtxtPort2.Show();
                PWM1txtPort.Hide();
                PWM2txtPort.Hide();
                lblPort.Text = "CAN Port";
            }
            grpChooseDriver.Hide();
            tabsMeta.TabPages.Clear();
            tabsMeta.TabPages.Add(metaJointFriction);
            tabsMeta.TabPages.Add(metaWheel);
            tabsMeta.TabPages.Add(metaGearing);
            chkBoxDriveWheel.Show();
            btnSave.Show();
            rbCAN.Show();
            rbPWM.Show();
            if (joint.Wheel != WheelType.NotAWheel) {
                cmbFrictionLevel.Show();
            }
            else {
                cmbFrictionLevel.Hide();
            }
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
            btnSave.Location = new System.Drawing.Point(9, 190);
            ClientSize = new System.Drawing.Size(340, 225);
        }
        public void ElevatorChosen() {
            twoChoices = false;
            Solenoid1txtPort.Hide();
            Solenoid2txtPort.Hide();
            RelaytxtPort.Hide();
            if (rbPWM.Checked) {
                lblPort.Text = "PWM Port";
                CANtxtPort1.Hide();
                CANtxtPort2.Hide();
                PWM1txtPort.Show();
                PWM2txtPort.Hide();
            }
            else {
                CANtxtPort1.Show();
                CANtxtPort2.Hide();
                PWM1txtPort.Hide();
                PWM2txtPort.Hide();
                lblPort.Text = "CAN Port";
            }
            lblBrakePort.Enabled = false;
            rbCAN.Show();
            chkBoxDriveWheel.Hide();
            rbPWM.Show();

            brakePortA.Enabled = chkBoxHasBrake.Checked;
            brakePortB.Enabled = chkBoxHasBrake.Checked;
            tabsMeta.TabPages.Clear();
            tabsMeta.Visible = true;
            tabsMeta.TabPages.Add(metaJointFriction);
            chkBoxHasBrake.Show();
            tabsMeta.TabPages.Add(metaElevatorBrake);
            tabsMeta.TabPages.Add(metaElevatorStages);
            tabsMeta.TabPages.Add(metaGearing);
            btnSave.Show();
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
            btnSave.Location = new System.Drawing.Point(9, 190);
            ClientSize = new System.Drawing.Size(340, 225);
        }
        public void SaveButtonPressed(object sender, EventArgs e) {
            this.Close();
        }
        public void BtnPWM_Click(object sender, EventArgs e) {
            lblPort.Text = "PWM Port";
            joint.PWM = true;
            if (!twoChoices) {
                CANtxtPort1.Hide();
                CANtxtPort2.Hide();
                PWM1txtPort.Show();
                PWM2txtPort.Hide();
            }
            else {
                CANtxtPort1.Hide();
                CANtxtPort2.Hide();
                PWM1txtPort.Show();
                PWM2txtPort.Show();

            }
        }
        public void BtnHasBrake_Click(object sender, EventArgs e) {
            if (chkBoxHasBrake.Checked == true) {
                brakePortB.Enabled = true;
                brakePortA.Enabled = true;
                joint.HasBrake = true;
            }
            else {
                brakePortB.Enabled = false;
                brakePortA.Enabled = false;
                joint.HasBrake = false;
            }
        }
        public void BtnCAN_Click(object sender, EventArgs e) {
            lblPort.Text = "CAN Port";
            joint.PWM = false;
            if (!twoChoices) {
                CANtxtPort1.Show();
                CANtxtPort2.Hide();
                PWM1txtPort.Hide();
                PWM2txtPort.Hide();
            } else {
                CANtxtPort1.Show();
                CANtxtPort2.Show();
                PWM1txtPort.Hide();
                PWM2txtPort.Hide();

            }
        }
        public void DriveWheelChoice(object sender, EventArgs e) {

            if (cmbWheelType.SelectedIndex == 1) {
                joint.Wheel = WheelType.Normal;
            }
            else if (cmbWheelType.SelectedIndex == 2) {
                joint.Wheel = WheelType.Omni;
            }
            else if (cmbWheelType.SelectedIndex == 3) {
                joint.Wheel = WheelType.Mecanum;
            }
            else {
                joint.Wheel = WheelType.NotAWheel;
            }
            if (joint.Wheel != WheelType.NotAWheel) {
                cmbFrictionLevel.Show();
            }
            else {
                cmbFrictionLevel.Hide();
            }
        }
        public void FrictionLevelChanged(object sender, EventArgs e) {
            if (cmbFrictionLevel.SelectedIndex == 0) {
                joint.Friction = FrictionLevel.Low;
            }
            else if (cmbFrictionLevel.SelectedIndex == 2) {
                joint.Friction = FrictionLevel.High;
            }
            else {
                joint.Friction = FrictionLevel.Medium;
            }
        }
        public void StagesChanged(object sender, EventArgs e) {
            if (cmbStages.SelectedIndex == 1) {
                joint.Stages = Stages.CascadingStageOne;
            }
            else if (cmbStages.SelectedIndex == 2) {
                joint.Stages = Stages.CascadingStageTwo;
            }
            else if (cmbStages.SelectedIndex == 3) {
                joint.Stages = Stages.ContinuousStage1;
            }
            else if (cmbStages.SelectedIndex == 4) {
                joint.Stages = Stages.ContinuousStage2;
            }
            else {
                joint.Stages = Stages.SingleStageElevator;
            }
        }
        public void InternalDiameterChanged(object sender, EventArgs e) {
            if (cmbPneumaticDiameter.SelectedItem.Equals("1 in")) {
                joint.Diameter = InternalDiameter.One;
            }
            else if (cmbPneumaticDiameter.SelectedItem.Equals(".25 in")) {
                joint.Diameter = InternalDiameter.PointTwoFive;
            }
            else {
                joint.Diameter = InternalDiameter.PointFive;
            }
        }
        public void PressureDiameterChanged(object sender, EventArgs e) {
            if (cmbPneumaticPressure.SelectedItem.Equals("10 psi")) {
                joint.Pressure = Pressure.psi10;
            }
            else if (cmbPneumaticPressure.SelectedItem.Equals("20 psi")) {
                joint.Pressure = Pressure.psi20;
            }
            else {
                joint.Pressure = Pressure.psi60;
            }
        }
        public void Solenoid1Change(object sender, EventArgs e) {
            joint.SolenoidPortA = (double)Solenoid1txtPort.Value;
        }
        public void Solenoid2Change(object sender, EventArgs e) {
            joint.SolenoidPortB = (double)Solenoid2txtPort.Value;
        }
        public void PWM1Change(object sender, EventArgs e) {
            joint.PWMport = (double)PWM1txtPort.Value;
        }
        public void PWM2Change(object sender, EventArgs e) {
            joint.PWMport2 = (double)PWM2txtPort.Value;

        }
        public void CAN1Change(object sender, EventArgs e) {
            joint.CANport = (double)CANtxtPort1.Value;
        }
        public void CAN2Change(object sender, EventArgs e) {
            joint.CANport2 = (double)CANtxtPort2.Value;
        }
        public void Brake1Changed(object sender, EventArgs e) {
            joint.BrakePortA = (double)brakePortA.Value;
        }
        public void Brake2Changed(object sender, EventArgs e) {
            joint.BrakePortB = (double)brakePortB.Value;
        }
        public void RelayChanged(object sender, EventArgs e) {
            joint.RelayPort = (double)RelaytxtPort.Value;
        }
        public void InputGearChanged(object sender, EventArgs e) {
            try {
                joint.InputGear = Convert.ToDouble(txtGearRationNum.Text);
            }
            catch {
                if (txtGearRationNum.Text.Length > 0 && !txtGearRationNum.Text.Equals("-")) {
                    MessageBox.Show("warning, incorrect input");
                }
            }
        }
        public void JointFrictionChanged(object sender, EventArgs e) {
            try {
                joint.JointFrictionLevel = Convert.ToDouble(this.JointFrictionLevel.Text);
            }
            catch {
                if (txtGearRationNum.Text.Length > 0 && !txtGearRationNum.Text.Equals("-")) {
                    MessageBox.Show("warning, incorrect input");
                }
            }
        }
        public void BtnHasJointFriction_Click(object sender, EventArgs e) {
            if (chkBoxHasJointFriction.Checked == true) {
                JointFrictionLevel.Enabled = true;
                joint.HasJointFriction = true;
            }
            else {
                JointFrictionLevel.Enabled = false;
                joint.HasJointFriction = false;
            }
        }
        public void OutputGearChanged(object sender, EventArgs e) {
            try {
                joint.OutputGear = Convert.ToDouble(txtGearRationDenom.Text);
            }
            catch {
                if (txtGearRationDenom.Text.Length > 0 && !txtGearRationDenom.Text.Equals("-")) {
                    MessageBox.Show("warning, incorrect input");
                }
            }
        }
        public void ReadFromData(JointData j) {
            try {
                joint = j;
                if (j.Wheel == WheelType.Normal) {
                    this.cmbWheelType.SelectedIndex = 1;
                    cmbFrictionLevel.Show();
                }
                else if (j.Wheel == WheelType.Omni) {
                    this.cmbWheelType.SelectedIndex = 2;
                    cmbFrictionLevel.Show();
                }
                else if (j.Wheel == WheelType.Mecanum) {
                    this.cmbWheelType.SelectedIndex = 3;
                    cmbFrictionLevel.Show();
                }
                else {
                    this.cmbWheelType.SelectedIndex = 0;
                    cmbFrictionLevel.Hide();
                }

                if (j.Friction == FrictionLevel.Low) {
                    this.cmbFrictionLevel.SelectedIndex = 0;
                }
                else if (j.Friction == FrictionLevel.High) {
                    this.cmbFrictionLevel.SelectedIndex = 2;
                }
                else {
                    this.cmbFrictionLevel.SelectedIndex = 1;
                }
                if (j.Diameter == InternalDiameter.One) {
                    this.cmbPneumaticDiameter.SelectedItem = "1 in";
                }
                else if (j.Diameter == InternalDiameter.PointTwoFive) {
                    this.cmbPneumaticDiameter.SelectedItem = ".25 in";
                }
                else {
                    this.cmbPneumaticDiameter.SelectedItem = ".5 in";
                }

                if (j.Pressure == Pressure.psi10) {
                    this.cmbPneumaticPressure.SelectedItem = "10 psi";
                }
                else if (j.Pressure == Pressure.psi20) {
                    this.cmbPneumaticPressure.SelectedItem = "20 psi";
                }
                else {
                    this.cmbPneumaticPressure.SelectedItem = "60 psi";
                }

                if (j.Stages == Stages.CascadingStageOne) {
                    this.cmbStages.SelectedIndex = 1;
                }
                else if (j.Stages == Stages.CascadingStageTwo) {
                    this.cmbStages.SelectedIndex = 2;
                }
                else if (j.Stages == Stages.ContinuousStage1) {
                    this.cmbStages.SelectedIndex = 3;
                }
                else if (j.Stages == Stages.ContinuousStage2) {
                    this.cmbStages.SelectedIndex = 4;
                }
                else {
                    this.cmbStages.SelectedIndex = 0;
                }

                this.PWM1txtPort.Value = (decimal)j.PWMport;

                this.PWM2txtPort.Value = (decimal)j.PWMport2;

                this.CANtxtPort1.Value = (decimal)j.CANport;

                this.CANtxtPort2.Value = (decimal)j.CANport2;

                chkBoxDriveWheel.Checked = j.DriveWheel;

                rbPWM.Checked = j.PWM;
                rbCAN.Checked = !j.PWM;
                if (j.PWM) {
                    lblPort.Text = "PWM Port";
                    CANtxtPort1.Hide();
                    CANtxtPort2.Hide();
                    PWM1txtPort.Show();
                    PWM2txtPort.Hide();
                }
                else {
                    lblPort.Text = "CAN Port";
                    CANtxtPort1.Show();
                    CANtxtPort2.Hide();
                    PWM1txtPort.Hide();
                    PWM2txtPort.Hide();
                }
                txtGearRationNum.Text = j.InputGear.ToString();

                txtGearRationDenom.Text = j.OutputGear.ToString();

                Solenoid1txtPort.Value = (decimal)j.SolenoidPortA;

                Solenoid2txtPort.Value = (decimal)j.SolenoidPortB;

                RelaytxtPort.Value = (decimal)j.RelayPort;

                chkBoxHasBrake.Checked = j.HasBrake;

                brakePortA.Value = (decimal)j.BrakePortA;

                brakePortB.Value = (decimal)j.BrakePortB;

                JointFrictionLevel.Value = (decimal)j.JointFrictionLevel;

                chkBoxHasJointFriction.Checked = j.HasJointFriction;
                if (j.HasJointFriction) {
                    JointFrictionLevel.Enabled = true;
                }
                else {
                    JointFrictionLevel.Enabled = false;
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.ToString());
            }
        }
    }
}