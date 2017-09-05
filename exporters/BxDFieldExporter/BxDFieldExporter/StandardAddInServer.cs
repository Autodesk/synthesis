using System;
using System.Runtime.InteropServices;
using Inventor;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using FieldExporter;
using System.Threading.Tasks;

namespace BxDFieldExporter
{
    //User Defined Interface Exposed through the Add-In Automation property
    public interface IAutomationInterface
    {
        void SetDone(bool isDone);
        bool GetDone();
        void SetCancel(bool isCancel);
        bool GetCancel();
    }
    //TLDR: exports the field

    [GuidAttribute("e50be244-9f7b-4b94-8f87-8224faba8ca1")]
    public partial class StandardAddInServer : Inventor.ApplicationAddInServer, IAutomationInterface
    {
        // all the global variables
        #region variables
        // Inventor application object.
        public static Inventor.Application InventorApplication;// the main inventor application
        [DllImport("user32.dll")]
        public static extern ushort GetKeyState(short key);
        ClientNodeResource oRsc;// client resources that buttons will use
        static Document nativeDoc;
        static bool runOnce;
        public static TaskCompletionSource<bool> task = null;
        EnvironmentManager envMan;
        static RibbonPanel ComponentControls;// the ribbon panels that the buttons will be a part of
        static RibbonPanel AddItems;
        static RibbonPanel RemoveItems;
        static RibbonPanel ExporterControl;
        static ButtonDefinition beginExporter;
        static ButtonDefinition addNewComponent;
        static ButtonDefinition editComponent;
        static ButtonDefinition addAssembly;
        static ButtonDefinition addPart;// contain the buttons that the user can interact with
        static ButtonDefinition removePart;
        static ButtonDefinition removeAssembly;
        static ButtonDefinition exportField;
        static ButtonDefinition removeComponent;
        static ButtonDefinition ContextDelete;
        Inventor.Environment oNewEnv;
        private static bool done;
        private static bool cancel = false;
        static Random rand;// random number genator that can create internal ids
        static int InternalID = 0;
        static ArrayList FieldComponents;// arraylist of all the field properties the user has set
        public static FieldDataComponent selectedComponent;// the current component that the user is editing
        static BrowserPanes oPanes;// all the browser panes in the active doc
        object resultObj;
        object other;// some objects to help in searching for ref ids
        object context;
        bool closing;
        static Inventor.BrowserPane oPane;// the application's browser pane
        UserInputEvents UIEvent;// the uievents that react to selections
        static Document oDoc;// a doc to add new highlight sets to
        static HighlightSet oSet;// highlight set that represents the selection
        static HighlightSet partSet; //highlight set for current selected parts 
        Inventor.UserInputEventsSink_OnSelectEventHandler click_OnSelectEventDelegate;// handles the selection events
        Inventor.UserInterfaceEventsSink_OnEnvironmentChangeEventHandler enviroment_OnChangeEventDelegate;
        static bool inExportView;// boolean to help in detecting wether or not to react to an event based on wether or not the application is exporting
        static ComponentPropertiesForm form;// form for inputting different properties of the component
        static String m_ClientId;// string the is the id of the application
        static Object currentSelected;// the current component that the user is editing, needed for the unselection stuff
        static bool found;// boolean to help in searching for objects and the corrosponding actions
        private static uint fieldID = 0; //Numerical ID to associate STLs with the field Property
        static CommandControls ComControls;
        #endregion
        public StandardAddInServer()
        {
        }
        // methods required by inventor
        #region ApplicationAddInServer Members
        // called when inventor starts the add in
        public void Activate(Inventor.ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            // This method is called by Inventor when it loads the addin.
            // The AddInSiteObject provides access to the Inventor Application object.
            // The FirstTime flag indicates if the addin is loaded for the first time.
            try
            {
                m_ClientId = "    ";// create a new client id for the buttons and such
                inExportView = false;// say that we aren't in export view
                InventorApplication = addInSiteObject.Application;// get the inventor object
                closing = false;
                FieldComponents = new ArrayList();// clear the field Component array
                form = new ComponentPropertiesForm();// init the component form to enter data into
                AddParallelEnvironment();
                UIEvent = InventorApplication.CommandManager.UserInputEvents;// get the application's userinput events object
                click_OnSelectEventDelegate = new UserInputEventsSink_OnSelectEventHandler(UIEvents_OnSelect);// make a new ui event reactor
                UIEvent.OnSelect += click_OnSelectEventDelegate;// add the event reactor to the onselect
                UIEvent.OnContextMenu += UIEvent_OnContextMenu;
                InventorApplication.ApplicationEvents.OnActivateDocument += ApplicationEvents_OnActivateDocument;
            }
            catch (Exception)
            {
            }
        }
        // called when the inventor is shutting down the add in
        public void Deactivate()
        {
            // This method is called by Inventor when the AddIn is unloaded.
            // The AddIn will be unloaded either manually by the user or
            // when the Inventor session is terminated

            // TODO: Add ApplicationAddInServer.Deactivate implementation


            // Release objects.
            try
            {
                WriteFieldComponentNames();
                foreach (FieldDataComponent data in FieldComponents)
                {
                    WriteSaveFieldComponent(data);
                }
                InventorApplication.ActiveDocument.Save();
            }
            catch (Exception) { }
            InventorApplication = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        // old method, needed by inventor
        public void ExecuteCommand(int commandID)
        {
            // Note:this method is now obsolete, you should use the 
            // ControlDefinition functionality for implementing commands.
        }
        // a method to help with addin apis
        public object Automation {
            // This property is provided to allow the AddIn to expose an API 
            // of its own to other programs. Typically, this  would be done by
            // implementing the AddIn's API interface in a class and returning 
            // that class object through this property.

            get {
                // TODO: Add ApplicationAddInServer.Automation getter implementation
                return this;
            }
        }

        #endregion

        #region API exposed methods
        public void SetDone(bool isDone)
        {
            done = isDone;
        }

        public bool GetDone()
        {
            return done;
        }

        public void SetCancel(bool isCancel)
        {
            cancel = isCancel;
        }

        public bool GetCancel()
        {
            return cancel;
        }
        #endregion


        private void ApplicationEvents_OnActivateDocument(_Document DocumentObject, EventTimingEnum BeforeOrAfter, NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {

            if (DocumentObject is PartDocument doc)
            {
                doc.DisabledCommandList.Add(InventorApplication.CommandManager.ControlDefinitions["BxD:RobotExporter:Environment"]);
            }
            else if (DocumentObject is PresentationDocument doc1)
            {
                doc1.DisabledCommandList.Add(InventorApplication.CommandManager.ControlDefinitions["BxD:RobotExporter:Environment"]);
            }
            else if (DocumentObject is DrawingDocument doc2)
            {
                doc2.DisabledCommandList.Add(InventorApplication.CommandManager.ControlDefinitions["BxD:RobotExporter:Environment"]);
            }
            HandlingCode = HandlingCodeEnum.kEventNotHandled;
        }
        // called when the exporter starts
        public void StartExport_OnExecute(NameValueMap Context)
        {
            try
            {
                if (!inExportView)
                {
                    nativeDoc = InventorApplication.ActiveDocument;
                    envMan = ((AssemblyDocument)InventorApplication.ActiveDocument).EnvironmentManager;
                    SetAllButtons(true); //Activates all the buttons in the field exporting environment
                    AssemblyDocument asmDoc = (AssemblyDocument)InventorApplication.ActiveDocument;// get the active assembly document
                    BrowserNodeDefinition oDef; // create browsernodedef to use to add browser node to the pane
                    oDoc = InventorApplication.ActiveDocument;// get the active document in inventor
                    oPanes = oDoc.BrowserPanes;// get the browserpanes to add
                    oSet = oDoc.CreateHighlightSet();// create a highlightset to add the selected occcurences to
                    oSet.Color = InventorApplication.TransientObjects.CreateColor(100, 0, 200);

                    partSet = oDoc.CreateHighlightSet();
                    partSet.Color = InventorApplication.TransientObjects.CreateColor(100, 0, 200);
                    rand = new Random();// create new random num generator to generate internal ids

                    try
                    {// if no browser pane previously created then create a new one
                        ClientNodeResources oRscs = oPanes.ClientNodeResources;
                        oRsc = oRscs.Add(m_ClientId, 1, null);// creat new client node resources
                        oDef = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition("Field Components", 3, null);// create the top node for the browser pane
                        oPane = oPanes.AddTreeBrowserPane("Field Exporter", m_ClientId, oDef);// add a new tree browser
                        oPane.Activate();// make the pane be shown to the user
                    }
                    catch (Exception)// we will assume that if the above method fails it is because there is already a browser node
                    {
                        bool found = false;
                        foreach (BrowserPane pane in oPanes)// iterate over the panes in the document
                        {
                            if (pane.Name.Equals("Field Exporter"))// if the pane has the correct name then we assume it is what we are looking for
                            {

                                oPane = pane;// if we have found the correct node then use 
                                oPane.Visible = true;// make the pane visible to the user
                                oPane.Activate();// make the pane the shown one
                                found = true;// tell the program we have found a previous top node
                            }
                        }
                        if (!found)
                        {
                            oDef = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition("Field Components", 3, oRsc);// if the pane was created but the node wasnt then init a node 
                            oPane = oPanes.AddTreeBrowserPane("Field Exporter", m_ClientId, oDef);// add a top node to the tree browser
                        }
                    }

                    oPane.Refresh();
                    ReadSaveFieldData();// read the save so the user doesn't loose any previous work
                    //TimerWatch();// begin the timer watcher to detect deselect
                }
                else
                {
                    MessageBox.Show("Please close out of the robot exporter in the other assembly");
                }
            }
            catch (Exception)
            {
            }
        }

        // called when the environments switch
        public void OnEnvironmentChange(Inventor.Environment environment, EnvironmentStateEnum EnvironmentState, EventTimingEnum BeforeOrAfter, NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {
            if (environment.Equals(oNewEnv) && EnvironmentState.Equals(EnvironmentStateEnum.kActivateEnvironmentState) && !closing)
            {
                closing = true;
                StartExport_OnExecute(null);
            }
            else if (environment.Equals(oNewEnv) && EnvironmentState.Equals(EnvironmentStateEnum.kTerminateEnvironmentState) && closing)
            {
                closing = false;
                inExportView = false;
                //cancelExporter_OnExecute(null);
            }
            HandlingCode = HandlingCodeEnum.kEventNotHandled;
            if (environment.InternalName.Equals("BxD:FieldExporter:Environment"))
            {
                BrowserNodeIcons(); //sets topnode icon if environment is changed to field exporter
            }
        }
        /// <summary>
        /// Prepares the exportation environment, generates Icons, descriptions, and tooltips for each button
        /// </summary>
        public void AddParallelEnvironment()
        {
            try
            {
                //Converts the Bitmaps from the resource folder into IPictureDisps that are usable by Inventor
                #region Make Icons
                stdole.IPictureDisp startExporterIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.StartExporter16));
                stdole.IPictureDisp startExporterIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.StartExporter32));

                stdole.IPictureDisp addNewComponentIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.AddNewType16));
                stdole.IPictureDisp addNewComponentIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.AddNewType32));

                stdole.IPictureDisp removeComponentIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.RemoveType16));
                stdole.IPictureDisp removeComponentIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.RemoveType32));

                stdole.IPictureDisp editComponentIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.EditType16));
                stdole.IPictureDisp editComponentIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.EditType32));

                stdole.IPictureDisp addAssemblyIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.AddAssembly16));
                stdole.IPictureDisp addAssemblyIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.AddAssembly32));

                stdole.IPictureDisp addPartIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.AddNewPart16));
                stdole.IPictureDisp addPartIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.AddNewPart32));

                stdole.IPictureDisp removeAssemblyIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.RemoveAssembly16));
                stdole.IPictureDisp removeAssemblyIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.RemoveAssembly32));

                stdole.IPictureDisp removePartIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.RemoveSubAssembly16));
                stdole.IPictureDisp removePartIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.RemoveSubAssembly32));

                stdole.IPictureDisp exportFieldIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.ExportField16));
                stdole.IPictureDisp exportFieldIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.ExportField32));

                stdole.IPictureDisp ttAddNewComponent = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.TTAddNewComponent));
                stdole.IPictureDisp ttRemoveComponent = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.TTRemoveComponent));
                stdole.IPictureDisp ttComponentProperties = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.TTComponentProperties));
                stdole.IPictureDisp ttAddAssembly = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.TTAddAssembly));
                stdole.IPictureDisp ttAddPart = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.TTAddPart));
                stdole.IPictureDisp ttRemovePart = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.TTRemovePart));
                stdole.IPictureDisp ttRemoveAssembly = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.TTRemoveAssembly));
                stdole.IPictureDisp ttExportField = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.TTExportField));
                #endregion

                // Get the Environments collection
                Environments oEnvironments = InventorApplication.UserInterfaceManager.Environments;

                // Create a new environment
                oNewEnv = oEnvironments.Add("Field Exporter", "BxD:FieldExporter:Environment", null, startExporterIconSmall, startExporterIconLarge);

                // Get the ribbon associated with the assembly environment
                Ribbon oAssemblyRibbon = InventorApplication.UserInterfaceManager.Ribbons["Assembly"];

                // Create contextual tabs and panels within them
                #region Create Buttons
                RibbonTab oContextualTabOne = oAssemblyRibbon.RibbonTabs.Add("Field Exporter", "BxD:FieldExporter:RibbonTab", "ClientId123", "", false, true);
                ComponentControls = oContextualTabOne.RibbonPanels.Add("Component Controls", "BxD:FieldExporter:ComponentControls", "{e50be244-9f7b-4b94-8f87-8224faba8ca1}");
                AddItems = oContextualTabOne.RibbonPanels.Add("Add Items", "BxD:FieldExporter:AddItems", "{e50be244-9f7b-4b94-8f87-8224faba8ca1}");
                AddItems.Reposition("BxD:FieldExporter:ComponentControls", false);
                RemoveItems = oContextualTabOne.RibbonPanels.Add("Remove Items", "BxD:FieldExporter:RemoveItems", "{e50be244-9f7b-4b94-8f87-8224faba8ca1}");
                RemoveItems.Reposition("BxD:FieldExporter:AddItems", false);
                ExporterControl = oContextualTabOne.RibbonPanels.Add("Robot Exporter Control", "BxD:FieldExporter:ExporterControl", "{e50be244-9f7b-4b94-8f87-8224faba8ca1}");// inits the part panels               

                ControlDefinitions controlDefs = InventorApplication.CommandManager.ControlDefinitions;// get the controls for Inventor
                beginExporter = controlDefs.AddButtonDefinition("Start Exporter", "BxD:FieldExporter:StartExporter", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, "Starts the field exporter", "Yay lets start!", startExporterIconSmall, startExporterIconLarge, ButtonDisplayEnum.kAlwaysDisplayText);
                beginExporter.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(StartExport_OnExecute);

                addNewComponent = controlDefs.AddButtonDefinition(" Add New Component ", "BxD:FieldExporter:AddNewComponent", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null, addNewComponentIconSmall, addNewComponentIconLarge, ButtonDisplayEnum.kAlwaysDisplayText);
                addNewComponent.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(AddNewComponent_OnExecute);
                ToolTip(addNewComponent, "Creates a physics object to hold parts or assemblies.",
                    "For example, \"floor\", \"wall.\" or \"airship.\"After creating a component, you can add parts or assemblies to the component using either the \"Add New Part\" or \"Add New Assembly\" button.",
                    ttAddNewComponent, "Add New Component");

                removeComponent = controlDefs.AddButtonDefinition(" Remove Component ", "BxD:FieldExporter:RemoveComponent", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null, removeComponentIconSmall, removeComponentIconLarge);
                removeComponent.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(RemoveComponent_OnExecute);
                ToolTip(removeComponent, "Removes a component from the field components hierarchy.",
                    "Removing the component will ungroup all parts or assemblies attached to the component.",
                    ttRemoveComponent, "Remove Component");

                editComponent = controlDefs.AddButtonDefinition(" Edit Component Properties ", "BxD:FieldExporter:EditComponentProperties", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null, editComponentIconSmall, editComponentIconLarge);// init the button
                editComponent.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(EditComponentProperites_OnExecute);// add the reacting method to the button
                ToolTip(editComponent, "Edit properties including collider type, friction and dynamic.",
                    @"Collider types include box (flat surfaces), sphere (round surfaces), and mesh (uneven or organic surfaces.) 

Use the friction slider to adjust the friction coefficient in the simulator. 

Checking “Dynamic” enables an object to be moved in the simulator. For example, check “Dynamic” for objects like balls or other game pieces. Do not check “Dynamic” for static objects like the floor and walls.",
                    ttComponentProperties, "Edit Component Properties");

                addAssembly = controlDefs.AddButtonDefinition(" Add New Assembly ", "BxD:FieldExporter:AddNewItem", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null, addAssemblyIconSmall, addAssemblyIconLarge);
                addAssembly.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(AddNewAssemblies_OnExecute);
                ToolTip(addAssembly, "Adds an assembly to a field component.",
                    "Click on a field component in the Field Exporter hierarchy so that it is highlighted. Click \"Add New Assembly\", and select the assembly to add to the component. To add multiple assemblies to one component, repeat the process.",
                    ttAddAssembly, "Add Assembly");

                addPart = controlDefs.AddButtonDefinition(" Add New Part ", "BxD:FieldExporter:AddNewPart", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null, addPartIconSmall, addPartIconLarge);
                addPart.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(AddNewPart_OnExecute);
                ToolTip(addPart, "Adds a part to a field component.",
                    "Click on a field component in the Field Exporter hierarchy so that it is highlighted. Click “Add New Part”, and select the part to add to the component. To add multiple part to one component, repeat the process.",
                    ttAddPart, "Add New Part");

                removeAssembly = controlDefs.AddButtonDefinition(" Remove Assembly ", "BxD:FieldExporter:RemoveAssembly", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null, removeAssemblyIconSmall, removeAssemblyIconLarge);
                removeAssembly.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(RemovePartAssembly_OnExecute);
                ToolTip(removeAssembly, "Removes an assembly from a field component.",
                    "Click on a field component in the Field Exporter hierarchy so that it is highlighted. Click \"Remove Assembly\", and select the assembly to remove from the component. To remove multiple assemblies from one component, repeat the process",
                    ttRemoveAssembly, "Remove Assembly");

                removePart = controlDefs.AddButtonDefinition(" Remove Part ", "BxD:FieldExporter:RemovePart", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null, removePartIconSmall, removePartIconLarge);
                removePart.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(RemovePartAssembly_OnExecute);
                ToolTip(removePart, "Removes a part from a field component.",
                   "Click on a field component in the Field Exporter hierarchy so that it is highlighted. Click “Remove Part”, and select the part to remove from the component. To remove multiple parts from one component, repeat the process.",
                   ttRemovePart, "Remove Assembly");

                exportField = controlDefs.AddButtonDefinition("Export Field", "BxD:FieldExporter:ExportField", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null, exportFieldIconSmall, exportFieldIconLarge);
                exportField.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(ExportField_OnExecute);
                ToolTip(exportField, "Exports the field for use in the Synthesis simulation.",
                    "After adding all components of a field, and populating those components with parts or assemblies, export the field. The field will be saved to Documents/Synthesis/Fields and can be accessed through Synthesis.",
                    ttExportField, "Export Field");
                #endregion

                ContextDelete = controlDefs.AddButtonDefinition("Delete", "BxD:FieldExporter:ContextMenu:Delete", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId);
                ContextDelete.OnExecute += ContextDelete_OnExecute;

                // add buttons to the part panels
                ComponentControls.CommandControls.AddButton(addNewComponent, true, true);
                ComponentControls.CommandControls.AddButton(removeComponent, true, true);
                ComponentControls.CommandControls.AddButton(editComponent, true, true);
                AddItems.CommandControls.AddButton(addAssembly, true, true);
                AddItems.CommandControls.AddButton(addPart, true, true);
                RemoveItems.CommandControls.AddButton(removeAssembly, true, true);
                RemoveItems.CommandControls.AddButton(removePart, true, true);
                ExporterControl.CommandControls.AddButton(exportField, true, true);
                UserInterfaceEvents UIEvents = InventorApplication.UserInterfaceManager.UserInterfaceEvents;

                enviroment_OnChangeEventDelegate = new UserInterfaceEventsSink_OnEnvironmentChangeEventHandler(OnEnvironmentChange);
                UIEvents.OnEnvironmentChange += enviroment_OnChangeEventDelegate;

                // Make the "SomeAnalysis" tab default for the environment
                oNewEnv.DefaultRibbonTab = "BxD:FieldExporter:RibbonTab";

                // Get the collection of parallel environments and add the new environment
                EnvironmentList oParEnvs = InventorApplication.UserInterfaceManager.ParallelEnvironments;

                oParEnvs.Add(oNewEnv);

                // Make the new parallel environment available only within the assembly environment
                // A ControlDefinition is automatically created when an environment is added to the
                // parallel environments list. The internal name of the definition is the same as
                // the internal name of the environment.
                ControlDefinition oParallelEnvButton = InventorApplication.CommandManager.ControlDefinitions["BxD:FieldExporter:Environment"];

                Inventor.Environment oEnv;
                oEnv = oEnvironments["BxD:FieldExporter:Environment"];
                oEnv.DisabledCommandList.Add(oParallelEnvButton);


            }
            catch (Exception)
            {
            }
        }
        //Called whenever a node is deleted manually so that it's cleared from internal lists as well
        private void ContextDelete_OnExecute(NameValueMap Context)
        {
            if (oPane.TopNode.Selected)
            {
                return;
            }
            foreach (BrowserNode Component in oPane.TopNode.BrowserNodes)
            {
                if (Component.Selected || !AreNodesSelected())
                {
                    RemoveComponent_OnExecute(Context);
                    return;
                }
                foreach (BrowserNode PartAsm in Component.BrowserNodes)
                {
                    if (PartAsm.Selected)
                    {
                        RemovePartAssembly_OnExecute(Context);
                        return;
                    }
                }
            }
            SetAllButtons(true);
        }
        //Passes the component property information to the exporation process
        private void UpdateLegacy(NameValueMap Context = null)
        {
            LegacyInterchange.PropSets = LegacyUtilities.GetLegacyProps(FieldComponents);
        }

        //Sets the tool tips
        public void ToolTip(ButtonDefinition button, string description, string expandedDescription, stdole.IPictureDisp picture, string title)
        {
            button.ProgressiveToolTip.Description = description;
            button.ProgressiveToolTip.ExpandedDescription = expandedDescription;
            button.ProgressiveToolTip.Image = picture;
            button.ProgressiveToolTip.IsProgressive = true;
            button.ProgressiveToolTip.Title = title;
        }

        //Sets the tool tips without the picture
        public void ToolTip(ButtonDefinition button, string description, string expandedDescription, string title)
        {
            button.ProgressiveToolTip.Description = description;
            button.ProgressiveToolTip.ExpandedDescription = expandedDescription;
            button.ProgressiveToolTip.IsProgressive = true;
            button.ProgressiveToolTip.Title = title;
        }

        // reacts to a selection
        private void UIEvents_OnSelect(ObjectsEnumerator JustSelectedEntities, ref ObjectCollection MoreSelectedEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
        {
            if (SelectionDevice == SelectionDeviceEnum.kGraphicsWindowSelection && inExportView)
            {// if the selection is from the graphical interface and the exporter is active
                oSet.Clear();// clear the highlight set to add a new component to the set
                foreach (object sel in JustSelectedEntities)
                {//looks at all things selected
                    if (sel is ComponentOccurrence)
                    {// react only if sel is a part/assembly
                        foreach (BrowserNode node in oPane.TopNode.BrowserNodes)
                        {
                            foreach (BrowserNode PartAsm in node.BrowserNodes)
                            {
                                if (PartAsm.BrowserNodeDefinition.Label == ((ComponentOccurrence)sel).Name)
                                {
                                    PartAsm.DoSelect();
                                }
                            }
                        }
                    }
                    else if (sel is UserCoordinateSystem)
                    {
                        foreach (UserCoordinateSystem ucs in (((AssemblyDocument)InventorApplication.ActiveDocument).ComponentDefinition).UserCoordinateSystems)
                        {
                            foreach (BrowserNode n in oPane.TopNode.BrowserNodes)
                            {// looks at all the browser nodes in the top node
                                if (n.BrowserNodeDefinition.Label.Equals(((UserCoordinateSystem)sel).Name))
                                {// if the browsernode is the same as the Components node then react
                                    n.DoSelect();// select the proper node
                                }
                            }
                        }
                    }
                }
            }
            else if (SelectionDevice == SelectionDeviceEnum.kBrowserSelection && inExportView)
            {// if the selection is from the browser and the exporter is active, cool feature is that browsernode.DoSelect() calls this so I do all the reactions in here
                foreach (object sel in JustSelectedEntities)
                {//looks at all things selected
                    if (sel is BrowserNodeDefinition)
                    {// react only if sel is a browsernodedef
                        foreach (FieldDataComponent f in FieldComponents)
                        {// looks at all the components of parts
                            if (f.same(((BrowserNodeDefinition)sel)))
                            {// if the browsernode is the same as a the Component's node
                                selectedComponent = f;// set the selected Component for the rest of the code to interact with
                                foreach (ComponentOccurrence o in selectedComponent.CompOccs)
                                {
                                    oSet.AddItem(o);
                                }
                            }
                        }
                        //If it isn't a Component

                    }
                }
            }
        }

        private void UIEvent_OnContextMenu(SelectionDeviceEnum SelectionDevice, NameValueMap AdditionalInfo, CommandBar CommandBar)
        {
            if (SelectionDevice == SelectionDeviceEnum.kBrowserSelection)
            {
                CommandBar.Controls.AddButton(ContextDelete, 1);
            }
        }

        // starts a timer to react to events in the browser/ graphics interface
        static bool RightDoc;
        private void TimerWatch()
        {
            try
            {
                System.Timers.Timer aTimer = new System.Timers.Timer();// creates a new timer
                aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);// handles an elasped time event
                aTimer.Interval = 250;// set time timer reaction interval to 1/4 of a second, we do this to get regular checking without lagging out the computer
                aTimer.AutoReset = true;// auto restarts the timer after the 1/4 second interval
                aTimer.Enabled = true;// starts the timer
                RightDoc = true;
            }
            catch (Exception)
            {
            }
        }
        //Checks whether or not the passed key was pressed
        public static bool CheckKeyPressed(Keys key)
        {
            return ((GetKeyState((short)key) & 0x80) == 0x80);
        }

        // reacts to the timer elapsed event, this allows us to select and unselect things as needed
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            found = false;
            if (!CheckKeyPressed(Keys.ShiftKey) || !CheckKeyPressed(Keys.ControlKey))
            {
                if (!runOnce)
                {
                    done = true;
                    InventorApplication.CommandManager.StopActiveCommand();
                    runOnce = true;
                    SetAllButtons(true);
                }
            }
            foreach (BrowserNode node in oPane.TopNode.BrowserNodes)
            {// looks through all the nodes under the top node
                if (node.Selected)
                {// if the node is seleted
                    foreach (FieldDataComponent t in FieldComponents)
                    {// looks at all the components in the fieldComponents
                        if (t.same(node.BrowserNodeDefinition))
                        {// if t is part of that browser node
                            if (!currentSelected.Equals(t))

                            {// is the selected node is no longer selected 
                                found = true;// tell the program it found the node
                                oSet.Clear();// clear the highlighted set in prep to add new occurrences
                                foreach (ComponentOccurrence io in t.CompOccs)
                                {// looks at the occurences that are part of the component
                                    oSet.AddItem(io);// add the occurence to the highlighted set
                                }
                                currentSelected = t;// change the selected dataComponent
                            }
                        }
                    }
                }
            }
            if (!found)
            {// if the program didn't find any selected node then assume that the user deselected 
                oSet.Clear();// clear the set because the user doesn't have anything selected
            }
            if (!InventorApplication.ActiveDocument.InternalName.Equals(nativeDoc.InternalName))
            {
                if (RightDoc)
                {
                    RightDoc = false;
                    oPane.Visible = false;// Hide the browser pane
                }
            }
            else
            {
                if (!RightDoc)
                {
                    RightDoc = true;

                    oPane.Visible = true;// Hide the browser pane
                    oPane.Activate();
                }

            }
        }
        // adds a new fieldDataComponent to the array and to the browser pane
        public static BrowserNodeDefinition AddComponent(string name)
        {
            BrowserNodeDefinition def = null;// creates a browsernodedef to be used when creating a new browsernode, null so if adding the node fails the code doesn't freak out
            try
            {
                bool same = false;// used to prevent duplicate Component names because they will not save
                foreach (FieldDataComponent Component in FieldComponents)
                {// look at all the fielddata Components in data Components
                    if (Component.Name.Equals(name))
                    {// check to see if it is the same
                        same = true;// if it is then tell the code
                    }
                }
                if (!same)
                {// if there is no duplicate name then add the Component
                    int th = InternalID;// get the next random number for the browser node's internal id
                    InternalID++;
                    ClientNodeResources oNodeRescs; // creates a ClientNodeResourcess that we add the ClientNodeResource to
                    ClientNodeResource oRes = null;// creates a ClientNodeResource for adding the browsernode, needs to be null for some reason, idk
                    oNodeRescs = oPanes.ClientNodeResources;// set the ClientNodeResources the the active document's ClientNodeResources
                    try
                    {
                        stdole.IPictureDisp componentIcon =
                            PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.ComponentBrowserNode16));
                        oRes = oNodeRescs.Add("MYID", 3, componentIcon);// create a new ClientNodeResource to be used when you add the browser node
                    }
                    catch (Exception)
                    {// if the method fails then assume that there is already a ClientNodeResource
                        oRes = oPanes.ClientNodeResources.ItemById("MYID", 3);// get the ClientNodeResource by the name
                    }
                    def = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition(name, th, oRes);// creates a new browser node def for the field data
                    oPane.TopNode.AddChild(def);// add the browsernode to the topnode
                    FieldComponents.Add(new FieldDataComponent(def, fieldID));// add the new field data Component to the array and use the browsernodedef to refence the object to the browser node
                    fieldID++; //makes the ID spacing move up a unit

                    //Sets LegacyInterchange stuff
                }
                else
                {// if there is already something with the name
                    MessageBox.Show("Please choose a name that hasn't already been used");// tell the user to use a different name
                }
            }
            catch (Exception)
            {
            }
            return def;// returns the browsernodedef
        }

        // adds new property Component to the browser pane
        public static void AddNewComponent_OnExecute(NameValueMap Context)
        {
            SetAllButtons(false);
            // create a new enter name form
            EnterName form = new EnterName();
            //show the form to the user
            System.Windows.Forms.Application.Run(form);
            SetAllButtons(true);
        }

        //Removes components from the browser pane
        public void RemoveComponent_OnExecute(NameValueMap Context)
        {
            SetAllButtons(false);
            ArrayList selectedNodes = new ArrayList();
            String names = "";
            foreach (BrowserNode node in oPane.TopNode.BrowserNodes)
            {// looks through all the nodes under the top node
                if (node.Selected)
                {// if the node is seleted
                    foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
                    {
                        if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
                        {
                            selectedNodes.Add(node);
                            names += node.BrowserNodeDefinition.Label + " ";
                        }
                    }
                }
            }
            //Uses the name to determine if there has been a component selected or not
            if (names == "")
            {
                MessageBox.Show("ERROR: No components were selected", "Remove Component");
                SetAllButtons(true);
                return;
            }
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete Component: " + "\n" + names, "Remove Component", MessageBoxButtons.OKCancel);
            if (dialogResult == DialogResult.OK)
            {
                //Iterates through the list of browser nodes and deletes the one that matches the name of the selected node
                ArrayList FieldComponentsCopy = (ArrayList)FieldComponents.Clone();
                foreach (BrowserNode node in selectedNodes)
                {
                    foreach (FieldDataComponent f in FieldComponentsCopy)
                    {
                        if (f.same(node.BrowserNodeDefinition))
                        {
                            LegacyInterchange.RemoveComponent(node.BrowserNodeDefinition.Label);
                            FieldComponents.Remove(f);
                            node.Delete();
                        }
                    }
                }
            }
            SetAllButtons(true);
        }

        // edits the properties of the Component
        public static void EditComponentProperites_OnExecute(NameValueMap Context)
        {
            if (!CheckComponentsSel())
            {
                MessageBox.Show("ERROR: No component is selected", "Edit Component Properties");
                return;
            }
            SetAllButtons(false);
            //read from the temp save the proper field values
            form.readFromData(selectedComponent);
            //show a dialog for the user to enter in values
            form.ShowDialog();
            SetAllButtons(true);
        }

        //Opens the "Add Assembly(s)" and adds assemblies to a component
        public async void AddNewAssemblies_OnExecute(NameValueMap Context)
        {
            if (!CheckComponentsSel())
            {
                MessageBox.Show("ERROR: No component is selected", "Add Assembly...");
                return;
            }
            //Create form and show it
            AddAssembly form = new AddAssembly();
            form.Show();

            SetAllButtons(false);

            SetDone(false);
            SetCancel(false);
            int componentsAdded = 0; //Tracks how many components are added
            while (!GetDone())
            {
                try
                {
                    task = new TaskCompletionSource<bool>();
                    ComponentOccurrence selectedAssembly = null;
                    selectedAssembly = (ComponentOccurrence)InventorApplication.CommandManager.Pick(SelectionFilterEnum.kAssemblyOccurrenceFilter, "Select an assembly to add");
                    await task.Task;
                    SetAllButtons(true);
                    if (!GetCancel())
                    {
                        if (selectedAssembly != null)
                        {
                            foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// look at all the nodes under the top node
                            {
                                if (node.Selected)// find the selected node
                                {
                                    foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
                                    {
                                        if (t.same(node.BrowserNodeDefinition))// if the fieldDataComponent is from that browsernode then run
                                        {
                                            LegacyInterchange.AddComponents(node.BrowserNodeDefinition.Label, selectedAssembly);
                                            t.CompOccs.Add(selectedAssembly);// add the assembly occurence to the arraylist
                                            partSet.AddItem(selectedAssembly); //add the assembly occurence to a set that is highlighted in purple
                                            ClientNodeResources nodeRescs = oPanes.ClientNodeResources;
                                            ClientNodeResource nodeRes = null;
                                            try
                                            {
                                                stdole.IPictureDisp assemblyIcon =
                                                    PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.AssemblyIcon16));
                                                nodeRes = nodeRescs.Add(node.BrowserNodeDefinition.Label, 1, assemblyIcon);
                                            }
                                            catch
                                            {
                                                nodeRes = oPanes.ClientNodeResources.ItemById(node.BrowserNodeDefinition.Label, 1);
                                            }
                                            node.AddChild((BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition(selectedAssembly.Name, rand.Next(), nodeRes));
                                            node.DoSelect();
                                        }
                                    }
                                }
                            }
                            componentsAdded++; //Tracks how many components are added                        
                        }

                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }

            partSet.Clear(); //Clears the highlighted set

            SetAllButtons(true);
        }
        /// <summary>
        ///Opens the "Add Part(s)" window and adds parts to a component
        /// </summary>
        /// <param name="Context"></param>
        public async void AddNewPart_OnExecute(NameValueMap Context)
        {

            if (!CheckComponentsSel())
            {
                MessageBox.Show("ERROR: No component is selected", "Add Assembly...");
                return;
            }
            //Create form and show it
            AddPart form = new AddPart();
            form.Show();

            SetAllButtons(false);

            int componentsAdded = 0; //Tracks how many components are added
            SetCancel(false);
            SetDone(false);
            while (!GetDone())
            {
                try
                {
                    task = new TaskCompletionSource<bool>();
                    ComponentOccurrence selectedPart = null;
                    AssemblyDocument asmDoc = (AssemblyDocument)
                                 InventorApplication.ActiveDocument;
                    selectedPart = (ComponentOccurrence)InventorApplication.CommandManager.Pick// have the user select a part
                              (SelectionFilterEnum.kAssemblyLeafOccurrenceFilter, "Select a part to add");
                    await task.Task;

                    SetAllButtons(true);
                    if (!GetCancel())
                    {
                        if (selectedPart != null)
                        {
                            foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// look at all the nodes under the top node
                            {
                                if (node.Selected)// find the selected node
                                {
                                    foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
                                    {
                                        if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
                                        {
                                            LegacyInterchange.AddComponents(node.BrowserNodeDefinition.Label, selectedPart);
                                            t.CompOccs.Add(selectedPart);// add the part occurence to the arraylist
                                            partSet.AddItem(selectedPart); //add the part occurence to a set that is highlighted in purple
                                            ClientNodeResources nodeRescs = oPanes.ClientNodeResources;
                                            ClientNodeResource nodeRes = null;
                                            try
                                            {
                                                stdole.IPictureDisp partIcon =
                                                    PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.PartIcon16));
                                                nodeRes = nodeRescs.Add(node.BrowserNodeDefinition.Label, 2, partIcon);
                                            }
                                            catch (Exception e)
                                            {
                                                nodeRes = oPanes.ClientNodeResources.ItemById(node.BrowserNodeDefinition.Label, 1);
                                            }
                                            node.AddChild((BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition(selectedPart.Name, rand.Next(), nodeRes));
                                            node.DoSelect();
                                            //LegacyInterchange.CompPropertyDictionary.Add(selectedPart.Name, node.BrowserNodeDefinition.Label);
                                        }
                                    }
                                }
                            }
                            componentsAdded++; //Tracks how many components are added
                        }
                    }

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            partSet.Clear(); //Clears the highlighted set

            SetAllButtons(true);
        }

        //Reads the saved data
        private void ReadSaveFieldData()
        {
            try
            {
                ArrayList lis = new ArrayList();// creates an arraylist which will contain occurences to add to the field data Component
                byte[] refKey;// creates a byte[] to hold the refkeys
                PropertySets sets = InventorApplication.ActiveDocument.PropertySets;// gets a property sets of the document 
                other = null;
                context = null;// clears the object because inventor needs it that way
                resultObj = null;
                String name = "";// make name that can store the names of the browser nodes
                Char[] arr = { '¯', '\\', '_', '(', ':', '(', ')', '_', '/', '¯' };// contains the limiter of the strings so we can read them
                foreach (Inventor.PropertySet s in sets)
                {// looks at all the properties in the propertysets
                    if (s.DisplayName.Equals("Number of Folders"))
                    {// is the name is correct the assume it is what we are looking fos
                        name = (String)s.ItemByPropId[2].Value;// reads the names for the field data Component
                    }
                }
                String[] names = name.Split(arr);// set names equal to the string of dataComponent without its limits
                foreach (String n in names)
                {// looks at the strings in names, each one represents a potential data Component
                    if (!n.Equals(""))
                    {// splitting the string creates empty string where the limiter was before, this deals with them
                        foreach (Inventor.PropertySet set in sets)
                        {// looks at all the sets in the propertysets of the document
                            if (set.Name.Equals(n))
                            {// if the set name is the same as the name then we assume they are the same
                                String[] keys = ((String)set.ItemByPropId[11].Value).Split(arr);// set names equal to the string of occurences refkeys without its limits
                                foreach (String m in keys)
                                {// looks at the strings in names, each one represents a potential occurence
                                    if (!m.Equals(""))
                                    {// splitting the string creates empty string where the limiter was before, this deals with them
                                        resultObj = null;
                                        other = null;// clear the objects because inventor wants it that way
                                        context = null;
                                        refKey = new byte[0];// clear the byte[] because inventor wants it that way
                                        InventorApplication.ActiveDocument.ReferenceKeyManager.StringToKey(m, ref refKey);// convert the refkey string to a byte[]
                                        if (InventorApplication.ActiveDocument.ReferenceKeyManager.CanBindKeyToObject(refKey, 0, out resultObj, out context))
                                        {// if the key can be bound then bind it
                                            object obje = StandardAddInServer.InventorApplication.ActiveDocument.ReferenceKeyManager.
                                            BindKeyToObject(refKey, 0, out other);// bind the object to the corrosponding occurence of the reference ker
                                            lis.Add((ComponentOccurrence)obje);// add the occurence to the arraylist in prep to add them tot the field data Component
                                        }
                                    }
                                }
                                BrowserNodeDefinition selectedFolder = AddComponent(((String)set.ItemByPropId[10].Value));// create a new browsernodedef with the name from the old dataComponent
                                FieldDataComponent field = null;
                                foreach (FieldDataComponent f in FieldComponents)
                                {
                                    if (f.same(selectedFolder))
                                    {
                                        field = f;
                                    }
                                }
                                if (field != null)
                                {
                                    field.colliderType = (ColliderType)set.ItemByPropId[2].Value;
                                    field.X = (double)set.ItemByPropId[3].Value;
                                    field.Y = (double)set.ItemByPropId[4].Value;
                                    field.Z = (double)set.ItemByPropId[5].Value;
                                    field.Scale = (double)set.ItemByPropId[6].Value;// read the data from the properties into the fielddataComponent
                                    field.Friction = (double)set.ItemByPropId[7].Value;
                                    field.Dynamic = (bool)set.ItemByPropId[8].Value;
                                    field.Mass = (double)set.ItemByPropId[9].Value;
                                    field.CompOccs = lis;// set the occurences array to the found occurences
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        //Writes the name of the dataComponents to a property set so we can read them later
        private void WriteFieldComponentNames()
        {
            String g = "";// a string to add the names of the data Components to, we do this so we can read the data at the start of the exporter
            PropertySets sets = InventorApplication.ActiveDocument.PropertySets;// the property sets of the document
            Inventor.PropertySet set = null;// a set to add the data to 
            foreach (FieldDataComponent Component in FieldComponents)
            {// looks at all the browser node under the top node
                g += Component.Name;// add the name of the node to the string so when we read the dataComponents we don't lose the name
                g += "¯\\_(:()_/¯";// adds the limiter to the string so we can tell where one name ends and another begins
            }
            try
            {
                set = sets.Add("Number of Folders");// add a new property set to the propertysets
            }
            catch (Exception)
            {// if that fails we assume that there is already that property set
                foreach (Inventor.PropertySet s in sets)
                {// looks at all the property sets in the propertsets
                    if (s.DisplayName.Equals("Number of Folders"))
                    {// if the name is the same then we assume that the property set is the same
                        set = s;// set the property set to the found one
                    }
                }
            }
            try
            {
                set.Add(g, "Number of Folders", 2);// write the value to the property
            }
            catch (Exception)
            {// if that fails we assume that there is already that property so we write to it
                set.ItemByPropId[2].Value = g;// write the value to the property
            }

        }

        //Writes the data Components to the propery set
        private void WriteSaveFieldComponent(FieldDataComponent f)
        {
            PropertySets sets = InventorApplication.ActiveDocument.PropertySets;// the property sets of the document
            Inventor.PropertySet set = null;// a set to add the data to 
            try
            {
                set = sets.Add(f.Name);// add new set to the property sets with the name of the data Component
            }
            catch (Exception)
            {// if that fails then we assume there is already a property set for this set
                foreach (Inventor.PropertySet s in sets)
                {// looks at the property set in the document's property sets
                    if (s.DisplayName.Equals(f.Name))// if the name is the same as another property set we assume we want to use this one
                    {
                        set = s;//set the property set to the found set
                    }
                }
            }
            String g = "";// string to store the ref key of the occurences of the data Component
            byte[] refKey = new byte[0];// create byte[] to hold the refkey of the object
            try
            {
                foreach (ComponentOccurrence n in f.CompOccs)
                {// looks at all the occurences
                    refKey = new byte[0];// clears the current refKey because Inventor needs them to be clear
                    n.GetReferenceKey(ref refKey, 0);// write the reference key of the occurence to the byte[]
                    g += (InventorApplication.ActiveDocument.ReferenceKeyManager.KeyToString(refKey)) + "¯\\_(:()_/¯";//convert the refkey to a string and adds a limiter so we can read the string at the start of the add in
                }
            }
            catch (Exception)
            {
            }
            try
            {// try to write the data to new properties
                set.Add(f.colliderType, "colliderComponent", 2);
                set.Add(f.X, "X", 3);
                set.Add(f.Y, "Y", 4);
                set.Add(f.Z, "Z", 5);
                set.Add(f.Scale, "Scale", 6);// write the data from the dataComponent into the properties
                set.Add(f.Friction, "Friction", 7);
                set.Add(f.Dynamic, "Dynamic", 8);
                set.Add(f.Mass, "Mass", 9);
                set.Add(f.Name, "Name", 10);
                set.Add(g, "items", 11);
            }
            catch (Exception)
            {// if there is an error then assume that the properties are already there and in that case write the new values
                set.ItemByPropId[2].Value = f.colliderType;
                set.ItemByPropId[3].Value = f.X;
                set.ItemByPropId[4].Value = f.Y;
                set.ItemByPropId[5].Value = f.Z;
                set.ItemByPropId[6].Value = f.Scale;// write the data from the dataComponent into the properties
                set.ItemByPropId[7].Value = f.Friction;
                set.ItemByPropId[8].Value = f.Dynamic;
                set.ItemByPropId[9].Value = f.Mass;
                set.ItemByPropId[10].Value = f.Name;
                set.ItemByPropId[11].Value = g;
            }
        }

        /// <summary>
        /// Begins the field exportation procecss by opening 
        /// the form that handles all IO procedures
        /// </summary>
        public void ExportField_OnExecute(NameValueMap Context)
        {
            SetAllButtons(false);
            if (FieldComponents.Count == 0)
            {
                MessageBox.Show("ERROR: No Field Components!");
                SetAllButtons(true);
                return;
            }
            UpdateLegacy();
            Program.ASSEMBLY_DOCUMENT = (AssemblyDocument)InventorApplication.ActiveDocument;
            Program.INVENTOR_APPLICATION = InventorApplication;
            LegacyUtilities.exporter.ShowDialog();
            SetAllButtons(true);
        }

        /// <summary>
        /// Generic function for removing both parts and assemblies
        /// </summary
        public void RemovePartAssembly_OnExecute(NameValueMap Context)
        {
            bool IsDone = false;
            SetAllButtons(false);
            List<BrowserNode> NodeRemoveQueue = new List<BrowserNode>();
            if (!oPane.TopNode.Selected)
            {
                foreach (BrowserNode Comp in oPane.TopNode.BrowserNodes)
                {
                    foreach (BrowserNode PartAsm in Comp.BrowserNodes)
                    {
                        if (PartAsm.Selected)
                        {
                            NodeRemoveQueue.Add(PartAsm);
                            IsDone = true;
                            if (MessageBox.Show("Are you sure you want to remove " + PartAsm.BrowserNodeDefinition.Label + " from " + Comp.BrowserNodeDefinition.Label + "?", "Remove Part/Assembly", MessageBoxButtons.OKCancel) == DialogResult.OK)
                            {
                                foreach (FieldDataComponent DataComp in FieldComponents)
                                {
                                    ArrayList ComponentRemoveQueue = new ArrayList();
                                    foreach (ComponentOccurrence CompOccurence in DataComp.CompOccs)
                                    {
                                        if (CompOccurence.Name == PartAsm.BrowserNodeDefinition.Label)
                                        {
                                            ComponentRemoveQueue.Add(CompOccurence);
                                            if ((CompOccurence.DefinitionDocumentType == DocumentTypeEnum.kAssemblyDocumentObject))
                                                LegacyInterchange.RemoveAssembly(PartAsm.BrowserNodeDefinition.Label);
                                            else
                                                LegacyInterchange.RemovePart(PartAsm.BrowserNodeDefinition.Label);
                                        }
                                    }
                                    foreach (ComponentOccurrence comp in ComponentRemoveQueue)
                                    {
                                        DataComp.CompOccs.Remove(comp);
                                    }
                                }
                            }
                        }
                    }
                    foreach (var node in NodeRemoveQueue)
                    {
                        node.Delete();
                    }
                }

            }
            SetAllButtons(true);
            if (IsDone)
            {
                return;
            }
            else
            {
                MessageBox.Show("ERROR: No parts or assemblies were selected", "Remove Part or Assembly...");
                return;
            }
        }

        /// <summary>
        /// Used to enable/disable all of the buttons with only one function
        /// </summary>
        /// <param name="Enabled">Determines if the buttons will be enabled or disabled</param>
        private static void SetAllButtons(bool Enabled)
        {
            addNewComponent.Enabled = Enabled;
            editComponent.Enabled = Enabled;
            removeComponent.Enabled = Enabled;
            addAssembly.Enabled = Enabled;
            beginExporter.Enabled = Enabled;
            exportField.Enabled = Enabled;
            addPart.Enabled = Enabled;
            removeAssembly.Enabled = Enabled;
            removePart.Enabled = Enabled;
        }
        /// <summary>
        /// Used to determine if a node is selected
        /// </summary>
        /// <returns>A boolean value representative of if a node is selected or not</returns>
        private static bool CheckComponentsSel()
        {
            foreach (BrowserNode node in oPane.TopNode.BrowserNodes)
            {
                if (node.Selected)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Used to determine if a node is selected, if there is one selected, 
        /// sets the value of a node to the value of the selected node
        /// </summary>
        /// <param name="Node">A node object that will have the selected node's data imbedded in it</param>
        /// <returns>A boolean value representative of if a node is selected or not</returns>
        private bool CheckComponentsSel(out BrowserNode Node)
        {
            foreach (BrowserNode node in oPane.TopNode.BrowserNodes)
            {
                if (node.Selected)
                {
                    Node = node;
                    return true;
                }
            }
            Node = null;
            return false;
        }
        /// <summary>
        /// Checks to see if a node is selected
        /// </summary>
        /// <returns>A boolean value to determine if a node has been selected</returns>
        public bool AreNodesSelected()
        {
            foreach (BrowserNode node in oPane.TopNode.BrowserNodes)
            {
                if (node.Selected)
                    return true;
                foreach (BrowserNode childNode in node.BrowserNodes)
                {
                    if (childNode.Selected)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Sets the initial browser node icon for the top node
        /// </summary>
        public static void BrowserNodeIcons()
        {
            Document activeDoc = InventorApplication.ActiveDocument;
            BrowserPane activePane = activeDoc.BrowserPanes.ActivePane;
            BrowserNode topNode = activePane.TopNode;

            ClientBrowserNodeDefinition topNodeDef = (ClientBrowserNodeDefinition)topNode.BrowserNodeDefinition;

            stdole.IPictureDisp fieldIcon =
                PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.FieldIcon16));

            ClientNodeResource fieldResource;
            fieldResource = activeDoc.BrowserPanes.ClientNodeResources.Add("FieldIcon16", 2, fieldIcon);

            topNodeDef.Icon = fieldResource;
        }
    }

    //class that helps with images
    internal class AxHostConverter : AxHost
    {
        private AxHostConverter()
            : base("")
        {
        }

        public static stdole.IPictureDisp ImageToPictureDisp(Image image)
        {
            return (stdole.IPictureDisp)GetIPictureDispFromPicture(image);
        }

        public static Image PictureDispToImage(stdole.IPictureDisp pictureDisp)
        {
            return GetPictureFromIPicture(pictureDisp);
        }
    }
}