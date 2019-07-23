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
    public class StandardAddInServer : ApplicationAddInServer
    {
        #region Variables 

        public static StandardAddInServer Instance { get; set; }

        public bool PendingChanges
        {
            get
            {
                if (InventorUtils.GUI.SkeletonBase == null)
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
        List<ComponentOccurrence> disabledAssemblyOccurences;
        Inventor.Environment ExporterEnv;
        bool EnvironmentEnabled = false;

        //Makes sure that the application doesn't create a bunch of dockable windows. Nobody wants that crap.
        bool HiddenExporter = false;

        //Ribbon Pannels
        RibbonPanel DriveTrainPanel;
        RibbonPanel JointPanel;
        RibbonPanel ChecklistPanel;
        RibbonPanel PluginPanel;
        RibbonPanel ExitPanel;

        //Standalone Buttons
        ButtonDefinition DriveTrainTypeButton;
        ButtonDefinition DrivetrainWeightButton;
        ButtonDefinition WheelAssignmentButton;

        ButtonDefinition AdvancedEditJointButton;
        ButtonDefinition EditJointButton;

        ButtonDefinition PreCheckButton;
        ButtonDefinition DOFButton;
        ButtonDefinition SettingsButton;

        //        ButtonDefinition QuitButton;
        ButtonDefinition ExportButton;


        //Highlighting
        public HighlightSet ChildHighlight;
        HighlightSet WheelHighlight;

        #endregion

        #region ApplicationAddInServer Methods

        /// <summary>
        /// Called when the <see cref="StandardAddInServer"/> is being loaded
        /// </summary>
        /// <param name="AddInSiteObject"></param>
        /// <param name="FirstTime"></param>
        public void Activate(ApplicationAddInSite AddInSiteObject, bool FirstTime)
        {
            MainApplication =
                AddInSiteObject
                    .Application; //Gets the application object, which is used in many different ways throughout this whole process
            string ClientID = "{0c9a07ad-2768-4a62-950a-b5e33b88e4a3}";
            AnalyticsUtils.SetUser(MainApplication.UserName);
            AnalyticsUtils.LogPage("Inventor");
            InventorUtils.LoadSettings();

            #region Add Parallel Environment

            #region Load Images

            stdole.IPictureDisp EditJointIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.JointEditor32)); //these are still here at request of QA
            stdole.IPictureDisp EditJointIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.JointEditor32));

            stdole.IPictureDisp DrivetrainTypeIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.DrivetrainType32)); //these are still here at request of QA
            stdole.IPictureDisp DrivetrainTypeIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.DrivetrainType32));

            stdole.IPictureDisp PrecheckIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.Guide32)); //these are still here at request of QA
            stdole.IPictureDisp PrecheckIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.Guide32));

            stdole.IPictureDisp DrivetrainWeightIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.RobotWeight32));
            stdole.IPictureDisp DrivetrainWeightIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.RobotWeight32));

            stdole.IPictureDisp SynthesisLogoSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.SynthesisLogo16));
            stdole.IPictureDisp SynthesisLogoLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.SynthesisLogo32));

            stdole.IPictureDisp GearLogoSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.Gears16));
            stdole.IPictureDisp GearLogoLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resources.Gears32));

            #endregion

            #region UI Creation

            #region Setup New Environment and Ribbon

            Environments environments = MainApplication.UserInterfaceManager.Environments;
            ExporterEnv = environments.Add("Robot Export", "BxD:RobotExporter:Environment", null, SynthesisLogoSmall,
                SynthesisLogoLarge);

            Ribbon assemblyRibbon = MainApplication.UserInterfaceManager.Ribbons["Assembly"];
            RibbonTab ExporterTab = assemblyRibbon.RibbonTabs.Add("Robot Export", "BxD:RobotExporter:RobotExporterTab",
                ClientID, "", false, true);

            ControlDefinitions ControlDefs = MainApplication.CommandManager.ControlDefinitions;

            DriveTrainPanel =
                ExporterTab.RibbonPanels.Add("Drive Train Setup", "BxD:RobotExporter:DriveTrainPanel", ClientID);
            JointPanel = ExporterTab.RibbonPanels.Add("Joint Setup", "BxD:RobotExporter:JointPanel", ClientID);
            ChecklistPanel =
                ExporterTab.RibbonPanels.Add("Robot Setup Checklist", "BxD:RobotExporter:ChecklistPanel", ClientID);
            PluginPanel =
                ExporterTab.RibbonPanels.Add("Plugin", "BxD:RobotExporter:PluginSettings", ClientID);
            ExitPanel = ExporterTab.RibbonPanels.Add("Finish", "BxD:RobotExporter:ExitPanel", ClientID);

            // Reset positioning of panels
            JointPanel.Reposition("BxD:RobotExporter:DriveTrainPanel", false);
            ChecklistPanel.Reposition("BxD:RobotExporter:JointPanel", false);
            ExitPanel.Reposition("BxD:RobotExporter:ChecklistPanel", false);

            #endregion

            #region Setup Buttons 
            // TODO: Delete these region things

            //Drive Train panel buttons
            DriveTrainTypeButton = ControlDefs.AddButtonDefinition("Drive Train\nType",
                "BxD:RobotExporter:SetDriveTrainType", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null,
                "Select the drivetrain type (tank, H-drive, or mecanum).", DrivetrainTypeIconSmall, DrivetrainTypeIconLarge);
            DriveTrainTypeButton.OnExecute += DriveTrainType_OnExecute;
            DriveTrainTypeButton.OnHelp += _OnHelp;
            DriveTrainPanel.CommandControls.AddButton(DriveTrainTypeButton, true);

            DrivetrainWeightButton = ControlDefs.AddButtonDefinition("Drive Train\nWeight",
                "BxD:RobotExporter:SetDriveTrainWeight", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null,
                "Assign the weight of the drivetrain.", DrivetrainWeightIconSmall, DrivetrainWeightIconLarge);
            DrivetrainWeightButton.OnExecute += SetWeight_OnExecute;
            DrivetrainWeightButton.OnHelp += _OnHelp;
            DriveTrainPanel.CommandControls.AddButton(DrivetrainWeightButton, true);

            // Joint panel buttons
            AdvancedEditJointButton = ControlDefs.AddButtonDefinition("Advanced Editor", "BxD:RobotExporter:AdvancedEditJoint",
                CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, "Joint editor for advanced users.", EditJointIconSmall, EditJointIconLarge);
            AdvancedEditJointButton.OnExecute += AdvancedEditJoint_OnExecute;
            AdvancedEditJointButton.OnHelp += _OnHelp;
            JointPanel.SlideoutControls.AddButton(AdvancedEditJointButton);

            EditJointButton = ControlDefs.AddButtonDefinition("Edit Joints", "BxD:RobotExporter:EditJoint",
                CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, "Edit existing joints.", EditJointIconSmall, EditJointIconLarge);
            EditJointButton.OnExecute += EditJoint_OnExecute;
            EditJointButton.OnHelp += _OnHelp;
            JointPanel.CommandControls.AddButton(EditJointButton, true);

            // ChecklistPanel buttons
            PreCheckButton = ControlDefs.AddButtonDefinition("Toggle Robot\nExport Guide", "BxD:RobotExporter:PreCheck",
                CommandTypesEnum.kNonShapeEditCmdType, ClientID, null,
                "View a checklist of all tasks necessary prior to export.", PrecheckIconSmall, PrecheckIconLarge);
            PreCheckButton.OnExecute += delegate {InventorUtils.EmbededPrecheckPane.Visible = !InventorUtils.EmbededPrecheckPane.Visible; };
            PreCheckButton.OnHelp += _OnHelp;
            ChecklistPanel.CommandControls.AddButton(PreCheckButton, true);

            DOFButton = ControlDefs.AddButtonDefinition("Toggle Degrees\nof Freedom View", "BxD:RobotExporter:DOF",
                CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, "View degrees of freedom.", PrecheckIconSmall, PrecheckIconLarge);
            DOFButton.OnExecute += DOF_OnExecute;
            DOFButton.OnHelp += _OnHelp;
            ChecklistPanel.CommandControls.AddButton(DOFButton, true);

            SettingsButton = ControlDefs.AddButtonDefinition("Plugin Settings", "BxD:RobotExporter:Settings",
                CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, "View degrees of freedom.", GearLogoSmall, GearLogoLarge);
            SettingsButton.OnExecute += Settings_OnExecute;
            SettingsButton.OnHelp += _OnHelp;
            PluginPanel.CommandControls.AddButton(SettingsButton, true);

            #endregion

            #endregion

            #region Final Environment Setup

            ExporterEnv.DefaultRibbonTab = "BxD:RobotExporter:RobotExporterTab";
            MainApplication.UserInterfaceManager.ParallelEnvironments.Add(ExporterEnv);
            ExporterEnv.DisabledCommandList.Add(
                MainApplication.CommandManager.ControlDefinitions["BxD:RobotExporter:Environment"]);

            #endregion

            #region Event Handler Assignment

            MainApplication.UserInterfaceManager.UserInterfaceEvents.OnEnvironmentChange +=
                UIEvents_OnEnvironmentChange;
            MainApplication.ApplicationEvents.OnActivateDocument += ApplicationEvents_OnActivateDocument;
            MainApplication.ApplicationEvents.OnDeactivateDocument += ApplicationEvents_OnDeactivateDocument;
            MainApplication.ApplicationEvents.OnCloseDocument += ApplicationEvents_OnCloseDocument;
            LegacyEvents.RobotModified += new Action(() => { PendingChanges = true; });

            #endregion

            #endregion

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
        /// Called when the <see cref="StandardAddInServer"/> is being unloaded
        /// </summary>
        public void Deactivate()
        {
            Marshal.ReleaseComObject(MainApplication);
            MainApplication = null;
            ExporterEnv.Delete();
           
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public void ExecuteCommand(int commandID)
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

        #endregion

        #region Environment Switching

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
            ChildHighlight.Color = InventorUtils.GetInventorColor(SynthesisGUI.PluginSettings.InventorChildColor);
            WheelHighlight = AsmDocument.CreateHighlightSet();
            WheelHighlight.Color = InventorUtils.GetInventorColor(System.Drawing.Color.Green);

            //Sets up events for selecting and deselecting parts in inventor
            ExporterSettingsForm.PluginSettingsValues.SettingsChanged += ExporterSettings_SettingsChanged;

            EnvironmentEnabled = true;

            // Load robot skeleton and prepare UI
            if (!InventorUtils.GUI.LoadRobotSkeleton())
            {
                ForceQuitExporter(AsmDocument);
                return;
            }

            disabledAssemblyOccurences = new List<ComponentOccurrence>();
            disabledAssemblyOccurences.AddRange(DisableUnconnectedComponents(AsmDocument));
            // If fails to load existing data, restart wizard
            InventorUtils.GUI.LoadRobotData(AsmDocument);
            
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

            EnvironmentEnabled = false;
        }

        #region Force Quit Functions

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

        #endregion

        #endregion

        #region Event Callbacks and Button Commands

        #region Application, Document, UI Event Handlers

        /// <summary>
        /// Makes the dockable windows invisible when the document switches. This avoids data loss. 
        /// Also re-enables the exporter in the document if it was disabled. This allows the user to use the exporter in that document at a later point.
        /// </summary>
        /// <param name="DocumentObject"></param>
        /// <param name="BeforeOrAfter"></param>
        /// <param name="Context"></param>
        /// <param name="HandlingCode"></param>
        private void ApplicationEvents_OnDeactivateDocument(_Document DocumentObject, EventTimingEnum BeforeOrAfter,
            NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {
            if (EnvironmentEnabled)
            {
                if (BeforeOrAfter == EventTimingEnum.kBefore)
                {
                    InventorUtils.HideAdvancedJointEditor();
                    HiddenExporter = true;
                }
            }

            HandlingCode = HandlingCodeEnum.kEventNotHandled;
        }

        /// <summary>
        /// Disables the environment button if you aren't in the assembly document that the exporter was originally opened in.
        /// </summary>
        /// <param name="DocumentObject"></param>
        /// <param name="BeforeOrAfter"></param>
        /// <param name="Context"></param>
        /// <param name="HandlingCode"></param>
        private void ApplicationEvents_OnActivateDocument(_Document DocumentObject, EventTimingEnum BeforeOrAfter,
            NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {
            if (BeforeOrAfter == EventTimingEnum.kBefore)
            {
                if (DocumentObject is AssemblyDocument assembly)
                {
                    if ((AsmDocument == null || assembly == AsmDocument) && HiddenExporter)
                    {
                        HiddenExporter = false;
                    }
                }
            } else if (BeforeOrAfter == EventTimingEnum.kAfter)
            {
                if (Properties.Settings.Default.ShowFirstLaunchInfo)
                {
                    var firstLaunchInfo = new FirstLaunchInfo();
                    firstLaunchInfo.ShowDialog();
                }
            }

            HandlingCode = HandlingCodeEnum.kEventNotHandled;
        }

        /// <summary>
        /// Called when the user closes a document. Used to release exporter when a document is closed.
        /// </summary>
        /// <param name="DocumentObject"></param>
        /// <param name="FullDocumentName"></param>
        /// <param name="BeforeOrAfter"></param>
        /// <param name="Context"></param>
        /// <param name="HandlingCode"></param>
        private void ApplicationEvents_OnCloseDocument(_Document DocumentObject, string FullDocumentName,
            EventTimingEnum BeforeOrAfter, NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {
            // Quit the exporter if the closing document has the exporter open
            if (BeforeOrAfter == EventTimingEnum.kBefore && EnvironmentEnabled)
            {
                if (DocumentObject is AssemblyDocument assembly)
                {
                    if (AsmDocument != null && assembly == AsmDocument)
                    {
                        ClosingExporter();
                    }
                }
            }

            HandlingCode = HandlingCodeEnum.kEventNotHandled;
        }

        /// <summary>
        /// Checks to make sure that you are in an assembly document and then readies for environment changing
        /// </summary>
        /// <param name="Environment"></param>
        /// <param name="EnvironmentState"></param>
        /// <param name="BeforeOrAfter"></param>
        /// <param name="Context"></param>
        /// <param name="HandlingCode"></param>
        private void UIEvents_OnEnvironmentChange(Inventor.Environment Environment,
            EnvironmentStateEnum EnvironmentState, EventTimingEnum BeforeOrAfter, NameValueMap Context,
            out HandlingCodeEnum HandlingCode)
        {
            if (Environment.Equals(ExporterEnv) && BeforeOrAfter == EventTimingEnum.kBefore)
            {
                if (EnvironmentState == EnvironmentStateEnum.kActivateEnvironmentState)
                {
                    // User may not open documents other than assemblies
                    if (!(Context.Item["Document"] is AssemblyDocument assembly))
                    {
                        MessageBox.Show("Only assemblies can be used with the robot exporter.",
                            "Invalid Document", MessageBoxButtons.OK);
                        exporterBlocked = true;

                        // Quit the exporter
                        if (Context.Item["Document"] is DrawingDocument drawing)
                            ForceQuitExporter(drawing);
                        else if (Context.Item["Document"] is PartDocument part)
                            ForceQuitExporter(part);
                        else if (Context.Item["Document"] is PresentationDocument presentation)
                            ForceQuitExporter(presentation);
                    }
                    // User may not open multiple documents in the exporter
                    else if (EnvironmentEnabled)
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
                else if (EnvironmentState == EnvironmentStateEnum.kTerminateEnvironmentState && EnvironmentEnabled)
                {
                    if (exporterBlocked)
                        exporterBlocked = false;
                    else
                        ClosingExporter();
                }
            }

            HandlingCode = HandlingCodeEnum.kEventNotHandled;
        }

        private bool exporterBlocked = false;
        private JointForm jointForm;
        private HighlightSet blueHighlightSet;
        private HighlightSet greenHighlightSet;
        private HighlightSet redHighlightSet;

        #endregion

        #region Custom Button Events

        private bool displayDOF = false;
        public AdvancedJointEditorUserControl AdvancedAdvancedJointEditor;

        public void DOF_OnExecute(NameValueMap Context)
        {

            AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "DOF", 0);

            displayDOF = !displayDOF;
            InventorUtils.EmbededKeyPane.Visible = displayDOF;

            if (displayDOF)
            {
                if (InventorUtils.GUI.SkeletonBase == null && !InventorUtils.GUI.LoadRobotSkeleton())
                    return;

                var rootNodes = new List<RigidNode_Base> {InventorUtils.GUI.SkeletonBase};
                var jointedNodes = new List<RigidNode_Base>();
                var problemNodes = new List<RigidNode_Base>();

                foreach (RigidNode_Base node in InventorUtils.GUI.SkeletonBase.ListAllNodes())
                {
                    if (node == InventorUtils.GUI.SkeletonBase) // Base node is already dealt with TODO: add ListChildren() to RigidNode_Base
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
                ClearDOFHighlight();

            }
        }

        public void ClearDOFHighlight()
        {
            blueHighlightSet.Clear();
            greenHighlightSet.Clear();
            redHighlightSet.Clear();
            displayDOF = false;
        }

        private void CreateHighlightSet(List<RigidNode_Base> nodes, HighlightSet highlightSet)
        {
            highlightSet.Clear();
            foreach (var componentOccurrence in InventorUtils.GetComponentOccurrencesFromNodes(nodes))
            {
                highlightSet.AddItem(componentOccurrence);
            }
        }

        public void EditJoint_OnExecute(NameValueMap Context)
        {
            AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "Edit Joint", 0);
            if (jointForm.Visible)
            {
                jointForm.Hide();
            }
            else
            {
                if (InventorUtils.GUI.SkeletonBase == null && !InventorUtils.GUI.LoadRobotSkeleton())
                    return;
//                Utilities.HideAdvancedJointEditor();
                jointForm.OnShowButtonClick();
                jointForm.ShowDialog();
                AdvancedAdvancedJointEditor.SetSkeleton(SynthesisGUI.Instance.SkeletonBase);
            }
        }

        private void AdvancedEditJoint_OnExecute(NameValueMap Context)
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
        /// <param name="Context"></param>
        private void SaveRobotData(NameValueMap Context)
        {
           
            if (InventorUtils.GUI.SaveRobotData())
                PendingChanges = false;
        }

        /// <summary>
        /// Saves the active robot to the active directory
        /// </summary>
        /// <param name="Context"></param>
        private void ExportButtonOnExecute(NameValueMap Context)
        {
            PromptExportToSynthesis();
        }

        public void PromptExportToSynthesis()
        {
            if (InventorUtils.GUI.PromptExportSettings())
                if (InventorUtils.GUI.ExportRobot() && InventorUtils.GUI.RMeta.FieldName != null)

                    InventorUtils.GUI.OpenSynthesis();
        }

        //Settings
        /// <summary>
        /// Opens the <see cref="DrivetrainWeightForm"/> form to allow the user to set the weight of their robot.
        /// </summary>
        /// <param name="Context"></param>
        private void SetWeight_OnExecute(NameValueMap Context)
        {
            AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "Set Weight", 0);
            if (InventorUtils.GUI.PromptRobotWeight())
                PendingChanges = true;
        }

        /// <summary>
        /// Opens the help page on bxd.autodesk.com. This is the callback used for all OnHelp events.
        /// </summary>
        /// <param name="Context"></param>
        /// <param name="HandlingCode"></param>
        private void _OnHelp(NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {
            Process.Start("http://bxd.autodesk.com/synthesis/tutorials-robot.html");
            HandlingCode = HandlingCodeEnum.kEventHandled;
        }

        #endregion

        #region RobotExportAPI Events

        /// <summary>
        /// Called when the user presses 'OK' in the settings menu
        /// </summary>
        /// <param name="Child"></param>
        /// <param name="UseFancyColors"></param>
        /// <param name="SaveLocation"></param>
        private void ExporterSettings_SettingsChanged(System.Drawing.Color Child, bool UseFancyColors,
            string SaveLocation, bool openSynthesis, string fieldLocation, string defaultRobotCompetition, bool useAnalytics)
        {
            ChildHighlight.Color = InventorUtils.GetInventorColor(Child);
            AnalyticsUtils.LogEvent("Toolbar", "Button Clicked", "Exporter Settings", 0);
            //Update Application
            Properties.Settings.Default.ExportToField = openSynthesis;
            Properties.Settings.Default.SelectedField = fieldLocation;
            Properties.Settings.Default.ChildColor = Child;
            Properties.Settings.Default.FancyColors = UseFancyColors;
            Properties.Settings.Default.SaveLocation = SaveLocation;
            Properties.Settings.Default.DefaultRobotCompetition = defaultRobotCompetition;
            Properties.Settings.Default.UseAnalytics = useAnalytics;
            Properties.Settings.Default.ConfigVersion =
                3; // Update this config version number when changes are made to the exporter which require settings to be reset or changed when the exporter starts
            Properties.Settings.Default.Save();
        }

        #endregion

        #endregion

        #region Miscellaneous Methods and Nested Classes

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
            int ValidationCount = 0;
            int FailedCount = 0;
            List<RigidNode_Base> nodes = baseNode.ListAllNodes();
            foreach (RigidNode_Base node in nodes)
            {
                bool FailedValidation = false;
                foreach (string componentName in node.ModelFullID.Split(new string[] {"-_-"},
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!CheckForOccurrence(componentName))
                    {
                        FailedCount++;
                        FailedValidation = true;
                    }
                }

                if (!FailedValidation)
                {
                    ValidationCount++;
                }
            }

            if (ValidationCount == nodes.Count)
            {
                message = string.Format("The assembly validated successfully. {0} / {1} nodes checked out.",
                    ValidationCount, nodes.Count);
                return true;
            }
            else
            {
                message = string.Format(
                    "The assembly failed to validate. {0} / {1} nodes checked out. {2} parts/assemblies were not found.",
                    ValidationCount, nodes.Count, FailedCount);
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
        /// <param name="Name"></param>
        /// <param name="jointNodeType"></param>
        private void SelectNode(string Name)
        {
            foreach (ComponentOccurrence Occ in AsmDocument.ComponentDefinition.Occurrences)
            {
                if (Occ.Name == Name)
                {
                    ChildHighlight.AddItem(Occ);
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

        #endregion
    }
}