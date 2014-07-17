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

    public void ShowDialog(SkeletalJoint_Base joint)
    {
        this.joint = joint;
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
        lblPort.Text = JointDriver.GetPortType(cType) + " Port" + (JointDriver.HasTwoPorts(cType) ? "s" : "");
        txtPortB.Visible = JointDriver.HasTwoPorts(cType);
        txtPortA.Maximum = txtPortB.Maximum = JointDriver.GetPortMax(cType);
        if (JointDriver.IsMotor(cType) == true)
        {
            this.Height = 360;
            btnSave.Location = new System.Drawing.Point(13, 280);
            btnSave.Visible = true;
            groupBox2.Visible = true;
        }
        else if (JointDriver.IsMotor(cType) == false)
        {
            this.Height = 300;
            btnSave.Location = new System.Drawing.Point(13, 220);
            btnSave.Visible = true;
            groupBox2.Visible = false;
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
        if (JointDriver.IsMotor(cType) && wheelType != WheelType.NOT_A_WHEEL)
        {
            WheelAnalyzer.SaveToJoint(joint, wheelType, friction);
        }

        Hide();
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

    private void groupBox1_Enter(object sender, EventArgs e)
    {

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

    private void groupBox2_Enter(object sender, EventArgs e)
    {

    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
        friction = (FrictionLevel)cmbFrictionLevel.SelectedIndex;
    }
}