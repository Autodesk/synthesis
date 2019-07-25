using System;
using System.Drawing;
using System.Windows.Forms;
using BxDRobotExporter.Exporter;

namespace BxDRobotExporter.GUI.Editors
{
    public delegate void SettingsEvent(PluginSettings values);

    public partial class ExporterSettingsForm : Form
    {
        /// <summary>
        /// The local copy of the setting values
        /// </summary>
        private static PluginSettings Values = new PluginSettings();

        public ExporterSettingsForm()
        {
            InitializeComponent();

            LoadValues();

            buttonOK.Click += delegate(object sender, EventArgs e)
            {
                SaveValues();
                Close();
            };

            buttonCancel.Click += delegate(object sender, EventArgs e) { Close(); };
        }

        /// <summary>
        /// Load values into the form
        /// </summary>
        private void LoadValues()
        {
            Values = RobotExporterAddInServer.PluginSettings;

            ChildHighlight.BackColor = Values.InventorChildColor;
            checkBox1.Checked = Values.UseAnalytics;
        }

        /// <summary>
        /// Save the form's values in a <see cref="PluginSettings"/> structure
        /// </summary>
        private void SaveValues()
        {
            Values.InventorChildColor = ChildHighlight.BackColor;
            Values.UseAnalytics = checkBox1.Checked;
            Values.OnSettingsChanged();
        }

        /// <summary>
        /// Get the default values for the <see cref="PluginSettings"/> structure
        /// </summary>
        /// <returns>Default values for the <see cref="Exporter"/></returns>
        public static PluginSettings GetDefaultSettings()
        {
            return new PluginSettings()
            {
                InventorChildColor = Color.FromArgb(255, 0, 125, 255),
                GeneralUseFancyColors = true,
                GeneralSaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Synthesis\Robots",
                UseAnalytics = true
            };
        }

        /// <summary>
        /// Sets the <see cref="Color"/> of the <see cref="Button"/> and by extension the <see cref="PluginSettings"/>
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

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.ShowDialog();
            if (folderBrowser.SelectedPath != null)
            {
                Values.GeneralSaveLocation = folderBrowser.SelectedPath;
            }
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            Values.UseAnalytics = checkBox1.Checked;
        }
    }
}