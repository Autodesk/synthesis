using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using SynthesisExporterInventor.GUI.Editors;
using SynthesisExporterInventor.GUI.Guide;
using SynthesisExporterInventor.Managers;
using SynthesisExporterInventor.Properties;
using SynthesisExporterInventor.Utilities;
using Inventor;
using SynthesisExporterInventor.GUI.JointView;
using SynthesisExporterInventor.GUI.Loading;
using static SynthesisExporterInventor.Utilities.ImageConverters.PictureDispConverter;
using Environment = Inventor.Environment;

namespace SynthesisExporterInventor
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

        private List<ComponentOccurrence> disabledAssemblyOccurrences;

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
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "Configure add-in settings.", ToIPictureDisp(new Bitmap(Resources.Gears16)), ToIPictureDisp(new Bitmap(Resources.Gears32)));
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

            Application.UserInterfaceManager.UserInteractionDisabled = true;
            var loadingBar = new LoadingBar("Loading Export Environment...");
            loadingBar.SetProgress(new ProgressUpdate("Preparing UI Managers...", 1, 10));
            loadingBar.Show();

            // Disable non-jointed components
            disabledAssemblyOccurrences = new List<ComponentOccurrence>();
            disabledAssemblyOccurrences.AddRange(InventorUtils.DisableUnconnectedComponents(OpenAssemblyDocument));

            loadingBar.SetProgress(new ProgressUpdate("Loading Robot Skeleton...", 2, 10));
            // Load robot skeleton and prepare UI

            loadingBar.SetProgress(new ProgressUpdate("Loading Joint Data...", 7, 10));

            loadingBar.SetProgress(new ProgressUpdate("Initializing UI...", 8, 10));
            // Create dockable window UI
            var uiMan = Application.UserInterfaceManager;
            jointViewKey.Init(uiMan);
            guideManager.Init(uiMan);

            guideManager.Visible = AddInSettingsManager.ShowGuide;

            loadingBar.SetProgress(new ProgressUpdate("Loading Robot Skeleton...", 9, 10));
            // Load skeleton into joint editors
            loadingBar.Close();
            Application.UserInterfaceManager.UserInteractionDisabled = false;
        }

        protected override void OnEnvironmentClose()
        {
            Application.UserInterfaceManager.UserInteractionDisabled = true;
            var loadingBar = new LoadingBar("Closing Export Environment...");
            loadingBar.SetProgress(new ProgressUpdate("Saving Robot Data...", 3, 5));
            loadingBar.Show();
            loadingBar.Close();
            Application.UserInterfaceManager.UserInteractionDisabled = false;

            // Re-enable disabled components
            if (disabledAssemblyOccurrences != null) InventorUtils.EnableComponents(disabledAssemblyOccurrences);
            disabledAssemblyOccurrences = null;

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