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
        Dim ListViewGroup3 As System.Windows.Forms.ListViewGroup = New System.Windows.Forms.ListViewGroup("Open Options", System.Windows.Forms.HorizontalAlignment.Left)
        Dim ListViewGroup4 As System.Windows.Forms.ListViewGroup = New System.Windows.Forms.ListViewGroup("Save Options", System.Windows.Forms.HorizontalAlignment.Left)
        Me.OptionsView = New System.Windows.Forms.ListView
        Me.OptionName = New System.Windows.Forms.ColumnHeader
        Me.OptionValue = New System.Windows.Forms.ColumnHeader
        Me.ComboAddIns = New System.Windows.Forms.ComboBox
        Me.Button1 = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'OptionsView
        '
        Me.OptionsView.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.OptionName, Me.OptionValue})
        Me.OptionsView.FullRowSelect = True
        Me.OptionsView.GridLines = True
        ListViewGroup3.Header = "Open Options"
        ListViewGroup3.Name = "GroupOpenOpts"
        ListViewGroup4.Header = "Save Options"
        ListViewGroup4.Name = "GroupSaveOpts"
        Me.OptionsView.Groups.AddRange(New System.Windows.Forms.ListViewGroup() {ListViewGroup3, ListViewGroup4})
        Me.OptionsView.Location = New System.Drawing.Point(1, 4)
        Me.OptionsView.Name = "OptionsView"
        Me.OptionsView.Size = New System.Drawing.Size(277, 291)
        Me.OptionsView.TabIndex = 0
        Me.OptionsView.UseCompatibleStateImageBehavior = False
        '
        'OptionName
        '
        Me.OptionName.Text = "Option"
        Me.OptionName.Width = 180
        '
        'OptionValue
        '
        Me.OptionValue.Text = "Value"
        Me.OptionValue.Width = 90
        '
        'ComboAddIns
        '
        Me.ComboAddIns.FormattingEnabled = True
        Me.ComboAddIns.Location = New System.Drawing.Point(1, 301)
        Me.ComboAddIns.Name = "ComboAddIns"
        Me.ComboAddIns.Size = New System.Drawing.Size(277, 21)
        Me.ComboAddIns.TabIndex = 1
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(1, 328)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(277, 34)
        Me.Button1.TabIndex = 2
        Me.Button1.Text = "Display options"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(279, 365)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.ComboAddIns)
        Me.Controls.Add(Me.OptionsView)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Name = "Form1"
        Me.Text = "Translator options"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents OptionsView As System.Windows.Forms.ListView
    Friend WithEvents ComboAddIns As System.Windows.Forms.ComboBox
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents OptionName As System.Windows.Forms.ColumnHeader
    Friend WithEvents OptionValue As System.Windows.Forms.ColumnHeader

End Class
