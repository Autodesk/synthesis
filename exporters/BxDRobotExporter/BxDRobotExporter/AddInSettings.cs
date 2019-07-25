using System.Drawing;
using BxDRobotExporter.Exporter;
using BxDRobotExporter.GUI.Editors;
using BxDRobotExporter.Properties;

namespace BxDRobotExporter
{
    /// <summary>
    /// The struct that stores settings for the <see cref="Exporter"/>
    /// </summary>
    public class AddInSettings
    {
        // Events
        public event SettingsEvent SettingsChanged;

        // Environment
        public Color InventorChildColor;
        public bool UseAnalytics;
        
        // Export
        public string GeneralSaveLocation;
        public bool GeneralUseFancyColors;
        public string FieldName;
        public string DefaultRobotCompetition;
        public bool OpenSynthesis;
        
        public AddInSettings()
        {
            LoadSettings();
        }

        internal void OnSettingsChanged()
        {
            SaveSettings();
            if (SettingsChanged != null) SettingsChanged.Invoke(this);
        }

        private void SaveSettings()
        {
            Settings.Default.ExportToField = OpenSynthesis;
            Settings.Default.SelectedField = FieldName;
            Settings.Default.ChildColor = InventorChildColor;
            Settings.Default.FancyColors = GeneralUseFancyColors;
            Settings.Default.SaveLocation = GeneralSaveLocation;
            Settings.Default.DefaultRobotCompetition = DefaultRobotCompetition;
            Settings.Default.UseAnalytics = UseAnalytics;
            Settings.Default.ConfigVersion = 3; // Update this config version number when changes are made to the exporter which require settings to be reset or changed when the exporter starts
            Settings.Default.Save();
        }

        /// <summary>
        /// Initializes all of the <see cref="RobotData"/> settings to the proper values. Should be called once in the Activate class
        /// </summary>
        public void LoadSettings()
        {
            // Old configurations get overriden (version numbers below 1)
            if (Settings.Default.SaveLocation == "" || Settings.Default.SaveLocation == "firstRun" || Settings.Default.ConfigVersion < 2)
                Settings.Default.SaveLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Synthesis\Robots";

            if (Settings.Default.ConfigVersion < 3)
            {
                InventorChildColor = Settings.Default.ChildColor;
                GeneralSaveLocation = Settings.Default.SaveLocation;
                GeneralUseFancyColors = Settings.Default.FancyColors;
                OpenSynthesis = Settings.Default.ExportToField;
                FieldName = Settings.Default.SelectedField;
                DefaultRobotCompetition = "GENERIC";
                UseAnalytics = true;
            }
            else
            {
                InventorChildColor = Settings.Default.ChildColor;
                GeneralSaveLocation = Settings.Default.SaveLocation;
                GeneralUseFancyColors = Settings.Default.FancyColors;
                OpenSynthesis = Settings.Default.ExportToField;
                FieldName = Settings.Default.SelectedField;
                DefaultRobotCompetition = Settings.Default.DefaultRobotCompetition;
                UseAnalytics = Settings.Default.UseAnalytics;
            }
        }
    }
}