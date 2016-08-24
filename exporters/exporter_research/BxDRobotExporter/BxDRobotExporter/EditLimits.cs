using Inventor;
using System;
using System.Windows.Forms;

namespace BxDRobotExporter {
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
                joint.UpperLim = Convert.ToDouble(txtUpper.Text);
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
                joint.LowerLim = Convert.ToDouble(txtLower.Text);
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
            txtLower.Text = j.LowerLim.ToString();
            txtUpper.Text = j.UpperLim.ToString();
            if (joint.jointOfType.Definition.JointType == AssemblyJointTypeEnum.kCylindricalJointType ||
                    joint.jointOfType.Definition.JointType == AssemblyJointTypeEnum.kSlideJointType)
            {
                this.lblTitle.Location = new System.Drawing.Point(75, 15);
                this.lblTitle.Text = "Edit Linear Limits(mm)";
            }
            else
            {
                this.lblTitle.Location = new System.Drawing.Point(61, 15);
                this.lblTitle.Text = "Edit Rotational Limits(Rad)";
            }
        }
    }
}
