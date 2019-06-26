using System;
using System.Windows.Forms;
using BxDRobotExporter.Wizard;

namespace BxDRobotExporter.JointEditor
{
    /// <summary>
    /// Used in the <see cref="DefineMovingPartsPage"/> to set joint driver information.
    /// </summary>
    /// <remarks>This is the most manual part of the entire guided export and could probably be improved.</remarks>
    public sealed partial class JointCard : UserControl
    {
        /// <summary>
        /// The <see cref="RigidNode_Base"/> which the driver will be applied to.
        /// </summary>
        public RigidNode_Base node;

        public JointCard(RigidNode_Base node, JointForm jointForm)
        {
            this.jointForm = jointForm;
            this.node = node;

            Dock = DockStyle.Top;

            InitializeComponent();
            jointCardEditor.SetParentCard(this);
            jointCardEditor.LoadSettings(node);
            RefillValues();
            AddHighlightAction(this);
        }

        public JointForm jointForm { get; set; }

        public void RefillValues()
        {
            RefillValues(node);
        }


        /// <summary>
        /// refills values from existing joint
        /// </summary>
        /// <param name="joint"></param>
        private void RefillValues(RigidNode_Base node)
        {
            var joint = node.GetSkeletalJoint();
            jointName.Text = ToStringUtils.NodeNameString(node);
            jointTypeValue.Text = ToStringUtils.JointTypeString(joint);
            driverValue.Text = ToStringUtils.DriverString(joint);
            wheelTypeValue.Text = ToStringUtils.WheelTypeString(joint);
        }

        /// <summary>
        /// Highlights the node in inventor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HighlightNode(object sender, EventArgs e)
        {
            StandardAddInServer.Instance.SelectNode(node);
        }

        private void AddHighlightAction(Control baseControl)
        {
            baseControl.MouseHover += HighlightNode;

            foreach (Control control in baseControl.Controls)
                AddHighlightAction(control);
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            jointForm.CollapseAllCards(this);
            jointCardEditor.Visible = !jointCardEditor.Visible;
        }

        public void SetCollapsed(bool collapse)
        {
            jointCardEditor.Visible = !collapse;
        }
    }
}