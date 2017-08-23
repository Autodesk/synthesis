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
        public delegate void JointEditorEvent(List<RigidNode_Base> nodes);

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
            
            RegisterContextAction("Edit Driver", EditDriver_Internal);
            RegisterContextAction("Edit Sensors", ListSensors_Internal);
            RegisterContextAction("Edit Limits", (List<RigidNode_Base> nodes) =>
                {
                    if (nodes.Count != 1) return;

                    RigidNode_Base node = nodes[0];
                    if (node != null && node.GetSkeletalJoint() != null)
                    {
                        EditLimits limitEditor = new EditLimits(node.GetSkeletalJoint())
                        {
                            StartPosition = FormStartPosition.Manual,
                            Location = new System.Drawing.Point(Cursor.Position.X - 10, Cursor.Position.Y - 10)
                        };
                        limitEditor.ShowDialog(ParentForm);
                    }
                });

            lstJoints.Activation = ItemActivation.Standard;
            lstJoints.ItemActivate += (object sender, EventArgs e) =>
                {

                    EditDriver_Internal(GetSelectedNodes());
                };
        }

        /// <summary>
        /// Register an action for the right click menu
        /// </summary>
        /// <param name="caption">The caption of the menu item</param>
        /// <param name="callback">The action to perform when the item is clicked</param>
        public void RegisterContextAction(string caption, JointEditorEvent callback)
        {
            ToolStripButton item = new ToolStripButton()
            {
                Text = caption
            };
            item.Click += (object sender, EventArgs e) =>
            {
                if (lstJoints.SelectedItems.Count > 0 && lstJoints.SelectedItems[0].Tag is RigidNode_Base)
                {
                    callback(GetSelectedNodes());
                }
                else
                {
                    callback(null);
                }
            };
            
            jointContextMenu.Items.Add(item);
            jointContextMenu.Opening += JointContextMenu_Opening;
        }

        public void AddSelection(RigidNode_Base node, bool clearActive)
        {
            if (clearActive) lstJoints.SelectedItems.Clear();
            foreach (ListViewItem listItem in lstJoints.Items.OfType<ListViewItem>())
            {
                if (!lstJoints.SelectedItems.Contains(listItem)) listItem.BackColor = Control.DefaultBackColor;
            }
            ListViewItem item = lstJoints.Items.OfType<ListViewItem>().FirstOrDefault(i => i.Tag == node);

            if (item != null)
            {
                item.BackColor = System.Drawing.Color.LightSteelBlue;
                item.Selected = true;
                item.Focused = true;
            }
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
        private void ListSensors_Internal(List<RigidNode_Base> nodes)
        {
            if (nodes == null || nodes.Count != 1) return;
            RigidNode_Base node = nodes[0];

            if (node == null) return;

            currentlyEditing = true;
            SensorListForm listForm = new SensorListForm(node.GetSkeletalJoint())
            {
                StartPosition = FormStartPosition.Manual,
                Location = new System.Drawing.Point(Cursor.Position.X - 10, Cursor.Position.Y - 10)
            };
            listForm.ShowDialog(ParentForm);
            ModifiedJoint?.Invoke(nodes);
            this.UpdateJointList();
            currentlyEditing = false;
        }

        /// <summary>
        /// The <see cref="JointEditorEvent"/> to open up a <see cref="DriveChooser"/> dialog
        /// </summary>
        /// <param name="node">The node connected to the joint with a driver to edit</param>
        private void EditDriver_Internal(List<RigidNode_Base> nodes)
        {
            if (nodes == null || nodes.Count == 0) return;

            currentlyEditing = true;

            driveChooserDialog.StartPosition = FormStartPosition.Manual;
            driveChooserDialog.Location = new System.Drawing.Point(Cursor.Position.X - 10, Cursor.Position.Y - 10);
            driveChooserDialog.ShowDialog(nodes[0].GetSkeletalJoint(), nodes, ParentForm);

            if (ModifiedJoint != null && driveChooserDialog.Saved)
            {
                ModifiedJoint(nodes);
            }
            UpdateJointList();
            currentlyEditing = false;
        }

        /// <summary>
        /// Highlight a node in the viewer when it's selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LstJoints_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lstJoints.Items.OfType<ListViewItem>())
            {
                item.BackColor = Control.DefaultBackColor;
            }

            if (lstJoints.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in lstJoints.SelectedItems.OfType<ListViewItem>())
                {
                    item.BackColor = System.Drawing.Color.LightSteelBlue;
                }

                SelectedJoint?.Invoke(GetSelectedNodes());
            }
            else
            {
                SelectedJoint?.Invoke(null);
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

                        ListViewItem item = new ListViewItem(new string[] {
                    Enum.GetName(typeof(SkeletalJointType),joint.GetJointType()).ToLowerInvariant(),
                        node.GetParent().ModelFileName,
                        node.ModelFileName, joint.cDriver!=null ? joint.cDriver.ToString() : "No driver",
                        wheelData!=null ? wheelData.GetTypeString() : "No Wheel",
                        joint.attachedSensors.Count.ToString()})
                        {
                            Tag = node
                        };
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

        private void JointContextMenu_Opening(object sender, EventArgs e)
        {
            List<SkeletalJointType> types = (from item in lstJoints.SelectedItems.OfType<ListViewItem>()
                                             select ((RigidNode_Base)item.Tag).GetSkeletalJoint().GetJointType()).ToList();

            if (types.Count > 1 && types[0] != types[1])
            {
                jointContextMenu.Items[0].Enabled = false;
            }
            else
            {
                jointContextMenu.Items[0].Enabled = true;
            }

            if (lstJoints.SelectedItems.Count > 1)
            {
                jointContextMenu.Items[1].Enabled = false;
                jointContextMenu.Items[2].Enabled = false;
            }
            else
            {
                jointContextMenu.Items[1].Enabled = true;
                jointContextMenu.Items[2].Enabled = true;
            }
        }

        private List<RigidNode_Base> GetSelectedNodes()
        {
            var items = from item in lstJoints.SelectedItems.OfType<ListViewItem>()
                        where item.Tag is RigidNode_Base
                        select item.Tag as RigidNode_Base;

            return items.ToList();
        }
    }
}