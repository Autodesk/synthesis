<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form2
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form2))
        Me.AxInventorViewControl1 = New AxInventorViewControlLib.AxInventorViewControl()
        CType(Me.AxInventorViewControl1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'AxInventorViewControl1
        '
        Me.AxInventorViewControl1.Enabled = True
        Me.AxInventorViewControl1.Location = New System.Drawing.Point(44, 44)
        Me.AxInventorViewControl1.Name = "AxInventorViewControl1"
        Me.AxInventorViewControl1.OcxState = CType(resources.GetObject("AxInventorViewControl1.OcxState"), System.Windows.Forms.AxHost.State)
        Me.AxInventorViewControl1.Size = New System.Drawing.Size(357, 312)
        Me.AxInventorViewControl1.TabIndex = 0
        '
        'Form2
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(449, 420)
        Me.Controls.Add(Me.AxInventorViewControl1)
        Me.Name = "Form2"
        Me.Text = "Form2"
        CType(Me.AxInventorViewControl1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents AxInventorViewControl1 As AxInventorViewControlLib.AxInventorViewControl
End Class
