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
        ControlGroups mainWindow;
        SkeletalJoint_Base joint;
        RobotSensorType[] sensorTypeOptions;

        public AddSensorForm(ControlGroups passMainWindow, SkeletalJoint_Base passJoint)
        {
            InitializeComponent();
            mainWindow = passMainWindow;
            joint = passJoint;
            typeBox.Items.Clear();
            sensorTypeOptions = (RobotSensorType[]) Enum.GetValues(typeof(RobotSensorType));
            foreach (RobotSensorType sensorType in sensorTypeOptions)
            {
                typeBox.Items.Add(Enum.GetName(typeof(RobotSensorType), sensorType).Replace('_', ' ').ToLowerInvariant());
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            RobotSensor addedSensor = new RobotSensor(sensorTypeOptions[typeBox.SelectedIndex]);

            addedSensor.module = Convert.ToInt16(moduleTextBox.Text);
            addedSensor.port = Convert.ToInt16(portTextBox.Text);
            ///Gets all of tentered polynomial coefficients, seperating by commas.

            string[] coefficients = coefficentTextBox.Text.Split(new char[] { ',' });

            addedSensor.polyCoeff = new float[coefficients.Length];
            int index = 0;

            foreach (string coefficient in coefficients)
            {
                ///Stores the coefficents from left to right.  The coefficient of the lowest power is at index 0.
                addedSensor.polyCoeff[index++] = Convert.ToSingle(coefficient);
            }

            addedSensor.useSecondarySource = secondaryBox.Checked;

            joint.attachedSensors.Add(addedSensor);
            Close();
        }
    }
}
