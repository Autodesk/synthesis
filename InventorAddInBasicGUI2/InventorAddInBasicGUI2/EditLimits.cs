using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventorAddInBasicGUI2
{
    public partial class EditLimits : Form
    {
        JointData joint;
        public EditLimits()
        {
            InitializeComponent();

        }
        // reactor to the upper limits text box, writes the changes to the jointdata
        public void UpperLimitChanged(object sender, EventArgs e)
        {
            try
            {
                joint.upperLim = Convert.ToDouble(txtUpper.Text);
            }
            catch(Exception ex)
            {
                if (txtUpper.Text.Length > 0)
                {
                    MessageBox.Show("warning, incorrect input");
                }
            }
        }
        // closes the window so the user get the feedback of knowing the form was saved, doesn't actually save anything
        public void SaveButtonPressed(object sender, EventArgs e)
        {
            this.Close();
        }
        // reactor to the lower limits text box, writes the changes to the jointdata
        public void LowerLimitChanged(object sender, EventArgs e)
        {
            try
            {
                joint.lowerLim = Convert.ToDouble(txtLower.Text);
            }
            catch(Exception ex)
            {
                if (txtLower.Text.Length > 0)
                {
                    MessageBox.Show("warning, incorrect input");
                }
            }
        }
        // reads the data from jointdata and put the fields to the correct value
        public void readFromData(JointData j)
        {
            joint = j;
            txtLower.Text = j.lowerLim.ToString();
            txtUpper.Text = j.upperLim.ToString();
        }
    }
}
