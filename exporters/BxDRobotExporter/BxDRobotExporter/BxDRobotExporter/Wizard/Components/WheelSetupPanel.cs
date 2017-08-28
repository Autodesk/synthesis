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
    /// Used in the <see cref="DefineWheelsPage"/> to set wheel properties. At some point, we probably could detect whether the wheel is left or right, but, gotta crank out that release on time.
    /// </summary>
    public partial class WheelSetupPanel : UserControl
    {
        public bool Valid { get => LeftRadioButton.Checked || RightRadioButton.Checked; }

        public WheelSetupPanel()
        {
            InitializeComponent();
        }

        public WheelSetupPanel(RigidNode_Base node, WizardData.WizardWheelType WheelType = WizardData.WizardWheelType.NORMAL)
        {
            InitializeComponent();

            WheelTypeComboBox.SelectedIndex = ((int)WheelType) - 1;
            FrictionComboBox.SelectedIndex = 1;

            this.node = node;
            MainGroupBox.Text = node.ModelFileName;

            this.MouseClick += delegate (object sender, MouseEventArgs e)
            {
                if (this.node != null)
                    StandardAddInServer.Instance.WizardSelect(this.node);
            };

            this.BackColor = Control.DefaultBackColor;
        }

        public RigidNode_Base node;

        private void LeftRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RightRadioButton.Checked = false;
        }

        private void RightRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            LeftRadioButton.Checked = false;
        }

        /// <summary>
        /// Gets the <see cref="WizardData.WheelSetupData"/> for this panel. The parent page then adds it to <see cref="WizardData.wheels"/>
        /// </summary>
        /// <returns></returns>
        public WizardData.WheelSetupData GetWheelData()
        {
            return new WizardData.WheelSetupData
            {
                FrictionLevel = (WizardData.WizardFrictionLevel)this.FrictionComboBox.SelectedIndex,
                WheelType = (WizardData.WizardWheelType)(this.WheelTypeComboBox.SelectedIndex + 1),
                PWMPort = (RightRadioButton.Checked) ? (byte)0x02 : (byte)0x01,
                Node = this.node
            };
        }

        /// <summary>
        /// Gets the <see cref="WizardData.WizardFrictionLevel"/> from the FrictionComboBox
        /// </summary>
        public WizardData.WizardFrictionLevel FrictionLevel { get => (WizardData.WizardFrictionLevel)this.FrictionComboBox.SelectedIndex; }

        /// <summary>
        /// Gets the <see cref="WizardData.WizardWheelType"/> from the WheelTypeComboBox.
        /// </summary>
        public WizardData.WizardWheelType WheelType { get => (WizardData.WizardWheelType)(this.WheelTypeComboBox.SelectedIndex + 1); }

        /// <summary>
        /// Used to invoke the <see cref="WheelSlotPanel.WheelTypeChanged"/> event in the parent <see cref="WheelSlotPanel"/>
        /// </summary>
        public event Action _WheelTypeChangedInternal;

        private void WheelTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _WheelTypeChangedInternal?.Invoke();
        }

        private void ViewInventorButton_Click(object sender, EventArgs e)
        {
            StandardAddInServer.Instance.WizardSelect(node);
        }
    }
}
