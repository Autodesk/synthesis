using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using BxDRobotExporter.Managers;

namespace BxDRobotExporter.GUI.Editors.JointSubEditors
{
    public partial class JointDriverEditorForm : Form
    {
        public JointDriverEditorForm()
        {
            InitializeComponent();
        }

        public bool Saved;

        private JointDriverType[] typeOptions;
        private SkeletalJoint_Base joint;
        private List<RigidNode_Base> nodes;

        public void ShowDialog(SkeletalJoint_Base baseJoint, List<RigidNode_Base> nodes, Form owner)
        {
            Saved = false;

            if (nodes.Count > 1)
            {
                bool same = true;

                foreach (RigidNode_Base node in nodes)
                {
                    JointDriver driver = node.GetSkeletalJoint().cDriver;
                    if (driver == null || driver.CompareTo(baseJoint.cDriver) != 0) 
                        same = false;
                }

                if (same) joint = baseJoint;
                else joint = SkeletalJoint_Base.JOINT_FACTORY(baseJoint.GetJointType());
            }
            else joint = baseJoint;
            this.nodes = nodes;
            typeOptions = JointDriver.GetAllowedDrivers(joint);

            // Used for capitalization
            TextInfo textInfo = new CultureInfo("en-US", true).TextInfo;

            cmbJointDriver.Items.Clear();
            cmbJointDriver.Items.Add("No Driver");
            foreach (JointDriverType type in typeOptions)
            {
                cmbJointDriver.Items.Add(textInfo.ToTitleCase(Enum.GetName(typeof(JointDriverType), type).Replace('_', ' ').ToLowerInvariant()));
            }
            if (joint.cDriver != null)
            {
                cmbJointDriver.SelectedIndex = Array.IndexOf(typeOptions, joint.cDriver.GetDriveType()) + 1;

                if (joint.cDriver.port1 < txtPort1.Minimum)
                    txtPort1.Value = txtPort1.Minimum;
                else if (joint.cDriver.port1 > txtPort1.Maximum)
                    txtPort1.Value = txtPort1.Maximum;
                else
                    txtPort1.Value = joint.cDriver.port1;

                if (joint.cDriver.port2 < txtPort2.Minimum)
                    txtPort2.Value = txtPort2.Minimum;
                else if (joint.cDriver.port2 > txtPort2.Maximum)
                    txtPort2.Value = txtPort2.Maximum;
                else
                    txtPort2.Value = joint.cDriver.port2;

                txtLowLimit.Value = (decimal)joint.cDriver.lowerLimit;
                txtHighLimit.Value = (decimal)joint.cDriver.upperLimit;

                rbPWM.Checked = !joint.cDriver.isCan;
                rbCAN.Checked = joint.cDriver.isCan;
                chkBoxHasBrake.Checked = joint.cDriver.hasBrake;
                if (joint.cDriver.OutputGear == 0)// prevents output gear from being 0
                {
                    joint.cDriver.OutputGear = 1;
                }
                if (joint.cDriver.InputGear == 0)// prevents input gear from being 0
                {
                    joint.cDriver.InputGear = 1;
                }
                OutputGeartxt.Value = (decimal) joint.cDriver.OutputGear;// reads the existing gearing and writes it to the input field so the user sees their existing value
                InputGeartxt.Value = (decimal) joint.cDriver.InputGear;// reads the existing gearing and writes it to the input field so the user sees their existing value

                {
                    PneumaticDriverMeta pneumaticMeta = joint.cDriver.GetInfo<PneumaticDriverMeta>();
                    if (pneumaticMeta != null)
                    {
                        numericUpDownPnuDia.Value = (decimal)pneumaticMeta.width;
                        cmbPneumaticPressure.SelectedIndex = (int)pneumaticMeta.pressureEnum;
                    }
                    else
                    {
                        numericUpDownPnuDia.Value = (decimal)1.0;
                        cmbPneumaticPressure.SelectedIndex = (int)PneumaticPressure.HIGH;
                    }
                }
                {
                    WheelDriverMeta wheelMeta = joint.cDriver.GetInfo<WheelDriverMeta>();
                    if (wheelMeta != null)
                    {
                        try
                        {
                            cmbWheelType.SelectedIndex = (int)wheelMeta.type;
                            cmbFrictionLevel.SelectedIndex = (int)wheelMeta.GetFrictionLevel();
                        }
                        catch
                        {
                            // If an exception was thrown (System.ArguementOutOfRangeException) it means
                            // the user did not choose a wheel type when they were configuring the 
                            // wheel joint
                            cmbWheelType.SelectedIndex = (int)WheelType.NORMAL;
                            cmbFrictionLevel.SelectedIndex = (int)FrictionLevel.MEDIUM;
                        }

                        chkBoxDriveWheel.Checked = wheelMeta.isDriveWheel;
                        cmbWheelType_SelectedIndexChanged(null, null);
                    }
                    else
                    {
                        cmbWheelType.SelectedIndex = (int)WheelType.NOT_A_WHEEL;
                        cmbFrictionLevel.SelectedIndex = (int)FrictionLevel.MEDIUM;
                    }
                }
                {
                    ElevatorDriverMeta elevatorMeta = joint.cDriver.GetInfo<ElevatorDriverMeta>();
                
                }
                {
                    switch (joint.cDriver.motor)
                    {
                        case MotorType.GENERIC:
                            RobotCompetitionDropDown.SelectedItem = RobotDataManager.PluginSettings.DefaultRobotCompetition.ToString();
                            MotorTypeDropDown.SelectedItem = "GENERIC";
                            break;
                        case MotorType.CIM:
                            RobotCompetitionDropDown.SelectedItem = "FRC";
                            MotorTypeDropDown.SelectedItem = "CIM";
                            break;
                        case MotorType.MINI_CIM:
                            RobotCompetitionDropDown.SelectedItem = "FRC";
                            MotorTypeDropDown.SelectedItem = "MIN_CIM";
                            break;
                        case MotorType.BAG_MOTOR:
                            RobotCompetitionDropDown.SelectedItem = "FRC";
                            MotorTypeDropDown.SelectedItem = "BAG_MOTOR";
                            break;
                        case MotorType.REDLINE_775_PRO:
                            RobotCompetitionDropDown.SelectedItem = "FRC";
                            MotorTypeDropDown.SelectedItem = "REDLINE_775_PRO";
                            break;
                        case MotorType.ANDYMARK_9015:
                            RobotCompetitionDropDown.SelectedItem = "FRC";
                            MotorTypeDropDown.SelectedItem = "ANDYMARK_9015";
                            break;
                        case MotorType.BANEBOTS_775_18v:
                            RobotCompetitionDropDown.SelectedItem = "FRC";
                            MotorTypeDropDown.SelectedItem = "BANEBOTS_775_18v";
                            break;
                        case MotorType.BANEBOTS_775_12v:
                            RobotCompetitionDropDown.SelectedItem = "FRC";
                            MotorTypeDropDown.SelectedItem = "BANEBOTS_775_12v";
                            break;
                        case MotorType.BANEBOTS_550_12v:
                            RobotCompetitionDropDown.SelectedItem = "FRC";
                            MotorTypeDropDown.SelectedItem = "BANEBOTS_550_12v";
                            break;
                        case MotorType.ANDYMARK_775_125:
                            RobotCompetitionDropDown.SelectedItem = "FRC";
                            MotorTypeDropDown.SelectedItem = "ANDYMARK_775_125";
                            break;
                        case MotorType.SNOW_BLOWER:
                            RobotCompetitionDropDown.SelectedItem = "FRC";
                            MotorTypeDropDown.SelectedItem = "SNOW_BLOWER";
                            break;
                        case MotorType.NIDEC_BLDC:
                            RobotCompetitionDropDown.SelectedItem = "FRC";
                            MotorTypeDropDown.SelectedItem = "NIDEC_BLDC";
                            break;
                        case MotorType.THROTTLE_MOTOR:
                            RobotCompetitionDropDown.SelectedItem = "FRC";
                            MotorTypeDropDown.SelectedItem = "THROTTLE_MOTOR";
                            break;
                        case MotorType.WINDOW_MOTOR:
                            RobotCompetitionDropDown.SelectedItem = "FRC";
                            MotorTypeDropDown.SelectedItem = "WINDOW_MOTOR";
                            break;
                        case MotorType.NEVEREST:
                            RobotCompetitionDropDown.SelectedItem = "FTC";
                            MotorTypeDropDown.SelectedItem = "NEVEREST";
                            break;
                        case MotorType.TETRIX_MOTOR:
                            RobotCompetitionDropDown.SelectedItem = "FTC";
                            MotorTypeDropDown.SelectedItem = "TETRIX_MOTOR";
                            break;
                        case MotorType.MODERN_ROBOTICS_MATRIX:
                            RobotCompetitionDropDown.SelectedItem = "FTC";
                            MotorTypeDropDown.SelectedItem = "MODERN_ROBOTICS_MATRIX";
                            break;
                        case MotorType.REV_ROBOTICS_HD_HEX_20_TO_1:
                            RobotCompetitionDropDown.SelectedItem = "FTC";
                            MotorTypeDropDown.SelectedItem = "REV_ROBOTICS_HD_HEX_20_TO_1";
                            break;
                        case MotorType.REV_ROBOTICS_HD_HEX_40_TO_1:
                            RobotCompetitionDropDown.SelectedItem = "FTC";
                            MotorTypeDropDown.SelectedItem = "REV_ROBOTICS_HD_HEX_40_TO_1";
                            break;
                        case MotorType.REV_ROBOTICS_CORE_HEX:
                            RobotCompetitionDropDown.SelectedItem = "FTC";
                            MotorTypeDropDown.SelectedItem = "REV_ROBOTICS_CORE_HEX";
                            break;
                        case MotorType.VEX_V5_Smart_Motor_600_RPM:
                            RobotCompetitionDropDown.SelectedItem = "VEX";
                            MotorTypeDropDown.SelectedItem = "VEX_V5_Smart_Motor_600_RPM";
                            break;
                        case MotorType.VEX_V5_Smart_Motor_200_RPM:
                            RobotCompetitionDropDown.SelectedItem = "VEX";
                            MotorTypeDropDown.SelectedItem = "VEX_V5_Smart_Motor_200_RPM";
                            break;
                        case MotorType.VEX_V5_Smart_Motor_100_RPM:
                            RobotCompetitionDropDown.SelectedItem = "VEX";
                            MotorTypeDropDown.SelectedItem = "VEX_V5_Smart_Motor_100_RPM";
                            break;
                        case MotorType.VEX_393_NORMAL_SPEED:
                            RobotCompetitionDropDown.SelectedItem = "VEX";
                            MotorTypeDropDown.SelectedItem = "VEX_393_NORMAL_SPEED";
                            break;
                        case MotorType.VEX_393_HIGH_SPEED:
                            RobotCompetitionDropDown.SelectedItem = "VEX";
                            MotorTypeDropDown.SelectedItem = "VEX_393_HIGH_SPEED";
                            break;
                        case MotorType.VEX_393_TURBO_GEAR_SET:
                            RobotCompetitionDropDown.SelectedItem = "VEX";
                            MotorTypeDropDown.SelectedItem = "VEX_393_TURBO_GEAR_SET";
                            break;
                    }
                }
            }
            else //Default values
            {
                cmbJointDriver.SelectedIndex = 0;
                txtPort1.Value = txtPort1.Minimum;
                txtPort2.Value = txtPort2.Minimum;
                txtLowLimit.Value = txtLowLimit.Minimum;
                txtHighLimit.Value = txtHighLimit.Minimum;
                InputGeartxt.Value = (decimal) 1.0;
                OutputGeartxt.Value = (decimal) 1.0;

                rbPWM.Checked = true;

                chkBoxHasBrake.Checked = false;

                numericUpDownPnuDia.Value = (decimal)0.5;
                cmbPneumaticPressure.SelectedIndex = (int)PneumaticPressure.MEDIUM;

                cmbWheelType.SelectedIndex = (int)WheelType.NOT_A_WHEEL;
                cmbFrictionLevel.SelectedIndex = (int)FrictionLevel.MEDIUM;
                chkBoxDriveWheel.Checked = false;

                RobotCompetitionDropDown.SelectedItem = RobotDataManager.PluginSettings.DefaultRobotCompetition;
                MotorTypeDropDown.SelectedItem = "GENERIC";
            }

            PrepLayout();
            base.Location = new System.Drawing.Point(Cursor.Position.X - 10, Cursor.Position.Y - base.Height - 10);
            this.ShowDialog(owner);
        }

