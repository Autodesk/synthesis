Imports Inventor

Imports System.Drawing
Imports System.Runtime.InteropServices
Imports Microsoft.Win32
Imports System.Windows.Forms
Imports stdole


Namespace VBNetRibbonDemo
    <ProgIdAttribute("VBNetRibbonDemo.StandardAddInServer"), _
    GuidAttribute("af480230-9c8c-435e-a827-6f520e11b97b")> _
    Public Class StandardAddInServer
        Implements Inventor.ApplicationAddInServer

        ' Inventor application object.
        Private m_inventorApplication As Inventor.Application

        Private mAsmButtonDef As ButtonDefinition
        Private mPartButtonDef As ButtonDefinition

        Private Const strAddInGuid As String = "af480230-9c8c-435e-a827-6f520e11b97b"

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

            Dim oStream1 As System.IO.Stream = Me.GetType().Assembly.GetManifestResourceStream("VBNetRibbonDemo.Tools.ico")
            Dim oIcon1 As Icon = New Icon(oStream1)
            Dim oIPictureDisp1 As Object = AxHostConverter.ImageToPictureDisp(oIcon1.ToBitmap())

            mAsmButtonDef = oCtrlDefs.AddButtonDefinition("Assembly Cmd", _
                                                          "Autodesk:RibbonVBTest:Button1", _
                                                          CommandTypesEnum.kQueryOnlyCmdType, _
                                                          strAddInGuid, _
                                                          "Description", _
                                                          "Tolltip text", _
                                                          oIPictureDisp1, _
                                                          Nothing, _
                                                          ButtonDisplayEnum.kDisplayTextInLearningMode)

            mPartButtonDef = oCtrlDefs.AddButtonDefinition("Part Cmd", _
                                                          "Autodesk:RibbonVBTest:Button2", _
                                                          CommandTypesEnum.kQueryOnlyCmdType, _
                                                          strAddInGuid, _
                                                          "Description", _
                                                          "Tolltip text", _
                                                          oIPictureDisp1, _
                                                          Nothing, _
                                                          ButtonDisplayEnum.kDisplayTextInLearningMode)


            If firstTime Then

                Dim UIManager As UserInterfaceManager = m_inventorApplication.UserInterfaceManager

                If m_inventorApplication.UserInterfaceManager.InterfaceStyle = InterfaceStyleEnum.kRibbonInterface Then

                    ' Get the assembly ribbon.
                    Dim assemblyRibbon As Inventor.Ribbon = UIManager.Ribbons.Item("Assembly")

                    ' Get the "Assemble" tab.
                    Dim assemblyTab As Inventor.RibbonTab = assemblyRibbon.RibbonTabs.Item("id_TabAssemble")

                    ' Create a new panel on the Assemble tab.
                    Dim panel1 As Inventor.RibbonPanel = assemblyTab.RibbonPanels.Add("Custom commands", _
                                                                                     "Autodes:RibbonVBTest:Panel1", _
                                                                                     strAddInGuid)
                    panel1.CommandControls.AddButton(mAsmButtonDef, True)



                    ' Get the part ribbon.
                    Dim partRibbon As Inventor.Ribbon = UIManager.Ribbons.Item("Part")

                    ' Get the "Model" tab.
                    Dim modelTab As Inventor.RibbonTab = partRibbon.RibbonTabs.Item("id_TabModel")

                    ' Create a new panel on the model tab.
                    Dim panel2 As Inventor.RibbonPanel = modelTab.RibbonPanels.Add("Custom commands", _
                                                                                     "Autodes:RibbonVBTest:Panel2", _
                                                                                     strAddInGuid)
                    panel2.CommandControls.AddButton(mPartButtonDef, True)

                Else

                    Dim oAsmCommandBar As CommandBar = UIManager.CommandBars("AMxAssemblyPanelCmdBar")
                    oAsmCommandBar.Controls.AddButton(mAsmButtonDef, 0)

                    Dim oPartCommandBar As CommandBar = UIManager.CommandBars("PMxPartFeatureCmdBar")
                    oPartCommandBar.Controls.AddButton(mPartButtonDef, 0)

                End If

            End If

            AddHandler mAsmButtonDef.OnExecute, AddressOf Me.mAsmButtonDef_OnExecute
            AddHandler mPartButtonDef.OnExecute, AddressOf Me.mPartButtonDef_OnExecute

        End Sub

        '//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        '// Use: Assembly Button Handler
        '//
        '//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        Private Sub mAsmButtonDef_OnExecute(ByVal Context As Inventor.NameValueMap)

            System.Windows.Forms.MessageBox.Show("Assembly button was clicked!!", "Assembly Button")

        End Sub

        '//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        '// Use: Part Button Handler
        '//
        '//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        Private Sub mPartButtonDef_OnExecute(ByVal Context As Inventor.NameValueMap)

            System.Windows.Forms.MessageBox.Show("Part button was clicked!!", "Part Button")

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

    End Class

End Namespace

