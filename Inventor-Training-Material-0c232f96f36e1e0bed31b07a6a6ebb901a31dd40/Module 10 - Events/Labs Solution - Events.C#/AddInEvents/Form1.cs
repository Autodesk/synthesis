// VBConversions Note: VB project level imports
using System.Collections;
using System;
using Microsoft.VisualBasic;
using System.Diagnostics;
// End of VB project level imports

using Inventor;


namespace AddInEvents
{
	public partial class Form1
	{
		
		// This class is used to wrap a Win32 hWnd as a .Net IWind32Window class.
		// This is primarily used for parenting a dialog to the Inventor window.
		//
		// For example:
		// myForm.Show(New WindowWrapper(m_inventorApplication.MainFrameHWND))
		//
		private class WindowWrapper : System.Windows.Forms.IWin32Window
		{
			
			public WindowWrapper(IntPtr handle)
			{
				_hwnd = handle;
			}
			
			public IntPtr Handle
			{
				get
				{
					return _hwnd;
				}
			}
			
			private IntPtr _hwnd;
		}
		
		
		private Inventor.Application mApplication;
		private CSimpleInteraction mSimpleInterac;
		
		public Form1(Inventor.Application oApplication)
		{
			
			// This call is required by the Windows Form Designer.
			InitializeComponent();
			
			// Add any initialization after the InitializeComponent() call.
			mApplication = oApplication;
			
		}
		
		public void ShowModeless()
		{
			base.Show(new WindowWrapper((IntPtr) mApplication.MainFrameHWND));
		}
		
		public void Button1_Click(System.Object sender, System.EventArgs e)
		{
			
			if (mApplication.ActiveDocument != null)
			{
				mSimpleInterac = new CSimpleInteraction(mApplication);
			}
			
		}
		
	}
}