        private bool ShouldSave()
        {
            if (joint.cDriver == null) return true;

            double inputGear = 1, outputGear = 1;
        
            inputGear = (double) InputGeartxt.Value;
            outputGear = (double)OutputGeartxt.Value;
                
            PneumaticDriverMeta pneumatic = joint.cDriver.GetInfo<PneumaticDriverMeta>();
            WheelDriverMeta wheel = joint.cDriver.GetInfo<WheelDriverMeta>();
            ElevatorDriverMeta elevator = joint.cDriver.GetInfo<ElevatorDriverMeta>();
            Enum.TryParse(MotorTypeDropDown.SelectedItem.ToString(), out MotorType motor);
            if (cmbJointDriver.SelectedIndex != typeOptions.ToList().IndexOf(joint.cDriver.GetDriveType()) + 1 ||
                txtPort1.Value != joint.cDriver.port1 ||
                txtPort2.Value != joint.cDriver.port2 ||
                txtLowLimit.Value != (decimal) joint.cDriver.lowerLimit ||
                txtHighLimit.Value != (decimal) joint.cDriver.upperLimit ||
                inputGear != joint.cDriver.InputGear || outputGear != joint.cDriver.OutputGear || 
                rbCAN.Checked != joint.cDriver.isCan || chkBoxHasBrake.Checked != joint.cDriver.hasBrake ||
                motor != joint.cDriver.motor)
                return true;

            if (pneumatic != null && 
                (numericUpDownPnuDia.Value != (decimal)pneumatic.width ||
                 cmbPneumaticPressure.SelectedIndex != (int)pneumatic.pressureEnum))
                return true;

            if (wheel != null &&
                (cmbWheelType.SelectedIndex != (int)wheel.type ||
                 cmbFrictionLevel.SelectedIndex != (int)wheel.GetFrictionLevel() ||
                 chkBoxDriveWheel.Checked != wheel.isDriveWheel))
                return true;

            if (elevator != null)
                return true;

            //If going from "NOT A WHEEL" to a wheel
            if (cmbWheelType.SelectedIndex != 0 && wheel == null && joint.cDriver.GetDriveType() == JointDriverType.MOTOR)
                return true;

            return false;
        }

