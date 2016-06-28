
Imports Inventor

Public Class VectorControl

    Private mApp As Inventor.Application

    Private xTxtBox As DigitTextBox
    Private yTxtBox As DigitTextBox
    Private zTxtBox As DigitTextBox

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        xTxtBox = New DigitTextBox
        xTxtBox.Name = "DigitTextBox1"
        xTxtBox.Location = New System.Drawing.Point(5, 20)
        xTxtBox.Size = New System.Drawing.Size(35, 30)
        xTxtBox.TabIndex = 1
        xTxtBox.Text = ""

        yTxtBox = New DigitTextBox
        yTxtBox.Name = "DigitTextBox1"
        yTxtBox.Location = New System.Drawing.Point(45, 20)
        yTxtBox.Size = New System.Drawing.Size(35, 30)
        yTxtBox.TabIndex = 2
        yTxtBox.Text = ""

        zTxtBox = New DigitTextBox
        zTxtBox.Name = "DigitTextBox3"
        zTxtBox.Location = New System.Drawing.Point(85, 20)
        zTxtBox.Size = New System.Drawing.Size(35, 30)
        zTxtBox.TabIndex = 3
        zTxtBox.Text = ""

        Me.Controls.Add(xTxtBox)
        Me.Controls.Add(yTxtBox)
        Me.Controls.Add(zTxtBox)

    End Sub


    Public WriteOnly Property SetApp() As Inventor.Application

        Set(ByVal value As Inventor.Application)
            mApp = value
        End Set

    End Property


    Public Property VectorName() As String

        Get
            VectorName = Label1.Text
        End Get

        Set(ByVal value As String)
            Label1.Text = value
        End Set

    End Property


    Public Property Vector() As Vector

        Get
            Dim x, y, z As Double

            If (xTxtBox.TextLength = 0) Then
                x = 0
            Else
                x = System.Double.Parse(xTxtBox.Text)
            End If

            If (yTxtBox.TextLength = 0) Then
                y = 0
            Else
                y = System.Double.Parse(yTxtBox.Text)
            End If

            If (zTxtBox.TextLength = 0) Then
                z = 0
            Else
                z = System.Double.Parse(zTxtBox.Text)
            End If

            Vector = mApp.TransientGeometry.CreateVector(x, y, z)

        End Get

        Set(ByVal value As Vector)

            xTxtBox.Text = value.X.ToString("F2")
            yTxtBox.Text = value.Y.ToString("F2")
            zTxtBox.Text = value.Z.ToString("F2")

        End Set

    End Property

End Class
