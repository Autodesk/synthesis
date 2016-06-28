
'A Digit only TextBox
Class DigitTextBox
    Inherits TextBox

    Private Declare Function SetWindowLong Lib "user32" Alias "SetWindowLongA" (ByVal hWnd As IntPtr, ByVal nIndex As Int32, ByVal dwNewint32 As Int32) As Int32
    Private Declare Function GetWindowLong Lib "user32" Alias "GetWindowLongA" (ByVal hWnd As IntPtr, ByVal nIndex As Int32) As Int32
    Private Const GWL_STYLE As Int32 = (-16)
    Private Const ES_NUMBER As Int32 = &H2000

    Public Sub New()
        ' Get the current style.
        Dim style As Integer = GetWindowLong(Me.Handle, GWL_STYLE)

        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0)

        ' Add ES_NUMBER to the style.
        SetWindowLong(Me.Handle, GWL_STYLE, style Or ES_NUMBER)
    End Sub

    ' Override the WndProc and ignore the WM_CONTEXTMENU and WM_PASTE messages.
    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
        Const WM_CONTEXTMENU As Int32 = &H7B
        Const WM_PASTE As Int32 = &H302

        If (m.Msg <> WM_PASTE) And (m.Msg <> WM_CONTEXTMENU) Then
            Me.DefWndProc(m)
        End If
    End Sub
End Class