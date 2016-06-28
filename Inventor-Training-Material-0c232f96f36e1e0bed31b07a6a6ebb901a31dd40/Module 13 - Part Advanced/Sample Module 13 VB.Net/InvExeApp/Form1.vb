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

'''macro class
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

    '*********** Declare here your public Sub routines ***********

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Sample
    '//
    '// Use: iFeature Insertion
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub iFeatureInsertion()

        Dim oCompDef As PartComponentDefinition
        oCompDef = _InvApplication.ActiveDocument.ComponentDefinition

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        ' Arbitrarily get the start face of the first extrusion.
        Dim oFace As Face
        oFace = oCompDef.Features.ExtrudeFeatures.Item(1).EndFaces.Item(1)

        ' Create the definition object for the specified ide file.


        Dim oideFile As String = OpenFile("(*.ide)|*.ide")
        ' Create the definition object for the specified ide file.
        If (String.IsNullOrEmpty(oideFile)) Then
            MessageBox.Show("no ide file is  selected!")
            Exit Sub
        End If
        Dim oiFeatDef As iFeatureDefinition
        oiFeatDef = oCompDef.Features.iFeatures.CreateiFeatureDefinition(oideFile)

        ' Set the iFeature input values.
        Dim oiFeatInput As iFeatureInput
        Dim oParamInput As iFeatureParameterInput

        For Each oiFeatInput In oiFeatDef.iFeatureInputs

            Select Case oiFeatInput.Name

                Case "Sketch Plane"
                    Dim oPlaneInput As iFeatureSketchPlaneInput
                    oPlaneInput = oiFeatInput
                    oPlaneInput.PlaneInput = oFace
                    Call oPlaneInput.SetPosition(oTG.CreatePoint(5, 5, 0.1), oTG.CreateVector(1, 0, 0), 0)

                Case "Diameter"
                    oParamInput = oiFeatInput
                    oParamInput.Expression = ".7 in"

                Case "Height"
                    oParamInput = oiFeatInput
                    oParamInput.Expression = "10.0 in"

                Case "Radius"
                    oParamInput = oiFeatInput
                    oParamInput.Expression = ".5 in"

            End Select
        Next

        ' Create the iFeature.
        Call oCompDef.Features.iFeatures.Add(oiFeatDef)

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Sample
    '//
    '// Use: iFeature Table-Driven
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub PlaceTableDriveniFeature()

        ' Get the part document and component definition of the active document.
        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.ActiveDocument
        
        Dim oPartDef As PartComponentDefinition
        oPartDef = oPartDoc.ComponentDefinition

        ' Get the selected face to use as input for the iFeature.
        Dim oFace As Face
        oFace = oPartDoc.SelectSet.Item(1)
       
        If oFace.SurfaceType <> SurfaceTypeEnum.kPlaneSurface Then
            MsgBox("A planar face must be selected.")
            Exit Sub
        End If

        Dim oFeatures As PartFeatures
        oFeatures = oPartDef.Features

        Dim oideFile As String = OpenFile("(*.ide)|*.ide")
        ' Create the definition object for the specified ide file.
        If (String.IsNullOrEmpty(oideFile)) Then
            MessageBox.Show("no ide file is  selected!")
            Exit Sub
        End If
        Dim oiFeatureDef As iFeatureDefinition
        oiFeatureDef = oPartDef.Features.iFeatures.CreateiFeatureDefinition(oideFile)

        ' Set the input, which in this case is only the sketch plane
        ' since the other input comes from the table.  The parameter input
        ' should not be available at this point since it can't be changed
        ' and is controlled by the table.
        '
        ' When an existing table driven iFeature is accessed then this should
        ' include the parameters so the programmer has access to the corresponding
        ' reference parameter that's created.
        Dim bFoundPlaneInput As Boolean
        bFoundPlaneInput = False
        Dim oInput As iFeatureInput
        For Each oInput In oiFeatureDef.iFeatureInputs

            Select Case oInput.Name
                Case "Profile Plane1"
                    Dim oPlaneInput As iFeatureSketchPlaneInput
                    oPlaneInput = oInput
                    oPlaneInput.PlaneInput = oFace
                    bFoundPlaneInput = True
            End Select
        Next

        If Not bFoundPlaneInput Then
            MsgBox("The ide file does not contain an iFeature input named ""Profile Plane1"".")
            Exit Sub
        End If

        '** Look through the table to find the row where "Size" is "4.5".
        Dim oTable As iFeatureTable
        oTable = oiFeatureDef.iFeatureTable

        ' Find the "Size" column.
        Dim oColumn As iFeatureTableColumn
        Dim bFoundSize As Boolean
        bFoundSize = False
        For Each oColumn In oTable.iFeatureTableColumns
            If oColumn.DisplayHeading = "Size" Then
                bFoundSize = True
                Exit For
            End If
        Next

        If Not bFoundSize Then
            MsgBox("The column ""Size"" was not found.")
            Exit Sub
        End If

        ' Find the row in the "Size" column with the value of "4.5"
        Dim oCell As iFeatureTableCell
        bFoundSize = False
        For Each oCell In oColumn
            If oCell.Value = "4.5" Then
                bFoundSize = True
                Exit For
            End If
        Next

        If Not bFoundSize Then
            MsgBox("The cell with value ""4.5"" was not found.")
            Exit Sub
        End If

        ' Set this row as the active row.
        oiFeatureDef.ActiveTableRow = oCell.Row

        ' Create the iFeature.
        Dim oiFeature As iFeature
        oiFeature = oFeatures.iFeatures.Add(oiFeatureDef)

    End Sub 

    Public Sub DerivedPartExample()

        Dim oCompDef As PartComponentDefinition
        oCompDef = _InvApplication.ActiveDocument.ComponentDefinition

        Dim oDerivedPartComps As DerivedPartComponents
        oDerivedPartComps = oCompDef.ReferenceComponents.DerivedPartComponents

        'derive from the sample Part1.ipt

        '1. Create the definition corresponding to the desired 
        ' type of derived part or assembly
        Dim oDerivedPartDef As DerivedPartUniformScaleDef
        oDerivedPartDef = oDerivedPartComps.CreateUniformScaleDef( _
                                                            "C:\Temp\Part1.ipt")
        '2. Set the various inputs.
        oDerivedPartDef.ScaleFactor = 0.75

        ' 3. Use the definition to create the derived component.
        Dim oDerivedComp As DerivedPartComponent
        oDerivedComp = oDerivedPartComps.Add(oDerivedPartDef) 

    End Sub

    '///////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use: Derive part with DeriveStyle control
    '//
    '//
    '///////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub CreateDerivedPart()

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _
                   _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject), _
                   True)

        Dim oDerivedPartComps As DerivedPartComponents
        oDerivedPartComps = oPartDoc.ComponentDefinition.ReferenceComponents.DerivedPartComponents

        ' assume there is one part named  PartRef.ipt in C:\temp\
        Dim oDerivedPartDef As DerivedPartUniformScaleDef
        oDerivedPartDef = oDerivedPartComps.CreateUniformScaleDef("C:\Temp\PartRef.ipt")
        oDerivedPartDef.ScaleFactor = 0.75

        oDerivedPartDef.IncludeAll()

        ' other properties
        'oDerivedPartDef.ExcludeAll
        'oDerivedPartDef.IncludeAlliMateDefinitions
        'oDerivedPartDef.IncludeAllSurfaces

        Dim oDerivedEntity As DerivedPartEntity
        For Each oDerivedEntity In oDerivedPartDef.Parameters

            If (TypeOf oDerivedEntity.ReferencedEntity Is Parameter) Then

                Dim oParameter As Parameter
                oParameter = oDerivedEntity.ReferencedEntity
                Debug.Print("Derived Parameter: " & oParameter.Name)

                'oDerivedEntity.IncludeEntity = False
            End If

        Next

        oDerivedPartDef.DeriveStyle = DerivedComponentStyleEnum.kDeriveAsSingleBodyNoSeams

        Dim oDerivedComp As DerivedPartComponent
        oDerivedComp = oDerivedPartComps.Add(oDerivedPartDef)

    End Sub

    '//////////////////////////////////////////////////////////////////////////
    '// 
    '// Use: Small utility method. Dumps the contents of the iTable
    '//////////////////////////////////////////////////////////////////////////
    Public Sub DumpiTable()

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.ActiveDocument

        If (Not oPartDoc.ComponentDefinition.IsiPartFactory) Then Exit Sub

        Dim oFactory As iPartFactory
        oFactory = oPartDoc.ComponentDefinition.iPartFactory

        Dim oWS As Microsoft.Office.Interop.Excel.Worksheet
        oWS = oFactory.ExcelWorkSheet

        Dim rowIdx As Integer
        Dim colIdx As Integer

        For rowIdx = 1 To oFactory.TableRows.Count + 1

            Debug.Print("-------------------------------------------------")

            For colIdx = 1 To oFactory.TableColumns.Count

                Debug.Print("Cell[" & rowIdx & "," & colIdx & "]: " & oWS.Cells(rowIdx, colIdx).Text)

            Next
        Next

    End Sub

    '//////////////////////////////////////////////////////////////////////////
    '//
    '// Use: Creation of an iPart from scratch
    '//////////////////////////////////////////////////////////////////////////
    Public Sub CreateiPart()

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _
                   _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject), _
                   True)

        oPartDoc.SaveAs("c:\Temp\APIiPart.ipt", False)

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        Dim oCompDef As PartComponentDefinition
        oCompDef = oPartDoc.ComponentDefinition

        Dim oPoints(3) As Point2d

        oPoints(0) = oTG.CreatePoint2d(0, 0)
        oPoints(1) = oTG.CreatePoint2d(5, 0)
        oPoints(2) = oTG.CreatePoint2d(5, 5)
        oPoints(3) = oTG.CreatePoint2d(0, 5)

        Dim oSketch As PlanarSketch
        oSketch = oCompDef.Sketches.Add(oCompDef.WorkPlanes(1))

        Dim oLines(3) As SketchLine

        oLines(0) = oSketch.SketchLines.AddByTwoPoints(oPoints(0), oPoints(1))
        oLines(1) = oSketch.SketchLines.AddByTwoPoints(oLines(0).EndSketchPoint, oPoints(2))
        oLines(2) = oSketch.SketchLines.AddByTwoPoints(oLines(1).EndSketchPoint, oPoints(3))
        oLines(3) = oSketch.SketchLines.AddByTwoPoints(oLines(2).EndSketchPoint, oLines(0).StartSketchPoint)

        Dim oProfile As Profile
        oProfile = oSketch.Profiles.AddForSolid

        Dim oExtrude As ExtrudeFeature
        oExtrude = oCompDef.Features.ExtrudeFeatures.AddByDistanceExtent(oProfile, 15, _
                        PartFeatureExtentDirectionEnum.kPositiveExtentDirection, _
                        PartFeatureOperationEnum.kNewBodyOperation, 0)

        oExtrude.FeatureDimensions(1).Parameter.Name = "Length"
        oExtrude.FeatureDimensions(2).Parameter.Name = "TaperAngle"

        Dim oFactory As iPartFactory
        oFactory = oCompDef.CreateFactory

        Dim oWS As Microsoft.Office.Interop.Excel.Worksheet
        oWS = oFactory.ExcelWorkSheet

        oWS.Cells(1, 1) = "Member<defaultRow>1</defaultRow><filename></filename>"
        oWS.Cells(1, 2) = "Part Number [Project]"
        oWS.Cells(1, 3) = "Length<free>150 mm</free>"
        oWS.Cells(1, 4) = "TaperAngle"

        oWS.Cells(2, 1) = "APIiPart-01"
        oWS.Cells(2, 2) = "APIiPart-01"
        oWS.Cells(2, 3) = "150 mm"
        oWS.Cells(2, 4) = "0 deg"

        oWS.Cells(3, 1) = "APIiPart-02"
        oWS.Cells(3, 2) = "APIiPart-02"
        oWS.Cells(3, 3) = "100 mm"
        oWS.Cells(3, 4) = "5 deg"

        oWS.Cells(4, 1) = "APIiPart-03"
        oWS.Cells(4, 2) = "APIiPart-03"
        oWS.Cells(4, 3) = "50 mm"
        oWS.Cells(4, 4) = "10 deg"

        Dim oWB As Microsoft.Office.Interop.Excel.Workbook
        oWB = oWS.Parent

        oWB.Save()
        oWB.Close()

        oPartDoc.Update()
        oPartDoc.Save()

    End Sub

    '//////////////////////////////////////////////////////////////////////////
    '//
    '// Use: Add a new row to the factory
    '//////////////////////////////////////////////////////////////////////////
    Public Sub AddRow()

        Dim oFactoryDoc As PartDocument
        oFactoryDoc = _InvApplication.ActiveDocument

        If (oFactoryDoc.ComponentDefinition.iPartFactory Is Nothing) Then
            MsgBox("Not an iPart document...")
            Exit Sub
        End If

        Dim oFactory As iPartFactory
        oFactory = oFactoryDoc.ComponentDefinition.iPartFactory

        Dim oWorksheet As Microsoft.Office.Interop.Excel.Worksheet
        oWorksheet = oFactory.ExcelWorkSheet

        Dim newRowIndex As Long
        newRowIndex = oFactory.TableRows.Count + 1

        'Write new row values
        oWorksheet.Cells(newRowIndex + 1, 1) = "Factory-0" + newRowIndex.ToString()
        oWorksheet.Cells(newRowIndex + 1, 2) = "Factory-0" + newRowIndex.ToString()
        oWorksheet.Cells(newRowIndex + 1, 3) = "15 mm"
        oWorksheet.Cells(newRowIndex + 1, 4) = "15 mm"
        oWorksheet.Cells(newRowIndex + 1, 5) = "5 mm"

        Dim oWorkbook As Microsoft.Office.Interop.Excel.Workbook
        oWorkbook = oWorksheet.Parent

        Dim oXlsApp As Microsoft.Office.Interop.Excel.Application
        oXlsApp = oWorkbook.Parent

        oWorkbook.Save()
        oWorkbook.Close()

        oWorksheet = Nothing
        oWorkbook = Nothing

        'Set our new row as default
        oFactory.DefaultRow = oFactory.TableRows(newRowIndex)
        oFactoryDoc.Save()

        'oFactoryDoc.close

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use: Multi Solid Bodies
    '//
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub CreateMultiBodies()

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _
                   _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject), _
                   True)

        Dim oPartDef As PartComponentDefinition
        oPartDef = oPartDoc.ComponentDefinition

        ' Create a sketch.
        Dim oSketch As PlanarSketch
        oSketch = oPartDef.Sketches.Add(oPartDef.WorkPlanes.Item(3))

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        ' Draw a rectangle and extrude it to create a new body.
        ' *** The kNewBodyOperation type of operation is new in Inventor 2010.
        Call oSketch.SketchLines.AddAsTwoPointRectangle( _
            oTG.CreatePoint2d(-3, -2), _
            oTG.CreatePoint2d(3, 2))

        Dim oProfile As Profile
        oProfile = oSketch.Profiles.AddForSolid

        Dim oExtrude As ExtrudeFeature
        oExtrude = oPartDef.Features.ExtrudeFeatures.AddByDistanceExtent( _
            oProfile, 2, PartFeatureExtentDirectionEnum.kNegativeExtentDirection, _
            PartFeatureOperationEnum.kNewBodyOperation)

        Dim oRenderStyle As RenderStyle = oPartDoc.RenderStyles("Glass (Limo Tint)")
        oPartDef.SurfaceBodies(1).SetRenderStyle(StyleSourceTypeEnum.kOverrideRenderStyle, oRenderStyle)

        ' Create a second sketch.
        oSketch = oPartDef.Sketches.Add(oPartDef.WorkPlanes.Item(3))

        ' *** The kNewBodyOperation type of operation is new in Inventor 2010.
        ' Draw a rectangle and extrude it to create a new body.
        Call oSketch.SketchLines.AddAsTwoPointRectangle( _
            oTG.CreatePoint2d(-2.5, -1.5), _
            oTG.CreatePoint2d(2.5, 1.5))

        oProfile = oSketch.Profiles.AddForSolid
        oExtrude = oPartDef.Features.ExtrudeFeatures.AddByDistanceExtent( _
            oProfile, 1.5, PartFeatureExtentDirectionEnum.kNegativeExtentDirection, _
            PartFeatureOperationEnum.kNewBodyOperation, "-5 deg")
 

    End Sub



    ''' <summary>
    ''' Create move feature 
    ''' assume a part document with one solid is opened
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CreateMoveFeature()

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.ActiveDocument

        Dim oPartDef As PartComponentDefinition
        oPartDef = oPartDoc.ComponentDefinition

        'collect the faces to move
        Dim oSolidColl As ObjectCollection
        oSolidColl = _InvApplication.TransientObjects.CreateObjectCollection()

        'add the first solid
        oSolidColl.Add(oPartDef.SurfaceBodies(1))

        'create move feature definition
        Dim oMoveFDef As MoveDefinition
        oMoveFDef = oPartDef.Features.MoveFeatures.CreateMoveDefinition(oSolidColl)

        ' Define a rotate, move, and free drag.
        oMoveFDef.AddRotateAboutAxis(oPartDef.WorkAxes(3), True, 3.14159265358979 / 4)
        oMoveFDef.AddMoveAlongRay(oPartDef.WorkAxes(1), True, 3)
        oMoveFDef.AddFreeDrag(0.5, 6, 3)

        'create the move feature
        Dim oMoveF As MoveFeature
        oMoveF = oPartDef.Features.MoveFeatures.Add(oMoveFDef)

    End Sub
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Sample
    '//
    '// Use: Combines bodies created by previous sample
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub CombineBodies()

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.ActiveDocument

        Dim oPartDef As PartComponentDefinition
        oPartDef = oPartDoc.ComponentDefinition

        'add first body
        Dim oBaseBody As SurfaceBody
        oBaseBody = oPartDef.SurfaceBodies(1)

        Dim oToolBodies As ObjectCollection
        oToolBodies = _InvApplication.TransientObjects.CreateObjectCollection

        'add second body
        oToolBodies.Add(oPartDef.SurfaceBodies(2))

        Dim oCombineFeature As CombineFeature
        oCombineFeature = oPartDef.Features.CombineFeatures.Add( _
              oBaseBody, _
     oToolBodies, _
     PartFeatureOperationEnum.kCutOperation, _
     False)

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Sample
    '//
    '// Use: Demonstrates use of "Feature.SetAffectedBodies" method
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub FeatureMultibody()

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _
                   _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject), _
                   True)

        Dim oPartDef As PartComponentDefinition
        oPartDef = oPartDoc.ComponentDefinition

        ' Create a sketch.
        Dim oSketch As PlanarSketch
        oSketch = oPartDef.Sketches.Add(oPartDef.WorkPlanes.Item(3))

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        ' *** The kNewBodyOperation type of operation is new in Inventor 2010.
        ' Draw a rectangle and oExtrude it to create a new body.
        Call oSketch.SketchLines.AddAsTwoPointRectangle( _
            oTG.CreatePoint2d(-4, -2), _
            oTG.CreatePoint2d(-3, 2))

        Dim oProfile As Profile
        oProfile = oSketch.Profiles.AddForSolid

        Dim oExtrude As ExtrudeFeature
        oExtrude = oPartDef.Features.ExtrudeFeatures.AddByDistanceExtent( _
            oProfile, 2, PartFeatureExtentDirectionEnum.kSymmetricExtentDirection, _
            PartFeatureOperationEnum.kNewBodyOperation)

        ' Create a second sketch.
        oSketch = oPartDef.Sketches.Add(oPartDef.WorkPlanes.Item(3))

        ' *** The kNewBodyOperation type of operation is new in Inventor 2010.
        ' Draw a rectangle and oExtrude it to create a new body.
        Call oSketch.SketchLines.AddAsTwoPointRectangle( _
            oTG.CreatePoint2d(-2, -2), _
            oTG.CreatePoint2d(-1, 2))

        oProfile = oSketch.Profiles.AddForSolid
        oExtrude = oPartDef.Features.ExtrudeFeatures.AddByDistanceExtent( _
            oProfile, 2, PartFeatureExtentDirectionEnum.kSymmetricExtentDirection, _
            PartFeatureOperationEnum.kNewBodyOperation)

        ' Create a third sketch.
        oSketch = oPartDef.Sketches.Add(oPartDef.WorkPlanes.Item(1))

        ' Draw a circle and oExtrude it to cut through the existing bodies.
        oSketch.SketchCircles.AddByCenterRadius(oTG.CreatePoint2d(0, 0), 0.5)

        oProfile = oSketch.Profiles.AddForSolid
        oExtrude = oPartDef.Features.ExtrudeFeatures.AddByThroughAllExtent( _
            oProfile, PartFeatureExtentDirectionEnum.kNegativeExtentDirection, _
            PartFeatureOperationEnum.kCutOperation)

        ' *** The SurfaceBodies property on the feature
        ' *** and the SetAffectedBodies method are new in Inventor 2010.
        ' Check to see if the feature affected all bodies.
        If oExtrude.SurfaceBodies.Count <> oPartDef.SurfaceBodies.Count Then
            Dim bodies As ObjectCollection
            bodies = _InvApplication.TransientObjects.CreateObjectCollection
            Dim body As SurfaceBody
            For Each body In oPartDef.SurfaceBodies
                bodies.Add(body)
            Next
            Call oExtrude.SetAffectedBodies(bodies)
        End If

    End Sub


   
End Class
