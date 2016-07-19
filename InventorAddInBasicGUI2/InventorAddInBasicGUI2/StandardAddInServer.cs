using System;
using System.Runtime.InteropServices;
using Inventor;
using Microsoft.Win32;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;

namespace InventorAddInBasicGUI2
{
    /// <summary>
    /// This is the primary AddIn Server class that implements the ApplicationAddInServer interface
    /// that all Inventor AddIns are required to implement. The communication between Inventor and
    /// the AddIn is via the methods on this interface.
    /// </summary>
    [GuidAttribute("0c9a07ad-2768-4a62-950a-b5e33b88e4a3")]
    public class StandardAddInServer : Inventor.ApplicationAddInServer
    {

        #region data

        private Inventor.Application m_inventorApplication;

        Document oDoc;

        BrowserPanes oPanes;

        static Boolean doubleClick;
        Boolean FirstTime;
        Boolean inExportView;

        static System.Timers.Timer time;

        Boolean Rotating;
        
        Inventor.ButtonDefinition startExport;
        Inventor.ButtonDefinition exportRobot;
        Inventor.ButtonDefinition selectJoint;
        Inventor.ButtonDefinition cancelExport;
        Inventor.ButtonDefinition selectJointInsideJoint;
        Inventor.ButtonDefinition editDrivers;
        Inventor.ButtonDefinition editLimits;
        Inventor.ButtonDefinition test;

        Inventor.BrowserPane oPane;

        Inventor.ComboBoxDefinitionSink_OnSelectEventHandler JointsComboBox_OnSelectEventDelegate;
        Inventor.ComboBoxDefinitionSink_OnSelectEventHandler LimitsComboBox_OnSelectEventDelegate;
        Inventor.UserInputEventsSink_OnSelectEventHandler click_OnSelectEventDelegate;
        Inventor.UserInputEventsSink_OnUnSelectEventHandler click_OnUnSelectEventDelegate;

        Form1 form;

        bool doWerk;

        EditLimits lims;

        UserControl1 control;

        public string m_ClientId;

        private ComboBoxDefinition JointsComboBox;
        private ComboBoxDefinition LimitsComboBox;

        ArrayList joints;

        JointData selectedJointData;

        Inventor.Ribbon partRibbon;

        Inventor.RibbonTab partTab;
        

        public static String pathToSaveTo;

        Inventor.RibbonPanel partPanel;
        Inventor.RibbonPanel partPanel2;
        Inventor.RibbonPanel partPanel3;

        JointData assemblyJoint;
        BrowserNode node1;
        BrowserNode node2;
        ObjectCollection obj;
        BrowserNode node3;
        String addInCLSIDString;

        ClientNodeResource oRsc;

        int i;
        UserInputEvents UIEvent;

        ArrayList jointList;
        #endregion

        public StandardAddInServer()
        {
        }

        #region ApplicationAddInServer Members

