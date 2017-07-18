using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InvAddIn
{
    class WindowWrapper
    {

    }
}

/*    ''' <summary>
    ''' Used to wrap a Win32 hWnd as a .Net IWind32Window class.
    ''' This is primarily used for parenting a dialog to the Inventor window.
    ''' </summary>
    ''' <remarks>
    ''' For example:
    ''' myForm.Show(New WindowWrapper(m_inventorApplication.MainFrameHWND))
    ''' </remarks>
    Public Class WindowWrapper
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
*/