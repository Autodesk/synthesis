﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Runtime.InteropServices;

namespace BxDRobotExporter.Wizard
{
    public enum WheelSide
    {
        UNASSIGNED = 0,
        LEFT = 1,
        RIGHT = 2
    }

    /// <summary>
    /// Prompts the user to define all the wheels on the robot.
    /// </summary>
    public partial class DefineWheelsPage : UserControl, IWizardPage
    {
        private float totalWeightKg = 0;
        private bool preferMetric = false;

        /// <summary>
        /// Dictionary associating node file names with their respective <see cref="RigidNode_Base"/>s
        /// </summary>
        private Dictionary<string, WheelSlotPanel> wheelSlots = new Dictionary<string, WheelSlotPanel>();
        private List<string> leftOrder = new List<string>();
        private List<string> rightOrder = new List<string>();

        public DefineWheelsPage()
        {
            InitializeComponent();

            // Hide horizontal scroll bars
            LeftWheelsPanel.AutoScroll = false;
            LeftWheelsPanel.HorizontalScroll.Maximum = 0;
            LeftWheelsPanel.AutoScroll = true;
            RightWheelsPanel.AutoScroll = false;
            RightWheelsPanel.HorizontalScroll.Maximum = 0;
            RightWheelsPanel.AutoScroll = true;

            DriveTrainDropdown.SelectedIndex = 0;

            NodeListBox.Enabled = false;

            // Load weight information
            preferMetric = Utilities.GUI.RMeta.PreferMetric;
            SetWeightBoxValue(Utilities.GUI.RMeta.TotalWeightKg * (preferMetric ? 1 : 2.20462f));
            WeightUnitSelector.SelectedIndex = Utilities.GUI.RMeta.PreferMetric ? 1 : 0;
        }

        /// <summary>
        /// Sets the limits of <see cref="WheelCountUpDown"/> and validates input.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DriveTrainDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (DriveTrainDropdown.SelectedIndex)
            {
                case 0: //Undefined
                    WizardData.Instance.driveTrain = WizardData.WizardDriveTrain.CUSTOM;
                    break;
                case 1: //Tank
                    WizardData.Instance.driveTrain = WizardData.WizardDriveTrain.TANK;
                    break;
                case 2: //Mecanum
                    WizardData.Instance.driveTrain = WizardData.WizardDriveTrain.MECANUM;
                    break;
                case 3: //Swerve
                    WizardData.Instance.driveTrain = WizardData.WizardDriveTrain.SWERVE;
                    break;
                case 4: //H-Drive
                    WizardData.Instance.driveTrain = WizardData.WizardDriveTrain.H_DRIVE;
                    break;
                case 5: //Custom
                    WizardData.Instance.driveTrain = WizardData.WizardDriveTrain.CUSTOM;
                    break;
            }

            NodeListBox.Enabled = true;
            Initialize();

            OnInvalidatePage();
        }

        /// <summary>
        /// Validates input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Panel_WheelTypeChanged(object sender, WheelTypeChangedEventArgs e)
        {
        }

        /// <summary>
        /// Gets the next unfilled <see cref="WheelSlotPanel"/>. Referenced in <see cref="NodeListBox_ItemCheck(object, ItemCheckEventArgs)"/>
        /// </summary>
        /// <returns></returns>

        #region IWizardPage Implementation
        /// <summary>
        /// Adds all the <see cref="WizardData.WheelSetupData"/> from each <see cref="WheelSetupPanel"/> to <see cref="WizardData.wheels"/>
        /// </summary>
        public void OnNext()
        {
            UpdateWeight();
            WizardData.Instance.weightKg = totalWeightKg;
            WizardData.Instance.preferMetric = preferMetric;
            WizardData.Instance.wheels = new List<WizardData.WheelSetupData>();

            foreach(KeyValuePair<string, WheelSlotPanel> slot in wheelSlots)
            {
                if (slot.Value.SetupPanel.Side != WheelSide.UNASSIGNED)
                    WizardData.Instance.wheels.Add(slot.Value.WheelData);
            }
        }

