using System;
using System.Runtime.InteropServices;
using Inventor;
using Microsoft.Win32;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;
using System.Xml;
using System.Timers;

namespace InventorAddInBasicGUI2
{
    /// <summary>
    /// This is the primary AddIn Server class that implements the ApplicationAddInServer interface
    /// that all Inventor AddIns are required to implement. The communication between Inventor and
    /// the AddIn is via the methods on this interface.
    /// </summary>


        //TLDR exports the robot to the simulator
    [GuidAttribute("0c9a07ad-2768-4a62-950a-b5e33b88e4a3")]
    public class StandardAddInServer : Inventor.ApplicationAddInServer
    {

        #region data
         
       public static Inventor.Application m_inventorApplication;

        Document oDoc;

        BrowserPanes oPanes;
        
        Boolean FirstTime;
        Boolean inExportView;

        int jointNumber;
        
        Boolean Rotating;

        static Inventor.ButtonDefinition startExport;
        static Inventor.ButtonDefinition exportRobot;
        static Inventor.ButtonDefinition cancelExport;
        static Inventor.ButtonDefinition selectJointInsideJoint;
        static Inventor.ButtonDefinition editDrivers;
        static Inventor.ButtonDefinition editLimits;
        static Inventor.ButtonDefinition test;

        static Inventor.BrowserPane oPane;

        static HighlightSet oSet;

        Inventor.ComboBoxDefinitionSink_OnSelectEventHandler JointsComboBox_OnSelectEventDelegate;
        Inventor.ComboBoxDefinitionSink_OnSelectEventHandler LimitsComboBox_OnSelectEventDelegate;
        Inventor.UserInputEventsSink_OnSelectEventHandler click_OnSelectEventDelegate;

        Form1 form;
        static bool doWerk;

        EditLimits lims;

        UserControl1 control;

        public string m_ClientId;

        static private ComboBoxDefinition JointsComboBox;
        static private ComboBoxDefinition LimitsComboBox;

        ArrayList joints;

        static Document nativeDoc;

        static JointData selectedJointData;

        Inventor.Ribbon partRibbon;

        Inventor.RibbonTab partTab;

        public Object z;
        public Object v;

        public static String pathToSaveTo;

        Inventor.RibbonPanel partPanel;
        Inventor.RibbonPanel partPanel2;
        Inventor.RibbonPanel partPanel3;

        Random rand;
        JointData assemblyJoint;
        ObjectCollection obj;
        BrowserNode node3;
        String addInCLSIDString;

        ClientNodeResource oRsc;

        int i;
        UserInputEvents UIEvent;

        static ArrayList jointList;
        #endregion

        public StandardAddInServer()
        {
        }

        #region ApplicationAddInServer Members

