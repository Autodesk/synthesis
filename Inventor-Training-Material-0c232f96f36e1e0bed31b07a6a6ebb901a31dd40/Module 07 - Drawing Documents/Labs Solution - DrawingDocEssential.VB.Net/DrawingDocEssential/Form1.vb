Imports Inventor
Public Class Form1

    Dim mApp As Inventor.Application

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
    End Sub

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            'Attach to the Existing Runnin Inventor Application
            mApp = System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application")

        Catch ex As Exception

            System.Windows.Forms.MessageBox.Show("Error: Inventor must be running...")

            Button1.Enabled = False
            Border2.Enabled = False
            Button3.Enabled = False
            Exit Sub

        End Try
    End Sub

    'Create a  Title block 
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        Dim oDoc As DrawingDocument
        oDoc = mApp.Documents.Add(DocumentTypeEnum.kDrawingDocumentObject, _
                                  mApp.FileManager.GetTemplateFile(DocumentTypeEnum.kDrawingDocumentObject), _
                                  True)

        Dim oSheet As Sheet
        oSheet = oDoc.Sheets.Add(DrawingSheetSizeEnum.kADrawingSheetSize, , "A Size")

        oSheet.AddDefaultBorder()

        Call oSheet.AddTitleBlock(oDoc.TitleBlockDefinitions.Item("ANSI A"))

    End Sub

    'To execute this command, a drawing document with a view need to be available
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Border2.Click

        Dim oDrawDoc As DrawingDocument
        oDrawDoc = mApp.ActiveDocument

        Dim oTG As TransientGeometry
        oTG = mApp.TransientGeometry

        'Create the new border definition
        Dim oBorderDef As BorderDefinition
        oBorderDef = oDrawDoc.BorderDefinitions.Add("Sample Border")

        'Open the border definition's sketch for edit.  This is done by calling the Edit
        ' method of the BorderDefinition to obtain a DrawingSketch.  This actually creates
        ' a copy of the border definition's and opens it for edit.
        Dim oSketch As DrawingSketch = Nothing

        Call oBorderDef.Edit(oSketch)

        'Use the functionality of the sketch to add geometry
        oSketch.SketchLines.AddAsTwoPointRectangle(oTG.CreatePoint2d(2, 2), oTG.CreatePoint2d(25.94, 19.59))

        Call oBorderDef.ExitEdit(True)

    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click

        ' Create a new drawing document.
        Dim oDoc As DrawingDocument
        oDoc = mApp.Documents.Add(DocumentTypeEnum.kDrawingDocumentObject, _
                                mApp.FileManager.GetTemplateFile(DocumentTypeEnum.kDrawingDocumentObject))

        ' Create a new B size sheet.
        Dim oSheet As Sheet
        oSheet = oDoc.Sheets.Add(DrawingSheetSizeEnum.kBDrawingSheetSize)

        ' Add the default border.
        oSheet.AddDefaultBorder()

        ' Add ANSI A TitleBlock
        Dim oTitleBlock As Inventor.TitleBlock = Nothing
        oTitleBlock = oSheet.AddTitleBlock(oDoc.TitleBlockDefinitions.Item("ANSI A"))

        ' Open the block document, invisibly.
        Dim oBlockPart As PartDocument
        oBlockPart = mApp.Documents.Open("C:\Temp\TestPart.ipt", False)

        Dim oTG As TransientGeometry
        oTG = mApp.TransientGeometry

        'Create base drawing view
        Dim oBaseView As DrawingView
        oBaseView = oSheet.DrawingViews.AddBaseView(oBlockPart, oTG.CreatePoint2d(10, 10), 1, _
                                                    ViewOrientationTypeEnum.kFrontViewOrientation, _
                                                    DrawingViewStyleEnum.kHiddenLineDrawingViewStyle)

        ' Create Projected views
        Dim oRightView As DrawingView
        oRightView = oSheet.DrawingViews.AddProjectedView(oBaseView, oTG.CreatePoint2d(20, 18), _
                                                         DrawingViewStyleEnum.kFromBaseDrawingViewStyle)

        Dim oIsoView As DrawingView
        oIsoView = oSheet.DrawingViews.AddProjectedView(oBaseView, oTG.CreatePoint2d(10, 20), _
                                                        DrawingViewStyleEnum.kFromBaseDrawingViewStyle)


        ' Find an edge in the part to dimension.  Any method can be used, (attributes, B-Rep query, selection, etc.).  This
        ' looks through the curves in the drawing view and finds the top horizontal curve.

        Dim oSelectedCurve As DrawingCurve = Nothing

        For Each oCurve As DrawingCurve In oBaseView.DrawingCurves

            ' Skip Circles
            If Not oCurve.StartPoint Is Nothing And Not oCurve.EndPoint Is Nothing Then

                If (WithinTol(oCurve.StartPoint.X, oCurve.EndPoint.X, 0.001)) Then

                    If oSelectedCurve Is Nothing Then

                        ' This is the first horizontal curve found.
                        oSelectedCurve = oCurve

                    Else

                        ' Check to see if this curve is higher (smaller x value) than the current selected
                        If oCurve.MidPoint.X < oSelectedCurve.MidPoint.X Then
                            oSelectedCurve = oCurve
                        End If

                    End If

                End If

            End If
        Next

        If oSelectedCurve is Nothing then
            MessageBox.Show("no curve selected!")
            Exit Sub 
        End If

        ' Create geometry intents point for the curve.

        Dim oGeomIntent1 As GeometryIntent = oSheet.CreateGeometryIntent(oSelectedCurve, PointIntentEnum.kStartPointIntent)
        Dim oGeomIntent2 As GeometryIntent = oSheet.CreateGeometryIntent(oSelectedCurve, PointIntentEnum.kEndPointIntent)

        Dim oGeneralDimensions = oSheet.DrawingDimensions.GeneralDimensions

        Dim oDimPos As Point2d = oTG.CreatePoint2d(oSelectedCurve.MidPoint.X - 2, oSelectedCurve.MidPoint.Y)

        ' Create the dimension.
        Dim oLinearDim As LinearGeneralDimension
        oLinearDim = oGeneralDimensions.AddLinear(oDimPos, oGeomIntent1, oGeomIntent2, DimensionTypeEnum.kAlignedDimensionType)

    End Sub

    Private Function WithinTol(ByVal Value1 As Double, ByVal Value2 As Double, ByVal tol As Double) As Boolean

        Return (Math.Abs(Value1 - Value2) < tol)

    End Function

End Class
