using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;

using InventorServices.Persistence;

namespace DynamoInventor
{
    internal abstract class Button
    {
		#region Private fields		
		private ButtonDefinition buttonDefinition;

        private ButtonDefinitionSink_OnExecuteEventHandler ButtonDefinition_OnExecuteEventDelegate;
		#endregion		

		#region Public properties
		public Inventor.ButtonDefinition ButtonDefinition
		{
			get { return buttonDefinition; }
		}

		#endregion
        
		#region Public constructors

		public Button(string displayName, 
                      string internalName, 
                      CommandTypesEnum commandType, 
                      string clientId, 
                      string description, 
                      string tooltip, 
                      Icon standardIcon, 
                      Icon largeIcon, 
                      ButtonDisplayEnum buttonDisplayType)
		{
			try
			{
				stdole.IPictureDisp standardIconIPictureDisp;				
				standardIconIPictureDisp = PictureDispConverter.ToIPictureDisp(standardIcon);			
				stdole.IPictureDisp largeIconIPictureDisp;
		        largeIconIPictureDisp = PictureDispConverter.ToIPictureDisp(largeIcon);   
				buttonDefinition = PersistenceManager.InventorApplication.CommandManager.ControlDefinitions.AddButtonDefinition(displayName, 
                                                                                                                             internalName, 
                                                                                                                             commandType, 
                                                                                                                             clientId, 
                                                                                                                             description, 
                                                                                                                             tooltip, 
                                                                                                                             standardIconIPictureDisp , 
                                                                                                                             largeIconIPictureDisp, 
                                                                                                                             buttonDisplayType);
												
                buttonDefinition.Enabled = true;			
                ButtonDefinition_OnExecuteEventDelegate = new ButtonDefinitionSink_OnExecuteEventHandler(ButtonDefinition_OnExecute);
                buttonDefinition.OnExecute += ButtonDefinition_OnExecuteEventDelegate;
			}

			catch(Exception e)
			{
                throw new ApplicationException(e.ToString());
			}
		}

		public Button(string displayName, 
                      string internalName, 
                      CommandTypesEnum commandType, 
                      string clientId, 
                      string description, 
                      string tooltip, 
                      ButtonDisplayEnum buttonDisplayType)
		{
			try
			{			
                buttonDefinition = PersistenceManager.InventorApplication.CommandManager.ControlDefinitions.AddButtonDefinition(displayName, 
                                                                                                                             internalName, 
                                                                                                                             commandType, 
                                                                                                                             clientId, 
                                                                                                                             description, 
                                                                                                                             tooltip, 
                                                                                                                             Type.Missing, 
                                                                                                                             Type.Missing, 
                                                                                                                             buttonDisplayType);
								
                buttonDefinition.Enabled = true;
				ButtonDefinition_OnExecuteEventDelegate = new ButtonDefinitionSink_OnExecuteEventHandler(ButtonDefinition_OnExecute);
                buttonDefinition.OnExecute += ButtonDefinition_OnExecuteEventDelegate;
			}

			catch(Exception e)
			{
                throw new ApplicationException(e.ToString());
			}
		}

		abstract protected void ButtonDefinition_OnExecute(NameValueMap context);

		#endregion
	}
}
