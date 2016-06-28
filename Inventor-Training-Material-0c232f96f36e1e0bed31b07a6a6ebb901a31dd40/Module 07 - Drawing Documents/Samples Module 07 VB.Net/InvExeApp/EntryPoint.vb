Imports System.Runtime.InteropServices
Imports System.Windows.Forms


Public Module EntryPoint

    Public _InvApplication As Inventor.Application

    Private Title As String = "Drawing Doc Sample"

    Private mForm As Form1

    <DllImport("user32.dll")> _
    Private Function FindWindow(ByVal lpClassName As String, ByVal lpWindowName As String) As IntPtr

    End Function

    <DllImport("user32.dll")> _
    Private Function ShowWindow(ByVal hWnd As IntPtr, ByVal nCmdShow As Integer) As Boolean

    End Function

    Public Sub Main(ByVal args As String())

        Console.Title = Title

        'Hide the Console Window
        Dim hWnd As IntPtr = FindWindow(Nothing, Console.Title)

        If (hWnd <> IntPtr.Zero) Then
            'SW_HIDE = 0
            ShowWindow(hWnd, 0)
        End If

        Dim retry As Boolean = False

        Do
            Try

                retry = False

                _InvApplication = System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application")

                mForm = New Form1(_InvApplication)
                mForm.Text = Title
                mForm.ShowDialog()

            Catch ex As Exception

            Dim Message As String = "Inventor is not running..."
            Dim Caption As String = Title + " Sample Error"
            Dim Buttons As MessageBoxButtons = MessageBoxButtons.RetryCancel

            Dim Result As DialogResult = MessageBox.Show(Message, Caption, Buttons, MessageBoxIcon.Exclamation)

            Select Case Result

                Case DialogResult.Retry
                    retry = True

                Case DialogResult.Cancel
                    Exit Sub

            End Select

        End Try
        Loop While (retry = True)

    End Sub

End Module
