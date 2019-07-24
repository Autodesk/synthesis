using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using BxDRobotExporter.ControlGUI;
using BxDRobotExporter.GUI.Editors;
using BxDRobotExporter.GUI.Editors.AdvancedJointEditor;
using BxDRobotExporter.GUI.Editors.JointEditor;
using BxDRobotExporter.Messages;
using BxDRobotExporter.Properties;
using Inventor;

namespace BxDRobotExporter
{
    /// <summary>
    /// This is where the magic happens. All top-level event handling, UI creation, and inventor communication is handled here.
    /// </summary>
    [Guid("0c9a07ad-2768-4a62-950a-b5e33b88e4a3")]
    public class RobotExporterAddInServer : ApplicationAddInServer
    {
        public static RobotExporterAddInServer Instance { get; set; }

        public bool PendingChanges
        {
            get
            {
                if (InventorUtils.Gui.SkeletonBase == null)
                    return false;
                else
                    return pendingChanges;
            }
            set
            {
//                if (QuitButton != null)
//                {
//                    QuitButton.Enabled = value; // Disable save button if changes have been saved
//                }
                pendingChanges = value;
            }
        }

        private bool pendingChanges = false;

        public Inventor.Application MainApplication;

        public AssemblyDocument AsmDocument;
        private List<ComponentOccurrence> disabledAssemblyOccurences;
        private Inventor.Environment exporterEnv;
        private bool environmentEnabled = false;

        //Makes sure that the application doesn't create a bunch of dockable windows. Nobody wants that crap.
        private bool hiddenExporter = false;

        //Ribbon Pannels
        private RibbonPanel driveTrainPanel;
        private RibbonPanel jointPanel;
        private RibbonPanel checklistPanel;
        private RibbonPanel pluginPanel;
        private RibbonPanel exitPanel;

        //Standalone Buttons
        private ButtonDefinition driveTrainTypeButton;
        private ButtonDefinition drivetrainWeightButton;
        private ButtonDefinition wheelAssignmentButton;

        private ButtonDefinition advancedEditJointButton;
        private ButtonDefinition editJointButton;

        private ButtonDefinition guideButton;
        private ButtonDefinition dofButton;
        private ButtonDefinition settingsButton;

        //        ButtonDefinition QuitButton;
        private ButtonDefinition exportButton;


        //Highlighting
        public HighlightSet ChildHighlight;
        private HighlightSet wheelHighlight;

