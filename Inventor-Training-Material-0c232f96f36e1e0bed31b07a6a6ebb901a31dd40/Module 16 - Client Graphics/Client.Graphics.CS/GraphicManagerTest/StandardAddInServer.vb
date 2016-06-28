'////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
'// Copyright (c) Autodesk, Inc. All rights reserved 
'// Written by Philippe Leefsma 2011 - ADN/Developer Technical Services
'//
'// Permission to use, copy, modify, and distribute this software in
'// object code form for any purpose and without fee is hereby granted, 
'// provided that the above copyright notice appears in all copies and 
'// that both that copyright notice and the limited warranty and
'// restricted rights notice below appear in all supporting 
'// documentation.
'//
'// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
'// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
'// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
'// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
'// UNINTERRUPTED OR ERROR FREE.
'////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
Imports Inventor
Imports System.Runtime.InteropServices
Imports Microsoft.Win32

Imports Autodesk.ADN.Utility.WinUtils
Imports Autodesk.ADN.Utility.InventorUtils

Namespace GraphicManagerTest

    <ProgIdAttribute("GraphicManagerTest.StandardAddInServer"), _
    GuidAttribute("45072b22-8df2-4a22-92eb-90a480ca2b73"), _
    ComVisible(True)> _
    Public Class StandardAddInServer
        Implements Inventor.ApplicationAddInServer

        ' Inventor application object.
        Private _Application As Inventor.Application

        Private _MainControlButtonDef As ButtonDefinition

#Region "ApplicationAddInServer Members"

        Public Sub Activate(ByVal addInSiteObject As Inventor.ApplicationAddInSite, _
                            ByVal firstTime As Boolean) _
                            Implements Inventor.ApplicationAddInServer.Activate

            ' This method is called by Inventor when it loads the AddIn.
            ' The AddInSiteObject provides access to the Inventor Application object.
            ' The FirstTime flag indicates if the AddIn is loaded for the first time.

            ' Initialize AddIn members.
            _Application = addInSiteObject.Application

            AdnInventorUtilities.Initialize(_Application, Me.GetType())

            Dim ctrlDefs As ControlDefinitions = _
            _Application.CommandManager.ControlDefinitions

            Dim Icon32 As System.Drawing.Icon = My.Resources.AU_Red_32x32
            Dim Icon16 As System.Drawing.Icon = My.Resources.AU_Red_16x16

            Dim IPictureDisp32 As Object = PictureDispConverter.ToIPictureDisp(Icon32)
            Dim IPictureDisp16 As Object = PictureDispConverter.ToIPictureDisp(Icon16)

            Try

                _MainControlButtonDef = ctrlDefs("Autodesk:AdskCGAUDemoVB:MainCtrl")

            Catch

                _MainControlButtonDef = _
                    ctrlDefs.AddButtonDefinition( _
                        "   Demo   " + vbCrLf + "   VB.Net   ", _
                        "Autodesk:AdskCGAUDemoVB:MainCtrl", _
                        CommandTypesEnum.kEditMaskCmdType, _
                        AdnInventorUtilities.AddInGuid, _
                        "Client Graphics Demo AU", _
                        "Client Graphics Demo AU", _
                        IPictureDisp16, _
                        IPictureDisp32, _
                        ButtonDisplayEnum.kDisplayTextInLearningMode)

            End Try

            AddHandler _MainControlButtonDef.OnExecute, AddressOf Me.MainControlButtonDef_OnExecute

            If (firstTime) Then

                Dim partRibbon As Ribbon = _Application.UserInterfaceManager.Ribbons("Part")
                Dim asmRibbon As Ribbon = _Application.UserInterfaceManager.Ribbons("Assembly")
                Dim dwgRibbon As Ribbon = _Application.UserInterfaceManager.Ribbons("Drawing")

                AddToRibbon(partRibbon, AdnInventorUtilities.AddInGuid)
                AddToRibbon(asmRibbon, AdnInventorUtilities.AddInGuid)
                AddToRibbon(dwgRibbon, AdnInventorUtilities.AddInGuid)

            End If

        End Sub

        Sub AddToRibbon(ByVal ribbon As Ribbon, ByVal addInGuid As String)

            Dim Tab As RibbonTab = ribbon.RibbonTabs("id_TabTools")

            Dim Panel As RibbonPanel

            Try

                Panel = Tab.RibbonPanels("Autodesk:AdskCGAUDemo:DemoPanel")

            Catch ex As Exception

                Panel = Tab.RibbonPanels.Add( _
                "AU Client Graphics", _
                "Autodesk:AdskCGAUDemo:DemoPanel", _
                addInGuid, _
                "", False)

            End Try

            Panel.CommandControls.AddButton( _
                _MainControlButtonDef, _
                True, True, "", False)
        End Sub

        Private Sub MainControlButtonDef_OnExecute(ByVal Context As Inventor.NameValueMap)

            ClientGraphicsMngTest.Demo()

        End Sub

        Public Sub Deactivate() Implements Inventor.ApplicationAddInServer.Deactivate

            ' This method is called by Inventor when the AddIn is unloaded.
            ' The AddIn will be unloaded either manually by the user or
            ' when the Inventor session is terminated.

            ' TODO:  Add ApplicationAddInServer.Deactivate implementation

            ' Release objects.
            Marshal.ReleaseComObject(_Application)
            _Application = Nothing

            System.GC.WaitForPendingFinalizers()
            System.GC.Collect()

        End Sub

        Public ReadOnly Property Automation() As Object Implements Inventor.ApplicationAddInServer.Automation

            ' This property is provided to allow the AddIn to expose an API 
            ' of its own to other programs. Typically, this  would be done by
            ' implementing the AddIn's API interface in a class and returning 
            ' that class object through this property.

            Get
                Return Nothing
            End Get

        End Property

        Public Sub ExecuteCommand(ByVal commandID As Integer) Implements Inventor.ApplicationAddInServer.ExecuteCommand

            ' Note:this method is now obsolete, you should use the 
            ' ControlDefinition functionality for implementing commands.

        End Sub

