using System;
using System.Runtime.InteropServices;
using Inventor;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Drawing;
using stdole;


namespace ZeroRibbon
{
    /// <summary>
    /// This is the primary AddIn Server class that implements the ApplicationAddInServer interface
    /// that all Inventor AddIns are required to implement. The communication between Inventor and
    /// the AddIn is via the methods on this interface.
    /// </summary>
    [GuidAttribute("bbfbc1b3-ac45-436c-a1f7-969a765df57d")]
    public class StandardAddInServer : Inventor.ApplicationAddInServer
    {

        // Inventor application object.
        private Inventor.Application m_inventorApplication;

        static string addInGuid = "bbfbc1b3-ac45-436c-a1f7-969a765df57d";

        ButtonDefinition Zero_GetStarted_LaunchPanel_ButtonDef;
        ButtonDefinition Zero_GetStarted_NewPanel_ButtonDef;

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

            System.IO.Stream oStream1 = assembly.GetManifestResourceStream("ZeroRibbon.resources.Icon1.ico");
            System.IO.Stream oStream2 = assembly.GetManifestResourceStream("ZeroRibbon.resources.Icon2.ico");

            System.Drawing.Icon oIcon1 = new System.Drawing.Icon(oStream1);
            System.Drawing.Icon oIcon2 = new System.Drawing.Icon(oStream2);

            object oIPictureDisp1 = AxHostConverter.ImageToPictureDisp(oIcon1.ToBitmap());
            object oIPictureDisp2 = AxHostConverter.ImageToPictureDisp(oIcon2.ToBitmap());

            try
            {
                Zero_GetStarted_LaunchPanel_ButtonDef = oCtrlDefs["Autodesk:ZeroRibbon:ButtonDef1"] as ButtonDefinition;
                Zero_GetStarted_NewPanel_ButtonDef = oCtrlDefs["Autodesk:ZeroRibbon:ButtonDef2"] as ButtonDefinition;
            }
            catch (Exception ex)
            {
                Zero_GetStarted_LaunchPanel_ButtonDef = oCtrlDefs.AddButtonDefinition("Ribbon Demo1",
                                                          "Autodesk:ZeroRibbon:ButtonDef1",
                                                          CommandTypesEnum.kEditMaskCmdType,
                                                          addInGuid,
                                                          "Ribbon Demo",
                                                          "Ribbon Demo Description",
                                                          oIPictureDisp1,
                                                          oIPictureDisp1,
                                                          ButtonDisplayEnum.kDisplayTextInLearningMode);

                Zero_GetStarted_NewPanel_ButtonDef = oCtrlDefs.AddButtonDefinition("Ribbon Demo2",
                                                        "Autodesk:ZeroRibbon:ButtonDef2",
                                                        CommandTypesEnum.kEditMaskCmdType,
                                                        addInGuid,
                                                        "Ribbon Demo",
                                                        "Ribbon Demo Description",
                                                        oIPictureDisp2,
                                                        oIPictureDisp2,
                                                        ButtonDisplayEnum.kDisplayTextInLearningMode);

                CommandCategory cmdCat = m_inventorApplication.CommandManager.CommandCategories.Add("RibbonDemo C#", "Autodesk:CmdCategory:RibbonDemoC#", addInGuid);


                cmdCat.Add(Zero_GetStarted_LaunchPanel_ButtonDef);
                cmdCat.Add(Zero_GetStarted_NewPanel_ButtonDef);
            }



            Ribbon ribbon = m_inventorApplication.UserInterfaceManager.Ribbons["ZeroDoc"];
            RibbonTab tab = ribbon.RibbonTabs["id_GetStarted"];
            RibbonPanel built_panel = tab.RibbonPanels["id_Panel_Launch"];
            built_panel.CommandControls.AddButton(Zero_GetStarted_LaunchPanel_ButtonDef, true);

            RibbonPanel panel1 = tab.RibbonPanels.Add("Ribbon Demo", "Autodesk:RibbonDemoC#:Panel1", addInGuid, "", false);
            panel1.CommandControls.AddButton(Zero_GetStarted_NewPanel_ButtonDef, true);


            Zero_GetStarted_LaunchPanel_ButtonDef.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(Zero_GetStarted_LaunchPanel_ButtonDef_OnExecute);
            Zero_GetStarted_NewPanel_ButtonDef.OnExecute += new ButtonDefinitionSink_OnExecuteEventHandler(Zero_GetStarted_NewPanel_ButtonDef_OnExecute);
        }

        void Zero_GetStarted_LaunchPanel_ButtonDef_OnExecute(NameValueMap Context)
        {
            System.Windows.Forms.MessageBox.Show("the button in LaunchPanel is clicked!");
        }

        void Zero_GetStarted_NewPanel_ButtonDef_OnExecute(NameValueMap Context)
        {
            System.Windows.Forms.MessageBox.Show("the button in my panel is clicked!");
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
