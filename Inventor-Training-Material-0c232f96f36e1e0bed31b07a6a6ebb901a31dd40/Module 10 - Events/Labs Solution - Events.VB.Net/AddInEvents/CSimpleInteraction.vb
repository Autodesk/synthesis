
Imports Inventor

'//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
'// Use: Implements a simple user interaction. User picks up an entity and its type will be displayed.
'//
'//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
Public Class CSimpleInteraction

    Private mApplication As Inventor.Application
    Private mInteractionEvents As InteractionEvents
    Private mSelectEvents As SelectEvents

    Public Sub New(ByVal oApplication As Inventor.Application)

        mApplication = oApplication

        'Initialize events
        mInteractionEvents = mApplication.CommandManager.CreateInteractionEvents()
        mSelectEvents = mInteractionEvents.SelectEvents

        'Set event handler VB.Net Style
        AddHandler mSelectEvents.OnSelect, AddressOf Me.mSelectEvents_OnSelect
        AddHandler mInteractionEvents.OnTerminate, AddressOf Me.mInteractionEvents_OnTerminate

        'Clear filter and set new ones if needed
        mSelectEvents.ClearSelectionFilter()

        'Always Disable mouse move if not needed for performances
        mSelectEvents.MouseMoveEnabled = False
        mSelectEvents.Enabled = True
        mSelectEvents.SingleSelectEnabled = True

        'Remember to Start/Stop the interaction event
        mInteractionEvents.Start()

    End Sub

    Public Sub CleanUp()

        'Remove handlers
        RemoveHandler mSelectEvents.OnSelect, AddressOf Me.mSelectEvents_OnSelect
        RemoveHandler mInteractionEvents.OnTerminate, AddressOf Me.mInteractionEvents_OnTerminate

        mSelectEvents = Nothing
        mInteractionEvents = Nothing

    End Sub

    Private Sub mSelectEvents_OnSelect(ByVal JustSelectedEntities As Inventor.ObjectsEnumerator, ByVal SelectionDevice As Inventor.SelectionDeviceEnum, ByVal ModelPosition As Inventor.Point, ByVal ViewPosition As Inventor.Point2d, ByVal View As Inventor.View)

        If JustSelectedEntities.Count <> 0 Then

            Dim selectedObj As Object = JustSelectedEntities(1)
            System.Windows.Forms.MessageBox.Show("Selected Entity: " & TypeName(selectedObj), "Simple Interaction")

        End If

    End Sub

    Private Sub mInteractionEvents_OnTerminate()
        CleanUp()
    End Sub

End Class