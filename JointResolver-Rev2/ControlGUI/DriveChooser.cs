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
using Inventor;


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
    RigidNode node;


    public void ShowDialog(SkeletalJoint_Base joint, RigidNode node)
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
            cmbJointDriver.SelectedIndex = Array.IndexOf(typeOptions, joint.cDriver.GetDriveType());
            cmbJointDriver_SelectedIndexChanged(null, null);
            txtPortA.Value = joint.cDriver.portA;
            txtPortB.Value = joint.cDriver.portB;
            txtLowLimit.Value = (decimal)joint.cDriver.lowerLimit;
            txtHighLimit.Value = (decimal)joint.cDriver.upperLimit;
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
            JointDriver type = null;
        }
        else
        {
            JointDriverType cType = typeOptions[cmbJointDriver.SelectedIndex - 1];
            lblPort.Text = JointDriver.GetPortType(cType) + " Port" + (JointDriver.HasTwoPorts(cType) ? "s" : "");
            txtPortB.Visible = JointDriver.HasTwoPorts(cType);
            txtPortA.Maximum = txtPortB.Maximum = JointDriver.GetPortMax(cType);

            lblLimits.Location = new System.Drawing.Point(11, 72);
            txtLowLimit.Location = new System.Drawing.Point(14, 92);
            txtHighLimit.Location = new System.Drawing.Point(140, 92);
            lblPort.Visible = true;
            txtPortA.Visible = true;
            txtPortB.Visible = true;
            grpDriveOptions.Size = new System.Drawing.Size(318, 128);

            if (JointDriver.IsMotor(cType) == false && JointDriver.IsPneumatic(cType) == false)
            {
                this.Height = 300;
                btnSave.Location = new System.Drawing.Point(13, 220);
                grpWheelOptions.Visible = false;
                grpGearRatio.Visible = false;
                grpPneumaticSpecs.Visible = false;
                grpDriveOptions.Visible = true;
            }

            else if (JointDriver.IsMotor(cType) == true || JointDriver.IsPneumatic(cType) == true)
            {
                if (JointDriver.IsMotor(cType) == true)
                {
                    this.Height = 420;
                    btnSave.Location = new System.Drawing.Point(13, 340);
                    grpWheelOptions.Visible = true;
                    grpGearRatio.Visible = true;
                    grpPneumaticSpecs.Visible = false;
                    grpDriveOptions.Visible = true;
                }
                else if (JointDriver.IsPneumatic(cType) == true)
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

        JointDriverType cType = typeOptions[cmbJointDriver.SelectedIndex];

        joint.cDriver = new JointDriver(cType);

        joint.cDriver.portA = (int)txtPortA.Value;
        joint.cDriver.portB = (int)txtPortB.Value;
        joint.cDriver.lowerLimit = (float)txtLowLimit.Value;
        joint.cDriver.upperLimit = (float)txtHighLimit.Value;

        //Only need to store wheel driver if run by motor and is a wheel.
        if (JointDriver.IsMotor(cType) && wheelType != WheelType.NOT_A_WHEEL)
        {
            WheelAnalyzer.SaveToJoint(wheelType, friction, node);
        }
        Hide();

        if (JointDriver.IsPneumatic(cType))
        {
            PneumaticAnalyzer.SaveToPneumaticJoint(diameter, pressure, node);
        }
        Hide();

    }

    private void cmbWheelType_SelectedIndexChanged(object sender, EventArgs e)
    {
        wheelType = (WheelType)cmbWheelType.SelectedIndex;

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
        friction = (FrictionLevel)cmbFrictionLevel.SelectedIndex;
    }

    private void cmbPneumaticDiameter_SelectedIndexChanged(object sender, EventArgs e)
    {
        diameter = (PneumaticDiameter)cmbPneumaticDiameter.SelectedIndex;
    }

    private void cmbPneumaticForce_SelectedIndexChanged(object sender, EventArgs e)
    {
        pressure = (PneumaticPressure)cmbPneumaticPressure.SelectedIndex;
    }
}