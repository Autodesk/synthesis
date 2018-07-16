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
        //UnitsOfMeasure measure;
        SkeletalJoint_Base joint;
        public EditLimits(SkeletalJoint_Base joint)
        {
            this.joint = joint;// read in the joint base so we can access the correspodinig Inventor Joint to see/ edit the limits
            InitializeComponent();
            //measure = SynthesisGUI.Instance.MainApplication.UnitsOfMeasure;
            //MessageBox.Show(measure.MassUnits.GetType().ToString());
            if(!(((InventorSkeletalJoint)joint).GetWrapped().asmJoint.JointType == AssemblyJointTypeEnum.kCylindricalJointType ||// if the joint is a rotational then enable the rotation stuff and disable the linear
                    ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.JointType == AssemblyJointTypeEnum.kSlideJointType))
            {
                this.LinearGroup.Enabled = false;
                this.Angular_Current_textbox.Text = Convert.ToString(((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPosition).ModelValue * (180 / Math.PI));// convert from RAD to DEG
                this.Angular_Start.Checked = false;
                this.Angular_End.Checked = false;
                this.Angular_Start_textbox.Enabled = false;
                this.Angular_End_textbox.Enabled = false;
                if (((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasAngularPositionLimits)// check if the joint has limits so we know whether or not to read in the joint data from Inventor and whether or not to activate the angular limits form
                {
                    this.Angular_Start.Checked = true;
                    this.Angular_End.Checked = true;
                    this.Angular_Start_textbox.Enabled = true;
                    this.Angular_End_textbox.Enabled = true;
                    this.Angular_Start_textbox.Text = Convert.ToString((((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionStartLimit).ModelValue * (180 / Math.PI)));
                    this.Angular_End_textbox.Text = Convert.ToString((((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionEndLimit).ModelValue * (180 / Math.PI)));
                }
            }
            else// if the joint is a linear joint then activate the linear stuff and deactivate the rotational stuff
            {
                this.Angular_Group_Box.Enabled = false;
                this.Linear_Current_textbox.Text = Convert.ToString(((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPosition).ModelValue / 2.54);// convert from CM to IN
                this.Linear_Start.Checked = false;
                this.Linear_End.Checked = false;
                this.Linear_Start_textbox.Enabled = false;
                this.Linear_End_textbox.Enabled = false;
                if (((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionStartLimit)// if the joint has a start limit then read it in and activate the corresponding fields
                {
                    this.Linear_Start_textbox.Text = Convert.ToString((((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionStartLimit).ModelValue) / 2.54);
                    this.Linear_Start_textbox.Enabled = true;
                    this.Linear_Start.Checked = true;
                }
                if (((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionEndLimit)// if the joint has an end limit then read it in and activate the corresponding fields
                {
                    this.Linear_End_textbox.Text = Convert.ToString((((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionEndLimit).ModelValue) / 2.54);
                    this.Linear_End_textbox.Enabled = true;
                    this.Linear_End.Checked = true;
                }
            }
            base.Text = joint.GetType().Name.Replace("_Base", "").Replace("Joint", " Joint");

            base.Location = new System.Drawing.Point(Cursor.Position.X - 10, Cursor.Position.Y - base.Height - 10);
        }

        private void btnOkay_Click(object sender, EventArgs e)
        {
            bool canClose = true;
            if (!(((InventorSkeletalJoint)joint).GetWrapped().asmJoint.JointType == AssemblyJointTypeEnum.kCylindricalJointType ||
                    ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.JointType == AssemblyJointTypeEnum.kSlideJointType))
            {
                if (this.Angular_End.Checked && this.Angular_Start.Checked)
                {
                    ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasAngularPositionLimits = true;
                    try
                    {
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPosition).Value = Convert.ToDouble(Angular_Current_textbox.Text) * (Math.PI / 180);
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
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionStartLimit).Value = Convert.ToDouble(Angular_Start_textbox.Text) * (Math.PI / 180);
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
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionEndLimit).Value = Convert.ToDouble(Angular_End_textbox.Text) * (Math.PI / 180);
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
                }
                else
                {
                    ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasAngularPositionLimits = false;
                }
            }
            else
            {
                
                ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionStartLimit = this.Linear_Start.Checked;
                ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionEndLimit = this.Linear_End.Checked;
                double currentPosition = 0, startLimit = 0, endLimit = 0;
                bool writeCurrentPosition = false, writeStartLimit = false, writeEndLimit = false;
                try
                {
                    currentPosition = Convert.ToDouble(Linear_Current_textbox.Text) * 2.54;
                    writeCurrentPosition = true;
                }
                catch (Exception)
                {
                    canClose = false;
                    if (Linear_Current_textbox.Text.Equals(""))
                    {
                        MessageBox.Show("Error, please make sure that the Current text box has a position");
                    }
                    else
                    {
                        MessageBox.Show("Error, please make sure that the Current text box has a position");
                    }
                }
                try
                {
                    if (((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionStartLimit)
                    {
                        startLimit = Convert.ToDouble(Linear_Start_textbox.Text) * 2.54;
                        writeStartLimit = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    canClose = false;
                    if (Linear_Start_textbox.Text.Equals(""))
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
                    if (((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionEndLimit)
                    {
                        endLimit = Convert.ToDouble(Linear_End_textbox.Text) * 2.54;
                        writeEndLimit = true;
                    }
                }
                catch (Exception)
                {
                    canClose = false;
                    if (Linear_End_textbox.Text.Equals(""))
                    {
                        MessageBox.Show("Error, please make sure that the End text box has a limit");
                    }
                    else
                    {
                        MessageBox.Show("Error, please make sure that the End text box has only numbers in it");
                    }
                }
                if (writeCurrentPosition && writeStartLimit && writeEndLimit)
                {
                    if (!(endLimit > currentPosition && startLimit > currentPosition) &&
                        !(endLimit < currentPosition && startLimit < currentPosition))
                    {
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPosition).Value = currentPosition;
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionStartLimit).Value = startLimit;
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionEndLimit).Value = endLimit;
                    } else
                    {
                        canClose = false;
                        MessageBox.Show("Please make sure the current position is between the start and end limits");
                    }

                } else if(writeCurrentPosition && writeStartLimit)
                {
                    ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPosition).Value = currentPosition;
                    ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionStartLimit).Value = startLimit;
                } else if (writeCurrentPosition && writeEndLimit)
                {
                    ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPosition).Value = currentPosition;
                    ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionEndLimit).Value = endLimit;
                } else if (writeCurrentPosition)
                {
                    ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPosition).Value = currentPosition;
                }
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

        private void Linear_Start_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Linear_Start.Checked)
            {
                this.Linear_Start_textbox.Enabled = true;
                try
                {
                    if (this.Linear_Start_textbox.Text.Equals(""))
                    {
                        this.Linear_Start_textbox.Text = Convert.ToString((((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionStartLimit).ModelValue) / 2.54);
                    }
                }
                catch (Exception)
                {

                }
            }
            else
            {
                this.Linear_Start_textbox.Enabled = false;
            }
        }

        private void Linear_End_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Linear_End.Checked)
            {
                this.Linear_End_textbox.Enabled = true;
                try
                {
                    if (this.Linear_End_textbox.Text.Equals(""))
                    {
                        this.Linear_End_textbox.Text = Convert.ToString((((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionEndLimit).ModelValue) / 2.54);
                    }
                }
                catch (Exception)
                {

                }
            }
            else
            {
                this.Linear_End_textbox.Enabled = false;
            }
        }
    }
}
