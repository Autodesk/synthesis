using System;
using System.Runtime.InteropServices;
using Inventor;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Timers;
using ExportProcess;
using InvAddIn;
using System.Diagnostics;

namespace BxDFieldExporter
{
    //User Defined Interface Exposed through the Add-In Automation property
    public interface AutomationInterface
    {
        void setDone(bool isDone);
        bool getDone();
        void setRunOnce(bool run);
        bool getRunOnce();
        void setCancel(bool isCancel);
    }
    //TLDR: exports the field


    [GuidAttribute("e50be244-9f7b-4b94-8f87-8224faba8ca1")]
    public class StandardAddInServer : Inventor.ApplicationAddInServer, AutomationInterface
    {
        // all the global variables
        #region variables
        // Inventor application object.
        public static Inventor.Application m_inventorApplication;// the main inventor application
        [DllImport("user32.dll")]
        public static extern ushort GetKeyState(short key);
        ClientNodeResource oRsc;// client resources that buttons will use
        static Document nativeDoc;
        static bool runOnce;
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
        static ButtonDefinition cancelExport;
        static ButtonDefinition exportField;
        static ButtonDefinition removeComponent;
        EditCoordinate coorForm;
        Inventor.Environment oNewEnv;
        static bool done;
        static bool cancel = false;
        static Random rand;// random number genator that can create internal ids
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
                m_inventorApplication = addInSiteObject.Application;// get the inventor object
                closing = false;
                FieldComponents = new ArrayList();// clear the field Component array
                form = new ComponentPropertiesForm();// init the component form to enter data into
                AddParallelEnvironment();
                UIEvent = m_inventorApplication.CommandManager.UserInputEvents;// get the application's userinput events object
                click_OnSelectEventDelegate = new UserInputEventsSink_OnSelectEventHandler(oUIEvents_OnSelect);// make a new ui event reactor
                UIEvent.OnSelect += click_OnSelectEventDelegate;// add the event reactor to the onselect 
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
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
                writeFieldComponentNames();
                foreach (FieldDataComponent data in FieldComponents)
                {
                    writeSaveFieldComponent(data);
                }
                m_inventorApplication.ActiveDocument.Save();
            }
            catch (Exception) { }
            m_inventorApplication = null;

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
        public object Automation
        {
            // This property is provided to allow the AddIn to expose an API 
            // of its own to other programs. Typically, this  would be done by
            // implementing the AddIn's API interface in a class and returning 
            // that class object through this property.

            get
            {
                // TODO: Add ApplicationAddInServer.Automation getter implementation
                return this;
            }
        }

        #endregion

        #region API exposed methods
        public void setDone(bool isDone)
        {
            done = isDone;
        }

        public bool getDone()
        {
            return done;
        }

        public void setRunOnce(bool run)
        {
            runOnce = run;
        }

        public bool getRunOnce()
        {
            return runOnce;
        }

        public void setCancel(bool isCancel)
        {
            cancel = isCancel;
        }
    
        #endregion 

