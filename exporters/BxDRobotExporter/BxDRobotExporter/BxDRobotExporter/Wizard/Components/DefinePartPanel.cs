using System;
using System.Drawing;
using System.Windows.Forms;
using Inventor;

namespace BxDRobotExporter.Wizard
{
    /// <summary>
    /// Used in the <see cref="DefineMovingPartsPage"/> to set joint driver information.
    /// </summary>
    /// <remarks>This is the most manual part of the entire guided export and could probably be improved.</remarks>
    public partial class DefinePartPanel : UserControl
    {
        /// <summary>
        /// The current unit of measure for the joint/node.
        /// </summary>
        private string unit = "°";

        /// <summary>
        /// Property stating whether the node has been merged into its parent.
        /// </summary>
        public bool Merged { get => WizardData.Instance.MergeQueue.Contains(node); }

        /// <summary>
        /// The <see cref="RigidNode_Base"/> which the driver will be applied to.
        /// </summary>
        public RigidNode_Base node;

        public DefinePartPanel(RigidNode_Base node)
        {
            
            InitializeComponent();
            BackColor = System.Drawing.Color.White;
            Dock = DockStyle.Top;
            MinimumSize = new Size(0, 0); // Minimum size only needed in editor

            this.node = node;

            string readableName = node.ModelFileName.Replace('_', ' ').Replace(".bxda", "");
            readableName = readableName.Substring(0, 1).ToUpperInvariant() + readableName.Substring(1); // Capitalize first character
            NodeGroupBox.Text = readableName;


            int nextPort = WizardData.Instance.NextFreePort;
            if (nextPort < PortOneUpDown.Maximum - 1)
            {
                PortOneUpDown.Value = nextPort;
                PortTwoUpDown.Value = nextPort + 1; // This may overlap with ports on next panel, but this only matters if the user chooses a two-port driver, which are less common
            }
            else
            {
                PortOneUpDown.Value = PortOneUpDown.Maximum - 1;
                PortTwoUpDown.Value = PortOneUpDown.Maximum;
            }
            this.rbPWM.Checked = true;
            // Add a highlight component action to all children. This is simpler than manually adding the hover event to each control.
            AddHighlightAction(this);
            if (!(node.GetSkeletalJoint().GetJointType().ToAssemblyJointType() == AssemblyJointTypeEnum.kCylindricalJointType ||// if the joint is a rotational then enable the rotation stuff and disable the linear
                    node.GetSkeletalJoint().GetJointType().ToAssemblyJointType() == AssemblyJointTypeEnum.kSlideJointType))
            {
                this.DriverComboBox.Items.Clear();
                this.DriverComboBox.Items.AddRange(new object[] {
                    "No Driver",
                    "Motor",
                    "Servo",
                    "Bumper Pneumatic",
                    "Relay Pneumatic",
                    "Worm Screw",
                    "Dual Motor"});
            } else
            {
                this.DriverComboBox.Items.Clear();
                this.DriverComboBox.Items.AddRange(new object[] {
                    "No Driver",
                    "Elevator",
                    "Bumper Pneumatic",
                    "Relay Pneumatic",
                    "Worm Screw"});
            }
            DriverComboBox.SelectedIndex = 0;
            DriverComboBox_SelectedIndexChanged(null, null);
        }
        /// <summary>
        /// refills values from existing joint
        /// </summary>
        /// <param name="joint"></param>
        public void refillValues(SkeletalJoint_Base joint)
        {
            JointDriverType[] typeOptions;
            typeOptions = JointDriver.GetAllowedDrivers(joint);
            DriverComboBox.SelectedIndex = Array.IndexOf(typeOptions, joint.cDriver.GetDriveType()) + 1;

            if (rbCAN.Checked)
            {
                PortOneUpDown.Maximum = PortTwoUpDown.Maximum = 64;
            }
            else
            {
                PortOneUpDown.Maximum = PortTwoUpDown.Maximum = 20;
            }

            if (joint.cDriver.port1 < PortOneUpDown.Minimum)
                PortOneUpDown.Value = PortOneUpDown.Minimum;
            else if (joint.cDriver.port1 > PortOneUpDown.Maximum)
                PortOneUpDown.Value = PortOneUpDown.Maximum;
            else
                PortOneUpDown.Value = joint.cDriver.port1;

            if (joint.cDriver.port2 < PortTwoUpDown.Minimum)
                PortTwoUpDown.Value = PortTwoUpDown.Minimum;
            else if (joint.cDriver.port2 > PortTwoUpDown.Maximum)
                PortTwoUpDown.Value = PortTwoUpDown.Maximum;
            else
                PortTwoUpDown.Value = joint.cDriver.port2;
            
            rbCAN.Checked = joint.cDriver.isCan;

            //if (joint.cDriver.OutputGear == 0)// prevents output gear from being 0
            //{
            //    joint.cDriver.OutputGear = 1;
            //}
            //if (joint.cDriver.InputGear == 0)// prevents input gear from being 0
            //{
            //    joint.cDriver.InputGear = 1;
            //}

            OutputGeartxt.Value = 1;// reads the existing gearing and writes it to the input field so the user sees their existing value
            InputGeartxt.Value = 1;// reads the existing gearing and writes it to the input field so the user sees their existing value

            chkBoxHasBrake.Checked = joint.cDriver.hasBrake;

            #region Meta info recovery
            {
                PneumaticDriverMeta pneumaticMeta = joint.cDriver.GetInfo<PneumaticDriverMeta>();
                if (pneumaticMeta != null)
                {
                    numericUpDownPnuDia.Value = (decimal)pneumaticMeta.width;
                    cmbPneumaticPressure.SelectedIndex = (int)pneumaticMeta.pressureEnum;
                }
                else
                {
                    numericUpDownPnuDia.Value = (decimal)0.5;
                    cmbPneumaticPressure.SelectedIndex = (int)PneumaticPressure.HIGH;
                }
            }
            {
                ElevatorDriverMeta elevatorMeta = joint.cDriver.GetInfo<ElevatorDriverMeta>();
            }
            #endregion
        }
        /// <summary>
        /// Handles all of the different kinds of data that should be displayed when the SelectedIndex of the <see cref="ComboBox"/> is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DriverComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(node.GetSkeletalJoint().GetJointType().ToAssemblyJointType() == AssemblyJointTypeEnum.kCylindricalJointType ||// if the joint is a rotational then enable the rotation stuff and disable the linear
                    node.GetSkeletalJoint().GetJointType().ToAssemblyJointType() == AssemblyJointTypeEnum.kSlideJointType))
            {
                switch (DriverComboBox.SelectedIndex)
                {
                    case 0: // No Driver
                        this.PortsGroupBox.Visible = false;
                        this.tabsMeta.Visible = false;
                        break;
                    case 1: //Motor
                        this.PortsGroupBox.Visible = true;
                        tabsMeta.Visible = true;
                        this.PortLayout.RowStyles[1].SizeType = SizeType.Percent;
                        this.PortLayout.RowStyles[1].Height = 50F;
                        PortsGroupBox.Text = "Port";
                        PortOneLabel.Text = "Port:";
                        if (!tabsMeta.TabPages.Contains(metaBrake)) tabsMeta.TabPages.Add(metaBrake);
                        if (tabsMeta.TabPages.Contains(metaPneumatic)) tabsMeta.TabPages.Remove(metaPneumatic);
                        if (tabsMeta.TabPages.Contains(metaGearing)) tabsMeta.TabPages.Remove(metaGearing);
                        PortOneLabel.Visible = true;
                        PortTwoLabel.Visible = false;
                        PortTwoUpDown.Visible = false;
                        unit = "°";
                        break;
                    case 2: //Servo
                        this.PortsGroupBox.Visible = true;
                        this.PortLayout.RowStyles[1].SizeType = SizeType.Absolute;
                        this.PortLayout.RowStyles[1].Height = 0;
                        this.tabsMeta.Visible = false;
                        PortsGroupBox.Text = "PWM Port";
                        PortOneLabel.Text = "PWM Port:";
                        PortOneLabel.Visible = true;
                        PortTwoLabel.Visible = false;
                        PortTwoUpDown.Visible = false;
                        unit = "°";
                        break;
                     case 3: //Bumper Pneumatics
                        this.PortsGroupBox.Visible = true;
                        tabsMeta.Visible = true;
                        this.PortLayout.RowStyles[1].SizeType = SizeType.Absolute;
                        this.PortLayout.RowStyles[1].Height = 0;
                        PortOneLabel.Text = "Solenoid Port 1:";
                        PortTwoLabel.Text = "Solenoid Port 2:";
                        if (!tabsMeta.TabPages.Contains(metaPneumatic)) tabsMeta.TabPages.Add(metaPneumatic);
                        if (tabsMeta.TabPages.Contains(metaBrake)) tabsMeta.TabPages.Remove(metaBrake);
                        if (tabsMeta.TabPages.Contains(metaGearing)) tabsMeta.TabPages.Remove(metaGearing);
                        PortsGroupBox.Text = "Solenoid Ports";
                        PortOneLabel.Visible = true;
                        PortTwoLabel.Visible = true;
                        PortTwoUpDown.Visible = true;
                        unit = "cm";
                        break;
                    case 4: //Relay Pneumatics
                        this.PortsGroupBox.Visible = true;
                        tabsMeta.Visible = true;
                        this.PortLayout.RowStyles[1].SizeType = SizeType.Absolute;
                        this.PortLayout.RowStyles[1].Height = 0;
                        PortOneLabel.Text = "Relay Port:";
                        if (!tabsMeta.TabPages.Contains(metaPneumatic)) tabsMeta.TabPages.Add(metaPneumatic);
                        if (tabsMeta.TabPages.Contains(metaBrake)) tabsMeta.TabPages.Remove(metaBrake);
                        if (tabsMeta.TabPages.Contains(metaGearing)) tabsMeta.TabPages.Remove(metaGearing);
                        PortsGroupBox.Text = "Relay Port";
                        PortOneLabel.Visible = true;
                        PortTwoLabel.Visible = false;
                        PortTwoUpDown.Visible = false;
                        unit = "cm"; 
                        break;
                    case 5: //Worm Screw
                        this.PortsGroupBox.Visible = true;
                        this.tabsMeta.Visible = false;
                        this.PortLayout.RowStyles[1].SizeType = SizeType.Percent;
                        this.PortLayout.RowStyles[1].Height = 50F;
                        PortsGroupBox.Text = "PWM Port";
                        PortOneLabel.Text = "PWM Port:";
                        PortOneLabel.Visible = true;
                        PortTwoLabel.Visible = false;
                        PortTwoUpDown.Visible = false;
                        unit = "°";
                        break;
                    case 6: //Dual Motor
                        this.PortsGroupBox.Visible = true;
                        this.tabsMeta.Visible = true;
                        this.PortLayout.RowStyles[1].SizeType = SizeType.Percent;
                        this.PortLayout.RowStyles[1].Height = 50F;
                        PortsGroupBox.Text = "Ports";
                        PortOneLabel.Text = "Port 1:";
                        PortTwoLabel.Text = "Port 2:";
                        if (!tabsMeta.TabPages.Contains(metaBrake)) tabsMeta.TabPages.Add(metaBrake);
                        if (tabsMeta.TabPages.Contains(metaPneumatic)) tabsMeta.TabPages.Remove(metaPneumatic);
                        if (tabsMeta.TabPages.Contains(metaGearing)) tabsMeta.TabPages.Remove(metaGearing);
                        PortOneLabel.Visible = true;
                        PortTwoLabel.Visible = true;
                        PortTwoUpDown.Visible = true;
                        unit = "°";
                        break;
                }
            } else
            {
                switch (DriverComboBox.SelectedIndex)
                {
                    case 0: // No Driver
                        this.PortsGroupBox.Visible = false;
                        this.tabsMeta.Visible = false;
                        break;
                    case 1: //Elevator
                        this.PortsGroupBox.Visible = true;
                        tabsMeta.Visible = true;
                        this.PortLayout.RowStyles[1].SizeType = SizeType.Percent;
                        this.PortLayout.RowStyles[1].Height = 50F;
                        PortsGroupBox.Text = "PWM Port";
                        PortOneLabel.Text = "PWM Port:";
                        if (!tabsMeta.TabPages.Contains(metaBrake)) tabsMeta.TabPages.Add(metaBrake);
                        if (tabsMeta.TabPages.Contains(metaPneumatic)) tabsMeta.TabPages.Remove(metaPneumatic);
                        if (tabsMeta.TabPages.Contains(metaGearing)) tabsMeta.TabPages.Remove(metaGearing);
                        PortOneLabel.Visible = true;
                        PortTwoLabel.Visible = false;
                        PortTwoUpDown.Visible = false;
                        unit = "°";
                        break;
                    case 2: //Bumper Pneumatics
                        this.PortsGroupBox.Visible = true;
                        tabsMeta.Visible = true;
                        this.PortLayout.RowStyles[1].SizeType = SizeType.Absolute;
                        this.PortLayout.RowStyles[1].Height = 0;
                        PortOneLabel.Text = "Solenoid Port 1:";
                        PortTwoLabel.Text = "Solenoid Port 2:";
                        if (!tabsMeta.TabPages.Contains(metaPneumatic)) tabsMeta.TabPages.Add(metaPneumatic);
                        if (tabsMeta.TabPages.Contains(metaBrake)) tabsMeta.TabPages.Remove(metaBrake);
                        if (tabsMeta.TabPages.Contains(metaGearing)) tabsMeta.TabPages.Remove(metaGearing);
                        PortsGroupBox.Text = "Solenoid Ports";
                        PortOneLabel.Visible = true;
                        PortTwoLabel.Visible = true;
                        PortTwoUpDown.Visible = true;
                        unit = "cm";
                        break;
                    case 3: //Relay Pneumatics
                        this.PortsGroupBox.Visible = true;
                        tabsMeta.Visible = true;
                        this.PortLayout.RowStyles[1].SizeType = SizeType.Absolute;
                        this.PortLayout.RowStyles[1].Height = 0;
                        PortOneLabel.Text = "Relay Port:";
                        if (!tabsMeta.TabPages.Contains(metaPneumatic)) tabsMeta.TabPages.Add(metaPneumatic);
                        if (tabsMeta.TabPages.Contains(metaBrake)) tabsMeta.TabPages.Remove(metaBrake);
                        if (tabsMeta.TabPages.Contains(metaGearing)) tabsMeta.TabPages.Remove(metaGearing);
                        PortsGroupBox.Text = "Relay Port";
                        PortOneLabel.Visible = true;
                        PortTwoLabel.Visible = false;
                        PortTwoUpDown.Visible = false;
                        unit = "cm";
                        break;
                    case 4: //Worm Screw
                        this.PortsGroupBox.Visible = true;
                        this.tabsMeta.Visible = false;
                        this.PortLayout.RowStyles[1].SizeType = SizeType.Percent;
                        this.PortLayout.RowStyles[1].Height = 50F;
                        PortsGroupBox.Text = "PWM Port";
                        PortOneLabel.Text = "PWM Port:";
                        PortOneLabel.Visible = true;
                        PortTwoLabel.Visible = false;
                        PortTwoUpDown.Visible = false;
                        unit = "°";
                        break;
                }
            }
        }
        

        /// <summary>
        /// Highlights the node in inventor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HighlightNode(object sender, EventArgs e)
        {
            InventorUtils.FocusAndHighlightNode(node, StandardAddInServer.Instance.MainApplication.ActiveView.Camera, 0.8);
        }

        private void AddHighlightAction(Control baseControl)
        {
            baseControl.MouseHover += HighlightNode;

            foreach (Control control in baseControl.Controls)
                AddHighlightAction(control);
        }

        /// <summary>
        /// Gets the <see cref="JointDriver"/> which will be applied to <see cref="DefinePartPanel.node"/>
        /// </summary>
        /// <returns></returns>
        public JointDriver GetJointDriver()
        {
            if (Merged)
                return null;
            if (!(node.GetSkeletalJoint().GetJointType().ToAssemblyJointType() == AssemblyJointTypeEnum.kCylindricalJointType ||// if the joint is a rotational then enable the rotation stuff and disable the linear
                    node.GetSkeletalJoint().GetJointType().ToAssemblyJointType() == AssemblyJointTypeEnum.kSlideJointType))
            {
                switch (DriverComboBox.SelectedIndex)
                {
                    case 1: //Motor
                        JointDriver driver = new JointDriver(JointDriverType.MOTOR);
                        driver.motor = MotorType.GENERIC;
                        driver.InputGear = 1;
                        driver.OutputGear = 1;
                        driver.hasBrake = chkBoxHasBrake.Checked;
                        driver.SetPort((int)PortOneUpDown.Value, 1);
                        driver.isCan = this.rbCAN.Checked;
                        return driver;
                    case 2: //Servo
                        driver = new JointDriver(JointDriverType.SERVO);
                        driver.SetPort((int)PortOneUpDown.Value, 1);
                        return driver;
                    case 3: //Bumper Pneumatic
                        driver = new JointDriver(JointDriverType.BUMPER_PNEUMATIC);
                        PneumaticDriverMeta pneumaticDriver = new PneumaticDriverMeta()
                        {
                            pressureEnum = (PneumaticPressure)cmbPneumaticPressure.SelectedIndex,
                            width = (double) numericUpDownPnuDia.Value
                        }; //The info about the wheel attached to the joint.
                        driver.AddInfo(pneumaticDriver);
                        driver.SetPort((int)PortOneUpDown.Value, (int)PortTwoUpDown.Value);
                        return driver;
                    case 4: //Relay Pneumatic
                        driver = new JointDriver(JointDriverType.RELAY_PNEUMATIC);
                        PneumaticDriverMeta pneumaticDriver2 = new PneumaticDriverMeta()
                        {
                            pressureEnum = (PneumaticPressure)cmbPneumaticPressure.SelectedIndex,
                            width = (double)numericUpDownPnuDia.Value
                        }; //The info about the wheel attached to the joint.
                        driver.AddInfo(pneumaticDriver2);
                        driver.SetPort((int)PortOneUpDown.Value, 1);
                        return driver;
                    case 5: //Worm Screw
                        driver = new JointDriver(JointDriverType.WORM_SCREW);
                        driver.SetPort((int)PortOneUpDown.Value);
                        driver.isCan = this.rbCAN.Checked;
                        return driver;
                    case 6: //Dual Motor
                        driver = new JointDriver(JointDriverType.DUAL_MOTOR);
                        driver.hasBrake = chkBoxHasBrake.Checked;
                        driver.SetPort((int)PortOneUpDown.Value, (int)PortTwoUpDown.Value);
                        driver.isCan = this.rbCAN.Checked;
                        return driver;
                }
                return null;
            }
            else
            {

                switch (DriverComboBox.SelectedIndex)
                {
                    case 1: //Elevator
                        JointDriver driver = new JointDriver(JointDriverType.ELEVATOR);
                        driver.InputGear = (double)InputGeartxt.Value;
                        driver.OutputGear = (double)OutputGeartxt.Value;
                        ElevatorDriverMeta elevatorDriver = new ElevatorDriverMeta()
                        {
                            type = ElevatorType.NOT_MULTI
                        }; //The info about the wheel attached to the joint.
                        driver.hasBrake = chkBoxHasBrake.Checked;
                        driver.AddInfo(elevatorDriver);
                        driver.SetPort((int)PortOneUpDown.Value, 1);
                        driver.isCan = this.rbCAN.Checked;
                        return driver;
                    case 2: //Bumper Pneumatic
                        driver = new JointDriver(JointDriverType.BUMPER_PNEUMATIC);
                        PneumaticDriverMeta pneumaticDriver = new PneumaticDriverMeta()
                        {
                            pressureEnum = (PneumaticPressure)cmbPneumaticPressure.SelectedIndex,
                            width = (double)numericUpDownPnuDia.Value
                        }; //The info about the wheel attached to the joint.
                        driver.AddInfo(pneumaticDriver);
                        driver.SetPort((int)PortOneUpDown.Value, (int)PortTwoUpDown.Value);
                        return driver;
                    case 3: //Relay Pneumatic
                        driver = new JointDriver(JointDriverType.RELAY_PNEUMATIC);
                        PneumaticDriverMeta pneumaticDriver2 = new PneumaticDriverMeta()
                        {
                            pressureEnum = (PneumaticPressure)cmbPneumaticPressure.SelectedIndex,
                            width = (double)numericUpDownPnuDia.Value
                        }; //The info about the wheel attached to the joint.
                        driver.AddInfo(pneumaticDriver2);
                        driver.SetPort((int)PortOneUpDown.Value, 1);
                        return driver;
                    case 4: //Worm Screw
                        driver = new JointDriver(JointDriverType.WORM_SCREW);
                        driver.SetPort((int)PortOneUpDown.Value);
                        driver.isCan = this.rbCAN.Checked;
                        return driver;
                }
                return null;

            }
        }
    }
}