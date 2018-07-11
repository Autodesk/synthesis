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
      
            stdole.IPictureDisp ExportRobotIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Export16));
            stdole.IPictureDisp ExportRobotIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Export32));

            stdole.IPictureDisp SaveRobotIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Save16));
            stdole.IPictureDisp SaveRobotIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Save32));

            stdole.IPictureDisp ExportSetupRobotIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Gears16));
            stdole.IPictureDisp ExportSetupRobotIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Gears32));

            stdole.IPictureDisp YeetRobotIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Wand16));//these are still here at request of QA
            stdole.IPictureDisp YeetRobotIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Wand32));

            stdole.IPictureDisp WeightRobotIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Weight16));
            stdole.IPictureDisp WeightRobotIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Weight32));

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
            ExporterEnv = environments.Add("Robot Exporter", "BxD:RobotExporter:Environment", null, ExportRobotIconSmall, ExportRobotIconLarge);

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
                Wizard.WizardUtilities.DetectWheels(Utilities.GUI.SkeletonBase, Wizard.WizardData.WizardDriveTrain.TANK, 6);
            };
            DebugPanel.CommandControls.AddButton(DedectionTestButton, true);
            //UI Test
            UITestButton = ControlDefs.AddButtonDefinition("UI Test", "BxD:RobotExporter:UITestButton", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, null, DebugButtonSmall, DebugButtonLarge);
            UITestButton.OnExecute += delegate (NameValueMap context)
            {
                    Wizard.WizardForm wizard = new Wizard.WizardForm();

                    wizard.ShowDialog();
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
        /// Enables or disables the <see cref="Inventor.Environment"/>
        /// </summary>
        /// <remarks>
        /// calls StartExporter and EndExporter
        /// </remarks>
        public void ToggleEnvironment()
        {
            if (EnvironmentEnabled)
            {
                ClosingExporter();
            }
            else
            {
                OpeningExporter();
            }
        }

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
            Utilities.GUI.jointEditorPane1.SelectedJoint += JointEditorPane_SelectedJoint;
            PluginSettingsForm.PluginSettingsValues.SettingsChanged += ExporterSettings_SettingsChanged;
            
            EnvironmentEnabled = true;
            
            // Load robot skeleton and prepare UI
            if (!Utilities.GUI.LoadRobotSkeleton())
            {
                ForceQuitExporter();
                return;
            }

            // Hide non-jointed components
            disabledAssemblyOccurences = new List<ComponentOccurrence>();
            disabledAssemblyOccurences.AddRange(DisableUnconnectedComponents(AsmDocument));

            // If fails to load existing data, restart wizard
            if (!Utilities.GUI.LoadRobotData(AsmDocument))
            {
                Wizard.WizardForm wizard = new Wizard.WizardForm();
                wizard.ShowDialog();
                PendingChanges = true; // Force save button on since no data has been saved to this file
            }
            else
                PendingChanges = false; // No changes are pending if data has been loaded

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
            EnableComponents(disabledAssemblyOccurences);
            disabledAssemblyOccurences.Clear();

            // Close add-in
            Utilities.DisposeDockableWindows();
            if (AsmDocument != null)
                Marshal.ReleaseComObject(AsmDocument);

            AsmDocument = null;
            ChildHighlight = null;

            EnvironmentEnabled = false;
        }

        /// <summary>
        /// Causes the exporter to close.
        /// </summary>
        private async void ForceQuitExporter()
        {
            await Task.Delay(1); // Delay is needed so that environment is closed after it has finished opening
            AsmDocument.EnvironmentManager.SetCurrentEnvironment(AsmDocument.EnvironmentManager.EditObjectEnvironment);
        }
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
            if (BeforeOrAfter == EventTimingEnum.kBefore && EnvironmentEnabled)
            {
                Utilities.HideDockableWindows();
                HiddenExporter = true;
            }
            
            // Re-enable the exporter in this assembly if it was disabled
            if (DocumentObject is AssemblyDocument assembly)
                EnableExporterInDoc(assembly);

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
            if (DocumentObject is PartDocument part)
            {
                part.DisabledCommandList.Add(MainApplication.CommandManager.ControlDefinitions["BxD:RobotExporter:Environment"]);
            }
            else if (DocumentObject is PresentationDocument presentation)
            {
                presentation.DisabledCommandList.Add(MainApplication.CommandManager.ControlDefinitions["BxD:RobotExporter:Environment"]);
            }
            else if (DocumentObject is DrawingDocument drawing)
            {
                drawing.DisabledCommandList.Add(MainApplication.CommandManager.ControlDefinitions["BxD:RobotExporter:Environment"]);
            }
            else if (DocumentObject is AssemblyDocument assembly && AsmDocument != null) // Don't disable the exporter if it isn't open
            {
                if (assembly.Equals(AsmDocument) && HiddenExporter)
                {
                    Utilities.ShowDockableWindows();
                    HiddenExporter = false;
                }
                else if (!assembly.Equals(AsmDocument))
                {
                    DisableExporterInDoc(assembly);
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
            if (Environment.Equals(ExporterEnv))
            {
                if (EnvironmentState == EnvironmentStateEnum.kActivateEnvironmentState && !EnvironmentEnabled && BeforeOrAfter == EventTimingEnum.kBefore)
                {
                    ToggleEnvironment();
                }
                else if (EnvironmentState == EnvironmentStateEnum.kTerminateEnvironmentState && EnvironmentEnabled && BeforeOrAfter == EventTimingEnum.kBefore)
                {
                    ToggleEnvironment();
                }
            }
            HandlingCode = HandlingCodeEnum.kEventNotHandled;
        }
        #endregion

        #region Custom Button Events

        /// <summary>
        /// Opens the <see cref="LiteExporterForm"/> through <see cref="Utilities.GUI"/>, then opens the <see cref="Wizard.WizardForm"/>
        /// </summary>
        /// <param name="Context"></param>
        public void BeginWizardExport_OnExecute(NameValueMap Context)
        {
            if (WarnIfUnsaved())
            {

                if (Utilities.GUI.SkeletonBase == null && !Utilities.GUI.LoadRobotSkeleton())
                    return;

                Wizard.WizardForm wizard = new Wizard.WizardForm();
                Utilities.HideDockableWindows();

                wizard.ShowDialog();
                    
                Utilities.GUI.ReloadPanels();
                Utilities.ShowDockableWindows();
            }
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
        /// Selects all the <see cref="ComponentOccurrence"/>s in inventor associated with the given joint or joints. 
        /// </summary>
        /// <param name="nodes"></param>
        public void JointEditorPane_SelectedJoint(List<RigidNode_Base> nodes)
        {
            ChildHighlight.Clear();
            if (nodes == null)
            {
                return;
            }
            if (nodes.Count == 1)
            {
                ComponentOccurrence occurrence = GetOccurrence(nodes[0].GetModelID().Substring(0, nodes[0].GetModelID().Length - 3));
                SelectNode(occurrence);
                ViewOccurrence(occurrence, 15, ViewDirection.Y, false);
            }

            else
            {
                List<ComponentOccurrence> occurrences = new List<ComponentOccurrence>();
                foreach (RigidNode_Base node in nodes)
                {
                    ComponentOccurrence occurrence = GetOccurrence(node.GetModelID().Substring(0, node.GetModelID().Length - 3));
                    SelectNode(occurrence);
                    occurrences.Add(occurrence);
                }
                ViewOccurrences(occurrences, 15, ViewDirection.Y, false);
            }

        }

        /// <summary>
        /// Called when the user presses 'OK' in the settings menu
        /// </summary>
        /// <param name="Child"></param>
        /// <param name="UseFancyColors"></param>
        /// <param name="SaveLocation"></param>
        private void ExporterSettings_SettingsChanged(System.Drawing.Color Child, bool UseFancyColors, string SaveLocation)
        {
            ChildHighlight.Color = Utilities.GetInventorColor(Child);

            //Update Application
            Properties.Settings.Default.ChildColor = Child;
            Properties.Settings.Default.FancyColors = UseFancyColors;
            Properties.Settings.Default.SaveLocation = SaveLocation;
            Properties.Settings.Default.ConfigVersion = 1; // Update this config version number when changes are made to the exporter which require settings to be reset or changed when the exporter starts
            Properties.Settings.Default.Save();
        } 
        #endregion
        #endregion

        #region Miscellaneous Methods and Nested Classes
        /// <summary>
        /// Disable the exporter command for an open document.
        /// </summary>
        /// <param name="doc">Document to disable.</param>
        void DisableExporterInDoc(AssemblyDocument doc)
        {
            ControlDefinition exporterControl = MainApplication.CommandManager.ControlDefinitions["BxD:RobotExporter:Environment"];

            // Don't disable when doc already has exporter disabled
            foreach (ControlDefinition control in doc.DisabledCommandList)
                if (control == exporterControl)
                    return;

            // Disable exporter in doc
            doc.DisabledCommandList.Add(exporterControl);
        }

        /// <summary>
        /// Enable the exporter command for an open document.
        /// </summary>
        /// <param name="doc">Document to enable.</param>
        void EnableExporterInDoc(AssemblyDocument doc)
        {
            ControlDefinition exporterControl = MainApplication.CommandManager.ControlDefinitions["BxD:RobotExporter:Environment"];

            // Remove the exporter control from the disable commands list
            for (int i = 0; i < doc.DisabledCommandList.Count; i++)
            {
                if (doc.DisabledCommandList[i] == exporterControl)
                {
                    doc.DisabledCommandList.Remove(i);
                    return;
                }
            }
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
                    disabledAssemblyOccurences.Add(c);
                    c.Enabled = false;
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
                c.Enabled = true;
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
        /// Moves the camera to the specified occurrence. Used in the wizard to point out the specified occurence.
        /// </summary>
        /// <param name="occurrence">The <see cref="ComponentOccurrence"/> for the <see cref="Camera"/> to focus on</param>
        /// <param name="viewDistance">The distence from <paramref name="occurrence"/> that the camera will be</param>
        /// <param name="viewDirection">The direction of the camera</param>
        /// <param name="animate">True if you want to animate the camera moving to the new position</param>
        public void ViewOccurrence(ComponentOccurrence occurrence, double viewDistance, ViewDirection viewDirection = ViewDirection.Y, bool animate = true)
        {
            //The translation from the origin of occurrence
            Vector translation = occurrence.Transformation.Translation;

            Camera cam = MainApplication.ActiveView.Camera;
            Inventor.Point partTrans = MainApplication.TransientGeometry.CreatePoint(translation.X, translation.Y, translation.Z);
            cam.Target = partTrans;

            if ((viewDirection & ViewDirection.Negative) == ViewDirection.Negative)
                viewDistance = -viewDistance;
            Inventor.Point eye = null;
            if ((viewDirection & ViewDirection.X) == ViewDirection.X)
            {
                eye = MainApplication.TransientGeometry.CreatePoint(translation.X + viewDistance, translation.Y, translation.Z);
            }
            else if ((viewDirection & ViewDirection.Y) == ViewDirection.Y)
            {
                eye = MainApplication.TransientGeometry.CreatePoint(translation.X, translation.Y + viewDistance, translation.Z);
            }
            else if ((viewDirection & ViewDirection.Z) == ViewDirection.Z)
            {
                eye = MainApplication.TransientGeometry.CreatePoint(translation.X, translation.Y, translation.Z + viewDistance);
            }

            cam.Eye = eye;
            cam.UpVector = MainApplication.TransientGeometry.CreateUnitVector(0, 0, 1);
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
        public void ViewOccurrences(List<ComponentOccurrence> occurrences, double viewDistance, ViewDirection viewDirection = ViewDirection.Y, bool animate = true)
        {
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

            Camera cam = MainApplication.ActiveView.Camera;
            Inventor.Point partTrans = MainApplication.TransientGeometry.CreatePoint(translation.X, translation.Y, translation.Z);
            cam.Target = partTrans;

            if ((viewDirection & ViewDirection.Negative) == ViewDirection.Negative)
                viewDistance = -viewDistance;
            Inventor.Point eye = null;
            if ((viewDirection & ViewDirection.X) == ViewDirection.X)
            {
                eye = MainApplication.TransientGeometry.CreatePoint(translation.X + viewDistance, translation.Y, translation.Z);
            }
            else if ((viewDirection & ViewDirection.Y) == ViewDirection.Y)
            {
                eye = MainApplication.TransientGeometry.CreatePoint(translation.X, translation.Y + viewDistance, translation.Z);
            }
            else if ((viewDirection & ViewDirection.Z) == ViewDirection.Z)
            {
                eye = MainApplication.TransientGeometry.CreatePoint(translation.X, translation.Y, translation.Z + viewDistance);
            }

            cam.Eye = eye;
            cam.UpVector = MainApplication.TransientGeometry.CreateUnitVector(0, 0, 1);
            if (animate)
                cam.Apply();
            else
                cam.ApplyWithoutTransition();
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
        /// Public method used to select a node from the wizard.
        /// </summary>
        /// <param name="node"></param>
        public void WizardSelect(RigidNode_Base node)
        {
            ChildHighlight.Clear();
            
            if(node.GetModelID().Split(new string[] { "-_-" }, StringSplitOptions.RemoveEmptyEntries).Length == 1)
            {
                ComponentOccurrence occurrence = GetOccurrence(node.GetModelID().Substring(0, node.GetModelID().Length - 3));
                SelectNode(occurrence);
                ViewOccurrence(occurrence, 15, ViewDirection.Y, false);
            }
            else
            {
                List<ComponentOccurrence> occurrences = new List<ComponentOccurrence>();
                foreach (string s in node.GetModelID().Split(new string[] { "-_-" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    ComponentOccurrence occurrence = GetOccurrence(s);
                    SelectNode(occurrence);
                    occurrences.Add(occurrence);
                }
                ViewOccurrences(occurrences, 15, ViewDirection.Y, false);
            }
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