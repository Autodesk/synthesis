using System;
using System.Runtime.InteropServices;
using Inventor;
using Microsoft.Win32;
using System.Windows.Forms;

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
        private Inventor.Application m_inventorApplication;
        private Ribbon partRibbon;
        private RibbonTab partTab;
        RibbonPanel partPanel;
        ButtonDefinition beginExporter;
        String m_ClientId;
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
                partPanel.CommandControls.AddButton(beginExporter);
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
        public void beginExporter_OnExecute(Inventor.NameValueMap Context)
        {

        }
    }
}