        /// <summary>
        /// Adds as many <see cref="WheelSlotPanel"/>s as there are wheels
        /// </summary>
        public void Initialize()
        {
            // Clear existing panels
            while (LeftWheelsPanel.Controls.Count > 0)
                LeftWheelsPanel.Controls[0].Dispose();

            while (RightWheelsPanel.Controls.Count > 0)
                RightWheelsPanel.Controls[0].Dispose();

            Dictionary<string, RigidNode_Base> availableNodes = new Dictionary<string, RigidNode_Base>(); // TODO: Rename this to availableNodes after a different merge
            wheelSlots = new Dictionary<string, WheelSlotPanel>();
            
            // Find all nodes that can be wheels
            Dictionary<string, int> duplicatePartNames = new Dictionary<string, int>();

            foreach (RigidNode_Base node in Utilities.GUI.SkeletonBase.ListAllNodes())
            {
                if ((node.GetSkeletalJoint() != null && node.GetSkeletalJoint().GetJointType() == SkeletalJointType.ROTATIONAL) ||
                    (WizardData.Instance.driveTrain == WizardData.WizardDriveTrain.SWERVE && node.GetParent().GetParent() != null))
                {
                    string readableName = node.ModelFileName.Replace('_', ' ').Replace(".bxda", "");
                    readableName = readableName.Substring(0, 1).ToUpperInvariant() + readableName.Substring(1); // Capitalize first character

                    if (availableNodes.ContainsKey(readableName))
                    {
                        // Add the part name to the list of duplicate parts
                        if (!duplicatePartNames.ContainsKey(node.ModelFileName))
                            duplicatePartNames.Add(node.ModelFileName, 2);

                        // Find the next available name
                        int identNum = duplicatePartNames[node.ModelFileName];
                        while (availableNodes.ContainsKey(readableName + ' ' + identNum) && identNum <= 100)
                            identNum++;

                        // Add the joint to the list with the new unique name
                        readableName += ' ' + identNum.ToString();

                        // Update the next available ID
                        duplicatePartNames[node.ModelFileName] = identNum;
                    }
                    
                    availableNodes.Add(readableName, node);
                }
            }

            // Generate panels
            foreach (KeyValuePair<string, RigidNode_Base> node in availableNodes)
            {
                WheelSlotPanel panel = new WheelSlotPanel();

                // Get default wheel type based on drive train
                WizardData.WizardWheelType type;
                if (WizardData.Instance.driveTrain == WizardData.WizardDriveTrain.MECANUM)
                    type = WizardData.WizardWheelType.MECANUM;
                else if (WizardData.Instance.driveTrain == WizardData.WizardDriveTrain.H_DRIVE)
                    type = WizardData.WizardWheelType.OMNI;
                else
                    type = WizardData.WizardWheelType.NORMAL;

                // Create panel
                panel.FillSlot(node.Value, node.Key, WheelSide.UNASSIGNED, type);
                panel.SetupPanel.removeHandler += RemoveNodeFromPanel;
                panel.SetupPanel.mouseDownHandler += SetupPanel_StartDrag;
                
                wheelSlots.Add(node.Key, panel);
            }

            _initialized = true;

            UpdateUI();
        }

        private bool _initialized = false;
        public bool Initialized
        {
            get => _initialized;
            set
            {
                _initialized = value;
            }
        }

        public event Action ActivateNext;
        private void OnActivateNext() => ActivateNext?.Invoke();

        public event Action DeactivateNext;
        private void OnDeactivateNext() => DeactivateNext?.Invoke();

        public event Action<bool> SetEndEarly;
        private void OnSetEndEarly(bool enabled) => SetEndEarly?.Invoke(enabled);

        public event InvalidatePageEventHandler InvalidatePage;
        public void OnInvalidatePage()
        {
            InvalidatePage?.Invoke(typeof(DefineMovingPartsPage));
        }
        #endregion
        
