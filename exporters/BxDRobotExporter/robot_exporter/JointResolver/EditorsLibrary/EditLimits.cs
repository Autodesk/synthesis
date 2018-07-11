using Inventor;
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
        SkeletalJoint_Base joint;
        public EditLimits(SkeletalJoint_Base joint)
        {
            this.joint = joint;
            InitializeComponent();
            if (((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasAngularPositionLimits)
            {
                MessageBox.Show(((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionStartLimit).ModelValue + " ");
            }
            IEnumerator<AngularDOF> angularDOF = joint.GetAngularDOF().GetEnumerator();
            {
                int i = 0;
                bool hasAnother = angularDOF.MoveNext();
                if (((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasAngularPositionLimits)
                {
                    this.Angular_Start.Checked = true;
                    this.Angular_End.Checked = true;
                    this.Angular_Current_textbox.Text = Convert.ToString(((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPosition).ModelValue);
                    this.Angular_Start_textbox.Text = Convert.ToString((((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionStartLimit).ModelValue ));
                    this.Angular_End_textbox.Text = Convert.ToString((((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionEndLimit).ModelValue));
                } else if(! (((InventorSkeletalJoint)joint).GetWrapped().asmJoint.JointType == AssemblyJointTypeEnum.kCylindricalJointType ||
                    ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.JointType == AssemblyJointTypeEnum.kSlideJointType))
                {
                    this.Angular_Start.Checked = false;
                    this.Angular_End.Checked = false;
                    this.Angular_Current_textbox.Text = Convert.ToString(((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPosition).ModelValue);
                    this.Angular_Start_textbox.Enabled = false;
                    this.Angular_End_textbox.Enabled = false;
                }
            }
            IEnumerator<LinearDOF> linearDOF = joint.GetLinearDOF().GetEnumerator();
            {
                int i = 0;
                bool hasAnother = linearDOF.MoveNext();
                //while (hasAnother)
                //{
                //    tabDOF.TabPages.Add(new LimitPane<LinearDOF>(
                //        linearDOF.Current, "Linear" + ((hasAnother = linearDOF.MoveNext()) ? " #" + (++i) : "")));
                //}
            }
            base.Text = joint.GetType().Name.Replace("_Base", "").Replace("Joint", " Joint");

            base.Location = new System.Drawing.Point(Cursor.Position.X - 10, Cursor.Position.Y - base.Height - 10);
        }

        private void btnOkay_Click(object sender, EventArgs e)
        {
            bool canClose = true;
            if (this.Angular_End.Checked && this.Angular_Start.Checked)
            {
                ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasAngularPositionLimits = true;
                try
                {
                    ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPosition).Value = Convert.ToDouble(Angular_Current_textbox.Text);
                }
                catch (Exception)
                {
                    canClose = false;
                    if (Angular_Current_textbox.Text.Equals(""))
                    {
                        MessageBox.Show("Error, please make sure that the Curremt text box has a position");
                    }
                    else
                    {
                        MessageBox.Show("Error, please make sure that the Current text box has a position");
                    }
                }
                try
                {
                    ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionStartLimit).Value = Convert.ToDouble(Angular_Start_textbox.Text);
                }
                catch (Exception)
                {
                    canClose = false;
                    if (Angular_Start_textbox.Text.Equals(""))
                    {
                        MessageBox.Show("Error, please make sure that the Start text box has a limit");
                    }
                    else
                    {
                        MessageBox.Show("Error, please make sure that the Start text box has only numbers in it");
                    }
                }
                try
                {
                    ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionEndLimit).Value = Convert.ToDouble(Angular_End_textbox.Text);
                }
                catch (Exception)
                {
                    canClose = false;
                    if (Angular_End_textbox.Text.Equals(""))
                    {
                        MessageBox.Show("Error, please make sure that the End text box has a limit");
                    }
                    else
                    {
                        MessageBox.Show("Error, please make sure that the End text box has only numbers in it");
                    }
                }
            } else
            {
                ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasAngularPositionLimits = false;
                canClose = true;
            }
            if (canClose)
            {
                LegacyInterchange.LegacyEvents.OnRobotModified();
                Close();
            }
        }

        void EditLimits_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Angular_End_CheckedChanged(object sender, EventArgs e)
        {
                if (this.Angular_End.Checked)
                {
                    this.Angular_Start.Checked = true;
                    this.Angular_End_textbox.Enabled = true;
                    this.Angular_Start_textbox.Enabled = true;
                    try
                    {
                        if (this.Angular_End_textbox.Text.Equals(""))
                        {
                            this.Angular_End_textbox.Text = Convert.ToString((((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionEndLimit).ModelValue));
                        }
                    }
                    catch (Exception)
                    {

                    }
                    try
                    {
                        if (this.Angular_Start_textbox.Text.Equals(""))
                        {
                            this.Angular_Start_textbox.Text = Convert.ToString((((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionStartLimit).ModelValue));
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                else
                {
                    this.Angular_Start.Checked = false;
                    this.Angular_End_textbox.Enabled = false;
                    this.Angular_Start_textbox.Enabled = false;
                }
        }

        private void Angular_Start_CheckedChanged(object sender, EventArgs e)
        {
                if (this.Angular_Start.Checked)
                {
                    this.Angular_End.Checked = true;
                    this.Angular_End_textbox.Enabled = true;
                    this.Angular_Start_textbox.Enabled = true;
                    try
                    {
                        if (this.Angular_End_textbox.Text.Equals(""))
                        {
                            this.Angular_End_textbox.Text = Convert.ToString((((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionEndLimit).ModelValue));
                        }
                    }
                    catch (Exception)
                    {

                    }
                    try
                    {
                    if (this.Angular_Start_textbox.Text.Equals(""))
                    {
                        this.Angular_Start_textbox.Text = Convert.ToString((((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionStartLimit).ModelValue));
                    }
                }
                    catch (Exception)
                    {

                    }
                }
                else
                {
                    this.Angular_End.Checked = false;
                    this.Angular_End_textbox.Enabled = false;
                    this.Angular_Start_textbox.Enabled = false;
                }
        }
    }
}
