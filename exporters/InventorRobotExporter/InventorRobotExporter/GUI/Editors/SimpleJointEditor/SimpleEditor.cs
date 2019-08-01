using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Inventor;
using InventorRobotExporter.Managers;
using InventorRobotExporter.Utilities;
using InventorRobotExporter.Utilities.ImageFormat;

namespace InventorRobotExporter.GUI.Editors.SimpleJointEditor
{
    public partial class SimpleEditor : Form
    {
        private RobotDataManager robotDataManager;
        private List<RigidNode_Base> nodeCache = new List<RigidNode_Base>();
        private readonly int defaultHeight;

        public SimpleEditor()
        {
            InitializeComponent();

            defaultHeight = Height; // use for sizing on different displays

            jointTypeInput.SelectedIndex = 0;
            jointDriverInput.SelectedIndex = 0;
            driveSideInput.SelectedIndex = 0;
            wheelTypeInput.SelectedIndex = 0;
        }

        public void UpdateSkeleton(RobotDataManager robotDataManager)
        {
            this.robotDataManager = robotDataManager;
            LoadJointsNavigator();
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
            LoadValues(nodeCache[jointNavigator.SelectedIndex]);
        }

        // Change between Drivetrain Wheel and Mechanism Joint
        private void JointTypeInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            DoLayout();
        }

        private void DoLayout()
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
            if (jointTypeInput.SelectedIndex == 0)
            {
                Height = defaultHeight - limitsBox.Height;
            }
            else if (jointTypeInput.SelectedIndex == 1)
            {
                // Show relevant elements to the Drivetrain joint type
                driveSideBox.Visible = true;
                driveSideInput.Visible = true;
                wheelTypeBox.Visible = true;
                wheelTypeInput.Visible = true;
                advancedButton.Visible = true;

                Height = defaultHeight - limitsBox.Height;
            }
            else if (jointTypeInput.SelectedIndex == 2)
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
        }

        private void LoadValues(RigidNode_Base node)
        {
            nameInput.Text = ToStringUtils.NodeNameString(node);

            var joint = node.GetSkeletalJoint();
            var jointDriver = joint.cDriver;
            
            if (jointDriver == null)
            {
                jointTypeInput.SelectedIndex = 0;
            } else if (jointDriver.port1 <= 2)
            {
                // Drivetrain wheel
                jointTypeInput.SelectedIndex = 1;
                driveSideInput.SelectedIndex = jointDriver.port1;
                wheelTypeInput.SelectedIndex = (int) jointDriver.GetInfo<WheelDriverMeta>().type - 1;
            }
            else
            {
                // Mechanism joint
                jointTypeInput.SelectedIndex = 2;
                weightAmountInput.Value = (decimal) (Math.Max(joint.weight, 0) * 2.20462f); // TODO: Re-use existing weight code

                jointDriverInput.Items.Clear();
                var typeOptions = JointDriver.GetAllowedDrivers(joint); // TODO: This doesn't protect multi-edit
                var textInfo = new CultureInfo("en-US", true).TextInfo;
                foreach (var type in typeOptions)
                {
                    var name = Enum.GetName(typeof(JointDriverType), type);
                    if (name != null) // TODO: Get rid of this mess
                        jointDriverInput.Items.Add(textInfo.ToTitleCase(name.Replace('_', ' ').ToLowerInvariant()));
                }
                jointDriverInput.SelectedIndex = Array.IndexOf(typeOptions, joint.cDriver.GetDriveType());

                // TODO: Get limit values
            }

            LoadPreviewIcon(node);
        }

        private void LoadPreviewIcon(RigidNode_Base node)
        {

            var iconCamera = RobotExporterAddInServer.Instance.Application.TransientObjects.CreateCamera();
            iconCamera.SceneObject = RobotExporterAddInServer.Instance.OpenAssemblyDocument.ComponentDefinition;

            const double zoom = 0.3; // Zoom, where a zoom of 1 makes the camera the size of the whole robot

            const int widthConst = 3; // The image needs to be wide to hide the XYZ coordinate labels in the bottom left corner

            var occurrences = InventorUtils.GetComponentOccurrencesFromNodes(new List<RigidNode_Base> { node });
            iconCamera.Fit();
            iconCamera.GetExtents(out _, out var height);

            InventorUtils.SetCameraView(InventorUtils.GetOccurrencesCenter(occurrences), 15, height * zoom * widthConst, height * zoom, iconCamera);


            jointPreviewImage.Image = AxHostConverter.PictureDispToImage(
                iconCamera.CreateImage(jointPreviewImage.Height * widthConst, jointPreviewImage.Height,
                    RobotExporterAddInServer.Instance.Application.TransientObjects.CreateColor(210, 222, 239),
                    RobotExporterAddInServer.Instance.Application.TransientObjects.CreateColor(175, 189, 209)));
        }

        private void SaveChanges()
        {
            SkeletalJoint_Base joint = nodeCache[jointNavigator.SelectedIndex].GetSkeletalJoint();

            if (jointTypeInput.SelectedIndex == 1)
            {
                // Drivetrain wheel

            } else if (jointTypeInput.SelectedIndex == 2)
            {
                // Mechanism Joint
                joint.weight = Convert.ToDouble(weightAmountInput.Value);
                joint.cDriver.SetLimits(float.Parse(limitEndInput.Text), float.Parse(limitStartInput.Text));
            }

            if (nameInput.Text != jointNavigator.Text)
            {
                RenameComponent(jointNavigator.Text, nameInput.Text);
            }

        }

        private void RenameComponent(String oldName, String newName)
        {
            AssemblyDocument document = (AssemblyDocument) RobotExporterAddInServer.Instance.Application.ActiveDocument;
            AssemblyComponentDefinition component = document.ComponentDefinition;

            foreach (ComponentOccurrence occurence in component.Occurrences)
            {
                if (string.Compare(occurence.Name, oldName) == 0)
                {
                    occurence.Name = newName;
                }
            }
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
            new AdvancedJointSettings(nodeCache[jointNavigator.SelectedIndex].GetSkeletalJoint()).ShowDialog();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            // TODO: Implement saving
            Close();
        }
    }
}
