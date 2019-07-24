using System;
using System.Windows.Forms;
using BxDRobotExporter.SkeletalStructure;
using Inventor;

namespace BxDRobotExporter.GUI.Editors.JointSubEditors
{
    public partial class JointLimitEditorForm : Form
    {
        //UnitsOfMeasure measure;
        private SkeletalJoint_Base joint;

        private bool canClose = true;
        private double currentPosition = 0, startLimit = 0, endLimit = 0;
        private bool writeCurrentPosition = false, writeStartLimit = false, writeEndLimit = false;
        public JointLimitEditorForm(SkeletalJoint_Base joint)
        {
            this.joint = joint;// read in the joint base so we can access the correspodinig Inventor Joint to see/ edit the limits
            InitializeComponent();
            AnalyticsUtils.LogPage("EditLimits");
            if (!(((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.JointType == AssemblyJointTypeEnum.kCylindricalJointType ||// if the joint is a rotational then enable the rotation stuff and disable the linear
                    ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.JointType == AssemblyJointTypeEnum.kSlideJointType))
            {
                this.LinearGroup.Enabled = false;
                this.Angular_Current_textbox.Value = (decimal)(((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPosition).ModelValue * (180 / Math.PI));// convert from RAD to DEG
                this.Angular_Start.Checked = false;
                this.Angular_End.Checked = false;
                this.Angular_Start_textbox.Enabled = false;
                this.Angular_End_textbox.Enabled = false;
                if (((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasAngularPositionLimits)// check if the joint has limits so we know whether or not to read in the joint data from Inventor and whether or not to activate the angular limits form
                {
                    this.Angular_Start.Checked = true;
                    this.Angular_End.Checked = true;
                    this.Angular_Start_textbox.Enabled = true;
                    this.Angular_End_textbox.Enabled = true;
                    this.Angular_Start_textbox.Value = (decimal)((((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionStartLimit).ModelValue * (180 / Math.PI)));
                    this.Angular_End_textbox.Value = (decimal)((((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionEndLimit).ModelValue * (180 / Math.PI)));
                }
            }
            else// if the joint is a linear joint then activate the linear stuff and deactivate the rotational stuff
            {
                this.Angular_Group_Box.Enabled = false;
                this.Linear_Current_textbox.Value = (decimal)(((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPosition).ModelValue / 2.54);// convert from CM to IN
                this.Linear_Start.Checked = false;
                this.Linear_End.Checked = false;
                this.Linear_Start_textbox.Enabled = false;
                this.Linear_End_textbox.Enabled = false;
                if (((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasLinearPositionStartLimit)// if the joint has a start limit then read it in and activate the corresponding fields
                {
                    this.Linear_Start_textbox.Value = (decimal)((((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionStartLimit).ModelValue) / 2.54);
                    this.Linear_Start_textbox.Enabled = true;
                    this.Linear_Start.Checked = true;
                }
                if (((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasLinearPositionEndLimit)// if the joint has an end limit then read it in and activate the corresponding fields
                {
                    this.Linear_End_textbox.Value = (decimal)((((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionEndLimit).ModelValue) / 2.54);
                    this.Linear_End_textbox.Enabled = true;
                    this.Linear_End.Checked = true;
                }
            }
            base.Text = joint.GetType().Name.Replace("_Base", "").Replace("Joint", " Joint");

            base.Location = new System.Drawing.Point(Cursor.Position.X - 10, Cursor.Position.Y - base.Height - 10);
        }

        private void CheckInputs()
        {
            canClose = true;
            currentPosition = 0;
            startLimit = 0;
            endLimit = 0;
            writeCurrentPosition = false;
            writeStartLimit = false;
            writeEndLimit = false;
            if (!(((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.JointType == AssemblyJointTypeEnum.kCylindricalJointType ||
                   ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.JointType == AssemblyJointTypeEnum.kSlideJointType))
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
            CheckInputs();
            if (!(((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.JointType == AssemblyJointTypeEnum.kCylindricalJointType ||
                    ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.JointType == AssemblyJointTypeEnum.kSlideJointType))
            {
                ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasAngularPositionLimits = this.Angular_End.Checked && this.Angular_Start.Checked;
                if (writeCurrentPosition && writeStartLimit && writeEndLimit && ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasAngularPositionLimits)
                {
                    if (!(endLimit > currentPosition && startLimit > currentPosition) &&
                        !(endLimit < currentPosition && startLimit < currentPosition))
                    {
                        if ((Math.Abs(startLimit - endLimit) <= 2 * Math.PI))
                        {
                            ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPosition).Value = currentPosition;
                            ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionStartLimit).Value = startLimit;
                            ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionEndLimit).Value = endLimit;
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = currentPosition + " rad";
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.GoToStart();
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
                    ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasAngularPositionLimits = false;
                }
            }
            else
            {
                
                ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasLinearPositionStartLimit = this.Linear_Start.Checked;
                ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasLinearPositionEndLimit = this.Linear_End.Checked;
                if (writeCurrentPosition && writeStartLimit && writeEndLimit && ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasLinearPositionEndLimit &&
                   ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasLinearPositionEndLimit)
                {
                    if (!(endLimit > currentPosition && startLimit > currentPosition) &&
                        !(endLimit < currentPosition && startLimit < currentPosition))
                    {
                        ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPosition).Value = currentPosition;
                        ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionStartLimit).Value = startLimit;
                        ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionEndLimit).Value = endLimit;
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = currentPosition + " cm";
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.GoToStart();
                    } else
                    {
                        canClose = false;
                        MessageBox.Show("Please make sure the current position is between the start and end limits");
                    }

                } else if(writeCurrentPosition && writeStartLimit && ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasLinearPositionStartLimit)
                {
                    ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPosition).Value = currentPosition;
                    ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionStartLimit).Value = startLimit;
                    ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = currentPosition + " cm";
                    ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.GoToStart();
                } else if (writeCurrentPosition && writeEndLimit && ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasLinearPositionEndLimit)
                {
                    ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPosition).Value = currentPosition;
                    ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionEndLimit).Value = endLimit;
                    ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = currentPosition + " cm";
                    ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.GoToStart();
                } else if (writeCurrentPosition)
                {
                    ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPosition).Value = currentPosition;
                    ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = currentPosition + " cm";
                    ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.GoToStart();
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
                        this.Angular_End_textbox.Value = (decimal)(((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionEndLimit).ModelValue);
                    }
                    if (this.Angular_Start_textbox.Text.Equals(""))
                    {
                        this.Angular_Start_textbox.Value = (decimal)(((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionStartLimit).ModelValue);
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
            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = pos + " cm";
            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.EndValue = pos+1 + " cm";
            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.GoToStart();
        }
        private void Angular_Current_textbox_ValueChanged(object sender, EventArgs e)
        {
            var pos = Convert.ToDouble(Angular_Current_textbox.Value) * (Math.PI / 180);
            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = pos + " rad";
            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.EndValue = pos+1 + " rad";
            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.GoToStart();
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
                        this.Angular_End_textbox.Value = (decimal)(((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionEndLimit).ModelValue);
                    }
                    if (this.Angular_Start_textbox.Text.Equals(""))
                    {
                        this.Angular_Start_textbox.Value = (decimal)(((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionStartLimit).ModelValue);
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
                    this.Linear_Start_textbox.Value = (decimal)((((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionStartLimit).ModelValue) / 2.54);
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
                    this.Linear_End_textbox.Value = (decimal)((((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionEndLimit).ModelValue) / 2.54);
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
            CheckInputs();
            double oldStartLimit = 0;
            double oldEndLimit = 0;
            double oldPos = 0;
            bool oldHasLimits = true;
            bool oldHasStartLimits = true;
            bool oldHasEndLimits = true;
            try {
                if (!(((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.JointType == AssemblyJointTypeEnum.kCylindricalJointType ||
                     ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.JointType == AssemblyJointTypeEnum.kSlideJointType))
                {
                    if (writeStartLimit && writeEndLimit && this.Angular_Start.Checked && this.Angular_End.Checked)
                    {
                        if ((Math.Abs(startLimit - endLimit) <= 2 * Math.PI))
                        {
                            oldHasLimits = ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasAngularPositionLimits;
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasAngularPositionLimits = true;
                            if (oldHasLimits)
                            {
                                oldStartLimit = ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionStartLimit).ModelValue;
                                oldEndLimit = ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionEndLimit).ModelValue;
                            }
                            oldPos = ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPosition).ModelValue;
                            ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPosition).Value = ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionEndLimit).ModelValue;
                            ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionStartLimit).Value = startLimit;
                            ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionEndLimit).Value = endLimit;
                            if (startLimit <= endLimit)
                            {
                                ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = startLimit + " rad";
                                ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.EndValue = endLimit + " rad";
                            } else
                            {
                                ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = endLimit + " rad";
                                ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.EndValue = startLimit + " rad";
                            }
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.SetIncrement(IncrementTypeEnum.kNumberOfStepsIncrement, Convert.ToString(Math.Abs((startLimit - endLimit) * 12)));
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.GoToStart();
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.PlayForward();
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.PlayReverse();
                            ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPosition).Value = oldPos;
                            if (oldHasLimits)
                            {
                                ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionStartLimit).Value = oldStartLimit;
                                ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionEndLimit).Value = oldEndLimit;
                            }
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = oldPos + " rad";
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.GoToStart();
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasAngularPositionLimits = oldHasLimits;
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
                        oldHasStartLimits = ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasLinearPositionStartLimit;
                        oldHasEndLimits = ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasLinearPositionEndLimit;
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasLinearPositionStartLimit = true;
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasLinearPositionEndLimit = true;
                        if (oldHasStartLimits)
                        {
                            oldStartLimit = ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionStartLimit).ModelValue;
                        }
                        if (oldHasEndLimits)
                        {
                            oldEndLimit = ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionEndLimit).ModelValue;
                        }
                        oldPos = ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPosition).ModelValue;
                        ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPosition).Value = ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionEndLimit).ModelValue;
                        ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionStartLimit).Value = startLimit;
                        ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionEndLimit).Value = endLimit;
                        if (startLimit <= endLimit)
                        {
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = startLimit + " cm";
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.EndValue = endLimit + " cm";
                        }
                        else
                        {
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = endLimit + " cm";
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.EndValue = startLimit + " cm";
                        }
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.SetIncrement(IncrementTypeEnum.kNumberOfStepsIncrement, Convert.ToString(Math.Abs((startLimit - endLimit)*.25)));
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.GoToStart();
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.PlayForward();
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.PlayReverse();
                        ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPosition).Value = oldPos;
                        ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionStartLimit).Value = oldStartLimit;
                        ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionEndLimit).Value = oldEndLimit;
                        if(oldHasStartLimits)
                        {
                            ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionStartLimit).Value = oldStartLimit;
                        }
                        if (oldHasEndLimits)
                        {
                            ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionEndLimit).Value = oldEndLimit;
                        }
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = oldPos + " cm";
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.GoToStart();
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasLinearPositionStartLimit = oldHasStartLimits;
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasLinearPositionEndLimit = oldHasEndLimits;
                    }
                    else if (!(this.Angular_Start.Checked && this.Angular_End.Checked))
                    {
                        MessageBox.Show("Please make sure you have entered both start and end limits");
                    }
                }
            }
            catch (Exception)//sometimes Inventor gets mad at weird limits, throwing an excpetion, telling it again somehow makes Inventor chill, hence the try/ catch
            {
                if (!(((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.JointType == AssemblyJointTypeEnum.kCylindricalJointType ||
                 ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.JointType == AssemblyJointTypeEnum.kSlideJointType))
                {
                    if (writeStartLimit && writeEndLimit && this.Angular_Start.Checked && this.Angular_End.Checked)
                    {
                        if ((Math.Abs(startLimit - endLimit) <= 2 * Math.PI))
                        {
                            ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPosition).Value = ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionEndLimit).ModelValue;
                            ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionStartLimit).Value = startLimit;
                            ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionEndLimit).Value = endLimit;
                            if (startLimit <= endLimit)
                            {
                                ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = startLimit + " rad";
                                ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.EndValue = endLimit + " rad";
                            }
                            else
                            {
                                ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = endLimit + " rad";
                                ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.EndValue = startLimit + " rad";
                            }
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.SetIncrement(IncrementTypeEnum.kNumberOfStepsIncrement, Convert.ToString(Math.Abs((startLimit - endLimit) * 12)));
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.GoToStart();
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.PlayForward();
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.PlayReverse();
                            ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPosition).Value = oldPos;
                            if (oldHasLimits)
                            {
                                ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionStartLimit).Value = oldStartLimit;
                                ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.AngularPositionEndLimit).Value = oldEndLimit;
                            }
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = oldPos + " rad";
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.GoToStart();
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasAngularPositionLimits = oldHasLimits;
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
                        ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPosition).Value = ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionEndLimit).ModelValue;
                        ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionStartLimit).Value = startLimit;
                        ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionEndLimit).Value = endLimit;
                        if (startLimit <= endLimit)
                        {
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = startLimit + " cm";
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.EndValue = endLimit + " cm";
                        }
                        else
                        {
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = endLimit + " cm";
                            ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.EndValue = startLimit + " cm";
                        }
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.SetIncrement(IncrementTypeEnum.kNumberOfStepsIncrement, Convert.ToString(Math.Abs((startLimit - endLimit) * .25)));
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.GoToStart();
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.PlayForward();
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.PlayReverse();
                        ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPosition).Value = oldPos;
                        ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionStartLimit).Value = oldStartLimit;
                        ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionEndLimit).Value = oldEndLimit;
                        if (oldHasStartLimits)
                        {
                            ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionStartLimit).Value = oldStartLimit;
                        }
                        if (oldHasEndLimits)
                        {
                            ((ModelParameter)((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.LinearPositionEndLimit).Value = oldEndLimit;
                        }
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.StartValue = oldPos + " cm";
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJointOccurrence.DriveSettings.GoToStart();
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasLinearPositionStartLimit = oldHasStartLimits;
                        ((INventorSkeletalJoint)joint).GetWrapped().AsmJoint.HasLinearPositionEndLimit = oldHasEndLimits;
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
