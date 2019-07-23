using System;
using System.Drawing;
using System.Windows.Forms;
using BxDRobotExporter.ControlGUI;
using BxDRobotExporter.Exporter;

namespace BxDRobotExporter.Editors
{
    public delegate void SettingsEvent(Color Child, bool UseFancyColors, string SaveLocation, bool OpenSynthesis, string FieldLocation, string defaultRobotCompetit, bool useAnalytics);

    public partial class ExporterSettingsForm : Form
    {
        /// <summary>
        /// The local copy of the setting values
        /// </summary>
        public static PluginSettingsValues Values = new PluginSettingsValues();

        public ExporterSettingsForm()
        {
            InitializeComponent();

            LoadValues();

            buttonOK.Click += delegate (object sender, EventArgs e)
            {
                SaveValues();
                Close();
            };

            buttonCancel.Click += delegate (object sender, EventArgs e)
            {
                Close();
            };
        }

        /// <summary>
        /// Load values into the form
        /// </summary>
        private void LoadValues()
        {
            Values = SynthesisGUI.PluginSettings;

            ChildHighlight.BackColor = Values.InventorChildColor;
            checkBox1.Checked = Values.useAnalytics;
        }
        
        /// <summary>
        /// Save the form's values in a <see cref="PluginSettingsValues"/> structure
        /// </summary>
        private void SaveValues()
        {
            Values.InventorChildColor = ChildHighlight.BackColor;
            Values.useAnalytics = checkBox1.Checked;
            Values.OnSettingsChanged();
        }

        /// <summary>
        /// Get the default values for the <see cref="PluginSettingsValues"/> structure
        /// </summary>
        /// <returns>Default values for the <see cref="Exporter"/></returns>
        public static PluginSettingsValues GetDefaultSettings()
        {
            return new PluginSettingsValues()
            {
                InventorChildColor = Color.FromArgb(255, 0, 125, 255),
                GeneralUseFancyColors = true,
                GeneralSaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Synthesis\Robots",
                useAnalytics = true
            };
        }

        /// <summary>
        /// The struct that stores settings for the <see cref="Exporter"/>
        /// </summary>
        public class PluginSettingsValues
        {

            public static event SettingsEvent SettingsChanged;
            internal void OnSettingsChanged()
            {
                SettingsChanged.Invoke(InventorChildColor, GeneralUseFancyColors, GeneralSaveLocation, openSynthesis, fieldName, defaultRobotCompetition, useAnalytics);
            }

            //General
            public string GeneralSaveLocation;
            public bool GeneralUseFancyColors;
            public string fieldName;
            public String defaultRobotCompetition;
            public bool openSynthesis;
            public bool useAnalytics;
            //Inventor
            public Color InventorChildColor;
        }

        /// <summary>
        /// Sets the <see cref="Color"/> of the <see cref="Button"/> and by extension the <see cref="PluginSettingsValues"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChildHighlight_Click(object sender, EventArgs e)
        {
            ColorDialog colorChoose = new ColorDialog();
            if(colorChoose.ShowDialog() == DialogResult.OK)
            {
                ChildHighlight.BackColor = colorChoose.Color;
            }
        }
        
        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.ShowDialog();
            if(folderBrowser.SelectedPath != null)
            {
                Values.GeneralSaveLocation = folderBrowser.SelectedPath;
            }
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            Values.useAnalytics = checkBox1.Checked;
        }
    }
}
