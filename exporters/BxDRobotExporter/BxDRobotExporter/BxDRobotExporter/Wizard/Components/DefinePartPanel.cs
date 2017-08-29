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
        
        public DefinePartPanel()
        {
            InitializeComponent();
        }
        
        public DefinePartPanel(RigidNode_Base node)
        {
            InitializeComponent();
            BackColor = DefaultBackColor;
            this.node = node;
            this.NodeGroupBox.Text = node.ModelFileName;

            DriverComboBox.SelectedIndex = 0;
            DriverComboBox_SelectedIndexChanged(null, null);
            PortTwoUpDown.Minimum = WizardData.Instance.nextFreePort;
            PortOneUpDown.Minimum = WizardData.Instance.nextFreePort;
        }

        private void AutoAssignCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            PortOneUpDown.Enabled = !AutoAssignCheckBox.Checked;
            if(DriverComboBox.SelectedIndex == 3 || DriverComboBox.SelectedIndex == 5)
            {
                PortTwoUpDown.Enabled = PortOneUpDown.Enabled = !AutoAssignCheckBox.Checked;
            }
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
                    PortTwoLabel.Enabled = false;
                    PortTwoUpDown.Enabled = false;
                    UpperLimitUpDown.Maximum = LowerLimitUpDown.Maximum = 360;
                    UpperLimitUpDown.Minimum = LowerLimitUpDown.Minimum = 0;
                    unit = "°";
                    break;
                case 2: //Servo
                    this.JointLimitGroupBox.Visible = true;
                    this.PortsGroupBox.Visible = true;
                    this.MetaTabControl.Visible = false;
                    PortTwoLabel.Enabled = false;
                    PortTwoUpDown.Enabled = false;
                    unit = "cm";
                    break;
                case 3: //Bumper Pneumatics
                    this.JointLimitGroupBox.Visible = true;
                    this.PortsGroupBox.Visible = true;
                    MetaTabControl.Visible = true;
                    if(!MetaTabControl.TabPages.Contains(PneumaticTab)) MetaTabControl.TabPages.Add(PneumaticTab);
                    PortTwoLabel.Enabled = true;
                    PortTwoUpDown.Enabled = true;
                    unit = "cm";
                    break;
                case 4: //Relay Pneumatics
                    this.JointLimitGroupBox.Visible = true;
                    this.PortsGroupBox.Visible = true;
                    MetaTabControl.Visible = true;
                    if(!MetaTabControl.TabPages.Contains(PneumaticTab)) MetaTabControl.TabPages.Add(PneumaticTab);
                    PortTwoUpDown.Enabled = false;
                    PortTwoLabel.Enabled = false;
                    unit = "cm";
                    break;
                case 5: //Dual Motor
                    this.JointLimitGroupBox.Visible = true;
                    this.PortsGroupBox.Visible = true;
                    this.MetaTabControl.Visible = false;
                    PortTwoLabel.Enabled = true;
                    PortTwoUpDown.Enabled = true;
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
        /// Warns the user and then merges the node into its parent node.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MergeNodeButton_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Are you sure you want to merge this node into the parent node? (Use this only if you don't want this part of the robot to move in the simulation)\n This cannot be undone.", "Merge Node", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                foreach(Control control in this.Controls)
                {
                    if (!(control is Button))
                        control.Enabled = false;
                }
                Label mergedLabel = new Label
                {
                    Text = "Merged " + node.ModelFileName + " into " + node.GetParent().ModelFileName,
                    Font = new Font(DefaultFont.FontFamily, 12.0f),
                    BackColor = System.Windows.Forms.Control.DefaultBackColor,
                    ForeColor = Color.DarkGray,
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                this.Controls.Add(mergedLabel);
                mergedLabel.BringToFront();

                WizardData.Instance.MergeQueue.Add(node);
            }
        }

        /// <summary>
        /// Highlights the node in inventor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InventorHighlightButton_Click(object sender, EventArgs e)
        {
            StandardAddInServer.Instance.WizardSelect(node);
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
                    driver.SetPort((AutoAssignCheckBox.Checked) ? WizardData.Instance.nextFreePort : (int)PortOneUpDown.Value, 1);
                    if (AutoAssignCheckBox.Checked) WizardData.Instance.nextFreePort++;
                    return driver;
                case 2: //Servo
                    driver = new JointDriver(JointDriverType.SERVO);
                    driver.SetLimits((float)(LowerLimitUpDown.Value / 100), (float)(UpperLimitUpDown.Value / 100));
                    driver.SetPort((AutoAssignCheckBox.Checked) ? WizardData.Instance.nextFreePort : (int)PortOneUpDown.Value, 1);
                    if (AutoAssignCheckBox.Checked) WizardData.Instance.nextFreePort++;
                    return driver;
                case 3: //Bumper Pneumatic
                    driver = new JointDriver(JointDriverType.BUMPER_PNEUMATIC);
                    driver.SetLimits((float)(LowerLimitUpDown.Value / 100), (float)(UpperLimitUpDown.Value / 100));
                    driver.SetPort((AutoAssignCheckBox.Checked) ? WizardData.Instance.nextFreePort : (int)PortOneUpDown.Value, (AutoAssignCheckBox.Checked) ? WizardData.Instance.nextFreePort + 1 : (int)PortTwoUpDown.Value);
                    if (AutoAssignCheckBox.Checked) WizardData.Instance.nextFreePort += 2;
                    return driver;
                case 4: //Relay Pneumatic
                    driver = new JointDriver(JointDriverType.RELAY_PNEUMATIC);
                    driver.SetLimits((float)(LowerLimitUpDown.Value / 100), (float)(UpperLimitUpDown.Value / 100));
                    driver.SetPort((AutoAssignCheckBox.Checked) ? WizardData.Instance.nextFreePort : (int)PortOneUpDown.Value, 1);
                    if (AutoAssignCheckBox.Checked) WizardData.Instance.nextFreePort++;
                    return driver;
                case 5: //Dual Motor
                    driver = new JointDriver(JointDriverType.DUAL_MOTOR);
                    ((RotationalJoint_Base)node.GetSkeletalJoint()).hasAngularLimit = true;
                    ((RotationalJoint_Base)node.GetSkeletalJoint()).angularLimitLow = (float)(LowerLimitUpDown.Value * (decimal)(Math.PI / 180));
                    ((RotationalJoint_Base)node.GetSkeletalJoint()).angularLimitHigh = (float)(UpperLimitUpDown.Value * (decimal)(Math.PI / 180));
                    driver.SetPort((AutoAssignCheckBox.Checked) ? WizardData.Instance.nextFreePort : (int)PortOneUpDown.Value, (AutoAssignCheckBox.Checked) ? WizardData.Instance.nextFreePort + 1 : (int)PortTwoUpDown.Value);
                    if (AutoAssignCheckBox.Checked) WizardData.Instance.nextFreePort += 2;
                    return driver;
            }
            return null;
        }
    }
}