using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BxDRobotExporter.GUI.DegreesOfFreedomViewer;
using BxDRobotExporter.GUI.Editors;
using BxDRobotExporter.GUI.Editors.AdvancedJointEditor;
using BxDRobotExporter.GUI.Editors.JointEditor;
using BxDRobotExporter.GUI.Guide;
using BxDRobotExporter.Managers;
using BxDRobotExporter.Properties;
using BxDRobotExporter.Utilities;
using BxDRobotExporter.Utilities.Synthesis;
using Inventor;
using static BxDRobotExporter.Utilities.ImageFormat.PictureDispConverter;

namespace BxDRobotExporter
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
        public RobotDataManager RobotDataManager { get; private set; }
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

        private ButtonDefinition guideButton;
        private ButtonDefinition dofButton;

        private ButtonDefinition settingsButton;

        // Dockable window managers
        private readonly AdvancedJointEditor advancedJointEditor = new AdvancedJointEditor();
        private readonly DOFKey dofKey = new DOFKey();
        private readonly Guide guide = new Guide(true);

        // Other managers
        public readonly HighlightManager HighlightManager = new HighlightManager();

        // UI elements
        private readonly JointForm jointForm = new JointForm();

        protected override Environment CreateEnvironment()
        {
            const string clientId = "{0c9a07ad-2768-4a62-950a-b5e33b88e4a3}";
            AnalyticsUtils.SetUser(Application.UserName);
            AnalyticsUtils.LogPage("Inventor");

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
            // DRIVETRAIN PANEL
            driveTrainPanel = exporterTab.RibbonPanels.Add("Drive Train Setup", "BxD:RobotExporter:DriveTrainPanel", clientId);

            driveTrainTypeButton = controlDefs.AddButtonDefinition("Drive Train\nType",
                "BxD:RobotExporter:SetDriveTrainType", CommandTypesEnum.kNonShapeEditCmdType, clientId, null,
                "Select the drivetrain type (tank, H-drive, or mecanum).", ToIPictureDisp(new Bitmap(Resources.DrivetrainType32)), ToIPictureDisp(new Bitmap(Resources.DrivetrainType32)));
            driveTrainTypeButton.OnExecute += context => new DrivetrainTypeForm(RobotDataManager).ShowDialog();
            driveTrainPanel.CommandControls.AddButton(driveTrainTypeButton, true);

            drivetrainWeightButton = controlDefs.AddButtonDefinition("Drive Train\nWeight",
                "BxD:RobotExporter:SetDriveTrainWeight", CommandTypesEnum.kNonShapeEditCmdType, clientId, null,
                "Assign the weight of the drivetrain.", ToIPictureDisp(new Bitmap(Resources.RobotWeight32)), ToIPictureDisp(new Bitmap(Resources.RobotWeight32)));
            drivetrainWeightButton.OnExecute += context => AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "Set Weight");
            drivetrainWeightButton.OnExecute += context => RobotDataManager.PromptRobotWeight();
            driveTrainPanel.CommandControls.AddButton(drivetrainWeightButton, true);

            // JOINT PANEL
            jointPanel = exporterTab.RibbonPanels.Add("Joint Setup", "BxD:RobotExporter:JointPanel", clientId);

            advancedEditJointButton = controlDefs.AddButtonDefinition("Advanced Editor", "BxD:RobotExporter:AdvancedEditJoint",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "Joint editor for advanced users.", ToIPictureDisp(new Bitmap(Resources.JointEditor32)), ToIPictureDisp(new Bitmap(Resources.JointEditor32)));
            advancedEditJointButton.OnExecute += context => AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "Advanced Edit Joint");
            advancedEditJointButton.OnExecute += context => advancedJointEditor.Visible = !advancedJointEditor.Visible;
            jointPanel.SlideoutControls.AddButton(advancedEditJointButton);

            editJointButton = controlDefs.AddButtonDefinition("Edit Joints", "BxD:RobotExporter:EditJoint",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "Edit existing joints.", ToIPictureDisp(new Bitmap(Resources.JointEditor32)), ToIPictureDisp(new Bitmap(Resources.JointEditor32)));
            editJointButton.OnExecute += context =>
            {
                AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "Edit Joint");
                jointForm.ShowDialog();
                advancedJointEditor.UpdateSkeleton(RobotDataManager);
            };
            jointPanel.CommandControls.AddButton(editJointButton, true);

            // PRECHECK PANEL
            precheckPanel = exporterTab.RibbonPanels.Add("Robot Setup Checklist", "BxD:RobotExporter:ChecklistPanel", clientId);

            guideButton = controlDefs.AddButtonDefinition("Toggle Robot\nExport Guide", "BxD:RobotExporter:Guide",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null,
                "View a checklist of all tasks necessary prior to export.", ToIPictureDisp(new Bitmap(Resources.Guide32)), ToIPictureDisp(new Bitmap(Resources.Guide32)));
            guideButton.OnExecute += context => guide.Visible = !guide.Visible;
            precheckPanel.CommandControls.AddButton(guideButton, true);

            dofButton = controlDefs.AddButtonDefinition("Toggle Degrees\nof Freedom View", "BxD:RobotExporter:DOF",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "View degrees of freedom.", ToIPictureDisp(new Bitmap(Resources.Guide32)), ToIPictureDisp(new Bitmap(Resources.Guide32)));
            dofButton.OnExecute += context =>
            {
                AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "DOF");
                dofKey.Visible = !dofKey.Visible;
                HighlightManager.DisplayDof = dofKey.Visible;
            };
            precheckPanel.CommandControls.AddButton(dofButton, true);

            // ADD-IN SETTINGS PANEL
            addInSettingsPanel = exporterTab.RibbonPanels.Add("Plugin", "BxD:RobotExporter:PluginSettings", clientId);

            settingsButton = controlDefs.AddButtonDefinition("Plugin Settings", "BxD:RobotExporter:Settings",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "View degrees of freedom.", ToIPictureDisp(new Bitmap(Resources.Gears16)), ToIPictureDisp(new Bitmap(Resources.Gears32)));
            settingsButton.OnExecute += context => new ExporterSettingsForm().ShowDialog();
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

            HighlightManager.EnvironmentOpening(OpenDocument);

            // Disable non-jointed components
            disabledAssemblyOccurrences = new List<ComponentOccurrence>();
            disabledAssemblyOccurrences.AddRange(InventorUtils.DisableUnconnectedComponents(OpenDocument));

            // Load robot skeleton and prepare UI
            RobotDataManager = new RobotDataManager();
            if (!RobotDataManager.LoadRobotSkeleton())
            {
                InventorUtils.ForceQuitExporter(OpenDocument);
                return;
            }

            RobotDataManager.LoadRobotData(OpenDocument);

            // Create dockable window UI
            var uiMan = Application.UserInterfaceManager;
            advancedJointEditor.CreateDockableWindow(uiMan);
            dofKey.CreateDockableWindow(uiMan);
            guide.CreateDockableWindow(uiMan);

            // Load skeleton into joint editors
            advancedJointEditor.UpdateSkeleton(RobotDataManager);
            jointForm.UpdateSkeleton(RobotDataManager);
        }

        protected override void OnEnvironmentClose()
        {
            AnalyticsUtils.EndSession();
            RobotDataManager.SaveRobotData(OpenDocument);

            var exportResult = MessageBox.Show(
                "The robot configuration has been saved to your assembly document.\nWould you like to export your robot to Synthesis?",
                "Robot Configuration Complete",
                MessageBoxButtons.YesNo);

            if (exportResult == DialogResult.Yes)
            {
                if (ExportForm.PromptExportSettings(RobotDataManager))
                    if (RobotDataManager.ExportRobot() && RobotDataManager.RobotField != null)
                        SynthesisUtils.OpenSynthesis(RobotDataManager.RobotName, RobotDataManager.RobotName);
            }

            // Re-enable disabled components
            if (disabledAssemblyOccurrences != null) InventorUtils.EnableComponents(disabledAssemblyOccurrences);
            disabledAssemblyOccurrences = null;

            advancedJointEditor.DestroyDockableWindow();
            guide.DestroyDockableWindow();
            dofKey.DestroyDockableWindow();
        }

        protected override void OnEnvironmentHide()
        {
            // Hide dockable windows when switching to a different document 
            advancedJointEditor.TemporaryHide();
            dofKey.TemporaryHide();
            guide.TemporaryHide();
        }

        protected override void OnEnvironmentShow()
        {
            // Restore visible state of dockable windows
            advancedJointEditor.Visible = advancedJointEditor.Visible;
            dofKey.Visible = dofKey.Visible;
            guide.Visible = guide.Visible;
            // And DOF highlight in case that gets cleared
            HighlightManager.DisplayDof = dofKey.Visible;
        }

        protected override bool IsDocumentSupported(Document document)
        {
            return document is AssemblyDocument; // The robot exporter is only compatible with assembly documents
        }
    }
}