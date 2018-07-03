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

        public float TotalMassKg = 0;

        public SetMassForm()
        {
            InitializeComponent();

            TotalMassKg = SynthesisGUI.Instance.TotalMassKg;
            
            if (!IsMetric)
                MassBox.Value = (decimal)(TotalMassKg * 2.20462);
            else
                MassBox.Value = (decimal)TotalMassKg;

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
                TotalMassKg = (float)MassBox.Value * 2.20462f;
            else
                TotalMassKg = (float)MassBox.Value;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
