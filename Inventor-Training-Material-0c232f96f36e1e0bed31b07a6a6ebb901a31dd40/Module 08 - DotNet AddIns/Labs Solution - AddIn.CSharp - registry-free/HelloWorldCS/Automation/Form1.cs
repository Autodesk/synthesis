using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace Automation
{
    public partial class Form1 : Form
    {
        Inventor.Application _InvApplication;
        HelloWorldCS.AutomationInterface _InvAddInInterface; 

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                _InvApplication = System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application") as Inventor.Application;
            }

            catch
            {
                Type inventorAppType = System.Type.GetTypeFromProgID("Inventor.Application");
                _InvApplication = System.Activator.CreateInstance(inventorAppType) as Inventor.Application;
            }

            _InvApplication.Visible = true;

            string addInCLSID = "{28e1f9bc-44fb-464c-ba98-e9c14c7eed44}";

            Inventor.ApplicationAddIn addIn = _InvApplication.ApplicationAddIns.get_ItemById(addInCLSID.ToUpper());

            //Make sure addin is activated
            if (!addIn.Activated)
            {
                addIn.Activate();
            }

            _InvAddInInterface = addIn.Automation as HelloWorldCS.AutomationInterface;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _InvAddInInterface.Execute(textBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string param = _InvAddInInterface.GetParam();
            System.Windows.Forms.MessageBox.Show(param, "Param from AddIn");
        }
    }
}
