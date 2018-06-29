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
        static bool IsMetric = false;

        public float TotalMass = 0;

        public SetMassForm()
        {
            InitializeComponent();

            TotalMass = SynthesisGUI.Instance.TotalMass;
            
            if (IsMetric)
                MassBox.Value = (decimal)(TotalMass / 2.20462);
            else
                MassBox.Value = (decimal)TotalMass;

            MetricBox.Checked = IsMetric;
        }

        private void MetricBox_CheckedChanged(object sender, EventArgs e)
        {
            if (MetricBox.Checked)
                UnitLabel.Text = "kg";
            else
                UnitLabel.Text = "lbs";
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            IsMetric = MetricBox.Checked;

            if (IsMetric)
                TotalMass = (float)MassBox.Value * 2.20462f;
            else
                TotalMass = (float)MassBox.Value;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
