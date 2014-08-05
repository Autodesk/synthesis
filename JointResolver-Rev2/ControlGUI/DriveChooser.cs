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
    public int pneumaticType;
    public string pnuematicTypeName;
    private JointDriverType driverType;
    private PneumaticDiameter diameter;
    private PneumaticPressure pressure;
    RigidNode node;


    public void ShowDialog(SkeletalJoint_Base joint, RigidNode node)
    {
        this.joint = joint;
        this.node = node;
        typeOptions = JointDriver.GetAllowedDrivers(joint);
        cmbJointDriver.Items.Clear();
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
        JointDriverType cType = typeOptions[cmbJointDriver.SelectedIndex];
        lblPort.Text = cType.GetPortType() + " Port" + (cType.HasTwoPorts() ? "s" : "");
        txtPortB.Visible = cType.HasTwoPorts();
        txtPortA.Maximum = txtPortB.Maximum = cType.GetPortMax();

        if (!cType.IsMotor() && !cType.IsPneumatic())
        {
            this.Height = 300;
            btnSave.Location = new System.Drawing.Point(13, 220);
            grpWheelOptions.Visible = false;
            grpGearRatio.Visible = false;
            grpPneumaticSpecs.Visible = false;
        }
        else if (cType.IsMotor())
        {
            this.Height = 420;
            btnSave.Location = new System.Drawing.Point(13, 340);
            grpWheelOptions.Visible = true;
            grpGearRatio.Visible = true;
            grpPneumaticSpecs.Visible = false;
        }
        else if (cType.IsPneumatic())
        {
            this.Height = 360;
            btnSave.Location = new System.Drawing.Point(13, 280);
            grpPneumaticSpecs.Visible = true;
            grpWheelOptions.Visible = false;
            grpGearRatio.Visible = false;
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

        joint.cDriver.portA = (int) txtPortA.Value;
        joint.cDriver.portB = (int) txtPortB.Value;
        joint.cDriver.lowerLimit = (float) txtLowLimit.Value;
        joint.cDriver.upperLimit = (float) txtHighLimit.Value;

        //Only need to store wheel driver if run by motor and is a wheel.
        if (cType.IsMotor() && wheelType != WheelType.NOT_A_WHEEL)
        {
            WheelAnalyzer.SaveToJoint(wheelType, friction, node);
        }

        Hide();

        if (cType.IsPneumatic())
        {
            //WheelAnalyzer.SaveToPneumaticJoint(driverType, diameter, pressure, node);
        }

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

    private void lblVelocity_Click(object sender, EventArgs e)
    {

    }

    private void lblPneumaticVelocityTell_Click(object sender, EventArgs e)
    {

    }

    private void lblPneumaticForceTell_Click(object sender, EventArgs e)
    {

    }

    private void txtPortA_ValueChanged(object sender, EventArgs e)
    {

    }

    private void txtPortB_ValueChanged(object sender, EventArgs e)
    {

    }

    private void txtLowLimit_ValueChanged(object sender, EventArgs e)
    {

    }

    private void lblPort_Click(object sender, EventArgs e)
    {

    }

    private void grpDriveOptions_Enter(object sender, EventArgs e)
    {

    }

    private void lblForce_Click(object sender, EventArgs e)
    {

    }

    private void lblOver_Click(object sender, EventArgs e)
    {

    }

    private void grpWheelOptions_Enter(object sender, EventArgs e)
    {

    }
}