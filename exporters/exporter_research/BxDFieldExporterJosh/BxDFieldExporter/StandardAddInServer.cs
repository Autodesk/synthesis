using System;
using System.Runtime.InteropServices;
using Inventor;
using Microsoft.Win32;
using System.Windows.Forms;
using BxDFieldExporter;
using System.Drawing;
using System.Collections;
using System.Timers;
namespace BxDFieldExporter
{
    /// <summary>
    /// This is the primary AddIn Server class that implements the ApplicationAddInServer interface
    /// that all Inventor AddIns are required to implement. The communication between Inventor and
    /// the AddIn is via the methods on this interface.
    /// </summary>
    /// 

        //TLDR: exports the field


    [GuidAttribute("e50be244-9f7b-4b94-8f87-8224faba8ca1")]
    public class StandardAddInServer : Inventor.ApplicationAddInServer
    {
        // all the global variables
        #region variables
        // Inventor application object.
        public static Inventor.Application m_inventorApplication;// the main inventor application
        private static Ribbon partRibbon;// ribbon that corrosponds to the view of inventor, e.g. Assembly, Part
        private static RibbonTab partTab;// part tab that all the panels will be added to
        ClientNodeResource oRsc;// client resources that buttons will use
        static Document nativeDoc;
        static bool runOnce;
        static RibbonPanel partPanel;
        static RibbonPanel partPanel2;// the ribbon panels that the buttons will be a part of
        static RibbonPanel partPanel3;
        static RibbonPanel partPanel4;// the ribbon panels that the buttons will be a part of
        static ButtonDefinition beginExporter;
        static ButtonDefinition addNewType;
        static ButtonDefinition editType;
        static ButtonDefinition addNewItem;
        static ButtonDefinition accessInnerAssemblies;// contain the buttons that the user can interact with
        static ButtonDefinition removeSubAssembly;
        static ButtonDefinition removeAssembly;
        static ButtonDefinition cancleExport;
        static ButtonDefinition exportField;
        KeyboardEvents keyEvents;
        static bool done;
        static Random rand;// random number genator that can create internal ids
        static ArrayList FieldTypes;// arraylist of all the field properties the user has set
        public static FieldDataType selectedType;// the current group that the user is editing
        static BrowserPanes oPanes;// all the browser panes in the active doc
        object resultObj;
        object other;// some objects to help in searching for ref ids
        object context;
        bool closing;
        static Inventor.BrowserPane oPane;// the application's browser pane
        UserInputEvents UIEvent;// the uievents that react to selections
        static Document oDoc;// a doc to add new highlight sets to
        static HighlightSet oSet;// highlight set that represents the selection
        Inventor.UserInputEventsSink_OnSelectEventHandler click_OnSelectEventDelegate;// handles the selection events
        Inventor.UserInterfaceEventsSink_OnEnvironmentChangeEventHandler enviroment_OnChangeEventDelegate;
        static bool inExportView;// boolean to help in detecting wether or not to react to an event based on wether or not the application is exporting
        static ComponentPropertiesForm form;// form for inputting different properties of the group
        static String m_ClientId;// string the is the id of the application
        static FieldDataType currentSelected;// the current group that the user is editing, needed for the unselection stuff
        static bool found;// boolean to help in searching for objects and the corrosponding actions
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
                m_ClientId = "  ";// create a new client id for the buttons and such
                inExportView = false;// say that we aren't in export view
                m_inventorApplication = addInSiteObject.Application;// get the inventor object
                closing = false;
                FieldTypes = new ArrayList();// clear the field type array
                form = new ComponentPropertiesForm();// init the component form to enter data into
                AddParallelEnvironment();
                UIEvent = m_inventorApplication.CommandManager.UserInputEvents;// get the application's userinput events object
                click_OnSelectEventDelegate = new UserInputEventsSink_OnSelectEventHandler(oUIEvents_OnSelect);// make a new ui event reactor
                UIEvent.OnSelect += click_OnSelectEventDelegate;// add the event reactor to the onselect reactor
                
            } catch (Exception e)
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
                writeBrowserFolderNames();
                foreach (FieldDataType data in FieldTypes)
                {
                    writeSave(data);
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
                return null;
            }
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
                    inExportView = true;
                    addNewType.Enabled = true;
                    editType.Enabled = true;
                    addNewItem.Enabled = true;
                    beginExporter.Enabled = false;// show the correct buttons
                    cancleExport.Enabled = true;
                    exportField.Enabled = true;
                    accessInnerAssemblies.Enabled = true;
                    removeAssembly.Enabled = true;
                    removeSubAssembly.Enabled = true;
                    AssemblyDocument asmDoc = (AssemblyDocument)m_inventorApplication.ActiveDocument;// get the active assembly document
                    BrowserNodeDefinition oDef; // create browsernodedef to use to add browser node to the pane
                    oDoc = m_inventorApplication.ActiveDocument;// get the active document in inventor
                    oPanes = oDoc.BrowserPanes;// get the browserpanes to add
                    oSet = oDoc.CreateHighlightSet();// create a highlightset to add the selected occcurences to
                    oSet.Color = m_inventorApplication.TransientObjects.CreateColor(125, 0, 255);
                    rand = new Random();// create new random num generator to generate internal ids
                    try
                    {// if no browser pane previously created then create a new one
                        ClientNodeResources oRscs = oPanes.ClientNodeResources;
                        oRsc = oRscs.Add(m_ClientId, 1, null);// creat new client node resources
                        oDef = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition("Top Node", 3, null);// create the top node for the browser pane
                        oPane = oPanes.AddTreeBrowserPane("Select Joints", m_ClientId, oDef);// add a new tree browser
                        oPane.Activate();// make the pane be shown to the user
                    }
                    catch (Exception)// we will assume that if the above method fails it is because there is already a browser node
                    {
                        bool found = false;
                        foreach (BrowserPane pane in oPanes)// iterate over the panes in the document
                        {
                            if (pane.Name.Equals("Select Joints"))// if the pane has the correct name then we assume it is what we are looking for
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
                            oDef = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition("Top Node", 3, oRsc);// if the pane was created but the node wasnt then init a node 
                            oPane = oPanes.AddTreeBrowserPane("Select Joints", m_ClientId, oDef);// add a top node to the tree browser
                        }

                    }
                    readSave();// read the save so the user doesn't loose any previous work
                    TimerWatch();// begin the timer watcher to detect deselect


