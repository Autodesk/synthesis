using System;
using System.Runtime.InteropServices;
using Inventor;
using Microsoft.Win32;
using System.Windows.Forms;
using BxDFieldExporter;
using System.Drawing;
using System.Collections;

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
        private static Inventor.Application m_inventorApplication;
        private static Ribbon partRibbon;
        private static RibbonTab partTab;
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
        static BrowserPanes oPanes;
        static Inventor.BrowserPane oPane;
        static Document oDoc;
        bool first;
        UserInputEvents UIEvent;
        Inventor.UserInputEventsSink_OnSelectEventHandler click_OnSelectEventDelegate;
        static bool inExportView;
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
                first = true;
                // Get the assembly ribbon.
                partRibbon = m_inventorApplication.UserInterfaceManager.Ribbons["Assembly"];
                // Get the "Part" tab.
                partTab = partRibbon.RibbonTabs.Add("Field Exporter", "BxD:FieldExporter", "{e50be244-9f7b-4b94-8f87-8224faba8ca1}");
                partPanel = partTab.RibbonPanels.Add("Exporter Control", "BxD:FieldExporter:ExporterControl", "{e50be244-9f7b-4b94-8f87-8224faba8ca1}");
                // TODO: Add ApplicationAddInServer.Activate implementation.
                // e.g. event initialization, command creation etc.
                ControlDefinitions controlDefs = m_inventorApplication.CommandManager.ControlDefinitions;
                FieldTypes = new ArrayList();
                beginExporter = controlDefs.AddButtonDefinition("Begin Exporter", "InventorAddInBrowserPaneAttempt5:BeginExporter", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                beginExporter.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(beginExporter_OnExecute);
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
                accessInnerAssemblies.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(accessSubassemblies_OnExecute);

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
            Boolean found;
            NativeBrowserNodeDefinition brow;
            if (SelectionDevice == SelectionDeviceEnum.kGraphicsWindowSelection && inExportView)
            {
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
            }
            else if (SelectionDevice == SelectionDeviceEnum.kBrowserSelection && inExportView)
            {
                foreach (Object sel in JustSelectedEntities)
                {
                    if (sel is BrowserFolder)
                    {
                        foreach (FieldDataType f in FieldTypes)
                        {
                            if (f.same((BrowserFolder)sel))
                            {
                                selectedType = f;
                            }
                        }
                    }
                }
            }
        }

        public static void addType(String name)
        {
            BrowserFolder selectedFolder = oPane.AddBrowserFolder(name);
            FieldTypes.Add(new FieldDataType(selectedFolder));
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
                    selectedType = (FieldDataType)FieldTypes[(FieldTypes.Add(new FieldDataType(selectedFolder)))];
                    foreach (FieldDataType t in FieldTypes)
                    {
                        if (t.same(folder))
                        {
                            t.copyToNewType(selectedType);
                        }
                    }
                    foreach (BrowserNode n in folder.BrowserNode.BrowserNodes)
                    {
                        n.Delete();
                    }
                    folder.Delete();
                    folder.Delete();
                    foreach (FieldDataType t in FieldTypes)
                    {
                        if (t.same(folder))
                        {
                            FieldTypes.Remove(t);
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

        public void addNewItem_OnExecute(Inventor.NameValueMap Context)
        {
            obj = m_inventorApplication.TransientObjects.CreateObjectCollection();
            AssemblyDocument asmDoc = (AssemblyDocument)
                         m_inventorApplication.ActiveDocument;
            ComponentOccurrence joint = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick
                      (SelectionFilterEnum.kAssemblyOccurrenceFilter, "Select a part to add");
            foreach(BrowserFolder folder in oPane.TopNode.BrowserFolders)
            {
                name = folder.Name;
                if (folder.BrowserNode.Selected)
                {
                    foreach (BrowserNode m in folder.BrowserNode.BrowserNodes)
                    {
                        obj.Add(m);
                    }
                    obj.Add(oPane.GetBrowserNodeFromObject(joint));
                    BrowserFolder selectedFolder = oPane.AddBrowserFolder(name, obj);
                    selectedType = (FieldDataType)FieldTypes[(FieldTypes.Add(new FieldDataType(selectedFolder)))];
                    foreach (FieldDataType t in FieldTypes)
                    {
                        if (t.same(folder))
                        {
                            t.copyToNewType(selectedType);
                        }
                    }
                            foreach (BrowserNode n in folder.BrowserNode.BrowserNodes)
                    {
                        n.Delete();
                    }
                    folder.Delete();
                    folder.Delete();
                    foreach (FieldDataType t in FieldTypes)
                    {
                        if (t.same(folder))
                        {
                            FieldTypes.Remove(t);
                        }
                    }
                }
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

        public static void beginExporter_OnExecute(Inventor.NameValueMap Context)
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
                addNewType.Enabled = true;
                editType.Enabled = true;
                addNewItem.Enabled = true;
                beginExporter.Enabled = false;
                cancleExport.Enabled = true;
                exportField.Enabled = true;
        }

        public void cancleExporter_OnExecute(Inventor.NameValueMap Context)
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