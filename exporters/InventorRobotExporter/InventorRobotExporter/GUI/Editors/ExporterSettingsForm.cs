using System;
using System.Drawing;
using System.Windows.Forms;
using InventorRobotExporter.Managers;

namespace InventorRobotExporter.GUI.Editors
{
    public delegate void SettingsEvent(AddInSettingsManager values);

    public partial class ExporterSettingsForm : Form
    {
        /// <summary>
        /// The local copy of the setting values
        /// </summary>
        private AddInSettingsManager values;

        public ExporterSettingsForm()
        {
            InitializeComponent();

            LoadValues();

            buttonOK.Click += (sender, args) =>
            {
                SaveValues();
                Close();
            };

            buttonCancel.Click += (sender, args) => Close();
        }

        /// <summary>
        /// Load values into the form
        /// </summary>
        private void LoadValues()
        {
            values = RobotExporterAddInServer.Instance.AddInSettingsManager;
            ChildHighlight.BackColor = values.JointHighlightColor;
            checkBox1.Checked = values.UseAnalytics;
        }

        /// <summary>
        /// Save the form's values in a <see cref="AddInSettingsManager"/> structure
        /// </summary>
        private void SaveValues()
        {
            values.JointHighlightColor = ChildHighlight.BackColor;
            values.UseAnalytics = checkBox1.Checked;
            values.SaveSettings();
        }

        /// <summary>
        /// Sets the <see cref="Color"/> of the <see cref="Button"/> and by extension the <see cref="AddInSettingsManager"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChildHighlight_Click(object sender, EventArgs e)
        {
            ColorDialog colorChoose = new ColorDialog();
            if (colorChoose.ShowDialog() == DialogResult.OK)
            {
                ChildHighlight.BackColor = colorChoose.Color;
            }
        }
    }
}