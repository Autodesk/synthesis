
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports Microsoft.SqlServer.MessageBox 'ExceptionMessageBox


Public Module EntryPoint

    Private mForm As Form1

    <DllImport("user32.dll")> _
    Private Function FindWindow(ByVal lpClassName As String, ByVal lpWindowName As String) As IntPtr

    End Function

    <DllImport("user32.dll")> _
    Private Function ShowWindow(ByVal hWnd As IntPtr, ByVal nCmdShow As Integer) As Boolean

    End Function

    Public Sub Main(ByVal args As String())

        Console.Title = "PartDocEssentials"

        Dim hWnd As IntPtr = FindWindow(Nothing, "PartDocEssentials")

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

                Dim errorBox As ExceptionMessageBox = New ExceptionMessageBox("Inventor is not running...", "Part Essentials")

                errorBox.Caption = "Part Essentials Error"

                errorBox.SetButtonText("Quit", "Retry", "Launch Inventor")

                errorBox.DefaultButton = ExceptionMessageBoxDefaultButton.Button1
                errorBox.Symbol = ExceptionMessageBoxSymbol.Exclamation
                errorBox.Buttons = ExceptionMessageBoxButtons.Custom

                errorBox.Show(Nothing)

                retry = False

                Select Case errorBox.CustomDialogResult

                    Case ExceptionMessageBoxDialogResult.Button1
                        Exit Sub

                    Case ExceptionMessageBoxDialogResult.Button2
                        retry = True

                    Case ExceptionMessageBoxDialogResult.Button3

                        Dim inventorAppType As Type = System.Type.GetTypeFromProgID("Inventor.Application")
                        Dim oApp As Inventor.Application = System.Activator.CreateInstance(inventorAppType)

                        oApp.Visible = True

                        mForm = New Form1(oApp)
                        mForm.ShowDialog()
                End Select
            End Try
        Loop While (retry = True)
    End Sub

End Module

