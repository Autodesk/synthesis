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
            base.Text = joint.GetType().Name.Replace("_Base", "").Replace("Joint", " Joint");

            base.Location = new System.Drawing.Point(Cursor.Position.X - 10, Cursor.Position.Y - base.Height - 10);
        }

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
            LegacyInterchange.LegacyEvents.OnRobotModified();
            Close();
        }

        private void restoreValues()
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

        void EditLimits_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            restoreValues();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            restoreValues();
            Close();
        }
    }
}
