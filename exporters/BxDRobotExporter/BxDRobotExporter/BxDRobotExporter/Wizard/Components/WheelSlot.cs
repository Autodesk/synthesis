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
    public partial class WheelSlotPanel : UserControl
    {
        private WheelSetupPanel wheelSetupPanel;
        public WheelSlotPanel()
        {
            InitializeComponent();
        }
        public void FillSlot(RigidNode_Base node, WizardData.WizardWheelType wheelType = WizardData.WizardWheelType.NORMAL)
        {
            wheelSetupPanel = new WheelSetupPanel(node, wheelType);
            wheelSetupPanel.Dock = DockStyle.Fill;

            this.SuspendLayout();
            while (Controls.Count > 0)
            {
                Controls[0].Dispose();
            }
            this.Controls.Add(wheelSetupPanel);
            wheelSetupPanel.Visible = true;
            this.ResumeLayout();

            wheelSetupPanel._WheelTypeChangedInternal += delegate () { OnWheelTypeChanged(); };
        }
        public void FillSlot(WheelSetupPanel setupPanel)
        {
            wheelSetupPanel = setupPanel;
            wheelSetupPanel.Dock = DockStyle.Fill;
            this.Controls.Add(wheelSetupPanel);
        }
        public void FreeSlot()
        {
            wheelSetupPanel.Dispose();
            InitializeComponent();
        }

        public WizardData.WheelSetupData WheelData
        {
            get => IsFilled ? wheelSetupPanel.GetWheelData() : new WizardData.WheelSetupData();
        }
        public WizardData.WizardWheelType WheelType
        {
            get => IsFilled ? wheelSetupPanel.WheelType : 0;
        }
        public RigidNode_Base Node { get => wheelSetupPanel?.Node; }
        public bool IsFilled { get => wheelSetupPanel != null; }

        public event WheelTypeChangedEventHandler WheelTypeChanged;
        
        private void OnWheelTypeChanged()
        {
            WheelTypeChanged?.Invoke(this, new WheelTypeChangedEventArgs { NewWheelType = WheelType });
        }
    }
}