        /// <summary>
        /// Changes the position of window elements based on the type of driver.
        /// </summary>
        private void PrepLayout()
        {
            chkBoxDriveWheel.Hide();
            rbCAN.Hide();
            rbPWM.Hide();

            if (cmbJointDriver.SelectedIndex <= 0)      //If the joint is not driven
            {
                grpDriveOptions.Visible = false;
                tabsMeta.TabPages.Clear();
            }
            else
            {
                JointDriverType cType = typeOptions[cmbJointDriver.SelectedIndex - 1];
                lblPort.Text = cType.GetPortType(rbCAN.Checked) + " Port" + (cType.HasTwoPorts() ? "s" : "");
                txtPort2.Visible = cType.HasTwoPorts();
            
                txtPort1.Maximum = txtPort2.Maximum = cType.GetPortMax(rbCAN.Checked);
                grpDriveOptions.Visible = true;

                if (cType.IsMotor())
                {
                    tabsMeta.Visible = true;
                    tabsMeta.TabPages.Clear();
                    tabsMeta.TabPages.Add(metaWheel);
                    tabsMeta.TabPages.Add(metaGearing);
                    tabsMeta.TabPages.Add(metaBrake);
                    tabsMeta.TabPages.Add(metaMotorType);
                    chkBoxDriveWheel.Show();
                    rbCAN.Show();
                    rbPWM.Show();
                }
                else if (cType.IsPneumatic())
                {
                    tabsMeta.Visible = true;
                    tabsMeta.TabPages.Clear();
                    tabsMeta.TabPages.Add(metaPneumatic);
                    tabsMeta.TabPages.Add(metaBrake);
                }
                else if (cType.IsElevator())
                {
                    tabsMeta.Visible = true;
                    tabsMeta.TabPages.Clear();
                    tabsMeta.TabPages.Add(metaBrake);
                    chkBoxHasBrake.Show();

                    rbCAN.Show();
                    rbPWM.Show();
                }
                else if (cType.IsWormScrew())
                {
                    rbCAN.Show();
                    rbPWM.Show();
                }
                else
                {
                    tabsMeta.TabPages.Clear();
                    tabsMeta.Visible = false;
                }
            }
            // Set window size
            tabsMeta.Visible = tabsMeta.TabPages.Count > 0;
        }