        /// <summary>
        /// Called when the <see cref="RobotExporterAddInServer"/> is being loaded
        /// </summary>
        /// <param name="addInSiteObject"></param>
        /// <param name="firstTime"></param>
        public void Activate(ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            MainApplication =
                addInSiteObject
                    .Application; //Gets the application object, which is used in many different ways throughout this whole process
            string clientId = "{0c9a07ad-2768-4a62-950a-b5e33b88e4a3}";
            AnalyticsUtils.SetUser(MainApplication.UserName);
            AnalyticsUtils.LogPage("Inventor");
            InventorUtils.LoadSettings();

            stdole.IPictureDisp editJointIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.JointEditor32)); //these are still here at request of QA
            stdole.IPictureDisp editJointIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.JointEditor32));

            stdole.IPictureDisp drivetrainTypeIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.DrivetrainType32)); //these are still here at request of QA
            stdole.IPictureDisp drivetrainTypeIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.DrivetrainType32));

            stdole.IPictureDisp guideIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.Guide32)); //these are still here at request of QA
            stdole.IPictureDisp guideIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.Guide32));

            stdole.IPictureDisp drivetrainWeightIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.RobotWeight32));
            stdole.IPictureDisp drivetrainWeightIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.RobotWeight32));

            stdole.IPictureDisp synthesisLogoSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.SynthesisLogo16));
            stdole.IPictureDisp synthesisLogoLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.SynthesisLogo32));

            stdole.IPictureDisp gearLogoSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.Gears16));
            stdole.IPictureDisp gearLogoLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.Gears32));

            Environments environments = MainApplication.UserInterfaceManager.Environments;
            exporterEnv = environments.Add("Robot Export", "BxD:RobotExporter:Environment", null, synthesisLogoSmall,
                synthesisLogoLarge);

            Ribbon assemblyRibbon = MainApplication.UserInterfaceManager.Ribbons["Assembly"];
            RibbonTab exporterTab = assemblyRibbon.RibbonTabs.Add("Robot Export", "BxD:RobotExporter:RobotExporterTab",
                clientId, "", false, true);

            ControlDefinitions controlDefs = MainApplication.CommandManager.ControlDefinitions;

            driveTrainPanel =
                exporterTab.RibbonPanels.Add("Drive Train Setup", "BxD:RobotExporter:DriveTrainPanel", clientId);
            jointPanel = exporterTab.RibbonPanels.Add("Joint Setup", "BxD:RobotExporter:JointPanel", clientId);
            checklistPanel =
                exporterTab.RibbonPanels.Add("Robot Setup Checklist", "BxD:RobotExporter:ChecklistPanel", clientId);
            pluginPanel =
                exporterTab.RibbonPanels.Add("Plugin", "BxD:RobotExporter:PluginSettings", clientId);
            exitPanel = exporterTab.RibbonPanels.Add("Finish", "BxD:RobotExporter:ExitPanel", clientId);

            // Reset positioning of panels
            jointPanel.Reposition("BxD:RobotExporter:DriveTrainPanel", false);
            checklistPanel.Reposition("BxD:RobotExporter:JointPanel", false);
            exitPanel.Reposition("BxD:RobotExporter:ChecklistPanel", false);

            // TODO: Delete these region things

            //Drive Train panel buttons
            driveTrainTypeButton = controlDefs.AddButtonDefinition("Drive Train\nType",
                "BxD:RobotExporter:SetDriveTrainType", CommandTypesEnum.kNonShapeEditCmdType, clientId, null,
                "Select the drivetrain type (tank, H-drive, or mecanum).", drivetrainTypeIconSmall, drivetrainTypeIconLarge);
            driveTrainTypeButton.OnExecute += DriveTrainType_OnExecute;
            driveTrainTypeButton.OnHelp += _OnHelp;
            driveTrainPanel.CommandControls.AddButton(driveTrainTypeButton, true);

            drivetrainWeightButton = controlDefs.AddButtonDefinition("Drive Train\nWeight",
                "BxD:RobotExporter:SetDriveTrainWeight", CommandTypesEnum.kNonShapeEditCmdType, clientId, null,
                "Assign the weight of the drivetrain.", drivetrainWeightIconSmall, drivetrainWeightIconLarge);
            drivetrainWeightButton.OnExecute += SetWeight_OnExecute;
            drivetrainWeightButton.OnHelp += _OnHelp;
            driveTrainPanel.CommandControls.AddButton(drivetrainWeightButton, true);

            // Joint panel buttons
            advancedEditJointButton = controlDefs.AddButtonDefinition("Advanced Editor", "BxD:RobotExporter:AdvancedEditJoint",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "Joint editor for advanced users.", editJointIconSmall, editJointIconLarge);
            advancedEditJointButton.OnExecute += AdvancedEditJoint_OnExecute;
            advancedEditJointButton.OnHelp += _OnHelp;
            jointPanel.SlideoutControls.AddButton(advancedEditJointButton);

            editJointButton = controlDefs.AddButtonDefinition("Edit Joints", "BxD:RobotExporter:EditJoint",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "Edit existing joints.", editJointIconSmall, editJointIconLarge);
            editJointButton.OnExecute += EditJoint_OnExecute;
            editJointButton.OnHelp += _OnHelp;
            jointPanel.CommandControls.AddButton(editJointButton, true);

            // ChecklistPanel buttons
            guideButton = controlDefs.AddButtonDefinition("Toggle Robot\nExport Guide", "BxD:RobotExporter:Guide",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null,
                "View a checklist of all tasks necessary prior to export.", guideIconSmall, guideIconLarge);
            guideButton.OnExecute += delegate {InventorUtils.EmbededGuidePane.Visible = !InventorUtils.EmbededGuidePane.Visible; };
            guideButton.OnHelp += _OnHelp;
            checklistPanel.CommandControls.AddButton(guideButton, true);

            dofButton = controlDefs.AddButtonDefinition("Toggle Degrees\nof Freedom View", "BxD:RobotExporter:DOF",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "View degrees of freedom.", guideIconSmall, guideIconLarge);
            dofButton.OnExecute += DOF_OnExecute;
            dofButton.OnHelp += _OnHelp;
            checklistPanel.CommandControls.AddButton(dofButton, true);

            settingsButton = controlDefs.AddButtonDefinition("Plugin Settings", "BxD:RobotExporter:Settings",
                CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "View degrees of freedom.", gearLogoSmall, gearLogoLarge);
            settingsButton.OnExecute += Settings_OnExecute;
            settingsButton.OnHelp += _OnHelp;
            pluginPanel.CommandControls.AddButton(settingsButton, true);

            exporterEnv.DefaultRibbonTab = "BxD:RobotExporter:RobotExporterTab";
            MainApplication.UserInterfaceManager.ParallelEnvironments.Add(exporterEnv);
            exporterEnv.DisabledCommandList.Add(
                MainApplication.CommandManager.ControlDefinitions["BxD:RobotExporter:Environment"]);

            MainApplication.UserInterfaceManager.UserInterfaceEvents.OnEnvironmentChange +=
                UIEvents_OnEnvironmentChange;
            MainApplication.ApplicationEvents.OnActivateDocument += ApplicationEvents_OnActivateDocument;
            MainApplication.ApplicationEvents.OnDeactivateDocument += ApplicationEvents_OnDeactivateDocument;
            MainApplication.ApplicationEvents.OnCloseDocument += ApplicationEvents_OnCloseDocument;
            LegacyEvents.RobotModified += new Action(() => { PendingChanges = true; });

            Instance = this;
        }
        private void Settings_OnExecute(NameValueMap context)
        {
            ExporterSettingsForm settingsForm = new ExporterSettingsForm();
            settingsForm.ShowDialog();
        }
        private void DriveTrainType_OnExecute(NameValueMap context)
        {
            DrivetrainTypeForm drivetrainTypeForm = new DrivetrainTypeForm();
            drivetrainTypeForm.ShowDialog();
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

        public void ExecuteCommand(int commandId)
        {
            // Note:this method is now obsolete, you should use the 
            // ControlDefinition functionality for implementing commands.
        }

        public object Automation
        {
            // This property is provided to allow the AddIn to expose an API 
            // of its own to other programs. Typically, this  would be done by
            // implementing the AddIn's API interface in a class and returning 
            // that class object through this property.

            get
            {
                // TODO: Add ApplicationAddInServer.Automation getter implementation
                return null;
            }
        }

        /// <summary>
        /// Gets the assembly document and makes the <see cref="DockableWindows"/>
        /// </summary>
        private void OpeningExporter()
        {
            AnalyticsUtils.StartSession();
            //Gets the assembly document and creates dockable windows
            AsmDocument = (AssemblyDocument) MainApplication.ActiveDocument;
            InventorUtils.CreateChildDialog();
            blueHighlightSet = AsmDocument.CreateHighlightSet();
            greenHighlightSet = AsmDocument.CreateHighlightSet();
            redHighlightSet = AsmDocument.CreateHighlightSet();
            blueHighlightSet.Color = InventorUtils.GetInventorColor(System.Drawing.Color.DodgerBlue);
            greenHighlightSet.Color = InventorUtils.GetInventorColor(System.Drawing.Color.LawnGreen);
            redHighlightSet.Color = InventorUtils.GetInventorColor(System.Drawing.Color.Red);
            ChildHighlight = AsmDocument.CreateHighlightSet();
            ChildHighlight.Color = InventorUtils.GetInventorColor(SynthesisGui.PluginSettings.InventorChildColor);
            wheelHighlight = AsmDocument.CreateHighlightSet();
            wheelHighlight.Color = InventorUtils.GetInventorColor(System.Drawing.Color.Green);

            //Sets up events for selecting and deselecting parts in inventor
            ExporterSettingsForm.PluginSettingsValues.SettingsChanged += ExporterSettings_SettingsChanged;

            environmentEnabled = true;

            // Load robot skeleton and prepare UI
            if (!InventorUtils.Gui.LoadRobotSkeleton())
            {
                ForceQuitExporter(AsmDocument);
                return;
            }

            disabledAssemblyOccurences = new List<ComponentOccurrence>();
            disabledAssemblyOccurences.AddRange(DisableUnconnectedComponents(AsmDocument));
            // If fails to load existing data, restart wizard
            InventorUtils.Gui.LoadRobotData(AsmDocument);
            
            InventorUtils.CreateDockableWindows(MainApplication);
            // Hide non-jointed components;

            // Reload panels in UI
            jointForm = new JointForm();
            InventorUtils.HideAdvancedJointEditor();
        }

        /// <summary>
        /// Disposes of some COM objects and exits the environment
        /// </summary>
        private void ClosingExporter()
        {
            AnalyticsUtils.EndSession();
            SaveRobotData(null);

            DialogResult exportResult = MessageBox.Show(
                "The robot configuration has been saved to your assembly document.\nWould you like to export your robot to Synthesis?",
                "Robot Configuration Complete",
                MessageBoxButtons.YesNo);

            if (exportResult == DialogResult.Yes)
            {
                PromptExportToSynthesis();
            }

            // Re-enable disabled components
            if (disabledAssemblyOccurences != null)
                EnableComponents(disabledAssemblyOccurences);
            disabledAssemblyOccurences = null;

            // Close add-in
            InventorUtils.DisposeDockableWindows();
            ForceQuitExporter(AsmDocument);

            // Dispose of document
            if (AsmDocument != null)
                Marshal.ReleaseComObject(AsmDocument);
            AsmDocument = null;

            environmentEnabled = false;
        }

        /// <summary>
        /// Causes the exporter to close.
        /// </summary>
        /// <param name="suppressClosingEvent">Whether or not the exporter closing handler should be suppressed from being called.</param>
        private async void ForceQuitExporter(AssemblyDocument document)
        {
            await Task.Delay(1); // Delay is needed so that environment is closed after it has finished opening
            document.EnvironmentManager.SetCurrentEnvironment(document.EnvironmentManager.EditObjectEnvironment);
        }

        /// <summary>
        /// Causes the exporter to close.
        /// </summary>
        /// <param name="suppressClosingEvent">Whether or not the exporter closing handler should be suppressed from being called.</param>
        private async void ForceQuitExporter(DrawingDocument document)
        {
            await Task.Delay(1); // Delay is needed so that environment is closed after it has finished opening
            document.EnvironmentManager.SetCurrentEnvironment(document.EnvironmentManager.EditObjectEnvironment);
        }

        /// <summary>
        /// Causes the exporter to close.
        /// </summary>
        /// <param name="suppressClosingEvent">Whether or not the exporter closing handler should be suppressed from being called.</param>
        private async void ForceQuitExporter(PartDocument document)
        {
            await Task.Delay(1); // Delay is needed so that environment is closed after it has finished opening
            document.EnvironmentManager.SetCurrentEnvironment(document.EnvironmentManager.EditObjectEnvironment);
        }

        /// <summary>
        /// Causes the exporter to close.
        /// </summary>
        /// <param name="suppressClosingEvent">Whether or not the exporter closing handler should be suppressed from being called.</param>
        private async void ForceQuitExporter(PresentationDocument document)
        {
            await Task.Delay(1); // Delay is needed so that environment is closed after it has finished opening
            document.EnvironmentManager.SetCurrentEnvironment(document.EnvironmentManager.EditObjectEnvironment);
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
                    InventorUtils.HideAdvancedJointEditor();
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
            } else if (beforeOrAfter == EventTimingEnum.kAfter)
            {
                if (Properties.Settings.Default.ShowFirstLaunchInfo)
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
        private void UIEvents_OnEnvironmentChange(Inventor.Environment environment,
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
                            ForceQuitExporter(drawing);
                        else if (context.Item["Document"] is PartDocument part)
                            ForceQuitExporter(part);
                        else if (context.Item["Document"] is PresentationDocument presentation)
                            ForceQuitExporter(presentation);
                    }
                    // User may not open multiple documents in the exporter
                    else if (environmentEnabled)
                    {
                        MessageBox.Show("The exporter may only be used in one assembly at a time. " +
                                        "Please finish using the exporter in \"" + AsmDocument.DisplayName +
                                        "\" to continue.",
                            "Too Many Assemblies", MessageBoxButtons.OK);
                        exporterBlocked = true;
                        ForceQuitExporter(assembly);
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

        private bool exporterBlocked = false;
        private JointForm jointForm;
        private HighlightSet blueHighlightSet;
        private HighlightSet greenHighlightSet;
        private HighlightSet redHighlightSet;

        private bool displayDof = false;
        public AdvancedJointEditorUserControl AdvancedAdvancedJointEditor;

        public void DOF_OnExecute(NameValueMap context)
        {

            AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "DOF", 0);

            displayDof = !displayDof;
            InventorUtils.EmbededKeyPane.Visible = displayDof;

            if (displayDof)
            {
                if (InventorUtils.Gui.SkeletonBase == null && !InventorUtils.Gui.LoadRobotSkeleton())
                    return;

                var rootNodes = new List<RigidNode_Base> {InventorUtils.Gui.SkeletonBase};
                var jointedNodes = new List<RigidNode_Base>();
                var problemNodes = new List<RigidNode_Base>();

                foreach (RigidNode_Base node in InventorUtils.Gui.SkeletonBase.ListAllNodes())
                {
                    if (node == InventorUtils.Gui.SkeletonBase) // Base node is already dealt with TODO: add ListChildren() to RigidNode_Base
                    {
                        continue;
                    }
                    if (node.GetSkeletalJoint() == null || node.GetSkeletalJoint().cDriver == null) // TODO: Figure out how to identify nodes that aren't set up (highlight red)
                    {
                        problemNodes.Add(node);
                    }
                    else
                    {
                        jointedNodes.Add(node);
                    }
                }

                ChildHighlight.Clear();
                CreateHighlightSet(rootNodes, blueHighlightSet);
                CreateHighlightSet(jointedNodes, greenHighlightSet);
                CreateHighlightSet(problemNodes, redHighlightSet);
            }
            else
            {
                ClearDofHighlight();

            }
        }

        public void ClearDofHighlight()
        {
            blueHighlightSet.Clear();
            greenHighlightSet.Clear();
            redHighlightSet.Clear();
            displayDof = false;
        }

        private void CreateHighlightSet(List<RigidNode_Base> nodes, HighlightSet highlightSet)
        {
            highlightSet.Clear();
            foreach (var componentOccurrence in InventorUtils.GetComponentOccurrencesFromNodes(nodes))
            {
                highlightSet.AddItem(componentOccurrence);
            }
        }

        public void EditJoint_OnExecute(NameValueMap context)
        {
            AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "Edit Joint", 0);
            if (jointForm.Visible)
            {
                jointForm.Hide();
            }
            else
            {
                if (InventorUtils.Gui.SkeletonBase == null && !InventorUtils.Gui.LoadRobotSkeleton())
                    return;
//                Utilities.HideAdvancedJointEditor();
                jointForm.OnShowButtonClick();
                jointForm.ShowDialog();
                AdvancedAdvancedJointEditor.SetSkeleton(SynthesisGui.Instance.SkeletonBase);
            }
        }

        private void AdvancedEditJoint_OnExecute(NameValueMap context)
        {
            AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "Advanced Edit Joint", 0);
            InventorUtils.ToggleAdvancedJointEditor();
            if (InventorUtils.IsAdvancedJointEditorVisible())
            {
                jointForm.Hide();
            }
        }

        /// <summary>
        /// Saves the active robot to the active directory
        /// </summary>
        /// <param name="context"></param>
        private void SaveRobotData(NameValueMap context)
        {
           
            if (InventorUtils.Gui.SaveRobotData())
                PendingChanges = false;
        }

        /// <summary>
        /// Saves the active robot to the active directory
        /// </summary>
        /// <param name="context"></param>
        private void ExportButtonOnExecute(NameValueMap context)
        {
            PromptExportToSynthesis();
        }

        public void PromptExportToSynthesis()
        {
            if (InventorUtils.Gui.PromptExportSettings())
                if (InventorUtils.Gui.ExportRobot() && InventorUtils.Gui.RMeta.FieldName != null)

                    InventorUtils.Gui.OpenSynthesis();
        }

        //Settings
        /// <summary>
        /// Opens the <see cref="DrivetrainWeightForm"/> form to allow the user to set the weight of their robot.
        /// </summary>
        /// <param name="context"></param>
        private void SetWeight_OnExecute(NameValueMap context)
        {
            AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "Set Weight", 0);
            if (InventorUtils.Gui.PromptRobotWeight())
                PendingChanges = true;
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
        private void ExporterSettings_SettingsChanged(System.Drawing.Color child, bool useFancyColors,
            string saveLocation, bool openSynthesis, string fieldLocation, string defaultRobotCompetition, bool useAnalytics)
        {
            ChildHighlight.Color = InventorUtils.GetInventorColor(child);
            AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "Exporter Settings", 0);
            //Update Application
            Properties.Settings.Default.ExportToField = openSynthesis;
            Properties.Settings.Default.SelectedField = fieldLocation;
            Properties.Settings.Default.ChildColor = child;
            Properties.Settings.Default.FancyColors = useFancyColors;
            Properties.Settings.Default.SaveLocation = saveLocation;
            Properties.Settings.Default.DefaultRobotCompetition = defaultRobotCompetition;
            Properties.Settings.Default.UseAnalytics = useAnalytics;
            Properties.Settings.Default.ConfigVersion =
                3; // Update this config version number when changes are made to the exporter which require settings to be reset or changed when the exporter starts
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Disables all components in a document that are not connected to another component by a joint.
        /// </summary>
        /// <param name="asmDocument">Document to traverse.</param>
        /// <returns>List of disabled components.</returns>
        private List<ComponentOccurrence> DisableUnconnectedComponents(AssemblyDocument asmDocument)
        {
            // Find all components in the assembly that are connected to a joint
            List<ComponentOccurrence> jointedAssemblyOccurences = new List<ComponentOccurrence>();
            foreach (AssemblyJoint joint in asmDocument.ComponentDefinition.Joints)
            {
                if (!joint.Definition.JointType.Equals(AssemblyJointTypeEnum.kRigidJointType))
                {
                    jointedAssemblyOccurences.Add(joint.AffectedOccurrenceOne);
                    jointedAssemblyOccurences.Add(joint.AffectedOccurrenceTwo);
                }
            }

            // Hide any components not associated with a joint
            List<ComponentOccurrence> disabledAssemblyOccurences = new List<ComponentOccurrence>();
            foreach (ComponentOccurrence c in asmDocument.ComponentDefinition.Occurrences)
            {
                if (!jointedAssemblyOccurences.Contains(c) || c.Grounded)
                {
                    try
                    {
                        //accounts for components that can't be disabled
                        disabledAssemblyOccurences.Add(c);
                        c.Enabled = false;
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return disabledAssemblyOccurences;
        }

        /// <summary>
        /// Enables all components in a list.
        /// </summary>
        /// <param name="components">Components to enable.</param>
        private void EnableComponents(List<ComponentOccurrence> components)
        {
            foreach (ComponentOccurrence c in components)
            {
                try
                {
                    //accounts for components that can't be disabled
                    c.Enabled = true;
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Checks if a baseNode matches up with the assembly. Passed as a <see cref="ValidationAction"/> to
        /// </summary>
        /// <param name="baseNode"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool ValidateAssembly(RigidNode_Base baseNode, out string message)
        {
            int validationCount = 0;
            int failedCount = 0;
            List<RigidNode_Base> nodes = baseNode.ListAllNodes();
            foreach (RigidNode_Base node in nodes)
            {
                bool failedValidation = false;
                foreach (string componentName in node.ModelFullID.Split(new string[] {"-_-"},
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!CheckForOccurrence(componentName))
                    {
                        failedCount++;
                        failedValidation = true;
                    }
                }

                if (!failedValidation)
                {
                    validationCount++;
                }
            }

            if (validationCount == nodes.Count)
            {
                message = string.Format("The assembly validated successfully. {0} / {1} nodes checked out.",
                    validationCount, nodes.Count);
                return true;
            }
            else
            {
                message = string.Format(
                    "The assembly failed to validate. {0} / {1} nodes checked out. {2} parts/assemblies were not found.",
                    validationCount, nodes.Count, failedCount);
                return false;
            }
        }

        /// <summary>
        /// Checks to see if a <see cref="ComponentOccurrence"/> of the specified name exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool CheckForOccurrence(string name)
        {
            foreach (ComponentOccurrence component in AsmDocument.ComponentDefinition.Occurrences)
            {
                if (component.Name == name)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Adds a <see cref="ComponentOccurrence"/> with the specified name to the specified <see cref="HighlightSet"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="jointNodeType"></param>
        private void SelectNode(string name)
        {
            foreach (ComponentOccurrence occ in AsmDocument.ComponentDefinition.Occurrences)
            {
                if (occ.Name == name)
                {
                    ChildHighlight.AddItem(occ);
                }
            }
        }

        /// <summary>
        /// Adds the specified <see cref="ComponentOccurrence"/> with the <see cref="HighlightSet"/> specified in <paramref name="jointNodeType"/>
        /// </summary>
        /// <param name="occurrence"></param>
        /// <param name="jointNodeType"></param>
        private void SelectNode(ComponentOccurrence occurrence)
        {
            ChildHighlight.AddItem(occurrence);
        }

        /// <summary>
        /// Sets the tooltip of a <see cref="ButtonDefinition"/>
        /// </summary>
        /// <param name="button">The <see cref="ButtonDefinition"/> the tool tip is being applied to</param>
        /// <param name="description">The description of the command which the <paramref name="button"/> executes</param>
        /// <param name="expandedDescription">The expanded description of the command which appears after hovering the cursor over the button for a few seconds</param>
        /// <param name="picture">The image that appears along side the <paramref name="expandedDescription"/></param>
        /// <param name="title">The bolded title appearing at the top of the tooltip</param>
        public void ToolTip(ButtonDefinition button, string title, string description,
            string expandedDescription = null, stdole.IPictureDisp picture = null)
        {
            if (description != null)
                button.ProgressiveToolTip.Description = description;
            if (expandedDescription != null)
            {
                button.ProgressiveToolTip.ExpandedDescription = expandedDescription;
                button.ProgressiveToolTip.IsProgressive = true;
            }

            if (picture != null)
            {
                button.ProgressiveToolTip.Image = picture;
                button.ProgressiveToolTip.IsProgressive = true;
            }

            button.ProgressiveToolTip.Title = title;
        }

        /// <summary>
        /// <see cref="ViewDirection.X"/>, <see cref="ViewDirection.Y"/>, <see cref="ViewDirection.Z"/> position the camera right of, in front of, and above the target relative to the front of the robot.
        /// Bitwise OR <see cref="ViewDirection.X"/>, <see cref="ViewDirection.Y"/>, or <see cref="ViewDirection.Z"/> with <see cref="ViewDirection.Negative"/> to invert these (make them left of, behind or above the robot).
        /// </summary>
        public enum ViewDirection : byte
        {
            /// <summary>
            /// Positions the camera to the right of the robot.
            /// </summary>
            X = 0b00000001,

            /// <summary>
            /// Positions the camera above the robot.
            /// </summary>
            Y = 0b00000010,

            /// <summary>
            /// Positions the robot in front of the robot.
            /// </summary>
            Z = 0b00000100,
            Negative = 0b00001000
        }
    }
}