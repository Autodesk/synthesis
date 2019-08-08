using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using InventorRobotExporter.GUI.Editors;
using InventorRobotExporter.GUI.Editors.AdvancedJointEditor;
using InventorRobotExporter.GUI.Editors.JointEditor;
using InventorRobotExporter.GUI.Guide;
using InventorRobotExporter.Managers;
using InventorRobotExporter.Properties;
using InventorRobotExporter.Utilities;
using InventorRobotExporter.Utilities.Synthesis;
using Inventor;
using InventorRobotExporter.GUI.JointView;
using InventorRobotExporter.GUI.Loading;
using NUnit.Framework;
using OpenTK.Input;
using static InventorRobotExporter.Utilities.ImageFormat.PictureDispConverter;
using Environment = Inventor.Environment;

namespace InventorRobotExporter
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
        private readonly AdvancedJointEditor advancedJointEditor = new AdvancedJointEditor();
        private readonly JointViewKey jointViewKey = new JointViewKey();
        public readonly GuideManager guideManager = new GuideManager(true);

        // Other managers
        public readonly HighlightManager HighlightManager = new HighlightManager();

        // UI elements
        private readonly JointEditorForm jointEditorForm = new JointEditorForm();

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

            driveTrainTypeButton = controlDefs.AddButtonDefinition("Drive Train\nLayout",
                "BxD:RobotExporter:SetDriveTrainType", CommandTypesEnum.kNonShapeEditCmdType, clientId, null,
                "Select the drivetrain type (tank, H-drive, or mecanum).", ToIPictureDisp(new Bitmap(Resources.DrivetrainType32)), ToIPictureDisp(new Bitmap(Resources.DrivetrainType32)));
            driveTrainTypeButton.OnExecute += context => new DrivetrainLayoutForm(RobotDataManager).ShowDialog();
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
            advancedEditJointButton.OnExecute += context =>
            {
                if (advancedJointEditor.Visible) return;
                advancedJointEditor.Visible = true;
                jointEditorForm.Visible = false;
            };
            jointPanel.SlideoutControls.AddButton(advancedEditJointButton);

            editJointButton = controlDefs.AddButtonDefinition("Edit Joints", "BxD:RobotExporter:EditJoint",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "Edit existing joints.", ToIPictureDisp(new Bitmap(Resources.JointEditor32)), ToIPictureDisp(new Bitmap(Resources.JointEditor32)));
            editJointButton.OnExecute += context =>
            {
                AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "Edit Joint");
                if (!jointEditorForm.HasJoints())
                {
                    var result = MessageBox.Show("No rotational or slider joints detected in the assembly! Add joints to your robot by using the \"Joint\" button under \"Assemble\" and restart the robot export environment to edit joints.\n\n" +
                                    "Would you like to view a video tutorial on adding joints to your assembly?", "No Joints Found", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                    if (result.Equals(DialogResult.Yes))
                        System.Diagnostics.Process.Start("https://youtu.be/fY3Vdkh8L0Y");
                    return;
                }
                
                if (jointEditorForm.Visible)
                {
                    jointEditorForm.Activate();
                }
                else
                {
                    jointEditorForm.PreShow();
                    jointEditorForm.Show();
                    advancedJointEditor.Visible = false;
                }
            };
            jointPanel.CommandControls.AddButton(editJointButton, true);

            // PRECHECK PANEL
            precheckPanel = exporterTab.RibbonPanels.Add("Export Precheck", "BxD:RobotExporter:ChecklistPanel", clientId);

            dofButton = controlDefs.AddButtonDefinition("Toggle Joint\nViewer", "BxD:RobotExporter:JointViewer",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "View status of all joints.", ToIPictureDisp(new Bitmap(Resources.Guide32)), ToIPictureDisp(new Bitmap(Resources.Guide32)));
            dofButton.OnExecute += context =>
            {
                AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "DOF");
                jointViewKey.Visible = !jointViewKey.Visible;
                HighlightManager.DisplayJointHighlight = jointViewKey.Visible;
            };
            precheckPanel.CommandControls.AddButton(dofButton, true);

            // ADD-IN SETTINGS PANEL
            addInSettingsPanel = exporterTab.RibbonPanels.Add("Add-In", "BxD:RobotExporter:AddInSettings", clientId);

            settingsButton = controlDefs.AddButtonDefinition("Add-In Settings", "BxD:RobotExporter:Settings",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "Configure add-in settings.", ToIPictureDisp(new Bitmap(Resources.Gears16)), ToIPictureDisp(new Bitmap(Resources.Gears32)));
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

            Application.UserInterfaceManager.UserInteractionDisabled = true;
            var loadingBar = new LoadingBar("Loading Export Environment...");
            loadingBar.SetProgress(new ProgressUpdate("Preparing UI Managers...", 1, 10));
            loadingBar.Show();
            HighlightManager.EnvironmentOpening(OpenAssemblyDocument);

            // Disable non-jointed components
            disabledAssemblyOccurrences = new List<ComponentOccurrence>();
            disabledAssemblyOccurrences.AddRange(InventorUtils.DisableUnconnectedComponents(OpenAssemblyDocument));

            loadingBar.SetProgress(new ProgressUpdate("Loading Robot Skeleton...", 2, 10));
            // Load robot skeleton and prepare UI
            RobotDataManager = new RobotDataManager();
            if (!RobotDataManager.LoadRobotSkeleton(new Progress<ProgressUpdate>(loadingBar.SetProgress)))
            {
                loadingBar.Close();
                Application.UserInterfaceManager.UserInteractionDisabled = false;
                InventorUtils.ForceQuitExporter(OpenAssemblyDocument);
                return;
            }

            if (RobotDataManager.wasForceQuit)
                return;

            loadingBar.SetProgress(new ProgressUpdate("Loading Joint Data...", 7, 10));
            RobotDataManager.LoadRobotData(OpenAssemblyDocument);

            loadingBar.SetProgress(new ProgressUpdate("Initializing UI...", 8, 10));
            // Create dockable window UI
            var uiMan = Application.UserInterfaceManager;
            advancedJointEditor.CreateDockableWindow(uiMan);
            jointViewKey.Init(uiMan);
            guideManager.Init(uiMan);

            guideManager.Visible = AddInSettingsManager.ShowGuide;

            loadingBar.SetProgress(new ProgressUpdate("Loading Robot Skeleton...", 9, 10));
            // Load skeleton into joint editors
            advancedJointEditor.LoadRobot(RobotDataManager);
            jointEditorForm.LoadRobot(RobotDataManager);
            loadingBar.Close();
            Application.UserInterfaceManager.UserInteractionDisabled = false;
        }

        protected override void OnEnvironmentClose()
        {
            AnalyticsUtils.EndSession();
            
            Application.UserInterfaceManager.UserInteractionDisabled = true;
            var loadingBar = new LoadingBar("Closing Export Environment...");
            loadingBar.SetProgress(new ProgressUpdate("Saving Robot Data...", 3, 5));
            loadingBar.Show();
            RobotDataManager.SaveRobotData(OpenAssemblyDocument);
            loadingBar.Close();
            Application.UserInterfaceManager.UserInteractionDisabled = false;

            if (!RobotDataManager.wasForceQuit)
            {
                var exportResult = MessageBox.Show(new Form { TopMost = true },
                    "The robot configuration has been saved to your assembly document.\nWould you like to export your robot to Synthesis?",
                    "Robot Configuration Complete",
                    MessageBoxButtons.YesNo);

                if (exportResult == DialogResult.Yes)
                {
                    RobotDataManager.VerifyJoints();
                    if (ExportForm.PromptExportSettings(RobotDataManager))
                        if (RobotDataManager.ExportRobot())
                            SynthesisUtils.OpenSynthesis(RobotDataManager.RobotName);
                }
            }

            // Re-enable disabled components
            if (disabledAssemblyOccurrences != null) InventorUtils.EnableComponents(disabledAssemblyOccurrences);
            disabledAssemblyOccurrences = null;

            advancedJointEditor.DestroyDockableWindow();
            guideManager.DestroyDockableWindow();
            jointViewKey.DestroyDockableWindow();
        }

        protected override void OnEnvironmentHide()
        {
            // Hide dockable windows when switching to a different document 
            advancedJointEditor.TemporaryHide();
            jointViewKey.TemporaryHide();
            guideManager.TemporaryHide();
        }

        protected override void OnEnvironmentShow()
        {
            // Restore visible state of dockable windows
            advancedJointEditor.Visible = advancedJointEditor.Visible;
            jointViewKey.Visible = jointViewKey.Visible;
            guideManager.Visible = guideManager.Visible;
            // And DOF highlight in case that gets cleared
            HighlightManager.DisplayJointHighlight = jointViewKey.Visible;
        }

        protected override bool IsDocumentSupported(Document document)
        {
            return document is AssemblyDocument; // The robot exporter is only compatible with assembly documents
        }
    }
}