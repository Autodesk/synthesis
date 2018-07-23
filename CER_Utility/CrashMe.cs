using System;
using System.Windows.Forms;

namespace CER_Utility
{
    public partial class CrashMe : Form
    {
        public CrashMe()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int i = 0;
            int j = 5 / i;
        }
    }
}
