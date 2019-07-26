using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using InventorRobotExporter.Managers;

namespace InventorRobotExporter.GUI.Editors
{
    public partial class ExportForm : Form
    {
        private static Dictionary<string, string> fields = new Dictionary<string, string>();

        public ExportForm(string initialRobotName)
        {
            InitializeComponent();
            InitializeFields();

            RobotNameTextBox.Text = initialRobotName;
            ColorBox.Checked = RobotExporterAddInServer.Instance.AddInSettingsManager.DefaultExportWithColors;
            
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
            this.FieldSelectComboBox.SelectedItem = RobotExporterAddInServer.Instance.AddInSettingsManager.DefaultField;
            this.OpenSynthesisBox.Checked = RobotExporterAddInServer.Instance.AddInSettingsManager.OpenSynthesis;
        }
        
        /// <summary>
        /// Prompts the user for the name of the robot, as well as other information.
        /// </summary>
        /// <returns>True if user pressed okay, false if they pressed cancel</returns>
        public static bool PromptExportSettings(RobotDataManager robotDataManager)
        { // TODO: Compact this down
            if (Prompt(robotDataManager.RobotName, out var robotName, out var colors, out var openSynthesis, out var field) == DialogResult.OK)
            {
                robotDataManager.RobotName = robotName;
                robotDataManager.RobotField = field;

                RobotExporterAddInServer.Instance.AddInSettingsManager.DefaultExportWithColors = colors;
                RobotExporterAddInServer.Instance.AddInSettingsManager.SaveSettings();

                return true;
            }

            return false;
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
                RobotExporterAddInServer.Instance.AddInSettingsManager.DefaultField = (string)settingsForm.FieldSelectComboBox.SelectedItem;
                RobotExporterAddInServer.Instance.AddInSettingsManager.OpenSynthesis = settingsForm.OpenSynthesisBox.Checked;

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

                if(File.Exists(RobotExporterAddInServer.Instance.AddInSettingsManager.ExportPath + "\\" + RobotNameTextBox.Text + @"\skeleton.bxdj") && MessageBox.Show("Overwrite Existing Robot?", "Save Robot", MessageBoxButtons.YesNo) == DialogResult.No)
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
