using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BxDFieldExporter;
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


        private void SelectPartsLabel_onClick(object sender, EventArgs e)
        {
            Close();
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
            }
            else if (keyData == Keys.Enter)
            {
                Close();
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            StandardAddInServer.task.TrySetResult(true);
        }

        private void okButton_Click(object sender, EventArgs e)
        {

        }
    }
}
