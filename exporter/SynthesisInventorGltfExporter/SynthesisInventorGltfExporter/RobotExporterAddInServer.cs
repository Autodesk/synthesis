using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Inventor;
using Environment = Inventor.Environment;

namespace SynthesisInventorGltfExporter
{
    /// <summary>
    /// This is where the magic happens. All top-level event handling, UI creation, and inventor communication is handled here.
    /// </summary>
    [Guid("fd06cbaa-b8e9-4c50-b78e-601cc74766fb")]
    public class RobotExporterAddInServer : SingleEnvironmentAddInServer
    {
        // Add-In
        public static RobotExporterAddInServer Instance { get; private set; }

        // Robot/Document

        public AssemblyDocument OpenAssemblyDocument => OpenDocument as AssemblyDocument; // We know this is an AssemblyDocument because of the condition in IsDocumentSupported()

        // private List<ComponentOccurrence> disabledAssemblyOccurrences;

        // Environment
        private Environment exporterEnv;

        //Ribbon Pannels
        private RibbonPanel addInSettingsPanel;

        //Standalone Buttons
        private ButtonDefinition settingsButton;

        // Dockable window managers

        // Other managers

        // UI elements

        protected override Environment CreateEnvironment()
        {
            const string clientId = "{0c9a07ad-2768-4a62-950a-b5e33b88e4a3}";

            InitEnvironment(clientId);

            Instance = this;
            return exporterEnv;
        }

        private void InitEnvironment(string clientId)
        {
            exporterEnv = Application.UserInterfaceManager.Environments.Add("Robot Export", "BxD:RobotExporter:Environment", null);

            var exporterTab = Application.UserInterfaceManager.Ribbons["Assembly"].RibbonTabs.Add("Robot Export", "BxD:RobotExporter:RobotExporterTab", clientId, "", false, true);
            var controlDefs = Application.CommandManager.ControlDefinitions;

            InitEnvironmentPanels(exporterTab, clientId, controlDefs);

            exporterEnv.DefaultRibbonTab = "BxD:RobotExporter:RobotExporterTab";
            exporterEnv.DisabledCommandList.Add(Application.CommandManager.ControlDefinitions["BxD:RobotExporter:Environment"]); // TODO: Can this be removed?
        }

        private void InitEnvironmentPanels(RibbonTab exporterTab, string clientId, ControlDefinitions controlDefs)
        {
            // ADD-IN SETTINGS PANEL
            addInSettingsPanel = exporterTab.RibbonPanels.Add("Add-In", "BxD:RobotExporter:AddInSettings", clientId);

            settingsButton = controlDefs.AddButtonDefinition("Add-In Settings", "BxD:RobotExporter:Settings",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "Configure add-in settings wow.");
            settingsButton.OnExecute += context =>
            {
            };
            addInSettingsPanel.CommandControls.AddButton(settingsButton, true);
        }

        protected override void DestroyEnvironment()
        {
            exporterEnv.Delete();
            exporterEnv = null;
        }

        protected override void OnEnvironmentOpen()
        {

        }

        protected override void OnEnvironmentClose()
        {
            Application.UserInterfaceManager.UserInteractionDisabled = true;
            Application.UserInterfaceManager.UserInteractionDisabled = false;

            // var exportResult = MessageBox.Show(new Form { TopMost = true },
            //     "The robot configuration has been saved to your assembly document.\nWould you like to export your robot to Synthesis?",
            //     "Robot Configuration Complete",
            //     MessageBoxButtons.YesNo);

            // if (exportResult == DialogResult.Yes)
            // {
            //     if (ExportForm.PromptExportSettings())
            //     {
            //         // GLTFDesignExporter.ExportDesign()
            //     }
            // }




        }

        protected override void OnEnvironmentHide()
        {
            // Hide dockable windows when switching to a different document 
        }

        protected override void OnEnvironmentShow()
        {
            // Restore visible state of dockable windows
            // And DOF highlight in case that gets cleared
        }

        protected override bool IsDocumentSupported(Document document)
        {
            return document is AssemblyDocument; // The robot exporter is only compatible with assembly documents
        }
    }
}