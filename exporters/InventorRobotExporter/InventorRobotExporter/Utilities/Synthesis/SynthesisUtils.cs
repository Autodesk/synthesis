using System;
using System.Diagnostics;

namespace InventorRobotExporter.Utilities.Synthesis
{
    public static class SynthesisUtils
    {
        private const string SynthesisPath = @"C:\Program Files\Autodesk\Synthesis\Synthesis\Synthesis.exe";

        /// <summary>
        /// Open Synthesis to a specific robot and field.
        /// </summary>
        public static void OpenSynthesis(string robotName)
        {
            if (robotName != null)
            {
                Process.Start(SynthesisPath, $"-robot \"{RobotExporterAddInServer.Instance.AddInSettingsManager.ExportPath + "\\" + robotName}\"");
                AnalyticsUtils.LogEvent("Exporter", "Opened Synthesis");
            }
        }
    }
}