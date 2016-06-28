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
        Me.OpenButton = New System.Windows.Forms.Button()
        Me.PreviewPic = New System.Windows.Forms.PictureBox()
        Me.ViewerButton = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lbFilename = New System.Windows.Forms.Label()
        Me.btnSetPro = New System.Windows.Forms.Button()
        Me.TextBoxAuthor = New System.Windows.Forms.TextBox()
        CType(Me.PreviewPic, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'OpenButton
        '
        Me.OpenButton.Location = New System.Drawing.Point(46, 446)
        Me.OpenButton.Name = "OpenButton"
        Me.OpenButton.Size = New System.Drawing.Size(460, 44)
        Me.OpenButton.TabIndex = 0
        Me.OpenButton.Text = "Open Inventor File"
        Me.OpenButton.UseVisualStyleBackColor = True
        '
        'PreviewPic
        '
        Me.PreviewPic.Location = New System.Drawing.Point(46, 40)
        Me.PreviewPic.Name = "PreviewPic"
        Me.PreviewPic.Size = New System.Drawing.Size(460, 386)
        Me.PreviewPic.TabIndex = 1
        Me.PreviewPic.TabStop = False
        '
        'ViewerButton
        '
        Me.ViewerButton.Location = New System.Drawing.Point(46, 495)
        Me.ViewerButton.Name = "ViewerButton"
        Me.ViewerButton.Size = New System.Drawing.Size(460, 44)
        Me.ViewerButton.TabIndex = 0
        Me.ViewerButton.Text = "Viewer with View control"
        Me.ViewerButton.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(43, 22)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(151, 15)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "view in picturebox"
        '
        'lbFilename
        '
        Me.lbFilename.AutoSize = True
        Me.lbFilename.Location = New System.Drawing.Point(240, 22)
        Me.lbFilename.Name = "lbFilename"
        Me.lbFilename.Size = New System.Drawing.Size(0, 15)
        Me.lbFilename.TabIndex = 3
        '
        'btnSetPro
        '
        Me.btnSetPro.Location = New System.Drawing.Point(46, 545)
        Me.btnSetPro.Name = "btnSetPro"
        Me.btnSetPro.Size = New System.Drawing.Size(353, 46)
        Me.btnSetPro.TabIndex = 4
        Me.btnSetPro.Text = "Set Property [Author]"
        Me.btnSetPro.UseVisualStyleBackColor = True
        '
        'TextBoxAuthor
        '
        Me.TextBoxAuthor.Location = New System.Drawing.Point(413, 553)
        Me.TextBoxAuthor.Name = "TextBoxAuthor"
        Me.TextBoxAuthor.Size = New System.Drawing.Size(92, 25)
        Me.TextBoxAuthor.TabIndex = 5
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(572, 594)
        Me.Controls.Add(Me.TextBoxAuthor)
        Me.Controls.Add(Me.btnSetPro)
        Me.Controls.Add(Me.lbFilename)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.PreviewPic)
        Me.Controls.Add(Me.ViewerButton)
        Me.Controls.Add(Me.OpenButton)
        Me.Name = "Form1"
        Me.Text = "Form1"
        CType(Me.PreviewPic, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents OpenButton As System.Windows.Forms.Button
    Friend WithEvents PreviewPic As System.Windows.Forms.PictureBox
    Friend WithEvents ViewerButton As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lbFilename As System.Windows.Forms.Label
    Friend WithEvents btnSetPro As System.Windows.Forms.Button
    Friend WithEvents TextBoxAuthor As System.Windows.Forms.TextBox

End Class
