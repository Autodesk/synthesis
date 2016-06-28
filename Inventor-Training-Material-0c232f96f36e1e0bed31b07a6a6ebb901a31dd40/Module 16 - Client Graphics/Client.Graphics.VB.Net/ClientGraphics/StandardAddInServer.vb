Imports Inventor
Imports System.Runtime.InteropServices
Imports Microsoft.Win32

Namespace ClientGraphics
    <ProgIdAttribute("ClientGraphics.StandardAddInServer"), _
    GuidAttribute("5f2b51ca-9aa8-4e7b-9d95-c4d9c1834cac")> _
    Public Class StandardAddInServer
        Implements Inventor.ApplicationAddInServer

        ' Inventor application object.
        Private m_inventorApplication As Inventor.Application


        ' GUID of the AddIn
        Private m_ClientId As String

        ' Variables for Screenshot
        'button defintion of Client Graphics
        Private WithEvents oCGButtonDef As ButtonDefinition
     


#Region "ApplicationAddInServer Members"

        Public Sub Activate(ByVal addInSiteObject As Inventor.ApplicationAddInSite, ByVal firstTime As Boolean) Implements Inventor.ApplicationAddInServer.Activate

            ' This method is called by Inventor when it loads the AddIn.
            ' The AddInSiteObject provides access to the Inventor Application object.
            ' The FirstTime flag indicates if the AddIn is loaded for the first time.

            ' Initialize AddIn members.
            m_inventorApplication = addInSiteObject.Application

            ' TODO:  Add ApplicationAddInServer.Activate implementation.
            ' e.g. event initialization, command creation etc.

            m_ClientId = "{5f2b51ca-9aa8-4e7b-9d95-c4d9c1834cac}"

            'icons for buttons
            Dim largeIconSize As Integer = 32

            Dim smallPicture1 As stdole.IPictureDisp = _
        Microsoft.VisualBasic.Compatibility.VB6.IconToIPicture( _
          New System.Drawing.Icon(My.Resources.CGIcon, 16, 16))
            Dim largePicture1 As stdole.IPictureDisp = _
              Microsoft.VisualBasic.Compatibility.VB6.IconToIPicture( _
                New System.Drawing.Icon( _
                  My.Resources.CGIcon, _
                  largeIconSize, _
                  largeIconSize))


            Dim controlDefs As ControlDefinitions = m_inventorApplication.CommandManager.ControlDefinitions

            'Screenshot button
            oCGButtonDef = _
              controlDefs.AddButtonDefinition( _
                "Client Graphics", "ClientGraphics:CGDef", _
                CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, , "Client Graphics", _
                smallPicture1, largePicture1)

            If m_inventorApplication.UserInterfaceManager.InterfaceStyle = InterfaceStyleEnum.kRibbonInterface Then  'add Ribbon UI
                AddRibbonUI()
            Else  'Add Classic UI

                AddClassicUI()
            End If



        End Sub

        Public Sub Deactivate() Implements Inventor.ApplicationAddInServer.Deactivate

            ' This method is called by Inventor when the AddIn is unloaded.
            ' The AddIn will be unloaded either manually by the user or
            ' when the Inventor session is terminated.

            ' TODO:  Add ApplicationAddInServer.Deactivate implementation

            ' Release objects.
            Marshal.ReleaseComObject(m_inventorApplication)
            m_inventorApplication = Nothing

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

        Private Sub AddRibbonUI()
            Dim ribNames() As String = _
              {"Drawing", "Part", "Assembly", "Presentation"}

            For Each ribName In ribNames
                Dim oRibbon As Object = _
                  m_inventorApplication.UserInterfaceManager.Ribbons(ribName)
                Dim oTab As Object = oRibbon.RibbonTabs("id_TabTools")
                Dim oCGPanel As Object

                Try
                    oCGPanel = _
                      oTab.RibbonPanels("ClientGraphics:RibbonPanel")
                Catch ex As Exception
                    oCGPanel = _
                      oTab.RibbonPanels.Add( _
                        "ClientGraphics", "ClientGraphics:RibbonPanel", m_ClientId)
                End Try
                oCGPanel.CommandControls.AddButton(oCGButtonDef, True)
            Next
        End Sub

        Private Sub AddClassicUI()

            Dim classicUIToolsMenuNames() As String = _
              {"PartToolsMenu", "AssemblyToolsMenu", "DrawingMangerToolsMenu", _
               "PresentationToolsMenu"}

            For Each classicUIToolsMenuName In classicUIToolsMenuNames

                Try
                    Dim oToolsMenu As Object = _
                    m_inventorApplication.UserInterfaceManager.CommandBars( _
                      classicUIToolsMenuName)

                    If Not oToolsMenu Is Nothing Then
                        'delete if exists
                        Dim oCommandBarCtrl As Object
                        For Each oCommandBarCtrl In oToolsMenu.Controls
                            Dim oCtrolDef As Object
                            oCtrolDef = oCommandBarCtrl.ControlDefinition
                            If Not oCtrolDef Is Nothing Then
                                If oCtrolDef.InternalName = "ClientGraphics:CGDef" Then
                                    oCommandBarCtrl.Delete()
                                End If
                            End If
                        Next

                        'add the button
                        oToolsMenu.Controls.AddButton(oCGButtonDef)
                    End If

                Catch ex As Exception
                End Try
            Next
        End Sub

#End Region
         

        Private Sub oCGButtonDef_OnExecute(ByVal Context As Inventor.NameValueMap) Handles oCGButtonDef.OnExecute

            'dialog of Client Graphics 
            Dim oCGFDlg As CGDialog = New CGDialog()
            oCGFDlg.invApp = m_inventorApplication
            
            oCGFDlg.ShowDialog()
        End Sub
    End Class

End Namespace

