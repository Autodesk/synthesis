
Imports InventorApprentice

Public Class Form1

    Dim mApprenticeServer As ApprenticeServerComponent
    Dim mCurrentDoc As ApprenticeServerDocument
    Dim mCurrentDrawingDoc As ApprenticeServerDrawingDocument

    Private Sub OpenButton_Click(sender As System.Object, e As System.EventArgs) Handles OpenButton.Click
        If (Not mCurrentDoc Is Nothing) Then
            mCurrentDoc.Close()
        End If

        Dim filename As String = OpenFile()

        If (filename <> "") Then

            mCurrentDoc = mApprenticeServer.Open(filename)

            If (mCurrentDoc.DocumentType = DocumentTypeEnum.kDrawingDocumentObject) Then

                mCurrentDrawingDoc = mCurrentDoc

            End If


            Dim oPicDisp As IPictureDisp = mCurrentDoc.Thumbnail
            Dim image As Image = Microsoft.VisualBasic.Compatibility.VB6.IPictureDispToImage(oPicDisp)

            'image.Save("c:\Temp\Thumbnail.bmp", System.Drawing.Imaging.ImageFormat.Bmp)

            PreviewPic.Image = image
            PreviewPic.Refresh()

            lbFilename.Text = "File: " + mCurrentDoc.DisplayName

            ViewerButton.Enabled = True
        End If
    End Sub

    Private Sub ViewerButton_Click(sender As System.Object, e As System.EventArgs) Handles ViewerButton.Click
        Dim viewer As Form2 = New Form2(mCurrentDoc.FullFileName)
        viewer.ShowDialog()
    End Sub

    'Small helper function that prompts user for a file selection
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

        OpenFile = filename

    End Function


    Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        mApprenticeServer = New ApprenticeServerComponent()
        ViewerButton.Enabled = False

        Me.PreviewPic.SizeMode = PictureBoxSizeMode.StretchImage

        mCurrentDoc = Nothing

    End Sub

    Private Sub btnSetPro_Click(sender As System.Object, e As System.EventArgs) Handles btnSetPro.Click
        SetProperty(TextBoxAuthor.Text)
    End Sub

    Private Sub SetProperty(ByVal author As String)

        Dim oApprenticeDoc As ApprenticeServerDocument
        oApprenticeDoc = mApprenticeServer.Open(OpenFile())

        'Get "Inventor Summary Information" PropertySet
        Dim oPropertySet As PropertySet = oApprenticeDoc.PropertySets("{F29F85E0-4FF9-1068-AB91-08002B27B3D9}")

        'Get Author property
        Dim oProperty As InventorApprentice.Property = oPropertySet.Item("Author")

        oProperty.Value = author

        oApprenticeDoc.PropertySets.FlushToFile()
        oApprenticeDoc.Close()

    End Sub
End Class
