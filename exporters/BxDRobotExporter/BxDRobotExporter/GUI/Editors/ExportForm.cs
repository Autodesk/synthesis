using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BxDRobotExporter.GUI.Editors
{
    public partial class ExportForm : Form
    {
        private static Dictionary<string, string> fields = new Dictionary<string, string>();

        public ExportForm(string initialRobotName)
        {
            InitializeComponent();
            InitializeFields();

            RobotNameTextBox.Text = initialRobotName;
            ColorBox.Checked = RobotData.PluginSettings.GeneralUseFancyColors;
            
        }

        /// <summary>
        /// Gets the paths for all the synthesis fields.
        /// </summary>
        public void InitializeFields()
        {
            if (fields.Count == 0)
            {
                var dirs = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Synthesis\Fields");

                foreach (var dir in dirs)
                {
                    fields.Add(dir.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last(), dir);
                    FieldSelectComboBox.Items.Add(dir.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last());
                }
            }
            else
            {
                foreach(KeyValuePair<string, string> pair in fields)
                {
                    FieldSelectComboBox.Items.Add(pair.Key);
                }
            }
            this.FieldSelectComboBox.SelectedItem = RobotData.PluginSettings.FieldName;
            this.OpenSynthesisBox.Checked = RobotData.PluginSettings.OpenSynthesis;
        }

        public static DialogResult Prompt(string initialRobotName, out string robotName, out bool colors, out bool openSynthesis, out string field)
        {
            try
            {
                ExportForm settingsForm = new ExportForm(initialRobotName);
                settingsForm.ShowDialog();
                robotName = settingsForm.RobotNameTextBox.Text;
                colors = settingsForm.ColorBox.Checked;
                openSynthesis = settingsForm.OpenSynthesisBox.Checked;
                RobotData.PluginSettings.FieldName = (string)settingsForm.FieldSelectComboBox.SelectedItem;
                RobotData.PluginSettings.OpenSynthesis = settingsForm.OpenSynthesisBox.Checked;

                field = null;
                
                if (settingsForm.OpenSynthesisBox.Checked && settingsForm.FieldSelectComboBox.SelectedItem != null)
                    field = fields[(string)settingsForm.FieldSelectComboBox.SelectedItem];

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
                var invalidChars = (new string(Path.GetInvalidPathChars()) + new string(Path.GetInvalidFileNameChars())).Distinct();

                foreach (char c in invalidChars)
                {
                    RobotNameTextBox.Text = RobotNameTextBox.Text.Replace(c.ToString(), "");
                }

                if(File.Exists(RobotData.PluginSettings.GeneralSaveLocation + "\\" + RobotNameTextBox.Text + @"\skeleton.bxdj") && MessageBox.Show("Overwrite Existing Robot?", "Save Robot", MessageBoxButtons.YesNo) == DialogResult.No)
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
            if (OpenSynthesisBox.Checked)
            {
                FieldLabel.Enabled = true;
                FieldSelectComboBox.Enabled = true;
            }
            else
            {
                FieldLabel.Enabled = false;
                FieldSelectComboBox.Enabled = false;
            }

            CheckFormIsValid();
        }

        private bool CheckFormIsValid()
        {
            ButtonOk.Enabled = RobotNameTextBox.Text.Length > 0 && (!OpenSynthesisBox.Checked || FieldSelectComboBox.SelectedItem != null);
            return ButtonOk.Enabled;
        }

        private void RobotNameTextBox_TextChanged(object sender, EventArgs e)
        {
            CheckFormIsValid();
        }

        private void FieldSelectComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckFormIsValid();
        }
    }
}
