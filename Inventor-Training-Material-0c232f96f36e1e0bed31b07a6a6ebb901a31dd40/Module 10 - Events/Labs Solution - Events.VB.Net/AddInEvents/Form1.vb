Imports Inventor

Public Class Form1

    ' This class is used to wrap a Win32 hWnd as a .Net IWind32Window class.
    ' This is primarily used for parenting a dialog to the Inventor window.
    '
    ' For example:
    ' myForm.Show(New WindowWrapper(m_inventorApplication.MainFrameHWND))
    '
    Private Class WindowWrapper
        Implements System.Windows.Forms.IWin32Window

        Public Sub New(ByVal handle As IntPtr)
            _hwnd = handle
        End Sub

        Public ReadOnly Property Handle() As IntPtr _
          Implements System.Windows.Forms.IWin32Window.Handle
            Get
                Return _hwnd
            End Get
        End Property

        Private _hwnd As IntPtr
    End Class


    Private mApplication As Inventor.Application
    Private mSimpleInterac As CSimpleInteraction

    Public Sub New(ByVal oApplication As Inventor.Application)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mApplication = oApplication

    End Sub

    Public Sub ShowModeless()
        MyBase.Show(New WindowWrapper(mApplication.MainFrameHWND))
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        If (Not mApplication.ActiveDocument Is Nothing) Then
            mSimpleInterac = New CSimpleInteraction(mApplication)
        End If

    End Sub

End Class