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
    public partial class DefinePartPanel : UserControl
    {
        private string unit = "°";

        public bool Merged { get => WizardData.Instance.MergeQueue.Contains(node); }

        public RigidNode_Base node;
        
        public DefinePartPanel()
        {
            InitializeComponent();

        }
        
        public DefinePartPanel(RigidNode_Base node)
        {
            InitializeComponent();
            BackColor = Control.DefaultBackColor;
            this.node = node;
            this.NodeGroupBox.Text = node.ModelFileName;

            DriverComboBox.SelectedIndex = 0;
            DriverComboBox_SelectedIndexChanged(null, null);
        }

        private void AutoAssignCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            PortOneUpDown.Enabled = !AutoAssignCheckBox.Checked;
            if(DriverComboBox.SelectedIndex == 3 || DriverComboBox.SelectedIndex == 5)
            {
                PortTwoUpDown.Enabled = !AutoAssignCheckBox.Checked;
            }
        }

        private void PoweredRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            UnpoweredRadioButton.Checked = !PoweredRadioButton.Checked;
        }

        private void UnpoweredRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            PoweredRadioButton.Checked = !UnpoweredRadioButton.Checked;

        }

        public JointDriverMeta GetJointData()
        {
            return null;
        }

        private void DriverComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (DriverComboBox.SelectedIndex)
            {
                case 0: // No Driver
                    foreach(Control control in Controls)
                    {
                        if (!control.Equals(DriverComboBox))
                            control.Enabled = false;
                    }
                    break;
                case 1: //Motor
                    foreach (Control control in Controls)
                    {
                        if (!control.Equals(DriverComboBox))
                            control.Enabled = true;
                    }
                    MetaTabControl.Visible = false;
                    PortTwoLabel.Enabled = false;
                    PortTwoUpDown.Enabled = false;
                    UpperLimitUpDown.Maximum = LowerLimitUpDown.Maximum = 360;
                    UpperLimitUpDown.Minimum = LowerLimitUpDown.Minimum = 0;
                    unit = "°";
                    break;
                case 2: //Servo
                    foreach (Control control in Controls)
                    {
                        if (!control.Equals(DriverComboBox))
                            control.Enabled = true;
                    }
                    MetaTabControl.Visible = false;
                    PortTwoLabel.Enabled = false;
                    PortTwoUpDown.Enabled = false;
                    unit = "cm";
                    break;
                case 3: //Bumper Pneumatics
                    foreach (Control control in Controls)
                    {
                        if (!control.Equals(DriverComboBox))
                            control.Enabled = true;
                    }
                    MetaTabControl.Visible = true;
                    if(!MetaTabControl.TabPages.Contains(PneumaticTab)) MetaTabControl.TabPages.Add(PneumaticTab);
                    PortTwoLabel.Enabled = true;
                    PortTwoUpDown.Enabled = true;
                    unit = "cm";
                    break;
                case 4: //Relay Pneumatics
                    foreach (Control control in Controls)
                    {
                        if (!control.Equals(DriverComboBox))
                            control.Enabled = true;
                    }
                    MetaTabControl.Visible = true;
                    if(!MetaTabControl.TabPages.Contains(PneumaticTab)) MetaTabControl.TabPages.Add(PneumaticTab);
                    PortTwoUpDown.Enabled = false;
                    PortTwoLabel.Enabled = false;
                    unit = "cm";
                    break;
                case 5: //Dual Motor
                    foreach (Control control in Controls)
                    {
                        if (!control.Equals(DriverComboBox))
                            control.Enabled = true;
                    }
                    MetaTabControl.Visible = false;
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

        private void InventorHighlightButton_Click(object sender, EventArgs e)
        {
            StandardAddInServer.Instance.WizardSelect(node);
        }

        public JointDriver GetJointDriver()
        {
            if (Merged)
                return null;
            switch(DriverComboBox.SelectedIndex)
            {
                case 1: //Motor
                    JointDriver driver = new JointDriver(JointDriverType.MOTOR);
                    driver.SetLimits((float)(LowerLimitUpDown.Value * (decimal)(Math.PI / 180d)), (float)(UpperLimitUpDown.Value * (decimal)(Math.PI / 180d)));
                    driver.SetPort((AutoAssignCheckBox.Checked) ? WizardData.Instance.NextFreePort : (int)PortOneUpDown.Value);
                    if (AutoAssignCheckBox.Checked) WizardData.Instance.NextFreePort++;
                    return driver;
                case 2: //Servo
                    driver = new JointDriver(JointDriverType.SERVO);
                    driver.SetLimits((float)(LowerLimitUpDown.Value / 100), (float)(UpperLimitUpDown.Value / 100));
                    driver.SetPort((AutoAssignCheckBox.Checked) ? WizardData.Instance.NextFreePort : (int)PortOneUpDown.Value);
                    if (AutoAssignCheckBox.Checked) WizardData.Instance.NextFreePort++;
                    return driver;
                case 3: //Bumper Pneumatic
                    driver = new JointDriver(JointDriverType.BUMPER_PNEUMATIC);
                    driver.SetLimits((float)(LowerLimitUpDown.Value / 100), (float)(UpperLimitUpDown.Value / 100));
                    driver.SetPort((AutoAssignCheckBox.Checked) ? WizardData.Instance.NextFreePort : (int)PortOneUpDown.Value);
                    if (AutoAssignCheckBox.Checked) WizardData.Instance.NextFreePort += 2;
                    return driver;
                case 4: //Relay Pneumatic
                    driver = new JointDriver(JointDriverType.BUMPER_PNEUMATIC);
                    driver.SetLimits((float)(LowerLimitUpDown.Value / 100), (float)(UpperLimitUpDown.Value / 100));
                    driver.SetPort((AutoAssignCheckBox.Checked) ? WizardData.Instance.NextFreePort : (int)PortOneUpDown.Value);
                    if (AutoAssignCheckBox.Checked) WizardData.Instance.NextFreePort++;
                    return driver;
                case 5: //Dual Motor
                    driver = new JointDriver(JointDriverType.DUAL_MOTOR);
                    driver.SetLimits((float)(LowerLimitUpDown.Value * (decimal)(Math.PI / 180d)), (float)(UpperLimitUpDown.Value * (decimal)(Math.PI / 180d)));
                    if (AutoAssignCheckBox.Checked) WizardData.Instance.NextFreePort += 2;
                    return driver;
            }
            return null;
        }

    }
}