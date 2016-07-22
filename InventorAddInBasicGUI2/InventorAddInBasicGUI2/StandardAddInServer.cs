using System;
using System.Runtime.InteropServices;
using Inventor;
using Microsoft.Win32;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;
using System.Xml;
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
         
       public static Inventor.Application m_inventorApplication;

        Document oDoc;

        BrowserPanes oPanes;

        static Boolean doubleClick;
        Boolean FirstTime;
        Boolean inExportView;

        int jointNumber;

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

                jointNumber = 0;

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
                //test.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(CouThings_OnExecute);
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
            HideInside(assembly);
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
                            assemblyJoint = new JointData(j, "Joint" + jointNumber);
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
        // old way of doing double clicks, over ridden by on select method
        /*private void oUIEvents_OnDoubleClick(ObjectsEnumerator SelectedEntities, SelectionDeviceEnum SelectionDevice, MouseButtonEnum Button, ShiftStateEnum ShiftKeys, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View, NameValueMap AdditionalInfo, out HandlingCodeEnum HandlingCode)

            {
                if (inExportView)
                {// if in export view then handle event
                    HandlingCode = HandlingCodeEnum.kEventHandled;
                    DoTimerStuff();
                }
                else
                {// if not in export view then let inventor deal with it
                    HandlingCode = HandlingCodeEnum.kEventNotHandled;
                }

           }
           */
        //reacts to double clicks
        public void DoTimerStuff()
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
        //reacts to a select event
        private void oUIEvents_OnSelect(ObjectsEnumerator JustSelectedEntities, ref ObjectCollection MoreSelectedEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
        {
            // checks for a double click
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
                // only reacts if the select comes from graphics window and we are exporting
                foreach (Object sel in JustSelectedEntities)
                {//looks at all things selected
                    if (sel is ComponentOccurrence)
                    {// react only if sel is a part/ assembly
                        found = false;
                        ComponentOccurrence comp = (ComponentOccurrence)sel;// converts sel to a component occurences, component occurences are parts/ assemblies
                        foreach (BrowserFolder n in oPane.TopNode.BrowserFolders)
                        {// looks at all browser folders in the top node
                            foreach (BrowserNode m in n.BrowserNode.BrowserNodes)
                            {// looks at all nodes in the browser folder
                                brow = (NativeBrowserNodeDefinition)m.BrowserNodeDefinition;// converts to native node definition so we can do fun stuff
                                if (brow.NativeObject.Equals(sel))// if the native object, the associated component occurences is the same as the selected one
                                {
                                    //comp.Visible = false;
                                    n.BrowserNode.DoSelect();// selects the node and tells the code it found what it is looking for
                                    found = true;
                                }
                            }
                        }
                        if (!found)
                        {// if the code didn't find sel inside of a browser folder then look at nodes
                            foreach (BrowserNode n in oPane.TopNode.BrowserNodes)
                            {
                                brow = (NativeBrowserNodeDefinition)n.BrowserNodeDefinition;// converts to native node definition so we can do fun stuff
                                if (brow.NativeObject.Equals(sel))// if the native object, the associated component occurences is the same as the selected one
                                {
                                    n.DoSelect();// if found inside of the nodes then select
                                }
                            }
                        }
                    }
                }
            } else if (SelectionDevice == SelectionDeviceEnum.kBrowserSelection && inExportView)
            {// if in export mode and select comes from the browser, cool feature, this is called by BrowserNode.DoSelect() so I had all reacts go through here
                foreach (Object sel in JustSelectedEntities)
                {// looks at all selected thinds
                    if (sel is BrowserFolder)
                    {// is sel is a browser folder we can do stuff
                        foreach (BrowserNode n in ((BrowserFolder)sel).BrowserNode.BrowserNodes)
                        {// looks at all nodes inside of the browserfolder 
                            if (n.NativeObject is AssemblyJoint)
                            { // if the pointed to object is an assembly joints the move on
                                foreach (JointData j in jointList)
                                {
                                    if (j.equals((AssemblyJoint)n.NativeObject))// if the jointdata is the same as the selected object
                                    {
                                        selectedJointData = j;// set the selected joint to the correct joint data
                                    }
                                }
                                if (((AssemblyJoint)n.NativeObject).Definition.JointType == AssemblyJointTypeEnum.kCylindricalJointType ||
                                    ((AssemblyJoint)n.NativeObject).Definition.JointType == AssemblyJointTypeEnum.kSlideJointType)
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
        // reacts to a deselect
        private void oUIEvents_OnUnSelect(ObjectsEnumerator UnSelectedEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
        {
            //MessageBox.Show("hi");
            //JointsComboBox.Clear();
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
        
        private void readSave()
        {
            try
            {
                int NumJoints = 0;
                PropertySets sets = m_inventorApplication.ActiveDocument.PropertySets;
                PropertySet set = null;
                object resultObj = null;
                object context = null;
                byte[] refObj;
                foreach (PropertySet p in sets)
                {
                    if (p.DisplayName.Equals("Number of Joints"))
                    {
                        NumJoints = (int) p.ItemByPropId[2].Value; ;
                    }
                }
                for (int i = 0; i < NumJoints; i++)
                {
                    foreach (PropertySet p in sets)
                    {
                        if (p.DisplayName.Equals("Joint" + i))
                        {
                            resultObj = null;
                            context = null;
                            refObj = new byte[0];
                            m_inventorApplication.ActiveDocument.ReferenceKeyManager.StringToKey(((String)p.ItemByPropId[2].Value), ref refObj);
                            //m_inventorApplication.ActiveDocument.ReferenceKeyManager.StringToKey(((JointData)jointList[1]).RefKey , refObj);
                            if (m_inventorApplication.ActiveDocument.ReferenceKeyManager.
                                    CanBindKeyToObject(refObj, 0, out resultObj, out context))
                            {
                                MessageBox.Show("yeas");
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
            }
        }
        //test button for doing experimental things
        public void test_OnExecute(Inventor.NameValueMap Context)
        {
            //writeSave(((JointData)jointList[0]));
            foreach(JointData j in jointList)
            {
                writeSave(j);
            }
            writeNumJoints();
            readSave();
        }
            /*AssemblyDocument asmDoc = (AssemblyDocument)m_inventorApplication.ActiveDocument;

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
            oPane.AddBrowserFolder("  xs" );*/
            /*try
            {
                System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(JointData));
                System.IO.FileStream file = System.IO.File.Create("jksdfjksdfjklsdfjklsdfjklsdfjklsdfk.xml");

                writer.Serialize(file, selectedJointData);
                file.Close();
            }catch(Exception x)
            {
                MessageBox.Show(x.ToString());
            }*/
            /*XmlTextWriter writer = new XmlTextWriter("prodfadsfsfasdasdauct.xml", System.Text.Encoding.UTF8);
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 2;
            writer.WriteStartElement("Table");
            createNode("1", "Product 1", "1000", writer);
            createNode("2", "Product 2", "2000", writer);
            createNode("3", "Product 3", "3000", writer);
            createNode("4", "Product 4", "4000", writer);
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            MessageBox.Show("XML File created ! ");
        }

        private void createNode(string pID, string pName, string pPrice, XmlTextWriter writer)
        {
            writer.WriteStartElement("Product");
            writer.WriteStartElement("Product_id");
            writer.WriteString(pID);
            writer.WriteEndElement();
            writer.WriteStartElement("Product_name");
            writer.WriteString(pName);
            writer.WriteEndElement();
            writer.WriteStartElement("Product_price");
            writer.WriteString(pPrice);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }*/
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
                //     c.OverrideOpacity = .15;
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
            try
            {
                foreach (AssemblyJoint j in occ.Joints)
                {
                    bool found = false;
                    foreach (JointData d in jointList)
                    {
                        if (d.equals(j))
                        {
                            found = true;// if there are duplicates then don't add a browser folder
                        }
                    }
                    if (!found)
                    {
                        obj = m_inventorApplication.TransientObjects.CreateObjectCollection();
                        node1 = oPane.GetBrowserNodeFromObject(j.AffectedOccurrenceOne);// add first part of joints
                        obj.Add(node1);
                        node2 = oPane.GetBrowserNodeFromObject(j.AffectedOccurrenceTwo);// add second part of joint
                        obj.Add(node2);
                        node3 = oPane.GetBrowserNodeFromObject(j);// add joint
                        obj.Add(node3);
                        //ComponentOccurrence assembly = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick(SelectionFilterEnum.kAssemblyLeafOccurrenceFilter, "Select a joint to edit");
                        //BrowserNode node4 = oPane.GetBrowserNodeFromObject(assembly);
                        // obj.Add(node4);
                        oPane.AddBrowserFolder("Joint " + i, obj); // add browser folder to browser pane
                        //node1 = oPane.GetBrowserNodeFromObject(j.AffectedOccurrenceOne);
                        joints.Add(j.AffectedOccurrenceOne);
                        joints.Add(j.AffectedOccurrenceTwo);// add affected parts to array
                        i++;
                        assemblyJoint = new JointData(j, "Joint" + jointNumber);
                        jointList.Add(assemblyJoint); // add jointdata from joint to array
                    }
                }
                if (occ.SubOccurrences.Count > 0)
                {// if the assembly has subcomponent then look for joints
                    foreach (ComponentOccurrence v in occ.SubOccurrences)
                    {
                        FindSubOccurences(v);// go into the subassembly and look for joints
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
            //HighlightSet set;
            //set = m_inventorApplication.ActiveDocument.CreateHighlightSet();
            //set.AddItem(m_inventorApplication.TransientObjects.CreateColor(255, 0, 0, .8));
            AssemblyDocument asmDoc = (AssemblyDocument) m_inventorApplication.ActiveDocument;
            foreach (ComponentOccurrence c in asmDoc.ComponentDefinition.Occurrences)
            {
                c.Enabled = true;// enable all parts/ assemblies in the doc
            //    c.OverrideOpacity = 1;
            }
            JointsComboBox.Enabled = false;
            LimitsComboBox.Enabled = false;
            exportRobot.Enabled = false;
            selectJoint.Enabled = false;
            startExport.Enabled = true;// change the button states to correct ones
            cancelExport.Enabled = false;
            editDrivers.Enabled = false;
            editLimits.Enabled = false;
            selectJointInsideJoint.Enabled = false;
            oPane.Visible = false; // hide browser pane
            jointList = new ArrayList(); // clear the jointList
            foreach(BrowserNode folder in oPane.TopNode.BrowserNodes)
            {
                folder.Delete();// delete the folders
            }
        }
        // cancels the export
        public void cancelExport_OnExecute(Inventor.NameValueMap Context)
        {
            SwitchSelectedJoint(DriveTypes.NoDriver);// change combo box selections to default
            SwitchSelectedLimit(false);
            inExportView = false;// exit export view
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
            selectJoint.Enabled = false;// change buttons the proper state
            startExport.Enabled = true;
            cancelExport.Enabled = false;
            editDrivers.Enabled = false;
            selectJointInsideJoint.Enabled = false;
            jointList = new ArrayList();// clear jointList
            oPane.Visible = false;// Hide the browser pane
            foreach (BrowserNode folder in oPane.TopNode.BrowserNodes)
            {
                folder.Delete();// delete the folders
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
        public Object z;
        public Object v;
        /*public void CouThings_OnExecute(Inventor.NameValueMap Context)
        {
            try
            {
                /*AssemblyDocument asmDoc = (AssemblyDocument)m_inventorApplication.ActiveDocument;
                AssemblyJoints joints = asmDoc.ComponentDefinition.Joints;
                ReferenceKeyManager refKeyMgr = asmDoc.ReferenceKeyManager;
                foreach (AssemblyJoint joint in joints)
                {
                    byte[] refKey = new byte[0];
                    ComponentOccurrence occ = joint.AffectedOccurrenceOne;
                    occ.GetReferenceKey(ref refKey, 0);
                    object resultObj = null;
                    object context = null;
                    object other = null;
                    if (refKeyMgr.CanBindKeyToObject(refKey, 0, out resultObj, out context))
                    {
                        object obj = refKeyMgr.BindKeyToObject(refKey, 0, out other);
                        ((ComponentOccurrence)obj).Visible = false;
                    }
                }*/ 
                /*
                ReferenceKeyManager refKeyMgr = m_inventorApplication.ActiveDocument.ReferenceKeyManager;
                foreach (JointData joint in jointList)
                {
                    object resultObj = null;
                    object context = null;
                    object other = null;
                    if (refKeyMgr.CanBindKeyToObject(joint.refKey, 0, out resultObj, out context))
                    {
                        object obj = refKeyMgr.BindKeyToObject(joint.refKey, 0, out other);
                        foreach (AssemblyJoint jodint in ((AssemblyDocument)m_inventorApplication.ActiveDocument).ComponentDefinition.Joints)
                        {
                            byte[] refKey = new byte[0];
                            jodint.GetReferenceKey(ref refKey, 0);
                            object resucltObj = null;
                            object contdext = null;
                            object othder = null;
                            if (refKeyMgr.CanBindKeyToObject(refKey, 0, out resucltObj, out contdext))
                            {
                                object obcj = refKeyMgr.BindKeyToObject(refKey, 0, out othder);
                                if (((AssemblyJoint)obcj).Equals(((AssemblyJoint)obj)))
                                {
                                    MessageBox.Show("found");
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
        } */
            /*try
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
                /*
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
            }*/
     
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
                        selectedJointData.Driver = DriveTypes.Motor;
                        form.MotorChosen();
                        form.readFromData(selectedJointData);
                        form.ShowDialog();
                    } else if (JointsComboBox.Text.Equals("Servo"))
                    {
                        selectedJointData.Driver = DriveTypes.Servo;
                        form.ServoChosen();
                        form.readFromData(selectedJointData);
                        form.ShowDialog();
                    } else if (JointsComboBox.Text.Equals("Bumper Pneumatic"))
                    {
                        selectedJointData.Driver = DriveTypes.BumperPnuematic;
                        form.BumperPneumaticChosen();
                        form.ShowDialog();
                    } else if (JointsComboBox.Text.Equals("Relay Pneumatic"))
                    {
                        selectedJointData.Driver = DriveTypes.RelayPneumatic;
                        form.RelayPneumaticChosen();
                        form.ShowDialog();
                    } else if (JointsComboBox.Text.Equals("Worm Screw"))
                    {
                        selectedJointData.Driver = DriveTypes.WormScrew;
                        form.WormScrewChosen();
                        form.ShowDialog();
                    } else if (JointsComboBox.Text.Equals("Dual Motor"))
                    {
                        selectedJointData.Driver = DriveTypes.DualMotor;
                        form.DualMotorChosen();
                        form.ShowDialog();
                    } else
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
                    } else if (JointsComboBox.Text.Equals("Bumper Pneumatic"))
                    {
                        selectedJointData.Driver = DriveTypes.BumperPnuematic;
                        form.BumperPneumaticChosen();
                        form.ShowDialog();
                    } else if (JointsComboBox.Text.Equals("Relay Pneumatic"))
                    {
                        selectedJointData.Driver = DriveTypes.RelayPneumatic;
                        form.RelayPneumaticChosen();
                        form.ShowDialog();
                    } else if (JointsComboBox.Text.Equals("Worm Screw"))
                    {
                        selectedJointData.Driver = DriveTypes.WormScrew;
                        form.WormScrewChosen();
                        form.ShowDialog();
                    } else
                    {
                        selectedJointData.Driver = DriveTypes.NoDriver;
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