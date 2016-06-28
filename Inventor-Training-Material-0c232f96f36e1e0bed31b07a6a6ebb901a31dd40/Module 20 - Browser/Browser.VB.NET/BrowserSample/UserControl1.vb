Imports Inventor



Public Class UserControl1

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        MsgBox("The control in my pane is clicked !")
    End Sub

    Public Sub DrawASketchRectangle(ByVal InvApp As Inventor.Application)

        Dim oPartDef As PartComponentDefinition = InvApp.ActiveDocument.ComponentDefinition

        Dim oPoint1 As Point2d = InvApp.TransientGeometry.CreatePoint2d(0, 0)
        Dim oPoint2 As Point2d = InvApp.TransientGeometry.CreatePoint2d(10, 10)

        oPartDef.Sketches(1).SketchLines.AddAsTwoPointRectangle(oPoint1, oPoint2)

        InvApp.ActiveDocument.Views(1).Fit()

    End Sub

End Class
