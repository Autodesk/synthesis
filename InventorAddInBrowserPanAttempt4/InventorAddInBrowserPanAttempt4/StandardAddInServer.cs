using System;
using System.Runtime.InteropServices;
using Inventor;
using Microsoft.Win32;

namespace InventorAddInBrowserPanAttempt4
{
    /// <summary>
    /// This is the primary AddIn Server class that implements the ApplicationAddInServer interface
    /// that all Inventor AddIns are required to implement. The communication between Inventor and
    /// the AddIn is via the methods on this interface.
    /// </summary>
    [GuidAttribute("153c54e3-f9bc-4759-8f4e-f4f5c2624d78")]
    public class StandardAddInServer : Inventor.ApplicationAddInServer
    {

        // Inventor application object.
        private Inventor.Application m_inventorApplication;
        private ButtonDefinition mAsmButtonDef;
        private ButtonDefinition mPartButtonDef;
        private String strAddInGuid = "e383aeef-321c-42c1-8ed0-3ab07dc33c5d";

        public StandardAddInServer()
        {
        }

        #region ApplicationAddInServer Members

        public void Activate(Inventor.ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            // This method is called by Inventor when it loads the addin.
            // The AddInSiteObject provides access to the Inventor Application object.
            // The FirstTime flag indicates if the addin is loaded for the first time.

            // Initialize AddIn members.
            m_inventorApplication = addInSiteObject.Application;
            ControlDefinitions mCtrlDefs = m_inventorApplication.CommandManager.ControlDefinitions;
            // TODO: Add ApplicationAddInServer.Activate implementation.
            // e.g. event initialization, command creation etc.
            IPictureDisp mIPictureDisp1 = _
             PictureDispConverter.ToIPictureDisp _
                                    (My.Resources.Tools);
            IPictureDisp mIPictureDisp2 = _
             PictureDispConverter.ToIPictureDisp _
                                    (My.Resources.wbTest);
            mAsmButtonDef = mCtrlDefs.AddButtonDefinition("Assembly Cmd", "Autodesk:RibbonVBTest:Button1", CommandTypesEnum.kQueryOnlyCmdType,
            strAddInGuid, "Description", "Tooltip text for Assembly", mIPictureDisp1, mIPictureDisp1,
            ButtonDisplayEnum.kDisplayTextInLearningMode);
            mPartButtonDef = mCtrlDefs.AddButtonDefinition("Part Cmd", "Autodesk:RibbonVBTest:Button2", CommandTypesEnum.kQueryOnlyCmdType,
            strAddInGuid, "Description", "Tooltip text for Part", mIPictureDisp2, mIPictureDisp2,
            ButtonDisplayEnum.kDisplayTextInLearningMode);
            if (firstTime)
            {
                UserInterfaceManager UIManager = m_inventorApplication.UserInterfaceManager;
                if(UIManager.InterfaceStyle == InterfaceStyleEnum.kRibbonInterface){
                    Inventor.Ribbon assemblyRibbon = UIManager.Ribbons.Item("Assembly");
                    Inventor.RibbonTab assemblyTab = assemblyRibbon.RibbonTabs.Item("id_TabAssemble");
                    Inventor.RibbonPanel panel1 = assemblyTab.RibbonPanels.Add("Custom commands",
                        "Autodesk:RibbonVBTest:Panel1",strAddInGuid);
                    panel1.CommandControls.AddButton(mAsmButtonDef, true);
                    Inventor.Ribbon partRibbon = UIManager.Ribbons.Item("Part");
                    Inventor.RibbonTab partTab = assemblyRibbon.RibbonTabs.Item("id_TabModel");
                    Inventor.RibbonPanel panel2 = modelTab.RibbonPanels.Add("Custom commands",
                                                                                     "Autodes:RibbonVBTest:Panel2",
                                                                                       strAddInGuid);
                    panel2.CommandControls.AddButton(mPartButtonDef, true);
                } else
                {
                    CommandBar mAsmCommandBar = UIManager.CommandBars("AMxAssemblyPanelCmdBar");
                    mAsmCommandBar.Controls.AddButton(mAsmButtonDef, 0);
                    CommandBar mPartCommandBar = UIManager.CommandBars("PMxPartFeatureCmdBar");
                        mPartCommandBar.Controls.AddButton(mPartButtonDef, 0);
                }
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

    }
}
