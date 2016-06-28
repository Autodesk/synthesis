using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ApprenticeDemo
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        public  Form2(string filename)
        {
        
        //This call is required by the Windows Form Designer.
        InitializeComponent();

        //Add any initialization after the InitializeComponent() call.
        axInventorViewControl1.FileName = filename;
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            axInventorViewControl1.Size = this.Size;
        }
    }
}
