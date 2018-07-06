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
    public partial class SetWeightForm : Form
    {
        public float TotalWeightKg = 0;
        public bool PreferMetric = false;

        public SetWeightForm()
        {
            InitializeComponent();

            TotalWeightKg = SynthesisGUI.Instance.RMeta.TotalWeightKg;
            PreferMetric = SynthesisGUI.Instance.RMeta.PreferMetric;

            if (TotalWeightKg > 0)
            {
                if (!PreferMetric)
                    WeightBox.Value = (decimal)(TotalWeightKg * 2.20462);
                else
                    WeightBox.Value = (decimal)TotalWeightKg;

                CalculatedWeightCheck.Checked = false;
            }
            else
            {
                WeightBox.Value = 0;
                CalculatedWeightCheck.Checked = true;
            }

            UnitBox.SelectedIndex = PreferMetric ? 1 : 0;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            PreferMetric = UnitBox.SelectedIndex == 1;

            if (CalculatedWeightCheck.Checked)
                TotalWeightKg = -1;
            else
            {
                if (!PreferMetric)
                    TotalWeightKg = (float)WeightBox.Value / 2.20462f;
                else
                    TotalWeightKg = (float)WeightBox.Value;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void CalculatedWeightCheck_CheckedChanged(object sender, EventArgs e)
        {
            WeightBox.Enabled = !CalculatedWeightCheck.Checked;
            WeightBox.Minimum = CalculatedWeightCheck.Checked ? 0 : 1;
            WeightBox.Value = CalculatedWeightCheck.Checked ? 0 : 100;
            UnitBox.Enabled = !CalculatedWeightCheck.Checked;
        }

        private void UnitBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Value did not change
            if (PreferMetric == (UnitBox.SelectedIndex == 1))
                return;

            PreferMetric = UnitBox.SelectedIndex == 1;

            if (UnitBox.SelectedIndex == 0)
                WeightBox.Value = (decimal)((float)WeightBox.Value / 2.20462f);
            else
                WeightBox.Value = (decimal)((float)WeightBox.Value * 2.20462f);
        }
    }
}
