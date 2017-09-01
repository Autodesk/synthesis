using System;
using System.Windows.Forms;
using Inventor;

namespace BxDFieldExporter

{
    public partial class AddPart : Form
    {
        //Used to access StandardAddInServer's exposed API
        private Inventor.Application mApplication;
        private IAutomationInterface mAddInInterface;

        public AddPart()
        {
            Location = new System.Drawing.Point(450, 350);
            InitializeComponent();
            TopMost = true;

            //Used to access StandardAddInServer's exposed API
            try
            {
                mApplication = System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application") as Inventor.Application;
            }

            catch
            {
                Type inventorAppType = System.Type.GetTypeFromProgID("Inventor.Application");
                mApplication = System.Activator.CreateInstance(inventorAppType) as Inventor.Application;
            }
            mApplication.Visible = true;

            //Iterates through Inventor Add-Ins collection  
            foreach (ApplicationAddIn oAddIn in mApplication.ApplicationAddIns)
            {
                //Looks for our DemoAddin CLSID;
                if (oAddIn.ClassIdString == "{E50BE244-9F7B-4B94-8F87-8224FABA8CA1}")
                {

                    //Calls Automation property    
                    mAddInInterface = (IAutomationInterface)oAddIn.Automation;
                }
            }
        }
        /// <summary>
        /// Closes the form and adds the last selected model to the selected component
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_OnClick(object sender, EventArgs e)
        {
            mAddInInterface.SetDone(true);
            StandardAddInServer.task.TrySetResult(true);
            Close();
        }
        /// <summary>
        /// Closes the form and doesn't add the last selected model to the selected component
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_onClick(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Checks for keyinput and closes the form with or without saving the model to a component depending on the entered key
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
            }
            else if (keyData == Keys.Enter)
            {
                OKButton_OnClick(null, null);
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
        /// <summary>
        /// Leaves the form open and adds the last selected model to the selected component
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyButton_Click(object sender, EventArgs e)
        {
            StandardAddInServer.task.TrySetResult(true);
        }

        private void AddPart_FormClosing(object sender, FormClosingEventArgs e)
        {
            mAddInInterface.SetCancel(true);
            StandardAddInServer.task.TrySetResult(true);
        }
    }
}
