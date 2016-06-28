
Imports Inventor


Public Class Form1

    Dim m_inventorApp As Inventor.Application = Nothing
    Dim DigitTextBox1 As DigitTextBox
    Dim DigitTextBox2 As DigitTextBox

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        DigitTextBox1 = New DigitTextBox
        DigitTextBox1.Name = "DigitTextBox1"
        DigitTextBox1.Location = New System.Drawing.Point(80, 15)
        DigitTextBox1.Size = New System.Drawing.Size(80, 10)
        DigitTextBox1.TabIndex = 1
        DigitTextBox1.Text = ""

        DigitTextBox2 = New DigitTextBox
        DigitTextBox2.Name = "DigitTextBox2"
        DigitTextBox2.Location = New System.Drawing.Point(80, 40)
        DigitTextBox2.Size = New System.Drawing.Size(80, 10)
        DigitTextBox2.TabIndex = 2
        DigitTextBox2.Text = ""

        Me.Controls.Add(DigitTextBox1)
        Me.Controls.Add(DigitTextBox2)


        ' Add any initialization after the InitializeComponent() call.

        Try ' Try to get an active instance of Inventor

            Try
                m_inventorApp = System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application")

            Catch ' If not active, create a new Inventor session

                Dim inventorAppType As Type = System.Type.GetTypeFromProgID("Inventor.Application")

                m_inventorApp = System.Activator.CreateInstance(inventorAppType)

                'Must be set visible explicitly
                m_inventorApp.Visible = True

            End Try

        Catch
            System.Windows.Forms.MessageBox.Show("Error: couldn't create Inventor instance")
        End Try

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        Dim oView As View = m_inventorApp.ActiveView

        If Not (oView Is Nothing) Then
            If (DigitTextBox1.Text.Length > 0 And DigitTextBox2.Text.Length > 0) Then

                oView.Width = System.Double.Parse(DigitTextBox1.Text)
                oView.Height = System.Double.Parse(DigitTextBox2.Text)

            End If
        End If

    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click

        If (TextBox3.Text.Length > 0) Then

            m_inventorApp.Caption = TextBox3.Text

        End If

    End Sub

End Class
