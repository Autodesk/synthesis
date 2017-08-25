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
    public partial class WheelSetupPanel : UserControl
    {
        public WheelSetupPanel()
        {
            InitializeComponent();
        }

        public WheelSetupPanel(RigidNode_Base node, WizardData.WizardWheelType WheelType = WizardData.WizardWheelType.NORMAL)
        {
            InitializeComponent();

            WheelTypeComboBox.SelectedIndex = ((int)WheelType) - 1;
            FrictionComboBox.SelectedIndex = 1;

            Node = node;
            MainGroupBox.Text = node.ModelFileName;

            this.MouseClick += delegate (object sender, MouseEventArgs e)
            {
                if (Node != null)
                    StandardAddInServer.Instance.WizardSelect(Node);
            };

            this.BackColor = Control.DefaultBackColor;

        }

        public RigidNode_Base Node;

        private void LeftRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RightRadioButton.Checked = false;
        }

        private void RightRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            LeftRadioButton.Checked = false;
        }

        public WizardData.WheelSetupData GetWheelData()
        {
            return new WizardData.WheelSetupData
            {
                FrictionLevel = (WizardData.WizardFrictionLevel)this.FrictionComboBox.SelectedIndex,
                WheelType = (WizardData.WizardWheelType)(this.WheelTypeComboBox.SelectedIndex + 1),
                PWMPort = (RightRadioButton.Checked) ? (byte)0x02 : (byte)0x01,
                Node = this.Node
            };
        }

        public WizardData.WizardFrictionLevel FrictionLevel { get => (WizardData.WizardFrictionLevel)this.FrictionComboBox.SelectedIndex; }
        public WizardData.WizardWheelType WheelType { get => (WizardData.WizardWheelType)(this.WheelTypeComboBox.SelectedIndex + 1); }

        public event Action _WheelTypeChangedInternal;

        private void WheelTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _WheelTypeChangedInternal?.Invoke();
        }
    }
}
