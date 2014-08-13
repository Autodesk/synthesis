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

            #region Meta info recovery
            {
                PneumaticDriverMeta pneumaticMeta = joint.cDriver.GetInfo<PneumaticDriverMeta>();
                if (pneumaticMeta != null)
                {
                    cmbPneumaticDiameter.SelectedIndex = (byte) pneumaticMeta.widthEnum;
                    cmbPneumaticPressure.SelectedIndex = (byte) pneumaticMeta.pressureEnum;
                }
                else
                {
                    cmbPneumaticDiameter.SelectedIndex = (byte) PneumaticDiameter.MEDIUM;
                    cmbPneumaticPressure.SelectedIndex = (byte) PneumaticPressure.HIGH;
                }
            }
            {
                WheelDriverMeta wheelMeta = joint.cDriver.GetInfo<WheelDriverMeta>();
                if (wheelMeta != null)
                {
                    // TODO:  This is a really sketchy hack and I don't even know where the cat is.
                    cmbWheelType.SelectedIndex = (byte) wheelMeta.type;
                    if (wheelMeta.forwardExtremeValue > 8)
                        cmbFrictionLevel.SelectedIndex = (byte) FrictionLevel.HIGH;
                    else if (wheelMeta.forwardExtremeValue > 4)
                        cmbFrictionLevel.SelectedIndex = (byte) FrictionLevel.MEDIUM;
                    else
                        cmbFrictionLevel.SelectedIndex = (byte) FrictionLevel.LOW;
                    cmbWheelType_SelectedIndexChanged(null, null);
                }
                else
                {
                    cmbWheelType.SelectedIndex = (byte) WheelType.NOT_A_WHEEL;
                    cmbFrictionLevel.SelectedIndex = (byte) FrictionLevel.MEDIUM;
                }
            }
            #endregion
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
            }
            else if (cType.IsPneumatic())
            {
                tabsMeta.Visible = true;
                tabsMeta.TabPages.Clear();
                tabsMeta.TabPages.Add(metaPneumatic);
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
            if (cType.IsMotor() && (WheelType) cmbWheelType.SelectedIndex != WheelType.NOT_A_WHEEL)
            {
                #region WHEEL_SAVING
                WheelDriverMeta wheelDriver = new WheelDriverMeta(); //The info about the wheel attached to the joint.
                wheelDriver.type = (WheelType) cmbWheelType.SelectedIndex;

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
                PneumaticDriverMeta pneumaticDriver = new PneumaticDriverMeta(); //The info about the wheel attached to the joint.
                pneumaticDriver.pressureEnum = (PneumaticPressure) cmbPneumaticPressure.SelectedIndex;
                pneumaticDriver.widthEnum = (PneumaticDiameter) cmbPneumaticDiameter.SelectedIndex;
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
        cmbFrictionLevel.Visible = (WheelType) cmbWheelType.SelectedIndex != WheelType.NOT_A_WHEEL;
    }
}