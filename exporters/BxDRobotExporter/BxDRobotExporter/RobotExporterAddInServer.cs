using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BxDRobotExporter.GUI.DegreesOfFreedomViewer;
using BxDRobotExporter.GUI.Editors;
using BxDRobotExporter.GUI.Editors.AdvancedJointEditor;
using BxDRobotExporter.GUI.Editors.JointEditor;
using BxDRobotExporter.GUI.Guide;
using BxDRobotExporter.Managers;
using BxDRobotExporter.Messages;
using BxDRobotExporter.Properties;
using Inventor;
using Application = Inventor.Application;
using Color = System.Drawing.Color;
using Environment = Inventor.Environment;

namespace BxDRobotExporter
{
    /// <summary>
    /// This is where the magic happens. All top-level event handling, UI creation, and inventor communication is handled here.
    /// </summary>
    [Guid("0c9a07ad-2768-4a62-950a-b5e33b88e4a3")]
    public class RobotExporterAddInServer : ApplicationAddInServer
    {
        public static RobotExporterAddInServer Instance { get; private set; }

        public readonly HighlightManager HighlightManager = new HighlightManager();
        public RobotDataManager RobotDataManager;

        public Application MainApplication;

        public AssemblyDocument AsmDocument;
        private List<ComponentOccurrence> disabledAssemblyOccurrences;

        // -- UI FIELDS
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
        private readonly AdvancedJointEditor advancedJointEditor = new AdvancedJointEditor();
        private readonly DOFKey dofKey = new DOFKey();
        private readonly Guide guide = new Guide();

        // UI elements
        private JointForm jointForm;

        // ???? (Weird flags which should be deleted)
        private bool exporterBlocked = false;
        private bool hiddenExporter = false;
        private bool environmentEnabled = false;


        /// <summary>
        /// Called when the <see cref="RobotExporterAddInServer"/> is being loaded
        /// </summary>
        /// <param name="addInSiteObject"></param>
        /// <param name="firstTime"></param>
        public void Activate(ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            MainApplication = addInSiteObject.Application;
            const string clientId = "{0c9a07ad-2768-4a62-950a-b5e33b88e4a3}";
            AnalyticsUtils.SetUser(MainApplication.UserName);
            AnalyticsUtils.LogPage("Inventor");
            LoadSettings();

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

            exporterEnv = MainApplication.UserInterfaceManager.Environments.Add("Robot Export", "BxD:RobotExporter:Environment", null, synthesisLogoSmall, synthesisLogoLarge);

            var exporterTab = MainApplication.UserInterfaceManager.Ribbons["Assembly"].RibbonTabs.Add("Robot Export", "BxD:RobotExporter:RobotExporterTab", clientId, "", false, true);

            var controlDefs = MainApplication.CommandManager.ControlDefinitions;

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
            driveTrainTypeButton.OnExecute += context => new DrivetrainTypeForm().ShowDialog();
            driveTrainTypeButton.OnHelp += _OnHelp;
            driveTrainPanel.CommandControls.AddButton(driveTrainTypeButton, true);

            drivetrainWeightButton = controlDefs.AddButtonDefinition("Drive Train\nWeight",
                "BxD:RobotExporter:SetDriveTrainWeight", CommandTypesEnum.kNonShapeEditCmdType, clientId, null,
                "Assign the weight of the drivetrain.", drivetrainWeightIconSmall, drivetrainWeightIconLarge);
            drivetrainWeightButton.OnExecute += context => AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "Set Weight");
            drivetrainWeightButton.OnExecute += context => RobotDataManager.PromptRobotWeight();
            drivetrainWeightButton.OnHelp += _OnHelp;
            driveTrainPanel.CommandControls.AddButton(drivetrainWeightButton, true);

            // Joint panel buttons
            advancedEditJointButton = controlDefs.AddButtonDefinition("Advanced Editor", "BxD:RobotExporter:AdvancedEditJoint",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "Joint editor for advanced users.", editJointIconSmall, editJointIconLarge);
            advancedEditJointButton.OnExecute += context => AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "Advanced Edit Joint");
            advancedEditJointButton.OnExecute += context => advancedJointEditor.Toggle();
            advancedEditJointButton.OnHelp += _OnHelp;
            jointPanel.SlideoutControls.AddButton(advancedEditJointButton);