#End Region

#Region "COM Registration"

        ' Registers this class as an AddIn for Inventor.
        ' This function is called when the assembly is registered for COM.
        <ComRegisterFunctionAttribute()> _
        Public Shared Sub Register(ByVal t As Type)

            Dim clssRoot As RegistryKey = Registry.ClassesRoot
            Dim clsid As RegistryKey = Nothing
            Dim subKey As RegistryKey = Nothing

            Try
                clsid = clssRoot.CreateSubKey("CLSID\" + AddInGuid(t))
                clsid.SetValue(Nothing, "GraphicManagerTest")
                subKey = clsid.CreateSubKey("Implemented Categories\{39AD2B5C-7A29-11D6-8E0A-0010B541CAA8}")
                subKey.Close()

                subKey = clsid.CreateSubKey("Settings")
                subKey.SetValue("AddInType", "Standard")
                subKey.SetValue("LoadOnStartUp", "1")

                'subKey.SetValue("SupportedSoftwareVersionLessThan", "")
                subKey.SetValue("SupportedSoftwareVersionGreaterThan", "11..")
                'subKey.SetValue("SupportedSoftwareVersionEqualTo", "")
                'subKey.SetValue("SupportedSoftwareVersionNotEqualTo", "")
                'subKey.SetValue("Hidden", "0")
                'subKey.SetValue("UserUnloadable", "1")
                subKey.SetValue("Version", 0)
                subKey.Close()

                subKey = clsid.CreateSubKey("Description")
                subKey.SetValue(Nothing, "GraphicManagerTest")

            Catch ex As Exception
                System.Diagnostics.Trace.Assert(False)
            Finally
                If Not subKey Is Nothing Then subKey.Close()
                If Not clsid Is Nothing Then clsid.Close()
                If Not clssRoot Is Nothing Then clssRoot.Close()
            End Try

        End Sub

        ' Unregisters this class as an AddIn for Inventor.
        ' This function is called when the assembly is unregistered.
        <ComUnregisterFunctionAttribute()> _
        Public Shared Sub Unregister(ByVal t As Type)

            Dim clssRoot As RegistryKey = Registry.ClassesRoot
            Dim clsid As RegistryKey = Nothing

            Try
                clssRoot = Microsoft.Win32.Registry.ClassesRoot
                clsid = clssRoot.OpenSubKey("CLSID\" + AddInGuid(t), True)
                clsid.SetValue(Nothing, "")
                clsid.DeleteSubKeyTree("Implemented Categories\{39AD2B5C-7A29-11D6-8E0A-0010B541CAA8}")
                clsid.DeleteSubKeyTree("Settings")
                clsid.DeleteSubKeyTree("Description")
            Catch
            Finally
                If Not clsid Is Nothing Then clsid.Close()
                If Not clssRoot Is Nothing Then clssRoot.Close()
            End Try

        End Sub

        ' This property uses reflection to get the value for the GuidAttribute attached to the class.
        Public Shared ReadOnly Property AddInGuid(ByVal t As Type) As String
            Get
                Dim guid As String = ""
                Try
                    Dim customAttributes() As Object = t.GetCustomAttributes(GetType(GuidAttribute), False)
                    Dim guidAttribute As GuidAttribute = CType(customAttributes(0), GuidAttribute)
                    guid = "{" + guidAttribute.Value.ToString() + "}"
                Finally
                    AddInGuid = guid
                End Try
            End Get
        End Property

#End Region

    End Class

End Namespace

