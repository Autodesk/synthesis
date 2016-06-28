Imports Inventor
Imports System.Reflection

Public Class Form1

    Dim _macros As Macros

    Public Sub New(ByVal oApp As Inventor.Application)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        _macros = New Macros(oApp)

        Dim methods As MemberInfo() = _macros.GetType().GetMembers()

        For Each member As MemberInfo In methods
            If (member.DeclaringType.Name = "Macros" And member.MemberType = MemberTypes.Method) Then
                ComboBoxMacros.Items.Add(member.Name)
            End If
        Next

        ComboBoxMacros.SelectedIndex = 0

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        Try
            Dim memberName As String = ComboBoxMacros.SelectedItem.ToString()

            Dim params() As Object = Nothing
            _macros.GetType().InvokeMember(memberName, BindingFlags.InvokeMethod, Nothing, _macros, params, Nothing, Nothing, Nothing)

        Catch ex As Exception
            Dim Caption As String = ex.Message
            Dim Buttons As MessageBoxButtons = MessageBoxButtons.OK
            Dim Result As DialogResult = MessageBox.Show(ex.StackTrace, Caption, Buttons, MessageBoxIcon.Exclamation)
        End Try

    End Sub

End Class


Public Class Macros

    Dim _InvApplication As Inventor.Application

    Public Sub New(ByVal oApp As Inventor.Application)

        _InvApplication = oApp

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use: Creates a new part, adds a new sketch and create some entities with constraints
    '//
    '//
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub CreateSketch()

        'Create new part document
        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _
                                        _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject), _
                                        True)

        Dim oPlanarEntity As WorkPlane
        oPlanarEntity = oPartDoc.ComponentDefinition.WorkPlanes("XY Plane")


        'Create new sketch
        Dim oSketch As PlanarSketch
        oSketch = oPartDoc.ComponentDefinition.Sketches.Add(oPlanarEntity)

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry


        'Create some sketch entities
        Dim oLine As SketchLine

        oLine = oSketch.SketchLines.AddByTwoPoints(oTG.CreatePoint2d(15, 5), oTG.CreatePoint2d(15, 0))

        oLine = oSketch.SketchLines.AddByTwoPoints(oLine.EndSketchPoint, oTG.CreatePoint2d(0, 0))

        oLine = oSketch.SketchLines.AddByTwoPoints(oLine.EndSketchPoint, oTG.CreatePoint2d(0, 10))

        oLine = oSketch.SketchLines.AddByTwoPoints(oLine.EndSketchPoint, oTG.CreatePoint2d(10, 10))

        Dim oArc As SketchArc
        oArc = oSketch.SketchArcs.AddByCenterStartEndPoint(oTG.CreatePoint2d(10, 5), oSketch.SketchLines(1).StartSketchPoint, oLine.EndSketchPoint)


        'Create some constraints
        Dim oRadiusConstraint As RadiusDimConstraint
        oRadiusConstraint = oSketch.DimensionConstraints.AddRadius(oArc, oTG.CreatePoint2d(15, 10))

        Dim oGeoConstraint As VerticalConstraint
        oGeoConstraint = oSketch.GeometricConstraints.AddVertical(oSketch.SketchLines(3), True)

        'Fit the view
        _InvApplication.ActiveView.Fit()

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Using sketch created in previous sample
    '//
    '// Use: Create Extrude Feature from existing sketch.
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub CreateExtrusion()

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.ActiveDocument

        Dim oSketch As Sketch
        oSketch = oPartDoc.ComponentDefinition.Sketches(1)

        Dim oProfile As Profile
        oProfile = oSketch.Profiles.AddForSolid()

        Dim oExtrudeFeatures As ExtrudeFeatures
        oExtrudeFeatures = oPartDoc.ComponentDefinition.Features.ExtrudeFeatures

        ' this is the old way, i.e. create the extrude feature directly. it is still supported for backward compatibility
        'Dim oExtrude As ExtrudeFeature
        'oExtrude = oExtrudeFeatures.AddByDistanceExtent(oProfile, 1, _
        '                                        PartFeatureExtentDirectionEnum.kPositiveExtentDirection, _
        '                                        PartFeatureOperationEnum.kJoinOperation)

           'Definition Way:
        ' Create an extrude definition.
        Dim extrudeDef As ExtrudeDefinition
        extrudeDef =oExtrudeFeatures.CreateExtrudeDefinition(oProfile, PartFeatureOperationEnum.kJoinOperation)
    
        ' Modify the extent and taper angles.
        extrudeDef.SetDistanceExtent(1, PartFeatureExtentDirectionEnum.kPositiveExtentDirection)      
        
        ' Create the extrusion.
        Dim extrude As ExtrudeFeature
         extrude =  oExtrudeFeatures.Add(extrudeDef)


        'Set Isometric View
        Dim oCamera As Camera
        oCamera = _InvApplication.ActiveView.Camera
        oCamera.ViewOrientationType = ViewOrientationTypeEnum.kIsoTopRightViewOrientation

        oCamera.Fit()
        oCamera.Apply()

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '//
    '//
    '// Use: Access Extrusion and update its length
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub UpdateExtrusion()

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.ActiveDocument

        Dim oExtrudeFeatures As ExtrudeFeatures
        oExtrudeFeatures = oPartDoc.ComponentDefinition.Features.ExtrudeFeatures

        Dim oExtrude As ExtrudeFeature
        oExtrude = oExtrudeFeatures(1)

        Dim oDistance As ModelParameter
        oDistance = oExtrude.Extent.Distance

        oDistance.Value = 5

        oPartDoc.Update()
        _InvApplication.ActiveView.Fit()

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use: Demonstrate how to select ProfilePaths inside a Profile object
    '//
    '//
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub CreateExtrusionFromModifiedProfile()

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _
                                        _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject), _
                                        True)

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        Dim oPlanarEntity As WorkPlane
        oPlanarEntity = oPartDoc.ComponentDefinition.WorkPlanes("XY Plane")

        'Create new sketch
        Dim oSketch As PlanarSketch
        oSketch = oPartDoc.ComponentDefinition.Sketches.Add(oPlanarEntity)

        'Create Sketch entities
        Call oSketch.SketchLines.AddAsTwoPointRectangle(oTG.CreatePoint2d(0, 0), oTG.CreatePoint2d(10, 10))

        Dim oCenter As SketchPoint
        oCenter = oSketch.SketchPoints.Add(oTG.CreatePoint2d(5, 5))

        Call oSketch.SketchLines.AddByTwoPoints(oSketch.SketchLines(1).StartSketchPoint, oCenter)
        Call oSketch.SketchLines.AddByTwoPoints(oCenter, oSketch.SketchLines(3).StartSketchPoint)
        Call oSketch.SketchLines.AddByTwoPoints(oSketch.SketchLines(1).EndSketchPoint, oCenter)
        Call oSketch.SketchLines.AddByTwoPoints(oCenter, oSketch.SketchLines(3).EndSketchPoint)

        'Create Profile
        Dim oProfile As Profile
        oProfile = oSketch.Profiles.AddForSolid

        'Delete some Paths
        Dim oProfilePath1 As ProfilePath
        Dim oProfilePath2 As ProfilePath

        oProfilePath1 = oProfile.Item(1)
        oProfilePath2 = oProfile.Item(3)

        oProfilePath1.Delete()
        oProfilePath2.Delete()

        'Create Extrusion
        Dim oExtrudeFeatures As ExtrudeFeatures
        oExtrudeFeatures = oPartDoc.ComponentDefinition.Features.ExtrudeFeatures

        'this is the old way, i.e. create the extrude feature directly. it is still supported for backward compat
        'Dim oExtrude As ExtrudeFeature
        'oExtrude = oExtrudeFeatures.AddByDistanceExtent(oProfile, 1, PartFeatureExtentDirectionEnum.kPositiveExtentDirection, PartFeatureOperationEnum.kJoinOperation)

            'Definition Way:
        ' Create an extrude definition.
        Dim extrudeDef As ExtrudeDefinition
        extrudeDef =oExtrudeFeatures.CreateExtrudeDefinition(oProfile, PartFeatureOperationEnum.kJoinOperation)
    
        ' Modify the extent and taper angles.
        extrudeDef.SetDistanceExtent(1, PartFeatureExtentDirectionEnum.kPositiveExtentDirection)      
        
        ' Create the extrusion.
        Dim extrude As ExtrudeFeature
         extrude =  oExtrudeFeatures.Add(extrudeDef)

        _InvApplication.ActiveView.Fit()

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use: Sketch3D Sample
    '//
    '//
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub Draw3DSketch()

        Dim oPartDef As PartComponentDefinition
        oPartDef = _InvApplication.ActiveDocument.ComponentDefinition

        Dim oSketch3d As Sketch3D
        oSketch3d = oPartDef.Sketches3D.Add

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        Dim oLastLine As SketchLine3D
        oLastLine = oSketch3d.SketchLines3D.AddByTwoPoints( _
                                        oPartDef.WorkPoints.Item(1), oTG.CreatePoint(5, 0, 0))

        oLastLine = oSketch3d.SketchLines3D.AddByTwoPoints( _
                                        oLastLine.EndSketchPoint, oTG.CreatePoint(5, 5, 0), True, 2)

        oLastLine = oSketch3d.SketchLines3D.AddByTwoPoints( _
                                        oLastLine.EndSketchPoint, oTG.CreatePoint(5, 5, 3))
    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use: Various SweepFeatures creation
    '//
    '//
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub SweepFeature()

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _
                                        _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject), _
                                        True)

        ' Set a reference to the component definition.
        Dim oCompDef As PartComponentDefinition
        oCompDef = oPartDoc.ComponentDefinition

        ' Set a reference to the transient geometry object.
        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        ' Create some workpoints that will be used for the 3D sketch.
        Dim oWPs(4) As WorkPoint
        oWPs(0) = oCompDef.WorkPoints.AddFixed(oTG.CreatePoint(0, 0, 0))
        oWPs(1) = oCompDef.WorkPoints.AddFixed(oTG.CreatePoint(3, 0, 0))
        oWPs(2) = oCompDef.WorkPoints.AddFixed(oTG.CreatePoint(3, 2, 0))
        oWPs(3) = oCompDef.WorkPoints.AddFixed(oTG.CreatePoint(3, 2, 2))
        oWPs(4) = oCompDef.WorkPoints.AddFixed(oTG.CreatePoint(6, 2, 2))

        ' Create a new 3D Sketch.
        Dim oSketch3d As Sketch3D
        oSketch3d = oPartDoc.ComponentDefinition.Sketches3D.Add

        ' Draw 3D lines. The first line is drawn between two of the work points.
        Dim oLine As SketchLine3D
        oLine = oSketch3d.SketchLines3D.AddByTwoPoints(oWPs(0), oWPs(1), True, 1)

        ' This second and subsequent lines are drawn between a 3D sketch point and a work point.
        ' The work point is obtained from the previous line. Because the two lines will share
        ' this 3D sketch point, Inventor will treat them as connected lines when creating any
        ' paths.
        oLine = oSketch3d.SketchLines3D.AddByTwoPoints(oLine.EndPoint, oWPs(2), True, 1)
        oLine = oSketch3d.SketchLines3D.AddByTwoPoints(oLine.EndPoint, oWPs(3), True, 0.75)
        oLine = oSketch3d.SketchLines3D.AddByTwoPoints(oLine.EndPoint, oWPs(4), True, 1)

        ' Create a 2D sketch.
        Dim oSketch As PlanarSketch
        oSketch = oCompDef.Sketches.Add(oCompDef.WorkPlanes.Item(2))

        ' Determine the model origin relative to the sketch space.
        Dim oOrigin As Point2d
        oOrigin = oSketch.ModelToSketchSpace(oTG.CreatePoint(0, 0, 0))

        ' Create two lines.
        Dim oNewPoint As Point2d
        oNewPoint = oTG.CreatePoint2d(oOrigin.X, oOrigin.Y - 4)
        Dim oSketchLine1 As SketchLine
        oSketchLine1 = oSketch.SketchLines.AddByTwoPoints(oOrigin, oNewPoint)
        oNewPoint.X = oNewPoint.X + 3
        Dim oSketchLine2 As SketchLine
        oSketchLine2 = oSketch.SketchLines.AddByTwoPoints(oSketchLine1.EndSketchPoint, oNewPoint)

        ' Create a fillet between the two lines.
        Call oSketch.SketchArcs.AddByFillet(oSketchLine1, oSketchLine2, 1, oSketchLine1.StartSketchPoint.Geometry, oSketchLine2.EndSketchPoint.Geometry)

        ' Get the end of the 2d sketch in model space.
        Dim oModelPoint As Point
        oModelPoint = oSketch.SketchToModelSpace(oSketchLine2.EndSketchPoint.Geometry)

        ' Create a work plane at the end of the 2D sketch.
        Dim oWP As WorkPlane
        oWP = oCompDef.WorkPlanes.AddByNormalToCurve(oSketchLine2, oSketchLine2.EndSketchPoint)

        ' Create a path. Because the 3D and 2D sketches physically connect the path will include
        ' both of them.
        Dim oPath As Path
        oPath = oCompDef.Features.SweepFeatures.CreatePath(oSketchLine2)

        ' Create a sketch containing a circle.
        oSketch = oCompDef.Sketches.Add(oWP)
        oOrigin = oSketch.ModelToSketchSpace(oTG.CreatePoint(0, 0, 0))
        Call oSketch.SketchCircles.AddByCenterRadius(oSketch.ModelToSketchSpace(oModelPoint), 0.375)

        ' Create a profile.
        Dim oProfile As Profile
        oProfile = oSketch.Profiles.AddForSolid

        ' Create the sweep feature.
        Dim oSweep As SweepFeature
        oSweep = oCompDef.Features.SweepFeatures.AddUsingPath(oProfile, oPath, PartFeatureOperationEnum.kJoinOperation)

    End Sub


    Public Sub SweepFeature2()

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _
                                        _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject), _
                                        True)

        Dim oCompDef As PartComponentDefinition
        oCompDef = oPartDoc.ComponentDefinition

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        Dim oWPs(1) As WorkPoint
        oWPs(0) = oCompDef.WorkPoints.AddFixed(oTG.CreatePoint(0, 0, 0))
        oWPs(1) = oCompDef.WorkPoints.AddFixed(oTG.CreatePoint(10, 0, 0))

        Dim oSketch3d As Sketch3D
        oSketch3d = oPartDoc.ComponentDefinition.Sketches3D.Add

        Dim oLine As SketchLine3D
        oLine = oSketch3d.SketchLines3D.AddByTwoPoints(oWPs(0), oWPs(1), True, 1)

        Dim oFitPoints As ObjectCollection
        oFitPoints = _InvApplication.TransientObjects.CreateObjectCollection

        oFitPoints.Add(oTG.CreatePoint(0, 0, 2))
        oFitPoints.Add(oTG.CreatePoint(2, 0, 1))
        oFitPoints.Add(oTG.CreatePoint(4, 0, 3))
        oFitPoints.Add(oTG.CreatePoint(6, 0, 2))
        oFitPoints.Add(oTG.CreatePoint(8, 0, 1))
        oFitPoints.Add(oTG.CreatePoint(10, 0, 0.5))

        Dim oSpline As SketchSpline3D
        oSpline = oSketch3d.SketchSplines3D.Add(oFitPoints)

        Dim oPath As Path
        oPath = oCompDef.Features.SweepFeatures.CreatePath(oLine)

        Dim oGuideRail As Path
        oGuideRail = oCompDef.Features.SweepFeatures.CreatePath(oSpline)

        Dim oSketch As PlanarSketch
        oSketch = oCompDef.Sketches.Add(oCompDef.WorkPlanes("YZ Plane"))

        'Call oSketch.SketchCircles.AddByCenterRadius(oSketch.ModelToSketchSpace(oWPs(1).Point), 0.375)

        Call oSketch.SketchLines.AddAsTwoPointRectangle(oSketch.ModelToSketchSpace(oTG.CreatePoint(0, -0.5, -0.5)), _
                                                        oSketch.ModelToSketchSpace(oTG.CreatePoint(0, 0.5, 0.5)))

        ' Create a profile.
        Dim oProfile As Profile
        oProfile = oSketch.Profiles.AddForSolid

        ' Create the sweep feature.
        Dim oSweep As SweepFeature
        oSweep = oCompDef.Features.SweepFeatures.AddUsingPathAndGuideRail(oProfile, oPath, oGuideRail, PartFeatureOperationEnum.kJoinOperation)

    End Sub


    Public Sub SweepFeature3()

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _
                                        _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject), _
                                        True)

        Dim oCompDef As PartComponentDefinition
        oCompDef = oPartDoc.ComponentDefinition

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        Dim oWPs(1) As WorkPoint
        oWPs(0) = oCompDef.WorkPoints.AddFixed(oTG.CreatePoint(0, 0, 0))
        oWPs(1) = oCompDef.WorkPoints.AddFixed(oTG.CreatePoint(10, 0, 0))

        Dim oSketch3d As Sketch3D
        oSketch3d = oPartDoc.ComponentDefinition.Sketches3D.Add

        Dim oLine As SketchLine3D
        oLine = oSketch3d.SketchLines3D.AddByTwoPoints(oWPs(0), oWPs(1), True, 1)

        Dim oFitPoints As ObjectCollection
        oFitPoints = _InvApplication.TransientObjects.CreateObjectCollection

        oFitPoints.Add(oTG.CreatePoint(0, 0, 1))
        oFitPoints.Add(oTG.CreatePoint(2, 1, 0))
        oFitPoints.Add(oTG.CreatePoint(4, 0, -1))
        oFitPoints.Add(oTG.CreatePoint(6, -1, 0))
        oFitPoints.Add(oTG.CreatePoint(8, 0, 1))
        oFitPoints.Add(oTG.CreatePoint(10, 1, 0))

        Dim oSpline As SketchSpline3D
        oSpline = oSketch3d.SketchSplines3D.Add(oFitPoints)

        Dim oPath As Path
        oPath = oCompDef.Features.SweepFeatures.CreatePath(oLine)

        Dim oGuideRail As Path
        oGuideRail = oCompDef.Features.SweepFeatures.CreatePath(oSpline)

        Dim oSketch As PlanarSketch
        oSketch = oCompDef.Sketches.Add(oCompDef.WorkPlanes("YZ Plane"))
        Call oSketch.SketchLines.AddAsTwoPointRectangle(oSketch.ModelToSketchSpace(oTG.CreatePoint(0, -0.5, -0.5)), _
                                                        oSketch.ModelToSketchSpace(oTG.CreatePoint(0, 0.5, 0.5)))


        ' Create a profile.
        Dim oProfile As Profile
        oProfile = oSketch.Profiles.AddForSolid

        ' Create the sweep feature.
        Dim oSweep As SweepFeature
        oSweep = oCompDef.Features.SweepFeatures.AddUsingPathAndGuideRail(oProfile, oPath, oGuideRail, PartFeatureOperationEnum.kJoinOperation)

    End Sub


    Public Sub SweepFeature4()

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _
                                        _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject), _
                                        True)

        Dim oCompDef As PartComponentDefinition
        oCompDef = oPartDoc.ComponentDefinition

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        Dim oSketch3d As Sketch3D
        oSketch3d = oPartDoc.ComponentDefinition.Sketches3D.Add

        Dim oFitPoints As ObjectCollection
        oFitPoints = _InvApplication.TransientObjects.CreateObjectCollection

        oFitPoints.Add(oTG.CreatePoint(0, 0, 0))
        oFitPoints.Add(oTG.CreatePoint(2, 0, 0))
        oFitPoints.Add(oTG.CreatePoint(4, 0, 0))

        Dim oSpline As SketchSpline3D
        oSpline = oSketch3d.SketchSplines3D.Add(oFitPoints)


        Dim oTwistPoints As ObjectCollection
        oTwistPoints = _InvApplication.TransientObjects.CreateObjectCollection

        Call oTwistPoints.Add(oSpline.FitPoint(1))
        Call oTwistPoints.Add(oSpline.FitPoint(2))
        Call oTwistPoints.Add(oSpline.FitPoint(3))


        Dim oTwistVectors As ObjectCollection
        oTwistVectors = _InvApplication.TransientObjects.CreateObjectCollection

        oTwistVectors.Add(oTG.CreateUnitVector(0, 0, 1))
        oTwistVectors.Add(oTG.CreateUnitVector(0, -1, 0))
        oTwistVectors.Add(oTG.CreateUnitVector(0, 1, 0))


        Dim oPath As Path
        oPath = oCompDef.Features.SweepFeatures.CreatePath(oSpline)

        Dim oSketch As PlanarSketch
        oSketch = oCompDef.Sketches.Add(oCompDef.WorkPlanes("YZ Plane"))
        Call oSketch.SketchLines.AddAsTwoPointRectangle(oSketch.ModelToSketchSpace(oTG.CreatePoint(0, -0.5, -0.5)), _
                                                        oSketch.ModelToSketchSpace(oTG.CreatePoint(0, 0.5, 0.5)))


        ' Create a profile.
        Dim oProfile As Profile
        oProfile = oSketch.Profiles.AddForSolid

        ' Create the sweep feature.
        Dim oSweep As SweepFeature
        oSweep = oCompDef.Features.SweepFeatures.AddUsingPathAndSectionTwists(oProfile, oPath, PartFeatureOperationEnum.kJoinOperation, oTwistPoints, oTwistVectors)

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use: WorkFeatures Creation
    '//
    '//
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub CreateWorkFeatures()

        Dim oPartDef As PartComponentDefinition
        oPartDef = _InvApplication.ActiveDocument.ComponentDefinition

        Dim oPlane1 As WorkPlane
        oPlane1 = oPartDef.WorkPlanes.AddByPlaneAndOffset(oPartDef.WorkPlanes.Item(1), 4)

        Dim oAxis1 As WorkAxis
        oAxis1 = oPartDef.WorkAxes.AddByTwoPlanes(oPlane1, oPartDef.WorkPlanes.Item(2))

        Dim oPlane2 As WorkPlane
        oPlane2 = oPartDef.WorkPlanes.AddByLinePlaneAndAngle(oAxis1, oPlane1, 3.14159 / 4)

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use: WorkFeatures Definition change
    '//
    '//
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub ChgWorkFeatureDef()

        Dim oPartDef As PartComponentDefinition
        oPartDef = _InvApplication.ActiveDocument.ComponentDefinition

        Dim oPlane2 As WorkPlane
        oPlane2 = oPartDef.WorkPlanes(5)

        'Change the WorkPlane Definition
        Call oPlane2.SetByTwoLines(oPartDef.WorkAxes(1), oPartDef.WorkAxes(4))

    End Sub



End Class
