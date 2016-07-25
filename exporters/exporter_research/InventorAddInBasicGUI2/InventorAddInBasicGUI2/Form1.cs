using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventorAddInBasicGUI2
{
    public partial class Form1 : Form
    {

        //Write xml variables to be modified by the instacne of the button click
        //Change me later

        JointData joint;

        public Form1()
        {
            InitializeComponent();
        }
        public void MotorChosen()
        {
            Solenoid1txtPort.Hide();
            Solenoid2txtPort.Hide();
            RelaytxtPort.Hide();
            if (rbPWM.Checked)
            {
                lblPort.Text = "PWM Port";
                CANtxtPort1.Hide();
                CANtxtPort2.Hide();
                PWM1txtPort.Show();
                PWM2txtPort.Show();
            }
            else
            {
                CANtxtPort1.Show();
                CANtxtPort2.Show();
                PWM1txtPort.Hide();
                PWM2txtPort.Hide();
                lblPort.Text = "CAN Port";
            }
            grpChooseDriver.Hide();
            tabsMeta.Visible = true;
            tabsMeta.TabPages.Clear();
            tabsMeta.TabPages.Add(metaWheel);
            tabsMeta.TabPages.Add(metaGearing);
            chkBoxDriveWheel.Show();
            btnSave.Show();
            rbCAN.Show();
            if (joint.Wheel != WheelType.NotAWheel)
            {
                cmbFrictionLevel.Show();
            } else
            {
                cmbFrictionLevel.Hide();
            }
            rbPWM.Show();
            
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
            btnSave.Location = new System.Drawing.Point(9, 190);
            ClientSize = new System.Drawing.Size(340, 225);
        }
        public void ServoChosen()
        {

            grpChooseDriver.Hide();
            tabsMeta.Visible = false;
            tabsMeta.TabPages.Clear();
            rbPWM.Show();
            chkBoxDriveWheel.Hide();
            ClientSize = new System.Drawing.Size(340, 130);
            btnSave.Show();
            btnSave.Location = new System.Drawing.Point(9, 95);
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            rbCAN.Hide();
            rbPWM.Hide();
            lblPort.Text = "PWM Port";
            RelaytxtPort.Hide();
            Solenoid1txtPort.Hide();
            Solenoid2txtPort.Hide();
            CANtxtPort1.Hide();
            CANtxtPort2.Hide();
            PWM1txtPort.Show();
            PWM2txtPort.Hide();
        }
        public void BumperPneumaticChosen()
        {
            grpChooseDriver.Hide();
            tabsMeta.Visible = true;
            tabsMeta.TabPages.Clear();
            tabsMeta.TabPages.Add(metaPneumatic);
            rbPWM.Hide();
            chkBoxDriveWheel.Hide();
            ClientSize = new System.Drawing.Size(340, 225);
            btnSave.Show();
            btnSave.Location = new System.Drawing.Point(9,190);
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
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
        public void RelayPneumaticChosen()
        {
            grpChooseDriver.Hide();
            tabsMeta.Visible = true;
            tabsMeta.TabPages.Clear();
            tabsMeta.TabPages.Add(metaPneumatic);
            rbPWM.Hide();
            chkBoxDriveWheel.Hide();
            ClientSize = new System.Drawing.Size(340, 225);
            btnSave.Show();
            btnSave.Location = new System.Drawing.Point(9, 190);
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
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
        public void WormScrewChosen()
        {
            Solenoid1txtPort.Hide();
            Solenoid2txtPort.Hide();
            RelaytxtPort.Hide();
            if (rbPWM.Checked)
            {
                lblPort.Text = "PWM Port";
                CANtxtPort1.Hide();
                CANtxtPort2.Hide();
                PWM1txtPort.Show();
                PWM2txtPort.Show();
            }
            else
            {
                CANtxtPort1.Show();
                CANtxtPort2.Show();
                PWM1txtPort.Hide();
                PWM2txtPort.Hide();
                lblPort.Text = "CAN Port";
            }
            grpChooseDriver.Hide();
            tabsMeta.Visible = false;
            tabsMeta.TabPages.Clear();
            rbPWM.Show();
            rbCAN.Show();
            chkBoxDriveWheel.Hide();
            ClientSize = new System.Drawing.Size(340, 130);
            btnSave.Show();
            btnSave.Location = new System.Drawing.Point(9, 95);
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
        }
        public void DualMotorChosen()
        {
            Solenoid1txtPort.Hide();
            Solenoid2txtPort.Hide();
            RelaytxtPort.Hide();
            if (rbPWM.Checked)
            {
                lblPort.Text = "PWM Port";
                CANtxtPort1.Hide();
                CANtxtPort2.Hide();
                PWM1txtPort.Show();
                PWM2txtPort.Show();
            }
            else
            {
                CANtxtPort1.Show();
                CANtxtPort2.Show();
                PWM1txtPort.Hide();
                PWM2txtPort.Hide();
                lblPort.Text = "CAN Port";
            }
            grpChooseDriver.Hide();
            tabsMeta.Visible = true;
            tabsMeta.TabPages.Clear();
            tabsMeta.TabPages.Add(metaWheel);
            tabsMeta.TabPages.Add(metaGearing);
            chkBoxDriveWheel.Show();
            btnSave.Show();
            rbCAN.Show();
            rbPWM.Show();
            if (joint.Wheel != WheelType.NotAWheel)
            {
                cmbFrictionLevel.Show();
            }
            else
            {
                cmbFrictionLevel.Hide();
            }
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
            btnSave.Location = new System.Drawing.Point(9, 190);
            ClientSize = new System.Drawing.Size(340, 225);
        }
        public void ElevatorChosen()
        {
            Solenoid1txtPort.Hide();
            Solenoid2txtPort.Hide();
            RelaytxtPort.Hide();
            if (rbPWM.Checked)
            {
                lblPort.Text = "PWM Port";
                CANtxtPort1.Hide();
                CANtxtPort2.Hide();
                PWM1txtPort.Show();
                PWM2txtPort.Show();
            }
            else
            {
                CANtxtPort1.Show();
                CANtxtPort2.Show();
                PWM1txtPort.Hide();
                PWM2txtPort.Hide();
                lblPort.Text = "CAN Port";
            }
            tabsMeta.Visible = true;
            lblBrakePort.Enabled = false;
            rbCAN.Show();
            chkBoxDriveWheel.Hide();
            rbPWM.Show();
            
            brakePortA.Enabled = chkBoxHasBrake.Checked;
            brakePortB.Enabled = chkBoxHasBrake.Checked;
            tabsMeta.TabPages.Clear();
            chkBoxHasBrake.Show();
            tabsMeta.TabPages.Add(metaElevatorBrake);
            tabsMeta.TabPages.Add(metaElevatorStages);
            tabsMeta.TabPages.Add(metaGearing);
            ClientSize = new System.Drawing.Size(340, 225);
            btnSave.Show();
            btnSave.Location = new System.Drawing.Point(9, 190);
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
        }
        public void SaveButtonPressed(object sender, EventArgs e)
        {
            this.Close();
        }
        public void btnPWM_Click(object sender, EventArgs e)
        {
            lblPort.Text = "PWM Port";
            joint.PWM = true;
        }
        public void btnHasBrake_Click(object sender, EventArgs e)
        {
            if (chkBoxHasBrake.Checked == true)
            {
                brakePortB.Enabled = true;
                brakePortA.Enabled = true;
                joint.HasBrake = true;
            } else
            {
                brakePortB.Enabled = false;
                brakePortA.Enabled = false;
                joint.HasBrake = false;
            }
        }
        public void btnCAN_Click(object sender, EventArgs e)
        {
            lblPort.Text = "CAN Port";
            joint.PWM = false;
        }
        public void driveWheelChoice(object sender, EventArgs e)
        {

            if (cmbWheelType.SelectedIndex == 1)
            {
                joint.Wheel = WheelType.Normal;
            } else if(cmbWheelType.SelectedIndex == 2)
            {
                joint.Wheel = WheelType.Omni;
            } else if (cmbWheelType.SelectedIndex == 3)
            {
                joint.Wheel = WheelType.Mecanum;
            } else
            {
                joint.Wheel = WheelType.NotAWheel;
            }
            if (joint.Wheel != WheelType.NotAWheel)
            {
                cmbFrictionLevel.Show();
            }
            else
            {
                cmbFrictionLevel.Hide();
            }
        }
        public void FrictionLevelChanged(object sender, EventArgs e)
        {
            if (cmbFrictionLevel.SelectedIndex == 0)
            {
                joint.Friction = FrictionLevel.Low;
            }
            else if (cmbFrictionLevel.SelectedIndex == 2)
            {
                joint.Friction = FrictionLevel.High;
            }
            else
            {
                joint.Friction = FrictionLevel.Medium;
            }
        }
        public void StagesChanged(object sender, EventArgs e)
        {
            if (cmbStages.SelectedIndex == 1)
            {
                joint.Stages = Stages.CascadingStageOne;
            }
            else if (cmbStages.SelectedIndex == 2)
            {
                joint.Stages = Stages.CascadingStageTwo;
            }
            else if (cmbStages.SelectedIndex == 3)
            {
                joint.Stages = Stages.ContinuousStage1;
            }
            else if (cmbStages.SelectedIndex == 4)
            {
                joint.Stages = Stages.ContinuousStage2;
            }
            else
            {
                joint.Stages = Stages.SingleStageElevator;
            }
        }
        public void InternalDiameterChanged(object sender, EventArgs e)
        {
            if (cmbPneumaticDiameter.SelectedItem .Equals("1 in"))
            {
                 joint.Diameter = InternalDiameter.One;
            }
            else if (cmbPneumaticDiameter.SelectedItem.Equals(".25 in"))
            {
                 joint.Diameter = InternalDiameter.PointTwoFive;
            }
            else
            {
                joint.Diameter = InternalDiameter.PointFive;
            }
        }
        public void PressureDiameterChanged(object sender, EventArgs e)
        {
            if (cmbPneumaticPressure.SelectedItem.Equals("10 psi"))
            {
                joint.Pressure = Pressure.psi10;
            } else if (cmbPneumaticPressure.SelectedItem.Equals("20 psi"))
            {
                joint.Pressure = Pressure.psi20;
            } else
            {
                joint.Pressure = Pressure.psi60;
            }
        }
        public void Solenoid1Change(object sender, EventArgs e)
        {
            joint.SolenoidPortA = (double) Solenoid1txtPort.Value;
        }
        public void Solenoid2Change(object sender, EventArgs e)
        {
            joint.SolenoidPortB = (double)Solenoid2txtPort.Value;
        }
        public void PWM1Change(object sender, EventArgs e)
        {
            joint.PWMport = (double)PWM1txtPort.Value;
        }
        public void PWM2Change(object sender, EventArgs e)
        {
            joint.PWMport2 = (double)PWM2txtPort.Value;

        }
        public void CAN1Change(object sender, EventArgs e)
        {
            joint.CANport = (double)CANtxtPort1.Value;
        }
        public void CAN2Change(object sender, EventArgs e)
        {
            joint.CANport2 = (double)CANtxtPort2.Value;
        }
        public void Brake1Changed(object sender, EventArgs e)
        {
            joint.BrakePortA = (double)brakePortA.Value;
        }
        public void Brake2Changed(object sender, EventArgs e)
        {
            joint.BrakePortB = (double)brakePortB.Value;
        }
        public void RelayChanged(object sender, EventArgs e)
        {
            joint.RelayPort = (double)RelaytxtPort.Value;
        }
        public void InputGearChanged(object sender, EventArgs e)
        {
            try
            {
                joint.InputGear = Convert.ToDouble(txtGearRationNum.Text);
            }
            catch
            {
                if (txtGearRationNum.Text.Length > 0)
                {
                    MessageBox.Show("warning, incorrect input");
                }
            }
        }
        public void OutputGearChanged(object sender, EventArgs e)
        {
            try
            {
                joint.OutputGear = Convert.ToDouble(txtGearRationDenom.Text);
            }
            catch
            {
                if (txtGearRationDenom.Text.Length > 0) {
                    MessageBox.Show("warning, incorrect input");
                }
            }
        }
        public void readFromData(JointData j)
        {
            joint = j;
            if (j.Wheel == WheelType.Normal)
            {
                this.cmbWheelType.SelectedIndex = 1;
                cmbFrictionLevel.Show();
            } else if (j.Wheel == WheelType.Omni)
            {
                this.cmbWheelType.SelectedIndex = 2;
                cmbFrictionLevel.Show();
            } else if (j.Wheel == WheelType.Mecanum)
            {
                this.cmbWheelType.SelectedIndex = 3;
                cmbFrictionLevel.Show();
            } else
            {
                this.cmbWheelType.SelectedIndex = 0;
                cmbFrictionLevel.Hide();
            }

            if(j.Friction == FrictionLevel.Low)
            {
                this.cmbFrictionLevel.SelectedIndex = 0;
            } 
            else if(j.Friction == FrictionLevel.High)
            {
                this.cmbFrictionLevel.SelectedIndex = 2;
            } else
            {
                this.cmbFrictionLevel.SelectedIndex = 1;
            }

            if(j.Diameter == InternalDiameter.One)
            {
                this.cmbPneumaticDiameter.SelectedItem = "1 in";
            } else if(j.Diameter == InternalDiameter.PointTwoFive)
            {
                this.cmbPneumaticDiameter.SelectedItem = ".25 in";
            } else
            {
                this.cmbPneumaticDiameter.SelectedItem = ".5 in";
            }

            if(j.Pressure == Pressure.psi10)
            {
                this.cmbPneumaticPressure.SelectedItem = "10 psi";
            } else if(j.Pressure == Pressure.psi20)
            {
                this.cmbPneumaticPressure.SelectedItem = "20 psi";
            } else
            {
                this.cmbPneumaticPressure.SelectedItem = "60 psi";
            }

            if(j.Stages == Stages.CascadingStageOne)
            {
                this.cmbStages.SelectedIndex = 1;
            } else if (j.Stages == Stages.CascadingStageTwo)
            {
                this.cmbStages.SelectedIndex = 2;
            } else if(j.Stages == Stages.ContinuousStage1)
            {
                this.cmbStages.SelectedIndex = 3;
            } else if (j.Stages == Stages.ContinuousStage2)
            {
                this.cmbStages.SelectedIndex = 4;
            } else
            {
                this.cmbStages.SelectedIndex = 0;
            }

            this.PWM1txtPort.Value = (decimal) j.PWMport;

            this.PWM2txtPort.Value = (decimal) j.PWMport2;

            this.CANtxtPort1.Value = (decimal) j.CANport;

            this.CANtxtPort2.Value = (decimal) j.CANport2;
            
            chkBoxDriveWheel.Checked = j.DriveWheel;

            rbPWM.Checked = j.PWM;
            rbCAN.Checked = !j.PWM;

            txtGearRationNum.Text = j.InputGear.ToString();

            txtGearRationDenom.Text = j.OutputGear.ToString();

            Solenoid1txtPort.Value = (decimal) j.SolenoidPortA;

            Solenoid2txtPort.Value = (decimal) j.SolenoidPortB;

            RelaytxtPort.Value = (decimal) j.RelayPort;
            
            chkBoxHasBrake.Checked = j.HasBrake;

            brakePortA.Value = (decimal) j.BrakePortA;

            brakePortB.Value = (decimal) j.BrakePortB;
        }
    }
}