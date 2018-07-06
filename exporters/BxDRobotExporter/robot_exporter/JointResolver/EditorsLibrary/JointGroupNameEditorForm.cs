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
    public partial class JointGroupNameEditorForm : Form
    {

        public string NewName;

        public JointGroupNameEditorForm(string name)
        {
            InitializeComponent();

            textBox1.Text = name;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0) NewName = textBox1.Text;
            LegacyInterchange.LegacyEvents.OnRobotModified();
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
        
    }
}
