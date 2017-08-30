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
                if (Utilities.GUI.SkeletonBase == null)
                    return false;
                else
                    return pendingChanges;
            }
            set => pendingChanges = value;
        }
        private bool pendingChanges = false;
        
        public Inventor.Application MainApplication;

        AssemblyDocument AsmDocument;
        Inventor.Environment ExporterEnv;
        bool EnvironmentEnabled = false;

        /// <summary>
        /// A dictionary containing all of the other open assembly documents. 
        /// </summary>
        Dictionary<AssemblyDocument, int> DisabledCommandDocuments = new Dictionary<AssemblyDocument, int>();

        //Makes sure that the application doesn't create a bunch of dockable windows. Nobody wants that crap.
        bool HiddenExporter = false;

        //Ribbon Pannels
        RibbonPanel BeginPanel;
        RibbonPanel FilePanel;
        RibbonPanel SettingsPanel;
        RibbonPanel HelpPanel;

        //Standalone Buttons
        ButtonDefinition LoadExportedRobotButton;
        ButtonDefinition ExporterSettingsButton;
        ButtonDefinition HelpButton;
        ButtonDefinition PreviewRobotButton;

        //Dropdown buttons
        ObjectCollection ExportButtonCollection;
        ButtonDefinition GenericExportButton;
        ButtonDefinition AdvancedExportButton;
        ButtonDefinition WizardExportButton;
        ButtonDefinition OneClickExportButton;

        ObjectCollection SaveButtonCollection;
        ButtonDefinition SaveButton;
        ButtonDefinition SaveAsButton;

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
            stdole.IPictureDisp StartExporterIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.StartRobotExporter16));
            stdole.IPictureDisp StartExporterIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.StartRobotExporter32));

            stdole.IPictureDisp ExportMeshesIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.ExportMeshes16));
            stdole.IPictureDisp ExportMeshesIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.ExportMeshes32));

            stdole.IPictureDisp ExportRobotIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.ExportRobot16));
            stdole.IPictureDisp ExportRobotIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.ExportRobot32));

            stdole.IPictureDisp ExporterSettingsIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.ExporterSettings16));
            stdole.IPictureDisp ExporterSettingsIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.ExporterSettings32));

            stdole.IPictureDisp HelpButtonIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Help16));
            stdole.IPictureDisp HelpButtonIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.Help32));

            stdole.IPictureDisp LoadExportedRobotIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.LoadRobot16));
            stdole.IPictureDisp LoadExportedRobotIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.LoadRobot32));

            stdole.IPictureDisp PreviewRobotIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.SelectJointInsideJoint16));
            stdole.IPictureDisp PreviewRobotIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.SelectJointInsideJoint32));

            stdole.IPictureDisp SaveRobotIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.SaveRobot16));
            stdole.IPictureDisp SaveRobotIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.SaveRobot32));

            stdole.IPictureDisp SaveRobotAsIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.SaveRobotAs16));
            stdole.IPictureDisp SaveRobotAsIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.SaveRobotAs32));

            stdole.IPictureDisp WizardExportIconSmall   = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.RobotMagicWand16));
            stdole.IPictureDisp WizardExportIconLarge   = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.RobotMagicWand32));

            stdole.IPictureDisp OneClickExportIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.RobotClick16));
            stdole.IPictureDisp OneClickExportIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.RobotClick32));

            #region DEBUG
#if DEBUG
            stdole.IPictureDisp DebugButtonSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.ViewerSettings16));
            stdole.IPictureDisp DebugButtonLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(Resource.ViewerSettings32));
