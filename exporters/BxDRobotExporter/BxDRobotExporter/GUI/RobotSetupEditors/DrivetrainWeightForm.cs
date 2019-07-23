using System;
using System.Windows.Forms;
using BxDRobotExporter.ControlGUI;

namespace BxDRobotExporter.Editors
{
    public partial class DrivetrainWeightForm : Form
    {
        public float TotalWeightKg = 0;
        public bool PreferMetric = false;

        public DrivetrainWeightForm()
        {
            InitializeComponent();
            AnalyticUtils.LogPage("SetWeightForm");
            TotalWeightKg = SynthesisGUI.Instance.RMeta.TotalWeightKg;
            PreferMetric = SynthesisGUI.Instance.RMeta.PreferMetric;

            SetWeightBoxValue(TotalWeightKg * (PreferMetric ? 1 : 2.20462f));
            CalculatedWeightCheck.Checked = TotalWeightKg <= 0;
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

        private void SetWeightBoxValue(float value)
        {
            if ((decimal)value > WeightBox.Maximum)
                WeightBox.Value = WeightBox.Maximum;
            else if ((decimal)value >= WeightBox.Minimum)
                WeightBox.Value = (decimal)value;
            else
                WeightBox.Value = 0;
        }

        private void UnitBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
