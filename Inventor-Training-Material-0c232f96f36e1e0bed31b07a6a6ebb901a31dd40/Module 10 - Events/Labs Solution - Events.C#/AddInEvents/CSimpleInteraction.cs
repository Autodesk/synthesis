// VBConversions Note: VB project level imports
using System.Collections;
using System;
using Microsoft.VisualBasic;
using System.Diagnostics;
// End of VB project level imports

using Inventor;



////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//// Use: Implements a simple user interaction. User picks up an entity and its type will be displayed.
////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace AddInEvents
{
	public class CSimpleInteraction
	{
		
		private Inventor.Application mApplication;
		private InteractionEvents mInteractionEvents;
		private SelectEvents mSelectEvents;
		
		public CSimpleInteraction(Inventor.Application oApplication)
		{
			
			mApplication = oApplication;
			
			//Initialize events
			mInteractionEvents = mApplication.CommandManager.CreateInteractionEvents();
			mSelectEvents = mInteractionEvents.SelectEvents;
			
			//Set event handler VB.Net Style
			mSelectEvents.OnSelect += new Inventor.SelectEventsSink_OnSelectEventHandler(this.mSelectEvents_OnSelect);
			mInteractionEvents.OnTerminate += new Inventor.InteractionEventsSink_OnTerminateEventHandler(this.mInteractionEvents_OnTerminate);
			
			//Clear filter and set new ones if needed
			mSelectEvents.ClearSelectionFilter();
			
			//Always Disable mouse move if not needed for performances
			mSelectEvents.MouseMoveEnabled = false;
			mSelectEvents.Enabled = true;
			mSelectEvents.SingleSelectEnabled = true;
			
			//Remember to Start/Stop the interaction event
			mInteractionEvents.Start();
			
		}
		
		public void CleanUp()
		{
			
			//Remove handlers
			mSelectEvents.OnSelect -= new Inventor.SelectEventsSink_OnSelectEventHandler(this.mSelectEvents_OnSelect);
			mInteractionEvents.OnTerminate -= new Inventor.InteractionEventsSink_OnTerminateEventHandler(this.mInteractionEvents_OnTerminate);
			
			mSelectEvents = null;
			mInteractionEvents = null;
			
		}
		
		private void mSelectEvents_OnSelect(Inventor.ObjectsEnumerator JustSelectedEntities, Inventor.SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Inventor.Point2d ViewPosition, Inventor.View View)
		{
			
			if (JustSelectedEntities.Count != 0)
			{
				
				object selectedObj = JustSelectedEntities[1];
				System.Windows.Forms.MessageBox.Show((string) ("Selected Entity: " + Information.TypeName(selectedObj)), "Simple Interaction");
				
			}
			
		}
		
		private void mInteractionEvents_OnTerminate()
		{
			CleanUp();
		}
		
	}
}
