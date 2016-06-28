using System;
using System.Windows.Forms;
using System.Drawing;
using Inventor;

namespace SimpleAddIn
{
	/// <summary>
	/// AddSlotOptionButton class
	/// </summary>
	
	internal class AddSlotOptionButton : Button
	{
		#region "Methods"

		public AddSlotOptionButton(string displayName, string internalName, CommandTypesEnum commandType, string clientId, string description, string tooltip, Icon standardIcon, Icon largeIcon, ButtonDisplayEnum buttonDisplayType)
			: base(displayName, internalName, commandType, clientId, description, tooltip, standardIcon, largeIcon, buttonDisplayType)
		{
			
		}
		public AddSlotOptionButton(string displayName, string internalName, CommandTypesEnum commandType, string clientId, string description, string tooltip, ButtonDisplayEnum buttonDisplayType)
			: base(displayName, internalName, commandType, clientId, description, tooltip, buttonDisplayType)
		{
			
		}

		override protected void ButtonDefinition_OnExecute(NameValueMap context)
		{
			try
			{
				//if same session, combobox definitions will already exist
				ComboBoxDefinition slotWidthComboBoxDefinition;
                slotWidthComboBoxDefinition = (Inventor.ComboBoxDefinition)InventorApplication.CommandManager.ControlDefinitions["Autodesk:SimpleAddIn:SlotWidthCboBox"];

				ComboBoxDefinition slotHeightComboBoxDefinition;
                slotHeightComboBoxDefinition = (Inventor.ComboBoxDefinition)InventorApplication.CommandManager.ControlDefinitions["Autodesk:SimpleAddIn:SlotHeightCboBox"];

				//add new item to combo boxes
				slotWidthComboBoxDefinition.AddItem(Convert.ToString(slotWidthComboBoxDefinition.ListCount + 1) + " cm", 0);
				slotHeightComboBoxDefinition.AddItem(Convert.ToString(slotHeightComboBoxDefinition.ListCount + 1) + " cm", 0);
			}
			catch(Exception e)
			{
				MessageBox.Show(e.ToString());
			}
		}

		#endregion
	}
}
