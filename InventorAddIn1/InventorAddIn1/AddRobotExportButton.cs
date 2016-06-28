using System;
using System.Windows.Forms;
using System.Drawing;
using Inventor;

namespace SimpleAddIn
{
    /// <summary>
    /// DrawSlotButton class
    /// </summary>

    internal class AddRobotExportButton : Button
    {


        public AddRobotExportButton(string displayName, string internalName, CommandTypesEnum commandType, string clientId, string description, string tooltip, Icon standardIcon, Icon largeIcon, ButtonDisplayEnum buttonDisplayType)
            : base(displayName, internalName, commandType, clientId, description, tooltip, standardIcon, largeIcon, buttonDisplayType)
        {

        }

        public AddRobotExportButton(string displayName, string internalName, CommandTypesEnum commandType, string clientId, string description, string tooltip, ButtonDisplayEnum buttonDisplayType)
            : base(displayName, internalName, commandType, clientId, description, tooltip, buttonDisplayType)
        {

        }

        override protected void ButtonDefinition_OnExecute(NameValueMap context)
        {
            try
            {
                MessageBox.Show("Exporting dat robit SOOOOOOOO HARD");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}


