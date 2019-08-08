using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using InventorRobotExporter.GUI.Editors.JointSubEditors;
using InventorRobotExporter.Managers;
using InventorRobotExporter.Utilities;
using InventorRobotExporter.Utilities.ImageFormat;

namespace InventorRobotExporter.GUI.Editors.JointEditor
{
    public sealed partial class JointCardUC : UserControl
    {
        private readonly JointEditorForm jointEditorForm;
        private readonly RigidNode_Base node;

        private bool isHighlighted;
        private AdvancedJointSettingsForm advancedSettingsForm;

        public JointCardUC(RigidNode_Base node, JointEditorForm jointEditorForm, RobotDataManager robotDataManager)
        {
            this.robotDataManager = robotDataManager;
            this.jointEditorForm = jointEditorForm;
            this.node = node;

            InitializeComponent();
            
            AnalyticsUtils.LogEvent("Joint Editor", "System", "Init");
            WinFormsUtils.DisableScrollSelection(this);

            AddHighlightAction(this);
        }

        public void LoadValues()
        {
            advancedSettingsForm = new AdvancedJointSettingsForm(node.GetSkeletalJoint());
            var joint = node.GetSkeletalJoint();
            var typeOptions = JointDriver.GetAllowedDrivers(joint); // TODO: This doesn't protect multi-edit

            jointTypeComboBox.Items.Clear();
            if (typeOptions.Contains(JointDriverType.MOTOR))
            {
                jointTypeComboBox.Items.AddRange(new object[]
                {
                    "(Select an option)",
                    "Drivetrain Wheel",
                    "Mechanism Joint"
                });
            }
            else
            {
                jointTypeComboBox.Items.AddRange(new object[]
                {
                    "(Select an option)",
                    "Mechanism Joint"
                });
            }

            driverTypeComboBox.Items.Clear();
            var textInfo = new CultureInfo("en-US", true).TextInfo;
            foreach (var type in typeOptions)
            {
                var name = Enum.GetName(typeof(JointDriverType), type);
                if (name != null) // TODO: Get rid of this mess
                    driverTypeComboBox.Items.Add(textInfo.ToTitleCase(name.Replace('_', ' ').ToLowerInvariant()));
            }

            jointName.Text = ToStringUtils.NodeNameString(node);

            // Defaults when switched:
            weightInput.Value = 0;
            dtSideComboBox.SelectedIndex = 0;
            wheelTypeComboBox.SelectedIndex = 0;
            driverTypeComboBox.SelectedIndex = 0;

            var jointDriver = joint.cDriver; // sometimes null
            if (jointDriver == null)
            {
                jointTypeComboBox.SelectedItem = "(Select an option)";

                HighlightCard();

            }
            else if (jointDriver.port1 <= 2) // Drivetrain wheel
            {
                jointTypeComboBox.SelectedItem = "Drivetrain Wheel";
                dtSideComboBox.SelectedIndex = jointDriver.port1;
                var wheelDriverMeta = jointDriver.GetInfo<WheelDriverMeta>();
                if (wheelDriverMeta != null)
                    wheelTypeComboBox.SelectedIndex = (int) wheelDriverMeta.type - 1;
            }
            else // Mechanism joint
            {
                jointTypeComboBox.SelectedItem = "Mechanism Joint";
                weightInput.Value = (decimal) Math.Max(joint.weight, 0);
                driverTypeComboBox.SelectedIndex = Array.IndexOf(typeOptions, joint.cDriver.GetDriveType());
            }
        }

        public void SaveValues()
        {
            var joint = node.GetSkeletalJoint();

            if ((string) jointTypeComboBox.SelectedItem == "(Select an option)")
            {
                joint.cDriver = null;
                joint.weight = 0;
            }
            else
            {
                // Driver type
                var cType = (string) jointTypeComboBox.SelectedItem == "Drivetrain Wheel" ? JointDriverType.MOTOR : JointDriver.GetAllowedDrivers(joint)[driverTypeComboBox.SelectedIndex];
                if (joint.cDriver == null || !joint.cDriver.GetDriveType().Equals(cType)) // Defaults
                {
                    joint.cDriver = new JointDriver(cType)
                    {
                        port1 = 3,
                        port2 = 3,
                        InputGear = 1,
                        OutputGear = 1,
                        lowerLimit = 0,
                        upperLimit = 0,
                        isCan = true
                    };
                }

                // Always
                joint.cDriver.hasBrake = true;
                joint.cDriver.motor = MotorType.GENERIC;
                joint.cDriver.InputGear = 1;
                joint.cDriver.OutputGear = advancedSettingsForm.GearRatio;
                joint.cDriver.lowerLimit = 0;
                joint.cDriver.upperLimit = 0;

                if ((string) jointTypeComboBox.SelectedItem == "Drivetrain Wheel")
                {
                    // Port/wheel side
                    joint.cDriver.port1 = dtSideComboBox.SelectedIndex;
                    joint.cDriver.isCan = true;

                    // Wheel type
                    var wheelDriver = new WheelDriverMeta
                    {
                        type = (WheelType) wheelTypeComboBox.SelectedIndex + 1,
                        isDriveWheel = true
                    };
                    wheelDriver.SetFrictionLevel(FrictionLevel.MEDIUM);
                    joint.cDriver.AddInfo(wheelDriver);

                    // Weight
                    joint.weight = 0;
                }
                else if ((string) jointTypeComboBox.SelectedItem == "Mechanism Joint")
                {
                    // Port/wheel side
                    joint.cDriver.port1 = advancedSettingsForm.PortId;
                    if (joint.cDriver.port1 <= 2)
                        joint.cDriver.port1 = 2;
                    joint.cDriver.isCan = advancedSettingsForm.IsCan;

                    // Wheel driver
                    joint.cDriver.RemoveInfo<WheelDriverMeta>();

                    // Weight
                    joint.weight = (double) weightInput.Value;
                }
            }
        }