#endif
            #endregion

            #endregion

            #region UI Creation

            #region Setup New Environment and Ribbon
            Environments environments = MainApplication.UserInterfaceManager.Environments;
            ExporterEnv = environments.Add("Robot Exporter", "BxD:RobotExporter:Environment", null, StartExporterIconSmall, StartExporterIconLarge);

            Ribbon assemblyRibbon = MainApplication.UserInterfaceManager.Ribbons["Assembly"];
            RibbonTab ExporterTab = assemblyRibbon.RibbonTabs.Add("Robot Exporter", "BxD:RobotExporter:RobotExporterTab", ClientID, "", false, true);

            ControlDefinitions ControlDefs = MainApplication.CommandManager.ControlDefinitions;

            BeginPanel = ExporterTab.RibbonPanels.Add("Begin", "BxD:RobotExporter:BeginPanel", ClientID);
            FilePanel = ExporterTab.RibbonPanels.Add("File", "BxD:RobotExporter:FilePanel", ClientID);
            SettingsPanel = ExporterTab.RibbonPanels.Add("Settings", "BxD:RobotExporter:SettingsPanel", ClientID);
            HelpPanel = ExporterTab.RibbonPanels.Add("Help", "BxD:RobotExporter:HelpPanel", ClientID);
            #endregion

            #region Setup Buttons
            //Generic Begin Export
            GenericExportButton = ControlDefs.AddButtonDefinition("Begin Export", "BxD:RobotExporter:BeginExport", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, "Opens a window with information on each export mode", ExportRobotIconSmall, ExportRobotIconLarge);
            GenericExportButton.OnExecute += BeginGenericExport_OnExecute;
            GenericExportButton.OnHelp += _OnHelp;

            //Begin Advanced Export
            AdvancedExportButton = ControlDefs.AddButtonDefinition("Begin Advanced Export", "BxD:RobotExporter:BeginAdvancedExport", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, "Exports your robot in advanced mode. Recommended if you intend to emulate code.", ExportRobotIconSmall, ExportRobotIconLarge);
            AdvancedExportButton.OnExecute += BeginAdvancedExport_OnExecute;
            AdvancedExportButton.OnHelp += _OnHelp;

            //Begin Wizard Export
            WizardExportButton = ControlDefs.AddButtonDefinition("Begin Guided Export", "BxD:RobotExporter:BeginWizardExport", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, "Exports the robot with the aid of a wizard to guide you through the process.", WizardExportIconSmall, WizardExportIconLarge);
            WizardExportButton.OnExecute += BeginWizardExport_OnExecute;
            WizardExportButton.OnHelp += _OnHelp;

            //Begin One Click Export
            OneClickExportButton = ControlDefs.AddButtonDefinition("Begin One Click Export", "BxD:RobotExporter:BeginOneClickExport", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, "Exports the robot with minimal user input. (Recommended for beginners)", OneClickExportIconSmall, OneClickExportIconLarge);
            OneClickExportButton.OnExecute += BeginOneClickExport_OnExecute;
            OneClickExportButton.OnHelp += _OnHelp;

            //Begin Export Control Definition
            ExportButtonCollection = MainApplication.TransientObjects.CreateObjectCollection();
            ExportButtonCollection.Add(WizardExportButton);
            ExportButtonCollection.Add(OneClickExportButton);
            ExportButtonCollection.Add(AdvancedExportButton);
            BeginPanel.CommandControls.AddSplitButton(GenericExportButton, ExportButtonCollection, true);
            
            //Load Exported Robot
            LoadExportedRobotButton = ControlDefs.AddButtonDefinition("Load Exported Robot", "BxD:RobotExporter:LoadExportedRobot", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, "Loads a robot you have already exported for further editing.", LoadExportedRobotIconSmall, LoadExportedRobotIconLarge);
            LoadExportedRobotButton.OnExecute += LoadExportedRobotButton_OnExecute;
            LoadExportedRobotButton.OnHelp += _OnHelp;
            BeginPanel.CommandControls.AddButton(LoadExportedRobotButton, true);

            //Preview Robot
            PreviewRobotButton = ControlDefs.AddButtonDefinition("Preview Robot", "BxD:RobotExporter:PreviewRobot", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, "Opens the robot viewer from the old exporter. Use this to test your joint limits.", PreviewRobotIconSmall, PreviewRobotIconLarge);
            PreviewRobotButton.OnExecute += PreviewRobotButton_OnExecute;
            PreviewRobotButton.OnHelp += _OnHelp;
            FilePanel.CommandControls.AddButton(PreviewRobotButton, true);

            //Exporter Settings
            ExporterSettingsButton = ControlDefs.AddButtonDefinition("Exporter Settings", "BxD:RobotExporter:ExporterSettings", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, "Opens the settings menu.", ExporterSettingsIconSmall, ExporterSettingsIconLarge);
            ExporterSettingsButton.OnExecute += ExporterSettings_OnExecute;
            ExporterSettingsButton.OnHelp += _OnHelp;
            SettingsPanel.CommandControls.AddButton(ExporterSettingsButton, true);

            //Help Button
            HelpButton = ControlDefs.AddButtonDefinition("Help", "BxD:RobotExporter:Help", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, "Takes you to the tutorials page at bxd.autodesk.com.", HelpButtonIconSmall, HelpButtonIconLarge);
            HelpButton.OnExecute += HelpButton_OnExecute;
            HelpButton.OnHelp += _OnHelp;
            HelpPanel.CommandControls.AddButton(HelpButton, true);

            //Save Button
            SaveButton = ControlDefs.AddButtonDefinition("Save", "BxD:RobotExporter:SaveRobot", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, "Saves your robot to its previous location.", SaveRobotIconSmall, SaveRobotIconLarge);
            SaveButton.OnExecute += SaveButton_OnExecute;
            SaveButton.OnHelp += _OnHelp;

            //Save As Button
            SaveAsButton = ControlDefs.AddButtonDefinition("Save As...", "BxD:RobotExporter:SaveAs", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, "Saves your robot to a new location.", SaveRobotAsIconSmall, SaveRobotAsIconLarge);
            SaveAsButton.OnExecute += SaveAsButton_OnExecute;
            SaveAsButton.OnHelp += _OnHelp;

            //Save Control Definition
            SaveButtonCollection = MainApplication.TransientObjects.CreateObjectCollection();
            SaveButtonCollection.Add(SaveButton);
            SaveButtonCollection.Add(SaveAsButton);
            FilePanel.CommandControls.AddSplitButton(SaveButton, SaveButtonCollection, true);

            #endregion

            #region DEBUG
