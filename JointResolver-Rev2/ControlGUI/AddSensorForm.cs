using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JointResolver.ControlGUI
{
    public partial class AddSensorForm : Form
    {
        SkeletalJoint_Base joint;
        RobotSensorType[] sensorTypeOptions;

        public AddSensorForm(SkeletalJoint_Base passJoint)
        {
            InitializeComponent();
            joint = passJoint;
            typeBox.Items.Clear();
            sensorTypeOptions = RobotSensor.GetAllowedSensors(joint);
            foreach (RobotSensorType sensorType in sensorTypeOptions)
            {
                typeBox.Items.Add(Enum.GetName(typeof(RobotSensorType), sensorType).Replace('_', ' ').ToLowerInvariant());
            }

            ///Only applies to cylindrical joints.  True use secondary means to use the linear component rather than rotation.
            if (joint is CylindricalJoint)
            {
                secondaryBox.Visible = true;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

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
                    polyCoeff[i] = Convert.ToSingle(coefficients[i]);
                }
                catch
                {
                    MessageBox.Show("For coefficients, enter only numbers followed by commas.");
                    return;
                }
            }
            addedSensor.equation = new Polynomial(polyCoeff);

            addedSensor.useSecondarySource = secondaryBox.Checked;

            joint.attachedSensors.Add(addedSensor);
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
                    polyCoeff[i] = Convert.ToSingle(coefficients[i]);
                }
                catch
                {
                    lblEquationParsed.Text = "Parse Error";
                    return;
                }
            }
            lblEquationParsed.Text = new Polynomial(polyCoeff).ToString();
        }

        private void AddSensorForm_Load(object sender, EventArgs e)
        {

        }
    }
}
