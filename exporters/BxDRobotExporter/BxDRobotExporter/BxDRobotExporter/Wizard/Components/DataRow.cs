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
    /// Used in <see cref="BasicRobotInfoPage"/> for setting the mass of the robot.
    /// </summary>
    public partial class MassDataRow : UserControl
    {
        public delegate void ValueUpdate(decimal change);
        private decimal prevValue;

        /// <summary>
        /// Invoked when <see cref="NumericUpDown.ValueChanged"/> is invoked.
        /// </summary>
        public event ValueUpdate MassChanged;

        private void OnMassChanged(decimal amount)
        {
            MassChanged?.Invoke(amount);
        }

        public MassDataRow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The node which the mass is applied to. If this is null, it sets the mass of the entire robot.
        /// </summary>
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

        /// <summary>
        /// Gets a float of the value in the <see cref="NumericUpDown"/>
        /// </summary>
        public float Value { get => (float)MassControl.Value; }
    }
}
