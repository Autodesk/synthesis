using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using BxDRobotExporter.ControlGUI;
using BxDRobotExporter.Editors.CommonJointEditorForms;

namespace BxDRobotExporter.Editors.JointEditor
{
    public partial class JointDriverEditorUserControl : UserControl // TODO: Use this UserControl in the DriveChooser form and remove the save button in DriveChooser
    {
        private bool disableAutoSave;
        private SkeletalJoint_Base joint;
        private JointCard jointCard;
        private List<RigidNode_Base> nodes;
        private JointDriverType[] typeOptions;

        public JointDriverEditorUserControl()
        {
            InitializeComponent();
            WinFormsUtils.DisableScrollSelection(this);
            EnableLiveSave(this);
        }

        public void Initialize(List<RigidNode_Base> nodes, JointCard jointCard)
        {
            this.nodes = nodes;
            this.jointCard = jointCard;
            AnalyticUtils.LogEvent("Joint Card Editor", "System", "Init");
        }

        private void EnableLiveSave(Control control)
        {
            WinFormsUtils.AddChangeListener(control, SaveChanges);
            foreach (Control subControl in control.Controls)
            {
                EnableLiveSave(subControl);
            }

            AnalyticUtils.LogEvent("Joint Card Editor", "System", "EnableLiveSave");
        }

