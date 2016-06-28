Imports Inventor
Imports System.Drawing


'//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
'// Use: Handles creation of controls for the addin
'//
'//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
Public Class CAddInControls

    Private mApplication As Inventor.Application

    'Holds our ButtonDefinitions
    Private mFileButtonDef As ButtonDefinition
    Private mAsmButtonDef As ButtonDefinition
    Private mPartButtonDef As ButtonDefinition

  
    Public Sub New(ByVal oApplication As Inventor.Application)

        mApplication = oApplication

    End Sub


    Public Sub CreateControls(ByVal firstTime As Boolean, ByVal AddInGuid As String)

        Dim oCtrlDefs As ControlDefinitions = mApplication.CommandManager.ControlDefinitions

        Dim oStream1 As System.IO.Stream = Me.GetType().Assembly.GetManifestResourceStream("AddInVBTest.Tools.ico")
        Dim oIcon1 As Icon = New Icon(oStream1)
        Dim oIPictureDisp1 As Object = Microsoft.VisualBasic.Compatibility.VB6.Support.IconToIPicture(oIcon1)

        mFileButtonDef = oCtrlDefs.AddButtonDefinition("Custom Open", _
                                                      "Autodesk:AddInVBTest:FileButton", _
                                                      CommandTypesEnum.kQueryOnlyCmdType, _
                                                      AddInGuid, _
                                                      "Description", _
                                                      "Tolltip text", _
                                                      Nothing, _
                                                      Nothing, _
                                                      ButtonDisplayEnum.kDisplayTextInLearningMode)

        mAsmButtonDef = oCtrlDefs.AddButtonDefinition("Assembly Cmd", _
                                                      "Autodesk:AddInVBTest:Button1", _
                                                      CommandTypesEnum.kQueryOnlyCmdType, _
                                                      AddInGuid, _
                                                      "Description", _
                                                      "Tolltip text", _
                                                      oIPictureDisp1, _
                                                      Nothing, _
                                                      ButtonDisplayEnum.kDisplayTextInLearningMode)

        mPartButtonDef = oCtrlDefs.AddButtonDefinition("Part Cmd", _
                                                      "Autodesk:AddInVBTest:Button2", _
                                                      CommandTypesEnum.kQueryOnlyCmdType, _
                                                      AddInGuid, _
                                                      "Description", _
                                                      "Tolltip text", _
                                                      oIPictureDisp1, _
                                                      Nothing, _
                                                      ButtonDisplayEnum.kDisplayTextInLearningMode)



        If (firstTime) Then

            If mApplication.UserInterfaceManager.InterfaceStyle = InterfaceStyleEnum.kRibbonInterface Then

                ' Add command to the File controls.
                Dim fileControls As CommandControls = mApplication.UserInterfaceManager.FileBrowserControls
                fileControls.AddButton(mFileButtonDef)

                ' Get the assembly ribbon.
                Dim assemblyRibbon As Inventor.Ribbon = mApplication.UserInterfaceManager.Ribbons.Item("Assembly")

                ' Get the "Assemble" tab.
                Dim assemblyTab As Inventor.RibbonTab = assemblyRibbon.RibbonTabs.Item("id_TabAssemble")

                ' Create a new panel on the Assemble tab.
                Dim panel As Inventor.RibbonPanel = assemblyTab.RibbonPanels.Add("Custom commands", "Autodes:AddInVBTest:Panel1", AddInGuid)
                panel.CommandControls.AddButton(mAsmButtonDef, True)

                'Part Ribbon
                panel = mApplication.UserInterfaceManager.Ribbons.Item("Part").RibbonTabs.Item("id_TabModel").RibbonPanels.Add("Custom commands", "Autodes:AddInVBTest:Panel2", AddInGuid)
                panel.CommandControls.AddButton(mPartButtonDef, True)

            Else

                'Dim oCommandBar As CommandBar = mApplication.UserInterfaceManager.CommandBars.Add("Custom Bar", "intNameCustomBarV1.0", CommandBarTypeEnum.kRegularCommandBar, "423e5757-c751-4f11-aa4e-50ab95239206")
                'oCommandBar.Controls.AddButton(mButtonDef, 0);

                Dim oAsmCommandBar As CommandBar = mApplication.UserInterfaceManager.CommandBars("AMxAssemblyPanelCmdBar")
                oAsmCommandBar.Controls.AddButton(mAsmButtonDef, 0)

                Dim oPartCommandBar As CommandBar = mApplication.UserInterfaceManager.CommandBars("PMxPartFeatureCmdBar")
                oPartCommandBar.Controls.AddButton(mPartButtonDef, 0)

            End If

        End If

            AddHandler mFileButtonDef.OnExecute, AddressOf Me.mFileButtonDef_OnExecute
            AddHandler mAsmButtonDef.OnExecute, AddressOf Me.mAsmButtonDef_OnExecute
            AddHandler mPartButtonDef.OnExecute, AddressOf Me.mPartButtonDef_OnExecute

    End Sub


    '//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use: Creates a custom File Open dialog 
    '//
    '//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Private Sub mFileButtonDef_OnExecute(ByVal Context As Inventor.NameValueMap)

        ' Create a new FileDialog object.
        Dim oFileDlg As FileDialog = Nothing
        mApplication.CreateFileDialog(oFileDlg)

        ' Define the filter to select part and assembly files or any file.
        oFileDlg.Filter = "Excel Files (*.xls)|*.xls"

        ' Define the part and assembly files filter to be the default filter.
        oFileDlg.FilterIndex = 1

        ' Set the title for the dialog.
        oFileDlg.DialogTitle = "Select Excel File"

        ' Set the initial directory that will be displayed in the dialog.
        oFileDlg.InitialDirectory = "C:\Temp"

        ' Set the flag so an error will be raised if the user clicks the Cancel button.
        oFileDlg.CancelError = True

        Try
            ' Show the open dialog.
            oFileDlg.ShowOpen()
            MsgBox("File Selected: " & oFileDlg.FileName)

        Catch ex As Exception

            MsgBox("User cancelled out of dialog")

        End Try

    End Sub

    '//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use: Displays assembly form
    '//
    '//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Private Sub mAsmButtonDef_OnExecute(ByVal Context As Inventor.NameValueMap)

    End Sub

    '//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use: Executes our ChangeProcessor test
    '//
    '//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Private Sub mPartButtonDef_OnExecute(ByVal Context As Inventor.NameValueMap)

    End Sub


End Class
