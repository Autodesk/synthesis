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
        public RigidNode_Base Node;
        
        public DefinePartPanel()
        {
            InitializeComponent();

        }
        
        public DefinePartPanel(RigidNode_Base node)
        {
            Node = node;
            this.NodeGroupBox.Text = node.ModelFileName;

            UpperLimitUpDown = new WizardUpDown();
            LowerLimitUpDown = new WizardUpDown();
            // 
            // UpperLimitUpDown
            // 
            this.UpperLimitUpDown.Location = new System.Drawing.Point(72, 26);
            this.UpperLimitUpDown.Name = "UpperLimitUpDown";
            this.UpperLimitUpDown.Size = new System.Drawing.Size(46, 20);
            this.UpperLimitUpDown.TabIndex = 3;
            this.UpperLimitUpDown.ValueChanged += new System.EventHandler(this.UpperLimitUpDown_ValueChanged);
            ((WizardUpDown)this.UpperLimitUpDown).Unit = "°";
            // 
            // LowerLimitUpDown
            // 
            this.LowerLimitUpDown.Location = new System.Drawing.Point(72, 53);
            this.LowerLimitUpDown.Name = "LowerLimitUpDown";
            this.LowerLimitUpDown.Size = new System.Drawing.Size(45, 20);
            this.LowerLimitUpDown.TabIndex = 0;
            this.LowerLimitUpDown.ValueChanged += new System.EventHandler(this.LowerLimitUpDown_ValueChanged);
            ((WizardUpDown)this.LowerLimitUpDown).Unit = "°";
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
                case 0: //Motor
                    MetaTabControl.Enabled = false;
                    PortTwoLabel.Enabled = false;
                    PortTwoUpDown.Enabled = false;
                    ((WizardUpDown)this.LowerLimitUpDown).Unit = "°";
                    ((WizardUpDown)this.UpperLimitUpDown).Unit = "°";
                    break;
                case 1: //Worm Screw
                    MetaTabControl.Enabled = false;
                    PortTwoLabel.Enabled = false;
                    PortTwoUpDown.Enabled = false;
                    ((WizardUpDown)this.LowerLimitUpDown).Unit = "in";
                    ((WizardUpDown)this.UpperLimitUpDown).Unit = "in";
                    break;
                case 2: 
                    MetaTabControl.Enabled = true;
                    PneumaticTab.Show();
                    ElevatorStagesTab.Hide();
                    ElevatorBrakeTab.Hide();
                    PortTwoLabel.Enabled = true;
                    PortTwoUpDown.Enabled = true;
                    ((WizardUpDown)this.LowerLimitUpDown).Unit = "in";
                    ((WizardUpDown)this.UpperLimitUpDown).Unit = "in";
                    break;
                case 3:
                    MetaTabControl.Enabled = true;
                    PneumaticTab.Show();
                    ElevatorBrakeTab.Hide();
                    ElevatorStagesTab.Hide();
                    PortTwoUpDown.Enabled = false;
                    PortTwoLabel.Enabled = false;
                    ((WizardUpDown)this.LowerLimitUpDown).Unit = "in";
                    ((WizardUpDown)this.UpperLimitUpDown).Unit = "in";
                    break;
                case 4:
                    MetaTabControl.Enabled = false;
                    PortTwoLabel.Enabled = false;
                    PortTwoUpDown.Enabled = false;
                    ((WizardUpDown)this.LowerLimitUpDown).Unit = "°";
                    ((WizardUpDown)this.UpperLimitUpDown).Unit = "°";
                    break;
                case 5:
                    MetaTabControl.Enabled = false;
                    break;
                case 6:
                    break;
            }

        }

        private void chkBoxHasBrake_CheckedChanged(object sender, EventArgs e)
        {
            BrakePortOneUpDown.Enabled = BrakePortTwoUpDown.Enabled = HasBrakeCheckBox.Checked;
        }

        private void UpperLimitUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void LowerLimitUpDown_ValueChanged(object sender, EventArgs e)
        {

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
                    Text = "Merged" + Node.ModelFileName + " into " + Node.GetParent().ModelFileName,
                    Font = new Font(DefaultFont.FontFamily, 12.0f),
                    BackColor = System.Windows.Forms.Control.DefaultBackColor,
                    ForeColor = Color.DarkGray,
                    AutoSize = false,
                    Dock = DockStyle.Fill
                };
                this.Controls.Add(mergedLabel);
                mergedLabel.BringToFront();
            }
        }

        private void InventorHighlightButton_Click(object sender, EventArgs e)
        {
            StandardAddInServer.Instance.WizardSelect(Node);
        }
    }
}
