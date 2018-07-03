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
        static bool IsMetric = false;

        public float TotalWeightKg = 0;

        public SetWeightForm()
        {
            InitializeComponent();

            TotalWeightKg = SynthesisGUI.Instance.TotalWeightKg;
            
            if (!IsMetric)
                WeightBox.Value = (decimal)(TotalWeightKg * 2.20462);
            else
                WeightBox.Value = (decimal)TotalWeightKg;

            UnitBox.SelectedIndex = IsMetric ? 1 : 0;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            IsMetric = UnitBox.SelectedIndex == 1;

            if (IsMetric)
                TotalWeightKg = (float)WeightBox.Value * 2.20462f;
            else
                TotalWeightKg = (float)WeightBox.Value;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
