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

    '*********** Declare here your public Sub routines ***********

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Sample
    '//
    '// Use: Create a empty Title Block Definition
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub AddTitleBlockDef()

        'Ensures the active document is a drawing
        If _InvApplication.ActiveDocument.DocumentType <> DocumentTypeEnum.kDrawingDocumentObject Then
            MsgBox("A Drawing Document must be active...")
            Exit Sub
        End If

        Dim oDrawDoc As DrawingDocument
        oDrawDoc = _InvApplication.ActiveDocument

        ' get the Title blocks (definitions)
        Dim oTitleBlks As TitleBlockDefinitions
        oTitleBlks = oDrawDoc.TitleBlockDefinitions

        ' add a new title block definition
        Dim oTitleBlk As TitleBlockDefinition
        oTitleBlk = oTitleBlks.Add("My TitleBlock")

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Sample
    '//
    '// Use: Create a Border Definition
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub CreateBorderDefinition()

        Dim oDrawDoc As DrawingDocument
        oDrawDoc = _InvApplication.ActiveDocument

        ' Create the new border definition.
        Dim oBorderDef As BorderDefinition
        oBorderDef = oDrawDoc.BorderDefinitions.Add("My Border")

        ' Open the border definition's sketch for edit.  This is done by calling the Edit
        ' method of the BorderDefinition to obtain a DrawingSketch. 
        Dim oSketch As DrawingSketch = Nothing
        Call oBorderDef.Edit(oSketch)

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        ' Use the functionality of the sketch to add geometry.
        Call oSketch.SketchLines.AddAsTwoPointRectangle(oTG.CreatePoint2d(2, 2), oTG.CreatePoint2d(53.88, 41.18))

        Call oBorderDef.ExitEdit(True)

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Sample 
    '//
    '// Use: Create Drawing Views
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub CreateViews()

        Dim oDrawingDoc As DrawingDocument
        oDrawingDoc = _InvApplication.ActiveDocument

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.Documents.Open("C:\Temp\Part1.ipt", False)

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        ' Create the base view.
        Dim oFrontView As DrawingView
        oFrontView = oDrawingDoc.ActiveSheet.DrawingViews.AddBaseView(oPartDoc, oTG.CreatePoint2d(35, 20), 1, _
                                                     ViewOrientationTypeEnum.kFrontViewOrientation, _
                                                     DrawingViewStyleEnum.kHiddenLineDrawingViewStyle)

        ' Create projected views.
        Dim oRightView As DrawingView
        oRightView = oDrawingDoc.ActiveSheet.DrawingViews.AddProjectedView(oFrontView, oTG.CreatePoint2d(15, 20), _
                                                        DrawingViewStyleEnum.kFromBaseDrawingViewStyle)

        Dim oIsoView As DrawingView
        oIsoView = oDrawingDoc.ActiveSheet.DrawingViews.AddProjectedView(oFrontView, oTG.CreatePoint2d(15, 35), _
                                                         DrawingViewStyleEnum.kHiddenLineRemovedDrawingViewStyle)

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Sample 
    '//
    '// Use: Retrieve dimensions from model sketches
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub RetrievDimensionsFromModel()

        Dim oDrawing As DrawingDocument
        oDrawing = _InvApplication.ActiveDocument

        Dim oView As DrawingView
        oView = oDrawing.ActiveSheet.DrawingViews(1)

        Dim oDimColl As GeneralDimensionsEnumerator
        oDimColl = oDrawing.ActiveSheet.DrawingDimensions.GeneralDimensions.Retrieve(oView)

    End Sub


End Class
