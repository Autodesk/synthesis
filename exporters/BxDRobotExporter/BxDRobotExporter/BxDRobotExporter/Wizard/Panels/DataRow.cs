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
    public partial class MassDataRow : UserControl
    {
        public delegate void ValueUpdate(decimal change);
        private decimal prevValue;
        public event ValueUpdate MassChanged;

        private void OnMassChanged(decimal amount)
        {
            MassChanged?.Invoke(amount);
        }

        public MassDataRow()
        {

        }

        public RigidNode_Base Node;

        public MassDataRow(string nodeName, RigidNode_Base node = null)
        {
            InitializeComponent();

            this.Node = node;

            NodeLabel.Text = nodeName;

            NodeLabel.Click += delegate (object sender, EventArgs e)
            {
                if (Node != null)
                    StandardAddInServer.Instance.WizardSelect(Node);
            };

            prevValue = MassControl.Value;

            MassControl.ValueChanged += delegate (object sender, EventArgs e)
            {
                OnMassChanged(MassControl.Value - prevValue);
                prevValue = MassControl.Value;
            };
        }

        public float Value { get => (float)MassControl.Value; }
    }
}