        private void HighlightSelf()
        {
            AnalyticsUtils.LogEvent("Joint Editor", "System", "Highlight");
            if (isHighlighted)
            {
                return;
            }

            jointEditorForm.ResetAllHighlight();
            isHighlighted = true;

            InventorUtils.FocusAndHighlightNode(node, RobotExporterAddInServer.Instance.Application.ActiveView.Camera, 0.8);
        }

        public void LoadPreviewIcon()
        {
            var iconCamera = RobotExporterAddInServer.Instance.Application.TransientObjects.CreateCamera();
            iconCamera.SceneObject = RobotExporterAddInServer.Instance.OpenAssemblyDocument.ComponentDefinition;

            const double zoom = 0.6; // Zoom, where a zoom of 1 makes the camera the size of the whole robot

            const int widthConst = 3; // The image needs to be wide to hide the XYZ coordinate labels in the bottom left corner

            var occurrences = InventorUtils.GetComponentOccurrencesFromNodes(new List<RigidNode_Base> {node});
            iconCamera.Fit();
            iconCamera.GetExtents(out _, out var height);

            InventorUtils.SetCameraView(InventorUtils.GetOccurrencesCenter(occurrences), 15, height * zoom * widthConst, height * zoom, iconCamera);


            pictureBox1.Image = AxHostConverter.PictureDispToImage(
                iconCamera.CreateImage(pictureBox1.Height * widthConst, pictureBox1.Height,
                    RobotExporterAddInServer.Instance.Application.TransientObjects.CreateColor(210, 222, 239),
                    RobotExporterAddInServer.Instance.Application.TransientObjects.CreateColor(175, 189, 209)));
        }

        public void ResetHighlight()
        {
            isHighlighted = false;
        }

        private void AddHighlightAction(Control baseControl)
        {
            baseControl.MouseHover += (sender, args) => HighlightSelf();

            foreach (Control control in baseControl.Controls)
                AddHighlightAction(control);
        }

        private void JointTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DoLayout();

            HighlightCard();
        }

        private void DoLayout()
        {
            switch ((string) jointTypeComboBox.SelectedItem)
            {
                case "Drivetrain Wheel":
                    InsertDriveTrainControls();
                    advancedButton.Visible = true;
                    return;
                case "Mechanism Joint":
                    InsertTableLayoutControls();
                    advancedButton.Visible = true;
                    return;
                default:
                    RemoveMechControls();
                    RemoveDrivetrainControls();
                    advancedButton.Visible = false;
                    return;
            }
        }

        private void InsertTableLayoutControls()
        {
            RemoveDrivetrainControls();
            tableLayoutPanel2.Controls.Add(weightInput, 1, 1);
            tableLayoutPanel2.Controls.Add(driverTypeComboBox, 1, 2);
            tableLayoutPanel2.Controls.Add(weightLabel, 0, 1);
            tableLayoutPanel2.Controls.Add(jointDriverLabel, 0, 2);
        }

        private void RemoveDrivetrainControls()
        {
            tableLayoutPanel2.Controls.Remove(dtSideComboBox);
            tableLayoutPanel2.Controls.Remove(wheelTypeComboBox);
            tableLayoutPanel2.Controls.Remove(sideLabel);
            tableLayoutPanel2.Controls.Remove(wheelTypeLabel);
        }

        private void InsertDriveTrainControls()
        {
            RemoveMechControls();
            tableLayoutPanel2.Controls.Add(dtSideComboBox, 1, 1);
            tableLayoutPanel2.Controls.Add(wheelTypeComboBox, 1, 2);
            tableLayoutPanel2.Controls.Add(sideLabel, 0, 1);
            tableLayoutPanel2.Controls.Add(wheelTypeLabel, 0, 2);
        }

        private void RemoveMechControls()
        {
            tableLayoutPanel2.Controls.Remove(weightInput);
            tableLayoutPanel2.Controls.Remove(driverTypeComboBox);
            tableLayoutPanel2.Controls.Remove(weightLabel);
            tableLayoutPanel2.Controls.Remove(jointDriverLabel);
        }

        private void AdvancedButton_Click(object sender, EventArgs e)
        {
            if ((string) jointTypeComboBox.SelectedItem == "(Select an option)")
                return;
            advancedSettingsForm.DoLayout((string) jointTypeComboBox.SelectedItem == "Drivetrain Wheel");
            advancedSettingsForm.ShowDialog();
        }

        private void HighlightCard()
        {
            Color highlightColor = Color.FromArgb(227,206,169);

            DriverLayout.BackColor = highlightColor;

            if (jointTypeComboBox.SelectedIndex != 0)
            {
                DriverLayout.ResetBackColor();
            }
            else
            {
                DriverLayout.BackColor = highlightColor;
            }
        }
    }

}