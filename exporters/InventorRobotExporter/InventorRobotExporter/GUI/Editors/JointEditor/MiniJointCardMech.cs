using System;
using System.Collections.Generic;
using System.Windows.Forms;
using InventorRobotExporter.GUI.Editors.JointSubEditors;
using InventorRobotExporter.Managers;
using InventorRobotExporter.Utilities;
using InventorRobotExporter.Utilities.ImageFormat;

namespace InventorRobotExporter.GUI.Editors.JointEditor
{
    public sealed partial class MiniJointCardMech : UserControl
    {
        private readonly JointForm jointForm;
        private readonly RigidNode_Base node;

        private bool isHighlighted;
        private bool hasLoadedEditor;

        public MiniJointCardMech(RigidNode_Base node, JointForm jointForm, RobotDataManager robotDataManager)
        {
            this.robotDataManager = robotDataManager;
            this.jointForm = jointForm;
            this.node = node;

            InitializeComponent();
            AnalyticsUtils.LogEvent("Joint Editor", "System", "Init");

            AddHighlightAction(this);
        }

        public void LoadValues()
        {
            var joint = node.GetSkeletalJoint();
            jointName.Text = ToStringUtils.NodeNameString(node);
//            jointTypeValue.Text = ToStringUtils.JointTypeString(joint, robotDataManager);
//            driverValue.Text = ToStringUtils.DriverString(joint);
//            wheelTypeValue.Text = ToStringUtils.WheelTypeString(joint);
        }

        public void LoadValuesRecursive()
        {
            LoadValues();
            hasLoadedEditor = false;
        }

        private void HighlightSelf()
        {
            AnalyticsUtils.LogEvent("Joint Editor", "System", "Highlight");
            if (isHighlighted)
            {
                return;
            }

            jointForm.ResetAllHighlight();
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

        private void editButton_Click(object sender, EventArgs e)
        {
            AnalyticsUtils.LogEvent("Joint Editor", "Control Clicked", "Edit Button");
        }
    }
}