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

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Sample
    '//
    '// Use: BRep Traversal. Counts number of planar faces in each FaceShell
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub BRepTraversal()

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.ActiveDocument

        ' Iterate through the FaceShell objects.
        Dim iShellCount As Integer
        Dim oShell As FaceShell

        For Each oShell In oPartDoc.ComponentDefinition.SurfaceBodies.Item(1).FaceShells
            iShellCount = iShellCount + 1

            ' Iterate over the faces in this shell.
            Dim iPlanarFaceCount As Integer
            iPlanarFaceCount = 0

            Dim oFace As Face
            For Each oFace In oShell.Faces
                ' Check to see if the face is planar.
                If oFace.SurfaceType = SurfaceTypeEnum.kPlaneSurface Then
                    iPlanarFaceCount = iPlanarFaceCount + 1
                End If
            Next

            MessageBox.Show("FaceShell[" & iShellCount & "] has " & iPlanarFaceCount & " planar faces.")
        Next
    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Sample
    '//
    '// Use: Get Face at specified point
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub GetFaceAtPoint()

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.ActiveDocument

        Dim oBody As SurfaceBody
        oBody = oPartDoc.ComponentDefinition.SurfaceBodies.Item(1)

        Dim oPoint As Point
        oPoint = _InvApplication.TransientGeometry.CreatePoint(0, 0, 0)

        Dim oFace As Face

        Try
            oFace = oBody.LocateUsingPoint(ObjectTypeEnum.kFaceObject, oPoint)
            MsgBox("Found face with area " & oFace.Evaluator.Area & " cm^2")
        Catch
            MsgBox("No face at the specified point.")
        End Try

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Sample
    '//
    '// Use: Find using ray sample
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub FindUsingRay()

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.ActiveDocument

        Dim oBody As SurfaceBody
        oBody = oPartDoc.ComponentDefinition.SurfaceBodies.Item(1)

        Dim oTg As TransientGeometry
        oTg = _InvApplication.TransientGeometry

        Dim oFoundEnts As ObjectsEnumerator = Nothing
        Dim oLocPoints As ObjectsEnumerator = Nothing

        Call oBody.FindUsingRay(oTg.CreatePoint(-5, 0, 0), _
                            oTg.CreateUnitVector(1, 0, 0), 0.00001, _
                            oFoundEnts, oLocPoints, True)

        If oFoundEnts.Count > 0 Then
            MsgBox("Found " & oFoundEnts.Count & " Entities")
        Else
            MsgBox("No entity along the specified ray.")
        End If

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Sample
    '//
    '// Use: Geometry Evaluator. Draws selected surface normals
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub FaceParametricNormals()

        Dim oFace As Face
        oFace = _InvApplication.ActiveDocument.SelectSet(1)

        Dim oEvaluator As SurfaceEvaluator
        oEvaluator = oFace.Evaluator

        Dim oParamRBox As Box2d
        oParamRBox = oEvaluator.ParamRangeRect

        Dim uStep As Double
        Dim vStep As Double

        uStep = System.Math.Round((oParamRBox.MaxPoint.X - oParamRBox.MinPoint.X) / 9, 10)
        vStep = System.Math.Round((oParamRBox.MaxPoint.Y - oParamRBox.MinPoint.Y) / 9, 10)

        Dim u As Long
        Dim v As Long

        For u = 0 To 9

            For v = 0 To 9

                Dim params(1) As Double
                params(0) = oParamRBox.MinPoint.X + u * uStep
                params(1) = oParamRBox.MinPoint.Y + v * vStep

                Dim normal(2) As Double
                Call oEvaluator.GetNormal(params, normal)


                Dim startPoint(2) As Double
                Call oEvaluator.GetPointAtParam(params, startPoint)

                Dim endPoint(2) As Double
                endPoint(0) = startPoint(0) + normal(0)
                endPoint(1) = startPoint(1) + normal(1)
                endPoint(2) = startPoint(2) + normal(2)

                'See client Graphics module
                ClientGraphicUtility.DrawLineTestDb(startPoint, endPoint)

            Next
        Next

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use: Exporting Inventor Geometry
    '//
    '//
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub GenerateFacets()

        Dim oDoc As Document
        oDoc = _InvApplication.ActiveDocument

        Dim oSurfBody As SurfaceBody
        oSurfBody = oDoc.ComponentDefinition.SurfaceBodies(1)

        Dim Tolerance As Double
        Dim VertexCount As Long
        Dim FacetCount As Long
        Dim VertexCoords(0) As Double
        Dim Normals(0) As Double
        Dim Indices(0) As Integer

        Tolerance = 0.01
        Call oSurfBody.CalculateFacets(Tolerance, VertexCount, FacetCount, VertexCoords, Normals, Indices)

        Dim i As Integer
        For i = 0 To UBound(Indices) Step 3

            Dim vIndex1 As Integer
            Dim vIndex2 As Integer
            Dim vIndex3 As Integer

            vIndex1 = (Indices(i) - 1) * 3
            vIndex2 = (Indices(i + 1) - 1) * 3
            vIndex3 = (Indices(i + 2) - 1) * 3

            Dim vertex1(2) As Double
            Dim vertex2(2) As Double
            Dim vertex3(2) As Double

            vertex1(0) = VertexCoords(vIndex1)
            vertex1(1) = VertexCoords(vIndex1 + 1)
            vertex1(2) = VertexCoords(vIndex1 + 2)

            vertex2(0) = VertexCoords(vIndex2)
            vertex2(1) = VertexCoords(vIndex2 + 1)
            vertex2(2) = VertexCoords(vIndex2 + 2)

            vertex3(0) = VertexCoords(vIndex3)
            vertex3(1) = VertexCoords(vIndex3 + 1)
            vertex3(2) = VertexCoords(vIndex3 + 2)

            Dim pt1 As Point
            Dim pt2 As Point
            Dim pt3 As Point

            pt1 = _InvApplication.TransientGeometry.CreatePoint(vertex1(0), vertex1(1), vertex1(2))
            pt2 = _InvApplication.TransientGeometry.CreatePoint(vertex2(0), vertex2(1), vertex2(2))
            pt3 = _InvApplication.TransientGeometry.CreatePoint(vertex3(0), vertex3(1), vertex3(2))

            'See client Graphics module
            Call ClientGraphicUtility.DrawLineTest(pt1, pt2)
            Call ClientGraphicUtility.DrawLineTest(pt2, pt3)
            Call ClientGraphicUtility.DrawLineTest(pt3, pt1)

            _InvApplication.ActiveView.Update()

        Next

        'cleanCGTest

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Use: Clean Client Graphics
    '//
    '//
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub CleanClientGraphics()
        ClientGraphicUtility.cleanCGTest()
    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Sample
    '//
    '// Use: Create a non parametric Feature based on a Transient Brep object
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub CreateBRep()

        ' Create a new part document, using the default part template.
        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _
                    _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject))

        ' Set a reference to the component definition.
        Dim oCompDef As PartComponentDefinition
        oCompDef = oPartDoc.ComponentDefinition

        ' Set a reference to the TransientBRep object.
        Dim oTransientBRep As TransientBRep
        oTransientBRep = _InvApplication.TransientBRep

        ' Create a range box that will define the extents of a block.
        Dim oBox As Box
        oBox = _InvApplication.TransientGeometry.CreateBox

        ' Expand in all directions by 1 cm.
        oBox.Expand(1)

        ' Create the block.
        Dim oBody As SurfaceBody
        oBody = oTransientBRep.CreateSolidBlock(oBox)

        ' Create bottom and top points for a cylinder.
        Dim oBottomPt As Point
        oBottomPt = _InvApplication.TransientGeometry.CreatePoint(0, 1, 0)

        Dim oTopPt As Point
        oTopPt = _InvApplication.TransientGeometry.CreatePoint(0, 3, 0)

        ' Create the cylinder body.
        Dim oCylinder As SurfaceBody
        oCylinder = oTransientBRep.CreateSolidCylinderCone(oBottomPt, oTopPt, 0.5, 0.5, 0.5)

        ' Boolean the bodies; "oBody" will return the result
        Call oTransientBRep.DoBoolean(oBody, oCylinder, BooleanTypeEnum.kBooleanTypeUnion)

        ' Create a base feature with the result body.
        Dim oBaseFeature As NonParametricBaseFeature
        oBaseFeature = oCompDef.Features.NonParametricBaseFeatures.Add(oBody)

    End Sub

End Class
