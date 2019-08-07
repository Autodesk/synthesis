using System;
using System.Windows.Forms;
using InventorRobotExporter.Utilities;

namespace InventorRobotExporter.GUI.Editors.JointSubEditors
{
    public partial class JointSensorEditorForm : Form
    {
        private SkeletalJoint_Base joint;
        private int sourceIndex = 0;
        private RobotSensorType[] sensorTypeOptions;

        public JointSensorEditorForm(SkeletalJoint_Base passJoint, int sourceIndex = -1)
        {
            InitializeComponent();
            AnalyticsUtils.LogPage("Advanced Sensor Editor");
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
                PortANumericUpDown.Value = (decimal)sensor.portA;
                PortBNumericUpDown.Value = (decimal)sensor.portB;
                ConversionNumericUpDown.Value = (decimal)sensor.conversionFactor;
            }
            switch (joint.GetJointType())
            {
                case SkeletalJointType.ROTATIONAL:
                    this.ConversionLbl.Text = "Counts Per Rev";
                    break;
                case SkeletalJointType.LINEAR:
                    this.ConversionLbl.Text = "Counts Per Inch";
                    break;
                case SkeletalJointType.CYLINDRICAL:
                    this.ConversionLbl.Text = "Counts Per Rev";
                    break;

            }
            if (typeBox.SelectedIndex == 0)
            {
                this.PortALbl.Enabled = true;
                this.PortANumericUpDown.Enabled = true;
                this.PortBLbl.Visible = true;
                this.PortBNumericUpDown.Visible = true;
                this.ConversionLbl.Visible = true;
                this.ConversionNumericUpDown.Visible = true;
            }
            else
            {
                this.PortALbl.Enabled = false;
                this.PortANumericUpDown.Enabled = false;
                this.PortBLbl.Visible = false;
                this.PortBNumericUpDown.Visible = false;
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
            addedSensor.portA = (int)this.PortANumericUpDown.Value;
            addedSensor.portB = (int)this.PortBNumericUpDown.Value;
            addedSensor.conversionFactor = (double)this.ConversionNumericUpDown.Value;
            
            if (sourceIndex >= 0 && sourceIndex < joint.attachedSensors.Count)
            {
                joint.attachedSensors[sourceIndex] = addedSensor;
            }
            else
            {
                joint.attachedSensors.Add(addedSensor);
            }

            Close();
        }
        
        public void ShowEncoderFields()
        {
            this.PortBLbl.Visible = true;
            this.PortBNumericUpDown.Visible = true;
            this.ConversionLbl.Visible = true;
            this.ConversionNumericUpDown.Visible = true;
        }
        public void HideSecondFields()
        {
            this.PortBLbl.Visible = false;
            this.PortBNumericUpDown.Visible = false;
            this.ConversionLbl.Visible = false;
            this.ConversionNumericUpDown.Visible = false;
        }

        private void typeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch ((RobotSensorType)typeBox.SelectedIndex)
            {
                case RobotSensorType.ENCODER:
                    ShowEncoderFields();
                    this.PortANumericUpDown.Enabled = true;
                    this.PortALbl.Enabled = true;
                    break;
                case RobotSensorType.LIMIT:
                    HideSecondFields();
                    this.PortANumericUpDown.Enabled = true;
                    this.ConversionLbl.Enabled = true;
                    break;
                case RobotSensorType.POTENTIOMETER:
                    HideSecondFields();
                    this.PortANumericUpDown.Enabled = true;
                    this.ConversionLbl.Enabled = true;
                    break;
                default:
                    HideSecondFields();
                    this.PortANumericUpDown.Enabled = false;
                    this.ConversionLbl.Enabled = false;
                    break;
            }
        }
    }
}