        // called when the exporter starts
        public void startExport_OnExecute(Inventor.NameValueMap Context)
        {

            try
            {
                if (!inExportView)
                {
                    nativeDoc = m_inventorApplication.ActiveDocument;
                    envMan = ((AssemblyDocument)m_inventorApplication.ActiveDocument).EnvironmentManager;
                    inExportView = true;
                    addNewComponent.Enabled = true;
                    editComponent.Enabled = true;
                    removeComponent.Enabled = true;
                    addAssembly.Enabled = true;
                    beginExporter.Enabled = false;// show the correct buttons
                    cancelExport.Enabled = true;
                    exportField.Enabled = true;
                    addPart.Enabled = true;
                    removeAssembly.Enabled = true;
                    removePart.Enabled = true;
                    AssemblyDocument asmDoc = (AssemblyDocument)m_inventorApplication.ActiveDocument;// get the active assembly document
                    BrowserNodeDefinition oDef; // create browsernodedef to use to add browser node to the pane
                    oDoc = m_inventorApplication.ActiveDocument;// get the active document in inventor
                    oPanes = oDoc.BrowserPanes;// get the browserpanes to add
                    oSet = oDoc.CreateHighlightSet();// create a highlightset to add the selected occcurences to
                    oSet.Color = m_inventorApplication.TransientObjects.CreateColor(100, 0, 200);

                    partSet = oDoc.CreateHighlightSet();
                    partSet.Color = m_inventorApplication.TransientObjects.CreateColor(100, 0, 200);
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

                                oPane = pane;// if we have found the correct node then use it
                                foreach (BrowserNode f in oPane.TopNode.BrowserNodes)
                                {
                                    f.Delete();// delete any residual browser nodes
                                }
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
                    readSaveFieldData();// read the save so the user doesn't loose any previous work
                    TimerWatch();// begin the timer watcher to detect deselect

                }
                else
                {
                    MessageBox.Show("Please close out of the robot exporter in the other assembly");
                }
            }
            catch (Exception e)
            {

                MessageBox.Show(e.ToString());
            }
        }
        public void OnEnvironmentChange(Inventor.Environment environment, EnvironmentStateEnum EnvironmentState, EventTimingEnum BeforeOrAfter, NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {
            if (environment.Equals(oNewEnv) && EnvironmentState.Equals(EnvironmentStateEnum.kActivateEnvironmentState) && !closing)
            {
                closing = true;
                startExport_OnExecute(null);
            }
            else if (environment.Equals(oNewEnv) && EnvironmentState.Equals(EnvironmentStateEnum.kTerminateEnvironmentState) && closing)
            {
                closing = false;
                cancleExporter_OnExecute(null);
            }
            HandlingCode = HandlingCodeEnum.kEventNotHandled;
        }

        public void AddParallelEnvironment()
        {
            try
            {
                stdole.IPictureDisp startExporterIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.StartExporter16));
                stdole.IPictureDisp startExporterIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.StartExporter32));

                stdole.IPictureDisp addNewComponentIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.AddNewType16));
                stdole.IPictureDisp addNewComponentIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.AddNewType32));

                stdole.IPictureDisp editComponentIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.EditType16));
                stdole.IPictureDisp editComponentIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.EditType32));

                stdole.IPictureDisp removeAssemblyIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.RemoveAssembly16));
                stdole.IPictureDisp removeAssemblyIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.RemoveAssembly32));

                stdole.IPictureDisp removePartIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.RemoveSubAssembly16));
                stdole.IPictureDisp removePartIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.RemoveSubAssembly16));

                stdole.IPictureDisp addPartIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.AddNewPart16));
                stdole.IPictureDisp addPartIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.AddNewPart32));

                stdole.IPictureDisp addAssemblyIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.AddAssembly16));
                stdole.IPictureDisp addAssemblyIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.AddAssembly32));

                stdole.IPictureDisp removeComponentIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.RemoveType16));
                stdole.IPictureDisp removeComponentIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.RemoveType32));

                stdole.IPictureDisp exportFieldIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.ExportField16));
                stdole.IPictureDisp exportFieldIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.ExportField32));

                stdole.IPictureDisp ttAddNewComponent = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.TTAddNewComponent));
                stdole.IPictureDisp ttAddAssembly = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.TTAddAssembly));
                stdole.IPictureDisp ttAddPart = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.TTAddPart));     
                stdole.IPictureDisp ttComponentProperties = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.TTComponentProperties));
                stdole.IPictureDisp ttExportField = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.TTExportField));
                stdole.IPictureDisp ttRemovePart = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.TTRemovePart));
                stdole.IPictureDisp ttRemoveComponent = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.TTRemoveComponent));
                stdole.IPictureDisp ttRemoveAssembly = PictureDispConverter.ToIPictureDisp(new Bitmap(BxDFieldExporter.Resource.TTRemoveAssembly));

                // Get the Environments collection
                Environments oEnvironments = m_inventorApplication.UserInterfaceManager.Environments;

                // Create a new environment
                oNewEnv = oEnvironments.Add("Field Exporter", "BxD:FieldExporter:Environment", null, startExporterIconSmall, startExporterIconLarge);

                // Get the ribbon associated with the assembly environment
                Ribbon oAssemblyRibbon = m_inventorApplication.UserInterfaceManager.Ribbons["Assembly"];

                // Create contextual tabs and panels within them
                RibbonTab oContextualTabOne = oAssemblyRibbon.RibbonTabs.Add("Field Exporter", "BxD:FieldExporter:RibbonTab", "ClientId123", "", false, true);


                ComponentControls = oContextualTabOne.RibbonPanels.Add("Component Controls", "BxD:FieldExporter:ComponentControls", "{e50be244-9f7b-4b94-8f87-8224faba8ca1}");
                AddItems = oContextualTabOne.RibbonPanels.Add("Add Items", "BxD:FieldExporter:AddItems", "{e50be244-9f7b-4b94-8f87-8224faba8ca1}");
                RemoveItems = oContextualTabOne.RibbonPanels.Add("Remove Items", "BxD:FieldExporter:RemoveItems", "{e50be244-9f7b-4b94-8f87-8224faba8ca1}");
                ExporterControl = oContextualTabOne.RibbonPanels.Add("Robot Exporter Control", "BxD:FieldExporter:ExporterControl", "{e50be244-9f7b-4b94-8f87-8224faba8ca1}");// inits the part panels
                AddItems.Reposition("BxD:FieldExporter:ComponentControls", false);
                RemoveItems.Reposition("BxD:FieldExporter:AddItems", false);
                ControlDefinitions controlDefs = m_inventorApplication.CommandManager.ControlDefinitions;// get the controls for Inventor
                beginExporter = controlDefs.AddButtonDefinition("Start Exporter", "BxD:FieldExporter:StartExporter", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, "Starts the field exporter", "Yay lets start!", startExporterIconSmall, startExporterIconLarge, ButtonDisplayEnum.kAlwaysDisplayText);
                beginExporter.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(startExport_OnExecute);

                addNewComponent = controlDefs.AddButtonDefinition(" Add New Component ", "BxD:FieldExporter:AddNewComponent", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null, addNewComponentIconSmall, addNewComponentIconLarge, ButtonDisplayEnum.kAlwaysDisplayText);
                addNewComponent.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(addNewComponent_OnExecute);
                toolTip(addNewComponent, "Creates a physics object to hold parts or assemblies.",
                    "For example, \"floor\", \"wall.\" or \"airship.\"After creating a component, you can add parts or assemblies to the component using either the \"Add New Part\" or \"Add New Assembly\" button.",
                    ttAddNewComponent, "Add New Component");

                addAssembly = controlDefs.AddButtonDefinition(" Add New Assembly ", "BxD:FieldExporter:AddNewItem", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null, addAssemblyIconSmall, addAssemblyIconLarge);
                addAssembly.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(addNewAssemblies_OnExecute);
                toolTip(addAssembly, "Adds an assembly to a field component.",
                    "Click on a field component in the Field Exporter hierarchy so that it is highlighted. Click \"Add New Assembly\", and select the assembly to add to the component. To add multiple assemblies to one component, repeat the process.",
                    ttAddAssembly, "Add Assembly");    

                removeAssembly = controlDefs.AddButtonDefinition(" Remove Assembly ", "BxD:FieldExporter:RemoveAssembly", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null, removeAssemblyIconSmall, removeAssemblyIconLarge);
                removeAssembly.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(removeAssembly_OnExecute);
                toolTip(removeAssembly, "Removes an assembly from a field component.",
                    "Click on a field component in the Field Exporter hierarchy so that it is highlighted. Click \"Remove Assembly\", and select the assembly to remove from the component. To remove multiple assemblies from one component, repeat the process",
                    ttRemoveAssembly, "Remove Assembly");

                removePart = controlDefs.AddButtonDefinition(" Remove Part ", "BxD:FieldExporter:RemovePart", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null, removePartIconSmall, removePartIconLarge);
                removePart.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(removePart_OnExecute);
                toolTip(removePart, "Removes a part from a field component.",
                   "Click on a field component in the Field Exporter hierarchy so that it is highlighted. Click “Remove Part”, and select the part to remove from the component. To remove multiple parts from one component, repeat the process.",
                   ttRemovePart, "Remove Assembly");

                editComponent = controlDefs.AddButtonDefinition(" Edit Component Properties ", "BxD:FieldExporter:EditComponentProperties", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null, editComponentIconSmall, editComponentIconLarge);// init the button
                editComponent.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(editComponentProperites_OnExecute);// add the reacting method to the button
                toolTip(editComponent, "Edit properties including collider type, friction and dynamic.",
                    @"Collider types include box (flat surfaces), sphere (round surfaces), and mesh (uneven or organic surfaces.) 

Use the friction slider to adjust the friction coefficient in the simulator. 

Checking “Dynamic” enables an object to be moved in the simulator. For example, check “Dynamic” for objects like balls or other game pieces. Do not check “Dynamic” for static objects like the floor and walls.",
                    ttComponentProperties, "Edit Component Properties");

                cancelExport = controlDefs.AddButtonDefinition("Cancel Export", "BxD:FieldExporter:CancelExport", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                cancelExport.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(cancleExporter_OnExecute);

                exportField = controlDefs.AddButtonDefinition("Export Field", "BxD:FieldExporter:ExportField", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null, exportFieldIconSmall, exportFieldIconLarge);
                exportField.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(exportField_OnExecute);
                toolTip(exportField, "Exports the field for use in the Synthesis simulation.",
                    "After adding all components of a field, and populating those components with parts or assemblies, export the field. The field will be saved to Documents/Synthesis/Fields and can be accessed through Synthesis.",
                    ttExportField, "Export Field");

                addPart = controlDefs.AddButtonDefinition(" Add New Part ", "BxD:FieldExporter:AddNewPart", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null, addPartIconSmall, addPartIconLarge);
                addPart.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(addNewPart_OnExecute);
                toolTip(addPart, "Adds a part to a field component.",
                    "Click on a field component in the Field Exporter hierarchy so that it is highlighted. Click “Add New Part”, and select the part to add to the component. To add multiple part to one component, repeat the process.",
                    ttAddPart, "Add New Part");

                removeComponent = controlDefs.AddButtonDefinition(" Remove Component ", "BxD:FieldExporter:RemoveComponent", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null, removeComponentIconSmall, removeComponentIconLarge);
                removeComponent.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(removeComponent_OnExecute);
                toolTip(removeComponent, "Removes a component from the field components hierarchy.",
                    "Removing the component will ungroup all parts or assemblies attached to the component.",
                    ttRemoveComponent, "Remove Component");

                ComponentControls.CommandControls.AddButton(addNewComponent, true, true);
                ComponentControls.CommandControls.AddButton(removeComponent, true, true);
                ComponentControls.CommandControls.AddButton(editComponent, true, true);
                AddItems.CommandControls.AddButton(addAssembly, true, true);
                AddItems.CommandControls.AddButton(addPart, true, true);
                RemoveItems.CommandControls.AddButton(removeAssembly, true, true);// add buttons to the part panels
                RemoveItems.CommandControls.AddButton(removePart, true, true);
                ExporterControl.CommandControls.AddButton(exportField, true, true);
                addNewComponent.Enabled = false;
                editComponent.Enabled = false;
                removeComponent.Enabled = false;
                addAssembly.Enabled = false;
                beginExporter.Enabled = true;// set the correct button states for not being in export mode
                cancelExport.Enabled = false;
                exportField.Enabled = false;
                addNewComponent.Enabled = false;
                addPart.Enabled = false;
                removeAssembly.Enabled = false;
                removePart.Enabled = false;
                UserInterfaceEvents UIEvents = m_inventorApplication.UserInterfaceManager.UserInterfaceEvents;

                enviroment_OnChangeEventDelegate = new UserInterfaceEventsSink_OnEnvironmentChangeEventHandler(OnEnvironmentChange);
                UIEvents.OnEnvironmentChange += enviroment_OnChangeEventDelegate;

                // Make the "SomeAnalysis" tab default for the environment
                oNewEnv.DefaultRibbonTab = "BxD:FieldExporter:RibbonTab";

                // Get the collection of parallel environments and add the new environment
                EnvironmentList oParEnvs = m_inventorApplication.UserInterfaceManager.ParallelEnvironments;

                oParEnvs.Add(oNewEnv);

                // Make the new parallel environment available only within the assembly environment
                // A ControlDefinition is automatically created when an environment is added to the
                // parallel environments list. The internal name of the definition is the same as
                // the internal name of the environment.
                ControlDefinition oParallelEnvButton = m_inventorApplication.CommandManager.ControlDefinitions["BxD:FieldExporter:Environment"];

                Inventor.Environment oEnv;
                oEnv = oEnvironments["BxD:FieldExporter:Environment"];
                oEnv.DisabledCommandList.Add(oParallelEnvButton);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        //Sets the tool tips
        public void toolTip(ButtonDefinition button, String description, String expandedDescription, stdole.IPictureDisp picture, String title)
        {
            button.ProgressiveToolTip.Description = description;
            button.ProgressiveToolTip.ExpandedDescription = expandedDescription;
            button.ProgressiveToolTip.Image = picture;
            button.ProgressiveToolTip.IsProgressive = true;
            button.ProgressiveToolTip.Title = title;
        }

        //Sets the tool tips without the picture
        public void toolTip(ButtonDefinition button, String description, String expandedDescription, String title)
        {
            button.ProgressiveToolTip.Description = description;
            button.ProgressiveToolTip.ExpandedDescription = expandedDescription;
            button.ProgressiveToolTip.IsProgressive = true;
            button.ProgressiveToolTip.Title = title;
        }

        // reacts to a selection
        private void oUIEvents_OnSelect(ObjectsEnumerator JustSelectedEntities, ref ObjectCollection MoreSelectedEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
        {
            oSet.Clear();// clear the highlight set to add a new component to the set
            if (SelectionDevice == SelectionDeviceEnum.kGraphicsWindowSelection && inExportView)
            {// if the selection is from the graphical interface and the exporter is active
                foreach (Object sel in JustSelectedEntities)
                {//looks at all things selected
                    if (sel is ComponentOccurrence)
                    {// react only if sel is a part/ assembly
                        foreach (FieldDataComponent Component in FieldComponents)
                        {// looks at all the components of parts
                            foreach (ComponentOccurrence occ in Component.compOcc)
                            {// looks at all the occurences in the Components items
                                if (((ComponentOccurrence)sel).Equals(occ))
                                {// if the occurence is contained by anyof the components then react
                                    foreach (BrowserNode n in oPane.TopNode.BrowserNodes)
                                    {// looks at all the browser nodes in the top node
                                        if (n.BrowserNodeDefinition.Equals(Component.node))
                                        {// if the browsernode is the same as the Components node then react
                                            n.DoSelect();// select the proper node
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (sel is UserCoordinateSystem)
                    {
                        foreach (UserCoordinateSystem ucs in ((AssemblyComponentDefinition)((AssemblyDocument)m_inventorApplication.ActiveDocument).ComponentDefinition).UserCoordinateSystems)
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
                foreach (Object sel in JustSelectedEntities)
                {//looks at all things selected
                    if (sel is BrowserNodeDefinition)
                    {// react only if sel is a browsernodedef
                        foreach (FieldDataComponent f in FieldComponents)
                        {// looks at all the components of parts
                            if (f.same(((BrowserNodeDefinition)sel)))
                            {// if the browsernode is the same as a the Component's node
                                selectedComponent = f;// set the selected Component for the rest of the code to interact with
                                foreach (ComponentOccurrence o in selectedComponent.compOcc)
                                {// looks at the occurences in the selected Component's part list
                                    oSet.AddItem(o);// show the user which parts are selected
                                }
                                editComponent.Enabled = true;
                                removeComponent.Enabled = true;
                                addAssembly.Enabled = true;
                                addPart.Enabled = true;
                                removeAssembly.Enabled = true;
                                removePart.Enabled = true;
                            }
                        } 
                    }
                }
            }
        }
        // starts a timer to react to events in the browser/ graphics interface
        static bool rightDoc;
        private void TimerWatch()
        {
            try
            {
                System.Timers.Timer aTimer = new System.Timers.Timer();// creates a new timer
                aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);// handles an elasped time event
                aTimer.Interval = 250;// set time timer reaction interval to 1/4 of a second, we do this to get regular checking without lagging out the computer
                aTimer.AutoReset = true;// auto restarts the timer after the 1/4 second interval
                aTimer.Enabled = true;// starts the timer
                rightDoc = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        public static bool checkKeyPressed(Keys key)
        {
            return ((GetKeyState((short)key) & 0x80) == 0x80);
        }
        // reacts to the timer elapsed event, this allows us to select and unselect things as needed
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            found = false;
            if (checkKeyPressed(Keys.ShiftKey) || checkKeyPressed(Keys.ControlKey))
            {
            }
            else
            {
                if (!runOnce)
                {
                    done = true;
                    m_inventorApplication.CommandManager.StopActiveCommand();
                    runOnce = true;
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
                                foreach (ComponentOccurrence io in t.compOcc)
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
                editComponent.Enabled = false;
                removeComponent.Enabled = false;
                addAssembly.Enabled = false;
                addPart.Enabled = false;
                removeAssembly.Enabled = false;
                removePart.Enabled = false;
            }
            if (!m_inventorApplication.ActiveDocument.InternalName.Equals(nativeDoc.InternalName))
            {
                if (rightDoc)
                {
                    rightDoc = false;

                    addNewComponent.Enabled = false;
                    editComponent.Enabled = false;
                    removeComponent.Enabled = false;
                    addAssembly.Enabled = false;
                    beginExporter.Enabled = true;// sets the correct buttons states
                    cancelExport.Enabled = false;
                    exportField.Enabled = false;
                    addPart.Enabled = false;
                    removeAssembly.Enabled = false;
                    removePart.Enabled = false;


                    oPane.Visible = false;// Hide the browser pane
                }
            }
            else
            {
                if (!rightDoc)
                {
                    rightDoc = true;

                    addNewComponent.Enabled = true;
                    removeComponent.Enabled = true;
                    editComponent.Enabled = true;
                    addAssembly.Enabled = true;
                    beginExporter.Enabled = false;// sets the correct buttons states
                    cancelExport.Enabled = true;
                    exportField.Enabled = true;
                    addPart.Enabled = true;
                    removeAssembly.Enabled = true;
                    removePart.Enabled = true;

                    oPane.Visible = true;// Hide the browser pane
                    oPane.Activate();
                }

            }
        }
        // adds a new fielddataComponent to the array and to the browser pane
        public static BrowserNodeDefinition addComponent(String name)
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
                    int th = rand.Next();// get the next random number for the browser node's internal id
                    ClientNodeResources oNodeRescs; // creates a ClientNodeResourcess that we add the ClientNodeResource to
                    ClientNodeResource oRes = null;// creates a ClientNodeResource for adding the browsernode, needs to be null for some reason, idk
                    oNodeRescs = oPanes.ClientNodeResources;// set the ClientNodeResources the the active document's ClientNodeResources
                    try
                    {
                        oRes = oNodeRescs.Add("MYID", 1, null);// create a new ClientNodeResource to be used when you add the browser node
                    }
                    catch (Exception)
                    {// if the method fails then assume that there is already a ClientNodeResource
                        oRes = oPanes.ClientNodeResources.ItemById("MYID", 1);// get the ClientNodeResource by the name
                    }
                    def = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition(name, th, oRes);// creates a new browser node def for the field data
                    oPane.TopNode.AddChild(def);// add the browsernode to the topnode
                    FieldComponents.Add(new FieldDataComponent(def, fieldID));// add the new field data Component to the array and use the browsernodedef to refence the object to the browser node
                    fieldID++; //makes the ID spacing move up a unit
                }
                else
                {// if there is already something with the name
                    MessageBox.Show("Please choose a name that hasn't already been used");// tell the user to use a different name
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            return def;// returns the browsernodedef
        }
        
        //Opens the "Add Part(s)" window and adds parts to a component
        public void addNewPart_OnExecute(Inventor.NameValueMap Context)
        {
            //Create form and show it
            AddPart form = new AddPart();
            form.Show();
            
            done = false;
            cancel = false;
            int componentsAdded = 0; //Tracks how many components are added
            while (!done)
            {
                ComponentOccurrence joint = null;
                AssemblyDocument asmDoc = (AssemblyDocument)
                             m_inventorApplication.ActiveDocument;
                joint = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick// have the user select a part
                          (SelectionFilterEnum.kAssemblyLeafOccurrenceFilter, "Select a part to add");
                if (joint != null)
                {
                    foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// look at all the nodes under the top node
                    {
                        if (node.Selected)// find the selected node
                        {
                            foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
                            {
                                if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
                                {
                                    t.compOcc.Add(joint);// add the part occurence to the arraylist
                                    partSet.AddItem(joint); //add the part occurence to a set that is highlighted in purple
                                    node.DoSelect();
                                }
                            }
                        }
                    }
                    componentsAdded++; //Tracks how many components are added
                }
            }

            partSet.Clear(); //Clears the highlighted set

            //If the user clicks "cancel", remove all parts previously added to the list
            if (cancel == true)
            {
                foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// look at all the nodes under the top node
                {
                    if (node.Selected)// find the selected node
                    {
                        foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
                        {
                            if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
                            {
                                for (int i = 0; i < componentsAdded; i++) //for each previously added component
                                {
                                    t.compOcc.RemoveAt(t.compOcc.Count - 1); //removes the component
                                }
                            }
                        }
                    }
                }
            } 
        }
        
        //Opens the "Add Assembly(s)" and adds assemblies to a component
        public void addNewAssemblies_OnExecute(Inventor.NameValueMap Context)
        {
            //Create form and show it
            AddAssembly form = new AddAssembly();
            form.Show();

            done = false;
            cancel = false;
            int componentsAdded = 0; //Tracks how many components are added
            while (!done)
            {
                ComponentOccurrence joint = null;
                AssemblyDocument asmDoc = (AssemblyDocument)
                             m_inventorApplication.ActiveDocument;
                joint = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick// have the user select an assembly
                          (SelectionFilterEnum.kAssemblyOccurrenceFilter, "Select an assembly to add");
                if (joint != null)
                {
                    foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// look at all the nodes under the top node
                    {
                        if (node.Selected)// find the selected node
                        {
                            foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
                            {
                                if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
                                {
                                    t.compOcc.Add(joint);// add the assembly occurence to the arraylist
                                    partSet.AddItem(joint); //add the assembly occurence to a set that is highlighted in purple
                                    node.DoSelect();
                                }
                            }
                        }
                    }
                    componentsAdded++; //Tracks how many components are added
                }
            }
            partSet.Clear(); //Clears the highlighted set

            //If the user clicks "cancel", remove all parts previously added to the list
            if (cancel == true)
            {
                foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// look at all the nodes under the top node
                {
                    if (node.Selected)// find the selected node
                    {
                        foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
                        {
                            if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
                            {
                                for (int i = 0; i < componentsAdded; i++) //for each previously added component
                                {
                                    t.compOcc.RemoveAt(t.compOcc.Count - 1); //remove previously added components
                                }
                            }
                        }
                    }
                }
            }
        }

        // removes an assembly from the arraylist of the Component
        public void removeAssembly_OnExecute(Inventor.NameValueMap Context)
        {
            try
            {
                runOnce = true;
                bool found = false;
                bool selected = false;
                foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// looks over all of the nodes
                {
                    if (node.Selected)
                    {
                        foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
                        {
                            if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
                            {
                                selected = true;// if one node is selected then we can add the new sub assembly
                            }
                        }
                    }
                }
                if (selected)// if there is a selected node then we can add a part to it
                {
                    done = false;
                    while (!done)
                    {
                        ComponentOccurrence joint = null;
                        AssemblyDocument asmDoc = (AssemblyDocument)
                                     m_inventorApplication.ActiveDocument;
                        joint = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick// have the user select a leaf occurrence or part
                                  (SelectionFilterEnum.kAssemblyOccurrenceFilter, "Select an assembly to remove");
                        if (joint != null)
                        {
                            foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// look at all the nodes under the top node
                            {
                                if (node.Selected)// find the selected node
                                {
                                    foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
                                    {
                                        if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
                                        {
                                            if (t.compOcc.Contains(joint))
                                            {// if the occurence is in the list the allow the remove
                                                t.compOcc.Remove(joint);// add the occurence to the arraylist
                                                m_inventorApplication.ActiveDocument.SelectSet.Clear();
                                                node.DoSelect();
                                                found = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!found)
                        {
                            MessageBox.Show("Warning, assembly not found in item component");// if the assembly wasn't found in the component then tell the user
                        }
                        runOnce = false;
                        found = false;
                    }
                }
                else
                {
                    MessageBox.Show("Please select a browser node to remove an assembly from");// if the user didn't select a browser node then tell them
                }
                runOnce = true;
            }
            catch (Exception)
            {

            }
        }
        
        // removes a part from the arraylist of the Component
        public void removePart_OnExecute(Inventor.NameValueMap Context)
        {
            try
            {
                runOnce = true;
                bool found = false;
                bool selected = false;
                foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// looks over all of the nodes
                {
                    if (node.Selected)
                    {
                        foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
                        {
                            if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
                            {
                                selected = true;// if one node is selected then we can add the new sub assembly
                            }
                        }
                    }
                }
                if (selected)// if there is a selected node then we can add a part to it
                {
                    done = false;
                    while (!done)
                    {
                        ComponentOccurrence joint = null;
                        AssemblyDocument asmDoc = (AssemblyDocument)
                                     m_inventorApplication.ActiveDocument;
                        joint = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick// have the user select a leaf occurrence or part
                                  (SelectionFilterEnum.kAssemblyLeafOccurrenceFilter, "Select a part to remove");
                        if (joint != null)
                        {
                            foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// look at all the nodes under the top node
                            {
                                if (node.Selected)// find the selected node
                                {
                                    foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
                                    {
                                        if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
                                        {
                                            if (t.compOcc.Contains(joint))// if the occurence is in the list the allow the remove
                                            {
                                                t.compOcc.Remove(joint);// add the occurence to the arraylist
                                                m_inventorApplication.ActiveDocument.SelectSet.Clear();
                                                node.DoSelect();
                                                found = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!found)
                        {
                            MessageBox.Show("Warning, part not found in item component");// if the part wasn't found in the component then tell the user
                        }
                        found = false;
                        runOnce = false;
                    }
                }
                else
                {
                    MessageBox.Show("Please select a browser node to remove a part from");// if the user didn't select a browser node then tell them
                }
                runOnce = true;
            }
            catch (Exception)
            {

            }
        }
        
        //Old Code for adding a single assembly
        public void addNewAssembly_OnExecute(Inventor.NameValueMap Context)
        {
            try
            {
                runOnce = true;
                done = false;
                bool selected = false;
                foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// looks over all of the nodes
                {
                    if (node.Selected)
                    {
                        foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
                        {
                            if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
                            {
                                selected = true;// if one node is selected then we can add the new sub assembly
                            }
                        }
                    }
                }
                if (selected)// if there is a selected node then we can add a part to it
                {
                    while (!done)
                    {
                        ComponentOccurrence joint = null;
                        AssemblyDocument asmDoc = (AssemblyDocument)
                                 m_inventorApplication.ActiveDocument;
                        joint = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick// have the user select a leaf occurrence or part
                                  (SelectionFilterEnum.kAssemblyOccurrenceFilter, "Select an assembly to add");
                        if (joint != null)
                        {
                            foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// look at all the nodes under the top node
                            {
                                if (node.Selected)// find the selected node
                                {
                                    foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
                                    {
                                        if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
                                        {
                                            t.compOcc.Add(joint);// add the occurence to the arraylist
                                            m_inventorApplication.ActiveDocument.SelectSet.Clear();
                                            node.DoSelect();
                                        }
                                    }
                                }
                            }
                        }
                        runOnce = false;
                    }
                }
                else
                {
                    MessageBox.Show("Please select a browser node to add a part to");// if the user didn't select a browser node then tell them
                }
                runOnce = false;
            }
            catch (Exception)
            {

            }
            runOnce = true;

        }

        //Old Code for adding a single part
        public void addNewSubAssembly_OnExecute(Inventor.NameValueMap Context)
        {
            try
            {
                runOnce = true;
                done = false;
                bool selected = false;
                foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// looks over all of the nodes
                {
                    if (node.Selected)
                    {
                        foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
                        {
                            if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
                            {
                                selected = true;// if one node is selected then we can add the new sub assembly

                            }
                        }
                    }
                }
                if (selected)// if there is a selected node then we can add a part to it
                {
                    while (!done)
                    {
                        ComponentOccurrence joint = null;
                        AssemblyDocument asmDoc = (AssemblyDocument)
                                     m_inventorApplication.ActiveDocument;
                        joint = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick// have the user select a leaf occurrence or part
                                  (SelectionFilterEnum.kAssemblyLeafOccurrenceFilter, "Select a part to add");
                        if (joint != null)
                        {
                            foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// look at all the nodes under the top node
                            {
                                if (node.Selected)// find the selected node
                                {
                                    foreach (FieldDataComponent t in FieldComponents)// look at all the field data Components
                                    {
                                        if (t.same(node.BrowserNodeDefinition))// is the fieldDataComponent is from that browsernode then run
                                        {
                                            t.compOcc.Add(joint);// add the occurence to the arraylist
                                            m_inventorApplication.ActiveDocument.SelectSet.Clear();
                                            node.DoSelect();
                                        }
                                    }
                                }
                            }
                            runOnce = false;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select a browser node to add a part to");// if the user didn't select a browser node then tell them
                }
                runOnce = true;
            }
            catch (Exception)
            {

            }
        }

        public void removeComponent_OnExecute(Inventor.NameValueMap Context)
        {
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
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete Component: " + "\n" + names, "Remove Component", MessageBoxButtons.OKCancel);
            if (dialogResult == DialogResult.OK)
            {
                foreach (BrowserNode node in selectedNodes)
                {
                    foreach (FieldDataComponent f in FieldComponents)
                    {
                        if (f.same(node.BrowserNodeDefinition))
                        {
                            FieldComponents.Remove(f);
                            node.Delete();
                        }
                    }
                }
            }
        }

        // edits the properties of the Component
        public static void editComponentProperites_OnExecute(Inventor.NameValueMap Context)
        {
            //read from the temp save the proper field values
            form.readFromData(selectedComponent);
            //show a dialog for the user to enter in values
            form.ShowDialog();
        }
       
        // adds new property Component to the browser pane
        public static void addNewComponent_OnExecute(Inventor.NameValueMap Context)
        {
            // create a new enter name form
            EnterName form = new EnterName();
            //show the form to the user
            System.Windows.Forms.Application.Run(form);
        }
        
        // cancels the export
        public void cancleExporter_OnExecute(Inventor.NameValueMap Context)
        {
            try
            {
                inExportView = false;// tell the event reactors to not react because we are no longer in export mode
                writeFieldComponentNames();// write the browser folder names to the property sets so we can read them next time the program is run
                foreach (FieldDataComponent data in FieldComponents)
                {// looks at all the components in fieldComponent
                    writeSaveFieldComponent(data);// writes the saved data to the property set
                }
                FieldComponents = new ArrayList();// clear the arraylist of components
                foreach (BrowserNode node in oPane.TopNode.BrowserNodes)
                {// looks at all the nodes under the top node
                    node.Delete();// deletes the nodes
                }
                oPane.Visible = false;// hide the browser pane because we aren't exporting anymore
                addNewComponent.Enabled = false;
                editComponent.Enabled = false;
                removeComponent.Enabled = false;
                addAssembly.Enabled = false;
                beginExporter.Enabled = true;// sets the correct buttons states
                cancelExport.Enabled = false;
                exportField.Enabled = false;
                addPart.Enabled = false;
                removeAssembly.Enabled = false;
                removePart.Enabled = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        
        // read the saved data
        private void readSaveFieldData()
        {
            try
            {
                ArrayList lis = new ArrayList();// creates an arraylist which will contain occurences to add to the field data Component
                byte[] refKey;// creates a byte[] to hold the refkeys
                PropertySets sets = m_inventorApplication.ActiveDocument.PropertySets;// gets a property sets of the document 
                other = null;
                context = null;// clears the object because inventor needs it that way
                resultObj = null;
                String name = "";// make name that can store the names of the browser nodes
                Char[] arr = { '¯', '\\', '_', '(', ':', '(', ')', '_', '/', '¯' };// contains the limiter of the strings so we can read them
                foreach (PropertySet s in sets)
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
                        foreach (PropertySet set in sets)
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
                                        m_inventorApplication.ActiveDocument.ReferenceKeyManager.StringToKey(m, ref refKey);// convert the refkey string to a byte[]
                                        if (m_inventorApplication.ActiveDocument.ReferenceKeyManager.CanBindKeyToObject(refKey, 0, out resultObj, out context))
                                        {// if the key can be bound then bind it
                                            object obje = StandardAddInServer.m_inventorApplication.ActiveDocument.ReferenceKeyManager.
                                            BindKeyToObject(refKey, 0, out other);// bind the object to the corrosponding occurence of the reference ker
                                            lis.Add((ComponentOccurrence)obje);// add the occurence to the arraylist in prep to add them tot the field data Component
                                        }
                                    }
                                }
                                BrowserNodeDefinition selectedFolder = addComponent(((String)set.ItemByPropId[10].Value));// create a new browsernodedef with the name from the old dataComponent
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
                                    field.compOcc = lis;// set the occurences array to the found occurences
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        
        // writes the name of the dataComponents to a property set so we can read them later
        private void writeFieldComponentNames()
        {
            String g = "";// a string to add the names of the data Components to, we do this so we can read the data at the start of the exporter
            PropertySets sets = m_inventorApplication.ActiveDocument.PropertySets;// the property sets of the document
            PropertySet set = null;// a set to add the data to 
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
                foreach (PropertySet s in sets)
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
        
        // writes the data Components to the propery set
        private void writeSaveFieldComponent(FieldDataComponent f)
        {
            PropertySets sets = m_inventorApplication.ActiveDocument.PropertySets;// the property sets of the document
            PropertySet set = null;// a set to add the data to 
            try
            {
                set = sets.Add(f.Name);// add new set to the property sets with the name of the data Component
            }
            catch (Exception)
            {// if that fails then we assume there is already a property set for this set
                foreach (PropertySet s in sets)
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
                foreach (ComponentOccurrence n in f.compOcc)
                {// looks at all the occurences
                    refKey = new byte[0];// clears the current refKey because Inventor needs them to be clear
                    n.GetReferenceKey(ref refKey, 0);// write the reference key of the occurence to the byte[]
                    g += (m_inventorApplication.ActiveDocument.ReferenceKeyManager.KeyToString(refKey)) + "¯\\_(:()_/¯";//convert the refkey to a string and adds a limiter so we can read the string at the start of the add in
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
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
        
        // exports the field
        public void exportField_OnExecute(Inventor.NameValueMap Context)
        {
            if(FieldComponents.Count == 0)
            {
                MessageBox.Show("ERROR: No Field Components!");
                return;
            }
            envMan.SetCurrentEnvironment(envMan.BaseEnvironment);
            FieldSaver exporter = new FieldSaver(m_inventorApplication, FieldComponents);
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