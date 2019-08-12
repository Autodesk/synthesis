using System;
using System.Linq;
using System.Windows.Forms;
using InventorRobotExporter.Properties;

namespace InventorRobotExporter.GUI.Messages
{
    public partial class AnalyticsSurvey : Form
    {
        public AnalyticsSurvey()
        {
            InitializeComponent();
        }
        private void SubmitButton_Click(object sender, System.EventArgs e)
        {
            Close();

            if (Settings.Default.UseAnalytics)
            {
                if (teamInput.TextLength > 0)
                {
                    string team = teamInput.Text;
                    // send analytics for team data, to be implemented
                }

                var checkedOption = this.Controls.OfType<RadioButton>().FirstOrDefault(n => n.Checked);
                if (checkedOption != null)
                {
                    string reason = checkedOption.Text;
                    // send analytics for reason data, to be implemented
                }
            }
        }

        private void TeamInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !Char.IsDigit(e.KeyChar) && e.KeyChar != 8; // only allows entering of up to 4 digits (only numbers allowed)
        }

        private void ChoiceOtherButton_CheckedChanged(object sender, System.EventArgs e)
        {
            otherInput.Visible = choiceOtherButton.Checked;
        }

        private void SkipButton_Click(object sender, System.EventArgs e)
        {
            Close();
        }

    }
}