                    /*AssemblyDocument oDocs;
                    oDocs = (AssemblyDocument)m_inventorApplication.ActiveDocument;
                    AssemblyComponentDefinition oCompDef;
                    oCompDef = oDocs.ComponentDefinition;
                    TransientGeometry oTG;
                    oTG = m_inventorApplication.TransientGeometry;
                    WorkPoint oWorkPoint1;
                    oWorkPoint1 = oCompDef.WorkPoints.AddFixed(oTG.CreatePoint(2, 20, 2));
                    WorkPoint oWorkPoint2;
                    oWorkPoint2 = oCompDef.WorkPoints.AddFixed(oTG.CreatePoint(4, 20, 0));
                    WorkPoint oWorkPoint3;
                    oWorkPoint3 = oCompDef.WorkPoints.AddFixed(oTG.CreatePoint(2, 20, 0));
                    UserCoordinateSystemDefinition oUCSDef;
                    oUCSDef = oCompDef.UserCoordinateSystems.CreateDefinition();
                    oUCSDef.SetByThreePoints(oWorkPoint1, oWorkPoint3, oWorkPoint2);
                    UserCoordinateSystem oUCS;
                    oUCS = oCompDef.UserCoordinateSystems.Add(oUCSDef);*/
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
        Inventor.Environment oNewEnv;
        public void AddParallelEnvironment()
        {
            try
            {
                stdole.IPictureDisp beginExporterIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap("C:\\Users\\t_gracj\\Desktop\\git\\synthesis\\exporters\\exporter_research\\BxDFieldExporter\\BxDFieldExporter\\StartExporter16.bmp"));
                stdole.IPictureDisp beginExporterIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap("C:\\Users\\t_gracj\\Desktop\\git\\synthesis\\exporters\\exporter_research\\BxDFieldExporter\\BxDFieldExporter\\StartExporter32.bmp"));
                stdole.IPictureDisp exportFieldIconSmall = PictureDispConverter.ToIPictureDisp(new Bitmap("C:\\Users\\t_gracj\\Desktop\\git\\synthesis\\exporters\\exporter_research\\BxDFieldExporter\\BxDFieldExporter\\ExportField16.bmp"));
                stdole.IPictureDisp exportFieldIconLarge = PictureDispConverter.ToIPictureDisp(new Bitmap("C:\\Users\\t_gracj\\Desktop\\git\\synthesis\\exporters\\exporter_research\\BxDFieldExporter\\BxDFieldExporter\\ExportField32.bmp"));
                // Get the Environments collection
                Environments oEnvironments = m_inventorApplication.UserInterfaceManager.Environments;
                
