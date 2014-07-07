using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class DriveChooser : Form
{
    public DriveChooser()
    {
        InitializeComponent();
    }

    private JointDriverType[] typeOptions;
    private SkeletalJoint_Base joint;
    private WheelPosition position;

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
            txtLowLimit.Value = (decimal)joint.cDriver.lowerLimit;
            txtHighLimit.Value = (decimal)joint.cDriver.upperLimit;
        }
        ShowDialog();
    }

    private void cmbJointDriver_SelectedIndexChanged(object sender, EventArgs e)
    {
        JointDriverType cType = typeOptions[cmbJointDriver.SelectedIndex];
        lblPort.Text = JointDriver.GetPortType(cType) + " Port" + (JointDriver.HasTwoPorts(cType) ? "s" : "");
        txtPortB.Visible = JointDriver.HasTwoPorts(cType);
        txtPortA.Maximum = txtPortB.Maximum = JointDriver.GetPortMax(cType);
        groupBox1.Visible = JointDriver.IsMotor(cType);
        bool windowReducer = JointDriver.IsMotor(cType);
        if (windowReducer == true)
        {
            btnSave.Location = new Point (13, 280);
            btnSave.Visible = true;
        } 
        else if (windowReducer == false)
        {
            this.Height -= 60;
            btnSave.Location = new Point (13, 220);
            btnSave.Visible = true;
        }
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        WheelDriverMeta wheelDriver;
        JointDriverType cType = typeOptions[cmbJointDriver.SelectedIndex];
        joint.cDriver = new JointDriver(cType);
        joint.cDriver.portA = (int)txtPortA.Value;
        joint.cDriver.portB = (int)txtPortB.Value;
        joint.cDriver.lowerLimit = (float)txtLowLimit.Value;
        joint.cDriver.upperLimit = (float)txtHighLimit.Value;

        //Only need to store wheel driver if run by motor and is a wheel.
        if (cType == JointDriverType.MOTOR && position != WheelPosition.NO_WHEEL)
        {
            wheelDriver = (WheelDriverMeta)JointDriverMeta.Create(JointDriverMetaType.WHEEL_DRIVER);
            wheelDriver.position = this.position;
            joint.cDriver.AddInfo(JointDriverMeta.Create(JointDriverMetaType.WHEEL_DRIVER));
        }

        Hide();
    }

    private void cmbWheelPosition_SelectedIndexChanged(object sender, EventArgs e)
    {
         position = (WheelPosition)cmbWheelPosition.SelectedIndex;
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
}