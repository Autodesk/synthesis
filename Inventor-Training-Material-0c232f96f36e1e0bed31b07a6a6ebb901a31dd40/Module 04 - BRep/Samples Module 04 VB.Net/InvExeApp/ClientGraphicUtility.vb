Imports Inventor

Public Class ClientGraphicUtility

    Public Shared Sub DrawLine(ByVal oDataSets As GraphicsDataSets, ByVal oClientGraphics As ClientGraphics, ByVal oStartPoint As Point, ByVal oEndPoint As Point)

        Dim oCoordSet As GraphicsCoordinateSet
        oCoordSet = oDataSets.CreateCoordinateSet(oDataSets.Count + 1)

        Dim oPointCoords(5) As Double

        oPointCoords(0) = oStartPoint.X
        oPointCoords(1) = oStartPoint.Y
        oPointCoords(2) = oStartPoint.Z

        oPointCoords(3) = oEndPoint.X
        oPointCoords(4) = oEndPoint.Y
        oPointCoords(5) = oEndPoint.Z

        Call oCoordSet.PutCoordinates(oPointCoords)


        Dim oLineNode As GraphicsNode
        oLineNode = oClientGraphics.AddNode(oClientGraphics.Count + 1)

        Dim oLineSet As LineStripGraphics
        oLineSet = oLineNode.AddLineStripGraphics

        oLineSet.LineWeight = 5

        Dim oIndex As GraphicsIndexSet
        oIndex = oDataSets.CreateIndexSet(oDataSets.Count + 1)

        Call oIndex.Add(1, 1)  ' Line from point 1
        Call oIndex.Add(2, 2)  ' to point 2

        Dim oColorSet As GraphicsColorSet
        oColorSet = oDataSets.CreateColorSet(1)
        Call oColorSet.Add(1, 255, 0, 0)
        oLineSet.ColorSet = oColorSet

        oLineSet.CoordinateSet = oCoordSet
        oLineSet.CoordinateIndexSet = oIndex

    End Sub

    Public Shared Sub DrawLineTest(ByVal oStartPoint As Point, ByVal oEndPoint As Point)

        Dim oDoc As Document
        oDoc = _InvApplication.ActiveDocument

        Dim oCompDef As ComponentDefinition
        oCompDef = oDoc.ComponentDefinition

        Dim oDataSets As GraphicsDataSets

        Try
            oDataSets = oDoc.GraphicsDataSetsCollection.Add("TestCG")
        Catch
            oDataSets = oDoc.GraphicsDataSetsCollection("TestCG")
        End Try

        Dim oClientGraphics As ClientGraphics

        Try
            oClientGraphics = oCompDef.ClientGraphicsCollection.Add("TestCG")
        Catch
            Err.Clear()
            oClientGraphics = oCompDef.ClientGraphicsCollection("TestCG")
        End Try

        Call DrawLine(oDataSets, oClientGraphics, oStartPoint, oEndPoint)

    End Sub

    Public Shared Sub DrawLineTestDb(ByVal startPoint() As Double, ByVal endPoint() As Double)

        Dim oTg As TransientGeometry
        oTg = _InvApplication.TransientGeometry

        Dim oStartPoint As Point
        Dim oEndPoint As Point

        oStartPoint = oTg.CreatePoint(startPoint(0), startPoint(1), startPoint(2))
        oEndPoint = oTg.CreatePoint(endPoint(0), endPoint(1), endPoint(2))

        Call DrawLineTest(oStartPoint, oEndPoint)

    End Sub

    Public Shared Sub cleanCGTest()

        Dim oDoc As Document
        oDoc = _InvApplication.ActiveDocument

        Dim oCompDef As ComponentDefinition
        oCompDef = oDoc.ComponentDefinition

        Dim oDataSets As GraphicsDataSets

        Try
            oDataSets = oDoc.GraphicsDataSetsCollection("TestCG")
        Catch
            Exit Sub
        End Try

        Dim oClientGraphics As ClientGraphics

        Try
            oClientGraphics = oCompDef.ClientGraphicsCollection("TestCG")
        Catch
            Exit Sub
        End Try

        oClientGraphics.Delete()
        oDataSets.Delete()
        _InvApplication.ActiveView.Update()

    End Sub

End Class