#if DEBUG
            DebugPanel = ExporterTab.RibbonPanels.Add("Debug", "BxD:RobotExporter:DebugPanel", ClientID);
            //Selection Test
            DedectionTestButton = ControlDefs.AddButtonDefinition("Detection Test", "BxD:RobotExporter:DetectionTestButton", CommandTypesEnum.kNonShapeEditCmdType, ClientID, null, null, DebugButtonSmall, DebugButtonLarge);
            DedectionTestButton.OnExecute += delegate (NameValueMap context)
            {
                Wizard.WizardUtilities.DetectWheels(Utilities.GUI.SkeletonBase, Wizard.WizardData.WizardDriveTrain.WESTERN, 6);
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
            LegacyInterchange.LegacyEvents.RobotModified += new Action( () => { pendingChanges = true; } );
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
            Marshal.ReleaseComObject(AsmDocument);
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
                EnvironmentEnabled = false;
                EndExporter();
            }
            else
            {
                EnvironmentEnabled = true;
                StartExporter();

            }
        }

        /// <summary>
        /// Gets the assembly document and makes the <see cref="DockableWindows"/>
        /// </summary>
        private void StartExporter()
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

            PreviewRobotButton.Enabled = false;
            SaveAsButton.Enabled = false;
            SaveButton.Enabled = false;
        }

        /// <summary>
        /// Disposes of some COM objects and exits the environment
        /// </summary>
        private void EndExporter()
        {
            AsmDocument = null;
            Utilities.DisposeDockableWindows();
            ChildHighlight = null;

            foreach(var doc in DisabledCommandDocuments)
            {
                doc.Key.DisabledCommandList.Remove(doc.Value);
            }
        }
        #endregion

        #region Event Callbacks and Button Commands
        #region Application, Document, UI Event Handlers
        /// <summary>
        /// Makes the dockable windows invisible when the document switches. This avoids data loss.
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
            HandlingCode = HandlingCodeEnum.kEventNotHandled;
        }

        /// <summary>
        /// Disables the environment button if you aren't in an assembly document.
        /// </summary>
        /// <param name="DocumentObject"></param>
        /// <param name="BeforeOrAfter"></param>
        /// <param name="Context"></param>
        /// <param name="HandlingCode"></param>
        private void ApplicationEvents_OnActivateDocument(_Document DocumentObject, EventTimingEnum BeforeOrAfter, NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {
            if (DocumentObject is PartDocument Part)
            {
                Part.DisabledCommandList.Add(MainApplication.CommandManager.ControlDefinitions["BxD:RobotExporter:Environment"]);
            }
            else if (DocumentObject is PresentationDocument Presentation)
            {
                Presentation.DisabledCommandList.Add(MainApplication.CommandManager.ControlDefinitions["BxD:RobotExporter:Environment"]);
            }
            else if (DocumentObject is DrawingDocument Drawing)
            {
                Drawing.DisabledCommandList.Add(MainApplication.CommandManager.ControlDefinitions["BxD:RobotExporter:Environment"]);
            }
            else if (DocumentObject.Equals(AsmDocument) && HiddenExporter)
            {
                Utilities.ShowDockableWindows();
                HiddenExporter = false;
            }
            else if (!DocumentObject.Equals(AsmDocument) && HiddenExporter)
            {
                DisabledCommandDocuments.Add((AssemblyDocument)DocumentObject, ((AssemblyDocument)DocumentObject).DisabledCommandList.Count);
                ((AssemblyDocument)DocumentObject).DisabledCommandList.Add(MainApplication.CommandManager.ControlDefinitions["BxD:RobotExporter:Environment"]);
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

        //Begin
        /// <summary>
        /// Opens the <see cref="Forms.ChooseExportModeForm"/> and prompts the user to select an export mode.
        /// </summary>
        /// <param name="Context"></param>
        private void BeginGenericExport_OnExecute(NameValueMap Context)
        {
            Forms.ChooseExportModeForm exportChoose = new Forms.ChooseExportModeForm();
            exportChoose.ShowDialog();
        }

        /// <summary>
        /// Opens the <see cref="LiteExporterForm"/> through <see cref="Utilities.GUI"/>
        /// </summary>
        /// <param name="Context"></param>
        public void BeginAdvancedExport_OnExecute(NameValueMap Context)
        {
            if ((!PendingChanges || this.WarnUnsaved()) && Utilities.GUI.ExportMeshes())
            {
                PreviewRobotButton.Enabled = true;
                SaveAsButton.Enabled = true;
                SaveButton.Enabled = true;
                pendingChanges = false;
            }
            else if (Utilities.GUI.SkeletonBase != null)
            {
                PreviewRobotButton.Enabled = true;
                SaveAsButton.Enabled = true;
                SaveButton.Enabled = true;
            }
        }

        /// <summary>
        /// Opens the <see cref="LiteExporterForm"/> through <see cref="Utilities.GUI"/>, then opens the <see cref="Wizard.WizardForm"/>
        /// </summary>
        /// <param name="Context"></param>
        public void BeginWizardExport_OnExecute(NameValueMap Context)
        {
            if ((!PendingChanges || this.WarnUnsaved()) && Utilities.GUI.ExportMeshes())
            {
                PreviewRobotButton.Enabled = true;
                SaveAsButton.Enabled = true;
                SaveButton.Enabled = true;

                Wizard.WizardForm wizard = new Wizard.WizardForm();
                Utilities.HideDockableWindows();
                wizard.ShowDialog();
                Utilities.ShowDockableWindows();
                Utilities.GUI.RobotSave();
            }
            else if (Utilities.GUI.SkeletonBase != null)
            {
                PreviewRobotButton.Enabled = true;
                SaveAsButton.Enabled = true;
                SaveButton.Enabled = true;

                Wizard.WizardForm wizard = new Wizard.WizardForm();
                Utilities.HideDockableWindows();
                wizard.ShowDialog();
                Utilities.ShowDockableWindows();
                Utilities.GUI.RobotSave();
            }
        }

        /// <summary>
        /// Opens the <see cref="Wizard.OneClickExportForm"/> which allows for a super easy exporting of a robot
        /// </summary>
        /// <param name="Context"></param>
        public void BeginOneClickExport_OnExecute(NameValueMap Context)
        {
            Wizard.OneClickExportForm oneClickExportForm = new Wizard.OneClickExportForm();
            if((!PendingChanges || this.WarnUnsaved()) && (oneClickExportForm.ShowDialog() == DialogResult.OK))
            {
                Utilities.GUI.RobotSave();
            }
        }

        /// <summary>
        /// Opens a <see cref="FolderBrowserDialog"/> and prompts the user to select a robot folder. 
        /// Note: soon this should be replaced with an <see cref="OpenFileDialog"/> when the old format is merged into one file.
        /// </summary>
        /// <param name="Context"></param>
        private void LoadExportedRobotButton_OnExecute(NameValueMap Context)
        {
            if ((!PendingChanges || this.WarnUnsaved()) && Utilities.GUI.OpenExisting(ValidateAssembly))
            {
                PreviewRobotButton.Enabled = true;
                SaveAsButton.Enabled = true;
                SaveButton.Enabled = true;
            }
        }


        //File
        /// <summary>
        /// Opens a standalone robot viewer to that will display joint information and such
        /// </summary>
        /// <param name="Context"></param>
        private void PreviewRobotButton_OnExecute(NameValueMap Context)
        {
            Utilities.GUI.PreviewRobot(Properties.Settings.Default.SaveLocation);
        }

        /// <summary>
        /// Saves the active robot to the active directory
        /// </summary>
        /// <param name="Context"></param>
        private void SaveButton_OnExecute(NameValueMap Context)
        {
            Utilities.GUI.RobotSave();
        }

        /// <summary>
        /// Opens the name robot menu again and prompts the user to save the robot to another location.
        /// </summary>
        /// <param name="Context"></param>
        private void SaveAsButton_OnExecute(NameValueMap Context)
        {
            Utilities.GUI.RobotSaveAs();
        }


        //Settings
        /// <summary>
        /// Opens the <see cref="PluginSettingsForm"/> form to allow the user to customize their experience
        /// </summary>
        /// <param name="Context"></param>
        private void ExporterSettings_OnExecute(NameValueMap Context)
        {
            Utilities.GUI.SettingsExporter_OnClick(this, null);
        }


        //Help
        /// <summary>
        /// Also opens the help page
        /// </summary>
        private void HelpButton_OnExecute(NameValueMap Context)
        {
            Process.Start("http://bxd.autodesk.com/tutorial-robot.html");
        }

        /// <summary>
        /// Opens the help page on bxd.autodesk.com. This is the callback used for all OnHelp events.
        /// </summary>
        /// <param name="Context"></param>
        /// <param name="HandlingCode"></param>
        private void _OnHelp(NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {
            Process.Start("http://bxd.autodesk.com/tutorial-robot.html");
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
            Properties.Settings.Default.Save();
        } 
        #endregion
        #endregion

        #region Miscellaneous Methods and Nested Classes
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

        public bool WarnUnsaved()
        {
            switch (MessageBox.Show("Would you like to save your robot?", "Save", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Yes:
                    SaveButton_OnExecute(null);
                    return true;
                case DialogResult.No:
                    return true;
                case DialogResult.Cancel:
                    return false;
            }
            return false;
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