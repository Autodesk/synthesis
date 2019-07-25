using System;
using System.Diagnostics;

namespace BxDRobotExporter.Utilities.Synthesis
{
    public class SynthesisUtils
    {
        /// <summary>
        /// Open Synthesis to a specific robot and field.
        /// </summary>
        public static void OpenSynthesis(string robotName, string fieldName)
        {
            if (robotName == null) return;
            if (fieldName == null) return;

            Process.Start(InventorDocumentIoUtils.SYNTHESIS_PATH, String.Format("-robot \"{0}\" -field \"{1}\"", RobotExporterAddInServer.Instance.AddInSettings.ExportPath + "\\" + robotName, fieldName));
        }
    }
}