                // Create a new environment
                oNewEnv = oEnvironments.Add("Field Exporter", "BxD:FieldExporter:Environment", null, beginExporterIconSmall, beginExporterIconLarge);

                // Get the ribbon associated with the assembly environment
                Ribbon oAssemblyRibbon = m_inventorApplication.UserInterfaceManager.Ribbons["Assembly"];

                // Create contextual tabs and panels within them
                RibbonTab oContextualTabOne = oAssemblyRibbon.RibbonTabs.Add("Field Exporter", "BxD:FieldExporter:RibbonTab", "ClientId123", "", false, true);
                
                partPanel = oContextualTabOne.RibbonPanels.Add("Exporter Control", "BxD:FieldExporter:ExporterControl", "{e50be244-9f7b-4b94-8f87-8224faba8ca1}");// inits the part panels
                partPanel2 = oContextualTabOne.RibbonPanels.Add("Model Control", "BxD:FieldExporter:ModelControl", "{e50be244-9f7b-4b94-8f87-8224faba8ca1}");
                
                ControlDefinitions controlDefs = m_inventorApplication.CommandManager.ControlDefinitions;// get the controls for Inventor
                beginExporter = controlDefs.AddButtonDefinition("Start Exporter", "BxD:FieldExporter:StartExporter", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null, beginExporterIconSmall, beginExporterIconLarge, ButtonDisplayEnum.kAlwaysDisplayText);
                beginExporter.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(startExport_OnExecute);

