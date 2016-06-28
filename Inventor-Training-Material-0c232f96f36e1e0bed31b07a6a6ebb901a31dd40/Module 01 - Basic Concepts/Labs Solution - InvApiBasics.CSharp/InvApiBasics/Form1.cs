using System;
using Inventor;

namespace InvApiBasics
{
    public partial class Form1: System.Windows.Forms.Form
    {
        Inventor.Application m_inventorApp = null;

        DigitTextBox DigitTextBox1; 
        DigitTextBox DigitTextBox2;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DigitTextBox1 = new DigitTextBox();
            DigitTextBox1.Name = "DigitTextBox1";
            DigitTextBox1.Location = new System.Drawing.Point(80, 15);
            DigitTextBox1.Size = new System.Drawing.Size(80, 10);
            DigitTextBox1.TabIndex = 1;
            DigitTextBox1.Text = "";

            DigitTextBox2 = new DigitTextBox();
            DigitTextBox2.Name = "DigitTextBox2";
            DigitTextBox2.Location = new System.Drawing.Point(80, 40);
            DigitTextBox2.Size = new System.Drawing.Size(80, 10);
            DigitTextBox2.TabIndex = 2;
            DigitTextBox2.Text = "";

            this.Controls.Add(DigitTextBox1);
            this.Controls.Add(DigitTextBox2);

            try //Try to get an active instance of Inventor
            {
                try
                {
                    m_inventorApp = System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application") as Inventor.Application;
                }
                catch
                {
                    Type inventorAppType = System.Type.GetTypeFromProgID("Inventor.Application");

                    m_inventorApp = System.Activator.CreateInstance(inventorAppType) as Inventor.Application;

                    //Must be set visible explicitly
                    m_inventorApp.Visible = true;
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Error: couldn't create Inventor instance");
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            View oView = m_inventorApp.ActiveView;

            if (oView != null)
            {
                if ((DigitTextBox1.Text.Length > 0) && (DigitTextBox2.Text.Length > 0))
                {
                    oView.Width = System.Int32.Parse(DigitTextBox1.Text);
                    oView.Height = System.Int32.Parse(DigitTextBox2.Text);
                }
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (TextBox3.Text.Length > 0)
            {
                m_inventorApp.Caption = TextBox3.Text;                 
            }
        }
    }
}
