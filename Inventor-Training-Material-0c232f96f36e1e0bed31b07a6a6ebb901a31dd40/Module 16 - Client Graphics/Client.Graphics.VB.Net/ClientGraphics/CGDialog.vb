'****************Client Graphics Samples********************
'****
'****    Witten by Xiaodong Liang
'****    Developer Technical Services
'****    Autodesk
'****    09/2010
'*************************************************************

Imports Inventor

'dialog of client graphics
Public Class CGDialog

    'inventor application
    Private m_invApp As Inventor.Application
    'InteractionEvents for Overlay graphics
    Private oIE As InteractionEvents

    'set inventor application
    Public Property invApp() As Inventor.Application
        Get
            Return m_invApp
        End Get
        Set(ByVal value As Inventor.Application)
            m_invApp = value
        End Set
    End Property

    'dialog is loaded
    Private Sub CGDialog_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        CBoxPrimitive.SelectedIndex = 0
    End Sub


    'Fit the view
    Public Sub FitView()

        m_invApp.ActiveView.Update()
        Dim oCamera As Camera = m_invApp.ActiveView.Camera
        oCamera.Fit()
        oCamera.Apply()
        '
    End Sub

#Region "Primitive"

    'when the user changes the selection of primitive
    Private Sub CBoxPrimitive_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CBoxPrimitive.SelectedIndexChanged

        Select Case CBoxPrimitive.SelectedIndex

            Case 1
                DrawPointGraphics()
            Case 2
                DrawLineGraphics()
            Case 3
                DrawLineStripGraphics()
            Case 4
                DrawTriangleGraphics()
            Case 5
                DrawTriangleStripGraphics()
            Case 6
                DrawTriangleFanGraphics()
            Case 7
                DrawTextGraphics()
            Case 8
                DrawCurveGraphics()
            Case 9
                DrawSurfaceGraphics()
        End Select

    End Sub

    'get the owner of the client graphics:
    'part,assembly --> ComponentDefinition
    'drawing ---> active sheet

    Private Sub getCG(ByRef oGraphicsNode As Object, _
                      Optional ByRef oCoordSet As Object = Nothing, _
                      Optional ByRef oOutDataSets As Object = Nothing)

        Dim oDoc As Document
        oDoc = m_invApp.ActiveDocument

        Dim oDataOwner As Object = Nothing
        Dim oGraphicsOwner As Object = Nothing

        'check the document type and get the owner of the datasets and graphics
        If oDoc.DocumentType = DocumentTypeEnum.kPartDocumentObject Or oDoc.DocumentType = DocumentTypeEnum.kAssemblyDocumentObject Then
            oDataOwner = oDoc
            oGraphicsOwner = oDoc.ComponentDefinition
        ElseIf oDoc.DocumentType = DocumentTypeEnum.kDrawingDocumentObject Then
            If oDoc.ActiveSheet Is Nothing Then
                MsgBox("The current document is a drawing. The command is supposed to draw client graphics on active sheet! But active sheet is null!")
                Exit Sub
            Else
                oDataOwner = oDoc.ActiveSheet
                oGraphicsOwner = oDoc.ActiveSheet
            End If
        End If

        'delete the data sets and graphics if they exist
        Try
            oDataOwner.GraphicsDataSetsCollection("TestCG").Delete()
        Catch ex As Exception
        End Try 
 
        Try
            oGraphicsOwner.ClientGraphicsCollection("TestCG").Delete()
        Catch ex As Exception
        End Try
 
        'create DataSets 
        Dim oDataSets As GraphicsDataSets = oDataOwner.GraphicsDataSetsCollection.Add("TestCG")
        oOutDataSets = oDataSets

        'create one coordinate data set
        oCoordSet = oDataSets.CreateCoordinateSet(oDataSets.Count + 1)

        'create graphics node
        Dim oClientGraphics As Inventor.ClientGraphics = oGraphicsOwner.ClientGraphicsCollection.Add("TestCG")
        oGraphicsNode = oClientGraphics.AddNode(oClientGraphics.Count + 1)

    End Sub

  

    'PointGraphics Sample
    Private Sub DrawPointGraphics()


        Dim oCoordSet As Object = Nothing
        Dim oGraphicsNode As Object = Nothing
        Dim oDataSets As Object = Nothing

        'get datasets, dataset, graphics node
        getCG(oGraphicsNode, oCoordSet, oDataSets)

        'prepare the coordinates
        Dim oPointCoords(8) As Double
        oPointCoords(0) = 1 : oPointCoords(1) = 1 : oPointCoords(2) = 0
        oPointCoords(3) = 0 : oPointCoords(4) = 1 : oPointCoords(5) = 0
        oPointCoords(6) = 1 : oPointCoords(7) = 1 : oPointCoords(8) = 1
        Call oCoordSet.PutCoordinates(oPointCoords)

        'add PointGraphics
        Dim oPointGraphics As PointGraphics
        oPointGraphics = oGraphicsNode.AddPointGraphics

        ' Create an image set
        Dim oImageSet As GraphicsImageSet
        oImageSet = oDataSets.CreateImageSet(oDataSets.Count + 1)

        Dim oImage As IPictureDisp
        oImage = Microsoft.VisualBasic.Compatibility.VB6.ImageToIPictureDisp(New System.Drawing.Bitmap(My.Resources.PointImage, 32, 32))

        Try
            Call oImageSet.Add(1, oImage)
        Catch ex As Exception
        End Try

        'set the related 
        oPointGraphics.PointRenderStyle = PointRenderStyleEnum.kCrossPointStyle
        oPointGraphics.CoordinateSet = oCoordSet
        oPointGraphics.SetCustomImage(oImageSet, 1)
        oPointGraphics.BurnThrough = True

        FitView()

    End Sub

    'LineGraphics Sample
    Private Sub DrawLineGraphics()


        Dim oCoordSet As Object = Nothing
        Dim oGraphicsNode As Object = Nothing

        'get datasets, dataset, graphics node
        getCG(oGraphicsNode, oCoordSet)

        Dim oPointCoords(11) As Double 'create 4 points
        oPointCoords(0) = 0 : oPointCoords(1) = 0 : oPointCoords(2) = 0
        oPointCoords(3) = 1 : oPointCoords(4) = 1 : oPointCoords(5) = 0
        oPointCoords(6) = 2 : oPointCoords(7) = 0 : oPointCoords(8) = 0
        oPointCoords(9) = 3 : oPointCoords(10) = 1 : oPointCoords(11) = 0
        Call oCoordSet.PutCoordinates(oPointCoords)

        'totally, 2 lines
        Dim oGraphic As LineGraphics
        oGraphic = oGraphicsNode.AddLineGraphics

        oGraphic.LineWeight = 5
        oGraphic.CoordinateSet = oCoordSet

        'add points to display the location of the points
        Dim oPGr As PointGraphics
        oPGr = oGraphicsNode.AddPointGraphics
        oPGr.CoordinateSet = oCoordSet

        FitView()

    End Sub

    'LineStripGraphics Sample

    Private Sub DrawLineStripGraphics()

        Dim oCoordSet As Object = Nothing
        Dim oGraphicsNode As Object = Nothing

        'get datasets, dataset, graphics node
        getCG(oGraphicsNode, oCoordSet)

        'Line strip: totally, 3 lines
        Dim oGraphic As LineStripGraphics
        oGraphic = oGraphicsNode.AddLineStripGraphics

        oGraphic.LineWeight = 5

        Dim oPointCoords(11) As Double 'create 4 points
        oPointCoords(0) = 0 : oPointCoords(1) = 0 : oPointCoords(2) = 0
        oPointCoords(3) = 1 : oPointCoords(4) = 1 : oPointCoords(5) = 0
        oPointCoords(6) = 2 : oPointCoords(7) = 0 : oPointCoords(8) = 0
        oPointCoords(9) = 3 : oPointCoords(10) = 1 : oPointCoords(11) = 0
        Call oCoordSet.PutCoordinates(oPointCoords)

        oGraphic.CoordinateSet = oCoordSet

        'add points to display the location of the points
        Dim oPGr As PointGraphics
        oPGr = oGraphicsNode.AddPointGraphics
        oPGr.CoordinateSet = oCoordSet
         

        FitView()

    End Sub

    'TriangleGraphics Sample

    Private Sub DrawTriangleGraphics()

        Dim oCoordSet As Object = Nothing
        Dim oGraphicsNode As Object = Nothing

        'get datasets, dataset, graphics node
        getCG(oGraphicsNode, oCoordSet)

        Dim oPointCoords(20) As Double    'create 7 points,
        oPointCoords(0) = 0 : oPointCoords(1) = 0 : oPointCoords(2) = 0
        oPointCoords(3) = 1 : oPointCoords(4) = 1 : oPointCoords(5) = 0
        oPointCoords(6) = 2 : oPointCoords(7) = 0 : oPointCoords(8) = 0
        oPointCoords(9) = 3 : oPointCoords(10) = 1 : oPointCoords(11) = 0
        oPointCoords(12) = 4 : oPointCoords(13) = 0 : oPointCoords(14) = 0
        oPointCoords(15) = 5 : oPointCoords(16) = 1 : oPointCoords(17) = 0
        oPointCoords(15) = 5 : oPointCoords(16) = 1 : oPointCoords(17) = 0
        oPointCoords(18) = 6 : oPointCoords(19) = 0 : oPointCoords(20) = 0

        Call oCoordSet.PutCoordinates(oPointCoords)

        'totally: 2 triangles
        Dim oGraphic As TriangleGraphics
        oGraphic = oGraphicsNode.AddTriangleGraphics

        oGraphic.CoordinateSet = oCoordSet


        'add points to display the location of the points
        Dim oPGr As PointGraphics
        oPGr = oGraphicsNode.AddPointGraphics
        oPGr.CoordinateSet = oCoordSet

        FitView()

    End Sub


    ' TriangleStripGraphics Sample
    Private Sub DrawTriangleStripGraphics()

        Dim oCoordSet As Object = Nothing
        Dim oGraphicsNode As Object = Nothing

        'get datasets, dataset, graphics node
        getCG(oGraphicsNode, oCoordSet)

        oGraphicsNode.Selectable = True

        Dim oPointCoords(20) As Double    'create 7 points,
        oPointCoords(0) = 0 : oPointCoords(1) = 0 : oPointCoords(2) = 0
        oPointCoords(3) = 1 : oPointCoords(4) = 1 : oPointCoords(5) = 0
        oPointCoords(6) = 2 : oPointCoords(7) = 0 : oPointCoords(8) = 0
        oPointCoords(9) = 3 : oPointCoords(10) = 1 : oPointCoords(11) = 0
        oPointCoords(12) = 4 : oPointCoords(13) = 0 : oPointCoords(14) = 0
        oPointCoords(15) = 5 : oPointCoords(16) = 1 : oPointCoords(17) = 0
        oPointCoords(15) = 5 : oPointCoords(16) = 1 : oPointCoords(17) = 0
        oPointCoords(18) = 6 : oPointCoords(19) = 0 : oPointCoords(20) = 0

        Call oCoordSet.PutCoordinates(oPointCoords)

        ' totally 5 triangles
        Dim oGraphic As TriangleStripGraphics
        oGraphic = oGraphicsNode.AddTriangleStripGraphics

        oGraphic.CoordinateSet = oCoordSet

        'add points to display the location of the points
        Dim oPGr As PointGraphics
        oPGr = oGraphicsNode.AddPointGraphics
        oPGr.CoordinateSet = oCoordSet


        FitView()


    End Sub

    'TriangleFanGraphics Sample
    Private Sub DrawTriangleFanGraphics()

        Dim oCoordSet As Object = Nothing
        Dim oGraphicsNode As Object = Nothing

        'get datasets, dataset, graphics node
        getCG(oGraphicsNode, oCoordSet)

        Dim oPointCoords(17) As Double 'Create 6 points

        oPointCoords(0) = 0 : oPointCoords(1) = 0 : oPointCoords(2) = 0
        oPointCoords(3) = 1 : oPointCoords(4) = 0 : oPointCoords(5) = 0
        oPointCoords(6) = 0 : oPointCoords(7) = 1 : oPointCoords(8) = 0
        oPointCoords(9) = -1 : oPointCoords(10) = 0 : oPointCoords(11) = 0
        oPointCoords(12) = 0 : oPointCoords(13) = -1 : oPointCoords(14) = 0
        oPointCoords(15) = 1 : oPointCoords(16) = 0 : oPointCoords(17) = 0

        ' totally 4 triangles. All around the first point
        Dim oGraphic As TriangleFanGraphics
        oGraphic = oGraphicsNode.AddTriangleFanGraphics

        Call oCoordSet.PutCoordinates(oPointCoords)
        oGraphic.CoordinateSet = oCoordSet

        'add points to display the location of the points
        Dim oPGr As PointGraphics
        oPGr = oGraphicsNode.AddPointGraphics
        oPGr.CoordinateSet = oCoordSet


        FitView()


    End Sub

    'CurveGraphics Sample
    Private Sub DrawCurveGraphics()

        Dim oCoordSet As Object = Nothing
        Dim oGraphicsNode As Object = Nothing

        'get datasets, dataset, graphics node
        getCG(oGraphicsNode, oCoordSet)


        Dim oTG As TransientGeometry
        oTG = m_invApp.TransientGeometry

        'create a circle 
        Dim oCircle As Inventor.Circle
        oCircle = oTG.CreateCircle(oTG.CreatePoint(0, 0, 0), oTG.CreateUnitVector(0, 0, 1), 5.0#)

        'add graphics with this circle
        Dim oGraphic As CurveGraphics
        oGraphic = oGraphicsNode.AddCurveGraphics(oCircle)

        oGraphic.LineWeight = 5
        oGraphic.Color = m_invApp.TransientObjects.CreateColor(255, 0, 0)

        FitView()

    End Sub

    'Text Client Graphics sample
     Private Sub DrawTextGraphics()

        Dim oCoordSet As Object = Nothing
        Dim oGraphicsNode As Object = Nothing

        getCG(oGraphicsNode, oCoordSet)

        Dim oTG As TransientGeometry
        oTG = m_invApp.TransientGeometry

        Dim oTextGraphics As TextGraphics
        oTextGraphics = oGraphicsNode.AddTextGraphics

        'Set the properties of the text
        oTextGraphics.Text = "Client Graphics Text"
        oTextGraphics.Anchor = oTG.CreatePoint(0, -0.5, 0)
        oTextGraphics.Bold = True
        oTextGraphics.Font = "Arial"
        oTextGraphics.FontSize = 40
        oTextGraphics.HorizontalAlignment = HorizontalTextAlignmentEnum.kAlignTextLeft
        oTextGraphics.Italic = True
        Call oTextGraphics.PutTextColor(0, 255, 0)
        oTextGraphics.VerticalAlignment = VerticalTextAlignmentEnum.kAlignTextMiddle


        FitView()

    End Sub


    'SurfaceGraphics Sample
    Private Sub DrawSurfaceGraphics()

        Dim oCoordSet As Object = Nothing
        Dim oGraphicsNode As Object = Nothing

        'get datasets, dataset, graphics node
        getCG(oGraphicsNode, oCoordSet)

        ' Set a reference to the transient geometry object for user later.
        Dim oTransGeom As TransientGeometry
        oTransGeom = m_invApp.TransientGeometry

        Dim oTransientBRep As TransientBRep
        oTransientBRep = m_invApp.TransientBRep

        ' Create a point representing the center of the bottom of the cone
        Dim oBottom As Point
        oBottom = m_invApp.TransientGeometry.CreatePoint(0, 0, 0)

        ' Create a point representing the tip of the cone
        Dim oTop As Point
        oTop = m_invApp.TransientGeometry.CreatePoint(0, 10, 0)

        ' Create a transient cone body
        Dim oBody As SurfaceBody
        oBody = oTransientBRep.CreateSolidCylinderCone(oBottom, oTop, 5, 5, 0)

        ' Reset the top point indicating the center of the top of the cylinder
        oTop = m_invApp.TransientGeometry.CreatePoint(0, -40, 0)

        ' Create a transient cylinder body
        Dim oCylBody As SurfaceBody
        oCylBody = oTransientBRep.CreateSolidCylinderCone(oBottom, oTop, 2.5, 2.5, 2.5)

        ' Union the cone and cylinder bodies
        Call oTransientBRep.DoBoolean(oBody, oCylBody, BooleanTypeEnum.kBooleanTypeUnion)

        ' Create client graphics based on the transient body
        Dim oSurfaceGraphics As SurfaceGraphics
        oSurfaceGraphics = oGraphicsNode.AddSurfaceGraphics(oBody)

        FitView()

    End Sub




#End Region

#Region "Using Index Set"

    Private Sub btnUseIndex_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUseIndex.Click

        Dim oCoordSet As Object = Nothing
        Dim oGraphicsNode As Object = Nothing
        Dim oDataSets As Object = Nothing

        'get datasets, dataset, graphics node
        getCG(oGraphicsNode, oCoordSet, oDataSets)

        Dim oPointCoords(11) As Double 'create 4 points
        oPointCoords(0) = 0 : oPointCoords(1) = 0 : oPointCoords(2) = 0
        oPointCoords(3) = 1 : oPointCoords(4) = 1 : oPointCoords(5) = 0
        oPointCoords(6) = 0 : oPointCoords(7) = 1 : oPointCoords(8) = 0
        oPointCoords(9) = 1 : oPointCoords(10) = 0 : oPointCoords(11) = 0
        Call oCoordSet.PutCoordinates(oPointCoords)

        'Line strip: totally, 3 lines
        Dim oGraphic As LineGraphics
        oGraphic = oGraphicsNode.AddLineGraphics

        oGraphic.LineWeight = 5

        Dim oCoordinateIndex As GraphicsIndexSet
        oCoordinateIndex = oDataSets.CreateIndexSet(oDataSets.Count + 1)
        oCoordinateIndex.Add(1, 1)  'from point 1
        oCoordinateIndex.Add(2, 3)  'connect to point 3
        oCoordinateIndex.Add(3, 3)  'from point 3
        oCoordinateIndex.Add(4, 2)  'connect to point 2
        oCoordinateIndex.Add(5, 2)  'from point 2
        oCoordinateIndex.Add(6, 4)  'connect to point 4


        'Create the color set: two colors.
        Dim oColorSet As GraphicsColorSet
        oColorSet = oDataSets.CreateColorSet(oDataSets.Count + 1)
        Call oColorSet.Add(1, 255, 0, 0)
        Call oColorSet.Add(2, 0, 255, 0)

        ' Create the index set for color
        Dim oColorIndex As GraphicsIndexSet
        oColorIndex = oDataSets.CreateIndexSet(oDataSets.Count + 1)
        oColorIndex.Add(1, 2)  'line 1 uses color 2
        oColorIndex.Add(2, 1)  'line 2 uses color 1
        oColorIndex.Add(3, 2)  'line 3 uses color 2


        oGraphic.CoordinateSet = oCoordSet
        oGraphic.CoordinateIndexSet = oCoordinateIndex

        oGraphic.ColorIndexSet = oColorIndex
        oGraphic.ColorSet = oColorSet
        oGraphic.ColorBinding = ColorBindingEnum.kPerItemColors

        'add points to display the location of the points
        Dim oPGr As PointGraphics
        oPGr = oGraphicsNode.AddPointGraphics
        oPGr.CoordinateSet = oCoordSet

        FitView()
    End Sub
#End Region

#Region "Using Strip"
    Private Sub btnUseStrip_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUseStrip.Click

        Dim oCoordSet As Object = Nothing
        Dim oGraphicsNode As Object = Nothing
        Dim oDataSets As Object = Nothing

        'get datasets, dataset, graphics node
        getCG(oGraphicsNode, oCoordSet, oDataSets)


        Dim oPointCoords(20) As Double    'create 7 points,
        oPointCoords(0) = 0 : oPointCoords(1) = 0 : oPointCoords(2) = 0
        oPointCoords(3) = 1 : oPointCoords(4) = 1 : oPointCoords(5) = 0
        oPointCoords(6) = 2 : oPointCoords(7) = 0 : oPointCoords(8) = 0
        oPointCoords(9) = 3 : oPointCoords(10) = 1 : oPointCoords(11) = 0
        oPointCoords(12) = 4 : oPointCoords(13) = 0 : oPointCoords(14) = 0
        oPointCoords(15) = 5 : oPointCoords(16) = 1 : oPointCoords(17) = 0
        oPointCoords(15) = 5 : oPointCoords(16) = 1 : oPointCoords(17) = 0
        oPointCoords(18) = 6 : oPointCoords(19) = 0 : oPointCoords(20) = 0

        Call oCoordSet.PutCoordinates(oPointCoords)


        'Create the color set: two colors.
        Dim oColorIndex As GraphicsIndexSet
        oColorIndex = oDataSets.CreateIndexSet(oDataSets.Count + 1)
        oColorIndex.Add(1, 1)
        oColorIndex.Add(2, 2)
        oColorIndex.Add(3, 1)
        oColorIndex.Add(4, 2)
        oColorIndex.Add(5, 1)


        ' Create the index set for color
        Dim oColorSet As GraphicsColorSet
        oColorSet = oDataSets.CreateColorSet(oDataSets.Count + 1)
        Call oColorSet.Add(1, 255, 0, 0)
        Call oColorSet.Add(2, 0, 255, 0)


        ' totally 5 triangles
        Dim oGraphic As TriangleStripGraphics
        oGraphic = oGraphicsNode.AddTriangleStripGraphics

        Dim oStrip(1) As Integer

        oStrip(0) = 3
        oStrip(1) = 4
        oGraphic.PutStripLengths(oStrip)

        oGraphic.CoordinateSet = oCoordSet

        oGraphic.ColorIndexSet = oColorIndex
        oGraphic.ColorSet = oColorSet
        oGraphic.ColorBinding = ColorBindingEnum.kPerStripColors

        'add points to display the location of the points
        Dim oPGr As PointGraphics
        oPGr = oGraphicsNode.AddPointGraphics
        oPGr.CoordinateSet = oCoordSet

        FitView()
    End Sub
#End Region

#Region "Using Model Native Data"
    'this sample uese the existing surface body of a part document. Please make sure 
    'a part document is opened with at least one surface body
    Private Sub btnUseModelData_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUseModelData.Click

        Dim oDoc As Document = m_invApp.ActiveDocument
        If oDoc.DocumentType = DocumentTypeEnum.kPartDocumentObject Then
            If oDoc.ComponentDefinition.SurfaceBodies.Count <> 0 Then
                Dim oCoordSet As Object = Nothing
                Dim oGraphicsNode As Object = Nothing
                Dim oDataSets As Object = Nothing

                'get datasets, dataset, graphics node
                getCG(oGraphicsNode, oCoordSet, oDataSets)

                Dim oTransGeom As TransientGeometry
                oTransGeom = m_invApp.TransientGeometry

                Dim oTransientBRep As TransientBRep
                oTransientBRep = m_invApp.TransientBRep

                'copy the surface body
                Dim oBody As SurfaceBody
                oBody = oTransientBRep.Copy(oDoc.ComponentDefinition.SurfaceBodies.Item(1))

                ' Create client graphics based on the transient body
                Dim oSurfaceGraphics As SurfaceGraphics
                oSurfaceGraphics = oGraphicsNode.AddSurfaceGraphics(oBody)

                Dim oMatrix As Matrix
                oMatrix = oTransGeom.CreateMatrix
                oMatrix.SetTranslation(oTransGeom.CreateVector(0, 0, 100))

                'there is a problem with Transformation if late-binding in VB.NET. So explictly decleare the node.
                Dim oTempGraphicsNode As GraphicsNode = oGraphicsNode
                oTempGraphicsNode.Transformation = oMatrix


                FitView()
            Else
                MsgBox("no surface body exists!")
            End If
          
        Else
            MsgBox("This sample uese the existing surface body of a part document.Please  make sure  at least one surface body exists! ")
        End If

    End Sub
#End Region

#Region "Store Client Graphics"
    'This sample creates a client feature with client graphics. Please make sure a 
    'part document is opened. After the code, save and close the document,
    ' finally open the document again. you will see the client graphics is displayed again
    Private Sub btnStore_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnStore.Click

        Dim oDoc As Document = m_invApp.ActiveDocument

        If oDoc.DocumentType = DocumentTypeEnum.kPartDocumentObject Then

            Dim partDoc As PartDocument
            partDoc = m_invApp.ActiveDocument

            'create client feature
            Dim cfDef As ClientFeatureDefinition
            cfDef = partDoc.ComponentDefinition.Features.ClientFeatures.CreateDefinition("My Test")

            Dim invCF As ClientFeature
            invCF = partDoc.ComponentDefinition.Features.ClientFeatures.Add(cfDef, "junk")

            Dim invCFD As ClientFeatureDefinition
            invCFD = invCF.Definition

            ' Get an arbitrary point on an edge of the feature, if the client feature has any elements.
            Dim oOrigin As Point
            ' There isn't any geometry so just place the graphics at the model origin.
            oOrigin = m_invApp.TransientGeometry.CreatePoint

            Dim oGraphicsData As GraphicsDataSets
            Try
                oGraphicsData = partDoc.GraphicsDataSetsCollection("TestCG_StoreData")
            Catch ex As Exception
                ' The Add2 method supported creating persistent graphics data for use with a client feature. 
                ' This is  supported directly on the client feature object from 2013

                'oGraphicsData = partDoc.GraphicsDataSetsCollection.Add2("TestCG_StoreData", True)

                oGraphicsData = partDoc.GraphicsDataSetsCollection.Add("TestCG_StoreData")
            End Try

            ' Create some client graphics.
            Dim oClientGraphics As Inventor.ClientGraphics
            Try
                oClientGraphics = invCFD.ClientGraphicsCollection("ClientFeatureTest")
            Catch ex As Exception
                oClientGraphics = invCFD.ClientGraphicsCollection.Add("ClientFeatureTest")
            End Try
 

            Dim oCoordSet As GraphicsCoordinateSet
            oCoordSet = oGraphicsData.CreateCoordinateSet(oGraphicsData.Count + 1)

            Dim oGraphicsNode As GraphicsNode
            oGraphicsNode = oClientGraphics.AddNode(oClientGraphics.Count + 1)

            Dim oPointCoords(11) As Double 'create 4 points
            oPointCoords(0) = 0 : oPointCoords(1) = 0 : oPointCoords(2) = 0
            oPointCoords(3) = 1 : oPointCoords(4) = 1 : oPointCoords(5) = 0
            oPointCoords(6) = 2 : oPointCoords(7) = 0 : oPointCoords(8) = 0
            oPointCoords(9) = 3 : oPointCoords(10) = 1 : oPointCoords(11) = 0
            Call oCoordSet.PutCoordinates(oPointCoords)

            'Line strip: totally, 3 lines
            Dim oGraphic As LineGraphics
            oGraphic = oGraphicsNode.AddLineGraphics

            oGraphic.LineWeight = 5
            oGraphic.CoordinateSet = oCoordSet

            FitView()

            MsgBox("client feature with client graphics is created! Please close the dialog and save the document. " & _
                   " You can close the document and open it again to verify.")

        Else
            MsgBox("This sample creates a client feature with client graphics. Please open a part document!")
        End If


        FitView()
    End Sub
#End Region

#Region "Interaction Client Graphics"

    '**********************************
    'Translated from sample of API help reference
    '***********************************
    Private Sub btnPreview_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPreview.Click
        Try

      
            Dim oDoc As Document = m_invApp.ActiveDocument
            If oDoc.DocumentType = DocumentTypeEnum.kAssemblyDocumentObject Then

                If oDoc.ComponentDefinition.Occurrences.Count <> 0 Then

                    MsgBox("Please select any occurrence and drag it. The command will create a cient graphics, copied from the occurence. It will move with mouse moving ")

                    Dim oClsDrag As clsDragComponent
                    oClsDrag = New clsDragComponent
                    oClsDrag.Initialize(m_invApp)

                Else
                    MsgBox("no occurrence in the assembly!")

                End If
            Else
                MsgBox("this sample is to drag an occurrence in the current assembly. Please open an assembly with at least one occurrence!")
            End If
        Catch ex As Exception
            MsgBox(ex.ToString())
        End Try
    End Sub


    '**********************************
    'Translated from sample of API help reference
    '***********************************
    'DrawOverlayGraphics
    'after the code, exit the dialog. the interactionevent is still running. Press 'ESC', the event stops and the graphics is removed.

    Private Sub btnOverlay_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOverlay.Click


        Dim oDoc As Document = m_invApp.ActiveDocument

        If Not oIE Is Nothing Then
            oIE.Stop()
            oIE = Nothing
        End If

        oIE = m_invApp.CommandManager.CreateInteractionEvents
        oIE.Start()


        Dim oIG As InteractionGraphics = oIE.InteractionGraphics

        Try
            Dim oDataSets As GraphicsDataSets = oIG.GraphicsDataSets
            ' Set a reference to the transient geometry object for use later.
            Dim oTransGeom As TransientGeometry = m_invApp.TransientGeometry

            ' Create a coordinate set.
            Dim oCoordSet As GraphicsCoordinateSet = oDataSets.CreateCoordinateSet(1)

            ' Create an array that contains coordinates that define a set of outwardly spiraling points.

            Dim oPointCoords(0 To 90) As Double
            Dim i As Long
            Dim dRadius As Double = 1

            Dim dAngle As Double
            For i = 0 To 29

                ' Define the X, Y, and Z components of the point.
                oPointCoords(i * 3 + 1) = dRadius * Math.Cos(dAngle)
                oPointCoords(i * 3 + 2) = dRadius * Math.Sin(dAngle)
                oPointCoords(i * 3 + 3) = i / 2

                ' Increment the angle and radius to create the spiral.
                dRadius = dRadius + 0.25
                dAngle = dAngle + (3.14159265358979 / 6)
            Next


            ' Assign the points into the coordinate set
            Call oCoordSet.PutCoordinates(oPointCoords)

            ' Create the ClientGraphics object.
            Dim oClientGraphics As Inventor.ClientGraphics = oIG.OverlayClientGraphics

            ' Create a new graphics node within the client graphics objects.
            Dim oLineNode As GraphicsNode = oClientGraphics.AddNode(1)

            ' Create a LineGraphics object within the node.
            Dim oLineSet As LineGraphics = oLineNode.AddLineGraphics

            ' Assign the coordinate set to the line graphics.
            oLineSet.CoordinateSet = oCoordSet

            ' Update the view to see the resulting spiral.
            oIG.UpdateOverlayGraphics(m_invApp.ActiveView)

            ' Create another graphics node for a line strip.
            Dim oLineStripNode As GraphicsNode = oClientGraphics.AddNode(2)

            ' Create a LineStripGraphics object within the new node.
            Dim oLineStrip As LineStripGraphics = oLineStripNode.AddLineStripGraphics

            ' Assign the same coordinate set to the line strip.
            oLineStrip.CoordinateSet = oCoordSet

            ' Create a color set to use in defining a explicit color to the line strip.
            Dim oColorSet As GraphicsColorSet = oDataSets.CreateColorSet(1)

            ' Add a single color to the set that is red.
            oColorSet.Add(1, 255, 0, 0)

            ' Assign the color set to the line strip.
            oLineStrip.ColorSet = oColorSet

            ' The two spirals are currently on top of each other so translate the
            ' new one in the x direction so they're side by side.
            Dim oMatrix As Matrix = oLineStripNode.Transformation

            oMatrix.SetTranslation(oTransGeom.CreateVector(15, 0, 0))
            oLineStripNode.Transformation = oMatrix

            ' Update the view to see the resulting spiral.
            oIG.UpdateOverlayGraphics(m_invApp.ActiveView)

            'oIE.Stop ()

            FitView()

            MsgBox("Please exit the dialog. the interactionevent is still running. Press 'ESC', the event stops and the graphics is removed.")

        Catch ex As Exception
            MsgBox(ex.ToString())
        End Try

    End Sub


#End Region
#Region "Advanced Functionalities"
    '**********************************
    'Translated from sample of API help reference
    '***********************************

    'This sample demonstrates the creation of client graphics based on the solid body of the active part.
    'The sample also demonstrates the use of the slicing functionality available for client graphics.
    ''Open a part that has a solid body and run the following code.
    Private Sub btnSlicing_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSlicing.Click

        Dim oDoc As Document
        oDoc = m_invApp.ActiveDocument

        If oDoc.DocumentType = DocumentTypeEnum.kPartDocumentObject Then
            If oDoc.ComponentDefinition.SurfaceBodies.Count = 0 Then
                MsgBox("Please  make sure  at least one surface body exists!")
                Exit Sub
            End If
        Else
            MsgBox("Please Open a part document with at least one surface body!")
            Exit Sub
        End If
        
        ' Set a reference to component definition of the active part.
        Dim oCompDef As PartComponentDefinition
        oCompDef = oDoc.ComponentDefinition

        Dim bApplyCap As Boolean
        If MsgBox("Do you want an end cap?", vbQuestion + vbYesNo, "SliceGraphics") = vbYes Then
            bApplyCap = True
        Else
            bApplyCap = False
        End If

        Dim oClientGraphics As Inventor.ClientGraphics
        Try
            oClientGraphics = oCompDef.ClientGraphicsCollection("SliceGraphicsID")
            oClientGraphics.Delete()
        Catch ex As Exception
        End Try
        oClientGraphics = oCompDef.ClientGraphicsCollection.Add("SliceGraphicsID")


        ' Create a new graphics node within the client graphics objects.
        Dim oSurfacesNode As GraphicsNode
        oSurfacesNode = oClientGraphics.AddNode(1)

        Dim oTransientBRep As TransientBRep
        oTransientBRep = m_invApp.TransientBRep

        ' Create a copy of the solid body in the part
        Dim oBody As SurfaceBody
        oBody = oTransientBRep.Copy(oCompDef.SurfaceBodies.Item(1))

        ' Create client graphics based on the transient body
        Dim oSurfaceGraphics As SurfaceGraphics
        oSurfaceGraphics = oSurfacesNode.AddSurfaceGraphics(oBody)

        ' Color it red
        oSurfacesNode.RenderStyle = oDoc.RenderStyles.Item("Red")

        ' Make the body in the part invisible
        oCompDef.SurfaceBodies.Item(1).Visible = False

        Dim oLineSegment As LineSegment
        oLineSegment = m_invApp.TransientGeometry.CreateLineSegment(oSurfacesNode.RangeBox.MaxPoint, oSurfacesNode.RangeBox.MinPoint)

        Dim oRootPoint As Point
        oRootPoint = oLineSegment.MidPoint

        ' Get the negative Z-axis vector
        Dim oNormal As Vector
        oNormal = m_invApp.TransientGeometry.CreateVector(0, 0, -1)

        ' Create a plane normal to Z axis with the root point at the center of the part
        Dim oPlane As Plane
        oPlane = m_invApp.TransientGeometry.CreatePlane(oRootPoint, oNormal)

        Dim oSlicingPlanes As ObjectCollection
        oSlicingPlanes = m_invApp.TransientObjects.CreateObjectCollection
        Call oSlicingPlanes.Add(oPlane)

        ' Slice the client graphics
        Call oSurfacesNode.SliceGraphics(bApplyCap, oSlicingPlanes)

        ' Update the view to see the result
        m_invApp.ActiveView.Update()
    End Sub

    '**********************************
    'Translated from sample of API help reference
    '***********************************

    ' This test applies texture coordinates expressing distance from the origin to 'the triangle mesh of whatever Part you have open. It then creates either a discrete-band or continuous color mapper and allows you to adjust the values of the mapper to change the range of values that map to various colors.  

    Private Sub btnMapping_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMapping.Click

        Dim oDoc As Document
        oDoc = m_invApp.ActiveDocument

        If oDoc.DocumentType = DocumentTypeEnum.kPartDocumentObject Then
            If oDoc.ComponentDefinition.SurfaceBodies.Count = 0 Then
                MsgBox("Please  make sure  at least one surface body exists!")
                Exit Sub
            End If
        Else
            MsgBox("Please Open a part document with at least one surface body!")
            Exit Sub
        End If


        Dim oMapClass As clsMapping = New clsMapping()
        oMapClass.Initialize(m_invApp)
        oMapClass.Demo()


    End Sub

#End Region

 
End Class