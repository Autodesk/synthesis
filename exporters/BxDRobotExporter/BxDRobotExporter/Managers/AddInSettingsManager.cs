using System.Drawing;
using BxDRobotExporter.GUI.Editors;
using BxDRobotExporter.Properties;

namespace BxDRobotExporter.Managers
{
    /// <summary>
    /// The struct that stores settings for the add-in
    /// </summary>
    public class AddInSettingsManager
    {
        // Environment
        public bool UseAnalytics;
        public Color JointHighlightColor;

        // Export constants
        public string ExportPath;

        // Export defaults (can be changed by user)
        public bool DefaultExportWithColors;
        public string DefaultField;
        public string DefaultRobotCompetition;
        public bool OpenSynthesis;
        
        // Events
        public event SettingsEvent SettingsChanged;
        
        public AddInSettingsManager()
        {
            LoadSettings(); // Load settings on construction
        }

        /// <summary>
        /// Saves all add-in settings
        /// </summary>
        public void SaveSettings()
        {
            Settings.Default.ExportToField = OpenSynthesis;
            Settings.Default.SelectedField = DefaultField;
            Settings.Default.ChildColor = JointHighlightColor;
            Settings.Default.FancyColors = DefaultExportWithColors;
            Settings.Default.SaveLocation = ExportPath;
            Settings.Default.DefaultRobotCompetition = DefaultRobotCompetition;
            Settings.Default.UseAnalytics = UseAnalytics;
            Settings.Default.ConfigVersion = 3; // Update this config version number when changes are made to the exporter which require settings to be reset or changed when the exporter starts
            Settings.Default.Save();
            SettingsChanged?.Invoke(this);
        }

        /// <summary>
        /// Loads all add-in settings
        /// </summary>
        public void LoadSettings()
        {
            // Old configurations get overriden (version numbers below 1) TODO: Remove all old versions
            if (Settings.Default.SaveLocation == "" || Settings.Default.SaveLocation == "firstRun" || Settings.Default.ConfigVersion < 2)
                Settings.Default.SaveLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Synthesis\Robots";

            if (Settings.Default.ConfigVersion < 3)
            {
                JointHighlightColor = Settings.Default.ChildColor;
                ExportPath = Settings.Default.SaveLocation;
                DefaultExportWithColors = Settings.Default.FancyColors;
                OpenSynthesis = Settings.Default.ExportToField;
                DefaultField = Settings.Default.SelectedField;
                DefaultRobotCompetition = "GENERIC";
                UseAnalytics = true;
            }
            else
            {
                JointHighlightColor = Settings.Default.ChildColor;
                ExportPath = Settings.Default.SaveLocation;
                DefaultExportWithColors = Settings.Default.FancyColors;
                OpenSynthesis = Settings.Default.ExportToField;
                DefaultField = Settings.Default.SelectedField;
                DefaultRobotCompetition = Settings.Default.DefaultRobotCompetition;
                UseAnalytics = Settings.Default.UseAnalytics;
            }
        }
    }
}