using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;


public partial class DriveChooser : Form
{
    public DriveChooser()
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

            if (joint.cDriver.portA < txtPortA.Minimum)
                txtPortA.Value = txtPortA.Minimum;
            else if (joint.cDriver.portA > txtPortA.Maximum)
                txtPortA.Value = txtPortA.Maximum;
            else
                txtPortA.Value = joint.cDriver.portA;

            if (joint.cDriver.portB < txtPortB.Minimum)
                txtPortB.Value = txtPortB.Minimum;
            else if (joint.cDriver.portB > txtPortB.Maximum)
                txtPortB.Value = txtPortB.Maximum;
            else
                txtPortB.Value = joint.cDriver.portB;

            txtLowLimit.Value = (decimal)joint.cDriver.lowerLimit;
            txtHighLimit.Value = (decimal)joint.cDriver.upperLimit;

            rbPWM.Checked = !joint.cDriver.isCan;
            rbCAN.Checked = joint.cDriver.isCan;
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

            #region Meta info recovery
            {
                PneumaticDriverMeta pneumaticMeta = joint.cDriver.GetInfo<PneumaticDriverMeta>();
                if (pneumaticMeta != null)
                {
                    cmbPneumaticDiameter.SelectedIndex = (int)pneumaticMeta.widthEnum;
                    cmbPneumaticPressure.SelectedIndex = (int)pneumaticMeta.pressureEnum;
                }
                else
                {
                    cmbPneumaticDiameter.SelectedIndex = (int)PneumaticDiameter.MEDIUM;
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
                if (elevatorMeta != null && (int)elevatorMeta.type < 7)
                {
                    cmbStages.SelectedIndex = (int)elevatorMeta.type;
                } else
                {
                    cmbStages.SelectedIndex = 0;
                }
            }
            #endregion
        }
        else //Default values
        {
            cmbJointDriver.SelectedIndex = 0;
            txtPortA.Value = txtPortA.Minimum;
            txtPortB.Value = txtPortB.Minimum;
            txtLowLimit.Value = txtLowLimit.Minimum;
            txtHighLimit.Value = txtHighLimit.Minimum;
            InputGeartxt.Value = (decimal) 1.0;
            OutputGeartxt.Value = (decimal) 1.0;

            rbPWM.Checked = true;

            cmbPneumaticDiameter.SelectedIndex = (int)PneumaticDiameter.MEDIUM;
            cmbPneumaticPressure.SelectedIndex = (int)PneumaticPressure.MEDIUM;

            cmbWheelType.SelectedIndex = (int)WheelType.NOT_A_WHEEL;
            cmbFrictionLevel.SelectedIndex = (int)FrictionLevel.MEDIUM;
            chkBoxDriveWheel.Checked = false;

            cmbStages.SelectedIndex = (int)ElevatorType.NOT_MULTI;
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

        if (cmbJointDriver.SelectedIndex != typeOptions.ToList().IndexOf(joint.cDriver.GetDriveType()) + 1 ||
            txtPortA.Value != joint.cDriver.portA ||
            txtPortB.Value != joint.cDriver.portB ||
            txtLowLimit.Value != (decimal) joint.cDriver.lowerLimit ||
            txtHighLimit.Value != (decimal) joint.cDriver.upperLimit ||
            inputGear != joint.cDriver.InputGear || outputGear != joint.cDriver.OutputGear || 
            rbCAN.Checked != joint.cDriver.isCan)
            return true;

        if (pneumatic != null && 
            (cmbPneumaticDiameter.SelectedIndex != (int)pneumatic.widthEnum ||
            cmbPneumaticPressure.SelectedIndex != (int)pneumatic.pressureEnum))
            return true;

        if (wheel != null &&
            (cmbWheelType.SelectedIndex != (int)wheel.type ||
            cmbFrictionLevel.SelectedIndex != (int)wheel.GetFrictionLevel() ||
            chkBoxDriveWheel.Checked != wheel.isDriveWheel))
            return true;

        if (elevator != null &&
            cmbStages.SelectedIndex != (int)elevator.type)
            return true;

        //If going from "NOT A WHEEL" to a wheel
        if (cmbWheelType.SelectedIndex != 0 && wheel == null && joint.cDriver.GetDriveType() == JointDriverType.MOTOR)
            return true;

        return false;
    }