        public void LoadValues() // TODO: Settings saving and loading should be done in a dedicated class
        {
            disableAutoSave = true;

            joint = nodes[0].GetSkeletalJoint();
            typeOptions = JointDriver.GetAllowedDrivers(joint); // TODO: This doesn't protect multi-edit

            // Used for capitalization
            TextInfo textInfo = new CultureInfo("en-US", true).TextInfo;
            
            CalculatedWeightCheck.Checked = joint.weight <= 0;
            UnitBox.SelectedIndex = SynthesisGUI.Instance.RMeta.PreferMetric ? 1 : 0;
            WeightBox.Value = (decimal) (Math.Max(joint.weight, 0) * (SynthesisGUI.Instance.RMeta.PreferMetric ? 1 : 2.20462f)); // TODO: Re-use existing weight code
            
            cmbDriveSide.Items.Clear(); // TODO: This is dependant on DT type
            cmbDriveSide.Items.Add("Left");
            cmbDriveSide.Items.Add("Right");
            cmbDriveSide.Items.Add("Other");

            cmbJointDriver.Items.Clear();
            cmbJointDriver.Items.Add("No Driver");
            foreach (JointDriverType type in typeOptions)
            {
                cmbJointDriver.Items.Add(textInfo.ToTitleCase(Enum.GetName(typeof(JointDriverType), type)
                    .Replace('_', ' ').ToLowerInvariant()));
            }

            if (joint.cDriver != null)
            {
                cmbDriveSide.SelectedItem = JointDataStringUtils.DriveTrainSideString(joint);

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

                txtLowLimit.Value = (decimal) joint.cDriver.lowerLimit;
                txtHighLimit.Value = (decimal) joint.cDriver.upperLimit;

                rbPWM.Checked = !joint.cDriver.isCan;
                rbCAN.Checked = joint.cDriver.isCan;
                chkBoxHasBrake.Checked = joint.cDriver.hasBrake;
                if (joint.cDriver.OutputGear == 0
                ) // prevents output gear from being 0 //TODO: should be enforced via form limits
                {
                    joint.cDriver.OutputGear = 1;
                }

                if (joint.cDriver.InputGear == 0
                ) // prevents input gear from being 0//TODO: should be enforced via form limits
                {
                    joint.cDriver.InputGear = 1;
                }

                OutputGeartxt.Value =
                    (decimal) joint.cDriver
                        .OutputGear; // reads the existing gearing and writes it to the input field so the user sees their existing value
                InputGeartxt.Value =
                    (decimal) joint.cDriver
                        .InputGear; // reads the existing gearing and writes it to the input field so the user sees their existing value

                #region Meta info recovery

                {
                    PneumaticDriverMeta pneumaticMeta = joint.cDriver.GetInfo<PneumaticDriverMeta>();
                    if (pneumaticMeta != null)
                    {
                        numericUpDownPnuDia.Value = (decimal) pneumaticMeta.width;
                        cmbPneumaticPressure.SelectedIndex = (int) pneumaticMeta.pressureEnum;
                    }
                    else
                    {
                        numericUpDownPnuDia.Value = (decimal) 1.0;
                        cmbPneumaticPressure.SelectedIndex = (int) PneumaticPressure.HIGH;
                    }
                }
                {
                    WheelDriverMeta wheelMeta = joint.cDriver.GetInfo<WheelDriverMeta>();
                    if (wheelMeta != null)
                    {
                        try
                        {
                            cmbWheelType.SelectedIndex = (int) wheelMeta.type;
                            cmbFrictionLevel.SelectedIndex = (int) wheelMeta.GetFrictionLevel();
                        }
                        catch
                        {
                            // If an exception was thrown (System.ArguementOutOfRangeException) it means
                            // the user did not choose a wheel type when they were configuring the 
                            // wheel joint
                            cmbWheelType.SelectedIndex = (int) WheelType.NORMAL;
                            cmbFrictionLevel.SelectedIndex = (int) FrictionLevel.MEDIUM;
                        }

                        chkBoxDriveWheel.Checked = wheelMeta.isDriveWheel;
                        cmbWheelType_SelectedIndexChanged(null, null);
                    }
                    else
                    {
                        cmbWheelType.SelectedIndex = (int) WheelType.NOT_A_WHEEL;
                        cmbFrictionLevel.SelectedIndex = (int) FrictionLevel.MEDIUM;
                    }
                }
                {
                    ElevatorDriverMeta elevatorMeta = joint.cDriver.GetInfo<ElevatorDriverMeta>();
                }
                {
                    switch (joint.cDriver.motor) // TODO: Use motor definition file
                    {
                        case MotorType.GENERIC:
                            RobotCompetitionDropDown.SelectedItem =
                                SynthesisGUI.PluginSettings.defaultRobotCompetition.ToString();
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

                #endregion
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

                numericUpDownPnuDia.Value = (decimal) 0.5;
                cmbPneumaticPressure.SelectedIndex = (int) PneumaticPressure.MEDIUM;

                cmbWheelType.SelectedIndex = (int) WheelType.NOT_A_WHEEL;
                cmbFrictionLevel.SelectedIndex = (int) FrictionLevel.MEDIUM;
                chkBoxDriveWheel.Checked = false;

                RobotCompetitionDropDown.SelectedItem = SynthesisGUI.PluginSettings.defaultRobotCompetition;
                MotorTypeDropDown.SelectedItem = "GENERIC";
            }

            UpdateLayout();
            base.Location = new System.Drawing.Point(Cursor.Position.X - 10, Cursor.Position.Y - base.Height - 10);

            disableAutoSave = false;
        }

        /// <summary>
        /// Changes the position of window elements based on the type of driver.
        /// </summary>
        private void UpdateLayout()
        {
            WeightBox.Enabled = !CalculatedWeightCheck.Checked;
            
            chkBoxDriveWheel.Hide();

            if (cmbJointDriver.SelectedIndex <= 0) //If the joint is not driven
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

                txtPort1.Enabled = true;
                txtPort1.Minimum = 3;

                if (cType.IsMotor())
                {
                    tabsMeta.Visible = true;
                    tabsMeta.TabPages.Clear();
                    tabsMeta.TabPages.Add(metaWheel);
                    tabsMeta.TabPages.Add(metaGearing);
                    tabsMeta.TabPages.Add(metaBrake);
                    tabsMeta.TabPages.Add(metaMotorType);
                    chkBoxDriveWheel.Visible = !cType.HasTwoPorts();
                    cmbDriveSide.Visible = !cType.HasTwoPorts() && chkBoxDriveWheel.Checked;
                    grpDriveOptions.Visible = !chkBoxDriveWheel.Checked;

                    rbCAN.Show();
                    rbPWM.Show();
                }
                else if (cType.IsPneumatic())
                {
                    tabsMeta.Visible = true;
                    tabsMeta.TabPages.Clear();
                    tabsMeta.TabPages.Add(metaPneumatic);
                    tabsMeta.TabPages.Add(metaBrake);
                    rbCAN.Hide();
                    rbPWM.Hide();
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

        /// <summary>
        /// Saves all the data from the DriveChooser frame to be used elsewhere in the program.  Also begins calculation of wheel radius.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveChanges(object sender, EventArgs e) // TODO: Settings saving and loading should be done in a dedicated class
        {
            AnalyticUtils.LogEvent("Joint Card Editor", "System", "SaveChanges");
            if (disableAutoSave)
            {
                return;
            }
            
            var preferMetric = UnitBox.SelectedIndex == 1;

            if (CalculatedWeightCheck.Checked)
                joint.weight = -1;
            else
            {
                if (!preferMetric)
                    joint.weight = (float)WeightBox.Value / 2.20462f;
                else
                    joint.weight = (float)WeightBox.Value;
            }

            if (cmbJointDriver.SelectedIndex <= 0)
            {
                joint.cDriver = null;
            }
            else
            {
                JointDriverType cType = typeOptions[cmbJointDriver.SelectedIndex - 1];

                var inputGear = (double) InputGeartxt.Value;
                var outputGear = (double) OutputGeartxt.Value;

                var port1Value = (int) txtPort1.Value;
                if (chkBoxDriveWheel.Checked)
                {
                    switch (cmbDriveSide.SelectedItem)
                    {
                        case "Right":
                            port1Value = 0;
                            break;
                        case "Left":
                            port1Value = 1;
                            break;
                        case "Other":
                            port1Value = 2;
                            break;
                    }
                }

                joint.cDriver = new JointDriver(cType)
                {
                    port1 = port1Value,
                    port2 = (int) txtPort2.Value,
                    InputGear = inputGear, // writes the input gear to the internal joint driver so it can be exported
                    OutputGear =
                        outputGear, // writes the output gear to the internal joint driver so it can be exported
                    lowerLimit = (float) txtLowLimit.Value,
                    upperLimit = (float) txtHighLimit.Value,
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
                    SynthesisGUI.PluginSettings.defaultRobotCompetition =
                        RobotCompetitionDropDown.SelectedItem.ToString();
                }

                //Only need to store wheel driver if run by motor and is a wheel.
                if (cType.IsMotor() && (WheelType) cmbWheelType.SelectedIndex != WheelType.NOT_A_WHEEL)
                {
                    #region WHEEL_SAVING

                    WheelDriverMeta wheelDriver = new WheelDriverMeta()
                    {
                        type = (WheelType) cmbWheelType.SelectedIndex,
                        isDriveWheel = chkBoxDriveWheel.Checked
                    }; //The info about the wheel attached to the joint.
                    //TODO: Find real values that make sense for the friction.  Also add Mecanum wheels.

                    wheelDriver.SetFrictionLevel((FrictionLevel) cmbFrictionLevel.SelectedIndex);

                    joint.cDriver.AddInfo(wheelDriver);

                    #endregion
                }
                else
                {
                    joint.cDriver.RemoveInfo<WheelDriverMeta>();
                }

                if (cType.IsPneumatic())
                {
                    #region PNEUMATIC_SAVING

                    PneumaticDriverMeta pneumaticDriver = new PneumaticDriverMeta()
                    {
                        pressureEnum = (PneumaticPressure) cmbPneumaticPressure.SelectedIndex,
                        width = (double) numericUpDownPnuDia.Value
                    }; //The info about the wheel attached to the joint.
                    joint.cDriver.AddInfo(pneumaticDriver);

                    #endregion
                }
                else
                {
                    joint.cDriver.RemoveInfo<PneumaticDriverMeta>();
                }

                if (cType.IsElevator())
                {
                    #region ELEVATOR_SAVING

                    ElevatorDriverMeta elevatorDriver = new ElevatorDriverMeta()
                    {
                        type = ElevatorType.NOT_MULTI
                    };
                    joint.cDriver.AddInfo(elevatorDriver);

                    #endregion
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

            jointCard?.LoadValues(); // TODO: Use event listener for change events

            disableAutoSave = false;
        }

        private void cmbWheelType_SelectedIndexChanged(object sender, EventArgs e)
        {
            AnalyticUtils.LogEvent("Joint Card Editor", "Option", "WheelType", cmbWheelType.SelectedIndex);
            if ((WheelType) cmbWheelType.SelectedIndex == WheelType.NOT_A_WHEEL)
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

        private void RobotCompetitionDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            AnalyticUtils.LogEvent("Joint Card Editor", "Option", "Competetion", RobotCompetitionDropDown.SelectedIndex);
            switch (RobotCompetitionDropDown.SelectedItem.ToString()) // TODO: Use some kind of motor definition file
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
        private void rbCAN_CheckedChanged(object sender, EventArgs e)
        {
            AnalyticUtils.LogEvent("Joint Card Editor", "Option", "CAN", rbCAN.Checked ? 0 : 1);
            UpdateLayout();
        }

        private void chkBoxDriveWheel_CheckedChanged(object sender, EventArgs e)
        {
            AnalyticUtils.LogEvent("Joint Card Editor", "Option", "Drive Wheel", chkBoxDriveWheel.Checked ? 0 : 1);
            UpdateLayout();
        }
        
        private void cmbJointDriver_SelectedIndexChanged(object sender, EventArgs e)
        {
            AnalyticUtils.LogEvent("Joint Card Editor", "Option", "Joint Driver", cmbJointDriver.SelectedIndex);
            UpdateLayout();
        }

        private void CalculatedWeightCheck_CheckedChanged(object sender, EventArgs e)
        {
            AnalyticUtils.LogEvent("Joint Card Editor", "Option", "Calculate Weight", CalculatedWeightCheck.Checked ? 0 : 1) ;
            UpdateLayout();
        }
    }
}