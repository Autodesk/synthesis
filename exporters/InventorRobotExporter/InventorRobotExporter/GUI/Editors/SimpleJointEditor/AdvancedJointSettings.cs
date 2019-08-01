using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InventorRobotExporter.GUI.Editors.JointSubEditors;
using InventorRobotExporter.Utilities;

namespace InventorRobotExporter.GUI.Editors.SimpleJointEditor
{
    public partial class AdvancedJointSettings : Form
    {
        private SkeletalJoint_Base joint;
        public double GearRatio = 1;
        public int PortId = 3;
        public bool IsCan = true;
        public bool EnableLimits = false;
        public float LowerLimit = 0;
        public float UpperLimit = 0;
        
        private readonly int defaultHeight;

        public AdvancedJointSettings(SkeletalJoint_Base passJoint)
        {
            joint = passJoint;
            AnalyticsUtils.LogPage("SensorListForm");
            InitializeComponent();
            defaultHeight = Height;
            UpdateSensorList();
            RestoreFields();
        }

        public void DoLayout(bool isDriveTrainWheel)
        {
            if (isDriveTrainWheel)
            {
                Height = defaultHeight - limitsBox.Height;
                limitsBox.Visible = false;
                portBox.Visible = false;
            }
            else
            {
                Height = defaultHeight;
                limitsBox.Visible = true;
                portBox.Visible = true;
            }
        }

        private void RestoreFields()
        {
            gearRatioInput.Value = joint.cDriver == null ? (decimal) GearRatio : (decimal) (joint.cDriver.OutputGear / joint.cDriver.InputGear);
            portInput.Value = Math.Max(3, joint.cDriver?.port1 ?? PortId);
            portTypeInput.SelectedItem = joint.cDriver != null && !joint.cDriver.isCan ? "PWM" : "CAN";
            sensorListView.ColumnWidthChanging += sensorListView_ColumnWidthChanging;
            limitStartInput.Value = 0;
            limitEndInput.Value = 0;
        }

        private void UpdateSensorList()
        {
            sensorListView.Items.Clear();

            foreach (RobotSensor sensor in joint.attachedSensors)
            {
                if (sensor.type.Equals(RobotSensorType.ENCODER))
                {// if the sensor is an encoder show both ports
                    ListViewItem item = new ListViewItem(new[] {
                    char.ToUpper(sensor.type.ToString()[0]) + sensor.type.ToString().Substring(1).ToLower(),
                        sensor.portA.ToString(), sensor.portB.ToString()});
                    item.Tag = sensor;
                    sensorListView.Items.Add(item);
                } else
                {
                    ListViewItem item = new ListViewItem(new[] {
                    char.ToUpper(sensor.type.ToString()[0]) + sensor.type.ToString().Substring(1).ToLower(),
                        sensor.portA.ToString()});
                    item.Tag = sensor;
                    sensorListView.Items.Add(item);

                }
            }
        }

        private void sensorListView_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.Cancel = true;
            e.NewWidth = sensorListView.Columns[e.ColumnIndex].Width;
        }

        private void AddSensorButton_Click(object sender, EventArgs e)
        {
            JointSensorEditorForm sensorEditorForm = new JointSensorEditorForm(joint, joint.attachedSensors.IndexOf(
                sensorListView.SelectedItems.Count > 0 &&
                sensorListView.SelectedItems[0].Tag is RobotSensor ?
                    (RobotSensor)sensorListView.SelectedItems[0].Tag : null));
            sensorEditorForm.ShowDialog(this);
            UpdateSensorList();
        }

        private void RemoveSensorButton_Click(object sender, EventArgs e)
        {
            if (sensorListView.SelectedItems.Count > 0 && sensorListView.SelectedItems[0].Tag is RobotSensor)
            {
                joint.attachedSensors.Remove((RobotSensor)sensorListView.SelectedItems[0].Tag);
                UpdateSensorList();
            }
        }

        private void SensorListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            addSensorButton.Text = joint.attachedSensors.IndexOf(
                                       sensorListView.SelectedItems.Count > 0 &&
                                       sensorListView.SelectedItems[0].Tag is RobotSensor ?
                                           (RobotSensor)sensorListView.SelectedItems[0].Tag : null) >= 0 ? "Edit Sensor" : "Add Sensor";
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            GearRatio = (double) gearRatioInput.Value;
            PortId = (int) portInput.Value;
            IsCan = (string) portTypeInput.SelectedItem == "CAN";
            EnableLimits = limitStartCheckbox.Checked;
            LowerLimit = (float) limitStartInput.Value;
            UpperLimit = (float) limitEndInput.Value;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            RestoreFields();
            Close();
        }

        private void LimitStartCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            limitStartInput.Value = 0;
            limitEndInput.Value = 0;
            limitStartInput.Enabled = limitStartCheckbox.Checked;
            limitEndInput.Enabled = limitStartCheckbox.Checked;
        }
    }
}
