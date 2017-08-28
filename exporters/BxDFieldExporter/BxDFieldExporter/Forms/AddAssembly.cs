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

        private void OKButton_OnClick(object sender, EventArgs e)
        {
            mAddInInterface.SetDone(true);
            StandardAddInServer.task.TrySetResult(true);
            Close();
        }

        private void CancelButton_onClick(object sender, EventArgs e)
        {
            mAddInInterface.SetCancel(true);
            StandardAddInServer.task.TrySetResult(true);
            Close();
        }

        private void ApplyButton_OnClick(object sender, EventArgs e)
        {
            StandardAddInServer.task.TrySetResult(true);
        }

        private void AddAssembly_Load(object sender, EventArgs e)
        {

        }

        private void CancelButton_onClick(object sender, FormClosedEventArgs e)
        {
            Close();
        }

        private void AddAssembly_FormClosing(object sender, FormClosingEventArgs e)
        {
            mAddInInterface.SetCancel(true);
            StandardAddInServer.task.TrySetResult(true);
        }

        private void AddAssembly_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.ShiftKey))
            {
                MessageBox.Show("WARNING: The field exporter does not accept multiple models at this time.");
                    
            }
        }

        ///// <summary>
        ///// Override ProcessCmdKey in order to collect escape and enter key input
        ///// </summary>
        ///// <param name="msg"></param>
        ///// <param name="keyData"></param>
        ///// <returns></returns>
        //protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        //{
        //    if (keyData == Keys.Escape)
        //    {
        //        this.Close();

        //    }
        //    else if (keyData == Keys.Enter)
        //    {
        //        this.Close();
        //    }

        //    return base.ProcessCmdKey(ref msg, keyData);
        //}


    }
}