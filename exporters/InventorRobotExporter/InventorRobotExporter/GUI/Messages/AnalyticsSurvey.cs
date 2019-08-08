using System.Windows.Forms;

namespace InventorRobotExporter.Messages
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
            Properties.Settings.Default.Save();

            if (Properties.Settings.Default.UseAnalytics)
            {
                if (teamTextBox.TextLength > 0)
                {
                    string team = teamTextBox.Text;
                    // send analytics for team data, to be implemented
                }

                if (reasonTextList.CheckedItems.Count > 0)
                {
                    // Only one item can be checked, so the reason has to be the first one
                    string reason = reasonTextList.CheckedItems[0].ToString();

                    if (reason.Equals("Other") && otherTextBox.Text.Length > 0) reason = otherTextBox.Text;
                    // send analytics for reason data, to be implemented
                }
            }
        }

        private void ReasonTextList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // These are checkboxes, not radio buttons
            // Prevents from multiple options being checked at once. The notification asks for the /main/ reason for using Synthesis
//            if (e.NewValue == CheckState.Checked)
//            {
//                for (int i = 0; i < reasonTextList.Items.Count; ++i)
//                {
//                    if (e.Index != i)
//                    {
//                        reasonTextList.SetItemChecked(i, false);
//                    }
//                }
//            }
            // If selecting the "Other" (should be last item in list) make a text box visible to create your own reason
            otherTextBox.Visible = (e.Index == (reasonTextList.Items.Count - 1));
        }

        private void SkipButton_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void TeamTextBox_TextChanged(object sender, System.EventArgs e)
        {

        }

        private void TeamLabel_Click(object sender, System.EventArgs e)
        {

        }
    }
}