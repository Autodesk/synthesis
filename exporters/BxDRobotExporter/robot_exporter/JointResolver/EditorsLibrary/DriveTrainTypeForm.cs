using System;
using System.Drawing;
using System.Windows.Forms;

namespace JointResolver.EditorsLibrary
{
    public partial class DriveTrainTypeForm : Form
    {
        private RadioButton selectedrb;
        public RigidNode_Base.DriveTrainType driveTrainType;

        public DriveTrainTypeForm()
        {
            InitializeComponent();
            
            switch (SynthesisGUI.Instance.SkeletonBase.driveTrainType)
            {
                case RigidNode_Base.DriveTrainType.TANK:
                    tankButton.Checked = true;
                    break;
                case RigidNode_Base.DriveTrainType.H_DRIVE:
                    hdriveButton.Checked = true;
                    break;
                case RigidNode_Base.DriveTrainType.CUSTOM:
                    otherButton.Checked = true;
                    break;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            switch (selectedrb.Name)
            {
                case "tankButton":
                    driveTrainType = RigidNode_Base.DriveTrainType.TANK;
                    break;
                case "hdriveButton":
                    driveTrainType = RigidNode_Base.DriveTrainType.H_DRIVE;
                    break;
                case "otherButton":
                    driveTrainType = RigidNode_Base.DriveTrainType.CUSTOM;
                    break;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }


        void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb == null)
            {
                MessageBox.Show("Sender is not a RadioButton");
                return;
            }

            // Ensure that the RadioButton.Checked property
            // changed to true.
            if (rb.Checked)
            {
                // Keep track of the selected RadioButton by saving a reference
                // to it.
                selectedrb = rb;
            }

            var selectedColor = Color.LightSalmon;
            switch (selectedrb.Name)
            {
                case "tankButton":
                    resetBackColor();
                    tankPicture.BackColor = selectedColor;
                    break;
                case "hdriveButton":
                    resetBackColor();
                    hdrivePicture.BackColor = selectedColor;
                    break;
                case "otherButton":
                    resetBackColor();
                    otherPicture.BackColor = selectedColor;
                    break;
            }
        }

        private void resetBackColor()
        {
            var defaultColor = SystemColors.Control;
            tankPicture.BackColor = defaultColor;
            hdrivePicture.BackColor = defaultColor;
            otherPicture.BackColor = defaultColor;
        }

        private void tankPicture_Click(object sender, EventArgs e)
        {
            tankButton.Checked = true;
        }

        private void hdrivePicture_Click(object sender, EventArgs e)
        {
            hdriveButton.Checked = true;
        }

        private void otherPicture_Click(object sender, EventArgs e)
        {
            otherButton.Checked = true;
        }
    }
}
