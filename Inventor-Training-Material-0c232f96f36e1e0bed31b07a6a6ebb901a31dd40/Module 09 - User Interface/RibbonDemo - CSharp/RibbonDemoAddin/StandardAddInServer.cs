using System;
using System.Runtime.InteropServices;
using Inventor;
using Microsoft.Win32;

using System.Drawing;
using stdole;
using System.Windows.Forms;

namespace RibbonDemoAddin
{
    /// <summary>
    /// This is the primary AddIn Server class that implements the ApplicationAddInServer interface
    /// that all Inventor AddIns are required to implement. The communication between Inventor and
    /// the AddIn is via the methods on this interface.
    /// </summary>
    [GuidAttribute("be3ab243-6c43-4dd7-9272-06685bda91af")]
    public class StandardAddInServer : Inventor.ApplicationAddInServer
    {
        static string addInGuid = "be3ab243-6c43-4dd7-9272-06685bda91af";

        ButtonDefinition _buttonDef1;

        // Inventor application object.
        static private Inventor.Application m_inventorApplication;

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

            // TODO: Add ApplicationAddInServer.Activate implementation.
            // e.g. event initialization, command creation etc.
            ControlDefinitions oCtrlDefs = m_inventorApplication.CommandManager.ControlDefinitions;

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            string[] resources = assembly.GetManifestResourceNames();

            System.IO.Stream oStream32 = assembly.GetManifestResourceStream("RibbonDemoAddin.resources.button 32x32.ico");
            System.IO.Stream oStream16 = assembly.GetManifestResourceStream("RibbonDemoAddin.resources.button 16x16.ico");

            System.Drawing.Icon oIcon32 = new System.Drawing.Icon(oStream32);
            System.Drawing.Icon oIcon16 = new System.Drawing.Icon(oStream16);

            object oIPictureDisp32 = AxHostConverter.ImageToPictureDisp(oIcon32.ToBitmap());
            object oIPictureDisp16 = AxHostConverter.ImageToPictureDisp(oIcon16.ToBitmap());

            try
            {
                _buttonDef1 = oCtrlDefs["Autodesk:BrowserDemo:ButtonDef1"] as ButtonDefinition;
            }
            catch (Exception ex)
            {
                _buttonDef1 = oCtrlDefs.AddButtonDefinition("Ribbon Demo1",
                                                          "Autodesk:RibbonDemoC#:ButtonDef1",
                                                          CommandTypesEnum.kEditMaskCmdType,
                                                          addInGuid,
                                                          "Ribbon Demo",
                                                          "Ribbon Demo Description",
                                                          oIPictureDisp16,
                                                          oIPictureDisp32,
                                                          ButtonDisplayEnum.kDisplayTextInLearningMode);

                CommandCategory cmdCat = m_inventorApplication.CommandManager.CommandCategories.Add("RibbonDemo C#", "Autodesk:CmdCategory:RibbonDemoC#", addInGuid);

                cmdCat.Add(_buttonDef1);
            }

            if (firstTime)
            {
                try
                {
                    if (m_inventorApplication.UserInterfaceManager.InterfaceStyle == InterfaceStyleEnum.kRibbonInterface)
                    {
                        Ribbon ribbon = m_inventorApplication.UserInterfaceManager.Ribbons["Part"];

                        RibbonTab tab = ribbon.RibbonTabs["id_TabModel"];

                        try
                        {
                            RibbonPanel panel = tab.RibbonPanels.Add("Ribbon Demo", "Autodesk:RibbonDemoC#:Panel1", addInGuid, "", false);

                            CommandControl control1 = panel.CommandControls.AddButton(_buttonDef1, true, true, "", false);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                    {
                        CommandBar oCommandBar = m_inventorApplication.UserInterfaceManager.CommandBars["PMxPartFeatureCmdBar"];
                        oCommandBar.Controls.AddButton(_buttonDef1, 0);
                    }
                }
                catch
                {
                    CommandBar oCommandBar = m_inventorApplication.UserInterfaceManager.CommandBars["PMxPartFeatureCmdBar"];
                    oCommandBar.Controls.AddButton(_buttonDef1, 0);
                }
            }

            _buttonDef1.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(_buttonDef1_OnExecute);
        }

        void _buttonDef1_OnExecute(NameValueMap Context)
        {
            System.Windows.Forms.MessageBox.Show("Button clicked!!", "Ribbon Demo");
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

            Marshal.ReleaseComObject(_buttonDef1);
            _buttonDef1 = null;

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
}
