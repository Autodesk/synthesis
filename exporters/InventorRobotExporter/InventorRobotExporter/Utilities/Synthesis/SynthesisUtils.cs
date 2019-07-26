using System;
using System.Diagnostics;

namespace InventorRobotExporter.Utilities.Synthesis
{
    public static class SynthesisUtils
    {
        private const string SYNTHESIS_PATH = @"C:\Program Files\Autodesk\Synthesis\Synthesis\Synthesis.exe";

        /// <summary>
        /// Open Synthesis to a specific robot and field.
        /// </summary>
        public static void OpenSynthesis(string robotName, string fieldName)
        {
            if (robotName == null) return;
            if (fieldName == null) return;

            Process.Start(SYNTHESIS_PATH, String.Format("-robot \"{0}\" -field \"{1}\"", RobotExporterAddInServer.Instance.AddInSettingsManager.ExportPath + "\\" + robotName, fieldName));
        }
    }
}