        public void Activate(Inventor.ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            try
            {
                // This method is called by Inventor when it loads the addin.
                // The AddInSiteObject provides access to the Inventor Application object.
                // The FirstTime flag indicates if the addin is loaded for the first time.

                // Initialize AddIn members.


                // TODO: Add ApplicationAddInServer.Activate implementation.
                // e.g. event initialization, command creation etc.
                GuidAttribute addInCLSID;
                addInCLSID = (GuidAttribute)GuidAttribute.GetCustomAttribute(typeof(StandardAddInServer), typeof(GuidAttribute));
                addInCLSIDString = "{" + addInCLSID.Value + "}";
                m_ClientId = "0c9a07ad-2768-4a62-950a-b5e33b88e4a3";
                inExportView = false;
                FirstTime = true;
                control = new UserControl1();
                m_inventorApplication = addInSiteObject.Application;
                jointNumber = 1;

                doWerk = false;

                Rotating = true;
                selectedJointData = null;


                form = new Form1();
                lims = new EditLimits();
                ControlDefinitions controlDefs = m_inventorApplication.CommandManager.ControlDefinitions;
                
                editDrivers = controlDefs.AddButtonDefinition("Edit Drivers", "BxD:RobotExporter:EditDrivers", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                editDrivers.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(EditDrivers_OnExecute);

                editLimits = controlDefs.AddButtonDefinition("Edit Limits", "BxD:RobotExporter:editLimits", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                editLimits.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(EditLimits_OnExecute);

                selectJointInsideJoint = controlDefs.AddButtonDefinition("Select a Joint Inside of a Joint", "BxD:RobotExporter:SelectaJointInsideofaJoint", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                selectJointInsideJoint.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(selectJointInsideJoint_OnExecute);

                startExport = controlDefs.AddButtonDefinition("Start Exporter", "BxD:RobotExporter:StartExporter", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                startExport.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(startExport_OnExecute);

                exportRobot = controlDefs.AddButtonDefinition("Export Robot", "BxD:RobotExporter:ExportRobot", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                exportRobot.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(exportRobot_OnExecute);

                cancelExport = controlDefs.AddButtonDefinition("Cancel Export", "BxD:RobotExporter:CancelExport", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                cancelExport.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(cancelExport_OnExecute);

                
                rand = new Random();
                test = controlDefs.AddButtonDefinition("test", "BxD:RobotExporter:test", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                test.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(test_OnExecute);
                // Get the assembly ribbon.
                partRibbon = m_inventorApplication.UserInterfaceManager.Ribbons["Assembly"];
                partTab = partRibbon.RibbonTabs.Add("Robot Exporter", "BxD:RobotExporter", "{55e5c0be-2fa4-4c95-a1f6-4782ea7a3258}");
                partPanel3 = partTab.RibbonPanels.Add("Exporter Control", "BxD:RobotExporter:ExporterControl", "{55e5c0be-2fa4-4c95-a1f6-4782ea7a3258}");
                partPanel = partTab.RibbonPanels.Add("Joints", "BxD:RobotExporter:Joints", "{55e5c0be-2fa4-4c95-a1f6-4782ea7a3258}");
                partPanel2 = partTab.RibbonPanels.Add("Limits", "BxD:RobotExporter:Limits", "{55e5c0be-2fa4-4c95-a1f6-4782ea7a3258}");

                JointsComboBox = m_inventorApplication.CommandManager.ControlDefinitions.AddComboBoxDefinition("Driver", "Autodesk:SimpleAddIn:Driver", CommandTypesEnum.kShapeEditCmdType, 100, addInCLSIDString, "Driver", "Driver", Type.Missing, Type.Missing, ButtonDisplayEnum.kDisplayTextInLearningMode);
                LimitsComboBox = m_inventorApplication.CommandManager.ControlDefinitions.AddComboBoxDefinition("Has Limits", "Autodesk:SimpleAddIn:HasLimits", CommandTypesEnum.kShapeEditCmdType, 100, addInCLSIDString, "Has Limits", "Has Limits", Type.Missing, Type.Missing, ButtonDisplayEnum.kDisplayTextInLearningMode);

                //add some initial items to the comboboxes
                JointsComboBox.AddItem("No Driver", 0);
                JointsComboBox.AddItem("Motor", 0);
                JointsComboBox.AddItem("Servo", 0);
                JointsComboBox.AddItem("Bumper Pneumatic", 0);
                JointsComboBox.AddItem("Relay Pneumatic", 0);
                JointsComboBox.AddItem("Worm Screw", 0);
                JointsComboBox.AddItem("Dual Motor", 0);
                JointsComboBox.ListIndex = 1;
                JointsComboBox.ToolTipText = JointsComboBox.Text;
                JointsComboBox.DescriptionText = "Slot width: " + JointsComboBox.Text;

                JointsComboBox_OnSelectEventDelegate = new ComboBoxDefinitionSink_OnSelectEventHandler(JointsComboBox_OnSelect);
                JointsComboBox.OnSelect += JointsComboBox_OnSelectEventDelegate;
                partPanel.CommandControls.AddComboBox(JointsComboBox);

                LimitsComboBox.AddItem("No Limits", 0);
                LimitsComboBox.AddItem("Limits", 0);
                LimitsComboBox.ListIndex = 1;
                LimitsComboBox.ToolTipText = JointsComboBox.Text;
                LimitsComboBox.DescriptionText = "Slot width: " + JointsComboBox.Text;

                partPanel3.CommandControls.AddButton(startExport);
                partPanel3.CommandControls.AddButton(exportRobot);
                partPanel3.CommandControls.AddButton(cancelExport);
                partPanel3.CommandControls.AddButton(selectJointInsideJoint);
                partPanel.CommandControls.AddButton(editDrivers);
                //partPanel3.CommandControls.AddButton(test);
                LimitsComboBox_OnSelectEventDelegate = new ComboBoxDefinitionSink_OnSelectEventHandler(LimitsComboBox_OnSelect);
                LimitsComboBox.OnSelect += LimitsComboBox_OnSelectEventDelegate;
                partPanel2.CommandControls.AddComboBox(LimitsComboBox);
                partPanel2.CommandControls.AddButton(editLimits);
                UIEvent = m_inventorApplication.CommandManager.UserInputEvents;
                click_OnSelectEventDelegate = new UserInputEventsSink_OnSelectEventHandler(oUIEvents_OnSelect);
                UIEvent.OnSelect += click_OnSelectEventDelegate;

                JointsComboBox.Enabled = false;
                LimitsComboBox.Enabled = false;
                editDrivers.Enabled = false;
                editLimits.Enabled = false;
                exportRobot.Enabled = false;
                startExport.Enabled = true;
                cancelExport.Enabled = false;
                selectJointInsideJoint.Enabled = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

        }

        public void Deactivate()
        {
            // This method is called by Inventor when the AddIn is unloaded.
            // The AddIn will be unloaded either manually by the user or
            // when the Inventor session is terminated

            // TODO: Add ApplicationAddInServer.Deactivate implementation

            // Release objects.
            Marshal.ReleaseComObject(m_inventorApplication);
            m_inventorApplication = null;

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

        public void selectJointInsideJoint_OnExecute(Inventor.NameValueMap Context)
        {
            try
            {
                ComponentOccurrence joint;
                AssemblyDocument asmDoc = (AssemblyDocument)
                    m_inventorApplication.ActiveDocument;
                joint = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick
                    (SelectionFilterEnum.kAssemblyLeafOccurrenceFilter, "Select an assembly to add");
                foreach (JointData j in jointList)
                {
                    if (joint.Equals(j.jointOfType.AffectedOccurrenceOne) || joint.Equals(j.jointOfType.AffectedOccurrenceTwo))
                    {
                        foreach (BrowserNode node in oPane.TopNode.BrowserNodes)
                        {
                            if (j.same(node.BrowserNodeDefinition))
                            {
                                node.DoSelect();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        //starts the exporter
        public void startExport_OnExecute(Inventor.NameValueMap Context)
        {
            try
            {
                if (!inExportView)
                {
                    nativeDoc = m_inventorApplication.ActiveDocument;
                    BrowserNodeDefinition def = null;
                    inExportView = true;
                    JointsComboBox.Enabled = true;
                    LimitsComboBox.Enabled = true;
                    exportRobot.Enabled = true;//set buttons to proper state
                    startExport.Enabled = false;
                    cancelExport.Enabled = true;
                    editDrivers.Enabled = true;
                    editLimits.Enabled = true;
                    selectJointInsideJoint.Enabled = true;
                    AssemblyDocument asmDoc = (AssemblyDocument)m_inventorApplication.ActiveDocument;
                    BrowserNodeDefinition oDef;
                    jointList = new ArrayList();
                    joints = new ArrayList();
                    i = 1;
                    oDoc = m_inventorApplication.ActiveDocument;
                    oPanes = oDoc.BrowserPanes;
                    ObjectCollection oOccurrenceNodes;
                    selectedJointData = null;
                    oOccurrenceNodes = m_inventorApplication.TransientObjects.CreateObjectCollection();
                    JointsComboBox.Enabled = false;
                    LimitsComboBox.Enabled = false;
                    editDrivers.Enabled = false;
                    editLimits.Enabled = false;
                    try
                    {// if no browser pane previously created then create a new one
                        ClientNodeResources oRscs = oPanes.ClientNodeResources;
                        oRsc = oRscs.Add(m_ClientId, 1, null);
                        oDef = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition("Top Node", 3, null);
                        oPane = oPanes.AddTreeBrowserPane("Select Joints", m_ClientId, oDef);
                        FirstTime = false;
                        oPane.Activate();
                    }
                    catch (Exception e)
                    {
                        bool found = false;
                        foreach (BrowserPane pane in oPanes)
                        {
                            if (pane.Name.Equals("Select Joints"))
                            {

                                oPane = pane;
                                foreach (BrowserFolder f in oPane.TopNode.BrowserFolders)
                                {
                                    f.Delete();
                                }
                                foreach (BrowserNode f in oPane.TopNode.BrowserNodes)
                                {
                                    f.Delete();
                                }
                                oPane.Visible = true;
                                oPane.Activate();// is there is already a pane then use that
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            oDef = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition("Top Node", 3, oRsc);// if the pane was created but the node wasnt then init a node 
                            oPane = oPanes.AddTreeBrowserPane("Select Joints", m_ClientId, oDef);
                        }
                    }

                    oSet = oDoc.CreateHighlightSet();
                    oSet.Color = m_inventorApplication.TransientObjects.CreateColor(125, 0, 255);

                    readSave();
                    foreach (ComponentOccurrence c in ((AssemblyDocument)m_inventorApplication.ActiveDocument).ComponentDefinition.Occurrences)
                    {
                        foreach (AssemblyJoint j in c.Joints)
                        {// look at all joints inside of the main doc
                            bool found = false;
                            foreach (JointData d in jointList)
                            {// looks at all joints in the joint data to check for duplicates
                                if (d.equals(j))
                                {
                                    found = true;
                                }
                            }
                            if (!found)
                            {// if there isn't a duplicate then add part to browser folder
                                int th = rand.Next();
                                ClientNodeResources oNodeRescs;
                                ClientNodeResource oRes = null;
                                oNodeRescs = oPanes.ClientNodeResources;
                                try
                                {
                                    oRes = oNodeRescs.Add("MYID", 1, null);
                                }
                                catch (Exception)
                                {
                                    oRes = oPanes.ClientNodeResources.ItemById("MYID", 1);
                                }
                                def = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition("Joint " + jointNumber.ToString(), th, oRes);
                                oPane.TopNode.AddChild(def);
                                joints.Add(j.AffectedOccurrenceOne);
                                joints.Add(j.AffectedOccurrenceTwo);
                                i++;
                                assemblyJoint = new JointData(j, "Joint " + jointNumber.ToString());
                                try
                                {
                                    assemblyJoint.LowerLim = ((ModelParameter)j.Definition.AngularPositionStartLimit).ModelValue;
                                    assemblyJoint.UpperLim = ((ModelParameter)j.Definition.AngularPositionEndLimit).ModelValue;
                                    assemblyJoint.HasLimits = true;
                                }
                                catch (Exception) { 
                                    assemblyJoint.LowerLim = ((ModelParameter)j.Definition.LinearPositionStartLimit).ModelValue;
                                    assemblyJoint.UpperLim = ((ModelParameter)j.Definition.LinearPositionEndLimit).ModelValue;
                                    assemblyJoint.HasLimits = true;
                                }
                                jointNumber++;
                                jointList.Add(assemblyJoint);// add new joint data to the array
                            }
                        }
                        if (c.SubOccurrences.Count > 0)
                        {// if there are parts/ assemblies inside the assembly then look at it for joints
                            foreach (ComponentOccurrence v in c.SubOccurrences)
                            {
                                FindSubOccurences(v);
                            }
                        }
                    }
                    Boolean contains = false;
                    foreach (ComponentOccurrence c in asmDoc.ComponentDefinition.Occurrences)
                    {// looks at all parts/ assemblies in the main assembly
                        contains = false;
                        foreach (ComponentOccurrence j in joints)
                        {
                            if ((j.Equals(c)))
                            {// checks is the part/ assembly is in a joint
                                contains = true;
                            }
                        }
                        if (!contains)
                        {// if the assembly/ part isn't part of a joint then hide it
                            c.Enabled = false;
                        }
                    }
                    TimerWatch();
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
        //reacts to a select event
        private void oUIEvents_OnSelect(ObjectsEnumerator JustSelectedEntities, ref ObjectCollection MoreSelectedEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
        {
            try
            {
                if (JustSelectedEntities.Count == 1)
                {
                    if (SelectionDevice == SelectionDeviceEnum.kGraphicsWindowSelection && inExportView)
                    {// && JustSelectedEntities.Count == 1)
                     // only reacts if the select comes from graphics window and we are exporting
                        foreach (Object sel in JustSelectedEntities)
                        {
                            //looks at all things selected
                            if (sel is ComponentOccurrence)
                            {// react only if sel is a part/ assembly// if the code didn't find sel inside of a browser folder then look at nodes
                                foreach (JointData joint in jointList)
                                {
                                    if ((sel).Equals(joint.jointOfType.AffectedOccurrenceOne) || sel.Equals(joint.jointOfType.AffectedOccurrenceTwo))
                                    {
                                        foreach (BrowserNode node in oPane.TopNode.BrowserNodes)
                                        {
                                            if (joint.same(node.BrowserNodeDefinition))
                                            {
                                                node.DoSelect();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (SelectionDevice == SelectionDeviceEnum.kBrowserSelection && inExportView)
                    {// if in export mode and select comes from the browser, cool feature, this is called by BrowserNode.DoSelect() so I had all reacts go through here

                        foreach (Object sel in JustSelectedEntities)
                        {
                            if (sel is BrowserNodeDefinition)
                            {
                                foreach (JointData joint in jointList)
                                {
                                    if (joint.same(((BrowserNodeDefinition)sel)))
                                    {
                                        oSet.Clear();
                                        selectedJointData = joint;
                                        oSet.AddItem(joint.jointOfType.AffectedOccurrenceOne);
                                        oSet.AddItem(joint.jointOfType.AffectedOccurrenceTwo);
                                        JointsComboBox.Enabled = true;
                                        LimitsComboBox.Enabled = true;
                                        editDrivers.Enabled = true;
                                        editLimits.Enabled = true;
                                        if (selectedJointData.jointOfType.Definition.JointType == AssemblyJointTypeEnum.kCylindricalJointType ||
                                            selectedJointData.jointOfType.Definition.JointType == AssemblyJointTypeEnum.kSlideJointType)
                                        {// if the assembly joint is linear
                                            JointTypeLinear();
                                        }
                                        else
                                        {// set the combo box choices to rotating
                                            JointTypeRotating();
                                        }
                                        SwitchSelectedJoint(selectedJointData.Driver);// set selected joint type in the combo box to the correct one
                                        SwitchSelectedLimit(selectedJointData.HasLimits);// set selected limit choice in the combo box to the correct one
                                    }
                                }
                            }

                        }
                    }
                }
                else
                {
                    ObjectCollection Obj = m_inventorApplication.TransientObjects.CreateObjectCollection();
                    oSet.Clear();
                    if (inExportView)
                    {
                        foreach (Object o in JustSelectedEntities)
                        {
                            if (o is BrowserNodeDefinition)
                            {
                                foreach (JointData joint in jointList)
                                {
                                    if (joint.same(((BrowserNodeDefinition)o)))
                                    {
                                        selectedJointData = joint;
                                        oSet.AddItem(joint.jointOfType.AffectedOccurrenceOne);
                                        oSet.AddItem(joint.jointOfType.AffectedOccurrenceTwo);
                                    }
                                }
                            }
                            else if (o is ComponentOccurrence)
                            {
                                foreach (JointData joint in jointList)
                                {
                                    if ((o).Equals(joint.jointOfType.AffectedOccurrenceOne) || o.Equals(joint.jointOfType.AffectedOccurrenceTwo))
                                    {
                                        foreach (BrowserNode node in oPane.TopNode.BrowserNodes)
                                        {
                                            if (joint.same(node.BrowserNodeDefinition))
                                            {
                                                Obj.Add(node.BrowserNodeDefinition);
                                              //  Obj.Add(node.BrowserNodeDefinition);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        m_inventorApplication.ActiveDocument.SelectSet.SelectMultiple(Obj);
                    }
                }
            }catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        // switch the selected 
        public void SwitchSelectedLimit(bool HasLimits)
        {
            doWerk = false;// make sure the limit combo box reactor doesn't react
            if (HasLimits)
            {  // if the joint data has limits then change the combo box selection
                LimitsComboBox.ListIndex = 2;
            }
            else
            { // if the joint data doesn't have limits then change the combo box selection
                LimitsComboBox.ListIndex = 1;
            }
            doWerk = true;// reanable the limits combo box reactor
        }
        // switch selection in combo box to the correct choice
        private void SwitchSelectedJoint(DriveTypes driver)
        {//  gets passed the DriveType of the selected joints
            doWerk = false;// disable the combo box selection reactor
            if (Rotating)
            {// if rotating read from Driver and select proper choice
                if (driver == DriveTypes.Motor)
                {
                    JointsComboBox.ListIndex = 2;
                } else if (driver == DriveTypes.Servo)
                {
                    JointsComboBox.ListIndex = 3;
                } else if (driver == DriveTypes.BumperPnuematic)
                {
                    JointsComboBox.ListIndex = 4;
                } else if (driver == DriveTypes.RelayPneumatic)
                {
                    JointsComboBox.ListIndex = 5;
                } else if (driver == DriveTypes.WormScrew)
                {
                    JointsComboBox.ListIndex = 6;
                } else if (driver == DriveTypes.DualMotor)
                {
                    JointsComboBox.ListIndex = 7;
                } else
                {
                    JointsComboBox.ListIndex = 1;
                }
            } else
            {// if linear read from Driver and select proper choice
                if (driver == DriveTypes.Elevator)
                {
                    JointsComboBox.ListIndex = 2;
                } else if (driver == DriveTypes.BumperPnuematic)
                {
                    JointsComboBox.ListIndex = 3;
                } else if (driver == DriveTypes.RelayPneumatic)
                {
                    JointsComboBox.ListIndex = 4;
                } else if (driver == DriveTypes.WormScrew)
                {
                    JointsComboBox.ListIndex = 5;
                } else
                {
                    JointsComboBox.ListIndex = 1;
                }
            }
            doWerk = true;// reenable the combo box selection reactor
        }
        // if the joint is rotating then set the proper combo box choices
        private void JointTypeRotating()
        {
            try
            {
                doWerk = false; // tells the reactor method to ignore the selection changes
                Rotating = true; // sets the type of joint to rotating
                JointsComboBox.Clear();
                JointsComboBox.AddItem("No Driver", 0);
                JointsComboBox.AddItem("Motor", 0);
                JointsComboBox.AddItem("Servo", 0);
                JointsComboBox.AddItem("Bumper Pneumatic", 0);
                JointsComboBox.AddItem("Relay Pneumatic", 0);
                JointsComboBox.AddItem("Worm Screw", 0);
                JointsComboBox.AddItem("Dual Motor", 0);
                JointsComboBox.ListIndex = 1;
                JointsComboBox.ToolTipText = JointsComboBox.Text;
                JointsComboBox.DescriptionText = "Slot width: " + JointsComboBox.Text;
                doWerk = true;// reenables the combo box reactor method
            } catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        // if the joint is linear then set the proper combo box choices
        private void JointTypeLinear()
        {
            doWerk = false; // tells the reactor method to ignore the selection changes
            Rotating = false; // sets the type of joint to not rotating
            JointsComboBox.Clear();
            JointsComboBox.AddItem("No Driver", 0);
            JointsComboBox.AddItem("Elevator", 0);
            JointsComboBox.AddItem("Bumper Pneumatic", 0);
            JointsComboBox.AddItem("Relay Pneumatic", 0);
            JointsComboBox.AddItem("Worm Screw", 0);
            JointsComboBox.ListIndex = 1;
            JointsComboBox.ToolTipText = JointsComboBox.Text;
            JointsComboBox.DescriptionText = "Slot width: " + JointsComboBox.Text;
            doWerk = true; // reenables the combo box reactor method
        }
        //BrowserNodeDefinition def;
        private void readSave()
        {
            BrowserNodeDefinition def;
            object other;
            object context;
            object resultObj;
            byte[] refObj;
            try
            {
                PropertySets sets = m_inventorApplication.ActiveDocument.PropertySets;
                other = null;
                context = null;
                JointData j;
                int k = 0;
                resultObj = null;
                foreach (PropertySet p in sets)
                {
                    if (p.DisplayName.Equals("Number of Joints"))
                    {
                        jointNumber = (int) p.ItemByPropId[2].Value;
                        k = (int)p.ItemByPropId[2].Value;
                    }
                }
                for (int n = 0; n <= jointNumber; n++)
                {
                    foreach (PropertySet p in sets)
                    {
                        if (p.Name.Equals("Joint " + n.ToString()))
                        {
                            resultObj = null;
                            context = null;
                            other = null;
                            refObj = new byte[0];
                            m_inventorApplication.ActiveDocument.ReferenceKeyManager.
                                    StringToKey(((String)p.ItemByPropId[2].Value), ref refObj);
                            if (m_inventorApplication.ActiveDocument.ReferenceKeyManager.CanBindKeyToObject(refObj, 0, out resultObj, out context))
                            {
                                object obje = m_inventorApplication.ActiveDocument.ReferenceKeyManager.
                                        BindKeyToObject(refObj, 0, out other);
                                int th = rand.Next();
                                ClientNodeResources oNodeRescs;
                                ClientNodeResource oRes = null;
                                oNodeRescs = oPanes.ClientNodeResources;
                                try
                                {
                                    oRes = oNodeRescs.Add("MYID", 1, null);
                                }
                                catch (Exception)
                                {
                                    oRes = oPanes.ClientNodeResources.ItemById("MYID", 1);
                                }
                                def = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition("Joint " + n.ToString(), th, oRes);
                               // ((BrowserFolder)def).AllowRename = false;
                                oPane.TopNode.AddChild(def);
                                joints.Add(((AssemblyJoint)obje).AffectedOccurrenceOne);
                                joints.Add(((AssemblyJoint)obje).AffectedOccurrenceTwo);
                                k++;
                                j = new JointData(((AssemblyJoint)obje), ((String)p.ItemByPropId[27].Value));
                                j.RefKey = (String)p.ItemByPropId[2].Value;
                                j.Driver = (DriveTypes)p.ItemByPropId[3].Value;
                                j.Wheel = (WheelType)p.ItemByPropId[4].Value;
                                j.Friction = (FrictionLevel)p.ItemByPropId[5].Value;
                                j.Diameter = (InternalDiameter)p.ItemByPropId[6].Value;
                                j.Pressure = (Pressure)p.ItemByPropId[7].Value;
                                j.Stages = (Stages)p.ItemByPropId[8].Value;
                                j.PWMport = (double)p.ItemByPropId[9].Value;
                                j.PWMport2 = (double)p.ItemByPropId[10].Value;
                                j.CANport = (double)p.ItemByPropId[11].Value;
                                j.CANport2 = (double)p.ItemByPropId[12].Value;
                                j.DriveWheel = (bool)p.ItemByPropId[13].Value;
                                j.PWM = (bool)p.ItemByPropId[14].Value;
                                j.InputGear = (double)p.ItemByPropId[15].Value;
                                j.OutputGear = (double)p.ItemByPropId[16].Value;
                                j.SolenoidPortA = (double)p.ItemByPropId[17].Value;
                                j.SolenoidPortB = (double)p.ItemByPropId[18].Value;
                                j.RelayPort = (double)p.ItemByPropId[19].Value;
                                j.HasBrake = (bool)p.ItemByPropId[20].Value;
                                j.BrakePortA = (double)p.ItemByPropId[21].Value;
                                j.BrakePortB = (double)p.ItemByPropId[22].Value;
                                j.UpperLim = (double)p.ItemByPropId[23].Value;
                                j.LowerLim = (double)p.ItemByPropId[24].Value;
                                j.HasLimits = (bool)p.ItemByPropId[25].Value;
                                j.Rotating = (bool)p.ItemByPropId[26].Value;
                                jointList.Add(j);
                                
                            } 
                        }
                    }
                }
            } catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        
        private void writeNumJoints()
        {
            PropertySets sets = m_inventorApplication.ActiveDocument.PropertySets;
            PropertySet set = null;
            try
            {
                set = sets.Add("Number of Joints");
            }
            catch (Exception e)
            {
                foreach (PropertySet s in sets)
                {
                    if (s.DisplayName.Equals("Number of Joints"))
                    {
                        set = s;
                    }
                }
            }
            try
            {
                set.Add(jointNumber, "Number of joints", 2);
            }
            catch (Exception e)
            {
                set.ItemByPropId[2].Value = jointNumber;
            }
        }

        private void writeSave(JointData j)
        {
            PropertySets sets = m_inventorApplication.ActiveDocument.PropertySets;
            PropertySet set = null;
            try
            {
                set = sets.Add(j.Name);
            }
            catch (Exception e)
            {
                foreach (PropertySet s in sets)
                {
                    if (s.DisplayName.Equals(j.Name))
                    {
                        set = s;
                    }
                }
            }
            try
            {
                set.Add(j.RefKey, "RefKey", 2);
                set.Add(j.Driver, "Driver", 3);
                set.Add(j.Wheel, "Wheel", 4);
                set.Add(j.Friction, "Friction", 5);
                set.Add(j.Diameter, "Diameter", 6);
                set.Add(j.Pressure, "Pressure", 7);
                set.Add(j.Stages, "Stages", 8);
                set.Add(j.PWMport, "PWMport", 9);
                set.Add(j.PWMport2, "PWMport2", 10);
                set.Add(j.CANport, "CANport", 11);
                set.Add(j.CANport2, "CANport2", 12);
                set.Add(j.DriveWheel, "DriveWheel", 13);
                set.Add(j.PWM, "PWM", 14);
                set.Add(j.InputGear, "InputGear", 15);
                set.Add(j.OutputGear, "OutputGear", 16);
                set.Add(j.SolenoidPortA, "SolenoidPortA", 17);
                set.Add(j.SolenoidPortB, "SolenoidPortB", 18);
                set.Add(j.RelayPort, "RelayPort", 19);
                set.Add(j.HasBrake, "HasBrake", 20);
                set.Add(j.BrakePortA, "BrakePortA", 21);
                set.Add(j.BrakePortB, "BrakePortB", 22);
                set.Add(j.UpperLim, "UpperLim", 23);
                set.Add(j.LowerLim, "LowerLim", 24);
                set.Add(j.HasLimits, "HasLimits", 25);
                set.Add(j.Rotating, "Rotating", 26);
                set.Add(j.Name, "Name", 27);
            }
            catch (Exception e)
            {
                set.ItemByPropId[2].Value = j.RefKey;
                set.ItemByPropId[3].Value = j.Driver;
                set.ItemByPropId[4].Value = j.Wheel;
                set.ItemByPropId[5].Value = j.Friction;
                set.ItemByPropId[6].Value = j.Diameter;
                set.ItemByPropId[7].Value = j.Pressure;
                set.ItemByPropId[8].Value = j.Stages;
                set.ItemByPropId[9].Value = j.PWMport;
                set.ItemByPropId[10].Value = j.PWMport2;
                set.ItemByPropId[11].Value = j.CANport;
                set.ItemByPropId[12].Value = j.CANport2;
                set.ItemByPropId[13].Value = j.DriveWheel;
                set.ItemByPropId[14].Value = j.PWM;
                set.ItemByPropId[15].Value = j.InputGear;
                set.ItemByPropId[16].Value = j.OutputGear;
                set.ItemByPropId[17].Value = j.SolenoidPortA;
                set.ItemByPropId[18].Value = j.SolenoidPortB;
                set.ItemByPropId[19].Value = j.RelayPort;
                set.ItemByPropId[20].Value = j.HasBrake;
                set.ItemByPropId[21].Value = j.BrakePortA;
                set.ItemByPropId[22].Value = j.BrakePortB;
                set.ItemByPropId[23].Value = j.UpperLim;
                set.ItemByPropId[24].Value = j.LowerLim;
                set.ItemByPropId[25].Value = j.HasLimits;
                set.ItemByPropId[26].Value = j.Rotating;
                set.ItemByPropId[27].Value = j.Name;
            }
        }
        //test button for doing experimental things
        public void test_OnExecute(Inventor.NameValueMap Context)
        {
            AssemblyDocument asmDoc = (AssemblyDocument)
                           m_inventorApplication.ActiveDocument;
            ComponentOccurrence assembly = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick
                      (SelectionFilterEnum.kAssemblyLeafOccurrenceFilter, "Select an assembly to open");
            obj = m_inventorApplication.TransientObjects.CreateObjectCollection();
            node3 = oPane.GetBrowserNodeFromObject(assembly);// add joint
            obj.Add(node3);
            oPane.AddBrowserFolder("Joint " + i, obj); // add browser folder to browser pane
            i++;
        }
        // looks at subcomponents for joints
        public void HideInside(ComponentOccurrence c)
        {
            bool contains = false;
            foreach (ComponentOccurrence j in joints)// looks at all parts/ assemblies inside of the joints list
            {

                if ((j.Equals(c)))
                {
                    contains = true;// if the selected part/ assembly is a part of a joint then don't disable it 
                }
            }
            if (!contains)
            {// if the part/ assembly isn't a part of a joint the disable it
                c.Enabled = false;
            }
            if (c.SubOccurrences.Count > 0)
            {
                foreach (ComponentOccurrence v in c.SubOccurrences)
                {
                    HideInside(v);// if the assembly has parts/ assemblies then look at those
                }
            }
        }
        // looks at sub assembly for joints
        public void FindSubOccurences(ComponentOccurrence occ)
        {
            BrowserNodeDefinition def;
            try
            {
                foreach (AssemblyJoint j in occ.Joints)
                {// look at all joints inside of the main doc
                    bool found = false;
                    foreach (JointData d in jointList)
                    {// looks at all joints in the joint data to check for duplicates
                        if (d.equals(j))
                        {
                            found = true;
                        }
                    }
                    if (!found)
                    {// if there isn't a duplicate then add part to browser folder
                        int th = rand.Next();
                        ClientNodeResources oNodeRescs;
                        ClientNodeResource oRes = null;
                        oNodeRescs = oPanes.ClientNodeResources;
                        try
                        {
                            oRes = oNodeRescs.Add("MYID", 1, null);
                        }
                        catch (Exception)
                        {
                            oRes = oPanes.ClientNodeResources.ItemById("MYID", 1);
                        }
                        def = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition("Joint " + jointNumber.ToString(), th, oRes);
                        oPane.TopNode.AddChild(def);
                        joints.Add(j.AffectedOccurrenceOne);
                        joints.Add(j.AffectedOccurrenceTwo);
                        i++;
                        assemblyJoint = new JointData(j, "Joint " + jointNumber.ToString());
                        jointNumber++;
                        jointList.Add(assemblyJoint);// add new joint data to the array
                    }
                }
                if (occ.SubOccurrences.Count > 0)
                {// if there are parts/ assemblies inside the assembly then look at it for joints
                    foreach (ComponentOccurrence v in occ.SubOccurrences)
                    {
                        FindSubOccurences(v);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        // exports the robot
        public void exportRobot_OnExecute(Inventor.NameValueMap Context)
        {
            control.saveFile();// save the file
            inExportView = false; // exit the export view
            SwitchSelectedJoint(DriveTypes.NoDriver);// select the proper combo box
            AssemblyDocument asmDoc = (AssemblyDocument) m_inventorApplication.ActiveDocument;
            foreach (ComponentOccurrence c in asmDoc.ComponentDefinition.Occurrences)
            {
                c.Enabled = true;// enable all parts/ assemblies in the doc
            }
            JointsComboBox.Enabled = false;
            LimitsComboBox.Enabled = false;
            exportRobot.Enabled = false;
            startExport.Enabled = true;// change the button states to correct ones
            cancelExport.Enabled = false;
            editDrivers.Enabled = false;
            editLimits.Enabled = false;
            selectJointInsideJoint.Enabled = false;
            oPane.Visible = false; // hide browser pane
            //writeNumJoints();
            foreach (JointData j in jointList)
            {
              //  writeSave(j);
            }
            jointList = new ArrayList(); // clear the jointList
            foreach (BrowserNode folder in oPane.TopNode.BrowserNodes)
            {
                folder.Delete();// delete the folders
            }
            try
            {
                m_inventorApplication.ActiveDocument.Save();
            } catch(Exception e)
            {

            }
        }
        // cancels the export
        static bool rightDoc;
        private void TimerWatch()
        {
            try
            {
                System.Timers.Timer aTimer = new System.Timers.Timer();
                aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                aTimer.Interval = 500;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
                rightDoc = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        static bool found;
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
           
            found = false;
            foreach (BrowserNode node in oPane.TopNode.BrowserNodes)
            {
                if (node.Selected)
                {
                    foreach (JointData t in jointList)
                    {
                        if (t.same(node.BrowserNodeDefinition))
                        {
                            if (selectedJointData.Name.Equals(t.Name))
                            {
                                found = true;
                                selectedJointData = t;
                           }
                        }
                    }
                }
            }
            if (!found)
            {
                selectedJointData = null;
                oSet.Clear();
                doWerk = false;
                JointsComboBox.ListIndex = 1;
                LimitsComboBox.ListIndex = 1;
                doWerk = true;
                JointsComboBox.Enabled = false;
                LimitsComboBox.Enabled = false;
                editDrivers.Enabled = false;
                editLimits.Enabled = false;
            }
            if (!m_inventorApplication.ActiveDocument.InternalName.Equals(nativeDoc.InternalName))
            {
                if (rightDoc)
                {
                    rightDoc = false;

                    JointsComboBox.Enabled = false;
                    LimitsComboBox.Enabled = false;
                    exportRobot.Enabled = false;// change buttons the proper state
                    startExport.Enabled = true;
                    cancelExport.Enabled = false;
                    editDrivers.Enabled = false;
                    editLimits.Enabled = false;
                    selectJointInsideJoint.Enabled = false;

                    oPane.Visible = false;// Hide the browser pane
                }
            } else
            {
                if (!rightDoc)
                {
                    rightDoc = true;

                    JointsComboBox.Enabled = true;
                    LimitsComboBox.Enabled = true;
                    exportRobot.Enabled = true;// change buttons the proper state
                    startExport.Enabled = false;
                    cancelExport.Enabled = true;
                    editDrivers.Enabled = true;
                    editLimits.Enabled = true;
                    selectJointInsideJoint.Enabled = true;

                    oPane.Visible = true;// Hide the browser pane
                    oPane.Activate();
                }

            }
        }
        public void cancelExport_OnExecute(Inventor.NameValueMap Context)
        {
            try
            {
                SwitchSelectedJoint(DriveTypes.NoDriver);// change combo box selections to default
                SwitchSelectedLimit(false);
                inExportView = false;// exit export view
                AssemblyDocument asmDoc = (AssemblyDocument)m_inventorApplication.ActiveDocument;
                foreach (ComponentOccurrence c in asmDoc.ComponentDefinition.Occurrences)
                {
                    c.Enabled = true;
                }
                JointsComboBox.Enabled = false;
                LimitsComboBox.Enabled = false;
                exportRobot.Enabled = false;// change buttons the proper state
                startExport.Enabled = true;
                cancelExport.Enabled = false;
                editDrivers.Enabled = false;
                editLimits.Enabled = false;
                selectJointInsideJoint.Enabled = false;

                oPane.Visible = false;// Hide the browser pane
                writeNumJoints();

                foreach (JointData l in jointList)
                {
                    writeSave(l);
                }
                jointList = new ArrayList();// clear jointList
                foreach (BrowserNode folder in oPane.TopNode.BrowserNodes)
                {
                    folder.Delete();// delete the folders
                }
                m_inventorApplication.ActiveDocument.Save();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        // passes call to the joints combo box, allows for the windows form to be activated without reselecting in the combo box
        public void EditDrivers_OnExecute(Inventor.NameValueMap Context)
        {
            JointsComboBox_OnSelect(Context);
        }
        // passes call to the limits combo box, allows for the windows form to be activated without reselecting in the combo box
        public void EditLimits_OnExecute(Inventor.NameValueMap Context)
        {
            LimitsComboBox_OnSelect(Context);
        }
        
        private void JointsComboBox_OnSelect(NameValueMap context)
        {
            try
            {
                if (doWerk)
                {
                    if(! (selectedJointData == null)) {
                        try
                        {
                            form.readFromData(selectedJointData);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                        }
                        if (Rotating)
                        {
                            if (JointsComboBox.Text.Equals("Motor"))
                            {
                                selectedJointData.Driver = DriveTypes.Motor;
                                form.MotorChosen();
                                form.readFromData(selectedJointData);
                                form.ShowDialog();
                            }
                            else if (JointsComboBox.Text.Equals("Servo"))
                            {
                                selectedJointData.Driver = DriveTypes.Servo;
                                form.ServoChosen();
                                form.readFromData(selectedJointData);
                                form.ShowDialog();
                            }
                            else if (JointsComboBox.Text.Equals("Bumper Pneumatic"))
                            {
                                selectedJointData.Driver = DriveTypes.BumperPnuematic;
                                form.BumperPneumaticChosen();
                                form.ShowDialog();
                            }
                            else if (JointsComboBox.Text.Equals("Relay Pneumatic"))
                            {
                                selectedJointData.Driver = DriveTypes.RelayPneumatic;
                                form.RelayPneumaticChosen();
                                form.ShowDialog();
                            }
                            else if (JointsComboBox.Text.Equals("Worm Screw"))
                            {
                                selectedJointData.Driver = DriveTypes.WormScrew;
                                form.WormScrewChosen();
                                form.ShowDialog();
                            }
                            else if (JointsComboBox.Text.Equals("Dual Motor"))
                            {
                                selectedJointData.Driver = DriveTypes.DualMotor;
                                form.DualMotorChosen();
                                form.ShowDialog();
                            }
                            else
                            {
                                selectedJointData.Driver = DriveTypes.NoDriver;
                            }
                        }
                        else
                        {
                            if (JointsComboBox.Text.Equals("Elevator"))
                            {
                                selectedJointData.Driver = DriveTypes.Elevator;
                                form.ElevatorChosen();
                                form.ShowDialog();
                            }
                            else if (JointsComboBox.Text.Equals("Bumper Pneumatic"))
                            {
                                selectedJointData.Driver = DriveTypes.BumperPnuematic;
                                form.BumperPneumaticChosen();
                                form.ShowDialog();
                            }
                            else if (JointsComboBox.Text.Equals("Relay Pneumatic"))
                            {
                                selectedJointData.Driver = DriveTypes.RelayPneumatic;
                                form.RelayPneumaticChosen();
                                form.ShowDialog();
                            }
                            else if (JointsComboBox.Text.Equals("Worm Screw"))
                            {
                                selectedJointData.Driver = DriveTypes.WormScrew;
                                form.WormScrewChosen();
                                form.ShowDialog();
                            }
                            else
                            {
                                selectedJointData.Driver = DriveTypes.NoDriver;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a joint to edit");
                    }
                }
            }catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        // reacts to the limits combobox being selected
        public void LimitsComboBox_OnSelect(Inventor.NameValueMap Context)
        {
            if (doWerk)
            {// if the select event shuold be reacted to
                if (!(selectedJointData == null))
                {
                    try
                    {
                        lims.readFromData(selectedJointData);// add the correct data to the form
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }
                    if (LimitsComboBox.Text.Equals("Limits"))// if limits is selected then set the selected joint gets the correct data
                    {
                        selectedJointData.HasLimits = true;
                        lims.ShowDialog();// show windows form
                    }
                    else
                    {
                        selectedJointData.HasLimits = false;
                    }
                }
                else
                {
                    MessageBox.Show("Please select a joint to edit");
                }
            }
        }
        
        /*public void FlattenAssembly() {
            AssemblyDocument sourceAsmDoc;
            sourceAsmDoc = (_AssemblyDocument)m_inventorApplication.ActiveDocument;

            ' Create the new, empty assembly.  
       AssemblyDocument targetAsmDoc;
       targetAsmDoc = m_inventorApplication.Documents.Add(
  kAssemblyDocumentObject, _
  m_inventorApplication.FileManager.GetTemplateFile(_
  kAssemblyDocumentObject))
    

   CopyAssemblyAsFlat(targetAsmDoc.ComponentDefinition, _
   sourceAsmDoc.ComponentDefinition.Occurrences)
    
   }
        private void CopyAssemblyAsFlat(
       AssemblyComponentDefinition TargetAsm,
       ComponentOccurrences Occurrences)
        {
            foreach (ComponentOccurrence occ in Occurrences) {

                if (occ.Visible && !occ.Suppressed && !occ.Excluded) {
                    if (occ.DefinitionDocumentType == DocumentTypeEnum.kPartDocumentObject) {
                        ComponentOccurrence newOcc;
                        newOcc = TargetAsm.Occurrences.AddByComponentDefinition(occ.Definition, occ.Transformation);
                        newOcc.Grounded = true;
                    } else {
                        CopyAssemblyAsFlat(TargetAsm, occ.SubOccurrences);
                    }
                }
            }
        }*/
    }
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