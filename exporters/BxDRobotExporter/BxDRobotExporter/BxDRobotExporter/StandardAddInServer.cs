using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using EditorsLibrary;
using System.Runtime.InteropServices;
using System.Collections;

namespace BxDRobotExporter
{
    public class ExporterFailedException : ApplicationException
    {
        public ExporterFailedException(string message) : base(message) {}
    }

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
                if (Utilities.GUI.SkeletonBase == null)
                    return false;
                else
                    return pendingChanges;
            }
            set
            {
                if (SaveButton != null)
                {
                    SaveButton.Enabled = value; // Disable save button if changes have been saved
                }

                pendingChanges = value;
            }
        }
        private bool pendingChanges = false;
        
        public Inventor.Application MainApplication;

        AssemblyDocument AsmDocument;
        List<ComponentOccurrence> disabledAssemblyOccurences;
        Inventor.Environment ExporterEnv;
        bool EnvironmentEnabled = false;

        //Makes sure that the application doesn't create a bunch of dockable windows. Nobody wants that crap.
        bool HiddenExporter = false;

        //Ribbon Pannels
        RibbonPanel SetupPanel;
        RibbonPanel SettingsPanel;
        RibbonPanel FilePanel;

        //Standalone Buttons
        ButtonDefinition WizardExportButton;
        ButtonDefinition SetWeightButton;
        ButtonDefinition SaveButton;
        ButtonDefinition ExportButton;

        //Highlighting
        HighlightSet ChildHighlight;
        HighlightSet WheelHighlight;

        #region DEBUG
#if DEBUG
        RibbonPanel DebugPanel;
        ButtonDefinition DedectionTestButton;
        ButtonDefinition UITestButton;
#endif
        #endregion
        #endregion

        #region ApplicationAddInServer Methods
        /// <summary>
        /// Called when the <see cref="StandardAddInServer"/> is being loaded
        /// </summary>
        /// <param name="AddInSiteObject"></param>
        /// <param name="FirstTime"></param>
        public void Activate(ApplicationAddInSite AddInSiteObject, bool FirstTime)
        {
            MainApplication = AddInSiteObject.Application; //Gets the application object, which is used in many different ways throughout this whole process
            string ClientID = "{0c9a07ad-2768-4a62-950a-b5e33b88e4a3}";
            Utilities.LoadSettings();

            #region Add Parallel Environment

            #region Load Images
      
            stdole.IPictureDisp ExportRobotIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.SynthesisLogo16));
            stdole.IPictureDisp ExportRobotIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.SynthesisLogo32));

            stdole.IPictureDisp SaveRobotIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Save16));
            stdole.IPictureDisp SaveRobotIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Save32));

            stdole.IPictureDisp ExportSetupRobotIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Gears16));
            stdole.IPictureDisp ExportSetupRobotIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Gears32));

            stdole.IPictureDisp YeetRobotIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Wand16));//these are still here at request of QA
            stdole.IPictureDisp YeetRobotIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Wand32));

            stdole.IPictureDisp WeightRobotIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Weight16));
            stdole.IPictureDisp WeightRobotIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Weight32));

            stdole.IPictureDisp SynthesisLogoSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.SynthesisLogo16));
            stdole.IPictureDisp SynthesisLogoLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.SynthesisLogo32));

            #region DEBUG
#if DEBUG
            stdole.IPictureDisp DebugButtonSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Wand16));
            stdole.IPictureDisp DebugButtonLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Wand32));
