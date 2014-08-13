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
    }

    private JointDriverType[] typeOptions;
    private SkeletalJoint_Base joint;
    private WheelType wheelType;
    private FrictionLevel friction;
    //private JointDriverType driverType;
    private PneumaticDiameter diameter;
    private PneumaticPressure pressure;
    private RigidNode_Base node;


    public void ShowDialog(SkeletalJoint_Base joint, RigidNode_Base node)
    {
        this.joint = joint;
        this.node = node;
        typeOptions = JointDriver.GetAllowedDrivers(joint);

        cmbJointDriver.Items.Clear();
        cmbJointDriver.Items.Add("No Driver");
        foreach (JointDriverType type in typeOptions)
        {
            cmbJointDriver.Items.Add(Enum.GetName(typeof(JointDriverType), type).Replace('_', ' ').ToLowerInvariant());
        }
        cmbJointDriver.SelectedIndex = 0;
        if (joint.cDriver != null)
        {
            cmbJointDriver.SelectedIndex = Array.IndexOf(typeOptions, joint.cDriver.GetDriveType()) + 1;
            cmbJointDriver_SelectedIndexChanged(null, null);
            txtPortA.Value = joint.cDriver.portA;
            txtPortB.Value = joint.cDriver.portB;
            txtLowLimit.Value = (decimal) joint.cDriver.lowerLimit;
            txtHighLimit.Value = (decimal) joint.cDriver.upperLimit;
        }
        ShowDialog();
    }

    /// <summary>
    /// Changes the position of window elements based on the type of driver.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void cmbJointDriver_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cmbJointDriver.SelectedIndex <= 0)      //If the joint is not driven
        {
            this.Height = 245;
            btnSave.Location = new System.Drawing.Point(13, 165);
            lblLimits.Location = new System.Drawing.Point(11, 22);
            txtLowLimit.Location = new System.Drawing.Point(14, 42);
            txtHighLimit.Location = new System.Drawing.Point(140, 42);
            lblPort.Visible = false;
            txtPortA.Visible = false;
            txtPortB.Visible = false;
            grpDriveOptions.Size = new System.Drawing.Size(318, 75);
        }
        else
        {
            JointDriverType cType = typeOptions[cmbJointDriver.SelectedIndex - 1];
            lblPort.Text = cType.GetPortType() + " Port" + (cType.HasTwoPorts() ? "s" : "");
            txtPortB.Visible = cType.HasTwoPorts();
            txtPortA.Maximum = txtPortB.Maximum = cType.GetPortMax();

            lblLimits.Location = new System.Drawing.Point(11, 72);
            txtLowLimit.Location = new System.Drawing.Point(14, 92);
            txtHighLimit.Location = new System.Drawing.Point(140, 92);
            lblPort.Visible = true;
            txtPortA.Visible = true;
            grpDriveOptions.Size = new System.Drawing.Size(318, 128);

            if (cType.IsMotor() == false && cType.IsPneumatic() == false)
            {
                this.Height = 300;
                btnSave.Location = new System.Drawing.Point(13, 220);
                grpWheelOptions.Visible = false;
                grpGearRatio.Visible = false;
                grpPneumaticSpecs.Visible = false;
                grpDriveOptions.Visible = true;
            }

            else if (cType.IsMotor() == true || cType.IsPneumatic() == true)
            {
                if (cType.IsMotor() == true)
                {
                    this.Height = 420;
                    btnSave.Location = new System.Drawing.Point(13, 340);
                    grpWheelOptions.Visible = true;
                    grpGearRatio.Visible = true;
                    grpPneumaticSpecs.Visible = false;
                    grpDriveOptions.Visible = true;
                }
                else if (cType.IsPneumatic() == true)
                {
                    this.Height = 360;
                    btnSave.Location = new System.Drawing.Point(13, 280);
                    grpPneumaticSpecs.Visible = true;
                    grpWheelOptions.Visible = false;
                    grpGearRatio.Visible = false;
                    grpDriveOptions.Visible = true;
                }
            }
        }
    }

    /// <summary>
    /// Saves all the data from the DriveChooser frame to be used elsewhere in the program.  Also begins calculation of wheel radius.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnSave_Click(object sender, EventArgs e)
    {
        if (cmbJointDriver.SelectedIndex <= 0)
        {
            joint.cDriver = null;
        }
        else
        {
            JointDriverType cType = typeOptions[cmbJointDriver.SelectedIndex - 1];

            joint.cDriver = new JointDriver(cType);

            joint.cDriver.portA = (int) txtPortA.Value;
            joint.cDriver.portB = (int) txtPortB.Value;
            joint.cDriver.lowerLimit = (float) txtLowLimit.Value;
            joint.cDriver.upperLimit = (float) txtHighLimit.Value;

            //Only need to store wheel driver if run by motor and is a wheel.
            if (cType.IsMotor() && wheelType != WheelType.NOT_A_WHEEL)
            {
                #region WHEEL_SAVING
                WheelDriverMeta wheelDriver = new WheelDriverMeta(); //The info about the wheel attached to the joint.
                wheelDriver.type = wheelType;

                //TODO: Find real values that make sense for the friction.  Also add Mecanum wheels.
                switch (friction)
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
                PneumaticDriverMeta pneumaticDriver = new PneumaticDriverMeta(); //The info about the wheel attached to the joint.
                switch (diameter)
                {
                    case PneumaticDiameter.HIGH:
                        pneumaticDriver.widthMM = 10;
                        break;
                    case PneumaticDiameter.LOW:
                        pneumaticDriver.widthMM = 1;
                        break;
                    case PneumaticDiameter.MEDIUM:
                    default:
                        pneumaticDriver.widthMM = 5;
                        break;
                }

                switch (pressure)
                {
                    case PneumaticPressure.MEDIUM:
                        pneumaticDriver.pressurePSI = 40;
                        break;
                    case PneumaticPressure.LOW:
                        pneumaticDriver.pressurePSI = 20;
                        break;
                    default:
                    case PneumaticPressure.HIGH:
                        pneumaticDriver.pressurePSI = 60;
                        break;
                }
                joint.cDriver.AddInfo(pneumaticDriver);
                #endregion
            }
            else
            {
                joint.cDriver.RemoveInfo<PneumaticDriverMeta>();
            }
        }
        Hide();
    }

    private void cmbWheelType_SelectedIndexChanged(object sender, EventArgs e)
    {
        wheelType = (WheelType) cmbWheelType.SelectedIndex;

        if (wheelType == WheelType.NOT_A_WHEEL)
        {
            cmbFrictionLevel.Visible = false;
        }
        else
        {
            cmbFrictionLevel.Visible = true;
        }
    }

    private void cmbFrictionLevel_SelectedIndexChanged(object sender, EventArgs e)
    {
        friction = (FrictionLevel) cmbFrictionLevel.SelectedIndex;
    }

    private void cmbPneumaticDiameter_SelectedIndexChanged(object sender, EventArgs e)
    {
        diameter = (PneumaticDiameter) cmbPneumaticDiameter.SelectedIndex;
    }

    private void cmbPneumaticForce_SelectedIndexChanged(object sender, EventArgs e)
    {
        pressure = (PneumaticPressure) cmbPneumaticPressure.SelectedIndex;
    }
}