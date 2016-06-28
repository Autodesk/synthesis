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


Public Class Macros

    Dim _InvApplication As Inventor.Application

    Public Sub New(ByVal oApp As Inventor.Application)

        _InvApplication = oApp

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

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use:
    '// [Sheet Metal Style Creation Example (Visual Basic)]
    '// This sample illustrates creating a new sheet metal style.
    '// It uses a sample bend table delivered with Inventor. You can
    '// edit the path below to reference any existing bend table.
    '// To use the sample make sure a bend table is available at the
    '// specified path, open a sheet metal document, and run the sample.
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub CreateSheetMetalStyle()

        ' Set a reference to the sheet metal document.
        ' This assumes a part document is active.
        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.ActiveDocument

        ' Make sure the document is a sheet metal document.
        If oPartDoc.SubType <> "{9C464203-9BAE-11D3-8BAD-0060B0CE6BB4}" Then
            MsgBox("A sheet metal document must be open.")
            Exit Sub
        End If

        ' Get the sheet metal component definition. Because this is a part document whose
        ' sub type is sheet metal, the document will return a SheetMetalComponentDefinition
        ' instead of a PartComponentDefinition.
        Dim oSheetMetalCompDef As SheetMetalComponentDefinition
        oSheetMetalCompDef = oPartDoc.ComponentDefinition

        ' Copy a sheet metal style to create a new one. There will always be at least
        ' one style in a document. This sample uses the first style, which is the default.
        Dim oStyle As SheetMetalStyle

        Try
            oStyle = oSheetMetalCompDef.SheetMetalStyles.Copy(
                oSheetMetalCompDef.SheetMetalStyles.Item(1), 
                "Custom Style")
        Catch
            MsgBox("Custom Style already exists :(")
            Exit Sub
        End Try

        ' Get the name of the parameter used for the thickness. We need the actual name
        ' to use in expressions to set the other values. It's best to get the name rather
        ' than hard code it because the name changes with various languages and the user
        ' can change the name in the Parameters dialog.

        ' This gets the name of the thickness from the component definition.
        Dim sThicknessName As String
        sThicknessName = oSheetMetalCompDef.Thickness.Name

        ' Set the various values associated with the style.
        oStyle.BendRadius = sThicknessName & " * 1.5"
        oStyle.BendReliefWidth = sThicknessName & " / 2"
        oStyle.BendReliefDepth = sThicknessName & " * 1.5"
        oStyle.CornerReliefSize = sThicknessName & " * 2.0"
        oStyle.MinimumRemnant = sThicknessName & " * 2.0"

        oStyle.BendReliefShape = BendReliefShapeEnum.kRoundBendReliefShape
        oStyle.BendTransition = BendTransitionEnum.kArcBendTransition
        oStyle.CornerReliefShape = CornerReliefShapeEnum.kRoundCornerReliefShape

        ' Add a linear unfold method.  Unfold methods are now separate
        ' from sheet metal styles.
        Try
            oSheetMetalCompDef.UnfoldMethods.AddLinearUnfoldMethod(
                                                    "Linear Sample",
                                                    "0.43")
        Catch
            MsgBox("Linear Sample UnfoldMethod already exists :(")
            Exit Sub
        End Try

        ' Add a bend table fold method. This uses error trapping to catch if an
        ' invalid bend table file was specified.
        Try
            Call oSheetMetalCompDef.UnfoldMethods.AddBendTableFromFile(
                                                "Table Sample", 
                                                OpenFile("Bend Table (*.txt)|*.txt"))
        Catch
            MsgBox("Unable to load bend table")
        End Try

        ' Make the new linear method the active unfold method for the document.
        Dim oUnfoldMethod As UnfoldMethod
        oUnfoldMethod = oSheetMetalCompDef.UnfoldMethods.Item("Linear Sample")
        oStyle.UnfoldMethod = oUnfoldMethod

        ' Activate this style, which will also update the part.
        oStyle.Activate()

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use:
    '// This sample illustrates getting information about sheet metal styles,
    '// unfold methods, and thickness.
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub SheetMetalStyleDisplay()

        ' Set a reference to the sheet metal document.
        ' This assumes a part document is active.
        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.ActiveDocument


        ' Make sure the document is a sheet metal document.
        If oPartDoc.SubType <> "{9C464203-9BAE-11D3-8BAD-0060B0CE6BB4}" Then
            MsgBox("A sheet metal document must be open.")
            Exit Sub
        End If

        ' Get the sheet metal component definition. Because this is a part document whose
        ' sub type is sheet metal, the document will return a SheetMetalComponentDefinition
        ' instead of a PartComponentDefinition.
        Dim oSheetMetalCompDef As SheetMetalComponentDefinition
        oSheetMetalCompDef = oPartDoc.ComponentDefinition

        oSheetMetalCompDef.ActiveSheetMetalStyle.PunchRepresentationType = PunchRepresentationTypeEnum.k2DSketchAndCenterMarkPunchRepresentation 
        ' Iterate through the sheet metal styles.
        Dim oStyle As SheetMetalStyle
        For Each oStyle In oSheetMetalCompDef.SheetMetalStyles

            ' Display information about the style.
            If oStyle Is oSheetMetalCompDef.ActiveSheetMetalStyle Then
                Debug.Print("** Active SheetMetal Style **")
            End If

            Debug.Print("Name: " & oStyle.Name)
            Debug.Print(" Bend Radius: " & oStyle.BendRadius)
            Debug.Print(" Bend Relief Depth: " & oStyle.BendReliefDepth)
            Debug.Print(" Bend Relief Width: " & oStyle.BendReliefWidth)

            Select Case oStyle.BendReliefShape
                Case BendReliefShapeEnum.kDefaultBendReliefShape
                    Debug.Print(" Bend Relief Shape: Default")
                Case BendReliefShapeEnum.kRoundBendReliefShape
                    Debug.Print(" Bend Relief Shape: Round")
                Case BendReliefShapeEnum.kStraightBendReliefShape
                    Debug.Print(" Bend Relief Shape: Straight")
                Case BendReliefShapeEnum.kTearBendReliefShape
                    Debug.Print(" Bend Relief Shape: Tear")
            End Select

            Select Case oStyle.BendTransition
                Case BendTransitionEnum.kDefaultBendTransition
                    Debug.Print(" Bend Transition: Default")
                Case BendTransitionEnum.kArcBendTransition
                    Debug.Print(" Bend Transition: Arc")
                Case BendTransitionEnum.kIntersectionBendTransition
                    Debug.Print(" Bend Transition: Intersection")
                Case BendTransitionEnum.kNoBendTransition
                    Debug.Print(" Bend Transition: No Bend")
                Case BendTransitionEnum.kStraightLineBendTransition
                    Debug.Print(" Bend Transition: Straight Line")
                Case BendTransitionEnum.kTrimToBendBendTransition
                    Debug.Print(" Bend Transition: Trom to Bend")
            End Select

            Select Case oStyle.CornerReliefShape
                Case CornerReliefShapeEnum.kDefaultCornerReliefShape
                    Debug.Print(" Corner Relief Shape: Default")
                Case CornerReliefShapeEnum.kRoundCornerReliefShape
                    Debug.Print(" Corner Relief Shape: Round")
                Case CornerReliefShapeEnum.kSquareCornerReliefShape
                    Debug.Print(" Corner Relief Shape: Square")
                Case CornerReliefShapeEnum.kTearCornerReliefShape
                    Debug.Print(" Corner Relief Shape: Tear")
                Case CornerReliefShapeEnum.kArcWeldCornerReliefShape
                    Debug.Print(" Corner Relief Shape: Arc Weld")
                Case CornerReliefShapeEnum.kFullRoundCornerReliefShape
                    Debug.Print(" Corner Relief Shape: Full Found")
                Case CornerReliefShapeEnum.kIntersectionCornerReliefShape
                    Debug.Print(" Corner Relief Shape: Intersection")
                Case CornerReliefShapeEnum.kLinearWeldReliefShape
                    Debug.Print(" Corner Relief Shape: Linear Weld")
                Case CornerReliefShapeEnum.kTrimToBendReliefShape
                    Debug.Print(" Corner Relief Shape: Trim to Bend")
                Case CornerReliefShapeEnum.kNoReplacementCornerReliefShape
                    Debug.Print(" Corner Relief Shape: No Replacement")
                Case CornerReliefShapeEnum.kRoundWithRadiusCornerReliefShape
                    Debug.Print(" Corner Relief Shape: Round with Radius")
            End Select

            Debug.Print(" Corner Relief Size: " & oStyle.CornerReliefSize)
            Debug.Print(" Minimum Remnant: " & oStyle.MinimumRemnant)

            Debug.Print(" Thickness: " & oStyle.Thickness)

            Debug.Print(" -------------------------- ")

        Next

        ' Display information about the unfold methods.
        Debug.Print("")
        Debug.Print("Unfold Methods")
        Dim oUnfoldMethod As UnfoldMethod
        For Each oUnfoldMethod In oSheetMetalCompDef.UnfoldMethods
            Debug.Print(" " & oUnfoldMethod.Name)
            Select Case oUnfoldMethod.UnfoldMethodType
                Case UnfoldMethodTypeEnum.kBendTableUnfoldMethod
                    Debug.Print(" Unfold Method Type: Bend Table")
                Case UnfoldMethodTypeEnum.kLinearUnfoldMethod
                    Debug.Print(" Unfold Method Type: Linear")
                    Debug.Print(" Value: " & oUnfoldMethod.kFactor)
                Case UnfoldMethodTypeEnum.kCustomEquationUnfoldMethod
                    Debug.Print(" Unfold Method Type: Custom Equation")
            End Select

            Debug.Print(" -------------------------- ")
        Next

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use: This sample illustrates editing the thickness of a sheet metal part
    '//
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub SetSheetMetalThickness()

        ' Set a reference to the sheet metal document.
        ' This assumes a sheet metal document is active.
        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.ActiveDocument

        ' Get the sheet metal component definition. Because this is a part document whose
        ' sub type is sheet metal, the document will return a SheetMetalComponentDefinition
        ' instead of a PartComponentDefinition.
        Dim oSheetMetalCompDef As SheetMetalComponentDefinition
        oSheetMetalCompDef = oPartDoc.ComponentDefinition

        ' Change Thickness
        oSheetMetalCompDef.ActiveSheetMetalStyle.Thickness = "0.50 in"

        ' Update the part.
        _InvApplication.ActiveDocument.Update()

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use:
    '// This sample demonstrates the creation of sheet metal face and cut features.
    '// It creates a new sheet metal document, create a face feature, a cut feature
    '// and another face feature.  The second face feature butts up to the first
    '// face feature so it automatically creates a bend between them.
    '//
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub FaceAndCutFeatureCreation()

        ' Create a new sheet metal document, using the default sheet metal template.
        Dim oSheetMetalDoc As PartDocument
        oSheetMetalDoc = _InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _
                     _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject, _
                                                                 SystemOfMeasureEnum.kDefaultSystemOfMeasure, _
                                                                 DraftingStandardEnum.kDefault_DraftingStandard, _
                                                                 "{9C464203-9BAE-11D3-8BAD-0060B0CE6BB4}"))

        ' Set a reference to the component definition.
        Dim oCompDef As SheetMetalComponentDefinition
        oCompDef = oSheetMetalDoc.ComponentDefinition

        ' Set a reference to the sheet metal features collection.
        Dim oSheetMetalFeatures As SheetMetalFeatures
        oSheetMetalFeatures = oCompDef.Features

        ' Create a new sketch on the X-Y work plane.
        Dim oSketch As PlanarSketch
        oSketch = oCompDef.Sketches.Add(oCompDef.WorkPlanes.Item(3))

        ' Set a reference to the transient geometry object.
        Dim oTransGeom As TransientGeometry
        oTransGeom = _InvApplication.TransientGeometry

        ' Draw a 20cm x 15cm rectangle with the corner at (0,0)
        Call oSketch.SketchLines.AddAsTwoPointRectangle( _
                                    oTransGeom.CreatePoint2d(0, 0), _
                                    oTransGeom.CreatePoint2d(20, 15))

        ' Create a profile.
        Dim oProfile As Profile
        oProfile = oSketch.Profiles.AddForSolid

        Dim oFaceFeatureDefinition As FaceFeatureDefinition
        oFaceFeatureDefinition = oSheetMetalFeatures.FaceFeatures.CreateFaceFeatureDefinition(oProfile)

        ' Create a face feature.
        Dim oFaceFeature As FaceFeature
        oFaceFeature = oSheetMetalFeatures.FaceFeatures.Add(oFaceFeatureDefinition)

        ' Get the top face for creating the new sketch.
        Dim aSelectTypes(0) As SelectionFilterEnum
        aSelectTypes(0) = SelectionFilterEnum.kPartFaceFilter
        Dim oFoundFaces As ObjectsEnumerator
        oFoundFaces = oCompDef.FindUsingPoint(oTransGeom.CreatePoint(1, 1, oCompDef.Thickness.Value), aSelectTypes, 0.001)
        Dim oFrontFace As Face
        oFrontFace = oFoundFaces.Item(1)

        ' Create a new sketch on this face, but use the method that allows you to
        ' control the orientation and orgin of the new sketch.
        oSketch = oCompDef.Sketches.Add(oFrontFace)

        ' Create the interior 3cm x 2cm rectangle for the cut.
        Call oSketch.SketchLines.AddAsTwoPointRectangle( _
                    oTransGeom.CreatePoint2d(2, 5.5), _
                    oTransGeom.CreatePoint2d(5, 11))

        ' Create a profile.
        oProfile = oSketch.Profiles.AddForSolid

        ' Create a cut definition object
        Dim oCutDefinition As CutDefinition
        oCutDefinition = oSheetMetalFeatures.CutFeatures.CreateCutDefinition(oProfile)

        ' Set extents to 'Through All'
        Call oCutDefinition.SetThroughAllExtent(PartFeatureExtentDirectionEnum.kNegativeExtentDirection)

        ' Create the cut feature
        Dim oCutFeature As CutFeature
        oCutFeature = oSheetMetalFeatures.CutFeatures.Add(oCutDefinition)

        ' Create a new sketch on the X-Z work plane.
        oSketch = oCompDef.Sketches.Add(oCompDef.WorkPlanes.Item(2))

        ' Draw a 15cm x 10cm rectangle with the corner at (0,0)
        Call oSketch.SketchLines.AddAsTwoPointRectangle( _
                                    oTransGeom.CreatePoint2d(0, 0), _
                                    oTransGeom.CreatePoint2d(-15, 10))

        ' Create a profile.oBendEdgesoBendEdges
        oProfile = oSketch.Profiles.AddForSolid

        oFaceFeatureDefinition = oSheetMetalFeatures.FaceFeatures.CreateFaceFeatureDefinition(oProfile)

        ' Create a face feature.
        oFaceFeature = oSheetMetalFeatures.FaceFeatures.Add(oFaceFeatureDefinition)

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use: The Unfold and Refold feature related calls
    '//
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub FoldPart()

        ' Set a reference to the sheet metal document.
        ' This assumes a part document is active.
        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.ActiveDocument

        ' Make sure the document is a sheet metal document.
        If oPartDoc.SubType <> "{9C464203-9BAE-11D3-8BAD-0060B0CE6BB4}" Then
            MsgBox("A sheet metal document must be open.")
            Exit Sub
        End If

        Dim sheetMetalDef As SheetMetalComponentDefinition
        sheetMetalDef = oPartDoc.ComponentDefinition

        ' Look at the face for a planar face that lies on the X-Y plane.
        Dim face As Face
        Dim baseFace As Face
        baseFace = Nothing
        For Each face In sheetMetalDef.SurfaceBodies.Item(1).Faces
            If face.SurfaceType = SurfaceTypeEnum.kPlaneSurface Then
                If System.Math.Round(face.PointOnFace.Z, 7) = 0 Then
                    baseFace = face
                    Exit For
                End If
            End If
        Next

        Dim sheetMetalFeatures As SheetMetalFeatures
        sheetMetalFeatures = sheetMetalDef.Features

        ' Check to see if a base found was found.
        If Not baseFace Is Nothing Then

            ' Unfold all of the bends so the part is flat.
            Dim unfoldFeature As UnfoldFeature
            unfoldFeature = sheetMetalFeatures.UnfoldFeatures.Add(baseFace)
            'MsgBox "Part unfolded."

            ' Refold each bend, one at a time.
            Dim i As Integer
            For i = 1 To sheetMetalDef.Bends.Count
                'MsgBox "Refolding bend " & i & " of " & sheetMetalDef.bends.count

                ' Add the bend to an ObjectCollection.
                Dim bends As ObjectCollection
                bends = _InvApplication.TransientObjects.CreateObjectCollection
                Call bends.Add(sheetMetalDef.Bends.Item(i))

                ' Create the refold feature.
                Call sheetMetalFeatures.RefoldFeatures.Add(unfoldFeature.StationaryFace, bends)
            Next
        End If

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use: Gets Flat bend info for active doc
    '//
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub GetBendResults()

        ' Set a reference to the sheet metal document.
        ' This assumes a part document is active.
        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.ActiveDocument

        ' Make sure the document is a sheet metal document.
        If oPartDoc.SubType <> "{9C464203-9BAE-11D3-8BAD-0060B0CE6BB4}" Then
            MsgBox("A sheet metal document must be open.")
            Exit Sub
        End If

        Dim oSheetMetalCompDef As SheetMetalComponentDefinition
        oSheetMetalCompDef = oPartDoc.ComponentDefinition

        If (Not oSheetMetalCompDef.HasFlatPattern) Then
            oSheetMetalCompDef.Unfold()
        End If

        Dim oFlatPattern As FlatPattern
        oFlatPattern = oSheetMetalCompDef.FlatPattern

        oFlatPattern.PunchRepresentationType = PunchRepresentationTypeEnum.kFormedFeaturePunchRepresentation 
 
        Dim oBendResult As FlatBendResult
        For Each oBendResult In oFlatPattern.FlatBendResults

            Dim strResult As String
            strResult = "Internal Name: " & oBendResult.InternalName & ", "

            If oBendResult.IsOnBottomFace Then
                strResult = strResult & "On Bottom, "
            Else
                strResult = strResult & "On Top, "
            End If

            strResult = strResult & "Angle: " & _InvApplication.ActiveDocument.UnitsOfMeasure.GetStringFromValue(oBendResult.Angle, UnitsTypeEnum.kDefaultDisplayAngleUnits) & ", "

            strResult = strResult & "Inner Radius: " & _InvApplication.ActiveDocument.UnitsOfMeasure.GetStringFromValue(oBendResult.InnerRadius, UnitsTypeEnum.kDefaultDisplayLengthUnits) & ", "

            If oBendResult.IsDirectionUp Then
                strResult = strResult & "Bend Direction: " & "Bend Up"
            Else
                strResult = strResult & "Bend Direction: " & "Bend Down"
            End If
                      

            Dim oWireEdge As Edge = oBendResult.Edge
            strResult = strResult & "start point of edge (" & _
                                    oWireEdge.StartVertex.Point.X & "  ," & _
                                    oWireEdge.StartVertex.Point.Y & " , " & _
                                    oWireEdge.StartVertex.Point.Z & ")"


             strResult = strResult & "end point of edge (" & _
                                    oWireEdge.StopVertex.Point.X & " , " & _
                                    oWireEdge.StopVertex.Point.Y & " , " & _
                                    oWireEdge.StopVertex.Point.Z & ")"

            oFlatPattern.WorkPoints.AddFixed(_InvApplication.TransientGeometry.CreatePoint(
                                                     oWireEdge.StartVertex.Point.X,
                                                      oWireEdge.StartVertex.Point.Y,
                                                       oWireEdge.StartVertex.Point.Z))
            oFlatPattern.WorkPoints.AddFixed(_InvApplication.TransientGeometry.CreatePoint(
                                                     oWireEdge.StopVertex.Point.X,
                                                      oWireEdge.StopVertex.Point.Y,
                                                       oWireEdge.StopVertex.Point.Z))
 

            Debug.Print(strResult)
        Next

        'Dim oE As Edge
        'For Each oE In oFlatPattern.GetEdgesOfType(FlatPatternEdgeTypeEnum.kBendUpFlatPatternEdge )
        '     oFlatPattern.WorkPoints.AddFixed(_InvApplication.TransientGeometry.CreatePoint(
        '                                             oE.StartVertex.Point.X,
        '                                              oE.StartVertex.Point.Y,
        '                                               oE.StartVertex.Point.Z))
        '    oFlatPattern.WorkPoints.AddFixed(_InvApplication.TransientGeometry.CreatePoint(
        '                                             oE.StopVertex.Point.X,
        '                                              oE.StopVertex.Point.Y,
        '                                               oE.StopVertex.Point.Z))
        'Next

    End Sub


    'User has to select a bend line in a drawing document
    'that belongs to the flat bend pattern of a SheetMetalPart
    Public Sub GetBendResultFromSelectedCurve()

        'Gets the selected curve segment
        Dim oDwCurveSegment As DrawingCurveSegment
        oDwCurveSegment = _InvApplication.ActiveDocument.SelectSet.Item(1)

        'Gets full drawing curve from the segment
        Dim oDrawingCurve As DrawingCurve
        oDrawingCurve = oDwCurveSegment.Parent

        'Gets edge
        Dim oEdge As Edge
        oEdge = oDrawingCurve.ModelGeometry

        'Retrieves component definition from the edge
        Dim oSMDef As SheetMetalComponentDefinition
        oSMDef = oEdge.Parent.ComponentDefinition

        Dim oFlatPattern As FlatPattern
        oFlatPattern = oSMDef.FlatPattern

        'Gets flat bend result corresponding to the edge
        Dim oBendResult As FlatBendResult
        oBendResult = oFlatPattern.FlatBendResults.Item(oEdge)


        'Prints Flat Bend Results
        Debug.Print("---------------- Flat Bend Infos ----------------")

        Debug.Print("Internal Name: " & oBendResult.InternalName)

        If oBendResult.IsOnBottomFace Then
            Debug.Print("Bend On Bottom Face")
        Else
            Debug.Print("Bend On Top Face")
        End If

        Debug.Print("Bend Angle = " & _InvApplication.ActiveDocument.UnitsOfMeasure.GetStringFromValue(oBendResult.Angle, UnitsTypeEnum.kDefaultDisplayAngleUnits))

        Debug.Print("Bend Radius = " & _InvApplication.ActiveDocument.UnitsOfMeasure.GetStringFromValue(oBendResult.InnerRadius, UnitsTypeEnum.kDefaultDisplayLengthUnits))

        If oBendResult.IsDirectionUp Then
            Debug.Print("Bend Direction: " & "Bend Up")
        Else
            Debug.Print("Bend Direction: " & "Bend Down")
        End If

        Debug.Print("-------------------------------------------------")

    End Sub

   

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use:
    '// This program demonstrates the creation of a punch tool feature.
    '// It uses one of the punch features that’s delivered with Inventor.
    '// It assumes you already have an existing sheet metal part and have
    '// selected a face to place the punch feature on.  The selected face
    '// should be large so there is room for the punch features.
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub PlacePunchFeature()

        ' Set a reference to the sheet metal document.
        ' This assumes a part document is active.
        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.ActiveDocument

        ' Make sure the document is a sheet metal document.
        If oPartDoc.SubType <> "{9C464203-9BAE-11D3-8BAD-0060B0CE6BB4}" Then
            MsgBox("A sheet metal document must be open.")
            Exit Sub
        End If

        Dim oSMDef As SheetMetalComponentDefinition
        oSMDef = oPartDoc.ComponentDefinition

        

        ' Get the selected face that will be used for the creation
        ' of the sketch that will contain the sketch points.
        Dim oFace As Face

        Try
            oFace = oPartDoc.SelectSet.Item(1)
        Catch
            MsgBox("A planar face must be selected.")
            Exit Sub
        End Try

        If oFace.SurfaceType <> SurfaceTypeEnum.kPlaneSurface Then
            MsgBox("A planar face must be selected.")
            Exit Sub
        End If

        ' Create a sketch on the selected face.
        Dim oSketch As PlanarSketch
        oSketch = oSMDef.Sketches.Add(oFace)

        ' Create some points on the sketch.  The model will need to
        ' be of a size that these points lie on the model.
        Dim oPoints As ObjectCollection
        oPoints = _InvApplication.TransientObjects.CreateObjectCollection

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        Dim oPoint As SketchPoint
        oPoint = oSketch.SketchPoints.Add(oTG.CreatePoint2d(8, 8), True)
        Call oPoints.Add(oPoint)

        oPoint = oSketch.SketchPoints.Add(oTG.CreatePoint2d(12, 6), True)
        Call oPoints.Add(oPoint)

        oPoint = oSketch.SketchPoints.Add(oTG.CreatePoint2d(10, 10), False)
        Call oPoints.Add(oPoint)

        Dim oSMFeatures As SheetMetalFeatures
        oSMFeatures = oSMDef.Features

        ' Create an iFeatureDefinition object for a punch tool.
        Dim oiFeatureDef As iFeatureDefinition
        oiFeatureDef = oSMFeatures.PunchToolFeatures.CreateiFeatureDefinition(OpenFile("iFeature Def (*.ide)|*.ide"))

        ' Set the input.
        Dim oInput As iFeatureInput
        For Each oInput In oiFeatureDef.iFeatureInputs
            Dim oParamInput As iFeatureParameterInput           
            Select Case oInput.Name               
                Case "Length"
                    oParamInput = oInput
                    oParamInput.Expression = "0.875 in"
                Case "Hole_Diameter"
                    oParamInput = oInput
                    oParamInput.Expression = "0.5 in"
                Case "Slot_Width"
                    oParamInput = oInput
                    oParamInput.Expression = "0.3875 in"
                Case "Fillet"
                    oParamInput = oInput
                    oParamInput.Expression = "0.0625 in"
                Case "Thickness"
                    oParamInput = oInput
                    oParamInput.Expression = "0.125 in"
            End Select
        Next

        ' Create the iFeature at a 45 degree angle.
        Dim oPunchTool As PunchToolFeature
        oPunchTool = oSMFeatures.PunchToolFeatures.Add(oPoints, oiFeatureDef, System.Math.PI / 4)
        
        'oSMDef.ActiveSheetMetalStyle.PunchRepresentationType = PunchRepresentationTypeEnum.kDefaultPunchRepresentation 
    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use:
    '// demonstrates creating an R12 DXF file that will have a layer called "Outer"
    '// where the curves for the outer shape will be created.
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub WriteSheetMetalDXF()

        ' Get the active document.  This assumes it is a part document.
        Dim oDoc As PartDocument
        oDoc = _InvApplication.ActiveDocument

        ' Get the DataIO object.
        Dim oDataIO As DataIO
        oDataIO = oDoc.ComponentDefinition.DataIO

        ' Build the string that defines the format of the DXF file.
        Dim sOut As String
        sOut = "FLAT PATTERN DXF?AcadVersion=R12&OuterProfileLayer=Outer"

        ' Create the DXF file.   
        oDataIO.WriteDataToFile(sOut, "c:\Temp\flat.dxf")

    End Sub

End Class