        public void Activate(Inventor.ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            try
            {
                GuidAttribute addInCLSID;
                addInCLSID = (GuidAttribute)GuidAttribute.GetCustomAttribute(typeof(StandardAddInServer), typeof(GuidAttribute));
                addInCLSIDString = "{" + addInCLSID.Value + "}";
                m_ClientId = "0c9a07ad-2768-4a62-950a-b5e33b88e4a3";
                inExportView = false;
                doubleClick = false;
                FirstTime = true;
                control = new UserControl1();
                // This method is called by Inventor when it loads the addin.
                // The AddInSiteObject provides access to the Inventor Application object.
                // The FirstTime flag indicates if the addin is loaded for the first time.

                // Initialize AddIn members.
                m_inventorApplication = addInSiteObject.Application;
                // TODO: Add ApplicationAddInServer.Activate implementation.
                // e.g. event initialization, command creation etc.


                doWerk = false;

                Rotating = true;

                form = new Form1();
                lims = new EditLimits();
                ControlDefinitions controlDefs = m_inventorApplication.CommandManager.ControlDefinitions;
                //m_TreeViewBrowser = controlDefs.AddButtonDefinition("HierarchyPane", "InventorAddInBrowserPaneAttempt5:HierarchyPane", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                //m_TreeViewBrowser.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(m_TreeViewBrowser_OnExecute);

                //doCouThings = controlDefs.AddButtonDefinition("Cou things", "BxD:RobotExporter:CouThings", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                //doCouThings.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(CouThings_OnExecute);

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

                selectJoint = controlDefs.AddButtonDefinition("Select Joint", "BxD:RobotExporter:SelectJoint", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                selectJoint.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(selectJoints_OnExecute);

                test = controlDefs.AddButtonDefinition("test", "BxD:RobotExporter:test", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                test.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(test_OnExecute);

                // Get the assembly ribbon.
                partRibbon = m_inventorApplication.UserInterfaceManager.Ribbons["Assembly"];
                // Get the "Part" tab.
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
                partPanel3.CommandControls.AddButton(selectJoint);
                partPanel3.CommandControls.AddButton(cancelExport);
                partPanel3.CommandControls.AddButton(selectJointInsideJoint);
                partPanel.CommandControls.AddButton(editDrivers);
                partPanel3.CommandControls.AddButton(test);
                LimitsComboBox_OnSelectEventDelegate = new ComboBoxDefinitionSink_OnSelectEventHandler(LimitsComboBox_OnSelect);
                LimitsComboBox.OnSelect += LimitsComboBox_OnSelectEventDelegate;
                partPanel2.CommandControls.AddComboBox(LimitsComboBox);
                partPanel2.CommandControls.AddButton(editLimits);
                //partPanel.CommandControls.AddButton(m_TreeViewBrowser);
                //partPanel.CommandControls.AddButton(doCouThings);

                UIEvent = m_inventorApplication.CommandManager.UserInputEvents;

                // doubleClick_OnSelectEventDelegate = new UserInputEventsSink_OnDoubleClickEventHandler(oUIEvents_OnDoubleClick);
                // UIEvent.OnDoubleClick += doubleClick_OnSelectEventDelegate;

                click_OnSelectEventDelegate = new UserInputEventsSink_OnSelectEventHandler(oUIEvents_OnSelect);
                click_OnUnSelectEventDelegate = new UserInputEventsSink_OnUnSelectEventHandler(oUIEvents_OnUnSelect);
                UIEvent.OnSelect += click_OnSelectEventDelegate;
                UIEvent.OnUnSelect += click_OnUnSelectEventDelegate;

                JointsComboBox.Enabled = false;
                LimitsComboBox.Enabled = false;
                exportRobot.Enabled = false;
                selectJoint.Enabled = false;
                startExport.Enabled = true;
                cancelExport.Enabled = false;
                editDrivers.Enabled = false;
                editLimits.Enabled = false;
                selectJointInsideJoint.Enabled = false;

                /*oDoc = m_inventorApplication.ActiveDocument;
                oPanes = oDoc.BrowserPanes;
                ClientNodeResources oRscs = oPanes.ClientNodeResources;
                stdole.IPictureDisp clientNodeIcon = AxHostConverter.ImageToPictureDisp(new Bitmap(@"C:\Users\t_gracj\Desktop\git\Exporter-Research\InventorAddInBrowserPaneAttempt5\InventorAddInBrowserPaneAttempt5\Resources\test.bmp"));
                ClientNodeResource oRsc = oRscs.Add(m_ClientId, 1, clientNodeIcon);
                BrowserNodeDefinition oDef = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition("Top Node", 3, oRsc);
                Inventor.BrowserPane oPane = oPanes.AddTreeBrowserPane("My Pane", m_ClientId, oDef);
                BrowserNodeDefinition oDef1 = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition("Node2", 5, oRsc);
                BrowserNode oNode1 = oPane.TopNode.AddChild(oDef1);
                BrowserNodeDefinition oDef2 = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition("Node3", 6, oRsc);
                BrowserNode oNode2 = oPane.TopNode.AddChild(oDef2);
                BrowserNode oNativeRootNode;
                oNativeRootNode = oDoc.BrowserPanes["Model"].TopNode;
                oPane.TopNode.AddChild(oNativeRootNode.BrowserNodeDefinition);*/
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
            AssemblyDocument asmDoc = (AssemblyDocument)
                           m_inventorApplication.ActiveDocument;
            ComponentOccurrence assembly = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick
                      (SelectionFilterEnum.kAssemblyOccurrenceFilter, "Select an assembly to open");
            /* if (assembly.SubOccurrences.Count > 0)
             {
                 foreach (ComponentOccurrence c in assembly.SubOccurrences)
                 {
                     if (!(c.Joints.Count > 0))
                     {
                         c.Enabled = false;
                         // c.OverrideOpacity = .15;
                     }
                 }
             } else
             {
                 MessageBox.Show("that is not an assembly");
             }*/
            /*try
            {
                ObjectCollection o = m_inventorApplication.TransientObjects.CreateObjectCollection();
                foreach(JointData j in jointList)
                {
                    o.Add(j.jointOfType);
                }
                //BrowserNode node = oPane.GetBrowserNodeFromObject(assembly);
                oPane.AddBrowserFolder("  xs", o);
            } catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }*/
            hideInside(assembly);
            ComponentOccurrence c = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick
                      (SelectionFilterEnum.kAssemblyLeafOccurrenceFilter, "Select a joint to edit");

            Boolean found;
            NativeBrowserNodeDefinition brow;
            found = false;
                    foreach (BrowserFolder n in oPane.TopNode.BrowserFolders)
                    {
                        foreach (BrowserNode m in n.BrowserNode.BrowserNodes)
                        {
                            brow = (NativeBrowserNodeDefinition)m.BrowserNodeDefinition;
                            if (brow.NativeObject.Equals(c))
                            {
                        MessageBox.Show("fghj");
                                n.BrowserNode.DoSelect();
                                found = true;
                            }
                        }
                    }
                    if (!found)
                    {
                        foreach (BrowserNode n in oPane.TopNode.BrowserNodes)
                        {
                            brow = (NativeBrowserNodeDefinition)n.BrowserNodeDefinition;
                            if (brow.NativeObject.Equals(c))
                            {
                                n.DoSelect();
                            }
                        }
                    }
                }

        public void selectJoints_OnExecute(Inventor.NameValueMap Context)
        {
            try
            {
                AssemblyDocument asmDoc = (AssemblyDocument)
                         m_inventorApplication.ActiveDocument;
                ComponentOccurrence joint = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick
                          (SelectionFilterEnum.kAssemblyOccurrenceFilter, "Select a joint to edit");
                ArrayList joints = new ArrayList();
                foreach (AssemblyJoint j in asmDoc.ComponentDefinition.Joints)
                {
                    joints.Add(j.AffectedOccurrenceOne);
                    joints.Add(j.AffectedOccurrenceOne);
                }
                Boolean found = false;
                foreach (ComponentOccurrence j in joints) {
                    if ((j.Equals(joint))) {
                        found = true;
                    }
                }
                if (!found) {
                    MessageBox.Show("Warning, Not a Joint");
                } else
                {
                    MessageBox.Show("That is a Joint");
                }
                //part2.Visible = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        //starts the exporter
        public void startExport_OnExecute(Inventor.NameValueMap Context)
        {
            try
            {
                
                inExportView = true;
                JointsComboBox.Enabled = true;
                LimitsComboBox.Enabled = true;
                exportRobot.Enabled = true;
                selectJoint.Enabled = true;//set buttons to proper state
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
                oOccurrenceNodes = m_inventorApplication.TransientObjects.CreateObjectCollection();
                /* if (!oPanes.ClientNodeResources.ItemById(m_ClientId, 1).Equals(null))
                 {
                     MessageBox.Show("yeas");
                 } else
                 {
                     MessageBox.Show("noo");
                 }*/
                try
                {// if no browser pane previously created then create a new one
                    ClientNodeResources oRscs = oPanes.ClientNodeResources;
                    stdole.IPictureDisp clientNodeIcon = AxHostConverter.ImageToPictureDisp(new Bitmap(@"C:\Users\t_gracj\Desktop\git\Exporter-Research\InventorAddInBrowserPaneAttempt5\InventorAddInBrowserPaneAttempt5\Resources\test.bmp"));
                    oRsc = oRscs.Add(m_ClientId, 1, clientNodeIcon);
                    oDef = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition("Top Node", 3, oRsc);
                    oPane = oPanes.AddTreeBrowserPane("Select Joints", m_ClientId, oDef);
                    FirstTime = false;
                    oPane.Activate();
                } catch (Exception e)
                {
                    bool found = false;
                    foreach(BrowserPane pane in oPanes)
                    {
                        if(pane.Name.Equals("Select Joints"))
                        {

                            oPane = pane;
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
                /*     foreach (AssemblyJoint j in asmDoc.ComponentDefinition.Joints)
                 {
                     obj = m_inventorApplication.TransientObjects.CreateObjectCollection();
                     node1 = oPane.GetBrowserNodeFromObject(j.AffectedOccurrenceOne);
                     obj.Add(node1);
                     node2 = oPane.GetBrowserNodeFromObject(j.AffectedOccurrenceTwo);
                     obj.Add(node2);
                     node3 = oPane.GetBrowserNodeFromObject(j);
                     obj.Add(node3);
                     oPane.AddBrowserFolder("Joint " + i, obj);
                     //node1 = oPane.GetBrowserNodeFromObject(j.AffectedOccurrenceOne);
                     joints.Add(j.AffectedOccurrenceOne);
                     joints.Add(j.AffectedOccurrenceTwo);
                     i++;
                     assemblyJoint = new JointData(j);
                     jointList.Add(assemblyJoint);

                     //oPane.TopNode.AddChild(node1.BrowserNodeDefinition);
                 }*/
                 // look at all top level parts/ assembly
                foreach (ComponentOccurrence c in ((AssemblyDocument)m_inventorApplication.ActiveDocument).ComponentDefinition.Occurrences)
                {
                    foreach (AssemblyJoint j in c.Joints)
                    {// look at all joints inside of the main doc
                        bool found = false;
                        foreach(JointData d in jointList)
                        {// looks at all joints in the joint data to check for duplicates
                            if (d.equals(j))
                            {
                                found = true;
                            }
                        }
                        if (!found)
                        {// if there isn't a duplicate then add part to browser folder
                            obj = m_inventorApplication.TransientObjects.CreateObjectCollection();
                            node1 = oPane.GetBrowserNodeFromObject(j.AffectedOccurrenceOne);
                            obj.Add(node1);// add the first affected part/ assembly
                            node2 = oPane.GetBrowserNodeFromObject(j.AffectedOccurrenceTwo);
                            obj.Add(node2);// add the second affected part/ assembly
                            node3 = oPane.GetBrowserNodeFromObject(j);
                            obj.Add(node3);// add the joint
                            //ComponentOccurrence assembly = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick(SelectionFilterEnum.kAssemblyLeafOccurrenceFilter, "Select a joint to edit");
                            //BrowserNode node4 = oPane.GetBrowserNodeFromObject(assembly);
                            // obj.Add(node4);
                            oPane.AddBrowserFolder("Joint " + i, obj);
                            //node1 = oPane.GetBrowserNodeFromObject(j.AffectedOccurrenceOne);
                            joints.Add(j.AffectedOccurrenceOne);
                            joints.Add(j.AffectedOccurrenceTwo);
                            i++;
                            assemblyJoint = new JointData(j);
                            jointList.Add(assemblyJoint);// add new joint data to the array
                        }
                    }
                    if (c.SubOccurrences.Count > 0)
                    {// if there are parts/ assemblies inside the assembly then look at it for joints
                        foreach (ComponentOccurrence v in c.SubOccurrences)
                        {
                            findSubOccurences(v);
                        }
                    }
                }
                Boolean contains = false;
                foreach (ComponentOccurrence c in asmDoc.ComponentDefinition.Occurrences)
                {// loks at all parts/ assemblies in the main assembly
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
                        //     c.OverrideOpacity = .15;
                    }
                    if (c.SubOccurrences.Count > 0)
                    {
                        foreach (ComponentOccurrence v in c.SubOccurrences)
                        {
                    //        hideInside(v);
                        }
                      //  c.Enabled = true;
                    }
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        //start export with normal nodes
        /*public void startExport_OnExecute(Inventor.NameValueMap Context)
        {
            try

            {
                inExportView = true;
                JointsComboBox.Enabled = true;
                LimitsComboBox.Enabled = true;
                exportRobot.Enabled = true;
                selectJoint.Enabled = true;
                startExport.Enabled = false;
                cancelExport.Enabled = true;
                selectJointInsideJoint.Enabled = true;
                AssemblyDocument asmDoc = (AssemblyDocument)m_inventorApplication.ActiveDocument;

                BrowserNode node1;
                BrowserNode node2;
                int i = 1;
                if (FirstTime == true)
                {
                    oDoc = m_inventorApplication.ActiveDocument;
                    oPanes = oDoc.BrowserPanes;
                    ObjectCollection oOccurrenceNodes;
                    oOccurrenceNodes = m_inventorApplication.TransientObjects.CreateObjectCollection();
                    ClientNodeResources oRscs = oPanes.ClientNodeResources;
                    stdole.IPictureDisp clientNodeIcon = AxHostConverter.ImageToPictureDisp(new Bitmap(@"C:\Users\t_gracj\Desktop\git\Exporter-Research\InventorAddInBrowserPaneAttempt5\InventorAddInBrowserPaneAttempt5\Resources\test.bmp"));
                    ClientNodeResource oRsc = oRscs.Add(m_ClientId, 1, clientNodeIcon);
                    BrowserNodeDefinition oDef = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition("Top Node", 3, oRsc);
                    oPane = oPanes.AddTreeBrowserPane("Select Joints", m_ClientId, oDef);
                    FirstTime = false;
                    ObjectCollection obj;
                    BrowserNodeDefinition node3;
                    foreach (AssemblyJoint j in asmDoc.ComponentDefinition.Joints)
                    {
                        obj = m_inventorApplication.TransientObjects.CreateObjectCollection();
                         node1 = oPane.GetBrowserNodeFromObject(j.AffectedOccurrenceOne);
                        joints.Add(j.AffectedOccurrenceOne);
                        joints.Add(j.AffectedOccurrenceTwo);
                        i++;
                        
                        oPane.TopNode.AddChild(node1.BrowserNodeDefinition);
                    }

                } else
                {
                    oPane.Visible = true;
                    oPane.Activate();
                }
                    Boolean contains = false;
                    foreach (ComponentOccurrence c in asmDoc.ComponentDefinition.Occurrences)
                    {
                        contains = false;
                        foreach (ComponentOccurrence j in joints)
                        {

                            if ((j.Equals(c)))
                            {
                                contains = true;
                            }
                        }
                        if (!contains)
                        {
                            c.Enabled = false;
                       //     c.OverrideOpacity = .15;
                        }
                    
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }*/

        /*private void oUIEvents_OnDoubleClick(ObjectsEnumerator SelectedEntities, SelectionDeviceEnum SelectionDevice, MouseButtonEnum Button, ShiftStateEnum ShiftKeys, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View, NameValueMap AdditionalInfo, out HandlingCodeEnum HandlingCode)

            {
                if (inExportView)
                {
                    HandlingCode = HandlingCodeEnum.kEventHandled;
                    thing();
                }
                else
                {
                    HandlingCode = HandlingCodeEnum.kEventNotHandled;
                }

           }
           */
        //reacts to double clicks
        public void thing()
        {
            if (doubleClick == false)
            {// if there is noo current double click then start a timer
                time = new System.Timers.Timer();
                time.Interval = 500;

                // Hook up the Elapsed event for the timer. 
                time.Elapsed += OnTimedEvent;

                // Have the timer fire repeated events (treu is the default)
                time.AutoReset = false;

                // Start the timer
                time.Enabled = true;
                doubleClick = true;
            }
            else
            { // if there is a double click active then do this
                MessageBox.Show("jnnnnm");
                time.Enabled = false;
                time.Stop();
                doubleClick = false;
            }
        }
        
        private void oUIEvents_OnSelect(ObjectsEnumerator JustSelectedEntities, ref ObjectCollection MoreSelectedEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
        {
            /* if (doubleClick == false) {
                 time = new System.Timers.Timer();
                 time.Interval = 500;

                 // Hook up the Elapsed event for the timer. 
                 time.Elapsed += OnTimedEvent;

                 // Have the timer fire repeated events (treu is the default)
                 time.AutoReset = false;

                 // Start the timer
                 time.Enabled = true;
                 doubleClick = true;
             } else
             {
                 MessageBox.Show("jnnnnm");
                 time = new System.Timers.Timer();
                 time.Interval = 2000;

                 // Hook up the Elapsed event for the timer. 
                 time.Elapsed += OnTimedEvent;

                 // Have the timer fire repeated events (treu is the default)
                 time.AutoReset = false;

                 // Start the timer
                 time.Enabled = true;
                 doubleClick = true;
             }*/
            Boolean found;
            NativeBrowserNodeDefinition brow;
            if (SelectionDevice == SelectionDeviceEnum.kGraphicsWindowSelection && inExportView) {// && JustSelectedEntities.Count == 1)
                foreach (Object sel in JustSelectedEntities)
                {
                    if (sel is ComponentOccurrence)
                    {
                        found = false;
                        ComponentOccurrence comp = (ComponentOccurrence)sel;
                        foreach (BrowserFolder n in oPane.TopNode.BrowserFolders)
                        {
                            foreach (BrowserNode m in n.BrowserNode.BrowserNodes)
                            {
                                brow = (NativeBrowserNodeDefinition)m.BrowserNodeDefinition;
                                if (brow.NativeObject.Equals(sel))
                                {
                                    //comp.Visible = false;
                                    n.BrowserNode.DoSelect();
                                    found = true;
                                }
                            }
                        }
                        if (!found)
                        {
                            foreach (BrowserNode n in oPane.TopNode.BrowserNodes)
                            {
                                brow = (NativeBrowserNodeDefinition)n.BrowserNodeDefinition;
                                if (brow.NativeObject.Equals(sel))
                                {
                                    n.DoSelect();
                                }
                            }
                        }
                    }
                }
            } else if (SelectionDevice == SelectionDeviceEnum.kBrowserSelection && inExportView)
            {
                foreach (Object sel in JustSelectedEntities)
                {
                    if (sel is BrowserFolder)
                    {
                        foreach (BrowserNode n in ((BrowserFolder)sel).BrowserNode.BrowserNodes)
                        {
                            if (n.NativeObject is AssemblyJoint)
                            {
                                if (((AssemblyJoint)n.NativeObject).Definition.JointType == AssemblyJointTypeEnum.kRotationalJointType)
                                {
                                    JointTypeRotating();
                                }
                                else
                                {
                                    JointTypeLinear();
                                }
                                foreach(JointData j in jointList)
                                {
                                    if (j.equals((AssemblyJoint)n.NativeObject))
                                    {
                                        selectedJointData = j;
                                    }
                                }
                                switchSelectedJoint(selectedJointData.driver);
                                switchSelectedLimit();
                            }
                        }
                    }
                }
            }
        }
        // reacts to a deselect
        private void oUIEvents_OnUnSelect(ObjectsEnumerator UnSelectedEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
        {
            //MessageBox.Show("hi");
            //JointsComboBox.Clear();
        }

        public void switchSelectedLimit()
        {
            doWerk = false;
            if (selectedJointData.HasLimits)
            {
                LimitsComboBox.ListIndex = 2;
            }
            else
            {
                LimitsComboBox.ListIndex = 1;
            }
            doWerk = true;
        }

        private void switchSelectedJoint(DriveTypes driver)
        {
            doWerk = false;
            if (Rotating)
            {
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
            {
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
            doWerk = true;
        }
        // if the joint is rotating then set the proper combo box choices
        private void JointTypeRotating()
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
        // when double click times out then set some variables
        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            time.Enabled = false;

            doubleClick = false;
        }
        //test button for doing experimental things
        public void test_OnExecute(Inventor.NameValueMap Context)
        {
            AssemblyDocument asmDoc = (AssemblyDocument)m_inventorApplication.ActiveDocument;

            jointList = new ArrayList();
            joints = new ArrayList();
            i = 1;
            // inits the browser pane
            if (FirstTime == true)
            {
                oDoc = m_inventorApplication.ActiveDocument;
                oPanes = oDoc.BrowserPanes;
                ObjectCollection oOccurrenceNodes;
                oOccurrenceNodes = m_inventorApplication.TransientObjects.CreateObjectCollection();
                ClientNodeResources oRscs = oPanes.ClientNodeResources;
                stdole.IPictureDisp clientNodeIcon = AxHostConverter.ImageToPictureDisp(new Bitmap(@"C:\Users\t_gracj\Desktop\git\Exporter-Research\InventorAddInBrowserPaneAttempt5\InventorAddInBrowserPaneAttempt5\Resources\test.bmp"));
                ClientNodeResource oRsc = oRscs.Add(m_ClientId, 1, clientNodeIcon);
                BrowserNodeDefinition oDef = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition("Top Node", 3, oRsc);
                oPane = oPanes.AddTreeBrowserPane("Select Joints", m_ClientId, oDef);
                FirstTime = false;
            }
            // gets a selected leaf node then adds the corrospoinding browser node to a folder
            ComponentOccurrence assembly = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick
                      (SelectionFilterEnum.kAssemblyLeafOccurrenceFilter, "Select a joint to edit");
            oPane.GetBrowserNodeFromObject(assembly);
            oPane.AddBrowserFolder("  xs" );
        }
        
        public void hideInside(ComponentOccurrence c)
        {
            bool contains = false;
            foreach (ComponentOccurrence j in joints)
            {

                if ((j.Equals(c)))
                {
                    contains = true;
                }
            }
            if (!contains)
            {
                c.Enabled = false;
                //     c.OverrideOpacity = .15;
            }
            if (c.SubOccurrences.Count > 0)
            {
                foreach (ComponentOccurrence v in c.SubOccurrences)
                {
                    hideInside(v);
                }
            }
        }

        public void findSubOccurences(ComponentOccurrence occ)
        {
            try
            {
                foreach (AssemblyJoint j in occ.Joints)
                {
                    bool found = false;
                    foreach (JointData d in jointList)
                    {
                        if (d.equals(j))
                        {
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        obj = m_inventorApplication.TransientObjects.CreateObjectCollection();
                        node1 = oPane.GetBrowserNodeFromObject(j.AffectedOccurrenceOne);
                        obj.Add(node1);
                        node2 = oPane.GetBrowserNodeFromObject(j.AffectedOccurrenceTwo);
                        obj.Add(node2);
                        node3 = oPane.GetBrowserNodeFromObject(j);
                        obj.Add(node3);
                        //ComponentOccurrence assembly = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick(SelectionFilterEnum.kAssemblyLeafOccurrenceFilter, "Select a joint to edit");
                        //BrowserNode node4 = oPane.GetBrowserNodeFromObject(assembly);
                        // obj.Add(node4);
                        oPane.AddBrowserFolder("Joint " + i, obj);
                        //node1 = oPane.GetBrowserNodeFromObject(j.AffectedOccurrenceOne);
                        joints.Add(j.AffectedOccurrenceOne);
                        joints.Add(j.AffectedOccurrenceTwo);
                        i++;
                        assemblyJoint = new JointData(j);
                        jointList.Add(assemblyJoint);
                    }
                }
                if (occ.SubOccurrences.Count > 0)
                {
                    foreach (ComponentOccurrence v in occ.SubOccurrences)
                    {
                        findSubOccurences(v);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void exportRobot_OnExecute(Inventor.NameValueMap Context)
        {
            control.saveFile();
            inExportView = false;
            switchSelectedJoint(DriveTypes.NoDriver);
            //HighlightSet set;
            //set = m_inventorApplication.ActiveDocument.CreateHighlightSet();
            //set.AddItem(m_inventorApplication.TransientObjects.CreateColor(255, 0, 0, .8));
            AssemblyDocument asmDoc = (AssemblyDocument) m_inventorApplication.ActiveDocument;
            foreach (ComponentOccurrence c in asmDoc.ComponentDefinition.Occurrences)
            {
                c.Enabled = true;
            //    c.OverrideOpacity = 1;
            }
            JointsComboBox.Enabled = false;
            LimitsComboBox.Enabled = false;
            exportRobot.Enabled = false;
            selectJoint.Enabled = false;
            startExport.Enabled = true;
            cancelExport.Enabled = false;
            editDrivers.Enabled = false;
            editLimits.Enabled = false;
            selectJointInsideJoint.Enabled = false;
            oPane.Visible = false;
            jointList = new ArrayList();
            foreach(BrowserNode folder in oPane.TopNode.BrowserNodes)
            {
                folder.Delete();
            }
        }
        
        public void cancelExport_OnExecute(Inventor.NameValueMap Context)
        {
            switchSelectedJoint(DriveTypes.NoDriver);
            inExportView = false;
            //HighlightSet set;
            //set = m_inventorApplication.ActiveDocument.CreateHighlightSet();
            //set.AddItem(m_inventorApplication.TransientObjects.CreateColor(255, 0, 0, .8));
            AssemblyDocument asmDoc = (AssemblyDocument)m_inventorApplication.ActiveDocument;
            foreach (ComponentOccurrence c in asmDoc.ComponentDefinition.Occurrences)
            {
                c.Enabled = true;
                //c.OverrideOpacity = 1;
            }
            JointsComboBox.Enabled = false;
            LimitsComboBox.Enabled = false;
            exportRobot.Enabled = false;
            selectJoint.Enabled = false;
            startExport.Enabled = true;
            cancelExport.Enabled = false;
            editDrivers.Enabled = false;
            selectJointInsideJoint.Enabled = false;
            jointList = new ArrayList();
            oPane.Visible = false;
            foreach (BrowserNode folder in oPane.TopNode.BrowserNodes)
            {
                folder.Delete();
            }
            try
            {
                //oPanes.ClientNodeResources.ItemById(m_ClientId, 1).Delete();
               // oPane.Delete();
               // oPanes.GetClientBrowserNodeDefinition("Top Node", 3).Delete();
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
        // another test button
        public void CouThings_OnExecute(Inventor.NameValueMap Context)
        {
            try
            {
                //set = m_inventorApplication.ActiveDocument.CreateHighlightSet();
                //set.AddItem(m_inventorApplication.TransientObjects.CreateColor(255, 0, 0, .8));
                AssemblyDocument asmDoc = (AssemblyDocument)
                         m_inventorApplication.ActiveDocument;

                // Have two parts selected in the assembly.
                /*ComponentOccurrence part1 = (ComponentOccurrence)
                           m_inventorApplication.CommandManager.Pick
              (SelectionFilterEnum.kAssemblyLeafOccurrenceFilter,
                                                "Select part 1");

                ComponentOccurrence part2 = (ComponentOccurrence)
                    m_inventorApplication.CommandManager.Pick
              (SelectionFilterEnum.kAssemblyLeafOccurrenceFilter,
                                                "Select part 2");
                MessageBox.Show(part1.Joints.ToString());*/
                // picks an assembly
                ComponentOccurrence joint = (ComponentOccurrence)
                           m_inventorApplication.CommandManager.Pick
              (SelectionFilterEnum.kAssemblyLeafOccurrenceFilter,
                                                "Select a joint to edit");
                ArrayList joints = new ArrayList();
                // adds joint to jonit list
                foreach (AssemblyJoint j in asmDoc.ComponentDefinition.Joints)
                {
                    joints.Add(j.AffectedOccurrenceOne);
                    joints.Add(j.AffectedOccurrenceOne);
                }
                Boolean contains = false;
                // looks at all parts/ assemblies in the main assembly
                foreach (ComponentOccurrence c in asmDoc.ComponentDefinition.Occurrences)
                {
                    contains = false;
                    foreach (ComponentOccurrence j in joints)
                    {
                        // looks for part/ assembly in joints
                        if ((j.Equals(c)))
                        {
                            contains = true;
                        }
                    }
                    // if not in the joints array then sets to invisible
                    if (!contains)
                    {
                        c.Transparent = true;
                    }
                }
                //part2.Visible = false;
            } catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
     
        private void JointsComboBox_OnSelect(NameValueMap context)
        {
            if (doWerk)
            {
                try
                {
                    form.readFromData(selectedJointData);
                } catch(Exception e) { 
                    MessageBox.Show(e.ToString());
                }
                if (Rotating)
                {
                    if (JointsComboBox.Text.Equals("Motor"))
                    {
                        selectedJointData.driver = DriveTypes.Motor;
                        form.MotorChosen();
                        form.readFromData(selectedJointData);
                        form.ShowDialog();
                    } else if (JointsComboBox.Text.Equals("Servo"))
                    {
                        selectedJointData.driver = DriveTypes.Servo;
                        form.ServoChosen();
                        form.readFromData(selectedJointData);
                        form.ShowDialog();
                    } else if (JointsComboBox.Text.Equals("Bumper Pneumatic"))
                    {
                        selectedJointData.driver = DriveTypes.BumperPnuematic;
                        form.BumperPneumaticChosen();
                        form.ShowDialog();
                    } else if (JointsComboBox.Text.Equals("Relay Pneumatic"))
                    {
                        selectedJointData.driver = DriveTypes.RelayPneumatic;
                        form.RelayPneumaticChosen();
                        form.ShowDialog();
                    } else if (JointsComboBox.Text.Equals("Worm Screw"))
                    {
                        selectedJointData.driver = DriveTypes.WormScrew;
                        form.WormScrewChosen();
                        form.ShowDialog();
                    } else if (JointsComboBox.Text.Equals("Dual Motor"))
                    {
                        selectedJointData.driver = DriveTypes.DualMotor;
                        form.DualMotorChosen();
                        form.ShowDialog();
                    } else
                    {
                        selectedJointData.driver = DriveTypes.NoDriver;
                    }
                }
                else
                {
                    if (JointsComboBox.Text.Equals("Elevator"))
                    {
                        selectedJointData.driver = DriveTypes.Elevator;
                        form.ElevatorChosen();
                        form.ShowDialog();
                    } else if (JointsComboBox.Text.Equals("Bumper Pneumatic"))
                    {
                        selectedJointData.driver = DriveTypes.BumperPnuematic;
                        form.BumperPneumaticChosen();
                        form.ShowDialog();
                    } else if (JointsComboBox.Text.Equals("Relay Pneumatic"))
                    {
                        selectedJointData.driver = DriveTypes.RelayPneumatic;
                        form.RelayPneumaticChosen();
                        form.ShowDialog();
                    } else if (JointsComboBox.Text.Equals("Worm Screw"))
                    {
                        selectedJointData.driver = DriveTypes.WormScrew;
                        form.WormScrewChosen();
                        form.ShowDialog();
                    } else
                    {
                        selectedJointData.driver = DriveTypes.NoDriver;
                    }
                }
            }
        }
        // reacts to the limits combobox being selected
        public void LimitsComboBox_OnSelect(Inventor.NameValueMap Context)
        {
            if (doWerk)
            {// if the select event shuold be reacted to
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
                } else
                {
                    selectedJointData.HasLimits = false;
                }
            }
        }
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