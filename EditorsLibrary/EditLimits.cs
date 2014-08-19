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
    public partial class EditLimits : Form
    {
        public EditLimits(SkeletalJoint_Base joint)
        {
            InitializeComponent();
            IEnumerator<AngularDOF> angularDOF = joint.GetAngularDOF().GetEnumerator();
            {
                int i = 0;
                bool hasAnother = angularDOF.MoveNext();
                while (hasAnother)
                {
                    tabDOF.TabPages.Add(new LimitPane<AngularDOF>(
                        angularDOF.Current, "Angular" + ((hasAnother = angularDOF.MoveNext()) ? " #" + (++i) : "")));
                }
            }
            IEnumerator<LinearDOF> linearDOF = joint.GetLinearDOF().GetEnumerator();
            {
                int i = 0;
                bool hasAnother = linearDOF.MoveNext();
                while (hasAnother)
                {
                    tabDOF.TabPages.Add(new LimitPane<LinearDOF>(
                        linearDOF.Current, "Linear" + ((hasAnother = linearDOF.MoveNext()) ? " #" + (++i) : "")));
                }
            }
        }

        bool savedValues = false;
        private void btnOkay_Click(object sender, EventArgs e)
        {
            // Force set values
            foreach (TabPage page in tabDOF.TabPages)
            {
                if (page is LimitPane<LinearDOF> && !((LimitPane<LinearDOF>) page).changedProps(true))
                    return;
                else if (page is LimitPane<AngularDOF> && !((LimitPane<AngularDOF>) page).changedProps(true))
                    return;
            }
            savedValues = true;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Restore values
            if (!savedValues)
            {
                foreach (TabPage page in tabDOF.TabPages)
                {
                    if (page is LimitPane<LinearDOF>)
                    {
                        ((LimitPane<LinearDOF>) page).resetProps();
                    }
                    else if (page is LimitPane<AngularDOF>)
                    {
                        ((LimitPane<AngularDOF>) page).resetProps();
                    }
                }
            }
            Close();
        }
    }
}
