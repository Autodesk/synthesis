// VBConversions Note: VB project level imports
using System.Collections;
using System;
using Microsoft.VisualBasic;
using System.Diagnostics;
// End of VB project level imports

using Inventor;
using System.Runtime.InteropServices;
using Microsoft.Win32;


namespace AddInEvents
{
	namespace AddInEvents
	{
		
		[ProgIdAttribute("AddInEvents.StandardAddInServer"), GuidAttribute("49d3dbd4-20c4-4c03-8027-49206f131596")]
        public class StandardAddInServer : Inventor.ApplicationAddInServer
		{
			
			// Inventor application object.
			private Inventor.Application m_inventorApplication;
			private Form1 mControlForm;
			
			private Inventor.ApplicationEvents m_ApplicationEvents;
			
			
#region ApplicationAddInServer Members
			
			
			public void Activate(Inventor.ApplicationAddInSite addInSiteObject, bool firstTime)
			{
				
				m_inventorApplication = addInSiteObject.Application;
				
				m_ApplicationEvents = m_inventorApplication.ApplicationEvents;
                m_ApplicationEvents.OnOpenDocument += this.m_ApplicationEvents_OnOpenDocument;
				
				mControlForm = new Form1(m_inventorApplication);
				mControlForm.ShowModeless();
				
			}
			
			private void m_ApplicationEvents_OnOpenDocument(Inventor._Document DocumentObject, string FullDocumentName, Inventor.EventTimingEnum BeforeOrAfter, Inventor.NameValueMap Context, Inventor.HandlingCodeEnum HandlingCode)
			{
				System.Windows.Forms.MessageBox.Show((string) ("OnOpenDocument: " + DocumentObject.DisplayName));
				
			}
			
			public void Deactivate()
			{
				
				// This method is called by Inventor when the AddIn is unloaded.
				// The AddIn will be unloaded either manually by the user or
				// when the Inventor session is terminated.
				
				// TODO:  Add ApplicationAddInServer.Deactivate implementation
				
				// Release objects.
				Marshal.ReleaseComObject(m_inventorApplication);
				m_inventorApplication = null;
				
				System.GC.WaitForPendingFinalizers();
				System.GC.Collect();
				
			}
			
			public object Automation
			{
				
				// This property is provided to allow the AddIn to expose an API
				// of its own to other programs. Typically, this  would be done by
				// implementing the AddIn's API interface in a class and returning
				// that class object through this property.
				
				get
				{
					return null;
				}
				
			}
			
			public void ExecuteCommand(int commandID)
			{
				
				// Note:this method is now obsolete, you should use the
				// ControlDefinition functionality for implementing commands.
				
			}
			
#endregion

            void m_ApplicationEvents_OnOpenDocument(_Document DocumentObject, string FullDocumentName, EventTimingEnum BeforeOrAfter, NameValueMap Context, out HandlingCodeEnum HandlingCode)
            {
                System.Windows.Forms.MessageBox.Show("OnOpenDocument fires!");

                HandlingCode = HandlingCodeEnum.kEventHandled;
            }
 
			
			
		}
		
	}
	
	
}
