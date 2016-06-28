Public Class Form2
    Public Sub New(ByVal filename As String)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        AxInventorViewControl1.FileName = filename

    End Sub


    Private Sub FormViewer_SizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.SizeChanged

        AxInventorViewControl1.Size = Me.Size

    End Sub
End Class