        /// <summary>
        /// Sets the side that a wheel is on. This will update the wizard UI to show the changes.
        /// </summary>
        /// <param name="nodeName">Readable name of the node to set side of.</param>
        /// <param name="side">Side to move node to.</param>
        public void SetWheelSide(string nodeName, WheelSide side)
        {
            if (nodeName == null)
                return;

            if (!wheelSlots.ContainsKey(nodeName))
                return;

            wheelSlots[nodeName].SetupPanel.Side = side;
            UpdateUI();
        }

        /// <summary>
        /// Sets the side that a wheel is on. This will update the wizard UI to show the changes.
        /// </summary>
        /// <param name="nodeName">Node to set side of.</param>
        /// <param name="side">Side to move node to.</param>
        public void SetWheelSide(RigidNode_Base node, WheelSide side)
        {
            if (node == null)
                return;

            foreach(KeyValuePair<string, WheelSlotPanel> slot in wheelSlots)
            {
                if (slot.Value.SetupPanel.Node == node)
                {
                    SetWheelSide(slot.Key, side);
                    return;
                }
            }
        }

        /// <summary>
        /// Reloads node list view and left/right wheel panels.
        /// </summary>
        private void UpdateUI()
        {
            ValidateLeftRightLists();

            // Remove all items from
            NodeListBox.Items.Clear();
            LeftWheelsPanel.Controls.Clear();
            LeftWheelsPanel.RowCount = 0;
            LeftWheelsPanel.RowStyles.Clear();
            RightWheelsPanel.Controls.Clear();
            RightWheelsPanel.RowCount = 0;
            RightWheelsPanel.RowStyles.Clear();

            // Pause layout calculations to prevent siezures
            LeftWheelsPanel.SuspendLayout();
            RightWheelsPanel.SuspendLayout();

            // Add left items to left side
            foreach (string name in leftOrder)
                AddControlToNewTableRow(wheelSlots[name], LeftWheelsPanel);

            // Add right items to right side
            foreach (string name in rightOrder)
                AddControlToNewTableRow(wheelSlots[name], RightWheelsPanel);

            // Add all remaining items to list box
            int unassignedNodes = 0;
            foreach (KeyValuePair<string, WheelSlotPanel> slot in wheelSlots)
            {
                if (slot.Value.SetupPanel.Side == WheelSide.UNASSIGNED)
                {
                    NodeListBox.Items.Add(slot.Key);
                    unassignedNodes++;
                }
            }

            // Shrink items width if a scroll bar will appear
            if (leftOrder.Count <= 3)
                LeftWheelsPanel.ColumnStyles[1].Width = 0;
            else
                LeftWheelsPanel.ColumnStyles[1].Width = SystemInformation.VerticalScrollBarWidth;

            // Shrink items width if a scroll bar will appear
            if (rightOrder.Count <= 3)
                RightWheelsPanel.ColumnStyles[1].Width = 0;
            else
                RightWheelsPanel.ColumnStyles[1].Width = SystemInformation.VerticalScrollBarWidth;

            OnSetEndEarly(unassignedNodes == 0); // Skip next page if no parts are left

            // Resume layout calculations
            LeftWheelsPanel.ResumeLayout();
            RightWheelsPanel.ResumeLayout();
        }

        private void ValidateLeftRightLists()
        {
            // Make sure all items in left orders are left wheels
            int n = 0;
            while (n < leftOrder.Count)
            {
                if (wheelSlots[leftOrder[n]].SetupPanel.Side != WheelSide.LEFT)
                    leftOrder.RemoveAt(n);
                else
                    n++;
            }

            // Make sure all items in right orders are right wheels
            n = 0;
            while (n < rightOrder.Count)
            {
                if (wheelSlots[rightOrder[n]].SetupPanel.Side != WheelSide.RIGHT)
                    rightOrder.RemoveAt(n);
                else
                    n++;
            }

            foreach (KeyValuePair<string, WheelSlotPanel> wheelSlot in wheelSlots)
            {
                // If it should exist in the left order, add it to the left order
                if (wheelSlot.Value.SetupPanel.Side == WheelSide.LEFT)
                    if (!leftOrder.Contains(wheelSlot.Key))
                        leftOrder.Add(wheelSlot.Key);

                // If it should exist in the right order, add it to the right order
                if (wheelSlot.Value.SetupPanel.Side == WheelSide.RIGHT)
                    if (!rightOrder.Contains(wheelSlot.Key))
                        rightOrder.Add(wheelSlot.Key);
            }
        }

