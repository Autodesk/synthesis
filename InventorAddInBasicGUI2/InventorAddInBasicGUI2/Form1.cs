using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventorAddInBasicGUI2
{
    public partial class Form1 : Form
    {

        //Write xml variables to be modified by the instacne of the button click
        //Change me later

        public void clos()
        {

        }
        public Form1()
        {
            InitializeComponent();
        }
        public void MotorChosen()
        {
            grpChooseDriver.Hide();
            cmbFrictionLevel.Hide();
            tabsMeta.Visible = true;
            tabsMeta.TabPages.Clear();
            tabsMeta.TabPages.Add(metaWheel);
            tabsMeta.TabPages.Add(metaGearing);
            chkBoxDriveWheel.Show();
            btnSave.Show();
            rbCAN.Show();
            rbPWM.Show();
            rbPWM.Checked = true;
            txtPortA.Maximum = new decimal(new int[] {21, 0, 0, 0});
            txtPortB.Hide();
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
            btnSave.Location = new System.Drawing.Point(9, 190);
            ClientSize = new System.Drawing.Size(340, 225);
            lblPort.Text = "PWM Port";
        }
        public void ServoChosen()
        {
            grpChooseDriver.Hide();
            tabsMeta.Visible = false;
            tabsMeta.TabPages.Clear();
            rbPWM.Show();
            chkBoxDriveWheel.Hide();
            ClientSize = new System.Drawing.Size(340, 130);
            btnSave.Show();
            btnSave.Location = new System.Drawing.Point(9, 95);
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            rbCAN.Hide();
            txtPortA.Maximum = new decimal(new int[] { 21, 0, 0, 0});
            rbPWM.Hide();
            txtPortB.Hide();
            lblPort.Text = "PWM Port";
        }
        public void BumperPneumaticChosen()
        {
            grpChooseDriver.Hide();
            tabsMeta.Visible = true;
            tabsMeta.TabPages.Clear();
            tabsMeta.TabPages.Add(metaPneumatic);
            rbPWM.Hide();
            chkBoxDriveWheel.Hide();
            ClientSize = new System.Drawing.Size(340, 225);
            btnSave.Show();
            btnSave.Location = new System.Drawing.Point(9,190);
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
            rbCAN.Hide();
            rbPWM.Hide();
            txtPortB.Show();
            lblPort.Text = "Solenoid Port";
        }
        public void RelayPneumaticChosen()
        {
            grpChooseDriver.Hide();
            tabsMeta.Visible = true;
            tabsMeta.TabPages.Clear();
            tabsMeta.TabPages.Add(metaPneumatic);
            rbPWM.Hide();
            chkBoxDriveWheel.Hide();
            ClientSize = new System.Drawing.Size(340, 225);
            btnSave.Show();
            btnSave.Location = new System.Drawing.Point(9, 190);
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
            rbCAN.Hide();
            rbPWM.Hide();
            txtPortB.Hide();
            lblPort.Text = "Relay Port";
        }
        public void WormScrewChosen()
        {
            grpChooseDriver.Hide();
            tabsMeta.Visible = false;
            tabsMeta.TabPages.Clear();
            rbPWM.Show();
            chkBoxDriveWheel.Hide();
            chkBoxDriveWheel.Hide();
            ClientSize = new System.Drawing.Size(340, 130);
            btnSave.Show();
            txtPortA.Maximum = new decimal(new int[] { 21, 0, 0, 0});
            btnSave.Location = new System.Drawing.Point(9, 95);
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            rbCAN.Hide();
            rbPWM.Hide();
            txtPortB.Hide();
            lblPort.Text = "PWM Port";
        }
        public void DualMotorChosen()
        {
            grpChooseDriver.Hide();
            cmbFrictionLevel.Hide();
            tabsMeta.Visible = true;
            tabsMeta.TabPages.Clear();
            tabsMeta.TabPages.Add(metaWheel);
            tabsMeta.TabPages.Add(metaGearing);
            chkBoxDriveWheel.Show();
            btnSave.Show();
            rbCAN.Show();
            rbPWM.Show();
            rbPWM.Checked = true;
            txtPortA.Maximum = new decimal(new int[] { 21, 0, 0, 0});
            txtPortB.Maximum = new decimal(new int[] { 21, 0, 0, 0});
            txtPortB.Show();
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
            btnSave.Location = new System.Drawing.Point(9, 190);
            ClientSize = new System.Drawing.Size(340, 225);
            lblPort.Text = "PWM Port";
        }
        public void ElevatorChosen()
        {
            tabsMeta.Visible = true;
            lblBrakePort.Enabled = false;
            rbCAN.Hide();
            chkBoxDriveWheel.Hide();
            rbPWM.Hide();
            txtPortB.Hide();
            brakePortA.Enabled = false;
            brakePortB.Enabled = false;
            tabsMeta.TabPages.Clear();
            chkBoxHasBrake.Show();
            lblPort.Text = "PWM Port";
            tabsMeta.TabPages.Add(metaElevatorBrake);
            tabsMeta.TabPages.Add(metaElevatorStages);
            tabsMeta.TabPages.Add(metaGearing);
            if (cmbStages.SelectedIndex == -1)
                cmbStages.SelectedIndex = 0;
            ClientSize = new System.Drawing.Size(340, 225);
            btnSave.Show();
            btnSave.Location = new System.Drawing.Point(9, 190);
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
        }
        public void btnPWM_Click(object sender, EventArgs e)
        {
            lblPort.Text = "PWM Port";
            txtPortA.Maximum = new decimal(new int[] { 21, 0, 0, 0});
            txtPortB.Maximum = new decimal(new int[] { 21, 0, 0, 0});
            txtPortB.Show();

        }
        public void btnPHasBrake_Click(object sender, EventArgs e)
        {
            if (chkBoxHasBrake.Checked == true)
            {
                brakePortB.Enabled = true;
                brakePortA.Enabled = true;
            } else
            {
                brakePortB.Enabled = false;
                brakePortA.Enabled = false;
            }
        }
        public void btnCAN_Click(object sender, EventArgs e)
        {
            lblPort.Text = "CAN Port";
            txtPortA.Maximum = new decimal(new int[] { 101, 0, 0,0});
            txtPortB.Maximum = new decimal(new int[] { 101, 0, 0, 0});
            txtPortB.Show();
        }
        public void driveWheelChoice(object sender, EventArgs e)
        {
            if(cmbWheelType.SelectedIndex != 0)
            {
                cmbFrictionLevel.Show();
            } else
            {
                cmbFrictionLevel.Hide();
            }
        }
    }
}