            editJointButton = controlDefs.AddButtonDefinition("Edit Joints", "BxD:RobotExporter:EditJoint",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "Edit existing joints.", editJointIconSmall, editJointIconLarge);
            editJointButton.OnExecute += context =>
            {
                AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "Edit Joint");
                jointForm.ShowDialog();
                advancedJointEditor.UpdateSkeleton(RobotDataManager.Instance.SkeletonBase);
            };
            editJointButton.OnHelp += _OnHelp;
            jointPanel.CommandControls.AddButton(editJointButton, true);

            // ChecklistPanel buttons
            guideButton = controlDefs.AddButtonDefinition("Toggle Robot\nExport Guide", "BxD:RobotExporter:Guide",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null,
                "View a checklist of all tasks necessary prior to export.", guideIconSmall, guideIconLarge);
            guideButton.OnExecute += context => guide.Visible = !guide.Visible;
            guideButton.OnHelp += _OnHelp;
            checklistPanel.CommandControls.AddButton(guideButton, true);

            dofButton = controlDefs.AddButtonDefinition("Toggle Degrees\nof Freedom View", "BxD:RobotExporter:DOF",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "View degrees of freedom.", guideIconSmall, guideIconLarge);
            dofButton.OnExecute += context =>
            {
                AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "DOF");
                HighlightManager.ToggleDofHighlight(RobotDataManager);
                dofKey.Visible = HighlightManager.DisplayDof;
            };
            dofButton.OnHelp += _OnHelp;
            checklistPanel.CommandControls.AddButton(dofButton, true);

            settingsButton = controlDefs.AddButtonDefinition("Plugin Settings", "BxD:RobotExporter:Settings",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "View degrees of freedom.", gearLogoSmall, gearLogoLarge);
            settingsButton.OnExecute += context => new ExporterSettingsForm().ShowDialog();
            settingsButton.OnHelp += _OnHelp;
            pluginPanel.CommandControls.AddButton(settingsButton, true);

            exporterEnv.DefaultRibbonTab = "BxD:RobotExporter:RobotExporterTab";
            MainApplication.UserInterfaceManager.ParallelEnvironments.Add(exporterEnv);
            exporterEnv.DisabledCommandList.Add(MainApplication.CommandManager.ControlDefinitions["BxD:RobotExporter:Environment"]);

            MainApplication.UserInterfaceManager.UserInterfaceEvents.OnEnvironmentChange += UIEvents_OnEnvironmentChange;
            MainApplication.ApplicationEvents.OnActivateDocument += ApplicationEvents_OnActivateDocument;
            MainApplication.ApplicationEvents.OnDeactivateDocument += ApplicationEvents_OnDeactivateDocument;
            MainApplication.ApplicationEvents.OnCloseDocument += ApplicationEvents_OnCloseDocument;

            Instance = this;
        }

        /// <summary>
        /// Called when the <see cref="RobotExporterAddInServer"/> is being unloaded
        /// </summary>
        public void Deactivate()
        {
            Marshal.ReleaseComObject(MainApplication);
            MainApplication = null;
            exporterEnv.Delete();

            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        // For ApplicationAddInServer interface
        public void ExecuteCommand(int commandId) {}
        public object Automation => null;

        /// <summary>
        /// Gets the assembly document and makes the <see cref="DockableWindows"/>
        /// </summary>
        private void OpeningExporter()
        {
            AnalyticsUtils.StartSession();
            //Gets the assembly document and creates dockable windows
            AsmDocument = (AssemblyDocument) MainApplication.ActiveDocument;
            RobotDataManager = new RobotDataManager();

            HighlightManager.EnvironmentOpening(AsmDocument);


            //Sets up events for selecting and deselecting parts in inventor
            ExporterSettingsForm.PluginSettingsValues.SettingsChanged += ExporterSettings_SettingsChanged;

            environmentEnabled = true;

            // Load robot skeleton and prepare UI
            if (!RobotDataManager.LoadRobotSkeleton())
            {
                InventorUtils.ForceQuitExporter(AsmDocument);
                return;
            }

            disabledAssemblyOccurrences = new List<ComponentOccurrence>();
            disabledAssemblyOccurrences.AddRange(InventorUtils.DisableUnconnectedComponents(AsmDocument));
            // If fails to load existing data, restart wizard
            RobotDataManager.LoadRobotData(AsmDocument);

            UserInterfaceManager uiMan = MainApplication.UserInterfaceManager;

            advancedJointEditor.CreateDockableWindow(uiMan, RobotDataManager.SkeletonBase);
            dofKey.CreateDockableWindow(uiMan);
            guide.CreateDockableWindow(uiMan);

            // Hide non-jointed components;

            // Reload panels in UI
            jointForm = new JointForm();
            advancedJointEditor.Hide();
        }

        /// <summary>
        /// Disposes of some COM objects and exits the environment
        /// </summary>
        private void ClosingExporter()
        {
            AnalyticsUtils.EndSession();
            RobotDataManager.SaveRobotData();

            var exportResult = MessageBox.Show(
                "The robot configuration has been saved to your assembly document.\nWould you like to export your robot to Synthesis?",
                "Robot Configuration Complete",
                MessageBoxButtons.YesNo);

            if (exportResult == DialogResult.Yes)
            {
                if (RobotDataManager.PromptExportSettings())
                    if (RobotDataManager.ExportRobot() && RobotDataManager.RMeta.FieldName != null)
                        RobotDataManager.OpenSynthesis();
            }

            // Re-enable disabled components
            if (disabledAssemblyOccurrences != null) InventorUtils.EnableComponents(disabledAssemblyOccurrences);
            disabledAssemblyOccurrences = null;

            advancedJointEditor.DestroyDockableWindow();
            guide.DestroyDockableWindow();
            dofKey.DestroyDockableWindow();

            InventorUtils.ForceQuitExporter(AsmDocument);

            // Dispose of document
            if (AsmDocument != null)
                Marshal.ReleaseComObject(AsmDocument);
            AsmDocument = null;

            environmentEnabled = false;
        }

        /// <summary>
        /// Makes the dockable windows invisible when the document switches. This avoids data loss. 
        /// Also re-enables the exporter in the document if it was disabled. This allows the user to use the exporter in that document at a later point.
        /// </summary>
        /// <param name="documentObject"></param>
        /// <param name="beforeOrAfter"></param>
        /// <param name="context"></param>
        /// <param name="handlingCode"></param>
        private void ApplicationEvents_OnDeactivateDocument(_Document documentObject, EventTimingEnum beforeOrAfter,
            NameValueMap context, out HandlingCodeEnum handlingCode)
        {
            if (environmentEnabled)
            {
                if (beforeOrAfter == EventTimingEnum.kBefore)
                {
                    advancedJointEditor.Hide();
                    hiddenExporter = true;
                }
            }

            handlingCode = HandlingCodeEnum.kEventNotHandled;
        }

        /// <summary>
        /// Disables the environment button if you aren't in the assembly document that the exporter was originally opened in.
        /// </summary>
        /// <param name="documentObject"></param>
        /// <param name="beforeOrAfter"></param>
        /// <param name="context"></param>
        /// <param name="handlingCode"></param>
        private void ApplicationEvents_OnActivateDocument(_Document documentObject, EventTimingEnum beforeOrAfter,
            NameValueMap context, out HandlingCodeEnum handlingCode)
        {
            if (beforeOrAfter == EventTimingEnum.kBefore)
            {
                if (documentObject is AssemblyDocument assembly)
                {
                    if ((AsmDocument == null || assembly == AsmDocument) && hiddenExporter)
                    {
                        hiddenExporter = false;
                    }
                }
            }
            else if (beforeOrAfter == EventTimingEnum.kAfter)
            {
                if (Settings.Default.ShowFirstLaunchInfo)
                {
                    var firstLaunchInfo = new FirstLaunchInfo();
                    firstLaunchInfo.ShowDialog();
                }
            }

            handlingCode = HandlingCodeEnum.kEventNotHandled;
        }

        /// <summary>
        /// Called when the user closes a document. Used to release exporter when a document is closed.
        /// </summary>
        /// <param name="documentObject"></param>
        /// <param name="fullDocumentName"></param>
        /// <param name="beforeOrAfter"></param>
        /// <param name="context"></param>
        /// <param name="handlingCode"></param>
        private void ApplicationEvents_OnCloseDocument(_Document documentObject, string fullDocumentName,
            EventTimingEnum beforeOrAfter, NameValueMap context, out HandlingCodeEnum handlingCode)
        {
            // Quit the exporter if the closing document has the exporter open
            if (beforeOrAfter == EventTimingEnum.kBefore && environmentEnabled)
            {
                if (documentObject is AssemblyDocument assembly)
                {
                    if (AsmDocument != null && assembly == AsmDocument)
                    {
                        ClosingExporter();
                    }
                }
            }

            handlingCode = HandlingCodeEnum.kEventNotHandled;
        }

        /// <summary>
        /// Checks to make sure that you are in an assembly document and then readies for environment changing
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="environmentState"></param>
        /// <param name="beforeOrAfter"></param>
        /// <param name="context"></param>
        /// <param name="handlingCode"></param>
        private void UIEvents_OnEnvironmentChange(Environment environment,
            EnvironmentStateEnum environmentState, EventTimingEnum beforeOrAfter, NameValueMap context,
            out HandlingCodeEnum handlingCode)
        {
            if (environment.Equals(exporterEnv) && beforeOrAfter == EventTimingEnum.kBefore)
            {
                if (environmentState == EnvironmentStateEnum.kActivateEnvironmentState)
                {
                    // User may not open documents other than assemblies
                    if (!(context.Item["Document"] is AssemblyDocument assembly))
                    {
                        MessageBox.Show("Only assemblies can be used with the robot exporter.",
                            "Invalid Document", MessageBoxButtons.OK);
                        exporterBlocked = true;

                        // Quit the exporter
                        if (context.Item["Document"] is DrawingDocument drawing)
                            InventorUtils.ForceQuitExporter(drawing);
                        else if (context.Item["Document"] is PartDocument part)
                            InventorUtils.ForceQuitExporter(part);
                        else if (context.Item["Document"] is PresentationDocument presentation) InventorUtils.ForceQuitExporter(presentation);
                    }
                    // User may not open multiple documents in the exporter
                    else if (environmentEnabled)
                    {
                        MessageBox.Show("The exporter may only be used in one assembly at a time. " +
                                        "Please finish using the exporter in \"" + AsmDocument.DisplayName +
                                        "\" to continue.",
                            "Too Many Assemblies", MessageBoxButtons.OK);
                        exporterBlocked = true;
                        InventorUtils.ForceQuitExporter(assembly);
                    }
                    else
                        OpeningExporter();
                }
                else if (environmentState == EnvironmentStateEnum.kTerminateEnvironmentState && environmentEnabled)
                {
                    if (exporterBlocked)
                        exporterBlocked = false;
                    else
                        ClosingExporter();
                }
            }

            handlingCode = HandlingCodeEnum.kEventNotHandled;
        }

        /// <summary>
        /// Opens the help page on bxd.autodesk.com. This is the callback used for all OnHelp events.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="handlingCode"></param>
        private void _OnHelp(NameValueMap context, out HandlingCodeEnum handlingCode)
        {
            Process.Start("http://bxd.autodesk.com/synthesis/tutorials-robot.html");
            handlingCode = HandlingCodeEnum.kEventHandled;
        }

        /// <summary>
        /// Called when the user presses 'OK' in the settings menu
        /// </summary>
        /// <param name="child"></param>
        /// <param name="useFancyColors"></param>
        /// <param name="saveLocation"></param>
        private void ExporterSettings_SettingsChanged(Color child, bool useFancyColors,
            string saveLocation, bool openSynthesis, string fieldLocation, string defaultRobotCompetition, bool useAnalytics)
        {
            HighlightManager.SetJointHighlightColor(child);
            AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "Exporter Settings");
            //Update Application
            Settings.Default.ExportToField = openSynthesis;
            Settings.Default.SelectedField = fieldLocation;
            Settings.Default.ChildColor = child;
            Settings.Default.FancyColors = useFancyColors;
            Settings.Default.SaveLocation = saveLocation;
            Settings.Default.DefaultRobotCompetition = defaultRobotCompetition;
            Settings.Default.UseAnalytics = useAnalytics;
            Settings.Default.ConfigVersion = 3; // Update this config version number when changes are made to the exporter which require settings to be reset or changed when the exporter starts
            Settings.Default.Save();
        }


        /// <summary>
        /// Initializes all of the <see cref="Managers.RobotDataManager"/> settings to the proper values. Should be called once in the Activate class
        /// </summary>
        private static void LoadSettings()
        {
            // Old configurations get overriden (version numbers below 1)
            if (Settings.Default.SaveLocation == "" || Settings.Default.SaveLocation == "firstRun" || Settings.Default.ConfigVersion < 2)
                Settings.Default.SaveLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Synthesis\Robots";

            if (Settings.Default.ConfigVersion < 3)
            {
                RobotDataManager.PluginSettings = ExporterSettingsForm.Values = new ExporterSettingsForm.PluginSettingsValues
                {
                    InventorChildColor = Settings.Default.ChildColor,
                    GeneralSaveLocation = Settings.Default.SaveLocation,
                    GeneralUseFancyColors = Settings.Default.FancyColors,
                    OpenSynthesis = Settings.Default.ExportToField,
                    FieldName = Settings.Default.SelectedField,
                    DefaultRobotCompetition = "GENERIC",
                    UseAnalytics = true
                };
            }
            else
            {
                RobotDataManager.PluginSettings = ExporterSettingsForm.Values = new ExporterSettingsForm.PluginSettingsValues
                {
                    InventorChildColor = Settings.Default.ChildColor,
                    GeneralSaveLocation = Settings.Default.SaveLocation,
                    GeneralUseFancyColors = Settings.Default.FancyColors,
                    OpenSynthesis = Settings.Default.ExportToField,
                    FieldName = Settings.Default.SelectedField,
                    DefaultRobotCompetition = Settings.Default.DefaultRobotCompetition,
                    UseAnalytics = Settings.Default.UseAnalytics
                };
            }
        }
    }
}