    /// <summary>
    /// Changes the position of window elements based on the type of driver.
    /// </summary>
    void PrepLayout()
    {
        chkBoxDriveWheel.Hide();
        chkBoxHasBrake.Hide();
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
            lblPort.Text = cType.GetPortType() + " Port" + (cType.HasTwoPorts() ? "s" : "");
            txtPortB.Visible = cType.HasTwoPorts();
            txtPortA.Maximum = txtPortB.Maximum = cType.GetPortMax();
            grpDriveOptions.Visible = true;

            if (cType.IsMotor())
            {
                tabsMeta.Visible = true;
                tabsMeta.TabPages.Clear();
                tabsMeta.TabPages.Add(metaWheel);
                tabsMeta.TabPages.Add(metaGearing);
                chkBoxDriveWheel.Show();
                rbCAN.Show();
                rbPWM.Show();
            }
            else if (cType.IsPneumatic())
            {
                tabsMeta.Visible = true;
                tabsMeta.TabPages.Clear();
                tabsMeta.TabPages.Add(metaPneumatic);
            }
            else if (cType.IsElevator())
            {
                tabsMeta.Visible = true;
                lblBrakePort.Enabled = false;
                brakePortA.Enabled = false;
                brakePortB.Enabled = false;
                tabsMeta.TabPages.Clear();
                chkBoxHasBrake.Show();
                //tabsMeta.TabPages.Add(metaElevatorBrake);
                tabsMeta.TabPages.Add(metaElevatorStages);
                tabsMeta.TabPages.Add(metaGearing);

                if(cmbStages.SelectedIndex == -1)
                    cmbStages.SelectedIndex = 0;
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
                portA = (int)txtPortA.Value,
                portB = (int)txtPortB.Value,
                InputGear = inputGear,// writes the input gear to the internal joint driver so it can be exported
                OutputGear = outputGear,// writes the output gear to the internal joint driver so it can be exported
                lowerLimit = (float)txtLowLimit.Value,
                upperLimit = (float)txtHighLimit.Value,
                isCan = rbCAN.Checked
            };
            //Only need to store wheel driver if run by motor and is a wheel.
            if (cType.IsMotor() && (WheelType)cmbWheelType.SelectedIndex != WheelType.NOT_A_WHEEL)
            {
                #region WHEEL_SAVING
                WheelDriverMeta wheelDriver = new WheelDriverMeta()
                {
                    type = (WheelType)cmbWheelType.SelectedIndex,
                    isDriveWheel = chkBoxDriveWheel.Checked
                }; //The info about the wheel attached to the joint.
                   //TODO: Find real values that make sense for the friction.  Also add Mecanum wheels.

                wheelDriver.SetFrictionLevel((FrictionLevel)cmbFrictionLevel.SelectedIndex);

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
                    pressureEnum = (PneumaticPressure)cmbPneumaticPressure.SelectedIndex,
                    widthEnum = (PneumaticDiameter)cmbPneumaticDiameter.SelectedIndex
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
                    type = (ElevatorType)cmbStages.SelectedIndex
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
                        portA = joint.cDriver.portA,
                        portB = joint.cDriver.portB,
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
            LegacyInterchange.LegacyEvents.OnRobotModified();
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
        if (chkBoxHasBrake.Checked)
        {
            lblBrakePort.Enabled = true;
            brakePortA.Enabled = true;
            brakePortB.Enabled = true;
        }
        else
        {
            lblBrakePort.Enabled = false;
            brakePortA.Enabled = false;
            brakePortB.Enabled = false;
        }
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
    }
}