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
    [GuidAttribute("e50be244-9f7b-4b94-8f87-8224faba8ca1")]
    public class StandardAddInServer : Inventor.ApplicationAddInServer
    {

        // Inventor application object.
        public static Inventor.Application m_inventorApplication;
        private static Ribbon partRibbon;
        private static RibbonTab partTab;
        ClientNodeResource oRsc;
        static RibbonPanel partPanel;
        static ButtonDefinition accessInnerAssemblies;
        static ButtonDefinition beginExporter;
        static ButtonDefinition addNewType;
        static ButtonDefinition editType;
        static ButtonDefinition addNewItem;
        static ButtonDefinition cancleExport;
        static ButtonDefinition exportField;
        static ArrayList FieldTypes;
        public static FieldDataType selectedType;
        MouseEventsSink_OnMouseClickEventHandler click_EventDelegate;
        static BrowserPanes oPanes;
        object resultObj;
        object other;
        object context;
        public Object z;
        public Object v;
        static Inventor.BrowserPane oPane;
        static Document oDoc;
        static HighlightSet oSet;
        UserInputEvents UIEvent;
        BrowserNode node1;
        Inventor.UserInputEventsSink_OnSelectEventHandler click_OnSelectEventDelegate;
        Inventor.UserInputEventsSink_OnUnSelectEventHandler click_OnUnSelectEventDelegate;
        static bool inExportView;
        InteractionEvents oInteractEvents;
        ObjectCollection obj;
        static ComponentPropertiesForm form;
        String name;
        static String m_ClientId;
        public StandardAddInServer()
        {
        }

        #region ApplicationAddInServer Members

        public void Activate(Inventor.ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            try
            {
                m_ClientId = "  ";
                // This method is called by Inventor when it loads the addin.
                // The AddInSiteObject provides access to the Inventor Application object.
                // The FirstTime flag indicates if the addin is loaded for the first time.

                // Initialize AddIn members.
                inExportView = false;
                m_inventorApplication = addInSiteObject.Application;
                // Get the assembly ribbon.
                partRibbon = m_inventorApplication.UserInterfaceManager.Ribbons["Assembly"];
                // Get the "Part" tab.
                partTab = partRibbon.RibbonTabs.Add("Field Exporter", "BxD:FieldExporter", "{e50be244-9f7b-4b94-8f87-8224faba8ca1}");
                partPanel = partTab.RibbonPanels.Add("Exporter Control", "BxD:FieldExporter:ExporterControl", "{e50be244-9f7b-4b94-8f87-8224faba8ca1}");
                // TODO: Add ApplicationAddInServer.Activate implementation.
                // e.g. event initialization, command creation etc.
                ControlDefinitions controlDefs = m_inventorApplication.CommandManager.ControlDefinitions;
                FieldTypes = new ArrayList();
                beginExporter = controlDefs.AddButtonDefinition("Start Exporter", "InventorAddInBrowserPaneAttempt5:StartExporter", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                beginExporter.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(startExport_OnExecute);
                addNewType = controlDefs.AddButtonDefinition("Add new type", "InventorAddInBrowserPaneAttempt5:AddNewType", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                addNewType.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(addNewType_OnExecute);
                addNewItem = controlDefs.AddButtonDefinition("Add new assembly", "InventorAddInBrowserPaneAttempt5:AddNewItem", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                addNewItem.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(addNewItem_OnExecute);
                editType = controlDefs.AddButtonDefinition("Edit type properties", "InventorAddInBrowserPaneAttempt5:EditTypeProperties", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                editType.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(editTypeProperites_OnExecute);
                cancleExport = controlDefs.AddButtonDefinition("Cancle export", "InventorAddInBrowserPaneAttempt5:cancleExport", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                cancleExport.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(cancleExporter_OnExecute);
                exportField = controlDefs.AddButtonDefinition("Export field", "InventorAddInBrowserPaneAttempt5:exportField", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                exportField.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(exportField_OnExecute);
                accessInnerAssemblies = controlDefs.AddButtonDefinition("Add new part in subassembly", "InventorAddInBrowserPaneAttempt5:AccessSubassembly", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                accessInnerAssemblies.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(test);

                form = new ComponentPropertiesForm();

                partPanel.CommandControls.AddButton(beginExporter);
                partPanel.CommandControls.AddButton(addNewType);
                partPanel.CommandControls.AddButton(addNewItem);
                partPanel.CommandControls.AddButton(accessInnerAssemblies);
                partPanel.CommandControls.AddButton(editType);
                partPanel.CommandControls.AddButton(cancleExport);
                partPanel.CommandControls.AddButton(exportField);
                addNewType.Enabled = false;
                editType.Enabled = false;
                addNewItem.Enabled = false;
                beginExporter.Enabled = true;
                cancleExport.Enabled = false;
                exportField.Enabled = false;
                addNewType.Enabled = false;
                accessInnerAssemblies.Enabled = false;
                UIEvent = m_inventorApplication.CommandManager.UserInputEvents;
                click_OnSelectEventDelegate = new UserInputEventsSink_OnSelectEventHandler(oUIEvents_OnSelect);
                UIEvent.OnSelect += click_OnSelectEventDelegate;
            } catch (Exception e)
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
            m_inventorApplication = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
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

        private void oUIEvents_OnSelect(ObjectsEnumerator JustSelectedEntities, ref ObjectCollection MoreSelectedEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
        {
            oSet.Clear();
            Boolean found;
            BrowserNodeDefinition brow;
            if (SelectionDevice == SelectionDeviceEnum.kGraphicsWindowSelection && inExportView)
            {
                foreach (Object sel in JustSelectedEntities)
                {
                    if (sel is ComponentOccurrence) { 
                        ComponentOccurrence comp = (ComponentOccurrence)sel;
                        foreach (BrowserNode n in oPane.TopNode.BrowserNodes)
                        {
                            brow = n.BrowserNodeDefinition;
                            //if (brow.NativeObject.Equals(sel))
                            //{
                            //    n.DoSelect();
                            //}
                        }
                    }
                }
            }
            else if (SelectionDevice == SelectionDeviceEnum.kBrowserSelection && inExportView)
            {
                foreach (Object sel in JustSelectedEntities)
                {
                    if (sel is BrowserNodeDefinition)
                    {
                        foreach (FieldDataType f in FieldTypes)
                        {
                            if (f.same(((BrowserNodeDefinition)sel)))
                            {
                                selectedType = f;
                                foreach (ComponentOccurrence o in selectedType.compOcc)
                                {
                                    oSet.AddItem(o);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TimerWatch()
        {
            try
            {
                System.Timers.Timer aTimer = new System.Timers.Timer();
                aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                aTimer.Interval = 500;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        static FieldDataType currentSelected;
        static bool found;
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            found = false;
            foreach (BrowserNode node in oPane.TopNode.BrowserNodes)
            {
                if (node.Selected)
                {
                    foreach (FieldDataType t in FieldTypes)
                    {
                        if (t.same(node.BrowserNodeDefinition))
                        {
                            if(!currentSelected.Equals(t))
                            {
                                found = true;
                                oSet.Clear();
                                foreach (ComponentOccurrence io in t.compOcc)
                                {
                                    oSet.AddItem(io);
                                }
                                currentSelected = t;
                            }
                        }
                    }
                }
            }
            if (!found)
            {
                oSet.Clear();
            }
        }
        public static BrowserNodeDefinition addType(String name)
        {
            BrowserNodeDefinition def = null;
            try
            {
                Random rand = new Random();
                double thi = rand.NextDouble();
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
                def = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition(name, th, oRes);
                oPane.TopNode.AddChild(def);
                FieldTypes.Add(new FieldDataType(def));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            return def;
        }
        
        public void accessSubassemblies_OnExecute(Inventor.NameValueMap Context)
        {
            try
            {
               // ArrayList l = new ArrayList();
            obj = m_inventorApplication.TransientObjects.CreateObjectCollection();
            AssemblyDocument asmDoc = (AssemblyDocument)
                         m_inventorApplication.ActiveDocument;
            ComponentOccurrence joint = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick
                      (SelectionFilterEnum.kAssemblyLeafOccurrenceFilter, "Select a part to add");
            foreach (BrowserFolder folder in oPane.TopNode.BrowserFolders)
            {
                name = folder.Name;
                if (folder.BrowserNode.Selected)
                {
                    foreach (BrowserNode m in folder.BrowserNode.BrowserNodes)
                    {
                        obj.Add(m);
                        //l.Add(m);
                    }
                    
                        obj.Add(oPane.GetBrowserNodeFromObject(joint));
                        //l.Add(oPane.GetBrowserNodeFromObject(joint));
                    
                    BrowserFolder selectedFolder = oPane.AddBrowserFolder(name, obj);
                    //BrowserFolder selectedFolder = oPane.AddBrowserFolder(name, l);
                  //  selectedType = (FieldDataType)FieldTypes[(FieldTypes.Add(new FieldDataType(selectedFolder)))];
                    foreach (FieldDataType t in FieldTypes)
                    {
                      //  if (t.same(folder))
                      //  {
                            t.copyToNewType(selectedType);
                      //  }
                    }
                    foreach (BrowserNode n in folder.BrowserNode.BrowserNodes)
                    {
                        n.Delete();
                    }
                    folder.Delete();
                    foreach (FieldDataType t in FieldTypes)
                    {
                        //if (t.same(folder))
                        //{
                            FieldTypes.Remove(t);
                        //}
                    }
                }
            }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

        }

        public void addNewItem_OnExecute(Inventor.NameValueMap Context)
        {
            try
            {
                ComponentOccurrence joint;
                bool selected = false;
                foreach (BrowserNode folder in oPane.TopNode.BrowserNodes)
                {
                    if (folder.Selected)
                    {
                        selected = true;
                    }
                }
                if (selected)
                {
                    obj = m_inventorApplication.TransientObjects.CreateObjectCollection();
                    AssemblyDocument asmDoc = (AssemblyDocument)
                                 m_inventorApplication.ActiveDocument;
                    joint = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick
                              (SelectionFilterEnum.kAssemblyLeafOccurrenceFilter, "Select a part to add");
                    foreach (BrowserNode node in oPane.TopNode.BrowserNodes)
                    {
                        if (node.Selected)
                        {
                            foreach (FieldDataType t in FieldTypes)
                            {
                                if (t.same(node.BrowserNodeDefinition))
                                {
                                    t.compOcc.Add(joint);
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select a browser folder to add a part to");
                }
            }
            catch (Exception)
            {

            }       
        }

        public static void editTypeProperites_OnExecute(Inventor.NameValueMap Context)
        {
            form.readFromData(selectedType);
            form.ShowDialog();
        }

        public static void addNewType_OnExecute(Inventor.NameValueMap Context)
        {
            EnterName form = new EnterName();
            System.Windows.Forms.Application.Run(form);
        }
        public void test(Inventor.NameValueMap Context)
        {
            PropertySets sets = m_inventorApplication.ActiveDocument.PropertySets;
            PropertySet set = null;
            try
            {
                set = sets.Add("hi");
            }
            catch (Exception e)
            {
                foreach (PropertySet se in sets)
                {
                    if (se.DisplayName.Equals("hi"))
                    {
                        set = se;
                    }
                }
            }
            AssemblyDocument asmDoc = (AssemblyDocument)
                         m_inventorApplication.ActiveDocument;
            ComponentOccurrence joint = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick
                      (SelectionFilterEnum.kAssemblyLeafOccurrenceFilter, "Select a part to add");
            object resultObj = null;
            object context = null;
            object other = null;
            byte[] refKey = new byte[0];
            joint.GetReferenceKey(ref refKey, 0);
            String s = m_inventorApplication.ActiveDocument.ReferenceKeyManager.KeyToString(refKey) + "¯\\_(:()_/¯";
            try
            {
                set.Add(s, "colliderType", 2);
            }
            catch (Exception)
            {
                set.ItemByPropId[2].Value = s;
            }
            Char[] arr = { '¯', '\\', '_', '(', ':', '(', ')', '_', '/', '¯' };
            String[] f = ((String)set.ItemByPropId[2].Value).Split(arr);
            foreach (String m in f)
            {
                if (!m.Equals(""))
                {
                    m_inventorApplication.ActiveDocument.ReferenceKeyManager.StringToKey(m, ref refKey);
                    if (m_inventorApplication.ActiveDocument.ReferenceKeyManager.CanBindKeyToObject(refKey, 0, out resultObj, out context))
                    {
                        object obje = StandardAddInServer.m_inventorApplication.ActiveDocument.ReferenceKeyManager.
                        BindKeyToObject(refKey, 0, out other);
                        ((ComponentOccurrence)obje).Visible = false;
                    }
                }
            }
        }
        public void startExport_OnExecute(Inventor.NameValueMap Context)
        {

            try
            {
                inExportView = true;
                addNewType.Enabled = true;
                editType.Enabled = true;
                addNewItem.Enabled = true;
                beginExporter.Enabled = false;
                cancleExport.Enabled = true;
                exportField.Enabled = true;
                accessInnerAssemblies.Enabled = true;
                AssemblyDocument asmDoc = (AssemblyDocument)m_inventorApplication.ActiveDocument;
                BrowserNodeDefinition oDef;
                oDoc = m_inventorApplication.ActiveDocument;
                oPanes = oDoc.BrowserPanes;
                ObjectCollection oOccurrenceNodes;
                oOccurrenceNodes = m_inventorApplication.TransientObjects.CreateObjectCollection();
                oSet = oDoc.CreateHighlightSet();
                oSet.Color = m_inventorApplication.TransientObjects.CreateColor(125, 0, 255);

                try
                {// if no browser pane previously created then create a new one
                    ClientNodeResources oRscs = oPanes.ClientNodeResources;
                    oRsc = oRscs.Add(m_ClientId, 1, null);
                    oDef = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition("Top Node", 3, null);
                    oPane = oPanes.AddTreeBrowserPane("Select Joints", m_ClientId, oDef);
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
                readSave();
                TimerWatch();
            }
            catch (Exception e)
            {

                MessageBox.Show(e.StackTrace);
            }
        }
        /*public static void beginExporter_OnExecute(Inventor.NameValueMap Context)
        {
                inExportView = true;
                addNewType.Enabled = true;
                editType.Enabled = true;
                addNewItem.Enabled = true;
                beginExporter.Enabled = false;
                cancleExport.Enabled = true;
                exportField.Enabled = true;
                accessInnerAssemblies.Enabled = true;
                oDoc = m_inventorApplication.ActiveDocument;
                oPanes = oDoc.BrowserPanes;
                ClientNodeResources oRscs = oPanes.ClientNodeResources;
                stdole.IPictureDisp clientNodeIcon = AxHostConverter.ImageToPictureDisp(new Bitmap(@"C:\Users\t_gracj\Desktop\git\Exporter-Research\InventorAddInBrowserPaneAttempt5\InventorAddInBrowserPaneAttempt5\Resources\test.bmp"));
                ClientNodeResource oRsc = oRscs.Add(m_ClientId, 1, clientNodeIcon);
                BrowserNodeDefinition oDef = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition("Top Node", 3, oRsc);
                oPane = oPanes.AddTreeBrowserPane("Object types", m_ClientId, oDef);
        }*/

        public void cancleExporter_OnExecute(Inventor.NameValueMap Context)
        {
            try
            {
                inExportView = false;
                foreach(FieldDataType data in FieldTypes)
                {
                    writeSave(data);
                }
                writeBrowserFolderNames();
                FieldTypes = new ArrayList();
                foreach (BrowserNode folder in oPane.TopNode.BrowserNodes)
                {
                    folder.Delete();
                }
                addNewType.Enabled = false;
                editType.Enabled = false;
                addNewItem.Enabled = false;
                beginExporter.Enabled = true;
                cancleExport.Enabled = false;
                exportField.Enabled = false;
                accessInnerAssemblies.Enabled = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void readSave()
        {
            try
            {
                ArrayList lis = new ArrayList();
                byte[] refKey;
                PropertySets sets = m_inventorApplication.ActiveDocument.PropertySets;
                other = null;
                context = null;
                String name = "";
                resultObj = null;
                Char[] arr = { '¯', '\\', '_', '(', ':', '(', ')', '_', '/', '¯' };
                foreach (PropertySet s in sets)
                {
                    if (s.DisplayName.Equals("Number of Folders"))
                    {
                        name = (String)s.ItemByPropId[2].Value;
                    }
                }
                String[] names = name.Split(arr);
                foreach (String n in names)
                {
                    if (!n.Equals(""))
                    {
                        foreach (PropertySet set in sets)
                        {
                            if (set.Name.Equals(n))
                            {
                                obj = m_inventorApplication.TransientObjects.CreateObjectCollection();
                                String[] keys = ((String)set.ItemByPropId[11].Value).Split(arr);
                                foreach (String m in keys)
                                {
                                    if (!m.Equals(""))
                                    {
                                        resultObj = null;
                                        other = null;
                                        context = null;
                                        refKey = new byte[0];
                                        m_inventorApplication.ActiveDocument.ReferenceKeyManager.StringToKey(m, ref refKey);
                                        if (m_inventorApplication.ActiveDocument.ReferenceKeyManager.CanBindKeyToObject(refKey, 0, out resultObj, out context))
                                        {
                                            object obje = StandardAddInServer.m_inventorApplication.ActiveDocument.ReferenceKeyManager.
                                            BindKeyToObject(refKey, 0, out other);
                                            lis.Add((ComponentOccurrence)obje);
                                        }
                                    }
                                }
                                BrowserNodeDefinition selectedFolder = addType(((String)set.ItemByPropId[10].Value));
                                FieldDataType field = new FieldDataType(selectedFolder);
                                field.colliderType = (ColliderType)set.ItemByPropId[2].Value;
                                field.X = (double)set.ItemByPropId[3].Value;
                                field.Y = (double)set.ItemByPropId[4].Value;
                                field.Z = (double)set.ItemByPropId[5].Value;
                                field.Scale = (double)set.ItemByPropId[6].Value;
                                field.Friction = (double)set.ItemByPropId[7].Value;
                                field.Dynamic = (bool)set.ItemByPropId[8].Value;
                                field.Mass = (double)set.ItemByPropId[9].Value;
                                field.compOcc = lis;
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

        private void writeBrowserFolderNames()
        {
            String g = "";
            PropertySets sets = m_inventorApplication.ActiveDocument.PropertySets;
            PropertySet set = null;
            foreach (BrowserFolder folder in oPane.TopNode.BrowserFolders)
            {
                g += folder.Name + "¯\\_(:()_/¯";

            }
            try
            {
                set = sets.Add("Number of Folders");
            }
            catch (Exception e)
            {
                foreach (PropertySet s in sets)
                {
                    if (s.DisplayName.Equals("Number of Folders"))
                    {
                        set = s;
                    }
                }
            }
            try
            {
              set.Add(g, "Number of Folders", 2);
            }
            catch (Exception e)
            {
                set.ItemByPropId[2].Value = g;
            }
        }

        private void writeSave(FieldDataType f)
        {
            PropertySets sets = m_inventorApplication.ActiveDocument.PropertySets;
            PropertySet set = null;
            try
            {
                set = sets.Add(f.Name);
            }
            catch (Exception e)
            {
                foreach (PropertySet s in sets)
                {
                    if (s.DisplayName.Equals(f.Name))
                    {
                        set = s;
                    }
                }
            }
            String g = "";
            byte[] refKey = new byte[0];
            try
            {
                foreach (ComponentOccurrence n in f.compOcc)
                {
                    refKey = new byte[0];
                    n.GetReferenceKey(ref refKey, 0);
                    g += (m_inventorApplication.ActiveDocument.ReferenceKeyManager.KeyToString(refKey)) + "¯\\_(:()_/¯";
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            try
            {
                
                set.Add(f.colliderType, "colliderType", 2);
                set.Add(f.X, "X", 3);
                set.Add(f.Y, "Y", 4);
                set.Add(f.Z, "Z", 5);
                set.Add(f.Scale, "Scale", 6);
                set.Add(f.Friction, "Friction", 7);
                set.Add(f.Dynamic, "Dynamic", 8);
                set.Add(f.Mass, "Mass", 9);
                set.Add(f.Name, "Name", 10);
                set.Add(g, "items", 11);
            }
            catch (Exception e)
            {
                set.ItemByPropId[2].Value = f.colliderType;
                set.ItemByPropId[3].Value = f.X;
                set.ItemByPropId[4].Value = f.Y;
                set.ItemByPropId[5].Value = f.Z;
                set.ItemByPropId[6].Value = f.Scale;
                set.ItemByPropId[7].Value = f.Friction;
                set.ItemByPropId[8].Value = f.Dynamic;
                set.ItemByPropId[9].Value = f.Mass;
                set.ItemByPropId[10].Value = f.Name;
                set.ItemByPropId[11].Value = g;
            }
        }

        public void exportField_OnExecute(Inventor.NameValueMap Context)
        {
            inExportView = false;
            FieldTypes = new ArrayList();
            foreach (BrowserNode folder in oPane.TopNode.BrowserNodes)
            {
                folder.Delete();
            }
            addNewType.Enabled = false;
            editType.Enabled = false;
            addNewItem.Enabled = false;
            beginExporter.Enabled = true;
            cancleExport.Enabled = false;
            exportField.Enabled = false;
            accessInnerAssemblies.Enabled = false;
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