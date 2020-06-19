using System;
using System.Drawing;
using System.Windows.Forms;
using SynthesisExporterInventor.Managers;
using SynthesisExporterInventor.Utilities;

namespace SynthesisExporterInventor.GUI.Editors
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

            okButton.Click += (sender, args) =>
            {
                SaveValues();
                Close();
                values.SaveSettings();
            };

            cancelButton.Click += (sender, args) => Close();
        }

        /// <summary>
        /// Load values into the form
        /// </summary>
        private void LoadValues()
        {
            values = RobotExporterAddInServer.Instance.AddInSettingsManager;
            ChildHighlight.BackColor = values.JointHighlightColor;
            checkBox1.Checked = values.UseAnalytics;
            checkBox2.Checked = values.ShowGuide;
        }

        /// <summary>
        /// Save the form's values in a <see cref="AddInSettingsManager"/> structure
        /// </summary>
        private void SaveValues()
        {
            if (values.ShowGuide != checkBox2.Checked) AnalyticsUtils.LogEvent("Settings", "Guide Toggle", checkBox2.Checked ? "Enabled" : "Disabled");
            if (!values.JointHighlightColor.Equals(ChildHighlight.BackColor)) AnalyticsUtils.LogEvent("Settings", "Highlight Color Changed");
            values.JointHighlightColor = ChildHighlight.BackColor;
            values.UseAnalytics = checkBox1.Checked;
            values.ShowGuide = checkBox2.Checked;
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