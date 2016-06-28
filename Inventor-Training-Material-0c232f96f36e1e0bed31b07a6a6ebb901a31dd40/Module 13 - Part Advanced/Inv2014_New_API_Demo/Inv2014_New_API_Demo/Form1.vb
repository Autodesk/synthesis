Imports Inventor
Imports System.Runtime.InteropServices
Imports System.Reflection
Imports System.Diagnostics
Imports System.Math

Public Class Form1

    Dim _InvApp As Inventor.Application
    Dim _macros As Macros
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            _InvApp = System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application")
        Catch ex As Exception
            MessageBox.Show("please open Inventor!")
            Exit Sub
        End Try


        ' Add any initialization after the InitializeComponent() call.
        _macros = New Macros(_InvApp)

        Dim methods As MemberInfo() = _macros.GetType().GetMembers()

        For Each member As MemberInfo In methods
            If (member.DeclaringType.Name = "Macros" And member.MemberType = MemberTypes.Method) Then
                ComboBoxMacros.Items.Add(member.Name)
            End If
        Next

        If ComboBoxMacros.Items.Count > 0 Then
            ComboBoxMacros.SelectedIndex = 0
            Button1.Enabled = True
        End If
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

'''macro class
Public Class Macros

    Dim ThisApplication As Inventor.Application

    Public Sub New(ByVal oApp As Inventor.Application)

        ThisApplication = oApp

    End Sub

    'Small helper function that prompts user for a file selection
    Private Function OpenFile(ByVal StrFilter As String) As String

        Dim filename As String = ""

        Dim ofDlg As System.Windows.Forms.OpenFileDialog = New System.Windows.Forms.OpenFileDialog()

        Dim user As String = System.Windows.Forms.SystemInformation.UserName

        ofDlg.Title = "Open File"
        ofDlg.InitialDirectory = "C:\Documents and Settings\" + user + "\Desktop\"

        ofDlg.Filter = StrFilter 'Example: "Inventor files (*.ipt; *.iam; *.idw)|*.ipt;*.iam;*.idw"
        ofDlg.FilterIndex = 1
        ofDlg.RestoreDirectory = True

        If (ofDlg.ShowDialog() = DialogResult.OK) Then
            filename = ofDlg.FileName
        End If

        OpenFile = filename

    End Function

    ''' <summary>
    ''' open demo file "ImprintSample.iam" to test
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CreateImprintFromAssembly()
        ' Get the active assembly document and its definition.
        Dim asmDoc As AssemblyDocument
        asmDoc = ThisApplication.ActiveDocument

        ' Have two parts selected in the assembly.
        Dim part1 As ComponentOccurrence
        Dim part2 As ComponentOccurrence
        part1 = ThisApplication.CommandManager.Pick(SelectionFilterEnum.kAssemblyLeafOccurrenceFilter, "Select part 1")
        part2 = ThisApplication.CommandManager.Pick(SelectionFilterEnum.kAssemblyLeafOccurrenceFilter, "Select part 2")

        ' Get the bodies associated with the occurrences.  Because of a problem when
        ' this sample was written the ImprintBodies method fails when used with SurfaceBodyProxy
        ' objects.  The code below works around this by creating new transformed bodies that
        ' will provide the equivalent result.

        ' Get the bodies in part space as transient bodies.
        Dim transBrep As TransientBRep
        transBrep = ThisApplication.TransientBRep
        Dim body1 As SurfaceBody
        Dim body2 As SurfaceBody
        body1 = transBrep.Copy(part1.Definition.SurfaceBodies.Item(1))
        body2 = transBrep.Copy(part2.Definition.SurfaceBodies.Item(1))

        ' Transform the bodies to be in the location represented in the assembly.
        Call transBrep.Transform(body1, part1.Transformation)
        Call transBrep.Transform(body2, part2.Transformation)

        ' Imprint the bodies.
        Dim newBody1 As SurfaceBody
        Dim newBody2 As SurfaceBody
        Dim body1OverlapFaces As Faces
        Dim body2OverlapFaces As Faces
        Dim body1OverlapEdges As Edges
        Dim body2OverlapEdges As Edges
        Call ThisApplication.TransientBRep.ImprintBodies(body1, body2, True, newBody1, newBody2, body1OverlapFaces, body2OverlapFaces, body1OverlapEdges, body2OverlapEdges)

        ' Create a new part document to show the resulting bodies in.
        Dim partDoc As PartDocument
        partDoc = ThisApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _
                      ThisApplication.FileManager.GetTemplateFileDocumentTypeEnum(DocumentTypeEnum.kPartDocumentObject))
        Dim partDef As PartComponentDefinition
        partDef = partDoc.ComponentDefinition

        ' Create a new feature from the first imprinted body.
        Dim non1 As NonParametricBaseFeature
        non1 = partDef.Features.NonParametricBaseFeatures.Add(newBody1)
        newBody1 = non1.SurfaceBodies.Item(1)

        ' Create a new feature from the second imprinted body.
        Dim non2 As NonParametricBaseFeature
        non2 = partDef.Features.NonParametricBaseFeatures.Add(newBody2)
        newBody2 = non2.SurfaceBodies.Item(1)
    End Sub


    ' This sample is intended to demonstrate a technique of finding the matching surfaces
    ' between the original input bodies and output imprinted bodies.  The API doesn't
    ' explicitly support this, but the sample takes advantage of the face that the faces
    ' are returned in a predictable order in the resulting bodies.  The sample assumes both
    ' bodies are in the active part.
    Public Sub SampleImprintMatching()
        ' Get the active assembly document and its definition.
        Dim doc As PartDocument
        doc = ThisApplication.ActiveDocument
        Dim def As PartComponentDefinition
        def = doc.ComponentDefinition

        ' Have two parts selected in the assembly.
        Dim part1 As ComponentOccurrence
        Dim part2 As ComponentOccurrence
        Dim body1 As SurfaceBody
        Dim body2 As SurfaceBody
        body1 = ThisApplication.CommandManager.Pick(SelectionFilterEnum.kPartBodyFilter, "Select body 1")
        body2 = ThisApplication.CommandManager.Pick(SelectionFilterEnum.kPartBodyFilter, "Select body 2")

        ' Imprint the bodies.
        Dim newBody1 As SurfaceBody
        Dim newBody2 As SurfaceBody
        Dim body1OverlapFaces As Faces
        Dim body2OverlapFaces As Faces
        Dim body1OverlapEdges As Edges
        Dim body2OverlapEdges As Edges
        Call ThisApplication.TransientBRep.ImprintBodies(body1, body2, True, newBody1, newBody2, body1OverlapFaces, body2OverlapFaces, body1OverlapEdges, body2OverlapEdges)

        Dim matchingIndexList1() As Integer
        Dim matchingIndexList2() As Integer
        ReDim matchingIndexList1(body1OverlapFaces.Count - 1)
        ReDim matchingIndexList2(body2OverlapFaces.Count - 1)
        Dim i As Integer
        For i = 1 To body1OverlapFaces.Count
            ' Find the indices of the overlapping face in the Faces collection.
            Dim j As Integer
            For j = 1 To newBody1.Faces.Count
                If body1OverlapFaces.Item(i) Is newBody1.Faces.Item(j) Then
                    matchingIndexList1(i - 1) = j
                    Exit For
                End If
            Next

            Dim body2Index As Integer
            For j = 1 To newBody2.Faces.Count
                If body2OverlapFaces.Item(i) Is newBody2.Faces.Item(j) Then
                    matchingIndexList2(i - 1) = j
                    Exit For
                End If
            Next
        Next

        ' The code below creates new non-parametric base features using the new imprinted bodies
        ' so that the results can be visualized.  The new bodies are created offset from the
        ' original so that they don't overlap and are more easily seen.

        ' Define a matrix to use in transforming the bodies.
        Dim trans As Matrix
        trans = ThisApplication.TransientGeometry.CreateMatrix

        ' Move the first imprinted body over based on the range so it doesn't lie on the original.
        trans.Cell(1, 4) = (body1.RangeBox.MaxPoint.X - body1.RangeBox.MinPoint.X) * 1.1
        Call ThisApplication.TransientBRep.Transform(newBody1, trans)

        ' Move the second imprinted body over based on the rangeso it doesn't lie on the original.
        trans.Cell(1, 4) = trans.Cell(1, 4) + (body1.RangeBox.MaxPoint.X - body1.RangeBox.MinPoint.X) * 1.1
        Call ThisApplication.TransientBRep.Transform(newBody2, trans)

        ' Create a new feature from the first imprinted body.
        Dim nonParaDef As NonParametricBaseFeatureDefinition
        nonParaDef = def.Features.NonParametricBaseFeatures.CreateDefinition
        Dim bodyColl As ObjectCollection
        bodyColl = ThisApplication.TransientObjects.CreateObjectCollection
        Call bodyColl.Add(newBody1)
        nonParaDef.BRepEntities = bodyColl
        nonParaDef.OutputType = BaseFeatureOutputTypeEnum.kSolidOutputType
        Dim non1 As NonParametricBaseFeature
        non1 = def.Features.NonParametricBaseFeatures.AddByDefinition(nonParaDef)
        newBody1 = non1.SurfaceBodies.Item(1)

        ' Create a new feature from the second imprinted body.
        nonParaDef = def.Features.NonParametricBaseFeatures.CreateDefinition
        bodyColl = ThisApplication.TransientObjects.CreateObjectCollection
        Call bodyColl.Add(newBody2)
        nonParaDef.BRepEntities = bodyColl
        nonParaDef.OutputType = BaseFeatureOutputTypeEnum.kSolidOutputType
        Dim non2 As NonParametricBaseFeature
        non2 = def.Features.NonParametricBaseFeatures.AddByDefinition(nonParaDef)
        newBody2 = non2.SurfaceBodies.Item(1)

        Dim cam As Camera
        cam = ThisApplication.ActiveView.Camera
        cam.Fit()
        cam.Apply()

        ' The code below demonstrates a technique to match up faces between
        ' the original bodies and the new bodies.  The order of the faces within
        ' the Faces collection of the original and new body is the same.  The
        ' additional faces in the new bodies are the first faces in the collection.
        Dim hs As HighlightSet
        hs = doc.CreateHighlightSet

        ' Get the difference in the number of faces in the original and new bodies.
        Dim diff As Integer
        diff = newBody1.Faces.Count - body1.Faces.Count

        ' Display the faces that match the faces in the original body.  It does this
        ' by skipping the new faces which are the first faces in the collection.
        For i = 1 To body1.Faces.Count
            Call hs.AddItem(body1.Faces.Item(i))
            Call hs.AddItem(newBody1.Faces.Item(i + diff))

            If Not (Abs(body1.Faces.Item(i).Evaluator.Area - newBody1.Faces.Item(i + diff).Evaluator.Area) < 0.0001) Then
                MsgBox("Modified matching face.")
            Else
                MsgBox("Matching Faces")
            End If

            hs.Clear()
        Next

        diff = newBody2.Faces.Count - body2.Faces.Count
        For i = 1 To body2.Faces.Count
            Call hs.AddItem(body2.Faces.Item(i))
            Call hs.AddItem(newBody2.Faces.Item(i + diff))

            If Not (Abs(body2.Faces.Item(i).Evaluator.Area - newBody2.Faces.Item(i + diff).Evaluator.Area) < 0.0001) Then
                MsgBox("Modified matching face.")
            Else
                MsgBox("Matching Faces")
            End If

            hs.Clear()
        Next

        For i = 1 To body1OverlapFaces.Count
            Call hs.AddItem(newBody1.Faces.Item(matchingIndexList1(i - 1)))
            Call hs.AddItem(newBody2.Faces.Item(matchingIndexList2(i - 1)))

            MsgBox("Overlapping Faces")
            hs.Clear()
        Next
    End Sub


    Public Sub SampleImprintMatching2()
        ' Get the active assembly document and its definition.
        Dim doc As PartDocument
        doc = ThisApplication.ActiveDocument
        Dim def As PartComponentDefinition
        def = doc.ComponentDefinition

        ' Have two parts selected in the assembly.
        Dim part1 As ComponentOccurrence
        Dim part2 As ComponentOccurrence
        Dim origBody1 As SurfaceBody
        Dim origBody2 As SurfaceBody
        origBody1 = ThisApplication.CommandManager.Pick(SelectionFilterEnum.kPartBodyFilter, "Select body 1")
        origBody2 = ThisApplication.CommandManager.Pick(SelectionFilterEnum.kPartBodyFilter, "Select body 2")

        ' Create alternate bodies.  Ideally this wouldn't be needed but this adds some special
        ' tags to the bodies that allow transient keys to continue to work after the ImprintBodies method is called.
        ' After this there is the original two bodies and the alternate version of each of the
        ' bodies.  The alternate may have more faces than the original, but the transient keys
        ' between the two should still match.
        Dim body1 As SurfaceBody
        Dim body2 As SurfaceBody
        body1 = origBody1.AlternateBody(Inventor.SurfaceGeometryFormEnum.SurfaceGeometryForm_NURBS)
        body2 = origBody2.AlternateBody(Inventor.SurfaceGeometryFormEnum.SurfaceGeometryForm_NURBS)

        ' Imprint the bodies.  After this call we now have three sets of bodies; the original, the alternate,
        ' and the imprinted bodies.  There should be additional faces in the imprinted bodies but the
        ' transient keys should still match for all three sets of bodies.
        Dim newBody1 As SurfaceBody
        Dim newBody2 As SurfaceBody
        Dim body1OverlapFaces As Faces
        Dim body2OverlapFaces As Faces
        Dim body1OverlapEdges As Edges
        Dim body2OverlapEdges As Edges
        Call ThisApplication.TransientBRep.ImprintBodies(body1, body2, True, newBody1, newBody2, body1OverlapFaces, body2OverlapFaces, body1OverlapEdges, body2OverlapEdges)

        ' The code below creates new non-parametric base features using the new imprinted bodies
        ' so that the results can be visualized.  The new bodies are created offset from the
        ' original so that they don't overlap and are more easily seen.
        '
        ' ** After this operation there will be four sets of bodies; the original, the alternate form,
        ' ** the imprinted bodies, and now the body created as a result of the feature creation.
        ' ** Because a new persisted body is created as part of the operation, new transient keys
        ' ** are assigned to the body, so the transient keys will no longer match this body.
        ' ** However, because they're essentially just copies of the imprinted bodies, the faces
        ' ** are in the same order in the two bodies so the indexes can be used to match between
        ' ** the two bodies.

        ' Define a matrix to use in transforming the bodies.
        Dim trans As Matrix
        trans = ThisApplication.TransientGeometry.CreateMatrix

        ' Move the first imprinted body over based on the range so it doesn't lie on the original.
        ' Moving the body is a modification of the existing body and leaves the transient keys as-is.
        ' However, it does change the face identities so comparing IUnknowns will no longer work.  Because
        ' of this I first save the indices of the overlapping faces while the faces are the same.
        Dim matchingIndexList1() As Integer
        Dim matchingIndexList2() As Integer
        ReDim matchingIndexList1(body1OverlapFaces.Count - 1)
        ReDim matchingIndexList2(body2OverlapFaces.Count - 1)
        Dim i As Integer
        For i = 1 To body1OverlapFaces.Count
            ' Find the indices of the overlapping face in the Faces collection.
            Dim j As Integer
            For j = 1 To newBody1.Faces.Count
                If body1OverlapFaces.Item(i) Is newBody1.Faces.Item(j) Then
                    matchingIndexList1(i - 1) = j
                    Exit For
                End If
            Next

            Dim body2Index As Integer
            For j = 1 To newBody2.Faces.Count
                If body2OverlapFaces.Item(i) Is newBody2.Faces.Item(j) Then
                    matchingIndexList2(i - 1) = j
                    Exit For
                End If
            Next
        Next

        ' Now do the transformation of the first body.
        trans.Cell(1, 4) = (body1.RangeBox.MaxPoint.X - body1.RangeBox.MinPoint.X) * 1.1
        Call ThisApplication.TransientBRep.Transform(newBody1, trans)

        ' Move the second imprinted body over based on the range so it doesn't lie on the original.
        trans.Cell(1, 4) = trans.Cell(1, 4) + (body1.RangeBox.MaxPoint.X - body1.RangeBox.MinPoint.X) * 1.1
        Call ThisApplication.TransientBRep.Transform(newBody2, trans)

        ' Create a new feature from the first imprinted body.
        Dim nonParaDef As NonParametricBaseFeatureDefinition
        nonParaDef = def.Features.NonParametricBaseFeatures.CreateDefinition
        Dim bodyColl As ObjectCollection
        bodyColl = ThisApplication.TransientObjects.CreateObjectCollection
        Call bodyColl.Add(newBody1)
        nonParaDef.BRepEntities = bodyColl
        nonParaDef.OutputType = BaseFeatureOutputTypeEnum.kSolidOutputType
        Dim non1 As NonParametricBaseFeature
        non1 = def.Features.NonParametricBaseFeatures.AddByDefinition(nonParaDef)
        Dim resultBody1 As SurfaceBody
        resultBody1 = non1.SurfaceBodies.Item(1)

        ' Create a new feature from the second imprinted body.
        nonParaDef = def.Features.NonParametricBaseFeatures.CreateDefinition
        bodyColl = ThisApplication.TransientObjects.CreateObjectCollection
        Call bodyColl.Add(newBody2)
        nonParaDef.BRepEntities = bodyColl
        nonParaDef.OutputType = BaseFeatureOutputTypeEnum.kSolidOutputType
        Dim non2 As NonParametricBaseFeature
        non2 = def.Features.NonParametricBaseFeatures.AddByDefinition(nonParaDef)
        Dim resultBody2 As SurfaceBody
        resultBody2 = non2.SurfaceBodies.Item(1)

        ' Fit the view to see the result.
        Dim cam As Camera
        cam = ThisApplication.ActiveView.Camera
        cam.Fit()
        cam.Apply()

        ' Create a highlight set used to see the matches.
        Dim hs As HighlightSet
        hs = doc.CreateHighlightSet

        ' Show the matches between the first imprint body and the original body.
        For i = 1 To newBody1.Faces.Count
            ' The face in the feature has a different transient key so get the
            ' face as the same index and assume it's the same.
            Dim newFace As Face
            newFace = resultBody1.Faces.Item(i)

            hs.AddItem(newFace)

            ' Get the corresponding face in the original body using the transient key from
            ' the imprinted face at the current index in the face collection.
            Dim otherFace As Face
            otherFace = origBody1.BindTransientKeyToObject(newBody1.Faces.Item(i).TransientKey)

            hs.AddItem(otherFace)

            MsgBox("Match")
            hs.Clear()
        Next

        ' Show the matches between the second imprint body and the original body.
        For i = 1 To newBody2.Faces.Count
            ' The face in the feature has a different transient key so get the
            ' face as the same index and assume it's the same.
            Dim newFace As Face = resultBody2.Faces.Item(i)

            hs.AddItem(newFace)

            ' Get the corresponding face in the original body using the transient key from
            ' the imprinted face at the current index in the face collection.
            Dim otherFace As Face = origBody2.BindTransientKeyToObject(newBody2.Faces.Item(i).TransientKey)

            hs.AddItem(otherFace)

            MsgBox("Match")
            hs.Clear()
        Next

        ' Highlight just the overlapping faces.
        ' The collection of overlapping faces returned by the ImprintBodies method correspond to
        ' the bodies returned by the ImprintBodies function.  This means the transient keys on the
        ' overlapping bodies will match all of the bodies except for the bodies created by the
        ' non-parametric base features.  The face index was save previously and will be used here.
        For i = 1 To body1OverlapFaces.Count
            Dim overlapFace1 As Face
            Dim overlapFace2 As Face
            overlapFace1 = resultBody1.Faces.Item(matchingIndexList1(i - 1))
            overlapFace2 = resultBody2.Faces.Item(matchingIndexList2(i - 1))

            hs.AddItem(overlapFace1)
            hs.AddItem(overlapFace2)

            MsgBox("Overlap Match")
            hs.Clear()
        Next
    End Sub



    ' This sample demonstrates several new curve creation techniques introduced in Inventor 2014.
    ' It creates a new part and then create a 2d control point spline and a 2d equation curve.
    ' Surfaces are created from these two curves by extruding them.  A 3d intersection curve is
    ' created between the extrusions.  A 3d control point spline and 3d equation curve are also created.
    Public Sub SketchCurves()
        ' Create a new part.
        Dim partDoc As PartDocument
        partDoc = ThisApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _
                      ThisApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject))
        Dim partDef As PartComponentDefinition
        partDef = partDoc.ComponentDefinition

        ' Create a 2D sketch on the X-Y plane.
        Dim sketch1 As PlanarSketch
        sketch1 = partDef.Sketches.Add(partDef.WorkPlanes.Item(3))

        Dim tg As TransientGeometry
        tg = ThisApplication.TransientGeometry

        ' Create a spline based on control points.
        Dim pnts As ObjectCollection
        pnts = ThisApplication.TransientObjects.CreateObjectCollection
        Call pnts.Add(tg.CreatePoint2d(2, 0))
        Call pnts.Add(tg.CreatePoint2d(4, 1))
        Call pnts.Add(tg.CreatePoint2d(4, 2))
        Call pnts.Add(tg.CreatePoint2d(6, 3))
        Call pnts.Add(tg.CreatePoint2d(8, 1))
        Dim controlPointSpline As SketchControlPointSpline
        controlPointSpline = sketch1.SketchControlPointSplines.Add(pnts)

        ' Create a 2D sketch on the Y-Z plane.
        Dim sketch2 As PlanarSketch
        sketch2 = partDef.Sketches.Add(partDef.WorkPlanes.Item(1))

        ' Create a spline based on an equation.
        Dim equationCurve As SketchEquationCurve
        equationCurve = sketch2.SketchEquationCurves.Add(CurveEquationTypeEnum.kParametric, CoordinateSystemTypeEnum.kCartesian, _
                                    ".001*t * cos(t)", ".001*t * sin(t)", 0, 360 * 3)

        ' Create a 3D sketch.
        Dim sketch3 As sketch3D
        sketch3 = partDef.Sketches3D.Add

        ' Create a 3D spline based on control points.
        pnts = ThisApplication.TransientObjects.CreateObjectCollection
        Call pnts.Add(tg.CreatePoint(10, 0, 0))
        Call pnts.Add(tg.CreatePoint(12, 1, 3))
        Call pnts.Add(tg.CreatePoint(12, 2, -5))
        Call pnts.Add(tg.CreatePoint(14, 3, 2))
        Call pnts.Add(tg.CreatePoint(16, 1, -3))
        Dim controlPointSpline2 As SketchControlPointSpline3D
        controlPointSpline2 = sketch3.SketchControlPointSplines3D.Add(pnts)

        ' Create a 3D spline based on an equation.
        Dim equationCurve2 As SketchEquationCurve3D
        equationCurve2 = sketch3.SketchEquationCurves3D.Add(CoordinateSystemTypeEnum.kCartesian, _
                                ".001*t * cos(t) + 8", ".001*t * sin(t)", "0.002*t", 0, 360 * 3)

        ThisApplication.ActiveView.Fit()

        ' Extrude the 2d curves.
        Dim prof As Profile
        prof = sketch1.Profiles.AddForSurface(controlPointSpline)
        Dim extrudeDef As ExtrudeDefinition
        extrudeDef = partDef.Features.ExtrudeFeatures.CreateExtrudeDefinition(prof, PartFeatureOperationEnum.kSurfaceOperation)
        Call extrudeDef.SetDistanceExtent(6, PartFeatureExtentDirectionEnum.kSymmetricExtentDirection)
        Dim extrude1 As ExtrudeFeature
        extrude1 = partDef.Features.ExtrudeFeatures.Add(extrudeDef)

        ' Change the work surface to not be transparent.
        Dim surf As WorkSurface
        surf = extrude1.SurfaceBodies.Item(1).Parent
        surf.Translucent = False

        prof = sketch2.Profiles.AddForSurface(equationCurve)
        extrudeDef = partDef.Features.ExtrudeFeatures.CreateExtrudeDefinition(prof, PartFeatureOperationEnum.kSurfaceOperation)
        Call extrudeDef.SetDistanceExtent(9, PartFeatureExtentDirectionEnum.kPositiveExtentDirection)
        Dim extrude2 As ExtrudeFeature
        extrude2 = partDef.Features.ExtrudeFeatures.Add(extrudeDef)

        ' Create a new sketch and an intersection curve.
        Dim interSketch As sketch3D
        interSketch = partDef.Sketches3D.Add

        Call interSketch.IntersectionCurves.Add(extrude1.SurfaceBodies.Item(1), extrude2.SurfaceBodies.Item(1))
    End Sub




    Public Sub SetOccurrenceAppearance()
        Dim asmDoc As AssemblyDocument
        asmDoc = ThisApplication.ActiveDocument

        ' Get an appearance from the document.  To assign an appearance is must
        ' exist in the document.  This looks for a local appearance and if that
        ' fails it copies the appearance from a library to the document.
        Dim localAsset As Asset

        localAsset = asmDoc.Assets.Item("Bamboo")
        Try
            ' Failed to get the appearance in the document, so import it.

            ' Get an asset library by name.  Either the displayed name (which
            ' can changed based on the current language) or the internal name
            ' (which is always the same) can be used.
            Dim assetLib As AssetLibrary
            assetLib = ThisApplication.AssetLibraries.Item("Autodesk Appearance Library")
            'Set assetLib = ThisApplication.AssetLibraries.Item("314DE259-5443-4621-BFBD-1730C6CC9AE9")

            ' Get an asset in the library.  Again, either the displayed name or the internal
            ' name can be used.
            Dim libAsset As Asset
            libAsset = assetLib.AppearanceAssets.Item("Bamboo")
            'Set libAsset = assetLib.AppearanceAssets.Item("ACADGen-082")

            ' Copy the asset locally.
            localAsset = libAsset.CopyTo(asmDoc)

        Catch ex As Exception
            MessageBox.Show(ex.ToString())
            Exit Sub
        End Try

       

        ' Have an occurrence selected.
        Dim occ As ComponentOccurrence
        occ = ThisApplication.CommandManager.Pick(SelectionFilterEnum.kAssemblyOccurrenceFilter, "Select an occurrence.")

        ' Assign the asset to the occurrence.
        occ.appearance = localAsset
    End Sub


    Public Sub RemoveAssemblyOverrides()
        ' Get the active assembly document.
        Dim asmDoc As AssemblyDocument
        asmDoc = ThisApplication.ActiveDocument

        ' Iterate through the objects that have an override.
        Dim obj As ComponentOccurrence
        For Each obj In asmDoc.ComponentDefinition.AppearanceOverridesObjects
            ' Set it so the occurrence uses the original color of the part.
            obj.AppearanceSourceType = AppearanceSourceTypeEnum.kPartAppearance
        Next
    End Sub


    Public Sub RemovePartOverrides()
        ' Get the active part document.
        Dim partDoc As PartDocument
        partDoc = ThisApplication.ActiveDocument

        ' Iterate through the objects that have an override.
        Dim obj As Object
        For Each obj In partDoc.ComponentDefinition.AppearanceOverridesObjects
            ' Set the source of the appearance based on the type of object.
            ' It's possible to use kPartAppearance in all cases, but this sets
            ' it to the default source used by Inventor when no overrides exist.
            If TypeOf obj Is SurfaceBody Then
                obj.AppearanceSourceType = AppearanceSourceTypeEnum.kPartAppearance
            ElseIf TypeOf obj Is PartFeature Then
                obj.AppearanceSourceType = AppearanceSourceTypeEnum.kBodyAppearance
            ElseIf TypeOf obj Is Face Then
                obj.AppearanceSourceType = AppearanceSourceTypeEnum.kFeatureAppearance
            Else
                MsgBox("Unexpected type with appearance override: " & TypeName(obj))
            End If
        Next
    End Sub


    Public Sub DumpDocumentAppearances()
        ' Check that a part or assembly document is active.
        If ThisApplication.ActiveDocumentType <> DocumentTypeEnum.kAssemblyDocumentObject And ThisApplication.ActiveDocumentType <> DocumentTypeEnum.kPartDocumentObject Then
            MsgBox("A part or assembly must be active.")
            Exit Sub
        End If

        Dim doc As Document
        doc = ThisApplication.ActiveDocument

        ' Open a file to write the results.
        Dim oLogFile As String = "C:\Temp\DocumentAppearanceDump.txt"
        Dim objWriter As New System.IO.StreamWriter(oLogFile, True)
 

        Dim appearance As Asset
        For Each appearance In doc.AppearanceAssets
            objWriter.WriteLine("    Appearance")
            objWriter.WriteLine("      DisplayName: " & appearance.DisplayName)
            objWriter.WriteLine("      HasTexture: " & appearance.HasTexture)
            objWriter.WriteLine("      IsReadOnly: " & appearance.IsReadOnly)
            objWriter.WriteLine("      Name: " & appearance.Name)

            Dim value As AssetValue
            For Each value In appearance
                Call PrintAssetValue(value, 8, objWriter)
            Next
        Next

        objWriter.Close()

        MsgBox("Finished writing output to ""C:\Temp\DocumentAppearanceDump.txt""")
    End Sub

    Public Sub DumpAllAppearancesInAllLibraries()
        ' Open a file to write the results.
        Dim oLogFile As String = "C:\Temp\AllLibAppearanceDump.txt"
        Dim objWriter As New System.IO.StreamWriter(oLogFile, True)


        ' Iterate through the libraries.
        Dim assetLib As AssetLibrary
        For Each assetLib In ThisApplication.AssetLibraries
            objWriter.WriteLine("Library" & assetLib.DisplayName)
            objWriter.WriteLine("  DisplayName: " & assetLib.DisplayName)
            objWriter.WriteLine("  FullFileName: " & assetLib.FullFileName)
            objWriter.WriteLine("  InternalName: " & assetLib.InternalName)
            objWriter.WriteLine("  IsReadOnly: " & assetLib.IsReadOnly)

            Dim appearance As Asset
            For Each appearance In assetLib.AppearanceAssets
                objWriter.WriteLine("    Appearance")
                objWriter.WriteLine("      DisplayName: " & appearance.DisplayName)
                objWriter.WriteLine("      Category: " & appearance.Category.DisplayName)
                objWriter.WriteLine("      HasTexture: " & appearance.HasTexture)
                objWriter.WriteLine("      IsReadOnly: " & appearance.IsReadOnly)
                objWriter.WriteLine("      Name: " & appearance.Name)

                Dim value As AssetValue
                For Each value In appearance
                    Call PrintAssetValue(value, 8, objWriter)
                Next
            Next
        Next

        objWriter.Close()

        MsgBox("Finished writing output to ""C:\Temp\AllLibAppearanceDump.txt""")
    End Sub

    Private Sub PrintAssetValue(ByVal InValue As AssetValue, ByVal Indent As Integer, ByRef objWriter As System.IO.StreamWriter)
        Dim indentChars As String
        indentChars = Space(Indent)

        objWriter.WriteLine(indentChars & "Value")
        objWriter.WriteLine(indentChars & "  DisplayName: " & InValue.DisplayName)
        objWriter.WriteLine(indentChars & "  Name: " & InValue.Name)
        objWriter.WriteLine(indentChars & "  IsReadOnly: " & InValue.IsReadOnly)

        Select Case InValue.ValueType
            Case AssetValueTypeEnum.kAssetValueTextureType
                objWriter.WriteLine(indentChars & "  Type: Texture")

                Dim textureValue As TextureAssetValue
                textureValue = InValue

                Dim texture As AssetTexture
                texture = textureValue.Value

                Select Case texture.TextureType
                    Case AssetTextureTypeEnum.kTextureTypeBitmap
                        objWriter.WriteLine(indentChars & "  TextureType: kTextureTypeBitmap")
                    Case AssetTextureTypeEnum.kTextureTypeChecker
                        objWriter.WriteLine(indentChars & "  TextureType: kTextureTypeChecker")
                    Case AssetTextureTypeEnum.kTextureTypeGradient
                        objWriter.WriteLine(indentChars & "  TextureType: kTextureTypeGradient")
                    Case AssetTextureTypeEnum.kTextureTypeMarble
                        objWriter.WriteLine(indentChars & "  TextureType: kTextureTypeMarble")
                    Case AssetTextureTypeEnum.kTextureTypeNoise
                        objWriter.WriteLine(indentChars & "  TextureType: kTextureTypeNoise")
                    Case AssetTextureTypeEnum.kTextureTypeSpeckle
                        objWriter.WriteLine(indentChars & "  TextureType: kTextureTypeSpeckle")
                    Case AssetTextureTypeEnum.kTextureTypeTile
                        objWriter.WriteLine(indentChars & "  TextureType: kTextureTypeTile")
                    Case AssetTextureTypeEnum.kTextureTypeUnknown
                        objWriter.WriteLine(indentChars & "  TextureType: kTextureTypeUnknown")
                    Case AssetTextureTypeEnum.kTextureTypeWave
                        objWriter.WriteLine(indentChars & "  TextureType: kTextureTypeWave")
                    Case AssetTextureTypeEnum.kTextureTypeWood
                        objWriter.WriteLine(indentChars & "  TextureType: kTextureTypeWood")
                    Case Else
                        objWriter.WriteLine(indentChars & "  TextureType: Unexpected type returned")
                End Select

                objWriter.WriteLine(indentChars & "  Values")
                Dim textureSubValue As AssetValue
                For Each textureSubValue In texture
                    Call PrintAssetValue(textureSubValue, Indent + 4, objWriter)
                Next
            Case AssetValueTypeEnum.kAssetValueTypeBoolean
                objWriter.WriteLine(indentChars & "  Type: Boolean")

                Dim booleanValue As BooleanAssetValue
                booleanValue = InValue

                objWriter.WriteLine(indentChars & "    Value: " & booleanValue.Value)
            Case AssetValueTypeEnum.kAssetValueTypeChoice
                objWriter.WriteLine(indentChars & "  Type: Choice")

                Dim choiceValue As ChoiceAssetValue
                choiceValue = InValue

                objWriter.WriteLine(indentChars & "    Value: " & choiceValue.Value)

                Dim names() As String
                Dim choices() As String
                Call choiceValue.GetChoices(names, choices)
                objWriter.WriteLine(indentChars & "    Choices:")
                Dim i As Integer
                For i = 0 To UBound(names)
                    objWriter.WriteLine(indentChars & "      " & names(i) & ", " & choices(i))
                Next
            Case AssetValueTypeEnum.kAssetValueTypeColor
                objWriter.WriteLine(indentChars & "  Type: Color")

                Dim colorValue As ColorAssetValue
                colorValue = InValue

                objWriter.WriteLine(indentChars & "  HasConnectedTexture: " & colorValue.HasConnectedTexture)
                objWriter.WriteLine(indentChars & "  HasMultipleValues: " & colorValue.HasMultipleValues)

                If Not colorValue.HasMultipleValues Then
                    objWriter.WriteLine(indentChars & "  Color: " & ColorString(colorValue.Value))
                Else
                    objWriter.WriteLine(indentChars & "  Colors")

                    Dim colors() As Color
                    colors = colorValue.Values

                    For i = 0 To UBound(colors)
                        objWriter.WriteLine(indentChars & "    Color: " & ColorString(colors(i)))
                    Next
                End If
            Case AssetValueTypeEnum.kAssetValueTypeFilename
                objWriter.WriteLine(indentChars & "  Type: Filename")

                Dim filenameValue As FilenameAssetValue
                filenameValue = InValue

                objWriter.WriteLine(indentChars & "    Value: " & filenameValue.Value)
            Case AssetValueTypeEnum.kAssetValueTypeFloat
                objWriter.WriteLine(indentChars & "  Type: Float")

                Dim floatValue As FloatAssetValue
                floatValue = InValue

                objWriter.WriteLine(indentChars & "    Value: " & floatValue.Value)
            Case AssetValueTypeEnum.kAssetValueTypeInteger
                objWriter.WriteLine(indentChars & "  Type: Integer")

                Dim integerValue As IntegerAssetValue
                integerValue = InValue

                objWriter.WriteLine(indentChars & "    Value: " & integerValue.Value)
            Case AssetValueTypeEnum.kAssetValueTypeInteger
                ' This value type is not currently used in any of the assets.
                objWriter.WriteLine(indentChars & "  Type: Reference")

                Dim refType As ReferenceAssetValue
                refType = InValue
            Case AssetValueTypeEnum.kAssetValueTypeString
                objWriter.WriteLine(indentChars & "  Type: String")

                Dim stringValue As StringAssetValue
                stringValue = InValue

                objWriter.WriteLine(indentChars & "    Value: """ & stringValue.Value & """")
        End Select
    End Sub

    Private Function ColorString(ByVal InColor As Color) As String
        ColorString = InColor.Red & "," & InColor.Green & "," & InColor.Blue & "," & InColor.Opacity
    End Function


    Public Sub CreateSimpleColorAppearance()
        Dim doc As Document
        doc = ThisApplication.ActiveDocument

        ' Only document appearances can be edited, so that's what's created.
        ' This assumes a part or assembly document is active.
        Dim docAssets As Assets
        docAssets = doc.Assets

        ' Create a new appearance asset.
        Dim appearance As Asset
        appearance = docAssets.Add(AssetTypeEnum.kAssetTypeAppearance, "Generic", _
                                        "MyShinyRed", "My Shiny Red Color")

        Dim tobjs As TransientObjects
        tobjs = ThisApplication.TransientObjects


        Dim color As ColorAssetValue
        color = appearance.Item("generic_diffuse")
        color.value = tobjs.CreateColor(255, 15, 15)

        Dim floatValue As FloatAssetValue
        floatValue = appearance.Item("generic_reflectivity_at_0deg")
        floatValue.value = 0.5

        floatValue = appearance.Item("generic_reflectivity_at_90deg")
        floatValue.value = 0.5
    End Sub


    ' *** Not working yet.
    ' Demonstrates the creation of a new appearance, assigning as the default appearance
    ' in the document and adding it to the favorites list.
    Public Sub CreateNewMarbleAppearance()
        Dim doc As Document
        doc = ThisApplication.ActiveDocument

        ' Only document appearances can be edited, so that's what's created.
        ' This assumes a part or assembly document is active.
        Dim docAssets As Assets
        docAssets = doc.Assets

        ' Create a new appearance asset.
        Dim appearance As Asset
        appearance = docAssets.Add(AssetTypeEnum.kAssetTypeAppearance, "Stone", _
                                        "RedGreenMarble", "Red and Green Marble")

        Dim tobjs As TransientObjects
        tobjs = ThisApplication.TransientObjects

        ' Set the diffuse color to be a marble texture.
        Dim stoneColor As TextureAssetValue
        stoneColor = appearance.Item("stone_color")

        Dim texture As AssetTexture
        texture = stoneColor.value
        Call texture.ChangeTextureType(AssetTextureTypeEnum.kTextureTypeMarble)

        texture.Item("marble_Color1").value = tobjs.CreateColor(255, 0, 0)
        texture.Item("marble_Color2").value = tobjs.CreateColor(0, 255, 0)

        stoneColor.Item("marble_Size").value = 2.5

        ' Assign this as the active appearance of the document.
        doc.ActiveAppearance = appearance

        ' Copy this appearance to the favorites.
        Call appearance.AddTo(ThisApplication.FavoriteAssets)
    End Sub






    ' 3D dimension is not provided with beta1
    'Public Sub Create3DDimension()
    '    ' Get the active document.  This assumes a part is open and active.
    '    Dim partDoc As PartDocument
    '    partDoc = ThisApplication.ActiveDocument

    '    ' Have the user select a linear edge.
    '    Dim dimEdge As Edge
    '    dimEdge = ThisApplication.CommandManager.Pick(SelectionFilterEnum.kPartEdgeFilter, "Select a linear edge.")

    '    Dim annots As ModelAnnotations
    '    annots = partDoc.ComponentDefinition.ModelAnnotations

    '    Dim intent1 As ModelGeometryIntent
    '    intent1 = partDoc.ComponentDefinition.CreateGeometryIntent(dimEdge.StartVertex)

    '    Dim intent2 As ModelGeometryIntent
    '    intent2 = partDoc.ComponentDefinition.CreateGeometryIntent(dimEdge.StopVertex)

    '    Dim tg As TransientGeometry
    '    tg = ThisApplication.TransientGeometry

    '    Dim linearDef As LinearModelDimensionDefinition
    '    linearDef = annots.ModelDimensions.LinearModelDimensions.CreateDefinition(intent1, intent2, tg.CreatePoint(10, 10, 10), kAlignedDimensionType)

    '    Dim linearDim As LinearModelDimension
    '    linearDim = annots.ModelDimensions.LinearModelDimensions.Add(linearDef)
    'End Sub

End Class

