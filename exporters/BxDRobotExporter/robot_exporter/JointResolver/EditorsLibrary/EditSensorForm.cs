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
                typeBox.Items.Add(Enum.GetName(typeof(RobotSensorType), sensorType).Replace('_', ' ').ToLowerInvariant());
            }
            Console.WriteLine(sourceIndex >= 0 && sourceIndex < joint.attachedSensors.Count);
            base.Text = (sourceIndex >= 0 && sourceIndex < joint.attachedSensors.Count) ? ("Editing Sensor # " + sourceIndex) : "New Sensor";
            if (sourceIndex >= 0 && sourceIndex < joint.attachedSensors.Count)
            {
                RobotSensor sensor = joint.attachedSensors[sourceIndex];
                typeBox.SelectedIndex = Array.IndexOf(sensorTypeOptions, sensor.type);
                portTextBox.Text = Convert.ToString(sensor.port);
                moduleTextBox.Text = Convert.ToString(sensor.module);
                secondaryBox.Checked = sensor.useSecondarySource;
                {
                    StringBuilder coeffTxt = new StringBuilder();
                    for (int i = sensor.equation.coeff.Length - 1; i >= 0; i--)
                    {
                        coeffTxt.Append(sensor.equation.coeff[i]);
                        if (i > 0)
                        {
                            coeffTxt.Append(",");
                        }
                    }
                    coefficentTextBox.Text = coeffTxt.ToString();
                    coefficentTextBox_TextChanged(null, null);
                }
            }

            ///Only applies to cylindrical joints.  True use secondary means to use the linear component rather than rotation.
            if (joint.GetJointType() == SkeletalJointType.CYLINDRICAL)
            {
                secondaryBox.Visible = true;
            }

            FormClosing += delegate (object sender, FormClosingEventArgs e) { LegacyInterchange.LegacyEvents.OnRobotModified(); };

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
            try
            {
                addedSensor.module = Convert.ToInt16(moduleTextBox.Text);
                addedSensor.port = Convert.ToInt16(portTextBox.Text);
            }
            catch
            {
                MessageBox.Show("Enter an integer for the module and port numbers.");
                return;
            }


            ///Gets all of the entered polynomial coefficients, seperating by commas.
            string[] coefficients = coefficentTextBox.Text.Split(',');

            float[] polyCoeff = new float[coefficients.Length];

            for (int i = 0; i < coefficients.Length; i++)
            {
                ///Stores the coefficents from left to right.  The coefficient of the lowest power is at index 0.
                try
                {
                    polyCoeff[coefficients.Length - 1 - i] = Convert.ToSingle(coefficients[i]);
                }
                catch
                {
                    MessageBox.Show("For coefficients, enter only numbers followed by commas.");
                    return;
                }
            }
            addedSensor.equation = new Polynomial(polyCoeff);
            addedSensor.useSecondarySource = secondaryBox.Checked;

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

        private void coefficentTextBox_TextChanged(object sender, EventArgs e)
        {
            string[] coefficients = coefficentTextBox.Text.Split(',');
            float[] polyCoeff = new float[coefficients.Length];
            for (int i = 0; i < coefficients.Length; i++)
            {
                ///Stores the coefficents from left to right.  The coefficient of the lowest power is at index 0.
                try
                {
                    polyCoeff[coefficients.Length - 1 - i] = Convert.ToSingle(coefficients[i]);
                }
                catch
                {
                    lblEquationParsed.Text = "Parse Error";
                    return;
                }
            }
            lblEquationParsed.Text = new Polynomial(polyCoeff).ToString();
        }

        private void secondaryBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
