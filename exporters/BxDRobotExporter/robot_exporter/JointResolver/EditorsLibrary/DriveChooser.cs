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


public partial class DriveChooser : Form
{
    public DriveChooser()
    {
        InitializeComponent();
        base.Layout += DriveChooser_Layout;
        FormClosing += delegate (object sender, FormClosingEventArgs e) { LegacyInterchange.LegacyEvents.OnRobotModified(); };
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

        cmbJointDriver.Items.Clear();
        cmbJointDriver.Items.Add("No Driver");
        foreach (JointDriverType type in typeOptions)
        {
            cmbJointDriver.Items.Add(Enum.GetName(typeof(JointDriverType), type).Replace('_', ' ').ToLowerInvariant());
        }
        if (joint.cDriver != null)
        {
            cmbJointDriver.SelectedIndex = Array.IndexOf(typeOptions, joint.cDriver.GetDriveType()) + 1;
            txtPortA.Value = joint.cDriver.portA;
            txtPortB.Value = joint.cDriver.portB;
            txtLowLimit.Value = (decimal)joint.cDriver.lowerLimit;
            txtHighLimit.Value = (decimal)joint.cDriver.upperLimit;

            #region Meta info recovery
            {
                PneumaticDriverMeta pneumaticMeta = joint.cDriver.GetInfo<PneumaticDriverMeta>();
                if (pneumaticMeta != null)
                {
                    cmbPneumaticDiameter.SelectedIndex = (byte)pneumaticMeta.widthEnum;
                    cmbPneumaticPressure.SelectedIndex = (byte)pneumaticMeta.pressureEnum;
                }
                else
                {
                    cmbPneumaticDiameter.SelectedIndex = (byte)PneumaticDiameter.MEDIUM;
                    cmbPneumaticPressure.SelectedIndex = (byte)PneumaticPressure.HIGH;
                }
            }
            {
                WheelDriverMeta wheelMeta = joint.cDriver.GetInfo<WheelDriverMeta>();
                if (wheelMeta != null)
                {
                    try
                    {
                        // TODO:  This is a really sketchy hack and I don't even know where the cat is.
                        cmbWheelType.SelectedIndex = (byte)wheelMeta.type;
                        if (wheelMeta.forwardExtremeValue > 8)
                            cmbFrictionLevel.SelectedIndex = (byte)FrictionLevel.HIGH;
                        else if (wheelMeta.forwardExtremeValue > 4)
                            cmbFrictionLevel.SelectedIndex = (byte)FrictionLevel.MEDIUM;
                        else
                            cmbFrictionLevel.SelectedIndex = (byte)FrictionLevel.LOW;
                    }
                    catch
                    {
                        // If an exception was thrown (System.ArguementOutOfRangeException) it means
                        // the user did not choose a wheel type when they were configuring the 
                        // wheel joint
                        cmbWheelType.SelectedIndex = (byte)WheelType.NORMAL;
                        cmbFrictionLevel.SelectedIndex = (byte)FrictionLevel.MEDIUM;
                    }

                    chkBoxDriveWheel.Checked = wheelMeta.isDriveWheel;
                    cmbWheelType_SelectedIndexChanged(null, null);
                }
                else
                {
                    cmbWheelType.SelectedIndex = (byte)WheelType.NOT_A_WHEEL;
                    cmbFrictionLevel.SelectedIndex = (byte)FrictionLevel.MEDIUM;
                }
            }
            {
                ElevatorDriverMeta elevatorMeta = joint.cDriver.GetInfo<ElevatorDriverMeta>();
                if (elevatorMeta != null)
                {
                    cmbStages.SelectedIndex = (byte)elevatorMeta.type;
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

            cmbPneumaticDiameter.SelectedIndex = (byte)PneumaticDiameter.MEDIUM;
            cmbPneumaticPressure.SelectedIndex = (byte)PneumaticPressure.MEDIUM;

            cmbWheelType.SelectedIndex = (byte)WheelType.NOT_A_WHEEL;
            cmbFrictionLevel.SelectedIndex = (byte)FrictionLevel.MEDIUM;
            chkBoxDriveWheel.Checked = false;

            cmbStages.SelectedIndex = (byte)ElevatorType.NOT_MULTI;
        }
        PerformLayout();
        this.ShowDialog(owner);
    }

    private bool shouldSave()
    {
        if (joint.cDriver == null) return true;

        PneumaticDriverMeta pneumatic = joint.cDriver.GetInfo<PneumaticDriverMeta>();
        WheelDriverMeta wheel = joint.cDriver.GetInfo<WheelDriverMeta>();
        ElevatorDriverMeta elevator = joint.cDriver.GetInfo<ElevatorDriverMeta>();

        bool shouldSave = false;

        if (cmbJointDriver.SelectedIndex != typeOptions.ToList().IndexOf(joint.cDriver.GetDriveType()) + 1 ||
            txtPortA.Value != joint.cDriver.portA ||
            txtPortB.Value != joint.cDriver.portB ||
            txtLowLimit.Value != (decimal) joint.cDriver.lowerLimit ||
            txtHighLimit.Value != (decimal) joint.cDriver.upperLimit)
            shouldSave = true;

        if (pneumatic != null && 
            (cmbPneumaticDiameter.SelectedIndex != (byte) pneumatic.widthEnum ||
            cmbPneumaticPressure.SelectedIndex != (byte) pneumatic.pressureEnum))
            shouldSave = true;

        if (wheel != null &&
            (cmbWheelType.SelectedIndex != (byte) wheel.type ||
            cmbFrictionLevel.SelectedIndex != (byte) Math.Min(Math.Floor(wheel.forwardExtremeValue / 4), 2) || //ayy lmao
            chkBoxDriveWheel.Checked != wheel.isDriveWheel))
            shouldSave = true;

        if (elevator != null &&
            cmbStages.SelectedIndex != (byte) elevator.type)
            shouldSave = true;

        //If going from "NOT A WHEEL" to a wheel
        if (cmbWheelType.SelectedIndex != 0 && wheel == null && joint.cDriver.GetDriveType() == JointDriverType.MOTOR)
            shouldSave = true;

        return shouldSave;
    }

    /// <summary>
    /// Changes the position of window elements based on the type of driver.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void DriveChooser_Layout(object sender, LayoutEventArgs e)
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
                rbPWM.Checked = true;
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
                tabsMeta.TabPages.Add(metaElevatorBrake);
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
        btnSave.Top = tabsMeta.TabPages.Count > 0 ? tabsMeta.Bottom + 3 : (grpDriveOptions.Visible ? grpDriveOptions.Bottom + 3 : grpChooseDriver.Bottom + 3);
        base.Height = btnSave.Bottom + 3 + (base.Height - base.ClientSize.Height);
    }

    private void cmbJointDriver_SelectedIndexChanged(object sender, EventArgs e)
    {
        PerformLayout();
    }

    /// <summary>
    /// Saves all the data from the DriveChooser frame to be used elsewhere in the program.  Also begins calculation of wheel radius.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnSave_Click(object sender, EventArgs e)
    {
        if (!shouldSave())
        {
            Hide();
            return;
        }

        if (cmbJointDriver.SelectedIndex <= 0)
        {
            joint.cDriver = null;
        }
        else
        {
            JointDriverType cType = typeOptions[cmbJointDriver.SelectedIndex - 1];

            joint.cDriver = new JointDriver(cType)
            {
                portA = (int)txtPortA.Value,
                portB = (int)txtPortB.Value,
                lowerLimit = (float)txtLowLimit.Value,
                upperLimit = (float)txtHighLimit.Value,
                isCan = rbCAN.Checked
            };
            //Only need to store wheel driver if run by motor and is a wheel.
            if (cType.IsMotor() && (WheelType) cmbWheelType.SelectedIndex != WheelType.NOT_A_WHEEL)
            {
                #region WHEEL_SAVING
                WheelDriverMeta wheelDriver = new WheelDriverMeta()
                {
                    type = (WheelType)cmbWheelType.SelectedIndex,
                    isDriveWheel = chkBoxDriveWheel.Checked
                }; //The info about the wheel attached to the joint.
                   //TODO: Find real values that make sense for the friction.  Also add Mecanum wheels.
                switch ((FrictionLevel) cmbFrictionLevel.SelectedIndex)
                {
                    case FrictionLevel.HIGH:
                        wheelDriver.forwardExtremeSlip = 1; //Speed of max static friction force.
                        wheelDriver.forwardExtremeValue = 10; //Force of max static friction force.
                        wheelDriver.forwardAsympSlip = 1.5f; //Speed of leveled off kinetic friction force.
                        wheelDriver.forwardAsympValue = 8; //Force of leveld off kinetic friction force.

                        if (wheelDriver.type == WheelType.OMNI) //Set to relatively low friction, as omni wheels can move sidways.
                        {
                            wheelDriver.sideExtremeSlip = 1; //Same as above, but orthogonal to the movement of the wheel.
                            wheelDriver.sideExtremeValue = .01f;
                            wheelDriver.sideAsympSlip = 1.5f;
                            wheelDriver.sideAsympValue = .005f;
                        }
                        else
                        {
                            wheelDriver.sideExtremeSlip = 1;
                            wheelDriver.sideExtremeValue = 10;
                            wheelDriver.sideAsympSlip = 1.5f;
                            wheelDriver.sideAsympValue = 8;
                        }
                        break;
                    case FrictionLevel.MEDIUM:
                        wheelDriver.forwardExtremeSlip = 1f;
                        wheelDriver.forwardExtremeValue = 7;
                        wheelDriver.forwardAsympSlip = 1.5f;
                        wheelDriver.forwardAsympValue = 5;

                        if (wheelDriver.type == WheelType.OMNI)
                        {
                            wheelDriver.sideExtremeSlip = 1;
                            wheelDriver.sideExtremeValue = .01f;
                            wheelDriver.sideAsympSlip = 1.5f;
                            wheelDriver.sideAsympValue = .005f;
                        }
                        else
                        {
                            wheelDriver.sideExtremeSlip = 1;
                            wheelDriver.sideExtremeValue = 7;
                            wheelDriver.sideAsympSlip = 1.5f;
                            wheelDriver.sideAsympValue = 5;
                        }
                        break;
                    case FrictionLevel.LOW:
                        wheelDriver.forwardExtremeSlip = 1;
                        wheelDriver.forwardExtremeValue = 5;
                        wheelDriver.forwardAsympSlip = 1.5f;
                        wheelDriver.forwardAsympValue = 3;

                        if (wheelDriver.type == WheelType.OMNI)
                        {
                            wheelDriver.sideExtremeSlip = 1;
                            wheelDriver.sideExtremeValue = .01f;
                            wheelDriver.sideAsympSlip = 1.5f;
                            wheelDriver.sideAsympValue = .005f;
                        }
                        else
                        {
                            wheelDriver.sideExtremeSlip = 1;
                            wheelDriver.sideExtremeValue = 5;
                            wheelDriver.sideAsympSlip = 1.5f;
                            wheelDriver.sideAsympValue = 3;
                        }
                        break;
                }

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
                        lowerLimit = joint.cDriver.lowerLimit,
                        upperLimit = joint.cDriver.upperLimit
                    };
                    joint.cDriver.CopyMetaInfo(driver);

                    node.GetSkeletalJoint().cDriver = driver;
                }
            }
        }

        Saved = true;
        Hide();
    }

    private void cmbWheelType_SelectedIndexChanged(object sender, EventArgs e)
    {
        cmbFrictionLevel.Visible = (WheelType) cmbWheelType.SelectedIndex != WheelType.NOT_A_WHEEL;
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


    #region UNUSED
    private void label1_Click(object sender, EventArgs e)
    {

    }

    private void DriveChooser_Load(object sender, EventArgs e)
    {

    }
    #endregion

    private void grpChooseDriver_Enter(object sender, EventArgs e)
    {

    }
}