#endif
            #endregion

            #endregion

            #region UI Creation

            #region Setup New Environment and Ribbon
            Environments environments = MainApplication.UserInterfaceManager.Environments;
            ExporterEnv = environments.Add("Robot Exporter", "BxD:RobotExporter:Environment", null, SynthesisLogoSmall, SynthesisLogoLarge);

            Ribbon assemblyRibbon = MainApplication.UserInterfaceManager.Ribbons["Assembly"];
            RibbonTab ExporterTab = assemblyRibbon.RibbonTabs.Add("Robot Exporter", "BxD:RobotExporter:RobotExporterTab", ClientID, "", false, true);

            ControlDefinitions ControlDefs = MainApplication.CommandManager.ControlDefinitions;

            SetupPanel = ExporterTab.RibbonPanels.Add("Start Over", "BxD:RobotExporter:SetupPanel", ClientID);
            SettingsPanel = ExporterTab.RibbonPanels.Add("Settings", "BxD:RobotExporter:SettingsPanel", ClientID);
            FilePanel = ExporterTab.RibbonPanels.Add("File", "BxD:RobotExporter:FilePanel", ClientID);

            // Reset positioning of panels
            SettingsPanel.Reposition("BxD:RobotExporter:SetupPanel", false);
            FilePanel.Reposition("BxD:RobotExporter:SettingsPanel", false);
            #endregion

            #region Setup Buttons
            //Begin Wizard Export
            WizardExportButton = ControlDefs.AddButtonDefinition("Exporter Setup", "BxD:RobotExporter:BeginWizardExport", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, "Quickly configure wheel and joint information.", ExportSetupRobotIconSmall, ExportSetupRobotIconLarge);
            WizardExportButton.OnExecute += BeginWizardExport_OnExecute;
            WizardExportButton.OnHelp += _OnHelp;
            SetupPanel.CommandControls.AddButton(WizardExportButton, true);

            //Set Weight
            SetWeightButton = ControlDefs.AddButtonDefinition("Robot Weight", "BxD:RobotExporter:SetWeight", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, "Change the weight of the robot.", WeightRobotIconSmall, WeightRobotIconLarge);
            SetWeightButton.OnExecute += SetWeight_OnExecute;
            SetWeightButton.OnHelp += _OnHelp;
            SettingsPanel.CommandControls.AddButton(SetWeightButton, true);

            //Save Button
            SaveButton = ControlDefs.AddButtonDefinition("Save Configuration", "BxD:RobotExporter:SaveRobot", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, "Save robot configuration to your assembly file for future exporting.", SaveRobotIconSmall, SaveRobotIconLarge);
            SaveButton.OnExecute += SaveButton_OnExecute;
            SaveButton.OnHelp += _OnHelp;
            FilePanel.CommandControls.AddButton(SaveButton, true);

            //Export Button
            ExportButton = ControlDefs.AddButtonDefinition("Export to Synthesis", "BxD:RobotExporter:ExportRobot", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, "Export your robot's model to Synthesis.", ExportRobotIconSmall, ExportRobotIconLarge);
            ExportButton.OnExecute += ExportButton_OnExecute;
            ExportButton.OnHelp += _OnHelp;
            FilePanel.CommandControls.AddButton(ExportButton, true);

            #endregion

            #region DEBUG
#if DEBUG
            DebugPanel = ExporterTab.RibbonPanels.Add("Debug", "BxD:RobotExporter:DebugPanel", ClientID);
            //Selection Test
            DedectionTestButton = ControlDefs.AddButtonDefinition("Detection Test", "BxD:RobotExporter:DetectionTestButton", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, null, DebugButtonSmall, DebugButtonLarge);
            DedectionTestButton.OnExecute += delegate (NameValueMap context)
            {
                if (Wizard.WizardUtilities.DetectWheels(Utilities.GUI.SkeletonBase, out List<RigidNode_Base> leftWheels, out List<RigidNode_Base> rightWheels))
                {
                    List<RigidNode_Base> allWheels = new List<RigidNode_Base>();
                    allWheels.AddRange(leftWheels);
                    allWheels.AddRange(rightWheels);
                    SelectNodes(allWheels);
                }
            };
            DebugPanel.CommandControls.AddButton(DedectionTestButton, true);
            //UI Test
            UITestButton = ControlDefs.AddButtonDefinition("UI Test", "BxD:RobotExporter:UITestButton", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, null, DebugButtonSmall, DebugButtonLarge);
            UITestButton.OnExecute += delegate (NameValueMap context)
            {
                    Wizard.WizardForm wizard = new Wizard.WizardForm();

                    wizard.ShowDialog();
                    if (Properties.Settings.Default.ShowExportOrAdvancedForm)
                    {
                        Form finishDialog = new Wizard.ExportOrAdvancedForm();
                        finishDialog.ShowDialog();
                    }
            };
            DebugPanel.CommandControls.AddButton(UITestButton, true);
