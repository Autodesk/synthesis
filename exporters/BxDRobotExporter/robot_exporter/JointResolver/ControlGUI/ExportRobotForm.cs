using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace JointResolver.ControlGUI
{
    public partial class ExportRobotForm : Form
    {
        static Dictionary<string, string> fields = new Dictionary<string, string>();

        public ExportRobotForm(string initialRobotName)
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
            this.FieldSelectComboBox.SelectedItem = SynthesisGUI.PluginSettings.fieldName;
            this.OpenSynthesisBox.Checked = SynthesisGUI.PluginSettings.openSynthesis;
        }

        public static DialogResult Prompt(string initialRobotName, out string robotName, out bool colors, out bool openSynthesis, out string field)
        {
            try
            {
                ExportRobotForm form = new ExportRobotForm(initialRobotName);
                form.ShowDialog();
                robotName = form.RobotNameTextBox.Text;
                colors = form.ColorBox.Checked;
                openSynthesis = form.OpenSynthesisBox.Checked;
                SynthesisGUI.PluginSettings.fieldName = (string)form.FieldSelectComboBox.SelectedItem;
                SynthesisGUI.PluginSettings.openSynthesis = form.OpenSynthesisBox.Checked;

                field = null;
                
                if (form.OpenSynthesisBox.Checked && form.FieldSelectComboBox.SelectedItem != null)
                    field = fields[(string)form.FieldSelectComboBox.SelectedItem];

                return form.DialogResult;
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
