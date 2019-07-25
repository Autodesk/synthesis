using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BxDRobotExporter.GUI.Editors.JointSubEditors;
using BxDRobotExporter.OGLViewer;
using BxDRobotExporter.Utilities.Synthesis;

namespace BxDRobotExporter.GUI.Editors.AdvancedJointEditor
{
    public partial class AdvancedJointEditorUserControl : UserControl
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
        private JointDriverEditorForm jointDriverEditorFormDialog = new JointDriverEditorForm();

        /// <summary>
        /// The list of nodes to be attached as metadata
        /// </summary>
        private List<RigidNode_Base> nodeList = null;
        
        private Timer selectionFinishedTimeout = new Timer();
        private RobotData robotData;

        /// <summary>
        /// Create a new JointEditorPane and register actions for the right click menu
        /// </summary>
        public AdvancedJointEditorUserControl()
        {
            InitializeComponent();
            this.DoLayout(null, null);
            
            RegisterContextAction("Edit Driver", EditDriver_Internal);
            RegisterContextAction("Edit Sensors", ListSensors_Internal);
            RegisterContextAction("Edit Limits", (List<RigidNode_Base> nodes) =>
                {
                    try// prevent the user from just left clicking on the black joint pane and trying to edit nothing
                    {
                        if (nodes.Count != 1) return;

                        var node = nodes[0];
                        if (node != null && node.GetSkeletalJoint() != null)// prevents the user from trying to edit a null joint
                        {
                            var limitEditor = new JointLimitEditorForm(node.GetSkeletalJoint());// show the limit editor form
                            limitEditor.ShowDialog(ParentForm);
                        }
                    }
                    catch (NullReferenceException)//catch when the user clicks on the pane without a node selected
                    {
                        MessageBox.Show("Please select a node!");
                    }
                });

            lstJoints.Activation = ItemActivation.Standard;
            lstJoints.ItemActivate += (object sender, EventArgs e) =>
                {

                    EditDriver_Internal(GetSelectedNodes());
                };

            selectionFinishedTimeout.Tick += FinishedSelecting;
            selectionFinishedTimeout.Interval = 55; // minimum accuracy of winforms timers
            
            SelectedJoint += nodes => InventorUtils.FocusAndHighlightNodes(nodes, RobotExporterAddInServer.Instance.Application.ActiveView.Camera,  1);
            ModifiedJoint += delegate (List<RigidNode_Base> nodes)
            {

                if (nodes == null || nodes.Count == 0) return;

                foreach (var node in nodes)
                {
                    if (node.GetSkeletalJoint() != null && node.GetSkeletalJoint().cDriver != null &&
                        node.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>() != null &&
                        node.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>().radius == 0 &&
                        node is OglRigidNode)
                    {
                        (node as OglRigidNode).GetWheelInfo(out var radius, out var width, out var center);

                        var wheelDriver = node.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>();
                        wheelDriver.center = center;
                        wheelDriver.radius = radius;
                        wheelDriver.width = width;
                        node.GetSkeletalJoint().cDriver.AddInfo(wheelDriver);

                    }
                }
            };
        }

        /// <summary>
        /// Register an action for the right click menu
        /// </summary>
        /// <param name="caption">The caption of the menu item</param>
        /// <param name="callback">The action to perform when the item is clicked</param>
        public void RegisterContextAction(string caption, JointEditorEvent callback)
        {
            var item = new ToolStripButton()
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
            foreach (var listItem in lstJoints.Items.OfType<ListViewItem>())
            {
                if (!lstJoints.SelectedItems.Contains(listItem)) listItem.BackColor = Control.DefaultBackColor;
            }
            var item = lstJoints.Items.OfType<ListViewItem>().FirstOrDefault(i => i.Tag == node);

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
            item_chChild.Width = this.lstJoints.Width / 3;
            item_chDrive.Width = this.lstJoints.Width / 3;
            item_chWheel.Width = this.lstJoints.Width / 3;
            item_chSensors.Width = this.lstJoints.Width / 8;

            this.lstJoints.Width = this.Width;
            this.lstJoints.Height = this.Height - 6;
        }

        /// <summary>
        /// The <see cref="JointEditorEvent"/> to open up a <see cref="JointSensorListForm"/>
        /// </summary>
        /// <param name="node">The node connected to the joint to edit the sensors on</param>
        private void ListSensors_Internal(List<RigidNode_Base> nodes)
        {
            if (nodes == null || nodes.Count != 1) return;
            var node = nodes[0];

            if (node == null) return;

            currentlyEditing = true;
            var listForm = new JointSensorListForm(node.GetSkeletalJoint());
            listForm.ShowDialog(ParentForm);
            ModifiedJoint?.Invoke(nodes);
            this.UpdateJointList();
            currentlyEditing = false;
        }

        /// <summary>
        /// The <see cref="JointEditorEvent"/> to open up a <see cref="JointDriverEditorForm"/> dialog
        /// </summary>
        /// <param name="node">The node connected to the joint with a driver to edit</param>
        private void EditDriver_Internal(List<RigidNode_Base> nodes)
        {
            if (nodes == null || nodes.Count == 0) return;

            currentlyEditing = true;

            jointDriverEditorFormDialog.ShowDialog(nodes[0].GetSkeletalJoint(), nodes, ParentForm);

            if (ModifiedJoint != null && jointDriverEditorFormDialog.Saved)
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
            selectionFinishedTimeout.Start(); // TODO: Find less hacky way to detect when selection is finished

        }
        
        private void FinishedSelecting(object sender, EventArgs e)
        {
            selectionFinishedTimeout.Stop();
            foreach (var item in lstJoints.Items.OfType<ListViewItem>())
            {
                item.BackColor = Control.DefaultBackColor;
            }

            if (lstJoints.SelectedItems.Count > 0)
            {
                foreach (var item in lstJoints.SelectedItems.OfType<ListViewItem>())
                {
                    item.BackColor = System.Drawing.Color.LightSteelBlue;
                }

                Refresh(); // Force repaint of the view

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
        public void UpdateSkeleton(RobotData robotData)
        {
            this.robotData = robotData;
            nodeList = robotData.RobotBaseNode?.ListAllNodes();
            UpdateJointList();
        }

        /// <summary>
        /// Update the list of joints
        /// </summary>
        private void UpdateJointList()
        {
            lstJoints.Items.Clear();

            if (nodeList == null) return;

            foreach (var node in nodeList)
            {
                if (node.GetSkeletalJoint() != null)
                {
                    var joint = node.GetSkeletalJoint();
                    if (joint != null)
                    {
                        var item = new ListViewItem(new[] {
                            JointToStringUtils.JointTypeString(joint, robotData), JointToStringUtils.NodeNameString(node), JointToStringUtils.DriverString(joint), JointToStringUtils.WheelTypeString(joint), JointToStringUtils.SensorCountString(joint)})
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
            var types = (from item in lstJoints.SelectedItems.OfType<ListViewItem>()
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

        private void lstJoints_SizeChanged(object sender, EventArgs e)
        {
            this.DoLayout(null, null);
        }
    }
}