        private void cmbJointDriver_SelectedIndexChanged(object sender, EventArgs e)
        {
            PrepLayout();
        }

        /// <summary>
        /// Saves all the data from the DriveChooser frame to be used elsewhere in the program.  Also begins calculation of wheel radius.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, EventArgs e)
        {
            bool canClose = true;
            if (!ShouldSave())
            {
                Close();
                return;
            }

            if (cmbJointDriver.SelectedIndex <= 0)
            {
                joint.cDriver = null;
            }
            else
            {
                JointDriverType cType = typeOptions[cmbJointDriver.SelectedIndex - 1];

                double inputGear = 1, outputGear = 1;

                inputGear = (double)InputGeartxt.Value;

                outputGear = (double)OutputGeartxt.Value;// tries to parse the double from the output gear

                joint.cDriver = new JointDriver(cType)
                {
                    port1 = (int)txtPort1.Value,
                    port2 = (int)txtPort2.Value,
                    InputGear = inputGear,// writes the input gear to the internal joint driver so it can be exported
                    OutputGear = outputGear,// writes the output gear to the internal joint driver so it can be exported
                    lowerLimit = (float)txtLowLimit.Value,
                    upperLimit = (float)txtHighLimit.Value,
                    isCan = rbCAN.Checked,
                    hasBrake = chkBoxHasBrake.Checked
                };
                if (cType.IsMotor())
                {
                    if (!Enum.TryParse(MotorTypeDropDown.SelectedItem.ToString(), out MotorType motor))
                    {
                        motor = MotorType.GENERIC;
                    }
                    joint.cDriver.motor = motor;
                    RobotDataManager.PluginSettings.DefaultRobotCompetition = RobotCompetitionDropDown.SelectedItem.ToString();
                }
                //Only need to store wheel driver if run by motor and is a wheel.
                if (cType.IsMotor() && (WheelType)cmbWheelType.SelectedIndex != WheelType.NOT_A_WHEEL)
                {
                    WheelDriverMeta wheelDriver = new WheelDriverMeta()
                    {
                        type = (WheelType)cmbWheelType.SelectedIndex,
                        isDriveWheel = chkBoxDriveWheel.Checked
                    }; //The info about the wheel attached to the joint.
                    //TODO: Find real values that make sense for the friction.  Also add Mecanum wheels.

                    wheelDriver.SetFrictionLevel((FrictionLevel)cmbFrictionLevel.SelectedIndex);

                    joint.cDriver.AddInfo(wheelDriver);
                }
                else
                {
                    joint.cDriver.RemoveInfo<WheelDriverMeta>();
                }

                if (cType.IsPneumatic())
                {
                    PneumaticDriverMeta pneumaticDriver = new PneumaticDriverMeta()
                    {
                        pressureEnum = (PneumaticPressure)cmbPneumaticPressure.SelectedIndex,
                        width = (double)numericUpDownPnuDia.Value
                    }; //The info about the wheel attached to the joint.
                    joint.cDriver.AddInfo(pneumaticDriver);
                }
                else
                {
                    joint.cDriver.RemoveInfo<PneumaticDriverMeta>();
                }

                if (cType.IsElevator())
                {
                    ElevatorDriverMeta elevatorDriver = new ElevatorDriverMeta()
                    {
                        type = ElevatorType.NOT_MULTI
                    };
                    joint.cDriver.AddInfo(elevatorDriver);
                }
                else
                {
                    joint.cDriver.RemoveInfo<ElevatorDriverMeta>();
                }
            }

            if (nodes.Count > 1)
            {
                foreach (RigidNode_Base node in nodes)
                {
                    if (joint.cDriver == null)
                    {
                        node.GetSkeletalJoint().cDriver = null;
                    }
                    else
                    {
                        JointDriver driver = new JointDriver(joint.cDriver.GetDriveType())
                        {
                            port1 = joint.cDriver.port1,
                            port2 = joint.cDriver.port2,
                            isCan = joint.cDriver.isCan,
                            OutputGear = joint.cDriver.OutputGear,
                            InputGear = joint.cDriver.InputGear,
                            lowerLimit = joint.cDriver.lowerLimit,
                            upperLimit = joint.cDriver.upperLimit
                        };
                        joint.cDriver.CopyMetaInfo(driver);

                        node.GetSkeletalJoint().cDriver = driver;
                    }
                }
            }

            if (canClose)// make sure there are no outstanding issues for the user to fix before we save
            {
                Saved = true;
                LegacyEvents.OnRobotModified();
                Close();
            }
        }

