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


        public Form1()
        {
            InitializeComponent();
        }

        
        public void stepForm(int stepAmount)
        {
            //useless
           // progressBar1.Step = (stepAmount);
        }

        //Click that looks for an active document and for specifically a ASSEMBLY and PART file
       public void MotorChosen()
        {
            grpChooseDriver.Hide();
            tabsMeta.Visible = true;
            tabsMeta.TabPages.Clear();
            tabsMeta.TabPages.Add(metaWheel);
            tabsMeta.TabPages.Add(metaGearing);
            chkBoxDriveWheel.Show();
            btnSave.Show();
            rbCAN.Show();
            rbPWM.Show();
            rbPWM.Checked = true;
            txtPortB.Hide();
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
            btnSave.Location = new System.Drawing.Point(9, 190);
            ClientSize = new System.Drawing.Size(340, 225);
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
            rbPWM.Hide();
            txtPortB.Hide();
        }
        public void BumperPneumaticChosen()
        {
            grpChooseDriver.Hide();
            tabsMeta.Visible = true;
            tabsMeta.TabPages.Clear();
            tabsMeta.TabPages.Add(metaPneumatic);
            rbPWM.Hide();
            chkBoxDriveWheel.Hide();
            ClientSize = new System.Drawing.Size(340, 300);
            btnSave.Show();
            btnSave.Location = new System.Drawing.Point(9,190);
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
            rbCAN.Hide();
            rbPWM.Hide();
            txtPortB.Show();

        }
        public void RelayPneumaticChosen()
        {
            grpChooseDriver.Hide();
            tabsMeta.Visible = true;
            tabsMeta.TabPages.Clear();
            tabsMeta.TabPages.Add(metaPneumatic);
            rbPWM.Hide();
            chkBoxDriveWheel.Hide();
            ClientSize = new System.Drawing.Size(340, 300);
            btnSave.Show();
            btnSave.Location = new System.Drawing.Point(9, 190);
            grpDriveOptions.Location = new System.Drawing.Point(10, 10);
            tabsMeta.Location = new System.Drawing.Point(10, 95);
            rbCAN.Hide();
            rbPWM.Hide();
            txtPortB.Show();

        }
        private void cmbJointDriver_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}