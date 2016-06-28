'**********************************
'Translated from sample of API help reference
'***********************************


Imports Inventor


Public Class clsDragComponent

    Private ThisApplication As Inventor.Application
    Private WithEvents mUserInputEvents As UserInputEvents
    Private WithEvents mInteractionEvents As InteractionEvents
    Private WithEvents mMouseEvents As MouseEvents

    Private mInteractionGraphics As InteractionGraphics
    Private mOccurrence As ComponentOccurrence
    Private mPreviousPos As Point
    Private mTransfo As Matrix

    Public Sub Initialize(ByVal val As Inventor.Application)
        ThisApplication = val

        Dim oAsm As AssemblyDocument
        oAsm = ThisApplication.ActiveDocument

        mUserInputEvents = ThisApplication.CommandManager.UserInputEvents
        mInteractionEvents = ThisApplication.CommandManager.CreateInteractionEvents
        mMouseEvents = mInteractionEvents.MouseEvents

        mMouseEvents.MouseMoveEnabled = True

        mInteractionGraphics = mInteractionEvents.InteractionGraphics

    End Sub



    Private Sub mUserInputEvents_OnDrag(ByVal DragState As Inventor.DragStateEnum, ByVal ShiftKeys As Inventor.ShiftStateEnum, ByVal ModelPosition As Inventor.Point, ByVal ViewPosition As Inventor.Point2d, ByVal View As Inventor.View, ByVal AdditionalInfo As Inventor.NameValueMap, ByRef HandlingCode As Inventor.HandlingCodeEnum) Handles mUserInputEvents.OnDrag
        Dim oSS As SelectSet
        oSS = ThisApplication.ActiveDocument.SelectSet

        If DragState = DragStateEnum.kDragStateDragHandlerSelection Then

            If oSS.Count = 1 And oSS.Item(1).Type = Inventor.ObjectTypeEnum.kComponentOccurrenceObject Then

                mOccurrence = oSS.Item(1)

                mInteractionEvents = ThisApplication.CommandManager.CreateInteractionEvents
                mMouseEvents = mInteractionEvents.MouseEvents

                mMouseEvents.MouseMoveEnabled = True

                mInteractionGraphics = mInteractionEvents.InteractionGraphics

                HandlingCode = HandlingCodeEnum.kEventCanceled

                mTransfo = mOccurrence.Transformation

                mPreviousPos = ModelPosition

                mInteractionEvents.Start()

                ThisApplication.ActiveView.Update()

            End If
        End If

    End Sub

    Private Sub mMouseEvents_OnMouseMove(ByVal Button As Inventor.MouseButtonEnum, ByVal ShiftKeys As Inventor.ShiftStateEnum, ByVal ModelPosition As Inventor.Point, ByVal ViewPosition As Inventor.Point2d, ByVal View As Inventor.View) Handles mMouseEvents.OnMouseMove
        Dim oTranslation As Vector
        oTranslation = mTransfo.Translation

        oTranslation.AddVector(mPreviousPos.VectorTo(ModelPosition))

        mTransfo.SetTranslation(oTranslation)

        mPreviousPos = ModelPosition

        Dim oBody As SurfaceBody
        oBody = ThisApplication.TransientBRep.Copy(mOccurrence.Definition.SurfaceBodies(1))

        Call ThisApplication.TransientBRep.Transform(oBody, mTransfo)

        mInteractionGraphics.PreviewClientGraphics.Delete()

        Dim oGraphicsNode As GraphicsNode
        oGraphicsNode = mInteractionGraphics.PreviewClientGraphics.AddNode(1)

        Dim oSurfaceGraphics As Inventor.SurfaceGraphics
        oSurfaceGraphics = oGraphicsNode.AddSurfaceGraphics(oBody)

        ThisApplication.ActiveView.Update()
    End Sub

    Private Sub mMouseEvents_OnMouseUp(ByVal Button As Inventor.MouseButtonEnum, ByVal ShiftKeys As Inventor.ShiftStateEnum, ByVal ModelPosition As Inventor.Point, ByVal ViewPosition As Inventor.Point2d, ByVal View As Inventor.View) Handles mMouseEvents.OnMouseUp
        mMouseEvents = Nothing
        mUserInputEvents = Nothing
        mInteractionEvents.Stop()
    End Sub

    Private Sub mInteractionEvents_OnTerminate() Handles mInteractionEvents.OnTerminate
        mMouseEvents = Nothing
        mUserInputEvents = Nothing
        mInteractionEvents = Nothing
    End Sub
End Class