        private void cmbWheelType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((WheelType)cmbWheelType.SelectedIndex == WheelType.NOT_A_WHEEL)
            {
                lblFriction.Visible = false;
                cmbFrictionLevel.Visible = false;
            }
            else
            {
                lblFriction.Visible = true;
                cmbFrictionLevel.Visible = true;
            }
        }

        private void chkBoxHasBrake_CheckedChanged(object sender, EventArgs e)
        {
            /*if (chkBoxHasBrake.Checked)
        {
            lblBrakePort.Enabled = true;
            brakePort1.Enabled = true;
            brakePort2.Enabled = true;
        }
        else
        {
            lblBrakePort.Enabled = false;
            brakePort1.Enabled = false;
            brakePort2.Enabled = false;
        }*/
        }

        private void rbCAN_CheckedChanged(object sender, EventArgs e)
        {
            if (rbCAN.Checked)
            {
                lblPort.Text = "CAN Port";

            }
            else
            {
                lblPort.Text = "PWM Port";
            }
            PrepLayout();
        }

        private void RobotCompetitionDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (RobotCompetitionDropDown.SelectedItem.ToString())
            {
                case "GENERIC":
                    MotorTypeDropDown.Items.Clear();
                    MotorTypeDropDown.Items.Add("GENERIC");
                    MotorTypeDropDown.Enabled = false;
                    break;
                case "FRC":
                    MotorTypeDropDown.Enabled = true;
                    MotorTypeDropDown.Items.Clear();
                    MotorTypeDropDown.Items.Add("GENERIC");
                    MotorTypeDropDown.Items.Add("CIM");
                    MotorTypeDropDown.Items.Add("MINI_CIM");
                    MotorTypeDropDown.Items.Add("BAG_MOTOR");
                    MotorTypeDropDown.Items.Add("REDLINE_775_PRO");
                    MotorTypeDropDown.Items.Add("ANDYMARK_9015");
                    MotorTypeDropDown.Items.Add("BANEBOTS_775_18v");
                    MotorTypeDropDown.Items.Add("BANEBOTS_775_12v");
                    MotorTypeDropDown.Items.Add("BANEBOTS_550_12v");
                    MotorTypeDropDown.Items.Add("ANDYMARK_775_125");
                    MotorTypeDropDown.Items.Add("SNOW_BLOWER");
                    MotorTypeDropDown.Items.Add("NIDEC_BLDC");
                    MotorTypeDropDown.Items.Add("THROTTLE_MOTOR");
                    MotorTypeDropDown.Items.Add("WINDOW_MOTOR");
                    MotorTypeDropDown.Items.Add("NEVEREST");
                    break;
                case "FTC":
                    MotorTypeDropDown.Enabled = true;
                    MotorTypeDropDown.Items.Clear();
                    MotorTypeDropDown.Items.Add("GENERIC");
                    MotorTypeDropDown.Items.Add("NEVEREST");
                    MotorTypeDropDown.Items.Add("TETRIX_MOTOR");
                    MotorTypeDropDown.Items.Add("MODERN_ROBOTICS_MATRIX");
                    MotorTypeDropDown.Items.Add("REV_ROBOTICS_HD_HEX_20_TO_1");
                    MotorTypeDropDown.Items.Add("REV_ROBOTICS_HD_HEX_40_TO_1");
                    MotorTypeDropDown.Items.Add("REV_ROBOTICS_CORE_HEX");
                    break;
                case "VEX":
                    MotorTypeDropDown.Enabled = true;
                    MotorTypeDropDown.Items.Clear();
                    MotorTypeDropDown.Items.Add("GENERIC");
                    MotorTypeDropDown.Items.Add("VEX_V5_Smart_Motor_600_RPM");
                    MotorTypeDropDown.Items.Add("VEX_V5_Smart_Motor_200_RPM");
                    MotorTypeDropDown.Items.Add("VEX_V5_Smart_Motor_100_RPM");
                    MotorTypeDropDown.Items.Add("VEX_393_NORMAL_SPEED");
                    MotorTypeDropDown.Items.Add("VEX_393_HIGH_SPEED");
                    MotorTypeDropDown.Items.Add("VEX_393_TURBO_GEAR_SET");
                    break;
            }
            MotorTypeDropDown.SelectedItem = "GENERIC";
        }

        private void LblPort_Click(object sender, EventArgs e)
        {

        }

        private void TxtPort1_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}