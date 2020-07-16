using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SynthesisInventorGltfExporter.GUI.Editors;
using SynthesisInventorGltfExporter.GUI.Guide;
using SynthesisInventorGltfExporter.Managers;
using SynthesisInventorGltfExporter.Properties;
using SynthesisInventorGltfExporter.Utilities;
using Inventor;
using SynthesisInventorGltfExporter.GUI.JointView;
using SynthesisInventorGltfExporter.GUI.Loading;
using SynthesisInventorGltfExporter.Utilities.Synthesis;
using static SynthesisInventorGltfExporter.Utilities.ImageConverters.PictureDispConverter;
using Environment = Inventor.Environment;

namespace SynthesisInventorGltfExporter
{
    /// <summary>
    /// This is where the magic happens. All top-level event handling, UI creation, and inventor communication is handled here.
    /// </summary>
    [Guid("0c9a07ad-2768-4a62-950a-b5e33b88e4a3")]
    public class RobotExporterAddInServer : SingleEnvironmentAddInServer
    {
        // Add-In
        public static RobotExporterAddInServer Instance { get; private set; }
        public readonly AddInSettingsManager AddInSettingsManager = new AddInSettingsManager();

        // Robot/Document

        public AssemblyDocument OpenAssemblyDocument => OpenDocument as AssemblyDocument; // We know this is an AssemblyDocument because of the condition in IsDocumentSupported()

        // private List<ComponentOccurrence> disabledAssemblyOccurrences;

        // Environment
        private Environment exporterEnv;

        //Ribbon Pannels
        private RibbonPanel driveTrainPanel;
        private RibbonPanel jointPanel;
        private RibbonPanel precheckPanel;
        private RibbonPanel addInSettingsPanel;

        //Standalone Buttons
        private ButtonDefinition driveTrainTypeButton;
        private ButtonDefinition drivetrainWeightButton;

        private ButtonDefinition advancedEditJointButton;
        private ButtonDefinition editJointButton;

        private ButtonDefinition dofButton;

        private ButtonDefinition settingsButton;

        // Dockable window managers
        private readonly JointViewKey jointViewKey = new JointViewKey();
        private readonly GuideManager guideManager = new GuideManager();

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
            exporterEnv = Application.UserInterfaceManager.Environments.Add("Robot Export", "BxD:RobotExporter:Environment", null, ToIPictureDisp(new Bitmap(Resources.SynthesisLogo16)), ToIPictureDisp(new Bitmap(Resources.SynthesisLogo32)));

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
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "Configure add-in settings wow.", ToIPictureDisp(new Bitmap(Resources.Gears16)), ToIPictureDisp(new Bitmap(Resources.Gears32)));
            settingsButton.OnExecute += context =>
            {
                AnalyticsUtils.LogPage("Exporter Settings");
                new ExporterSettingsForm().ShowDialog();
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
            AnalyticsUtils.StartSession();

        }

        protected override void OnEnvironmentClose()
        {
            Application.UserInterfaceManager.UserInteractionDisabled = true;
            var loadingBar = new LoadingBar("Closing Export Environment...");
            loadingBar.SetProgress(new ProgressUpdate("Saving Robot Data...", 3, 5));
            loadingBar.Show();
            loadingBar.Close();
            Application.UserInterfaceManager.UserInteractionDisabled = false;

            var exportResult = MessageBox.Show(new Form { TopMost = true },
                "The robot configuration has been saved to your assembly document.\nWould you like to export your robot to Synthesis?",
                "Robot Configuration Complete",
                MessageBoxButtons.YesNo);

            if (exportResult == DialogResult.Yes)
            {
                if (ExportForm.PromptExportSettings())
                {
                    // GLTFDesignExporter.ExportDesign()
                }
            }

            guideManager.DestroyDockableWindow();
            jointViewKey.DestroyDockableWindow();
            AnalyticsUtils.EndSession();
        }

        protected override void OnEnvironmentHide()
        {
            // Hide dockable windows when switching to a different document 
            jointViewKey.TemporaryHide();
            guideManager.TemporaryHide();
        }

        protected override void OnEnvironmentShow()
        {
            // Restore visible state of dockable windows
            jointViewKey.Visible = jointViewKey.Visible;
            guideManager.Visible = guideManager.Visible;
            // And DOF highlight in case that gets cleared
        }

        protected override bool IsDocumentSupported(Document document)
        {
            return document is AssemblyDocument; // The robot exporter is only compatible with assembly documents
        }
    }
}