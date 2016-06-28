using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;


namespace AsmDocEssentials
{
    //A Digit only TextBox
    public class DigitTextBox: TextBox
    {
        private const Int32 GWL_STYLE = -16;
        private const Int32 ES_NUMBER = 0x2000;

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public DigitTextBox()
        {
            //Get the current style.
            int style = GetWindowLong(base.Handle, GWL_STYLE);

            base.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);

            //Add ES_NUMBER to the style.
            SetWindowLong(base.Handle, GWL_STYLE, style|ES_NUMBER);
        }

        //Override the WndProc and ignore the WM_CONTEXTMENU and WM_PASTE messages.
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name="FullTrust")]
        protected override void WndProc(ref Message msg) 
        {
            const Int32 WM_CONTEXTMENU = 0x7B;
            const Int32 WM_PASTE = 0x302;

            if ((msg.Msg != WM_PASTE) && (msg.Msg != WM_CONTEXTMENU))
            {
                base.DefWndProc(ref msg);
            }
        }
    }
}
