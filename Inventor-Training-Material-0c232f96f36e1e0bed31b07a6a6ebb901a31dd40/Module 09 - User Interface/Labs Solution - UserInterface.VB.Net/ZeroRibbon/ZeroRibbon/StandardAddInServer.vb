Imports Inventor
Imports System.Runtime.InteropServices
Imports Microsoft.Win32
Imports System.Drawing
Imports System.Windows.Forms
Imports stdole

Namespace ZeroRibbon
    <ProgIdAttribute("ZeroRibbon.StandardAddInServer"), _
    GuidAttribute("c7261520-f76a-4808-ab1d-9c4a23a31d68")> _
    Public Class StandardAddInServer
        Implements Inventor.ApplicationAddInServer

        ' Inventor application object.
        Private m_inventorApplication As Inventor.Application

        Private WithEvents Zero_GetStarted_LaunchPanel_ButtonDef As ButtonDefinition
        Private WithEvents Zero_GetStarted_NewPanel_ButtonDef As ButtonDefinition

        Private Const strAddInGuid As String = "c7261520-f76a-4808-ab1d-9c4a23a31d68"

#Region "ApplicationAddInServer Members"

        Public Sub Activate(ByVal addInSiteObject As Inventor.ApplicationAddInSite, ByVal firstTime As Boolean) Implements Inventor.ApplicationAddInServer.Activate

            ' This method is called by Inventor when it loads the AddIn.
            ' The AddInSiteObject provides access to the Inventor Application object.
            ' The FirstTime flag indicates if the AddIn is loaded for the first time.

            ' Initialize AddIn members.
            m_inventorApplication = addInSiteObject.Application

            ' TODO:  Add ApplicationAddInServer.Activate implementation.
            ' e.g. event initialization, command creation etc.


            Dim oCtrlDefs As ControlDefinitions = m_inventorApplication.CommandManager.ControlDefinitions

            Dim oStream1 As System.IO.Stream = Me.GetType().Assembly.GetManifestResourceStream("ZeroRibbon.Tools.ico")

            Dim oIcon1 As Icon = New Icon(oStream1)
            Dim oIPictureDisp1 As Object = AxHostConverter.ImageToPictureDisp(oIcon1.ToBitmap())


            Zero_GetStarted_LaunchPanel_ButtonDef = oCtrlDefs.AddButtonDefinition("My Command1", _
                                                          "Autodesk:ZeroRibbon:Button1", _
                                                          CommandTypesEnum.kQueryOnlyCmdType, _
                                                          strAddInGuid, _
                                                          "Description", _
                                                          "Tolltip text", _
                                                          oIPictureDisp1, _
                                                          Nothing, _
                                                          ButtonDisplayEnum.kDisplayTextInLearningMode)

            Zero_GetStarted_NewPanel_ButtonDef = oCtrlDefs.AddButtonDefinition("My Command2", _
                                                          "Autodesk:ZeroRibbon:Button2", _
                                                          CommandTypesEnum.kQueryOnlyCmdType, _
                                                          strAddInGuid, _
                                                          "Description", _
                                                          "Tolltip text", _
                                                          oIPictureDisp1, _
                                                          Nothing, _
                                                          ButtonDisplayEnum.kDisplayTextInLearningMode)

            Dim UIManager As UserInterfaceManager = m_inventorApplication.UserInterfaceManager


            ' Get the assembly ribbon.
            Dim assemblyRibbon As Inventor.Ribbon = UIManager.Ribbons.Item("ZeroDoc")
          
            ' Get the "Assemble" tab.
            Dim assemblyTab As Inventor.RibbonTab = assemblyRibbon.RibbonTabs.Item("id_GetStarted")

            ' get Launch panel
            Dim built_panel As Inventor.RibbonPanel = assemblyTab.RibbonPanels.Item("id_Panel_Launch")

            ' add my command to this panel
            built_panel.CommandControls.AddButton(Zero_GetStarted_LaunchPanel_ButtonDef, True)

            ' Create a new panel on the Assemble tab.
            Dim panel1 As Inventor.RibbonPanel = assemblyTab.RibbonPanels.Add("My Panel", _
                                                                             "Autodes:ZeroRibbon:Panel1", _
                                                                            strAddInGuid)

            ' add my command to this new panel
            panel1.CommandControls.AddButton(Zero_GetStarted_NewPanel_ButtonDef, True)

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

#End Region

        Private Sub Zero_GetStarted_LaunchPanel_ButtonDef_OnExecute(Context As Inventor.NameValueMap) Handles Zero_GetStarted_LaunchPanel_ButtonDef.OnExecute
            MsgBox("the button in LaunchPanel is clicked!")
        End Sub

        Private Sub Zero_GetStarted_NewPanel_ButtonDef_OnExecute(Context As Inventor.NameValueMap) Handles Zero_GetStarted_NewPanel_ButtonDef.OnExecute
            MsgBox("the button in my panel is clicked!")
        End Sub
    End Class

    ''from http://blogs.msdn.com/b/andreww/archive/2007/07/30/converting-between-ipicturedisp-and-system-drawing-image.aspx

    Friend Class AxHostConverter
        Inherits AxHost
        Private Sub New()
            MyBase.New("")
        End Sub


        Public Shared Function ImageToPictureDisp(image As Image) As stdole.IPictureDisp
            Return DirectCast(GetIPictureDispFromPicture(image), stdole.IPictureDisp)
        End Function


        Public Shared Function PictureDispToImage(pictureDisp As stdole.IPictureDisp) As Image
            Return GetPictureFromIPicture(pictureDisp)
        End Function
    End Class


End Namespace

