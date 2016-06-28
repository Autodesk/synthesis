Public Class Form1

    Dim mAddInInterface As HelloWorldVBNet.HelloWorldVBNet.MyInterface
Private Sub Button1_Click( ByVal sender As System.Object,  ByVal e As System.EventArgs) Handles Button1.Click
        
        If (TextBox.Text.Length > 0) Then

            mAddInInterface.MyFunction(TextBox.Text)

        End If
End Sub

Private Sub Form1_Load( ByVal sender As System.Object,  ByVal e As System.EventArgs) Handles MyBase.Load
         Try
            Dim oApp As Inventor.Application = System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application")

            Try

                Dim oAddIn As Inventor.ApplicationAddIn = oApp.ApplicationAddIns.ItemById("{958eaca3-2151-415a-aa22-02d5e495389a}")
                mAddInInterface = oAddIn.Automation

            Catch ex As Exception

                System.Windows.Forms.MessageBox.Show("Error: AddIn not found...")
                Button1.Enabled = False
                Exit Sub

            End Try

        Catch ex As Exception

            System.Windows.Forms.MessageBox.Show("Error: Inventor must be running...")
            Button1.Enabled = False
            Exit Sub

        End Try
End Sub
End Class
