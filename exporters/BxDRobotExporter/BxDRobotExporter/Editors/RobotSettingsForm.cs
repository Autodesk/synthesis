using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BxDRobotExporter.ControlGUI;

namespace BxDRobotExporter.Editors
{
    public partial class RobotSettingsForm : Form
    {
        static Dictionary<string, string> fields = new Dictionary<string, string>();

        public RobotSettingsForm(string initialRobotName)
        {
            InitializeComponent();
            InitializeFields();

            RobotNameTextBox.Text = initialRobotName;
            ColorBox.Checked = SynthesisGUI.PluginSettings.GeneralUseFancyColors;
            
        }

        /// <summary>
        /// Gets the paths for all the synthesis fields.
        /// </summary>
        public void InitializeFields()
        {
            this.OpenSynthesisBox.Checked = SynthesisGUI.PluginSettings.openSynthesis;
        }

        public static DialogResult Prompt(string initialRobotName, out string robotName, out bool colors, out bool openSynthesis, out string field)
        {
            try
            {
                RobotSettingsForm settingsForm = new RobotSettingsForm(initialRobotName);
                settingsForm.ShowDialog();
                robotName = settingsForm.RobotNameTextBox.Text;
                colors = settingsForm.ColorBox.Checked;
                openSynthesis = settingsForm.OpenSynthesisBox.Checked;
                SynthesisGUI.PluginSettings.openSynthesis = settingsForm.OpenSynthesisBox.Checked;

                field = null;

                return settingsForm.DialogResult;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                throw;
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (CheckFormIsValid())
            {
                var InvalidChars = (new string(Path.GetInvalidPathChars()) + new string(Path.GetInvalidFileNameChars())).Distinct();

                foreach (char c in InvalidChars)
                {
                    RobotNameTextBox.Text = RobotNameTextBox.Text.Replace(c.ToString(), "");
                }

                if(File.Exists(SynthesisGUI.PluginSettings.GeneralSaveLocation + "\\" + RobotNameTextBox.Text + @"\skeleton.bxdj") && MessageBox.Show("Overwrite Existing Robot?", "Save Robot", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Please enter a name for your robot.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenSynthesisBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckFormIsValid();
        }

        private bool CheckFormIsValid()
        {
            ButtonOk.Enabled = RobotNameTextBox.Text.Length > 0;
            return ButtonOk.Enabled;
        }

        private void RobotNameTextBox_TextChanged(object sender, EventArgs e)
        {
            CheckFormIsValid();
        }
    }
}
