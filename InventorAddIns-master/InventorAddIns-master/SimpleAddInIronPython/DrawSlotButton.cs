using System;
using System.Windows.Forms;
using System.Drawing;
using Inventor;

//These directives are needed to work with IronPython from C#
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using IronPython.Runtime;

namespace SimpleAddIn
{
	/// <summary>
	/// DrawSlotButton class
	/// </summary>
	
	internal class DrawSlotButton : Button
	{
		

		public DrawSlotButton(string displayName, string internalName, CommandTypesEnum commandType, string clientId, string description, string tooltip, Icon standardIcon, Icon largeIcon, ButtonDisplayEnum buttonDisplayType)
            : base(displayName, internalName, commandType, clientId, description, tooltip, standardIcon, largeIcon, buttonDisplayType)
		{
			
		}

		public DrawSlotButton(string displayName, string internalName, CommandTypesEnum commandType, string clientId, string description, string tooltip, ButtonDisplayEnum buttonDisplayType)
			: base(displayName, internalName, commandType, clientId, description, tooltip, buttonDisplayType)
		{
			
		}

		override protected void ButtonDefinition_OnExecute(NameValueMap context)
		{
			try
			{
				//check to make sure a sketch is active
				if (InventorApplication.ActiveEditObject is PlanarSketch)
				{
					//if same session, combobox definitions will already exist
					
					Inventor.Application oAppTest;
					oAppTest = InventorApplication;
					string filePath;
					filePath = "C:\\Projects\\InventorAPI\\InventorAddIns\\SimpleAddInIronPython\\PythonTest.py";
					PlanarSketch  planarSketch;
					planarSketch = (PlanarSketch)InventorApplication.ActiveEditObject;
					ComboBoxDefinition slotWidthComboBoxDefinition;
                    slotWidthComboBoxDefinition = (Inventor.ComboBoxDefinition)InventorApplication.CommandManager.ControlDefinitions["Autodesk:SimpleAddIn:SlotWidthCboBox"];
					ComboBoxDefinition slotHeightComboBoxDefinition;
                    slotHeightComboBoxDefinition = (Inventor.ComboBoxDefinition)InventorApplication.CommandManager.ControlDefinitions["Autodesk:SimpleAddIn:SlotHeightCboBox"];

					//get the selected width from combo box
					double slotWidth;
					slotWidth = slotWidthComboBoxDefinition.ListIndex;
		
					//get the selected height from combo box
					double slotHeight;
					slotHeight = slotHeightComboBoxDefinition.ListIndex;
					
					Engine thisEngine = new Engine(filePath,planarSketch,oAppTest,slotHeight,slotWidth);
					if (slotWidth > 0 && slotHeight > 0)
					{
						bool thisRun;
						thisRun = thisEngine.Execute();
					}
					else
					{
						//valid width/height was not specified
						MessageBox.Show("Please specify valid slot width and height");
					}
		
				}
				else
				{
					//no sketch is active, so display an error
					MessageBox.Show("A sketch must be active for this command");

				}		
			}
			catch(Exception e)
			{
				MessageBox.Show(e.ToString());
			}
		}
	}
}

