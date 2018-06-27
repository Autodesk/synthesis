using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditorsLibrary
{
    public partial class SetMassForm : Form
    {
        public float TotalMass = 0;

        public SetMassForm()
        {
            InitializeComponent();

            TotalMass = SynthesisGUI.Instance.TotalMass;
            MassBox.Value = (decimal) TotalMass;
        }

        private void MassBox_ValueChanged(object sender, EventArgs e)
        {
            TotalMass = (float) MassBox.Value;
        }
    }
}
