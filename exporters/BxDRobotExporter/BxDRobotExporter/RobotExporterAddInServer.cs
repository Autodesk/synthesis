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
using Inventor;

namespace BxDRobotExporter
{
    /// <summary>
    /// This is where the magic happens. All top-level event handling, UI creation, and inventor communication is handled here.
    /// </summary>
    [Guid("0c9a07ad-2768-4a62-950a-b5e33b88e4a3")]
    public class RobotExporterAddInServer : SingleEnvironmentAddInServer, ApplicationAddInServer
    {
        // ------ Add-In ------
        public static RobotExporterAddInServer Instance { get; private set; }
        public readonly AddInSettings AddInSettings = new AddInSettings();

        // ------ Robot Document ------
        private RobotData robotData;
        private List<ComponentOccurrence> disabledAssemblyOccurrences;

        // ------ UI ------
        // Environment
        private Environment exporterEnv;

        //Ribbon Pannels
        private RibbonPanel driveTrainPanel;
        private RibbonPanel jointPanel;
        private RibbonPanel checklistPanel;
        private RibbonPanel pluginPanel;
        private RibbonPanel exitPanel;

        //Standalone Buttons
        private ButtonDefinition driveTrainTypeButton;
        private ButtonDefinition drivetrainWeightButton;

        private ButtonDefinition advancedEditJointButton;
        private ButtonDefinition editJointButton;

        private ButtonDefinition guideButton;
        private ButtonDefinition dofButton;
        private ButtonDefinition settingsButton;

        // Dockable window managers
        private readonly AdvancedJointEditor advancedJointEditor = new AdvancedJointEditor(false);
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

