
Imports Inventor

Public Class Form1

    Dim mApp As Inventor.Application
    Dim mUOM As UnitsOfMeasure

    Public Sub New(ByVal oApp As Inventor.Application)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mApp = oApp

    End Sub


    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        If (TextBox1.Text.Length > 0) Then

            Dim oDoc As PartDocument = mApp.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _
                    mApp.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject), True)

            'Author property is contained in "Inventor Summary Information" Set
            Dim oPropertySet As PropertySet = oDoc.PropertySets("{F29F85E0-4FF9-1068-AB91-08002B27B3D9}")

            'Using Inventor.Property to avoid confusion
            Dim oProperty As Inventor.Property = oPropertySet.Item("Author")

            oProperty.Value = TextBox1.Text

            'Get "Inventor User Defined Properties" set 
            oPropertySet = oDoc.PropertySets("{D5CDD505-2E9C-101B-9397-08002B2CF9AE}")

            'Create new property
            oProperty = oPropertySet.Add("MyPropValue", "MyProp")

            'Save document, prompt user for filename
            Dim oDLG As FileDialog = Nothing
            mApp.CreateFileDialog(oDLG)

            oDLG.FileName = "C:\Temp\NewPart.ipt"
            oDLG.Filter = "Inventor Files (*.ipt)|*.ipt"
            oDLG.DialogTitle = "Save Part"

            oDLG.ShowSave()

            If (oDLG.FileName <> "") Then

                oDoc.SaveAs(oDLG.FileName, False)
                'oDoc.Close()

            End If

        End If

    End Sub


    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click

        Try

            Dim oPartDoc As PartDocument = mApp.ActiveDocument
            Dim oParam As Parameter = oPartDoc.ComponentDefinition.Parameters("Length")

            Dim value As Double = mUOM.GetValueFromExpression(TextBox2.Text, UnitsTypeEnum.kDefaultDisplayLengthUnits)
            oParam.Value = value

            oPartDoc.Update()
            mApp.ActiveView.Fit()

        Catch ex As Exception

            System.Windows.Forms.MessageBox.Show("Error: Parameter ""Length"" does not exist in active document...")

        End Try

    End Sub


    Private Sub TextBox2_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox2.TextChanged

        mUOM = mApp.ActiveDocument.UnitsOfMeasure

        If (TextBox2.Text.Length > 0) Then

            If (mUOM.IsExpressionValid(TextBox2.Text, UnitsTypeEnum.kDefaultDisplayLengthUnits)) Then

                TextBox2.ForeColor = Drawing.Color.Black
                Button2.Enabled = True

            Else

                TextBox2.ForeColor = Drawing.Color.Red
                Button2.Enabled = False

            End If

        End If

    End Sub


End Class
