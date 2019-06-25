using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Inventor;


namespace BxDRobotExporter.Wizard
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
            InitializeComponent();
            
            BackColor = System.Drawing.Color.White;
            Dock = DockStyle.Top;
            MinimumSize = new Size(0, 0); // Minimum size only needed in editor
            ParentJointForm = jointForm;

            this.node = node;

            RefillValues(node);

//            StandardAddInServer.Instance.MainApplication.ActiveView.SaveAsBitmap("C:\\Users\\t_wanglia\\Downloads\\shot.bmp", 1024, 768);

            AddHighlightAction(this);
        }

        public JointForm ParentJointForm { get; set; }

        /// <summary>
        /// refills values from existing joint
        /// </summary>
        /// <param name="joint"></param>
        private void RefillValues(RigidNode_Base node)
        {
            jointCardEditor.LoadSettings(node);
            
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
            ParentJointForm.CollapseAllCards(this);
            jointCardEditor.Visible = !jointCardEditor.Visible;
        }

        public void SetCollapsed(bool collapse)
        {
            jointCardEditor.Visible = !collapse;
        }
    }
}