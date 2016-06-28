Imports Inventor

Public Class Form1

    Dim mApp As Inventor.Application

    Public Sub New(ByVal oApp As Inventor.Application)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mApp = oApp

    End Sub

 

    'Create a part document from scratch, add a sketch, some entities and create 
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

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


        '  //this is the old way, i.e. create the extrude feature directly. it is still supported for backward compatibility
        'Dim oExtrude As ExtrudeFeature = _
        'oDoc.ComponentDefinition.Features.ExtrudeFeatures.AddByDistanceExtent(oProfile, 5.0, PartFeatureExtentDirectionEnum.kPositiveExtentDirection, PartFeatureOperationEnum.kNewBodyOperation)


        'Definition Way:
        ' Create an extrude definition.
        Dim extrudeDef As ExtrudeDefinition
        extrudeDef = oDoc.ComponentDefinition.Features.ExtrudeFeatures.CreateExtrudeDefinition(oProfile, PartFeatureOperationEnum.kNewBodyOperation)
    
        ' Modify the extent and taper angles.
        extrudeDef.SetDistanceExtent(8, PartFeatureExtentDirectionEnum.kNegativeExtentDirection)
        extrudeDef.SetDistanceExtentTwo(20)
        extrudeDef.TaperAngle = "-2 deg"
        extrudeDef.TaperAngleTwo = "-10 deg"
    
        ' Create the extrusion.
        Dim extrude As ExtrudeFeature
         extrude =  oDoc.ComponentDefinition.Features.ExtrudeFeatures.Add(extrudeDef)


        'Fit the view programmatically
        Dim oCamera As Camera = mApp.ActiveView.Camera
        
        oCamera.ViewOrientationType = ViewOrientationTypeEnum.kIsoTopRightViewOrientation
        oCamera.Apply()

        mApp.ActiveView.Fit()

    End Sub

   
End Class
