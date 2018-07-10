using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            BackColor = Color.White;
            MetaTabControl.AutoSize = true;
            PneumaticTab.AutoSize = true;
            PneumaticTab.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            this.node = node;
            NodeGroupBox.Text = node.ModelFileName;

            DriverComboBox.SelectedIndex = 0;
            DriverComboBox_SelectedIndexChanged(null, null);
            PortOneUpDown.Minimum = 3;
            PortTwoUpDown.Minimum = 3;

            PortOneUpDown.Value = WizardData.Instance.NextFreePort;
            PortTwoUpDown.Value = PortOneUpDown.Value + 1; // This may overlap with ports on next panel, but this only matters if the user chooses a two-port driver, which are less common

            // Add a highlight component action to all children. This is simpler than manually adding the hover event to each control.
            AddHighlightAction(this);
        }

        /// <summary>
        /// Handles all of the different kinds of data that should be displayed when the SelectedIndex of the <see cref="ComboBox"/> is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DriverComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (DriverComboBox.SelectedIndex)
            {
                case 0: // No Driver
                    this.JointLimitGroupBox.Visible = false;
                    this.PortsGroupBox.Visible = false;
                    this.MetaTabControl.Visible = false;
                    break;
                case 1: //Motor
                    this.JointLimitGroupBox.Visible = true;
                    this.PortsGroupBox.Visible = true;
                    MetaTabControl.Visible = false;
                    PortsGroupBox.Text = "Port";
                    PortOneLabel.Visible = false;
                    PortTwoLabel.Visible = false;
                    PortTwoUpDown.Visible = false;
                    UpperLimitUpDown.Maximum = LowerLimitUpDown.Maximum = 360;
                    UpperLimitUpDown.Minimum = LowerLimitUpDown.Minimum = 0;
                    unit = "°";
                    break;
                case 2: //Servo
                    this.JointLimitGroupBox.Visible = true;
                    this.PortsGroupBox.Visible = true;
                    this.MetaTabControl.Visible = false;
                    PortsGroupBox.Text = "Port";
                    PortOneLabel.Visible = false;
                    PortTwoLabel.Visible = false;
                    PortTwoUpDown.Visible = false;
                    unit = "cm";
                    break;
                case 3: //Bumper Pneumatics
                    this.JointLimitGroupBox.Visible = true;
                    this.PortsGroupBox.Visible = true;
                    MetaTabControl.Visible = true;
                    if(!MetaTabControl.TabPages.Contains(PneumaticTab)) MetaTabControl.TabPages.Add(PneumaticTab);
                    PortsGroupBox.Text = "Ports";
                    PortOneLabel.Visible = true;
                    PortTwoLabel.Visible = true;
                    PortTwoUpDown.Visible = true;
                    unit = "cm";
                    break;
                case 4: //Relay Pneumatics
                    this.JointLimitGroupBox.Visible = true;
                    this.PortsGroupBox.Visible = true;
                    MetaTabControl.Visible = true;
                    if(!MetaTabControl.TabPages.Contains(PneumaticTab)) MetaTabControl.TabPages.Add(PneumaticTab);
                    PortsGroupBox.Text = "Port";
                    PortOneLabel.Visible = false;
                    PortTwoLabel.Visible = false;
                    PortTwoUpDown.Visible = false;
                    unit = "cm";
                    break;
                case 5: //Dual Motor
                    this.JointLimitGroupBox.Visible = true;
                    this.PortsGroupBox.Visible = true;
                    this.MetaTabControl.Visible = false;
                    PortsGroupBox.Text = "Ports";
                    PortOneLabel.Visible = true;
                    PortTwoLabel.Visible = true;
                    PortTwoUpDown.Visible = true;
                    UpperLimitUpDown.Maximum = LowerLimitUpDown.Maximum = 360;
                    UpperLimitUpDown.Minimum = LowerLimitUpDown.Minimum = 0;
                    unit = "°";
                    break;
            }

        }

        private void UpperLimitUpDown_ValueChanged(object sender, EventArgs e)
        {
            TotalFreedomLabel.Text = (UpperLimitUpDown.Value - LowerLimitUpDown.Value).ToString() + unit;
        }

        private void LowerLimitUpDown_ValueChanged(object sender, EventArgs e)
        {
            TotalFreedomLabel.Text = (UpperLimitUpDown.Value - LowerLimitUpDown.Value).ToString() + unit;
        }

        /// <summary>
        /// Highlights the node in inventor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HighlightNode(object sender, EventArgs e)
        {
            StandardAddInServer.Instance.WizardSelect(node);
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
            switch(DriverComboBox.SelectedIndex)
            {
                case 1: //Motor
                    JointDriver driver = new JointDriver(JointDriverType.MOTOR);
                    ((RotationalJoint_Base)node.GetSkeletalJoint()).hasAngularLimit = true;
                    ((RotationalJoint_Base)node.GetSkeletalJoint()).angularLimitLow = (float)(LowerLimitUpDown.Value * (decimal)(Math.PI / 180));
                    ((RotationalJoint_Base)node.GetSkeletalJoint()).angularLimitHigh = (float)(UpperLimitUpDown.Value * (decimal)(Math.PI / 180));
                    driver.SetPort((int)PortOneUpDown.Value, 1);
                    return driver;
                case 2: //Servo
                    driver = new JointDriver(JointDriverType.SERVO);
                    driver.SetLimits((float)(LowerLimitUpDown.Value / 100), (float)(UpperLimitUpDown.Value / 100));
                    driver.SetPort((int)PortOneUpDown.Value, 1);
                    return driver;
                case 3: //Bumper Pneumatic
                    driver = new JointDriver(JointDriverType.BUMPER_PNEUMATIC);
                    driver.SetLimits((float)(LowerLimitUpDown.Value / 100), (float)(UpperLimitUpDown.Value / 100));
                    driver.SetPort((int)PortOneUpDown.Value, (int)PortTwoUpDown.Value);
                    return driver;
                case 4: //Relay Pneumatic
                    driver = new JointDriver(JointDriverType.RELAY_PNEUMATIC);
                    driver.SetLimits((float)(LowerLimitUpDown.Value / 100), (float)(UpperLimitUpDown.Value / 100));
                    driver.SetPort((int)PortOneUpDown.Value, 1);
                    return driver;
                case 5: //Dual Motor
                    driver = new JointDriver(JointDriverType.DUAL_MOTOR);
                    ((RotationalJoint_Base)node.GetSkeletalJoint()).hasAngularLimit = true;
                    ((RotationalJoint_Base)node.GetSkeletalJoint()).angularLimitLow = (float)(LowerLimitUpDown.Value * (decimal)(Math.PI / 180));
                    ((RotationalJoint_Base)node.GetSkeletalJoint()).angularLimitHigh = (float)(UpperLimitUpDown.Value * (decimal)(Math.PI / 180));
                    driver.SetPort((int)PortOneUpDown.Value, (int)PortTwoUpDown.Value);
                    return driver;
            }
            return null;
        }
    }
}