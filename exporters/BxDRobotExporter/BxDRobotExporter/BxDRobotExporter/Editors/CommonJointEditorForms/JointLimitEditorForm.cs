using System;
using System.Windows.Forms;
using BxDRobotExporter.SkeletalStructure;
using Inventor;

namespace BxDRobotExporter.Editors.CommonJointEditorForms
{
    public partial class JointLimitEditorForm : Form
    {
        //UnitsOfMeasure measure;
        SkeletalJoint_Base joint;

        bool canClose = true;
        double currentPosition = 0, startLimit = 0, endLimit = 0;
        bool writeCurrentPosition = false, writeStartLimit = false, writeEndLimit = false;
        public JointLimitEditorForm(SkeletalJoint_Base joint)
        {
            this.joint = joint;// read in the joint base so we can access the correspodinig Inventor Joint to see/ edit the limits
            InitializeComponent();
            AnalyticUtils.LogPage("EditLimits");
            if (!(((InventorSkeletalJoint)joint).GetWrapped().asmJoint.JointType == AssemblyJointTypeEnum.kCylindricalJointType ||// if the joint is a rotational then enable the rotation stuff and disable the linear
                    ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.JointType == AssemblyJointTypeEnum.kSlideJointType))
            {
                this.LinearGroup.Enabled = false;
                this.Angular_Current_textbox.Value = (decimal)(((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPosition).ModelValue * (180 / Math.PI));// convert from RAD to DEG
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
                    this.Angular_Start_textbox.Value = (decimal)((((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionStartLimit).ModelValue * (180 / Math.PI)));
                    this.Angular_End_textbox.Value = (decimal)((((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionEndLimit).ModelValue * (180 / Math.PI)));
                }
            }
            else// if the joint is a linear joint then activate the linear stuff and deactivate the rotational stuff
            {
                this.Angular_Group_Box.Enabled = false;
                this.Linear_Current_textbox.Value = (decimal)(((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPosition).ModelValue / 2.54);// convert from CM to IN
                this.Linear_Start.Checked = false;
                this.Linear_End.Checked = false;
                this.Linear_Start_textbox.Enabled = false;
                this.Linear_End_textbox.Enabled = false;
                if (((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionStartLimit)// if the joint has a start limit then read it in and activate the corresponding fields
                {
                    this.Linear_Start_textbox.Value = (decimal)((((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionStartLimit).ModelValue) / 2.54);
                    this.Linear_Start_textbox.Enabled = true;
                    this.Linear_Start.Checked = true;
                }
                if (((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionEndLimit)// if the joint has an end limit then read it in and activate the corresponding fields
                {
                    this.Linear_End_textbox.Value = (decimal)((((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionEndLimit).ModelValue) / 2.54);
                    this.Linear_End_textbox.Enabled = true;
                    this.Linear_End.Checked = true;
                }
            }
            base.Text = joint.GetType().Name.Replace("_Base", "").Replace("Joint", " Joint");

            base.Location = new System.Drawing.Point(Cursor.Position.X - 10, Cursor.Position.Y - base.Height - 10);
        }

        private void checkInputs()
        {
            canClose = true;
            currentPosition = 0;
            startLimit = 0;
            endLimit = 0;
            writeCurrentPosition = false;
            writeStartLimit = false;
            writeEndLimit = false;
            if (!(((InventorSkeletalJoint)joint).GetWrapped().asmJoint.JointType == AssemblyJointTypeEnum.kCylindricalJointType ||
                   ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.JointType == AssemblyJointTypeEnum.kSlideJointType))
            {
                currentPosition = Convert.ToDouble(Angular_Current_textbox.Value) * (Math.PI / 180);
                writeCurrentPosition = true;
                if (this.Angular_End.Checked && this.Angular_Start.Checked)
                {
                    startLimit = Convert.ToDouble(Angular_Start_textbox.Value) * (Math.PI / 180);
                    writeStartLimit = true;
                    endLimit = Convert.ToDouble(Angular_End_textbox.Value) * (Math.PI / 180);
                    writeEndLimit = true;
                }
            }
            else
            {
                currentPosition = Convert.ToDouble(Linear_Current_textbox.Text) * 2.54;
                writeCurrentPosition = true;
                if (this.Linear_Start.Checked)
                {
                    startLimit = Convert.ToDouble(Linear_Start_textbox.Text) * 2.54;
                    writeStartLimit = true;
                }
                if (this.Linear_End.Checked)
                {
                    endLimit = Convert.ToDouble(Linear_End_textbox.Text) * 2.54;
                    writeEndLimit = true;
                }
            }
        }

        private void btnOkay_Click(object sender, EventArgs e)
        {
            checkInputs();
            if (!(((InventorSkeletalJoint)joint).GetWrapped().asmJoint.JointType == AssemblyJointTypeEnum.kCylindricalJointType ||
                    ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.JointType == AssemblyJointTypeEnum.kSlideJointType))
            {
                ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasAngularPositionLimits = this.Angular_End.Checked && this.Angular_Start.Checked;
                if (writeCurrentPosition && writeStartLimit && writeEndLimit && ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasAngularPositionLimits)
                {
                    if (!(endLimit > currentPosition && startLimit > currentPosition) &&
                        !(endLimit < currentPosition && startLimit < currentPosition))
                    {
                        if ((Math.Abs(startLimit - endLimit) <= 2 * Math.PI))
                        {
                            ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPosition).Value = currentPosition;
                            ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionStartLimit).Value = startLimit;
                            ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionEndLimit).Value = endLimit;
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = currentPosition + " rad";
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.GoToStart();
                        }
                        else
                        {
                            canClose = false;
                            MessageBox.Show("Please make sure the start and end limits aren't over 360 degrees apart");
                        }
                    }
                    else
                    {
                        canClose = false;
                        MessageBox.Show("Please make sure the current position is between the start and end limits");
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
                if (writeCurrentPosition && writeStartLimit && writeEndLimit && ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionEndLimit &&
                   ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionEndLimit)
                {
                    if (!(endLimit > currentPosition && startLimit > currentPosition) &&
                        !(endLimit < currentPosition && startLimit < currentPosition))
                    {
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPosition).Value = currentPosition;
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionStartLimit).Value = startLimit;
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionEndLimit).Value = endLimit;
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = currentPosition + " cm";
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.GoToStart();
                    } else
                    {
                        canClose = false;
                        MessageBox.Show("Please make sure the current position is between the start and end limits");
                    }

                } else if(writeCurrentPosition && writeStartLimit && ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionStartLimit)
                {
                    ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPosition).Value = currentPosition;
                    ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionStartLimit).Value = startLimit;
                    ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = currentPosition + " cm";
                    ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.GoToStart();
                } else if (writeCurrentPosition && writeEndLimit && ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionEndLimit)
                {
                    ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPosition).Value = currentPosition;
                    ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionEndLimit).Value = endLimit;
                    ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = currentPosition + " cm";
                    ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.GoToStart();
                } else if (writeCurrentPosition)
                {
                    ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPosition).Value = currentPosition;
                    ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = currentPosition + " cm";
                    ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.GoToStart();
                }
            }
            if (canClose)
            {
                LegacyEvents.OnRobotModified();
                Close();
            }
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
                    if (this.Angular_End_textbox.Text.Equals(""))
                    {
                        this.Angular_End_textbox.Value = (decimal)(((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionEndLimit).ModelValue);
                    }
                    if (this.Angular_Start_textbox.Text.Equals(""))
                    {
                        this.Angular_Start_textbox.Value = (decimal)(((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionStartLimit).ModelValue);
                    }
                }
                else
                {
                    this.Angular_Start.Checked = false;
                    this.Angular_End_textbox.Enabled = false;
                    this.Angular_Start_textbox.Enabled = false;
                }
        }

        private void Linear_Current_textbox_ValueChanged(object sender, EventArgs e)
        {
            var pos = Convert.ToDouble(Linear_Current_textbox.Text) * 2.54;
            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = pos + " cm";
            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.EndValue = pos+1 + " cm";
            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.GoToStart();
        }
        private void Angular_Current_textbox_ValueChanged(object sender, EventArgs e)
        {
            var pos = Convert.ToDouble(Angular_Current_textbox.Value) * (Math.PI / 180);
            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = pos + " rad";
            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.EndValue = pos+1 + " rad";
            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.GoToStart();
        }
        private void Angular_Start_CheckedChanged(object sender, EventArgs e)
        {
                if (this.Angular_Start.Checked)
                {
                    this.Angular_End.Checked = true;
                    this.Angular_End_textbox.Enabled = true;
                    this.Angular_Start_textbox.Enabled = true;
                    if (this.Angular_End_textbox.Text.Equals(""))
                    {
                        this.Angular_End_textbox.Value = (decimal)(((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionEndLimit).ModelValue);
                    }
                    if (this.Angular_Start_textbox.Text.Equals(""))
                    {
                        this.Angular_Start_textbox.Value = (decimal)(((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionStartLimit).ModelValue);
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
                if (this.Linear_Start_textbox.Text.Equals(""))
                {
                    this.Linear_Start_textbox.Value = (decimal)((((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionStartLimit).ModelValue) / 2.54);
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
                if (this.Linear_End_textbox.Text.Equals(""))
                {
                    this.Linear_End_textbox.Value = (decimal)((((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionEndLimit).ModelValue) / 2.54);
                }
            }
            else
            {
                this.Linear_End_textbox.Enabled = false;
            }
        }

        private void AnimateJointButton_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            checkInputs();
            double oldStartLimit = 0;
            double oldEndLimit = 0;
            double oldPos = 0;
            bool oldHasLimits = true;
            bool oldHasStartLimits = true;
            bool oldHasEndLimits = true;
            try {
                if (!(((InventorSkeletalJoint)joint).GetWrapped().asmJoint.JointType == AssemblyJointTypeEnum.kCylindricalJointType ||
                     ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.JointType == AssemblyJointTypeEnum.kSlideJointType))
                {
                    if (writeStartLimit && writeEndLimit && this.Angular_Start.Checked && this.Angular_End.Checked)
                    {
                        if ((Math.Abs(startLimit - endLimit) <= 2 * Math.PI))
                        {
                            oldHasLimits = ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasAngularPositionLimits;
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasAngularPositionLimits = true;
                            if (oldHasLimits)
                            {
                                oldStartLimit = ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionStartLimit).ModelValue;
                                oldEndLimit = ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionEndLimit).ModelValue;
                            }
                            oldPos = ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPosition).ModelValue;
                            ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPosition).Value = ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionEndLimit).ModelValue;
                            ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionStartLimit).Value = startLimit;
                            ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionEndLimit).Value = endLimit;
                            if (startLimit <= endLimit)
                            {
                                ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = startLimit + " rad";
                                ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.EndValue = endLimit + " rad";
                            } else
                            {
                                ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = endLimit + " rad";
                                ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.EndValue = startLimit + " rad";
                            }
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.SetIncrement(IncrementTypeEnum.kNumberOfStepsIncrement, Convert.ToString(Math.Abs((startLimit - endLimit) * 12)));
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.GoToStart();
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.PlayForward();
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.PlayReverse();
                            ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPosition).Value = oldPos;
                            if (oldHasLimits)
                            {
                                ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionStartLimit).Value = oldStartLimit;
                                ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionEndLimit).Value = oldEndLimit;
                            }
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = oldPos + " rad";
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.GoToStart();
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasAngularPositionLimits = oldHasLimits;
                        }
                        else
                        {
                            MessageBox.Show("Please make sure the start and end limits aren't over 360 degrees apart");
                        }
                    } else if(! (this.Angular_Start.Checked && this.Angular_End.Checked))
                    {
                        MessageBox.Show("Please make sure you have entered both start and end limits");
                    }
                }
                else
                {
                    if (writeStartLimit && writeEndLimit && Linear_Start.Checked && Linear_End.Checked)
                    {
                        oldHasStartLimits = ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionStartLimit;
                        oldHasEndLimits = ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionEndLimit;
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionStartLimit = true;
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionEndLimit = true;
                        if (oldHasStartLimits)
                        {
                            oldStartLimit = ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionStartLimit).ModelValue;
                        }
                        if (oldHasEndLimits)
                        {
                            oldEndLimit = ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionEndLimit).ModelValue;
                        }
                        oldPos = ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPosition).ModelValue;
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPosition).Value = ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionEndLimit).ModelValue;
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionStartLimit).Value = startLimit;
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionEndLimit).Value = endLimit;
                        if (startLimit <= endLimit)
                        {
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = startLimit + " cm";
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.EndValue = endLimit + " cm";
                        }
                        else
                        {
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = endLimit + " cm";
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.EndValue = startLimit + " cm";
                        }
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.SetIncrement(IncrementTypeEnum.kNumberOfStepsIncrement, Convert.ToString(Math.Abs((startLimit - endLimit)*.25)));
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.GoToStart();
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.PlayForward();
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.PlayReverse();
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPosition).Value = oldPos;
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionStartLimit).Value = oldStartLimit;
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionEndLimit).Value = oldEndLimit;
                        if(oldHasStartLimits)
                        {
                            ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionStartLimit).Value = oldStartLimit;
                        }
                        if (oldHasEndLimits)
                        {
                            ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionEndLimit).Value = oldEndLimit;
                        }
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = oldPos + " cm";
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.GoToStart();
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionStartLimit = oldHasStartLimits;
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionEndLimit = oldHasEndLimits;
                    }
                    else if (!(this.Angular_Start.Checked && this.Angular_End.Checked))
                    {
                        MessageBox.Show("Please make sure you have entered both start and end limits");
                    }
                }
            }
            catch (Exception)//sometimes Inventor gets mad at weird limits, throwing an excpetion, telling it again somehow makes Inventor chill, hence the try/ catch
            {
                if (!(((InventorSkeletalJoint)joint).GetWrapped().asmJoint.JointType == AssemblyJointTypeEnum.kCylindricalJointType ||
                 ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.JointType == AssemblyJointTypeEnum.kSlideJointType))
                {
                    if (writeStartLimit && writeEndLimit && this.Angular_Start.Checked && this.Angular_End.Checked)
                    {
                        if ((Math.Abs(startLimit - endLimit) <= 2 * Math.PI))
                        {
                            ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPosition).Value = ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionEndLimit).ModelValue;
                            ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionStartLimit).Value = startLimit;
                            ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionEndLimit).Value = endLimit;
                            if (startLimit <= endLimit)
                            {
                                ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = startLimit + " rad";
                                ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.EndValue = endLimit + " rad";
                            }
                            else
                            {
                                ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = endLimit + " rad";
                                ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.EndValue = startLimit + " rad";
                            }
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.SetIncrement(IncrementTypeEnum.kNumberOfStepsIncrement, Convert.ToString(Math.Abs((startLimit - endLimit) * 12)));
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.GoToStart();
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.PlayForward();
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.PlayReverse();
                            ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPosition).Value = oldPos;
                            if (oldHasLimits)
                            {
                                ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionStartLimit).Value = oldStartLimit;
                                ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.AngularPositionEndLimit).Value = oldEndLimit;
                            }
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = oldPos + " rad";
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.GoToStart();
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasAngularPositionLimits = oldHasLimits;
                        }
                        else
                        {
                            MessageBox.Show("Please make sure the start and end limits aren't over 360 degrees apart");
                        }
                    }
                    else if (!(this.Angular_Start.Checked && this.Angular_End.Checked))
                    {
                        MessageBox.Show("Please make sure you have entered both start and end limits");
                    }
                }
                else
                {
                    if (writeStartLimit && writeEndLimit)
                    {
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPosition).Value = ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionEndLimit).ModelValue;
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionStartLimit).Value = startLimit;
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionEndLimit).Value = endLimit;
                        if (startLimit <= endLimit)
                        {
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = startLimit + " cm";
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.EndValue = endLimit + " cm";
                        }
                        else
                        {
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = endLimit + " cm";
                            ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.EndValue = startLimit + " cm";
                        }
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.SetIncrement(IncrementTypeEnum.kNumberOfStepsIncrement, Convert.ToString(Math.Abs((startLimit - endLimit) * .25)));
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.GoToStart();
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.PlayForward();
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.PlayReverse();
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPosition).Value = oldPos;
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionStartLimit).Value = oldStartLimit;
                        ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionEndLimit).Value = oldEndLimit;
                        if (oldHasStartLimits)
                        {
                            ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionStartLimit).Value = oldStartLimit;
                        }
                        if (oldHasEndLimits)
                        {
                            ((ModelParameter)((InventorSkeletalJoint)joint).GetWrapped().asmJoint.LinearPositionEndLimit).Value = oldEndLimit;
                        }
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.StartValue = oldPos + " cm";
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJointOccurrence.DriveSettings.GoToStart();
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionStartLimit = oldHasStartLimits;
                        ((InventorSkeletalJoint)joint).GetWrapped().asmJoint.HasLinearPositionEndLimit = oldHasEndLimits;
                    }
                    else if (!(this.Linear_Start.Checked && this.Linear_End.Checked))
                    {
                        MessageBox.Show("Please make sure you have entered both start and end limits");
                    }
                }
            }
            this.Enabled = true;
        }
    }
}