#endif
            #endregion

            #endregion

            #region Final Environment Setup
            ExporterEnv.DefaultRibbonTab = "BxD:RobotExporter:RobotExporterTab";
            MainApplication.UserInterfaceManager.ParallelEnvironments.Add(ExporterEnv);
            ExporterEnv.DisabledCommandList.Add(MainApplication.CommandManager.ControlDefinitions["BxD:RobotExporter:Environment"]);
            #endregion

            #region Event Handler Assignment
            MainApplication.UserInterfaceManager.UserInterfaceEvents.OnEnvironmentChange += UIEvents_OnEnvironmentChange;
            MainApplication.ApplicationEvents.OnActivateDocument += ApplicationEvents_OnActivateDocument;
            MainApplication.ApplicationEvents.OnDeactivateDocument += ApplicationEvents_OnDeactivateDocument;
            MainApplication.ApplicationEvents.OnCloseDocument += ApplicationEvents_OnCloseDocument;
            LegacyInterchange.LegacyEvents.RobotModified += new Action( () => { PendingChanges = true; } );
            #endregion 

            #endregion

            Instance = this;
        }

        /// <summary>
        /// Called when the <see cref="StandardAddInServer"/> is being unloaded
        /// </summary>
        public void Deactivate()
        {
            Marshal.ReleaseComObject(MainApplication);
            MainApplication = null;
            FilePanel.Parent.Delete();
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
            //Gets the assembly document and creates dockable windows
            AsmDocument = (AssemblyDocument)MainApplication.ActiveDocument;
            Utilities.CreateDockableWindows(MainApplication);
            ChildHighlight = AsmDocument.CreateHighlightSet();
            ChildHighlight.Color = Utilities.GetInventorColor(SynthesisGUI.PluginSettings.InventorChildColor);
            WheelHighlight = AsmDocument.CreateHighlightSet();
            WheelHighlight.Color = Utilities.GetInventorColor(System.Drawing.Color.Green);

            //Sets up events for selecting and deselecting parts in inventor
            Utilities.GUI.jointEditorPane1.SelectedJoint += SelectNodes;
            PluginSettingsForm.PluginSettingsValues.SettingsChanged += ExporterSettings_SettingsChanged;
            
            EnvironmentEnabled = true;
            
            // Load robot skeleton and prepare UI
            if (!Utilities.GUI.LoadRobotSkeleton())
            {
                ForceQuitExporter(AsmDocument);
                return;
            }

            disabledAssemblyOccurences = new List<ComponentOccurrence>();
            disabledAssemblyOccurences.AddRange(DisableUnconnectedComponents(AsmDocument));
            // If fails to load existing data, restart wizard
            if (!Utilities.GUI.LoadRobotData(AsmDocument))
            {
                Wizard.WizardForm wizard = new Wizard.WizardForm();
                wizard.ShowDialog();
                if (Properties.Settings.Default.ShowExportOrAdvancedForm)
                {
                    Form finishDialog = new Wizard.ExportOrAdvancedForm();
                    finishDialog.ShowDialog();
                }
                PendingChanges = true; // Force save button on since no data has been saved to this file
            }
            else
                PendingChanges = false; // No changes are pending if data has been loaded

            // Hide non-jointed components;
            
            // Reload panels in UI
            Utilities.GUI.ReloadPanels();
            Utilities.ShowDockableWindows();
        }
        
        /// <summary>
        /// Disposes of some COM objects and exits the environment
        /// </summary>
        private void ClosingExporter()
        {
            WarnIfUnsaved(false);

            // Re-enable disabled components
            if (disabledAssemblyOccurences != null)
                EnableComponents(disabledAssemblyOccurences);
            disabledAssemblyOccurences = null;

            // Close add-in
            Utilities.DisposeDockableWindows();

            // Dispose of document
            if (AsmDocument != null)
                Marshal.ReleaseComObject(AsmDocument);
            AsmDocument = null;

            ChildHighlight = null;

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
        private void ApplicationEvents_OnDeactivateDocument(_Document DocumentObject, EventTimingEnum BeforeOrAfter, NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {
            if (EnvironmentEnabled)
            {
                if (BeforeOrAfter == EventTimingEnum.kBefore)
                {
                    Utilities.HideDockableWindows();
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
        private void ApplicationEvents_OnActivateDocument(_Document DocumentObject, EventTimingEnum BeforeOrAfter, NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {
            if (BeforeOrAfter == EventTimingEnum.kBefore)
            {
                if (DocumentObject is AssemblyDocument assembly)
                {
                    if ((AsmDocument == null || assembly == AsmDocument) && HiddenExporter)
                    {
                        Utilities.ShowDockableWindows();
                        HiddenExporter = false;
                    }
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
        private void ApplicationEvents_OnCloseDocument(_Document DocumentObject, string FullDocumentName, EventTimingEnum BeforeOrAfter, NameValueMap Context, out HandlingCodeEnum HandlingCode )
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
        private void UIEvents_OnEnvironmentChange(Inventor.Environment Environment, EnvironmentStateEnum EnvironmentState, EventTimingEnum BeforeOrAfter, NameValueMap Context, out HandlingCodeEnum HandlingCode)
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
                                        "Please finish using the exporter in \"" + AsmDocument.DisplayName + "\" to continue.",
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
        #endregion

        #region Custom Button Events

        /// <summary>
        /// Opens the <see cref="LiteExporterForm"/> through <see cref="Utilities.GUI"/>, then opens the <see cref="Wizard.WizardForm"/>
        /// </summary>
        /// <param name="Context"></param>
        public void BeginWizardExport_OnExecute(NameValueMap Context)
        {
                if (Utilities.GUI.SkeletonBase == null && !Utilities.GUI.LoadRobotSkeleton())
                    return;

                Wizard.WizardForm wizard = new Wizard.WizardForm();
                Utilities.HideDockableWindows();

                wizard.ShowDialog();
                if (Properties.Settings.Default.ShowExportOrAdvancedForm)
                {
                    Form finishDialog = new Wizard.ExportOrAdvancedForm();
                    finishDialog.ShowDialog();
                }
                Utilities.GUI.ReloadPanels();
                Utilities.ShowDockableWindows();
           
        }

        /// <summary>
        /// Saves the active robot to the active directory
        /// </summary>
        /// <param name="Context"></param>
        private void SaveButton_OnExecute(NameValueMap Context)
        {
            if (Utilities.GUI.SaveRobotData())
                PendingChanges = false;
        }

        /// <summary>
        /// Saves the active robot to the active directory
        /// </summary>
        /// <param name="Context"></param>
        private void ExportButton_OnExecute(NameValueMap Context)
        {
            if (Utilities.GUI.PromptExportSettings())
                if (Utilities.GUI.ExportRobot() && Utilities.GUI.RMeta.FieldName != null) 

                    Utilities.GUI.OpenSynthesis();
        }

        public void ForceExport()
        {
            ExportButton_OnExecute(null);
        }

    //Settings
    /// <summary>
    /// Opens the <see cref="SetWeightForm"/> form to allow the user to set the weight of their robot.
    /// </summary>
    /// <param name="Context"></param>
    private void SetWeight_OnExecute(NameValueMap Context)
        {
            if (Utilities.GUI.PromptRobotWeight())
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
        /// Selects a list of nodes in Inventor.
        /// </summary>
        /// <param name="nodes">List of nodes to select.</param>
        public void SelectNodes(List<RigidNode_Base> nodes)
        {
            ChildHighlight.Clear();

            if (nodes == null)
            {
                return;
            }

            // Get all node ID's
            List<string> nodeIDs = new List<string>(); ;
            foreach (RigidNode_Base node in nodes)
                nodeIDs.AddRange(node.GetModelID().Split(new String[] { "-_-" }, StringSplitOptions.RemoveEmptyEntries));
            
            // Select all nodes
            List<ComponentOccurrence> occurrences = new List<ComponentOccurrence>();
            foreach (string id in nodeIDs)
            {
                ComponentOccurrence occurrence = GetOccurrence(id);

                if (occurrence != null)
                {
                    SelectNode(occurrence);
                    occurrences.Add(occurrence);
                }
            }

            // Set camera view
            ViewOccurrences(occurrences, 15, ViewDirection.Y);
        }
        
        /// <summary>
        /// Public method used to select a node.
        /// </summary>
        /// <param name="node">Node to select.</param>
        public void SelectNode(RigidNode_Base node)
        {
            SelectNodes(new List<RigidNode_Base>() { node });
        }

        /// <summary>
        /// Called when the user presses 'OK' in the settings menu
        /// </summary>
        /// <param name="Child"></param>
        /// <param name="UseFancyColors"></param>
        /// <param name="SaveLocation"></param>
        private void ExporterSettings_SettingsChanged(System.Drawing.Color Child, bool UseFancyColors, string SaveLocation, bool openSynthesis, string fieldLocation)
        {
            ChildHighlight.Color = Utilities.GetInventorColor(Child);

            //Update Application
            Properties.Settings.Default.ExportToField = openSynthesis;
            Properties.Settings.Default.SelectedField = fieldLocation;
            Properties.Settings.Default.ChildColor = Child;
            Properties.Settings.Default.FancyColors = UseFancyColors;
            Properties.Settings.Default.SaveLocation = SaveLocation;
            Properties.Settings.Default.ConfigVersion = 2; // Update this config version number when changes are made to the exporter which require settings to be reset or changed when the exporter starts
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
                    {//accounts for components that can't be disabled
                        disabledAssemblyOccurences.Add(c);
                        c.Enabled = false;
                    }
                    catch (Exception) { }
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
                {//accounts for components that can't be disabled
                    c.Enabled = true;
                } catch (Exception){ }
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
                foreach (string componentName in node.ModelFullID.Split(new string[] { "-_-" }, StringSplitOptions.RemoveEmptyEntries))
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
                message = string.Format("The assembly validated successfully. {0} / {1} nodes checked out.", ValidationCount, nodes.Count);
                return true;
            }
            else
            {
                message = string.Format("The assembly failed to validate. {0} / {1} nodes checked out. {2} parts/assemblies were not found.", ValidationCount, nodes.Count, FailedCount);
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
        /// Gets the <see cref="ComponentOccurrence"/> of the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ComponentOccurrence GetOccurrence(string name)
        {
            foreach (ComponentOccurrence component in AsmDocument.ComponentDefinition.Occurrences)
            {
                if (component.Name == name)
                    return component;
            }
            return null;
        }

        /// <summary>
        /// Sets the position and target of the camera.
        /// </summary>
        /// <param name="focus">Point that camera should look at.</param>
        /// <param name="viewDistance">Distance the camera should be from that point</param>
        /// <param name="viewDirection">Direction to view the point from.</param>
        /// <param name="animate">True to animate movement of camera.</param>
        public void SetCameraView(Vector focus, double viewDistance, ViewDirection viewDirection = ViewDirection.Y, bool animate = true)
        {
            Camera cam = MainApplication.ActiveView.Camera;

            Inventor.Point focusPoint = MainApplication.TransientGeometry.CreatePoint(focus.X, focus.Y, focus.Z);

            cam.Target = focusPoint;

            // Flip view for negative direction
            if ((viewDirection & ViewDirection.Negative) == ViewDirection.Negative)
                viewDistance = -viewDistance;
            
            Inventor.UnitVector up = null;

            // Find camera position and upwards direction
            if ((viewDirection & ViewDirection.X) == ViewDirection.X)
            {
                focus.X += viewDistance;
                up = MainApplication.TransientGeometry.CreateUnitVector(0, 1, 0);
            }

            if ((viewDirection & ViewDirection.Y) == ViewDirection.Y)
            {
                focus.Y += viewDistance;
                up = MainApplication.TransientGeometry.CreateUnitVector(0, 0, 1);
            }

            if ((viewDirection & ViewDirection.Z) == ViewDirection.Z)
            {
                focus.Z += viewDistance;
                up = MainApplication.TransientGeometry.CreateUnitVector(0, 1, 0);
            }

            cam.Eye = MainApplication.TransientGeometry.CreatePoint(focus.X, focus.Y, focus.Z);
            cam.UpVector = up;

            // Apply settings
            if (animate)
                cam.Apply();
            else
                cam.ApplyWithoutTransition();
        }

        /// <summary>
        /// Moves the camera to the midpoint of all the specified occurrences. Used in the wizard to point out the specified occurence.
        /// </summary>
        /// <param name="occurrences">The <see cref="ComponentOccurrence"/>s for the <see cref="Camera"/> to focus on</param>
        /// <param name="viewDistance">The distence from <paramref name="occurrence"/> that the camera will be</param>
        /// <param name="viewDirection">The direction of the camera</param>
        /// <param name="animate">True if you want to animate the camera moving to the new position</param>
        public void ViewOccurrences(List<ComponentOccurrence> occurrences, double viewDistance, ViewDirection viewDirection = ViewDirection.Y, bool animate = false)
        {
            if (occurrences.Count < 1)
                return;

            double xSum = 0, ySum = 0, zSum = 0;
            int i = 0;
            foreach(ComponentOccurrence occurrence in occurrences)
            {
                xSum += occurrence.Transformation.Translation.X;
                ySum += occurrence.Transformation.Translation.Y;
                zSum += occurrence.Transformation.Translation.Z;

                i++;
            }
            Vector translation = MainApplication.TransientGeometry.CreateVector((xSum / i), (ySum / i), (zSum / i));

            SetCameraView(translation, viewDistance, viewDirection, animate);
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
        public void ToolTip(ButtonDefinition button, string title, string description, string expandedDescription = null, stdole.IPictureDisp picture = null)
        {
            if(description != null)
                button.ProgressiveToolTip.Description = description;
            if(expandedDescription != null)
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
        /// If the user has unsaved work, warn them that they are about to exit with unsaved work
        /// </summary>
        /// <returns>True if the user wishes to continue without saving/no saving is needed.</returns>
        public bool WarnIfUnsaved(bool allowCancel = true)
        {
            if (!PendingChanges)
                return true; // No changes to save

            DialogResult saveResult = MessageBox.Show("Save robot configuration?", "Save",
                                                      allowCancel ? MessageBoxButtons.YesNoCancel : MessageBoxButtons.YesNo);

            if (saveResult == DialogResult.Yes)
            {
                SaveButton_OnExecute(null);
                return !PendingChanges;
            }
            else if (saveResult == DialogResult.No)
                return true; // Continue without saving
            else
                return false; // Don't continue
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