            var editJointIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.JointEditor32)); //these are still here at request of QA
            var editJointIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.JointEditor32));

            var drivetrainTypeIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.DrivetrainType32)); //these are still here at request of QA
            var drivetrainTypeIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.DrivetrainType32));

            var guideIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.Guide32)); //these are still here at request of QA
            var guideIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.Guide32));

            var drivetrainWeightIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.RobotWeight32));
            var drivetrainWeightIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.RobotWeight32));

            var synthesisLogoSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.SynthesisLogo16));
            var synthesisLogoLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.SynthesisLogo32));

            var gearLogoSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.Gears16));
            var gearLogoLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.Gears32));

            exporterEnv = Application.UserInterfaceManager.Environments.Add("Robot Export", "BxD:RobotExporter:Environment", null, synthesisLogoSmall, synthesisLogoLarge);

            var exporterTab = Application.UserInterfaceManager.Ribbons["Assembly"].RibbonTabs.Add("Robot Export", "BxD:RobotExporter:RobotExporterTab", clientId, "", false, true);

            var controlDefs = Application.CommandManager.ControlDefinitions;

            driveTrainPanel = exporterTab.RibbonPanels.Add("Drive Train Setup", "BxD:RobotExporter:DriveTrainPanel", clientId);
            jointPanel = exporterTab.RibbonPanels.Add("Joint Setup", "BxD:RobotExporter:JointPanel", clientId);
            checklistPanel = exporterTab.RibbonPanels.Add("Robot Setup Checklist", "BxD:RobotExporter:ChecklistPanel", clientId);
            pluginPanel = exporterTab.RibbonPanels.Add("Plugin", "BxD:RobotExporter:PluginSettings", clientId);
            exitPanel = exporterTab.RibbonPanels.Add("Finish", "BxD:RobotExporter:ExitPanel", clientId);

            // Reset positioning of panels
            jointPanel.Reposition("BxD:RobotExporter:DriveTrainPanel", false);
            checklistPanel.Reposition("BxD:RobotExporter:JointPanel", false);
            exitPanel.Reposition("BxD:RobotExporter:ChecklistPanel", false);

            //Drive Train panel buttons
            driveTrainTypeButton = controlDefs.AddButtonDefinition("Drive Train\nType",
                "BxD:RobotExporter:SetDriveTrainType", CommandTypesEnum.kNonShapeEditCmdType, clientId, null,
                "Select the drivetrain type (tank, H-drive, or mecanum).", drivetrainTypeIconSmall, drivetrainTypeIconLarge);
            driveTrainTypeButton.OnExecute += context => new DrivetrainTypeForm(robotData).ShowDialog();
            driveTrainPanel.CommandControls.AddButton(driveTrainTypeButton, true);

            drivetrainWeightButton = controlDefs.AddButtonDefinition("Drive Train\nWeight",
                "BxD:RobotExporter:SetDriveTrainWeight", CommandTypesEnum.kNonShapeEditCmdType, clientId, null,
                "Assign the weight of the drivetrain.", drivetrainWeightIconSmall, drivetrainWeightIconLarge);
            drivetrainWeightButton.OnExecute += context => AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "Set Weight");
            drivetrainWeightButton.OnExecute += context => robotData.PromptRobotWeight();
            driveTrainPanel.CommandControls.AddButton(drivetrainWeightButton, true);

            // Joint panel buttons
            advancedEditJointButton = controlDefs.AddButtonDefinition("Advanced Editor", "BxD:RobotExporter:AdvancedEditJoint",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "Joint editor for advanced users.", editJointIconSmall, editJointIconLarge);
            advancedEditJointButton.OnExecute += context => AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "Advanced Edit Joint");
            advancedEditJointButton.OnExecute += context => advancedJointEditor.Visible = !advancedJointEditor.Visible;
            jointPanel.SlideoutControls.AddButton(advancedEditJointButton);

            editJointButton = controlDefs.AddButtonDefinition("Edit Joints", "BxD:RobotExporter:EditJoint",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "Edit existing joints.", editJointIconSmall, editJointIconLarge);
            editJointButton.OnExecute += context =>
            {
                AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "Edit Joint");
                jointForm.ShowDialog();
                advancedJointEditor.UpdateSkeleton(robotData);
            };
            jointPanel.CommandControls.AddButton(editJointButton, true);

            // ChecklistPanel buttons
            guideButton = controlDefs.AddButtonDefinition("Toggle Robot\nExport Guide", "BxD:RobotExporter:Guide",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null,
                "View a checklist of all tasks necessary prior to export.", guideIconSmall, guideIconLarge);
            guideButton.OnExecute += context => guide.Visible = !guide.Visible;
            checklistPanel.CommandControls.AddButton(guideButton, true);

            dofButton = controlDefs.AddButtonDefinition("Toggle Degrees\nof Freedom View", "BxD:RobotExporter:DOF",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "View degrees of freedom.", guideIconSmall, guideIconLarge);
            dofButton.OnExecute += context =>
            {
                AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "DOF");
                HighlightManager.ToggleDofHighlight(robotData);
                dofKey.Visible = HighlightManager.DisplayDof;
            };
            checklistPanel.CommandControls.AddButton(dofButton, true);

            settingsButton = controlDefs.AddButtonDefinition("Plugin Settings", "BxD:RobotExporter:Settings",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "View degrees of freedom.", gearLogoSmall, gearLogoLarge);
            settingsButton.OnExecute += context => new ExporterSettingsForm().ShowDialog();
            pluginPanel.CommandControls.AddButton(settingsButton, true);

            exporterEnv.DefaultRibbonTab = "BxD:RobotExporter:RobotExporterTab";
            exporterEnv.DisabledCommandList.Add(Application.CommandManager.ControlDefinitions["BxD:RobotExporter:Environment"]); // TODO: Can this be removed?

            Instance = this;

            return exporterEnv;
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
            robotData = new RobotData();
            if (!robotData.LoadRobotSkeleton())
            {
                InventorUtils.ForceQuitExporter(OpenDocument);
                return;
            }

            robotData.LoadRobotData(OpenDocument);
            
            // Create dockable window UI
            var uiMan = Application.UserInterfaceManager;
            advancedJointEditor.CreateDockableWindow(uiMan);
            dofKey.CreateDockableWindow(uiMan);
            guide.CreateDockableWindow(uiMan);

            // Load skeleton into joint editors
            advancedJointEditor.UpdateSkeleton(robotData);
            jointForm.UpdateSkeleton(robotData);
        }

        protected override void OnEnvironmentClose()
        {
            AnalyticsUtils.EndSession();
            robotData.SaveRobotData(OpenDocument);

            var exportResult = MessageBox.Show(
                "The robot configuration has been saved to your assembly document.\nWould you like to export your robot to Synthesis?",
                "Robot Configuration Complete",
                MessageBoxButtons.YesNo);

            if (exportResult == DialogResult.Yes)
            {
                if (robotData.PromptExportSettings())
                    if (robotData.ExportRobot() && robotData.ExportDefaultField != null)
                        robotData.OpenSynthesis();
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
        }
        
        protected override bool DocumentCondition(Document document)
        {
            return document is AssemblyDocument;
        }
    }
}