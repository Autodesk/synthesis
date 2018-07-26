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
        public void FillSlot(RigidNode_Base node)
        {

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
        }
    }
}
