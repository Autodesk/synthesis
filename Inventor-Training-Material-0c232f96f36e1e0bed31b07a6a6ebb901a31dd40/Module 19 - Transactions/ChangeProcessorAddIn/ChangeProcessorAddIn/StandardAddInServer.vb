Imports Inventor
Imports System.Runtime.InteropServices
Imports Microsoft.Win32

Namespace ChangeProcessorAddIn
    <ProgIdAttribute("ChangeProcessorAddIn.StandardAddInServer"), _
    GuidAttribute("287e5b24-75d2-4774-aff6-340b6b650ad9")> _
    Public Class StandardAddInServer
        Implements Inventor.ApplicationAddInServer

        ' Inventor application object.
        Private m_inventorApplication As Inventor.Application


        Private addin_guid As String = "{287e5b24-75d2-4774-aff6-340b6b650ad9}"
        Private ChangeDefName As String = "CreateLine"
        ' Member variable for the button.
        Private WithEvents m_oButtonDef1 As ButtonDefinition


        ' Declarations for the change processor.
        Private WithEvents m_oChangeDefinition As ChangeDefinition
        Private m_oChangeManager As ChangeManager
        Private WithEvents m_oMyCommandProcessor As ChangeProcessor



#Region "ApplicationAddInServer Members"

        Public Sub Activate(ByVal addInSiteObject As Inventor.ApplicationAddInSite, ByVal firstTime As Boolean) Implements Inventor.ApplicationAddInServer.Activate

            ' This method is called by Inventor when it loads the AddIn.
            ' The AddInSiteObject provides access to the Inventor Application object.
            ' The FirstTime flag indicates if the AddIn is loaded for the first time.

            ' Initialize AddIn members.
            m_inventorApplication = addInSiteObject.Application

            ' TODO:  Add ApplicationAddInServer.Activate implementation.
            ' e.g. event initialization, command creation etc.

            '' add a button to part ribbon >> sketch tab >> first panel 
            '****
            m_oButtonDef1 = m_inventorApplication.CommandManager.ControlDefinitions.AddButtonDefinition("Create My Line", _
           "TestTriangleButton", CommandTypesEnum.kNonShapeEditCmdType, addin_guid, _
           "Test of button 1", "Test Button 1")


            ' Add the button to the first panel of part sketch
            Dim oSketchTab = m_inventorApplication.UserInterfaceManager.Ribbons("Part").RibbonTabs("id_TabSketch")
            Dim oFirstPanel = oSketchTab.RibbonPanels(1)

            oFirstPanel.CommandControls.AddButton(m_oButtonDef1)
            '******


            ' Create the ChangeDefinitions collection for this Add-In.

            m_oChangeManager = m_inventorApplication.ChangeManager
            Dim oChangeDefinitions As ChangeDefinitions
            oChangeDefinitions = m_oChangeManager.Add(addin_guid)

            ' Create a ChangeDefinition for the command.
            m_oChangeDefinition = oChangeDefinitions.Add(ChangeDefName, "Create My Line")
             

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


        ' button execute event
        Private Sub m_oButtonDef1_OnExecute(Context As Inventor.NameValueMap) Handles m_oButtonDef1.OnExecute

            If Not m_oChangeDefinition Is Nothing Then
                ' use the ChangeDefinition we have defined
                'execute the operations 
               
                Dim colChangeDefs As ChangeDefinitions = m_oChangeManager(addin_guid)
                Dim mobjChangeDef As ChangeDefinition = colChangeDefs(ChangeDefName)
                m_oMyCommandProcessor = mobjChangeDef.CreateChangeProcessor 

                m_oMyCommandProcessor.Execute(m_inventorApplication.ActiveDocument)
            Else
                MsgBox("change definition is null!")
            End If
        End Sub

        ' Event that's called to execute the change.  This is where the actual work
        ' within Inventor is done.  Anything done within this event is automatically
        ' wrapped in a transaction.   
        Private Sub m_oMyCommandProcessor_OnExecute(Document As Inventor._Document, Context As Inventor.NameValueMap, ByRef Succeeded As Boolean) Handles m_oMyCommandProcessor.OnExecute

            ' add some geometries to a sketch
            Dim objPartDoc As PartDocument
            objPartDoc = Document

            Dim objPartCompDef As PartComponentDefinition
            objPartCompDef = objPartDoc.ComponentDefinition

            Dim objTransGeom As TransientGeometry
            objTransGeom = objPartDoc.Parent.TransientGeometry

            Dim colLine As SketchLine
            colLine = objPartCompDef.Sketches(1).SketchLines.AddByTwoPoints _
                (objTransGeom.CreatePoint2d(0, 0), objTransGeom.CreatePoint2d(4, 0))


        End Sub


        ' Member variables used to store the various inputs of the command.
        Private m_dSize As Double

        ' This event is fired by Inventor when a transcript is being replayed.  
        ' Inventor provides the original input that was used.  The global variables 
        ' containing the command input should be initialized here.

        Private Sub m_oMyCommandProcessor_OnWriteToScript(Document As Inventor._Document,
                                                          Context As Inventor.NameValueMap,
                                                          ByRef ResultInputs As String) Handles m_oMyCommandProcessor.OnWriteToScript

            ' Set the return string to contain the triangle size.
            ResultInputs = Format(m_dSize, "0.00000000")

        End Sub
        'This event is fired by Inventor when the change processor is running.  You provide
        ' a string that encapsulates all of the input required by your command.  If the 
        ' transcript is played back this string is passed back to you through the
        ' OnReadFromScript event.

        Private Sub m_oMyCommandProcessor_OnReadFromScript(Document As Inventor._Document,
                                                           Inputs As String,
                                                           Context As Inventor.NameValueMap) Handles m_oMyCommandProcessor.OnReadFromScript
           
            m_dSize = Val(Inputs)
        End Sub



  

        'Event that 's called by Inventor when a transcript containing 
        ' calls to this change processor is being played back.

        Private Sub m_oChangeDefinition_OnReplay(Context As Inventor.NameValueMap, ByRef ResultProcessor As Inventor.ChangeProcessor) Handles m_oChangeDefinition.OnReplay
            ' Create a new change processor.
            m_oMyCommandProcessor = m_oChangeDefinition.CreateChangeProcessor

            ' Set the return argument.
            ResultProcessor = m_oMyCommandProcessor

          

        End Sub
    End Class

End Namespace

