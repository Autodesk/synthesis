using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Input;
using System.Runtime.InteropServices;

namespace BxDRobotExporter.Wizard
{
    /// <summary>
    /// <see cref="UserControl"/> added to a <see cref="FlowLayoutPanel"/> in the <see cref="DefineWheelsPage"/>
    /// </summary>
    public partial class WheelSlotPanel : UserControl
    {
        /// <summary>
        /// Field containing the <see cref="WheelSetupPanel"/> in this slot.
        /// </summary>
        public WheelSetupPanel SetupPanel;
        public String name;

        public WheelSlotPanel()
        {
            InitializeComponent();
            MinimumSize = new Size(0, 0); // Minimum size only used in editor
            BackColor = Color.White;
            IsFilled = false;
        }

        public void FillSlot(RigidNode_Base node, String name, WheelSide side, WizardData.WizardWheelType wheelType = WizardData.WizardWheelType.NORMAL)
        {
            this.name = name;
            SetupPanel = new WheelSetupPanel(node, name, wheelType);
            
            SetupPanel.Dock = DockStyle.Top;
            Dock = DockStyle.Top;
            SetupPanel.Side = side;
            SuspendLayout();

            while (Controls.Count > 0)
            {
                Controls[0].Dispose();
            }

            Controls.Add(SetupPanel);
            SetupPanel.Visible = true;
            ResumeLayout();
            
            SetupPanel._WheelTypeChangedInternal += delegate () { OnWheelTypeChanged(); };

            IsFilled = true;
        }

        /// <summary>
        /// Fills the slot with a prexisting <see cref="WheelSetupPanel"/>. TODO: Use this for soft value saving when someone unchecks something.
        /// </summary>
        /// <param name="setupPanel"></param>
        public void FillSlot(WheelSetupPanel setupPanel)
        {
            SetupPanel = setupPanel;
            SetupPanel.Dock = DockStyle.Fill;
            this.Controls.Add(SetupPanel);

            IsFilled = true;
        }

        /// <summary>
        /// Frees up the slot and sets <see cref="IsFilled"/> to false
        /// </summary>
        public void FreeSlot()
        {
            SetupPanel.Dispose();
            InitializeComponent();

            IsFilled = false;
        }

        /// <summary>
        /// If the slot is filled, gets the <see cref="WizardData.WheelSetupData"/> via <see cref="WheelSetupPanel.GetWheelData()"/>
        /// </summary>
        public WizardData.WheelSetupData WheelData { get => IsFilled ? SetupPanel.GetWheelData() : new WizardData.WheelSetupData(); }

        /// <summary>
        /// If the slot is filled, gets the <see cref="WizardData.WizardWheelType"/> from the panel via the <see cref="WheelSetupPanel.WheelType"/> property.
        /// </summary>
        public WizardData.WizardWheelType WheelType { get => IsFilled ? SetupPanel.WheelType : 0; }

        /// <summary>
        /// Gets the <see cref="RigidNode_Base"/> from the <see cref="WheelSetupPanel"/> if it isn't null.
        /// </summary>
        public RigidNode_Base Node { get => SetupPanel?.Node; }

        /// <summary>
        /// Property used for filling the correct slots on the <see cref="DefineWheelsPage"/>
        /// </summary>
        public bool IsFilled { get; private set; }

        /// <summary>
        /// Event used for drive train verification in the parent <see cref="DefineWheelsPage"/>
        /// </summary>
        public event WheelTypeChangedEventHandler WheelTypeChanged;
        private void OnWheelTypeChanged()
        {
            WheelTypeChanged?.Invoke(this, new WheelTypeChangedEventArgs { NewWheelType = WheelType });
        }
    }
}
