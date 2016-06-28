////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved 
// Written by Philippe Leefsma 2011 - ADN/Developer Technical Services
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted, 
// provided that the above copyright notice appears in all copies and 
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting 
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Runtime.InteropServices;
using Inventor;
using Microsoft.Win32;

using Autodesk.ADN.Utility.WinUtils;
using Autodesk.ADN.Utility.InventorUtils;

namespace ClientGraphicsDemoAU
{
    /// <summary>
    /// This is the primary AddIn Server class that implements the ApplicationAddInServer interface
    /// that all Inventor AddIns are required to implement. The communication between Inventor and
    /// the AddIn is via the methods on this interface.
    /// </summary>
    [GuidAttribute("3eac4958-45c3-42a2-8e29-fa9a88679af5"), 
    ComVisible(true)] 
    public class StandardAddInServer : Inventor.ApplicationAddInServer
    {
        // Inventor application object.
        Inventor.Application _Application;

        ButtonDefinition _MainControlButtonDef;

        ApplicationAddInSite _addInSiteObject;

        SamplesDockWnd _dockableWindow;

        public StandardAddInServer()
        {
  
        }

        #region ApplicationAddInServer Members

        public void Activate(Inventor.ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            _addInSiteObject = addInSiteObject;

            // This method is called by Inventor when it loads the addin.
            // The AddInSiteObject provides access to the Inventor Application object.
            // The FirstTime flag indicates if the addin is loaded for the first time.
            
            // Initialize AddIn members.
            _Application = addInSiteObject.Application;

            AdnInventorUtilities.Initialize(_Application, this.GetType());

            ControlDefinitions ctrlDefs =
               _Application.CommandManager.ControlDefinitions;

            System.Drawing.Icon Icon32 = Resources.CGAUDemo_32x32;
            System.Drawing.Icon Icon16 = Resources.CGAUDemo_16x16;

            object IPictureDisp32 = PictureDispConverter.ToIPictureDisp(Icon32);
            object IPictureDisp16 = PictureDispConverter.ToIPictureDisp(Icon16);

            try
            {
                _MainControlButtonDef = 
                    ctrlDefs["Autodesk:AdskCGAUDemo:MainCtrl"] as ButtonDefinition;
            }
            catch
            {
                _MainControlButtonDef =
                    ctrlDefs.AddButtonDefinition(
                        "   Demo   \n   Control   ",
                        "Autodesk:AdskCGAUDemo:MainCtrl",
                        CommandTypesEnum.kEditMaskCmdType,
                        AdnInventorUtilities.AddInGuid,
                        "Client Graphics Demo AU",
                        "Client Graphics Demo AU",
                        IPictureDisp16,
                        IPictureDisp32,
                        ButtonDisplayEnum.kDisplayTextInLearningMode);
            }

            _MainControlButtonDef.OnExecute += 
                new ButtonDefinitionSink_OnExecuteEventHandler(MainControlButtonDef_OnExecute);

            if (firstTime)
            {
                Ribbon partRibbon = _Application.UserInterfaceManager.Ribbons["Part"];
                Ribbon asmRibbon = _Application.UserInterfaceManager.Ribbons["Assembly"];
                Ribbon dwgRibbon = _Application.UserInterfaceManager.Ribbons["Drawing"];

                AddToRibbon(partRibbon, AdnInventorUtilities.AddInGuid);
                AddToRibbon(asmRibbon, AdnInventorUtilities.AddInGuid);
                AddToRibbon(dwgRibbon, AdnInventorUtilities.AddInGuid);
            } 
        }

        void AddToRibbon(Ribbon ribbon, string addInGuid)
        {
            RibbonTab Tab = ribbon.RibbonTabs["id_TabTools"];

            RibbonPanel Panel;
                
            try
            {
                Panel = Tab.RibbonPanels["Autodesk:AdskCGAUDemo:DemoPanel"];
            }
            catch
            {
                Panel = Tab.RibbonPanels.Add(
                "AU Client Graphics",
                "Autodesk:AdskCGAUDemo:DemoPanel",
                addInGuid,
                "", false);
            }

            Panel.CommandControls.AddButton(
                _MainControlButtonDef,
                true, true, "", false);
        }

