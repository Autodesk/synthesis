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
        Me.Button1 = New System.Windows.Forms.Button
        Me.Border2 = New System.Windows.Forms.Button
        Me.Button3 = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'Button1
        '
        Me.Button1.AutoSize = True
        Me.Button1.Location = New System.Drawing.Point(12, 12)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(161, 32)
        Me.Button1.TabIndex = 0
        Me.Button1.Text = "Add TitleBlock"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Border2
        '
        Me.Border2.Location = New System.Drawing.Point(12, 50)
        Me.Border2.Name = "Border2"
        Me.Border2.Size = New System.Drawing.Size(161, 38)
        Me.Border2.TabIndex = 1
        Me.Border2.Text = "Add BorderDefinition"
        Me.Border2.UseVisualStyleBackColor = True
        '
        'Button3
        '
        Me.Button3.AutoSize = True
        Me.Button3.Location = New System.Drawing.Point(12, 94)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(161, 38)
        Me.Button3.TabIndex = 6
        Me.Button3.Text = "Drawing Complete "
        Me.Button3.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(185, 147)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.Border2)
        Me.Controls.Add(Me.Button1)
        Me.Name = "Form1"
        Me.Text = "Drawing Essentials"
        Me.TopMost = True
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Border2 As System.Windows.Forms.Button
    Friend WithEvents Button3 As System.Windows.Forms.Button

End Class
