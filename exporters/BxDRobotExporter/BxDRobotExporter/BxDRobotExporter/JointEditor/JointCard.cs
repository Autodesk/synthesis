using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Inventor;

namespace BxDRobotExporter.JointEditor
{
    public sealed partial class JointCard : UserControl
    {
        private readonly JointForm jointForm;
        private readonly RigidNode_Base node;

        private bool isHighlighted;

        public JointCard(RigidNode_Base node, JointForm jointForm)
        {
            this.jointForm = jointForm;
            this.node = node;

            InitializeComponent();

            jointEditor.Initialize(new List<RigidNode_Base> {node}, this);

            AddHighlightAction(this);
        }

        public void LoadValues()
        {
            var joint = node.GetSkeletalJoint();
            jointName.Text = ToStringUtils.NodeNameString(node);
            jointTypeValue.Text = ToStringUtils.JointTypeString(joint);
            driverValue.Text = ToStringUtils.DriverString(joint);
            wheelTypeValue.Text = ToStringUtils.WheelTypeString(joint);
        }

        public void LoadValuesRecursive()
        {
            LoadValues();
            jointEditor.LoadValues();
        }

        private void HighlightSelf()
        {
            if (isHighlighted)
            {
                return;
            }

            jointForm.ResetAllHighlight();
            isHighlighted = true;

            StandardAddInServer.Instance.SelectNode(node);
            Camera cam = StandardAddInServer.Instance.MainApplication.ActiveView.Camera; // TODO: This should be done with a separate camera
            pictureBox1.Image = AxHostConverter.PictureDispToImage(cam.CreateImage(pictureBox1.Width, pictureBox1.Height));
            StandardAddInServer.Instance.SelectNode(node);
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

        private void ToggleCollapsed()
        {
            jointForm.CollapseAllCards(this);
            SetCollapsed(!IsCollapsed());
        }

        public void SetCollapsed(bool collapse)
        {
            jointEditor.Visible = !collapse;
        }


        public bool IsCollapsed()
        {
            return !jointEditor.Visible;
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            ToggleCollapsed();
        }
    }
}