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
    public partial class EditSensorForm : Form
    {
        SkeletalJoint_Base joint;
        int sourceIndex = 0;
        RobotSensorType[] sensorTypeOptions;

        public EditSensorForm(SkeletalJoint_Base passJoint, int sourceIndex = -1)
        {
            InitializeComponent();
            this.sourceIndex = sourceIndex;
            joint = passJoint;
            typeBox.Items.Clear();
            sensorTypeOptions = RobotSensor.GetAllowedSensors(joint);
            foreach (RobotSensorType sensorType in sensorTypeOptions)
            {
                typeBox.Items.Add(char.ToUpper(Enum.GetName(typeof(RobotSensorType), sensorType).Replace('_', ' ')[0])
                    + Enum.GetName(typeof(RobotSensorType), sensorType).Replace('_', ' ').Substring(1).ToLower());
            }
            Console.WriteLine(sourceIndex >= 0 && sourceIndex < joint.attachedSensors.Count);
            base.Text = (sourceIndex >= 0 && sourceIndex < joint.attachedSensors.Count) ? ("Editing Sensor # " + sourceIndex) : "New Sensor";
            if (sourceIndex >= 0 && sourceIndex < joint.attachedSensors.Count)
            {
                RobotSensor sensor = joint.attachedSensors[sourceIndex];
                typeBox.SelectedIndex = Array.IndexOf(sensorTypeOptions, sensor.type);
                Port1NumericUpDown.Value = (decimal)sensor.port1;
                Port2NumericUpDown.Value = (decimal)sensor.port2;
            }
            if (typeBox.SelectedIndex == 0)
            {
                this.port1Lbl.Enabled = true;
                this.Port1NumericUpDown.Enabled = true;
                this.Port2Lbl.Visible = true;
                this.Port2NumericUpDown.Visible = true;
                this.ConversionLbl.Visible = true;
                this.ConversionNumericUpDown.Visible = true;
            }
            else
            {
                this.port1Lbl.Enabled = false;
                this.Port1NumericUpDown.Enabled = false;
                this.Port2Lbl.Visible = false;
                this.Port2NumericUpDown.Visible = false;
                this.ConversionLbl.Visible = false;
                this.ConversionNumericUpDown.Visible = false;
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            RobotSensor addedSensor;

            try
            {
                addedSensor = new RobotSensor(sensorTypeOptions[typeBox.SelectedIndex]);
            }
            catch
            {
                MessageBox.Show("Please select a sensor type.");
                return;
            }

            //Doesn't save if numbers aren't entered correctly.
            addedSensor.port1 = (int)this.Port1NumericUpDown.Value;
            addedSensor.port2 = (int)this.Port2NumericUpDown.Value;
            addedSensor.conversionFactor = (int)this.ConversionNumericUpDown.Value;
            
            if (sourceIndex >= 0 && sourceIndex < joint.attachedSensors.Count)
            {
                joint.attachedSensors[sourceIndex] = addedSensor;
            }
            else
            {
                joint.attachedSensors.Add(addedSensor);
            }

            LegacyInterchange.LegacyEvents.OnRobotModified();
            Close();
        }
        
        public void ShowEncoderFields()
        {
            this.Port2Lbl.Visible = true;
            this.Port2NumericUpDown.Visible = true;
            this.ConversionLbl.Visible = true;
            this.ConversionNumericUpDown.Visible = true;
            this.ConversionLbl.Text = "Counts per Rev";
        }
        public void HideSecondFields()
        {
            this.Port2Lbl.Visible = false;
            this.Port2NumericUpDown.Visible = false;
            this.ConversionLbl.Visible = false;
            this.ConversionNumericUpDown.Visible = false;
        }

        private void typeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch ((RobotSensorType)typeBox.SelectedIndex)
            {
                case RobotSensorType.ENCODER:
                    ShowEncoderFields();
                    this.Port1NumericUpDown.Enabled = true;
                    this.port1Lbl.Enabled = true;
                    break;
                case RobotSensorType.LIMIT:
                    HideSecondFields();
                    this.Port1NumericUpDown.Enabled = true;
                    this.ConversionLbl.Enabled = true;
                    break;
                case RobotSensorType.POTENTIOMETER:
                    HideSecondFields();
                    this.Port1NumericUpDown.Enabled = true;
                    this.ConversionLbl.Enabled = true;
                    break;
                default:
                    HideSecondFields();
                    this.Port1NumericUpDown.Enabled = false;
                    this.ConversionLbl.Enabled = false;
                    break;
            }
        }
    }
}