        private void UpdateWeight()
        {
            if (WeightUnitSelector.SelectedIndex == 0)
                totalWeightKg = (float)WeightBox.Value / 2.20462f;
            else
                totalWeightKg = (float)WeightBox.Value;

            preferMetric = WeightUnitSelector.SelectedIndex == 1;
        }

        private void SetWeightBoxValue(float value)
        {
            if ((decimal)value > WeightBox.Maximum)
                WeightBox.Value = WeightBox.Maximum;
            else if ((decimal)value >= WeightBox.Minimum)
                WeightBox.Value = (decimal)value;
            else
                WeightBox.Value = 0;
        }

        /// <summary>
        /// Called when an item is selected in the node list. Begins process of drag and drop to a panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NodeListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (NodeListBox.SelectedItem != null)
            {
                // Highlight node in Inventor
                StandardAddInServer.Instance.WizardSelect(wheelSlots[NodeListBox.SelectedItem.ToString()].Node);
                // Start drag-and-drop process
                NodeListBox.DoDragDrop(NodeListBox.SelectedItem.ToString(), DragDropEffects.Move);
            }
        }

        /// <summary>
        /// Called when the user starts dragging from a setup panel.
        /// </summary>
        /// <param name="name">Name of node controlled by setup panel.</param>
        private void SetupPanel_StartDrag(string name)
        {
            if (name == null)
                return;

            if (!wheelSlots.ContainsKey(name))
                return;

            // Highlight node in Inventor
            StandardAddInServer.Instance.WizardSelect(wheelSlots[name].SetupPanel.Node);
            // Start drag-and-drop process
            NodeListBox.DoDragDrop(name, DragDropEffects.Move);
        }

        /// <summary>
        /// Called when the user moves the mouse over a wheel panel while dragging.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WheelsPanel_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        /// <summary>
        /// Called when the user drops a dragged item into the left wheel panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeftWheelsPanel_DragDrop(object sender, DragEventArgs e)// called when the user "drops" a seleected value into the group
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string nodeName = (string)e.Data.GetData(DataFormats.StringFormat, true);

                SetWheelSide(nodeName, WheelSide.LEFT);
            }
        }

        /// <summary>
        /// Called when the user drops a dragged item into the right wheel panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RightWheelsPanel_DragDrop(object sender, DragEventArgs e)// called when the user "drops" a seleected value into the group
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string nodeName = (string)e.Data.GetData(DataFormats.StringFormat, true);

                SetWheelSide(nodeName, WheelSide.RIGHT);
            }
        }

        /// <summary>
        /// Called when the user drops a dragged item into the wheel list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NodeListBox_DragDrop(object sender, DragEventArgs e)// called when the user "drops" a seleected value into the group
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string nodeName = (string)e.Data.GetData(DataFormats.StringFormat, true);

                SetWheelSide(nodeName, WheelSide.UNASSIGNED);
            }
        }

        private void AddControlToNewTableRow(Control control, TableLayoutPanel table, RowStyle rowStyle = null)
        {
            if (rowStyle == null)
                rowStyle = new RowStyle();
            
            table.RowCount++;
            table.RowStyles.Add(rowStyle);
            table.Controls.Add(control);
            table.SetRow(control, table.RowCount - 1);
            table.SetColumn(control, 0);
        }

        private void RemoveNodeFromPanel(string name)
        {
            SetWheelSide(name, WheelSide.UNASSIGNED);
        }

        private void NodeListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (NodeListBox.SelectedItem != null)
            {
                // Highlight node in Inventor
                StandardAddInServer.Instance.WizardSelect(wheelSlots[NodeListBox.SelectedItem.ToString()].Node);
            }
        }
    }
}