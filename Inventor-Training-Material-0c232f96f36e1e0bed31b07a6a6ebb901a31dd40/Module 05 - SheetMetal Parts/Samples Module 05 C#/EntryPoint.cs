// VBConversions Note: VB project level imports
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Linq;
using System;
using System.Collections;
using System.Xml.Linq;
using System.Windows.Forms;
// End of VB project level imports

using System.Runtime.InteropServices;



namespace InvExeApp
{
	public sealed class EntryPoint
	{
		
		public static Inventor.Application _InvApplication;
		
		private static string Title = "SheetMetal Samples";
		
		private static Form1 mForm;
		
		[DllImport("user32.dll")]private static  extern IntPtr FindWindow(string lpClassName, string lpWindowName);
		
		
		[DllImport("user32.dll")]private static  extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		
		
		public static void Main(string[] args)
		{
			
			Console.Title = Title;
			
			//Hide the Console Window
			IntPtr hWnd = FindWindow(null, Console.Title);
			
			if (hWnd != IntPtr.Zero)
			{
				//SW_HIDE = 0
				ShowWindow(hWnd, 0);
			}
			
			bool retry = false;
			
			do
			{
				try
				{
					
					retry = false;
					
					_InvApplication =  (Inventor.Application) (System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application"));
					
					mForm = new Form1(_InvApplication);
					mForm.Text = Title;
					mForm.ShowDialog();
					
				}
				catch (Exception)
				{
					
					string Message = "Inventor is not running...";
					string Caption = Title + " Sample Error";
					MessageBoxButtons Buttons = MessageBoxButtons.RetryCancel;
					
					DialogResult Result = MessageBox.Show(Message, Caption, Buttons, MessageBoxIcon.Exclamation);
					
					switch (Result)
					{
						
					case DialogResult.Retry:
						retry = true;
						break;
						
					case DialogResult.Cancel:
						return;
						
				}
				
			}
		} while (retry == true);
		
	}
	
}

}
