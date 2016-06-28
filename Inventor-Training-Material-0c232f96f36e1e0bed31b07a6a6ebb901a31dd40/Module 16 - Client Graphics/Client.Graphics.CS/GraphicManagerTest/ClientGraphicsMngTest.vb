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
Imports System.Collections.Generic

Imports Inventor

Imports Autodesk.ADN.Utility.Graphics
Imports Autodesk.ADN.Utility.Interaction
Imports Autodesk.ADN.Utility.InventorUtils

'//////////////////////////////////////////////////////////////////////////////////////////////
'// Description: Illustrates the use of the C# ClientGraphicsManager library
'//
'//              This sample further illustrates:
'//                  - advanced use of Interaction Graphics
'//                  - how to manipulate mouse events with Interaction Graphics
'//                  - how to handle interactive manipulators during interaction
'//
'//              Once the interaction is starting, the user will be prompted to select the
'//              position of the three vertices of a graphics triangle. The user will then be
'//              able to interactively select and drag each vertex. 
'//              The graphics will be updated dynamically.
'//
'//////////////////////////////////////////////////////////////////////////////////////////////
Public Class ClientGraphicsMngTest

    '//////////////////////////////////////////////////////////////////////////////////////////////
    '// Class member to hold our data
    '// 
    '//////////////////////////////////////////////////////////////////////////////////////////////
    Private _clientGraphicsMng As AdnClientGraphicsManager

    Private _interactionManager As AdnInteractionManager

    Private _lineGraph As LineGraphics
    Private _triGraph As TriangleGraphics
    Private _selectedNode As GraphicsNode
    Private _vertices As List(Of SurfaceGraphics)

    Private _graphicMode As GraphicModeEnum

    Private _Tg As TransientGeometry
    Private _TBrep As TransientBRep

    '//////////////////////////////////////////////////////////////////////////////////////////////
    '// An enum that will help us to keep track in which state we are for drawing our 
    '// client graphics
    '//////////////////////////////////////////////////////////////////////////////////////////////
    Private Enum GraphicModeEnum

        kFirstVertex
        kSecondVertex
        kThirdVertex
        kVertexMove
        kIdle

    End Enum

    '//////////////////////////////////////////////////////////////////////////////////////////////
    '// The Demo command is starting the Interaction, this is the only public method of that class
    '//              
    '//////////////////////////////////////////////////////////////////////////////////////////////
    Public Shared Sub Demo()

        Dim instance As New ClientGraphicsMngTest
        instance.DoDemo()

    End Sub

    '//////////////////////////////////////////////////////////////////////////////////////////////
    '// The DoDemo method is doing the real job of starting the command
    '//              
    '//////////////////////////////////////////////////////////////////////////////////////////////
    Private Sub DoDemo()

        Dim InvApp As Inventor.Application = AdnInventorUtilities.InvApplication

        _Tg = InvApp.TransientGeometry
        _TBrep = InvApp.TransientBRep

        _interactionManager = New AdnInteractionManager(InvApp)

        _interactionManager.Initialize()

        _interactionManager.SelectEvents.SingleSelectEnabled = True

        _interactionManager.MouseEvents.MouseMoveEnabled = True

        AddHandler _interactionManager.OnTerminateEvent, _
           AddressOf OnTerminate

        AddHandler _interactionManager.MouseEvents.OnMouseDown, _
            AddressOf MouseEvents_OnMouseDown

        AddHandler _interactionManager.MouseEvents.OnMouseMove, _
            AddressOf MouseEvents_OnMouseMove

        _graphicMode = GraphicModeEnum.kFirstVertex

        _interactionManager.Start("Select first vertex: ")

        _clientGraphicsMng = New AdnClientGraphicsManager( _
            AdnInventorUtilities.InvApplication, _
            AdnInventorUtilities.AddInGuid)

        _clientGraphicsMng.SetGraphicsSource( _
            _interactionManager.InteractionEvents)

        ' This is required to enable selection
        _clientGraphicsMng.InteractionGraphicsMode = _
            AdnInteractionGraphicsModeEnum.kPreviewGraphics

        _clientGraphicsMng.WorkingGraphics.ClientGraphics.Selectable = _
            GraphicsSelectabilityEnum.kAllGraphicsSelectable

        _vertices = New List(Of SurfaceGraphics)

        AddVertex(_Tg.CreatePoint(0, 0, 0))

    End Sub

    '//////////////////////////////////////////////////////////////////////////////////////////////
    '// Add a new vertex, represented by a sphere SurfaceGraphics
    '//              
    '//////////////////////////////////////////////////////////////////////////////////////////////
    Private Sub AddVertex(ByVal position As Point)

        Dim node As GraphicsNode = _
            _clientGraphicsMng.CreateNewGraphicsNode(_vertices.Count + 1)

        Dim vertex As SurfaceGraphics = _clientGraphicsMng.DrawSurface( _
          _TBrep.CreateSolidSphere(position, 8.0), _
          node)

        vertex.SetTransformBehavior( _
            position, _
            DisplayTransformBehaviorEnum.kPixelScaling, _
            1.0)

        vertex.Parent.Selectable = True

        _vertices.Add(vertex)

    End Sub

    '//////////////////////////////////////////////////////////////////////////////////////////////
    '// On MouseDown event, we either create a new vertex or start select events if we selected
    '// all 3 vertices
    '//
    '//////////////////////////////////////////////////////////////////////////////////////////////
    Private Sub MouseEvents_OnMouseDown( _
        ByVal Button As MouseButtonEnum, _
        ByVal ShiftKeys As ShiftStateEnum, _
        ByVal ModelPosition As Inventor.Point, _
        ByVal ViewPosition As Point2d, _
        ByVal View As Inventor.View)

        'Only interested in left mouse button
        If (Button <> MouseButtonEnum.kLeftMouseButton) Then
            Return
        End If


        Select Case _graphicMode

            Case GraphicModeEnum.kFirstVertex

                _lineGraph = _clientGraphicsMng.DrawLine( _
                   AdnInventorUtilities.ToArray(ModelPosition), _
                   AdnInventorUtilities.ToArray(ModelPosition), _
                   Nothing)

                AddVertex(_Tg.CreatePoint(0, 0, 0))

                _interactionManager.InteractionEvents.StatusBarText = "Select second vertex: "

                _graphicMode = GraphicModeEnum.kSecondVertex

            Case GraphicModeEnum.kSecondVertex

                Dim coordSet As GraphicsCoordinateSet = _
                   _lineGraph.CoordinateSet

                coordSet.Add(3, ModelPosition)

                Dim clrSet As GraphicsColorSet = _
                   _clientGraphicsMng.WorkingGraphics.GraphicsDataSets.CreateColorSet( _
                       _clientGraphicsMng.WorkingGraphics.GetDataSetFreeId)

                clrSet.Add(1, 221, 0, 0)
                clrSet.Add(2, 255, 170, 0)
                clrSet.Add(3, 17, 136, 136)

                _triGraph = _clientGraphicsMng.DrawTriangle( _
                    Nothing, Nothing, Nothing, _
                    _lineGraph.Parent)

                _triGraph.CoordinateSet = coordSet

                _triGraph.ColorBinding = ColorBindingEnum.kPerVertexColors
                _triGraph.ColorSet = clrSet


                AddVertex(_Tg.CreatePoint(0, 0, 0))

                _interactionManager.InteractionEvents.StatusBarText = "Select third vertex: "

                _graphicMode = GraphicModeEnum.kThirdVertex

            Case GraphicModeEnum.kThirdVertex

                _interactionManager.InteractionEvents.StatusBarText = "Select any vertex to move it: "

                AddHandler _interactionManager.MouseEvents.OnMouseUp, _
                    AddressOf MouseEvents_OnMouseUp

                AddHandler _interactionManager.SelectEvents.OnSelect, _
                    AddressOf SelectEvents_OnSelect

                _graphicMode = GraphicModeEnum.kIdle

            Case Else

        End Select

    End Sub

    '//////////////////////////////////////////////////////////////////////////////////////////////
    '// On MouseUp event is used to detect when user has finished dragging a vertex
    '//              
    '//////////////////////////////////////////////////////////////////////////////////////////////
    Private Sub MouseEvents_OnMouseUp( _
        ByVal Button As MouseButtonEnum, _
        ByVal ShiftKeys As ShiftStateEnum, _
        ByVal ModelPosition As Inventor.Point, _
        ByVal ViewPosition As Point2d, _
        ByVal View As Inventor.View)

        'Only interested in left mouse button
        If (Button <> MouseButtonEnum.kLeftMouseButton) Then
            Return
        End If

        _graphicMode = GraphicModeEnum.kIdle

    End Sub

    '//////////////////////////////////////////////////////////////////////////////////////////////
    '// Moves a vertex either during creation or during user drag
    '//              
    '//////////////////////////////////////////////////////////////////////////////////////////////
    Private Sub MoveVertex(ByVal vertex As SurfaceGraphics, _
                           ByVal ModelPosition As Inventor.Point)

        Dim node As GraphicsNode = vertex.Parent
        Dim pos As Matrix = node.Transformation

        pos.SetTranslation(_Tg.CreateVector( _
            ModelPosition.X, _
            ModelPosition.Y, _
            ModelPosition.Z))

        node.Transformation = pos

    End Sub

    '//////////////////////////////////////////////////////////////////////////////////////////////
    '// On MouseMove event is used to drag vertices when user moves mouse
    '//              
    '//////////////////////////////////////////////////////////////////////////////////////////////
    Private Sub MouseEvents_OnMouseMove( _
        ByVal Button As MouseButtonEnum, _
        ByVal ShiftKeys As ShiftStateEnum, _
        ByVal ModelPosition As Inventor.Point, _
        ByVal ViewPosition As Point2d, _
        ByVal View As Inventor.View)

        Select Case _graphicMode

            Case GraphicModeEnum.kFirstVertex

                MoveVertex(_vertices(_vertices.Count - 1), ModelPosition)

            Case GraphicModeEnum.kSecondVertex

                MoveVertex(_vertices(_vertices.Count - 1), ModelPosition)

                _lineGraph.CoordinateSet(2) = ModelPosition

            Case GraphicModeEnum.kThirdVertex

                _lineGraph.Delete()

                MoveVertex(_vertices(_vertices.Count - 1), ModelPosition)

                _triGraph.CoordinateSet(3) = ModelPosition

            Case GraphicModeEnum.kVertexMove

                MoveVertex(_selectedNode(1), ModelPosition)

                _triGraph.CoordinateSet(_selectedNode.Id) = ModelPosition

            Case Else

        End Select

        _clientGraphicsMng.UpdateView()

    End Sub

    '//////////////////////////////////////////////////////////////////////////////////////////////
    '// OnSelect events is used to detect a GraphicNode has been selected, that means a 
    '//   vertex is going to be dragged, until user release the mouse button (MouseUp event)           
    '//////////////////////////////////////////////////////////////////////////////////////////////
    Private Sub SelectEvents_OnSelect( _
       ByVal JustSelectedEntities As Inventor.ObjectsEnumerator, _
       ByVal SelectionDevice As Inventor.SelectionDeviceEnum, _
       ByVal ModelPosition As Inventor.Point, _
       ByVal ViewPosition As Inventor.Point2d, _
       ByVal View As Inventor.View)

        _selectedNode = JustSelectedEntities(1)

        If (Not _selectedNode Is Nothing) Then
            _graphicMode = GraphicModeEnum.kVertexMove
        End If

    End Sub

    '//////////////////////////////////////////////////////////////////////////////////////////////
    '// Final update of view upon terminate interaction events
    '//              
    '//////////////////////////////////////////////////////////////////////////////////////////////
    Private Sub OnTerminate()

        _clientGraphicsMng.UpdateView()

    End Sub

End Class
