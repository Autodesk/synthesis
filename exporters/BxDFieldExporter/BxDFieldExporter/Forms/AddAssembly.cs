using System;
using System.Windows.Forms;
using Inventor;

namespace BxDFieldExporter
{
    public partial class AddAssembly : Form
    {
        //Used to access StandardAddInServer's exposed API
        private Inventor.Application mApplication;
        private IAutomationInterface mAddInInterface;

        public AddAssembly()
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
                //Looks for StandardAddInServer's Class ID;
                if (oAddIn.ClassIdString == "{E50BE244-9F7B-4B94-8F87-8224FABA8CA1}")
                {
                    //Calls Automation property    
                    mAddInInterface = (IAutomationInterface)oAddIn.Automation;
                }

            }
        }
        /// <summary>
        /// When the okay button is pressed, closes the form and adds the last selected model to the selected component
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
        /// Closes the form and does not add the last selected model to the component
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_onClick(object sender, EventArgs e)
        {
            mAddInInterface.SetCancel(true);
            StandardAddInServer.task.TrySetResult(true);
            Close();
        }
        /// <summary>
        /// Leaves the form open and adds the last selected model to the component
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyButton_OnClick(object sender, EventArgs e)
        {
            StandardAddInServer.task.TrySetResult(true);
        }
        /// <summary>
        /// Properly closes the form so Standard Addin Server is aware of it closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddAssembly_FormClosing(object sender, FormClosingEventArgs e)
        {
            mAddInInterface.SetCancel(true);
            StandardAddInServer.task.TrySetResult(true);
        }

    }
}