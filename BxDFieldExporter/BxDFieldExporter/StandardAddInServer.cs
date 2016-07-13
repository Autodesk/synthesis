using System;
using System.Runtime.InteropServices;
using Inventor;
using Microsoft.Win32;
using System.Windows.Forms;
using BxDFieldExporter;
using System.Drawing;

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
        static BrowserPanes oPanes;
        static Inventor.BrowserPane oPane;
        static Document oDoc;
        bool first;
        ObjectCollection obj;
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

                beginExporter = controlDefs.AddButtonDefinition("Begin Exporter", "InventorAddInBrowserPaneAttempt5:BeginExporter", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                beginExporter.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(beginExporter_OnExecute);
                addNewType = controlDefs.AddButtonDefinition("Add new type", "InventorAddInBrowserPaneAttempt5:AddNewType", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                addNewType.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(addNewType_OnExecute);
                addNewItem = controlDefs.AddButtonDefinition("Add new item", "InventorAddInBrowserPaneAttempt5:AddNewItem", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                addNewItem.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(addNewItem_OnExecute);
                editType = controlDefs.AddButtonDefinition("Edit type properties", "InventorAddInBrowserPaneAttempt5:EditTypeProperties", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                editType.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(editTypeProperites_OnExecute);
                /*accessInnerAssemblies = controlDefs.AddButtonDefinition("Access subassembly", "InventorAddInBrowserPaneAttempt5:AccessSubassembly", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, null, null);
                accessInnerAssemblies.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(accessSubassemblies_OnExecute);*/

                partPanel.CommandControls.AddButton(beginExporter);
                partPanel.CommandControls.AddButton(addNewType);
                partPanel.CommandControls.AddButton(addNewItem);
                partPanel.CommandControls.AddButton(editType);
                partPanel.CommandControls.AddButton(accessInnerAssemblies);
                addNewType.Enabled = false;
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

        public static void addType(String name)
        {
            oPane.AddBrowserFolder(name);
        }

        /*public void accessSubassemblies_OnExecute(Inventor.NameValueMap Context)
        {
            ObjectCollection oj =  obj = m_inventorApplication.TransientObjects.CreateObjectCollection();
            AssemblyDocument asmDoc = (AssemblyDocument)
                         m_inventorApplication.ActiveDocument;
            ComponentOccurrence joint = (ComponentOccurrence)m_inventorApplication.CommandManager.Pick
                      (SelectionFilterEnum.kAssemblyOccurrenceFilter, "Select an assembly to access");
            foreach (ComponentOccurrence c in joint.SubOccurrences)
            {
                oj.Add(c);
                MessageBox.Show("DSsd");
            }
            joint.Visible = false;
            foreach(ComponentOccurrence v in oj)
            {
                v.Visible = true;
            }
        }*/

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
                    oPane.AddBrowserFolder(name, obj);
                    foreach (BrowserNode n in folder.BrowserNode.BrowserNodes)
                    {
                        n.Delete();
                    }
                    folder.Delete();
                }
            }
                
        }

        public static void editTypeProperites_OnExecute(Inventor.NameValueMap Context)
        {
            ComponentPropertiesForm form = new ComponentPropertiesForm();
            System.Windows.Forms.Application.Run(form);
        }

        public static void addNewType_OnExecute(Inventor.NameValueMap Context)
        {
            EnterName form = new EnterName();
            System.Windows.Forms.Application.Run(form);
        }

        public static void beginExporter_OnExecute(Inventor.NameValueMap Context)
        {
                oDoc = m_inventorApplication.ActiveDocument;
                oPanes = oDoc.BrowserPanes;
                ObjectCollection oOccurrenceNodes;
                oOccurrenceNodes = m_inventorApplication.TransientObjects.CreateObjectCollection();
                ClientNodeResources oRscs = oPanes.ClientNodeResources;
                stdole.IPictureDisp clientNodeIcon = AxHostConverter.ImageToPictureDisp(new Bitmap(@"C:\Users\t_gracj\Desktop\git\Exporter-Research\InventorAddInBrowserPaneAttempt5\InventorAddInBrowserPaneAttempt5\Resources\test.bmp"));
                ClientNodeResource oRsc = oRscs.Add(m_ClientId, 1, clientNodeIcon);
                BrowserNodeDefinition oDef = (BrowserNodeDefinition)oPanes.CreateBrowserNodeDefinition("Top Node", 3, oRsc);
                oPane = oPanes.AddTreeBrowserPane("Object types", m_ClientId, oDef);
            addNewType.Enabled = true;
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