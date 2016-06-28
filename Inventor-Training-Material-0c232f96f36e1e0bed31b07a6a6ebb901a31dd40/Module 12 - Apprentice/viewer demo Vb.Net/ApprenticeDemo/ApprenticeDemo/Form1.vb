Public Class Form1
    Dim mFilename As String
    Private Sub btnOpen_Click(sender As System.Object, e As System.EventArgs) Handles btnOpen.Click

        mFilename = OpenFile()

        If (mFilename <> "") Then
            AxInventorViewControl1.FileName = mFilename
        End If
    End Sub

    Private Function OpenFile() As String

        Dim filename As String = ""

        Dim ofDlg As System.Windows.Forms.OpenFileDialog = New System.Windows.Forms.OpenFileDialog()

        Dim user As String = System.Windows.Forms.SystemInformation.UserName

        ofDlg.Title = "Open Inventor File"
        ofDlg.InitialDirectory = "C:\Documents and Settings\" + user + "\Desktop\"

        '"Inventor Files (*.ipt)|*.ipt|Inventor Assemblies (*.iam)|*.iam|Inventor Drawings (*.idw)|*.idw"
        ofDlg.Filter = "Inventor files (*.ipt; *.iam; *.idw)|*.ipt;*.iam;*.idw"
        ofDlg.FilterIndex = 1
        ofDlg.RestoreDirectory = True

        If (ofDlg.ShowDialog() = DialogResult.OK) Then
            filename = ofDlg.FileName
        End If

        Return filename

    End Function
End Class