        void MainControlButtonDef_OnExecute(NameValueMap Context)
        {
            if (_dockableWindow == null)
            {
                _dockableWindow = new SamplesDockWnd(_addInSiteObject, DockingStateEnum.kDockLeft);

                _dockableWindow.InitializeSamples();

                _dockableWindow.Show();
            }
            else
            {
                _dockableWindow.Visible = true;
            }
        }

        public void Deactivate()
        {
            // This method is called by Inventor when the AddIn is unloaded.
            // The AddIn will be unloaded either manually by the user or
            // when the Inventor session is terminated

            // TODO: Add ApplicationAddInServer.Deactivate implementation

            // Release objects.
            Marshal.ReleaseComObject(_Application);
            _Application = null;

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

        #region COM Registration functions

        /// <summary>
        /// Registers this class as an Add-In for Autodesk Inventor.
        /// This function is called when the assembly is registered for COM.
        /// </summary>
        [ComRegisterFunctionAttribute()]
        public static void Register(Type t)
        {
            RegistryKey clssRoot = Registry.ClassesRoot;
            RegistryKey clsid = null;
            RegistryKey subKey = null;

            try
            {
                clsid = clssRoot.CreateSubKey("CLSID\\" + AddInGuid(t));
                clsid.SetValue(null, "ClientGraphicsDemoAU");
                subKey = clsid.CreateSubKey("Implemented Categories\\{39AD2B5C-7A29-11D6-8E0A-0010B541CAA8}");
                subKey.Close();

                subKey = clsid.CreateSubKey("Settings");
                subKey.SetValue("AddInType", "Standard");
                subKey.SetValue("LoadOnStartUp", "1");

                //subKey.SetValue("SupportedSoftwareVersionLessThan", "");
                subKey.SetValue("SupportedSoftwareVersionGreaterThan", "14..");
                //subKey.SetValue("SupportedSoftwareVersionEqualTo", "");
                //subKey.SetValue("SupportedSoftwareVersionNotEqualTo", "");
                //subKey.SetValue("Hidden", "0");
                //subKey.SetValue("UserUnloadable", "1");
                subKey.SetValue("Version", 0);
                subKey.Close();

                subKey = clsid.CreateSubKey("Description");
                subKey.SetValue(null, "ClientGraphicsDemoAU");
            }
            catch
            {
                System.Diagnostics.Trace.Assert(false);
            }
            finally
            {
                if (subKey != null) subKey.Close();
                if (clsid != null) clsid.Close();
                if (clssRoot != null) clssRoot.Close();
            }

        }

        /// <summary>
        /// Unregisters this class as an Add-In for Autodesk Inventor.
        /// This function is called when the assembly is unregistered.
        /// </summary>
        [ComUnregisterFunctionAttribute()]
        public static void Unregister(Type t)
        {
            RegistryKey clssRoot = Registry.ClassesRoot;
            RegistryKey clsid = null;

            try
            {
                clssRoot = Microsoft.Win32.Registry.ClassesRoot;
                clsid = clssRoot.OpenSubKey("CLSID\\" + AddInGuid(t), true);
                clsid.SetValue(null, "");
                clsid.DeleteSubKeyTree("Implemented Categories\\{39AD2B5C-7A29-11D6-8E0A-0010B541CAA8}");
                clsid.DeleteSubKeyTree("Settings");
                clsid.DeleteSubKeyTree("Description");
            }
            catch { }
            finally
            {
                if (clsid != null) clsid.Close();
                if (clssRoot != null) clssRoot.Close();
            }
        }

        // This function uses reflection to get the value for the GuidAttribute attached to the class.
        private static String AddInGuid(Type t)
        {
            string guid = "";

            try
            {
                Object[] customAttributes = t.GetCustomAttributes(typeof(GuidAttribute), false);
                GuidAttribute guidAttribute = (GuidAttribute)customAttributes[0];
                guid = "{" + guidAttribute.Value.ToString() + "}";
            }
            catch
            {
            }

            return guid;

        }

        #endregion
    }
}
