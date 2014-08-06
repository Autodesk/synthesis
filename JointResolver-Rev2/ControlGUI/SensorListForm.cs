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
    public partial class SensorListForm : Form
    {
        SkeletalJoint_Base joint;

        public SensorListForm(SkeletalJoint_Base passJoint)
        {
            InitializeComponent();

            joint = passJoint;
            this.UpdateSensorList();
        }

        private void UpdateSensorList()
        {
            sensorListView.Items.Clear();

            foreach (RobotSensor sensor in joint.attachedSensors)
            {
                System.Windows.Forms.ListViewItem item = new System.Windows.Forms.ListViewItem(new string[] { 
                    sensor.type.ToString(), sensor.module.ToString(), sensor.port.ToString()});
                item.Tag = sensor;
                sensorListView.Items.Add(item);
            }
        }

        /// <summary>
        /// Removes the selected sensor from the list of sensors in the joint.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteButton_Click(object sender, EventArgs e)
        {
            joint.attachedSensors.Remove((RobotSensor)sensorListView.SelectedItems[0].Tag);
            this.UpdateSensorList();
        }
    }
}
