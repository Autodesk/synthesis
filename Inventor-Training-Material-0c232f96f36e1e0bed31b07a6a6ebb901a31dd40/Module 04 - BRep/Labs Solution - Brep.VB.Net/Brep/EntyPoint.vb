Imports System.Runtime.InteropServices
Imports System.Windows.Forms


Public Module EntryPoint

    Private mForm As Form1

    <DllImport("user32.dll")> _
    Private Function FindWindow(ByVal lpClassName As String, ByVal lpWindowName As String) As IntPtr

    End Function

    <DllImport("user32.dll")> _
    Private Function ShowWindow(ByVal hWnd As IntPtr, ByVal nCmdShow As Integer) As Boolean

    End Function

    Public Sub Main(ByVal args As String())

        Console.Title = "BRep Sample"

        'Hide the Console Window
        Dim hWnd As IntPtr = FindWindow(Nothing, Console.Title)

        If (hWnd <> IntPtr.Zero) Then
            'SW_HIDE = 0
            ShowWindow(hWnd, 0)
        End If

        Dim retry As Boolean = False

        Do
            Try

                Dim oApp As Inventor.Application = System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application")
                mForm = New Form1(oApp)
                mForm.ShowDialog()

            Catch ex As Exception

                Dim Message As String = "Inventor is not running..."
                Dim Caption As String = "BRep Sample Error"
                Dim Buttons As MessageBoxButtons = MessageBoxButtons.RetryCancel

                Dim Result As DialogResult = MessageBox.Show(Message, Caption, Buttons, MessageBoxIcon.Exclamation)

                retry = False

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
