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
    public partial class NameRobotForm : Form
    {
        public enum NameMode { Initial, SaveAs }
        private NameMode nameMode;

        public NameRobotForm(NameMode mode)
        {
            InitializeComponent();

            nameMode = mode;

            PathTextBox.Text =SynthesisGUI.PluginSettings.GeneralSaveLocation;
        }

        public static DialogResult NameRobot(out string RobotName, NameMode mode = NameMode.SaveAs)
        {
            try
            {
                NameRobotForm form = new NameRobotForm(mode);
                form.ShowDialog();
                RobotName = form.RobotNameTextBox.Text;
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
            if(RobotNameTextBox.Text != null)
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

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            if((nameMode == NameMode.Initial && MessageBox.Show("Are you sure you want to cancel? (All export progress would be lost)", 
                "Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) || nameMode == NameMode.SaveAs)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }

        }

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            if(ExportLocationDialog.ShowDialog() == DialogResult.OK)
            {
                PathTextBox.Text = ExportLocationDialog.SelectedPath;
                SynthesisGUI.PluginSettings.GeneralSaveLocation = ExportLocationDialog.SelectedPath;
            }
        }
    }
}
