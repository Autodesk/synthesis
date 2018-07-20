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
            MessageBox.Show("" + node.GetSkeletalJoint().GetJointType().ToAssemblyJointType().ToString());
            InitializeComponent();
            BackColor = Color.White;
            Dock = DockStyle.Top;
            MinimumSize = new Size(0, 0); // Minimum size only needed in editor

            this.node = node;

            string readableName = node.ModelFileName.Replace('_', ' ').Replace(".bxda", "");
            readableName = readableName.Substring(0, 1).ToUpperInvariant() + readableName.Substring(1); // Capitalize first character
            NodeGroupBox.Text = readableName;

            DriverComboBox.SelectedIndex = 0;
            DriverComboBox_SelectedIndexChanged(null, null);
            
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
                    this.PortsGroupBox.Visible = false;
                    this.MetaTabControl.Visible = false;
                    break;
                case 1: //Motor
                    this.PortsGroupBox.Visible = true;
                    MetaTabControl.Visible = false;
                    PortsGroupBox.Text = "PWM Port";
                    PortOneLabel.Text = "PWM Port:";
                    PortOneLabel.Visible = false;
                    PortTwoLabel.Visible = false;
                    PortTwoUpDown.Visible = false;
                    unit = "°";
                    break;
                case 2: //Servo
                    this.PortsGroupBox.Visible = true;
                    this.MetaTabControl.Visible = false;
                    PortsGroupBox.Text = "PWM Port";
                    PortOneLabel.Text = "PWM Port:";
                    PortOneLabel.Visible = false;
                    PortTwoLabel.Visible = false;
                    PortTwoUpDown.Visible = false;
                    unit = "cm";
                    break;
                case 3: //Bumper Pneumatics
                    this.PortsGroupBox.Visible = true;
                    MetaTabControl.Visible = true;
                    PortOneLabel.Text = "Solenoid Port 1:";
                    PortTwoLabel.Text = "Solenoid Port 2:";
                    if (!MetaTabControl.TabPages.Contains(PneumaticTab)) MetaTabControl.TabPages.Add(PneumaticTab);
                    PortsGroupBox.Text = "Solenoid Ports";
                    PortOneLabel.Visible = true;
                    PortTwoLabel.Visible = true;
                    PortTwoUpDown.Visible = true;
                    unit = "cm";
                    break;
                case 4: //Relay Pneumatics
                    this.PortsGroupBox.Visible = true;
                    MetaTabControl.Visible = true;
                    PortOneLabel.Text = "Relay Port:";
                    if (!MetaTabControl.TabPages.Contains(PneumaticTab)) MetaTabControl.TabPages.Add(PneumaticTab);
                    PortsGroupBox.Text = "Relay Port";
                    PortOneLabel.Visible = false;
                    PortTwoLabel.Visible = false;
                    PortTwoUpDown.Visible = false;
                    unit = "cm";
                    break;
                case 5: //Dual Motor
                    this.PortsGroupBox.Visible = true;
                    this.MetaTabControl.Visible = false;
                    PortsGroupBox.Text = "PWM Ports";
                    PortOneLabel.Text = "PWM Port 1:";
                    PortTwoLabel.Text = "PWM Port 2:";
                    PortOneLabel.Visible = true;
                    PortTwoLabel.Visible = true;
                    PortTwoUpDown.Visible = true;
                    unit = "°";
                    break;
            }

        }
        

        /// <summary>
        /// Highlights the node in inventor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HighlightNode(object sender, EventArgs e)
        {
            StandardAddInServer.Instance.SelectNode(node);
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
                    driver.SetPort((int)PortOneUpDown.Value, 1);
                    return driver;
                case 2: //Servo
                    driver = new JointDriver(JointDriverType.SERVO);
                    driver.SetPort((int)PortOneUpDown.Value, 1);
                    return driver;
                case 3: //Bumper Pneumatic
                    driver = new JointDriver(JointDriverType.BUMPER_PNEUMATIC);
                    driver.SetPort((int)PortOneUpDown.Value, (int)PortTwoUpDown.Value);
                    return driver;
                case 4: //Relay Pneumatic
                    driver = new JointDriver(JointDriverType.RELAY_PNEUMATIC);
                    driver.SetPort((int)PortOneUpDown.Value, 1);
                    return driver;
                case 5: //Dual Motor
                    driver = new JointDriver(JointDriverType.DUAL_MOTOR);
                    ((RotationalJoint_Base)node.GetSkeletalJoint()).hasAngularLimit = true;
                    driver.SetPort((int)PortOneUpDown.Value, (int)PortTwoUpDown.Value);
                    return driver;
            }
            return null;
        }
    }
}