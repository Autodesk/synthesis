using System;
using System.Collections.Generic;
using System.Windows.Forms;
using InventorRobotExporter.Managers;
using InventorRobotExporter.Utilities;
using InventorRobotExporter.Utilities.ImageFormat;

namespace InventorRobotExporter.GUI.Editors.SimpleJointEditor
{
    public partial class SimpleEditor : Form
    {
        private readonly RobotDataManager robotDataManager;
        private List<RigidNode_Base> nodeCache;
        private int defaultHeight;

        public SimpleEditor(RobotDataManager robotDataManager)
        {
            this.robotDataManager = robotDataManager;
            nodeCache = new List<RigidNode_Base>();

            InitializeComponent();
            LoadJointsNavigator();

            defaultHeight = Height; // use for sizing on different displays

            jointTypeInput.SelectedIndex = 0;
            jointDriverInput.SelectedIndex = 0;
            driveSideInput.SelectedIndex = 0;
            wheelTypeInput.SelectedIndex = 0;
        }

        private void ClearType()
        {
            // Hides all elements to return the window to its default state
            weightBox.Visible = false;
            weightAmountInput.Visible = false;
            jointDriverBox.Visible = false;
            jointDriverInput.Visible = false;
            driveSideBox.Visible = false;
            driveSideInput.Visible = false;
            wheelTypeBox.Visible = false;
            wheelTypeInput.Visible = false;
            limitsBox.Visible = false;
            limitStartCheckbox.Visible = false;
            limitEndCheckbox.Visible = false;
            limitStartInput.Visible = false;
            limitEndInput.Visible = false;
            animateMovementButton.Visible = false;
            advancedButton.Visible = false;
        }

        private void ShowMechanismType()
        {
            // Show relevant elements to the Mechanism joint type
            weightBox.Visible = true;
            weightAmountInput.Visible = true;
            jointDriverBox.Visible = true;
            jointDriverInput.Visible = true;
            limitsBox.Visible = true;
            limitStartCheckbox.Visible = true;
            limitEndCheckbox.Visible = true;
            limitStartInput.Visible = true;
            limitEndInput.Visible = true;
            animateMovementButton.Visible = true;
            advancedButton.Visible = true;

            Height = defaultHeight;
        }

        private void ShowDrivetrainType()
        {
            // Show relevant elements to the Drivetrain joint type
            driveSideBox.Visible = true;
            driveSideInput.Visible = true;
            wheelTypeBox.Visible = true;
            wheelTypeInput.Visible = true;
            advancedButton.Visible = true;

            Height = defaultHeight - limitsBox.Height;
        }

        private void LoadJointsNavigator()
        {
            // Loops through all nodes and adds components with joints to the Joint Navigator
            foreach (RigidNode_Base node in robotDataManager.RobotBaseNode.ListAllNodes())
            {
                if (node.GetSkeletalJoint() != null)
                {
                    jointNavigator.Items.Add(ToStringUtils.NodeNameString(node));
                    nodeCache.Add(node);
                }
            }
            jointNavigator.SelectedIndex = 0;
        }

        // Switch joints from joint list
        private void JointNavigator_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
            // TODO: Create logic for updating and saving data
        }

        // Change between Drivetrain Wheel and Mechanism Joint
        private void JointTypeInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearType();
            if (jointTypeInput.SelectedIndex == 0)
            {
                Height = defaultHeight - limitsBox.Height;
            } else if (jointTypeInput.SelectedIndex == 1)
            {
                ShowDrivetrainType();
            } else if (jointTypeInput.SelectedIndex == 2)
            {
                ShowMechanismType();
            }
        }

        private void UpdateDisplay()
        {
            RigidNode_Base node = nodeCache[jointNavigator.SelectedIndex];
            if (jointTypeInput.SelectedIndex == 1)
            {
                // Drivetrain wheel
                driveSideInput.Text = ToStringUtils.DriveTrainSideString(node.GetSkeletalJoint());
                wheelTypeInput.Text = ToStringUtils.WheelTypeString(node.GetSkeletalJoint());
            } else if (jointTypeInput.SelectedIndex == 2)
            {
                // Mechanism joint
                weightAmountInput.Text = "22.05"; // TODO: Get actual weight value
                jointDriverInput.Text = ToStringUtils.DriverString(node.GetSkeletalJoint());
                // TODO: Get limit values
            }

            LoadPreviewIcon(node);
        }

        public void LoadPreviewIcon(RigidNode_Base node)
        {
            // TODO: Look into creating better previews

            var iconCamera = RobotExporterAddInServer.Instance.Application.TransientObjects.CreateCamera();
            iconCamera.SceneObject = RobotExporterAddInServer.Instance.OpenAssemblyDocument.ComponentDefinition;

            const double zoom = 0.3; // Zoom, where a zoom of 1 makes the camera the size of the whole robot

            const int widthConst = 1; // The image needs to be wide to hide the XYZ coordinate labels in the bottom left corner

            var occurrences = InventorUtils.GetComponentOccurrencesFromNodes(new List<RigidNode_Base> { node });
            iconCamera.Fit();
            iconCamera.GetExtents(out _, out var height);

            InventorUtils.SetCameraView(InventorUtils.GetOccurrencesCenter(occurrences), 15, height * zoom * widthConst, height * zoom, iconCamera);


            jointPreviewImage.Image = AxHostConverter.PictureDispToImage(
                iconCamera.CreateImage(jointPreviewImage.Height * widthConst, jointPreviewImage.Height,
                    RobotExporterAddInServer.Instance.Application.TransientObjects.CreateColor(210, 222, 239),
                    RobotExporterAddInServer.Instance.Application.TransientObjects.CreateColor(175, 189, 209)));
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            if (jointNavigator.SelectedIndex - 1 > -1)
            {
                jointNavigator.SelectedIndex -= 1;
            }
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (jointNavigator.SelectedIndex + 1 < jointNavigator.Items.Count - 1)
            {
                jointNavigator.SelectedIndex += 1;
            }
        }

        private void AnimateMovementButton_Click(object sender, EventArgs e)
        {
            // TODO: Animate the selected joint
        }

        private void AdvancedButton_Click(object sender, EventArgs e)
        {
            new AdvancedJointSettings().ShowDialog();
        }
        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            // TODO: Implement saving
            Close();
        }

    }
}