                addNewType = controlDefs.AddButtonDefinition("Add new type", "BxD:FieldExporter:AddNewType", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                addNewType.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(addNewType_OnExecute);
                addNewItem = controlDefs.AddButtonDefinition("Add new assembly", "BxD:FieldExporter:AddNewItem", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                addNewItem.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(addNewAssembly_OnExecute);
                removeAssembly = controlDefs.AddButtonDefinition("Remove assembly from type", "BxD:FieldExporter:RemoveAssemblyFromType", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                removeAssembly.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(removeAssembly_OnExecute);
                removeSubAssembly = controlDefs.AddButtonDefinition("Remove subassembly from type", "BxD:FieldExporter:RemoveSubAssemblyFromType", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                removeSubAssembly.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(removeSubAssembly_OnExecute);
                editType = controlDefs.AddButtonDefinition("Edit type properties", "BxD:FieldExporter:EditTypeProperties", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);// init the button
                editType.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(editTypeProperites_OnExecute);// add the reacting method to the button
                cancleExport = controlDefs.AddButtonDefinition("Cancel export", "BxD:FieldExporter:cancleExport", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                cancleExport.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(cancleExporter_OnExecute);
                exportField = controlDefs.AddButtonDefinition("Export field", "BxD:FieldExporter:exportField", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null, exportFieldIconSmall, exportFieldIconLarge);
                exportField.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(exportField_OnExecute);
                accessInnerAssemblies = controlDefs.AddButtonDefinition("Add new part in subassembly", "BxD:FieldExporter:AccessSubassembly", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                accessInnerAssemblies.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(addNewSubAssembly_OnExecute);
                //partPanel.CommandControls.AddButton(beginExporter, true, true);
                //partPanel.CommandControls.AddButton(cancleExport);
                partPanel.CommandControls.AddButton(exportField, true, true);
                partPanel2.CommandControls.AddButton(removeSubAssembly);
                partPanel2.CommandControls.AddButton(removeAssembly);// add buttons to the part panels
                partPanel2.CommandControls.AddButton(addNewType);
                partPanel2.CommandControls.AddButton(addNewItem);
                partPanel2.CommandControls.AddButton(accessInnerAssemblies);
                partPanel2.CommandControls.AddButton(editType);
                addNewType.Enabled = false;
                editType.Enabled = false;
                addNewItem.Enabled = false;
                beginExporter.Enabled = true;// set the correct button states for not being in export mode
                cancleExport.Enabled = false;
                exportField.Enabled = false;
                addNewType.Enabled = false;
                accessInnerAssemblies.Enabled = false;
                removeAssembly.Enabled = false;
                removeSubAssembly.Enabled = false;
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
        
        public void test(Inventor.NameValueMap Context)
        {
            AssemblyDocument asmDoc = (AssemblyDocument)
                                 m_inventorApplication.ActiveDocument;
            WorkPoint joint = (WorkPoint)m_inventorApplication.CommandManager.Pick// have the user select a leaf occurrence or part
                      (SelectionFilterEnum.kWorkPointFilter, "Select a part to remove");
        }
        // reacts to a selection
        private void oUIEvents_OnSelect(ObjectsEnumerator JustSelectedEntities, ref ObjectCollection MoreSelectedEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
        {
            oSet.Clear();// clear the highlight set to add a new group to the set
            if (SelectionDevice == SelectionDeviceEnum.kGraphicsWindowSelection && inExportView)
            {// if the selection is from the graphical interface and the exporter is active
                foreach (Object sel in JustSelectedEntities)
                {//looks at all things selected
                    if (sel is ComponentOccurrence)
                    {// react only if sel is a part/ assembly
                        foreach (FieldDataType type in FieldTypes)
                        {// looks at all the groups of parts
                            foreach (ComponentOccurrence occ in type.compOcc)
                            {// looks at all the occurences in the types items
                                if (((ComponentOccurrence)sel).Equals(occ))
                                {// if the occurence is contained by anyof the groups then react
                                    foreach(BrowserNode n in oPane.TopNode.BrowserNodes)
                                    {// looks at all the browser nodes in the top node
                                        if (n.BrowserNodeDefinition.Equals(type.node))
                                        {// if the browsernode is the same as the types node then react
                                            n.DoSelect();// select the proper node
                                        }
                                    }
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
                        foreach (FieldDataType f in FieldTypes)
                        {// looks at all the groups of parts
                            if (f.same(((BrowserNodeDefinition)sel)))
                            {// if the browsernode is the same as a the type's node
                                selectedType = f;// set the selected type for the rest of the code to interact with
                                foreach (ComponentOccurrence o in selectedType.compOcc)
                                {// looks at the occurences in the selected type's part list
                                    oSet.AddItem(o);// show the user which parts are selected
                                }
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
                aTimer.Interval = 500;// set time timer reaction interval to 1/2 of a second, we do this to get regular checking without lagging out the computer
                aTimer.AutoReset = true;// auto restarts the timer after the 1/2 second interval
                aTimer.Enabled = true;// starts the timer
                rightDoc = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        [DllImport("user32.dll")]
        public static extern ushort GetKeyState(short nVirtKey);

        public const ushort keyDownBit = 0x80;
        public static bool IsKeyPressed(Keys key)
        {
            return ((GetKeyState((short)key) & keyDownBit) == keyDownBit);
        }
        // reacts to the timer elapsed event, this allows us to select and unselect things as needed
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            found = false;
            if (IsKeyPressed(Keys.ShiftKey) || IsKeyPressed(Keys.ControlKey))
            {
            } else
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
                    foreach (FieldDataType t in FieldTypes)
                    {// looks at all the groups in the fieldtypes
                        if (t.same(node.BrowserNodeDefinition))
                        {// if t is part of that browser node
                            if(!currentSelected.Equals(t))
                            {// is the selected node is no longer selected 
                                found = true;// tell the program it found the node
                                oSet.Clear();// clear the highlighted set in prep to add new occurrences
                                foreach (ComponentOccurrence io in t.compOcc)
                                {// looks at the occurences that are part of the group
                                    oSet.AddItem(io);// add the occurence to the highlighted set
                                }
                                currentSelected = t;// change the selected datatype
                            }
                        }
                    }
                }
            }
            if (!found)
            {// if the program didn't find any selected node then assume that the user deselected 
                oSet.Clear();// clear the set because the user doesn't have anything selected
            }
            if (!m_inventorApplication.ActiveDocument.InternalName.Equals(nativeDoc.InternalName))
            {
                if (rightDoc)
                {
                    rightDoc = false;

                    addNewType.Enabled = false;
                    editType.Enabled = false;
                    addNewItem.Enabled = false;
                    beginExporter.Enabled = true;// sets the correct buttons states
                    cancleExport.Enabled = false;
                    exportField.Enabled = false;
                    accessInnerAssemblies.Enabled = false;
                    removeAssembly.Enabled = false;
                    removeSubAssembly.Enabled = false;

                    oPane.Visible = false;// Hide the browser pane
                }
            }
            else
            {
                if (!rightDoc)
                {
                    rightDoc = true;

                    addNewType.Enabled = true;
                    editType.Enabled = true;
                    addNewItem.Enabled = true;
                    beginExporter.Enabled = false;// sets the correct buttons states
                    cancleExport.Enabled = true;
                    exportField.Enabled = true;
                    accessInnerAssemblies.Enabled = true;
                    removeAssembly.Enabled = true;
                    removeSubAssembly.Enabled = true;

                    oPane.Visible = true;// Hide the browser pane
                    oPane.Activate();
                }

            }
        }
        // adds a new fielddatatype to the array and to the browser pane
        public static BrowserNodeDefinition addType(String name)
        {
            BrowserNodeDefinition def = null;// creates a browsernodedef to be used when creating a new browsernode, null so if adding the node fails the code doesn't freak out
            try
            {
                bool same = false;// used to prevent duplicate type names because they will not save
                foreach (FieldDataType type in FieldTypes)
                {// look at all the fielddata types in data types
                    if (type.Name.Equals(name))
                    {// check to see if it is the same
                        same = true;// if it is then tell the code
                    }
                }
                if (!same)
                {// if there is no duplicate name then add the type
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
                    FieldTypes.Add(new FieldDataType(def));// add the new field data type to the array and use the browsernodedef to refence the object to the browser node
                } else
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
        // removes an assembly from the arraylist of the type
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
                        selected = true;// if one node is selected then we can add the new sub assembly
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
                        if(joint != null) { 
                            foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// look at all the nodes under the top node
                            {
                                if (node.Selected)// find the selected node
                                {
                                    foreach (FieldDataType t in FieldTypes)// look at all the field data types
                                    {
                                        if (t.same(node.BrowserNodeDefinition))// is the fieldDataType is from that browsernode then run
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
        // removes an assembly from the arraylist of the type
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
                        selected = true;// if one node is selected then we can add the new sub assembly
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
                                    foreach (FieldDataType t in FieldTypes)// look at all the field data types
                                    {
                                        if (t.same(node.BrowserNodeDefinition))// is the fieldDataType is from that browsernode then run
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
                            MessageBox.Show("Warning, assembly not found in item group");// if the assembly wasn't found in the group then tell the user
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
        // removes a part from the arraylist of the type
        public void removeSubAssembly_OnExecute(Inventor.NameValueMap Context)
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
                        selected = true;// if one node is selected then we can add the new sub assembly
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
                                  (SelectionFilterEnum.kAssemblyOccurrenceFilter, "Select a part to remove");
                        if (joint != null)
                        {
                            foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// look at all the nodes under the top node
                            {
                                if (node.Selected)// find the selected node
                                {
                                    foreach (FieldDataType t in FieldTypes)// look at all the field data types
                                    {
                                        if (t.same(node.BrowserNodeDefinition))// is the fieldDataType is from that browsernode then run
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
                            MessageBox.Show("Warning, part not found in item group");// if the part wasn't found in the group then tell the user
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
        // adds a part to the arraylist of the type
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
                        selected = true;// if one node is selected then we can add the new sub assembly
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
                                  (SelectionFilterEnum.kAssemblyOccurrenceFilter, "Select a part to add");
                        if (joint != null)
                        {
                            foreach (BrowserNode node in oPane.TopNode.BrowserNodes)// look at all the nodes under the top node
                            {
                                if (node.Selected)// find the selected node
                                {
                                    foreach (FieldDataType t in FieldTypes)// look at all the field data types
                                    {
                                        if (t.same(node.BrowserNodeDefinition))// is the fieldDataType is from that browsernode then run
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
        // edits the properties of the type
        public static void editTypeProperites_OnExecute(Inventor.NameValueMap Context)
        {
            //read from the temp save the proper field values
            form.readFromData(selectedType);
            //show a dialog for the user to enter in values
            form.ShowDialog();
        }
        // adds new property type to the browser pane
        public static void addNewType_OnExecute(Inventor.NameValueMap Context)
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
                writeBrowserFolderNames();// write the browser folder names to the property sets so we can read them next time the program is run
                foreach (FieldDataType data in FieldTypes)
                {// looks at all the groups in fieldtype
                    writeSave(data);// writes the saved data to the property set
                }
                FieldTypes = new ArrayList();// clear the arraylist of groups
                foreach (BrowserNode node in oPane.TopNode.BrowserNodes)
                {// looks at all the nodes under the top node
                    node.Delete();// deletes the nodes
                }
                oPane.Visible = false;// hide the browser pane because we aren't exporting anymore
                addNewType.Enabled = false;
                editType.Enabled = false;
                addNewItem.Enabled = false;
                beginExporter.Enabled = true;// sets the correct buttons states
                cancleExport.Enabled = false;
                exportField.Enabled = false;
                accessInnerAssemblies.Enabled = false;
                removeAssembly.Enabled = false;
                removeSubAssembly.Enabled = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        // read the saved data
        private void readSave()
        {
            try
            {
                ArrayList lis = new ArrayList();// creates an arraylist which will contain occurences to add to the field data type
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
                        name = (String)s.ItemByPropId[2].Value;// reads the names for the field data type
                    }
                }
                String[] names = name.Split(arr);// set names equal to the string of datatype without its limits
                foreach (String n in names)
                {// looks at the strings in names, each one represents a potential data type
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
                                            lis.Add((ComponentOccurrence)obje);// add the occurence to the arraylist in prep to add them tot the field data type
                                        }
                                    }
                                }
                                BrowserNodeDefinition selectedFolder = addType(((String)set.ItemByPropId[10].Value));// create a new browsernodedef with the name from the old datatype
                                FieldDataType field = new FieldDataType(selectedFolder);// create a new field with browser node that was just created as its corrosponding node
                                field.colliderType = (ColliderType)set.ItemByPropId[2].Value;
                                field.X = (double)set.ItemByPropId[3].Value;
                                field.Y = (double)set.ItemByPropId[4].Value;
                                field.Z = (double)set.ItemByPropId[5].Value;
                                field.Scale = (double)set.ItemByPropId[6].Value;// read the data from the properties into the fielddatatype
                                field.Friction = (double)set.ItemByPropId[7].Value;
                                field.Dynamic = (bool)set.ItemByPropId[8].Value;
                                field.Mass = (double)set.ItemByPropId[9].Value;
                                field.compOcc = lis;// set the occurences array to the found occurences
                                FieldTypes.Add(field);// add the field to the datatypes array
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
        // writes the name of the datatypes to a property set so we can read them later
        private void writeBrowserFolderNames()
        {
            String g = "";// a string to add the names of the data types to, we do this so we can read the data at the start of the exporter
            PropertySets sets = m_inventorApplication.ActiveDocument.PropertySets;// the property sets of the document
            PropertySet set = null;// a set to add the data to 
            foreach (BrowserNode node in oPane.TopNode.BrowserNodes)
            {// looks at all the browser node under the top node
                g += node.BrowserNodeDefinition.Label;// add the name of the node to the string so when we read the datatypes we don't lose the name
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
        // writes the data types to the propery set
        private void writeSave(FieldDataType f)
        {
            PropertySets sets = m_inventorApplication.ActiveDocument.PropertySets;// the property sets of the document
            PropertySet set = null;// a set to add the data to 
            try
            {
                set = sets.Add(f.Name);// add new set to the property sets with the name of the data type
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
            String g = "";// string to store the ref key of the occurences of the data type
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
                set.Add(f.colliderType, "colliderType", 2);
                set.Add(f.X, "X", 3);
                set.Add(f.Y, "Y", 4);
                set.Add(f.Z, "Z", 5);
                set.Add(f.Scale, "Scale", 6);// write the data from the datatype into the properties
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
                set.ItemByPropId[6].Value = f.Scale;// write the data from the datatype into the properties
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
            inExportView = false;// tell the event reactors to not react because we are no longer in export mode
            writeBrowserFolderNames();// write the browser folder names to the property sets so we can read them next time the program is run
            foreach (FieldDataType data in FieldTypes)
            {// looks at all the groups in fieldtype
                writeSave(data);// writes the saved data to the property set
            }
            FieldTypes = new ArrayList();// clear the arraylist of groups
            foreach (BrowserNode node in oPane.TopNode.BrowserNodes)
            {// looks at all the nodes under the top node
                node.Delete();// deletes the nodes
            }
            oPane.Visible = false;// hide the browser pane because we aren't exporting anymore
            addNewType.Enabled = false;
            editType.Enabled = false;
            addNewItem.Enabled = false;
            beginExporter.Enabled = true;// sets the correct buttons states
            cancleExport.Enabled = false;
            exportField.Enabled = false;
            accessInnerAssemblies.Enabled = false;
            removeAssembly.Enabled = false;
            removeSubAssembly.Enabled = false;
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