using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditorsLibrary
{
    public partial class PleaseWaitForm : Form
    {

        public bool finished;

        public PleaseWaitForm()
        {
            InitializeComponent();
        }

        private void PleaseWaitForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!finished) e.Cancel = true;
        }

    }
}
