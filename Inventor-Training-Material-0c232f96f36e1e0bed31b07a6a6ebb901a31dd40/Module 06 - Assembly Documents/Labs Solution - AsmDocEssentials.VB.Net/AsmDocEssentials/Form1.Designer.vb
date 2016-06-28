<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.VectorControl2 = New AsmDocEssentials.VectorControl()
        Me.VectorControl1 = New AsmDocEssentials.VectorControl()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(9, 14)
        Me.Button1.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(165, 25)
        Me.Button1.TabIndex = 2
        Me.Button1.Text = "Create Assembly"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Button2
        '
        Me.Button2.Location = New System.Drawing.Point(9, 256)
        Me.Button2.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(165, 36)
        Me.Button2.TabIndex = 3
        Me.Button2.Text = "Transform Occurrence"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(11, 227)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(111, 15)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "Angle (Deg.):"
        '
        'Button3
        '
        Me.Button3.Location = New System.Drawing.Point(9, 46)
        Me.Button3.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(165, 27)
        Me.Button3.TabIndex = 5
        Me.Button3.Text = "Add Occurrence"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'VectorControl2
        '
        Me.VectorControl2.Location = New System.Drawing.Point(9, 165)
        Me.VectorControl2.Margin = New System.Windows.Forms.Padding(5, 3, 5, 3)
        Me.VectorControl2.Name = "VectorControl2"
        Me.VectorControl2.Size = New System.Drawing.Size(165, 52)
        Me.VectorControl2.TabIndex = 1
        Me.VectorControl2.VectorName = "Axis vector:"
        '
        'VectorControl1
        '
        Me.VectorControl1.Location = New System.Drawing.Point(9, 111)
        Me.VectorControl1.Margin = New System.Windows.Forms.Padding(5, 3, 5, 3)
        Me.VectorControl1.Name = "VectorControl1"
        Me.VectorControl1.Size = New System.Drawing.Size(165, 52)
        Me.VectorControl1.TabIndex = 0
        Me.VectorControl1.VectorName = "Translation vector:"
        '
        'Button4
        '
        Me.Button4.Location = New System.Drawing.Point(9, 322)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(165, 42)
        Me.Button4.TabIndex = 0
        Me.Button4.Text = "Lab Demo- Constraint"
        Me.Button4.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(214, 403)
        Me.Controls.Add(Me.Button4)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.VectorControl2)
        Me.Controls.Add(Me.VectorControl1)
        Me.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.Name = "Form1"
        Me.Text = "Assembly"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents UserControl11 As AsmDocEssentials.VectorControl
    Friend WithEvents UserControl12 As AsmDocEssentials.VectorControl
    Friend WithEvents VectorControl1 As AsmDocEssentials.VectorControl
    Friend WithEvents VectorControl2 As AsmDocEssentials.VectorControl
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Button3 As System.Windows.Forms.Button
    Friend WithEvents Button4 As System.Windows.Forms.Button

End Class
