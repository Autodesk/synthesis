using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace EditorsLibrary
{
    public partial class JointEditorPane : UserControl
    {

        /// <summary>
        /// An event triggered in the joint editor pane
        /// </summary>
        /// <param name="node">The Tag of the ListView item to be operated on</param>
        public delegate void JointEditorEvent(RigidNode_Base node);

        /// <summary>
        /// A JointEditorEvent triggered upon the selection of a joint
        /// </summary>
        public event JointEditorEvent SelectedJoint;

        /// <summary>
        /// A JointEditorEvent triggered upon joint modification
        /// </summary>
        public event JointEditorEvent ModifiedJoint;

        /// <summary>
        /// Whether or not joint data is currently being edited
        /// </summary>
        private bool currentlyEditing = false;

        /// <summary>
        /// Dialog for choosing a joint driver
        /// </summary>
        private DriveChooser driveChooserDialog = new DriveChooser();

        /// <summary>
        /// The list of nodes to be attached as metadata
        /// </summary>
        private List<RigidNode_Base> nodeList = null;

        /// <summary>
        /// Create a new JointEditorPane and register actions for the right click menu
        /// </summary>
        public JointEditorPane()
        {
            InitializeComponent();
            
            RegisterContextAction("Edit Driver", editDriver_Internal);
            RegisterContextAction("Edit Sensors", listSensors_Internal);
            RegisterContextAction("Edit Limits", (RigidNode_Base node) =>
                {
                    if (node != null && node.GetSkeletalJoint() != null)
                    {
                        EditLimits limitEditor = new EditLimits(node.GetSkeletalJoint());
                        limitEditor.StartPosition = FormStartPosition.Manual;
                        limitEditor.Location = new System.Drawing.Point(Cursor.Position.X - 10, Cursor.Position.Y - 10);
                        limitEditor.ShowDialog(ParentForm);
                    }
                });
        }

        /// <summary>
        /// Register an action for the right click menu
        /// </summary>
        /// <param name="caption">The caption of the menu item</param>
        /// <param name="callback">The action to perform when the item is clicked</param>
        public void RegisterContextAction(string caption, JointEditorEvent callback)
        {
            ToolStripButton item = new ToolStripButton();
            item.Text = caption;

            item.Click += (object sender, EventArgs e) =>
            {
                if (lstJoints.SelectedItems.Count == 1 && lstJoints.SelectedItems[0].Tag is RigidNode_Base)
                {
                    callback((RigidNode_Base) lstJoints.SelectedItems[0].Tag);
                }
                else
                {
                    callback(null);
                }
            };
            
            jointContextMenu.Items.Add(item);
        }

        /// <summary>
        /// Set up the control's layout
        /// </summary>
        /// <param name="sender">The object sending the event</param>
        /// <param name="e">The event arguments</param>
        private void DoLayout(object sender, EventArgs e)
        {
            //Scales the columns to the width
            item_chType.Width = this.lstJoints.Width / 8;
            item_chParent.Width = this.lstJoints.Width / 8;
            item_chChild.Width = this.lstJoints.Width / 8;
            item_chDrive.Width = this.lstJoints.Width / 3;
            item_chWheel.Width = this.lstJoints.Width / 8;
            item_chSensors.Width = this.lstJoints.Width / 8;

            this.lstJoints.Width = this.Width;
            this.lstJoints.Height = this.Height - 6;
        }

        /// <summary>
        /// The <see cref="JointEditorEvent"/> to open up a <see cref="SensorListForm"/>
        /// </summary>
        /// <param name="node">The node connected to the joint to edit the sensors on</param>
        private void listSensors_Internal(RigidNode_Base node)
        {
            if (node == null) return;

            currentlyEditing = true;
            SensorListForm listForm = new SensorListForm(node.GetSkeletalJoint());
            listForm.StartPosition = FormStartPosition.Manual;
            listForm.Location = new System.Drawing.Point(Cursor.Position.X - 10, Cursor.Position.Y - 10);
            listForm.ShowDialog(ParentForm);
            if (ModifiedJoint != null)
            {
                ModifiedJoint(node);
            }
            this.UpdateJointList();
            currentlyEditing = false;
        }

        /// <summary>
        /// The <see cref="JointEditorEvent"/> to open up a <see cref="DriveChooser"/> dialog
        /// </summary>
        /// <param name="node">The node connected to the joint with a driver to edit</param>
        private void editDriver_Internal(RigidNode_Base node)
        {
            if (node == null) return;

            currentlyEditing = true;
            SkeletalJoint_Base joint = node.GetSkeletalJoint();
            driveChooserDialog.StartPosition = FormStartPosition.Manual;
            driveChooserDialog.Location = new System.Drawing.Point(Cursor.Position.X - 10, Cursor.Position.Y - 10);
            driveChooserDialog.ShowDialog(joint, node, ParentForm);
            if (ModifiedJoint != null && driveChooserDialog.Saved)
            {
                ModifiedJoint(node);
            }
            UpdateJointList();
            currentlyEditing = false;
        }

        /// <summary>
        /// Highlight a node in the viewer when it's selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstJoints_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstJoints.SelectedItems.Count == 1 && lstJoints.SelectedItems[0].Tag is RigidNode_Base)
            {
                if (SelectedJoint != null)
                    SelectedJoint((RigidNode_Base) lstJoints.SelectedItems[0].Tag);
            }
            else
            {
                if (SelectedJoint != null)
                    SelectedJoint(null);
            }
        }

        /// <summary>
        /// Load a list of nodes into the editor pane
        /// </summary>
        /// <param name="root">The base node</param>
        public void SetSkeleton(RigidNode_Base root)
        {
            if (root == null) nodeList = null;
            else nodeList = root.ListAllNodes();
            UpdateJointList();
        }

        /// <summary>
        /// Update the list of joints
        /// </summary>
        private void UpdateJointList()
        {
            lstJoints.Items.Clear();

            if (nodeList == null) return;

            foreach (RigidNode_Base node in nodeList)
            {
                if (node.GetSkeletalJoint() != null)
                {
                    SkeletalJoint_Base joint = node.GetSkeletalJoint();
                    if (joint != null)
                    {
                        WheelDriverMeta wheelData = null;
                        if (joint.cDriver != null)
                        {
                            wheelData = joint.cDriver.GetInfo<WheelDriverMeta>();
                        }

                        System.Windows.Forms.ListViewItem item = new System.Windows.Forms.ListViewItem(new string[] { 
                    Enum.GetName(typeof(SkeletalJointType),joint.GetJointType()).ToLowerInvariant(),
                        node.GetParent().modelFileName,
                        node.modelFileName, joint.cDriver!=null ? joint.cDriver.ToString() : "No driver",
                        wheelData!=null ? wheelData.GetTypeString() : "No Wheel",
                        joint.attachedSensors.Count.ToString()});
                        item.Tag = node;
                        lstJoints.Items.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Get the current state of the editor pane
        /// </summary>
        /// <returns>Whether or not a joint is currently being edited</returns>
        public bool IsEditingJoint()
        {
            return currentlyEditing;
        }
    }
}