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
        public static event OnWheelSetupPanelRemove remove;
        public static event OnWheelSetupPanelHover hover;
        public String name;
        public bool isRightWheel;

        public WheelSetupPanel(RigidNode_Base node, String name, WizardData.WizardWheelType WheelType = WizardData.WizardWheelType.NORMAL)
        {
            this.name = name;
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

            BackColor = Color.White;
        }

        public RigidNode_Base node;

        

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
                PWMPort = isRightWheel ? (byte)0x02 : (byte)0x01,
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
        private void button1_Click(object sender, EventArgs e)
        {
            remove(name);
        }

        private void WheelSetupPanel_MouseHover(object sender, EventArgs e)
        {
            hover(name);
        }
    }

    public delegate string OnWheelSetupPanelRemove(string str);
    public delegate string OnWheelSetupPanelHover(string str);
}
