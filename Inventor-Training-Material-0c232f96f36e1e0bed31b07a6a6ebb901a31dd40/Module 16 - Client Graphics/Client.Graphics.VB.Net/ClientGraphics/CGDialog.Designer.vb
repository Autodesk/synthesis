<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CGDialog
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
        Me.btnSlicing = New System.Windows.Forms.Button
        Me.btnMapping = New System.Windows.Forms.Button
        Me.btnUseStrip = New System.Windows.Forms.Button
        Me.btnUseIndex = New System.Windows.Forms.Button
        Me.btnUseModelData = New System.Windows.Forms.Button
        Me.btnPreview = New System.Windows.Forms.Button
        Me.btnOverlay = New System.Windows.Forms.Button
        Me.CBoxPrimitive = New System.Windows.Forms.ComboBox
        Me.btnStore = New System.Windows.Forms.Button
        Me.Label1 = New System.Windows.Forms.Label
        Me.SuspendLayout()
        '
        'btnSlicing
        '
        Me.btnSlicing.Location = New System.Drawing.Point(84, 460)
        Me.btnSlicing.Name = "btnSlicing"
        Me.btnSlicing.Size = New System.Drawing.Size(179, 43)
        Me.btnSlicing.TabIndex = 1
        Me.btnSlicing.Text = "Slcing"
        Me.btnSlicing.UseVisualStyleBackColor = True
        '
        'btnMapping
        '
        Me.btnMapping.Location = New System.Drawing.Point(84, 519)
        Me.btnMapping.Name = "btnMapping"
        Me.btnMapping.Size = New System.Drawing.Size(179, 43)
        Me.btnMapping.TabIndex = 0
        Me.btnMapping.Text = "Mapping"
        Me.btnMapping.UseVisualStyleBackColor = True
        '
        'btnUseStrip
        '
        Me.btnUseStrip.Location = New System.Drawing.Point(84, 166)
        Me.btnUseStrip.Name = "btnUseStrip"
        Me.btnUseStrip.Size = New System.Drawing.Size(179, 43)
        Me.btnUseStrip.TabIndex = 5
        Me.btnUseStrip.Text = "Using Strip"
        Me.btnUseStrip.UseVisualStyleBackColor = True
        '
        'btnUseIndex
        '
        Me.btnUseIndex.Location = New System.Drawing.Point(84, 108)
        Me.btnUseIndex.Name = "btnUseIndex"
        Me.btnUseIndex.Size = New System.Drawing.Size(179, 43)
        Me.btnUseIndex.TabIndex = 7
        Me.btnUseIndex.Text = "Using Index Set"
        Me.btnUseIndex.UseVisualStyleBackColor = True
        '
        'btnUseModelData
        '
        Me.btnUseModelData.Location = New System.Drawing.Point(84, 224)
        Me.btnUseModelData.Name = "btnUseModelData"
        Me.btnUseModelData.Size = New System.Drawing.Size(179, 43)
        Me.btnUseModelData.TabIndex = 2
        Me.btnUseModelData.Text = "Using Model Native Data"
        Me.btnUseModelData.UseVisualStyleBackColor = True
        '
        'btnPreview
        '
        Me.btnPreview.Location = New System.Drawing.Point(84, 397)
        Me.btnPreview.Name = "btnPreview"
        Me.btnPreview.Size = New System.Drawing.Size(179, 43)
        Me.btnPreview.TabIndex = 5
        Me.btnPreview.Text = "Preview"
        Me.btnPreview.UseVisualStyleBackColor = True
        '
        'btnOverlay
        '
        Me.btnOverlay.Location = New System.Drawing.Point(84, 339)
        Me.btnOverlay.Name = "btnOverlay"
        Me.btnOverlay.Size = New System.Drawing.Size(179, 43)
        Me.btnOverlay.TabIndex = 4
        Me.btnOverlay.Text = "Overlay"
        Me.btnOverlay.UseVisualStyleBackColor = True
        '
        'CBoxPrimitive
        '
        Me.CBoxPrimitive.Items.AddRange(New Object() {"Please Select an Option", "Point", "Line", "LineStrip", "Triangle", "TriangleStrip", "TriangleFan", "Text", "Curve", "Surface"})
        Me.CBoxPrimitive.Location = New System.Drawing.Point(84, 47)
        Me.CBoxPrimitive.Name = "CBoxPrimitive"
        Me.CBoxPrimitive.Size = New System.Drawing.Size(210, 23)
        Me.CBoxPrimitive.TabIndex = 9
        '
        'btnStore
        '
        Me.btnStore.Location = New System.Drawing.Point(84, 282)
        Me.btnStore.Name = "btnStore"
        Me.btnStore.Size = New System.Drawing.Size(179, 43)
        Me.btnStore.TabIndex = 2
        Me.btnStore.Text = "Store Graphics"
        Me.btnStore.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(80, 29)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(79, 15)
        Me.Label1.TabIndex = 16
        Me.Label1.Text = "Primitive"
        '
        'CGDialog
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(340, 585)
        Me.Controls.Add(Me.btnPreview)
        Me.Controls.Add(Me.btnStore)
        Me.Controls.Add(Me.btnOverlay)
        Me.Controls.Add(Me.btnSlicing)
        Me.Controls.Add(Me.btnUseModelData)
        Me.Controls.Add(Me.btnMapping)
        Me.Controls.Add(Me.btnUseStrip)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.CBoxPrimitive)
        Me.Controls.Add(Me.btnUseIndex)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "CGDialog"
        Me.Text = "Client Graphics"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnSlicing As System.Windows.Forms.Button
    Friend WithEvents btnMapping As System.Windows.Forms.Button
    Friend WithEvents btnUseModelData As System.Windows.Forms.Button
    Friend WithEvents btnUseStrip As System.Windows.Forms.Button
    Friend WithEvents btnUseIndex As System.Windows.Forms.Button
    Friend WithEvents btnPreview As System.Windows.Forms.Button
    Friend WithEvents btnOverlay As System.Windows.Forms.Button
    Friend WithEvents CBoxPrimitive As System.Windows.Forms.ComboBox
    Friend WithEvents btnStore As System.Windows.Forms.Button
    Friend WithEvents Label1 As System.Windows.Forms.Label
End Class
