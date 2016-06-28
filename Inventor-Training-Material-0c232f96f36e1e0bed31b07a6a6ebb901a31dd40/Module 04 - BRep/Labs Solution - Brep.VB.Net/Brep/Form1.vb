Imports Inventor

Public Class Form1

    Dim mApp As Inventor.Application

    Public Sub New(ByVal oApp As Inventor.Application)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mApp = oApp

    End Sub

    ''' <summary>
    ''' select a SketchLine and SketchCircle and get  intersect points of LineSegment and circle
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        If (Not mApp.ActiveDocument Is Nothing) Then

            If (mApp.ActiveDocument.DocumentType = DocumentTypeEnum.kPartDocumentObject) Then

                Dim oDoc As PartDocument = mApp.ActiveDocument

                If (oDoc.SelectSet.Count = 2) Then

                    If (TypeOf (oDoc.SelectSet(1)) Is SketchLine And TypeOf (oDoc.SelectSet(2)) Is SketchCircle) Then

                        Dim oSketchLine As SketchLine = oDoc.SelectSet(1)
                        Dim oSketchCircle As SketchCircle = oDoc.SelectSet(2)

                        Dim oLineSeg2d As LineSegment2d = oSketchLine.Geometry
                        Dim oCircle2d As Circle2d = oSketchCircle.Geometry

                        Dim objectsEnum As ObjectsEnumerator = oLineSeg2d.IntersectWithCurve(oCircle2d)

                        If (objectsEnum Is Nothing) Then
                            System.Windows.Forms.MessageBox.Show("No physical intersection between Line and Circle")
                            Exit Sub
                        End If

                        Dim strResult As String = "Intersection point(s):" + vbCrLf

                        Dim i As Integer
                        For i = 1 To objectsEnum.Count
                            Dim oPoint As Point2d = objectsEnum(i)
                            strResult += "[" + oPoint.X.ToString("F2") + ", " + oPoint.Y.ToString("F2") + "]" + vbCrLf
                        Next

                        System.Windows.Forms.MessageBox.Show(strResult)
                        Exit Sub

                    Else

                        System.Windows.Forms.MessageBox.Show("Entity 1 must be a SketchLine, Entity 2 must be a SketchCircle")
                        Exit Sub

                    End If

                Else

                    System.Windows.Forms.MessageBox.Show("Incorrect selection of sketch entities")
                    Exit Sub

                End If

            End If

        End If

    End Sub

    ''' <summary>
    ''' select a SketchLine and SketchCircle and get their  intersect points
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click

        If (Not mApp.ActiveDocument Is Nothing) Then

            If (mApp.ActiveDocument.DocumentType = DocumentTypeEnum.kPartDocumentObject) Then

                Dim oDoc As PartDocument = mApp.ActiveDocument

                If (oDoc.SelectSet.Count = 2) Then

                    If (TypeOf (oDoc.SelectSet(1)) Is SketchLine And TypeOf (oDoc.SelectSet(2)) Is SketchCircle) Then

                        Dim oSketchLine As SketchLine = oDoc.SelectSet(1)
                        Dim oSketchCircle As SketchCircle = oDoc.SelectSet(2)

                        Dim oLine2d As Line2d = mApp.TransientGeometry.CreateLine2d(oSketchLine.Geometry.StartPoint, oSketchLine.Geometry.Direction)
                        Dim oCircle2d As Circle2d = oSketchCircle.Geometry

                        Dim objectsEnum As ObjectsEnumerator = oLine2d.IntersectWithCurve(oCircle2d)

                        If (objectsEnum Is Nothing) Then
                            System.Windows.Forms.MessageBox.Show("No intersection between extended Line and Circle")
                            Exit Sub
                        End If

                        Dim strResult As String = "Intersection point(s):" + vbCrLf

                        Dim i As Integer
                        For i = 1 To objectsEnum.Count
                            Dim oPoint As Point2d = objectsEnum(i)
                            strResult += "[" + oPoint.X.ToString("F2") + ", " + oPoint.Y.ToString("F2") + "]" + vbCrLf
                        Next

                        System.Windows.Forms.MessageBox.Show(strResult)
                        Exit Sub

                    Else

                        System.Windows.Forms.MessageBox.Show("Entity 1 must be a SketchLine, Entity 2 must be a SketchCircle")
                        Exit Sub

                    End If

                Else

                    System.Windows.Forms.MessageBox.Show("Incorrect selection of sketch entities")
                    Exit Sub

                End If

            End If

        End If
    End Sub


    ''' <summary>
    ''' select an edge a dump its first deriv and tangent
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click

        If (Not mApp.ActiveDocument Is Nothing) Then

            If (mApp.ActiveDocument.DocumentType = DocumentTypeEnum.kPartDocumentObject) Then

                Dim oDoc As PartDocument = mApp.ActiveDocument

                If (oDoc.SelectSet.Count = 1) Then

                    If (TypeOf (oDoc.SelectSet(1)) Is Edge) Then

                        Dim oEdge As Edge = oDoc.SelectSet(1)

                        Dim oCurveEval As CurveEvaluator = oEdge.Evaluator

                        Dim MinParam As Double
                        Dim MaxParam As Double

                        oCurveEval.GetParamExtents(MinParam, MaxParam)

                        Dim length As Double
                        oCurveEval.GetLengthAtParam(MinParam, MaxParam, length)

                        Dim MidParam As Double
                        oCurveEval.GetParamAtLength(MinParam, length * 0.5, MidParam)

                        Dim Params() As Double = {MidParam}

                        Dim Points(3 * Params.Length - 1) As Double
                        oCurveEval.GetPointAtParam(Params, Points)

                        Dim Directions(3 * Params.Length - 1) As Double
                        Dim Curvatures(Params.Length - 1) As Double
                        oCurveEval.GetCurvature(Params, Directions, Curvatures)

                        Dim Tangents(3 * Params.Length - 1) As Double
                        oCurveEval.GetTangent(Params, Tangents)

                        Dim FirstDeriv(3 * Params.Length - 1) As Double
                        oCurveEval.GetFirstDerivatives(Params, FirstDeriv)


                        Dim strResult As String = "Curve Properties: " + vbCrLf + vbCrLf

                        strResult += " - Length: " + length.ToString("F2") + vbCrLf + vbCrLf

                        strResult += " - Middle point: [" + Points(0).ToString("F2") + ", " + _
                                                            Points(1).ToString("F2") + ", " + _
                                                            Points(2).ToString("F2") + "]" + vbCrLf + vbCrLf

                        strResult += " - Curvature: " + Curvatures(0).ToString("F2") + vbCrLf + vbCrLf

                        strResult += " - Tangent: [" + Tangents(0).ToString("F2") + ", " + _
                                                       Tangents(1).ToString("F2") + ", " + _
                                                       Tangents(2).ToString("F2") + "]" + vbCrLf + vbCrLf

                        strResult += " - First derivative: [" + FirstDeriv(0).ToString("F2") + ", " + _
                                                                FirstDeriv(1).ToString("F2") + ", " + _
                                                                FirstDeriv(2).ToString("F2") + "]" + vbCrLf + vbCrLf

                        System.Windows.Forms.MessageBox.Show(strResult, "Curve Evaluator")
                        Exit Sub

                    Else

                        System.Windows.Forms.MessageBox.Show("Selected entity must be an Edge", "Curve Evaluator")
                        Exit Sub

                    End If

                Else

                    System.Windows.Forms.MessageBox.Show("A single Edge must be selected first", "Curve Evaluator")
                    Exit Sub

                End If

            End If

        End If

    End Sub

    '// create a feature
    Private Sub createFeature()
           Dim oDoc As PartDocument = mApp.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _
                   mApp.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject), True)

        ' Get the XZ Plane
        Dim oWorkPlane As WorkPlane = oDoc.ComponentDefinition.WorkPlanes(2)

        Dim oSketch As PlanarSketch = oDoc.ComponentDefinition.Sketches.Add(oWorkPlane)

        Dim oTG As TransientGeometry = mApp.TransientGeometry

        'Create some transient points used for defining the lines (see BRep Module)
        Dim oPoints(4) As Point2d

        oPoints(0) = oTG.CreatePoint2d(0, 0)
        oPoints(1) = oTG.CreatePoint2d(-10, 0)
        oPoints(2) = oTG.CreatePoint2d(-10, -10)
        oPoints(3) = oTG.CreatePoint2d(5, -10)
        oPoints(4) = oTG.CreatePoint2d(5, -5)

        'Add the sketchlines, coincident constraints will be created automatically 
        'since the "Line.EndSketchPoint" are provided each time we create a new line
        Dim oLines(4) As SketchLine

        oLines(0) = oSketch.SketchLines.AddByTwoPoints(oPoints(0), oPoints(1))
        oLines(1) = oSketch.SketchLines.AddByTwoPoints(oLines(0).EndSketchPoint, oPoints(2))
        oLines(2) = oSketch.SketchLines.AddByTwoPoints(oLines(1).EndSketchPoint, oPoints(3))
        oLines(3) = oSketch.SketchLines.AddByTwoPoints(oLines(2).EndSketchPoint, oPoints(4))

        oSketch.SketchArcs.AddByCenterStartEndPoint(oTG.CreatePoint2d(0, -5), oLines(3).EndSketchPoint, oLines(0).StartSketchPoint)

        'Create a profile for the extrusion, here no need to worry since there is only 
        'a single profile that is possible
        Dim oProfile As Profile = oSketch.Profiles.AddForSolid

 
        'Definition Way:
        ' Create an extrude definition.
        Dim extrudeDef As ExtrudeDefinition
        extrudeDef = oDoc.ComponentDefinition.Features.ExtrudeFeatures.CreateExtrudeDefinition(oProfile, PartFeatureOperationEnum.kJoinOperation)
    
        ' Modify the extent and taper angles.
        extrudeDef.SetDistanceExtent(1, PartFeatureExtentDirectionEnum.kNegativeExtentDirection)
        
    
        ' Create the extrusion.
        Dim extrude As ExtrudeFeature
         extrude =  oDoc.ComponentDefinition.Features.ExtrudeFeatures.Add(extrudeDef)


    End Sub

    ''' <summary>
    '''create more feature based on the start face of the first extrude feature
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Button4_Click( ByVal sender As System.Object,  ByVal e As System.EventArgs) Handles Button4.Click

        ' // create a new document with an extrude feature.
        createFeature()

        Dim oPartDoc As  PartDocument = mApp.ActiveDocument

        'get a start face of the extrude feature
        Dim oExtrudeF as ExtrudeFeature  =  oPartDoc.ComponentDefinition.Features.ExtrudeFeatures(1)
        Dim oFirstFace As  Face  = oExtrudeF.StartFaces(1) 

          'add a new sketch on the basis of the start face
         Dim oSketch As  PlanarSketch  = oPartDoc.ComponentDefinition.Sketches.Add(oFirstFace, false) 
         Dim oTG As TransientGeometry = mApp.TransientGeometry 

         'create a circle and make a profile from the sketch
          oSketch.SketchCircles.AddByCenterRadius(oTG.CreatePoint2d(5, 5), 1) 

         Dim oProfile As  Profile= oSketch.Profiles.AddForSolid(true, Nothing , Nothing) 

         'get ExtrudeFeatures collection
         Dim extrudes As   ExtrudeFeatures  = oPartDoc.ComponentDefinition.Features.ExtrudeFeatures 

         'Create an extrude definition in the new surface body
         Dim  extrudeDef As ExtrudeDefinition = extrudes.CreateExtrudeDefinition(oProfile, PartFeatureOperationEnum.kJoinOperation) 

        'Modify the extent
         extrudeDef.SetDistanceExtent(2, PartFeatureExtentDirectionEnum.kPositiveExtentDirection)             

         'Create the extrusion.
         Dim extrude As   ExtrudeFeature  = extrudes.Add(extrudeDef)  
         Dim  oFilletFs as FilletFeatures= oPartDoc.ComponentDefinition.Features.FilletFeatures 
            
        'create fillet definition
          Dim  oFilletDef As FilletDefinition = oFilletFs.CreateFilletDefinition() 

        ' FaceCollection
         Dim oFacesCollOne as FaceCollection= mApp.TransientObjects.CreateFaceCollection() 
         oFacesCollOne.Add(oFirstFace) 

         Dim oFacesCollTwo as FaceCollection = mApp.TransientObjects.CreateFaceCollection() 
         oFacesCollTwo.Add(extrude.SideFaces(1)) 'cylinder face

          oFilletDef.AddFaceSet(oFacesCollOne, oFacesCollTwo, 0.1) 

            oFilletFs.Add(oFilletDef) 


        'Fit the view programmatically
        Dim oCamera As Camera = mApp.ActiveView.Camera
        
        oCamera.ViewOrientationType = ViewOrientationTypeEnum.kIsoTopRightViewOrientation
        oCamera.Apply()

        mApp.ActiveView.Fit()
    End Sub
End Class