using System;
using System.Windows.Forms;
using System.Drawing;
using Inventor;

namespace SimpleAddIn
{
	/// <summary>
	/// ToggleSlotStateButton class
	/// </summary>
	
	internal class ToggleSlotStateButton : Button
	{
		#region "Methods"

		public ToggleSlotStateButton(string displayName, string internalName, CommandTypesEnum commandType, string clientId, string description, string tooltip, Icon standardIcon, Icon largeIcon, ButtonDisplayEnum buttonDisplayType)
			: base(displayName, internalName, commandType, clientId, description, tooltip, standardIcon, largeIcon, buttonDisplayType)
		{
			
		}

		public ToggleSlotStateButton(string displayName, string internalName, CommandTypesEnum commandType, string clientId, string description, string tooltip, ButtonDisplayEnum buttonDisplayType)
			: base(displayName, internalName, commandType, clientId, description, tooltip, buttonDisplayType)
		{
			
		}

		override protected void ButtonDefinition_OnExecute(NameValueMap context)
		{	
			try
			{
				//if same session, button definition will already exist
				ButtonDefinition drawSlotButtonDefinition;
                drawSlotButtonDefinition = (Inventor.ButtonDefinition)InventorApplication.CommandManager.ControlDefinitions["Autodesk:SimpleAddIn:DrawSlotCmdBtn"];

				if (drawSlotButtonDefinition.Enabled == true)
					drawSlotButtonDefinition.Enabled = false;
				else
					drawSlotButtonDefinition.Enabled = true;
			}
			catch(Exception e)
			{
				MessageBox.Show(e.ToString());
			}
		}
		#endregion
	}
}
