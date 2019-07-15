using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BxDRobotExporter.Wizard
{
    public enum WheelSide
    {
        UNASSIGNED = 0,
        LEFT = 1,
        RIGHT = 2,
        MIDDLE = 3,
        LEFTBACK = 4,
        RIGHTBACK = 5
    }

    /// <summary>
    /// Prompts the user to define all the wheels on the robot.
    /// </summary>
    public partial class DefineWheelsPage : UserControl, IWizardPage
    {
        private float totalWeightKg = 0;
        private bool preferMetric = false;
        private int numberOfJoints;
        /// <summary>
        /// Dictionary associating node file names with their respective <see cref="RigidNode_Base"/>s
        /// </summary>
        private Dictionary<string, WheelSetupPanel> setupPanels = new Dictionary<string, WheelSetupPanel>();
        // lists containing which wheels belong to which panel
        private List<string> leftOrder = new List<string>();
        private List<string> rightOrder = new List<string>();
        private List<string> leftBackOrder = new List<string>();
        private List<string> rightBackOrder = new List<string>();
        private List<string> middleOrder = new List<string>();

        public DefineWheelsPage()
        {
            InitializeComponent();

            AutoFillToolTip.SetToolTip(AutoFillButton, "Attempt to detect left and right wheels automatically. Remember to double check your configuration after using this tool.");

            // Hide horizontal scroll bars
            LeftWheelsPanel.AutoScroll = false;
            LeftWheelsPanel.HorizontalScroll.Maximum = 0;
            LeftWheelsPanel.AutoScroll = true;
            RightWheelsPanel.AutoScroll = false;
            RightWheelsPanel.HorizontalScroll.Maximum = 0;
            RightWheelsPanel.AutoScroll = true;
            MiddleWheelsPanel.AutoScroll = false;
            MiddleWheelsPanel.HorizontalScroll.Maximum = 0;
            MiddleWheelsPanel.AutoScroll = true;
            RightBackWheelsPanel.AutoScroll = false;
            RightBackWheelsPanel.HorizontalScroll.Maximum = 0;
            RightBackWheelsPanel.AutoScroll = true;
            LeftBackWheelsPanel.AutoScroll = false;
            LeftBackWheelsPanel.HorizontalScroll.Maximum = 0;
            LeftBackWheelsPanel.AutoScroll = true;

            // Load weight information
            preferMetric = Utilities.GUI.RMeta.PreferMetric;
            SetWeightBoxValue(Utilities.GUI.RMeta.TotalWeightKg * (preferMetric ? 1 : 2.20462f));
            WeightUnitSelector.SelectedIndex = Utilities.GUI.RMeta.PreferMetric ? 1 : 0;
            
            FillFromPreviousSetup(Utilities.GUI.SkeletonBase);
        }

        /// <summary>
        /// Sets the limits of <see cref="WheelCountUpDown"/> and validates input.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DriveTrainDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (DriveTrainDropdown.SelectedIndex)// depending on drive train type show or hide the corresponding groups and panels
            {
                case 0: //Undefined
                    this.LeftWheelsGroup.Size = new System.Drawing.Size(224, 480);
                    this.LeftWheelsPanel.Size = new System.Drawing.Size(218, 461);
                    this.RightWheelsGroup.Size = new System.Drawing.Size(224, 480);
                    this.RightWheelsPanel.Size = new System.Drawing.Size(218, 461);
                    this.MainLayout.RowCount = 3;
                    this.MiddleWheelsGroup.Visible = false;
                    this.MiddleWheelsPanel.Visible = false;
                    this.LeftBackWheelsPanel.Visible = false;
                    this.LeftBackWheelsGroup.Visible = false;
                    this.RightBackWheelsPanel.Visible = false;
                    this.RightBackWheelsGroup.Visible = false;

                    SynthesisGUI.Instance.SkeletonBase.driveTrainType = RigidNode_Base.DriveTrainType.NONE;
                    break;
                case 1: //Tank
                    this.LeftWheelsGroup.Size = new System.Drawing.Size(224, 480);
                    this.LeftWheelsPanel.Size = new System.Drawing.Size(218, 461);
                    this.RightWheelsGroup.Size = new System.Drawing.Size(224, 480);
                    this.RightWheelsPanel.Size = new System.Drawing.Size(218, 461);
                    this.MainLayout.RowCount = 3;
                    this.MiddleWheelsGroup.Visible = false;
                    this.MiddleWheelsPanel.Visible = false;
                    this.LeftBackWheelsPanel.Visible = false;
                    this.LeftBackWheelsGroup.Visible = false;
                    this.RightBackWheelsPanel.Visible = false;
                    this.RightBackWheelsGroup.Visible = false;
                    SynthesisGUI.Instance.SkeletonBase.driveTrainType = RigidNode_Base.DriveTrainType.TANK;
                    break;
                case 2: //H-Drive
                    this.LeftWheelsGroup.Size = new System.Drawing.Size(224, 374);
                    this.LeftWheelsPanel.Size = new System.Drawing.Size(218, 355);
                    this.RightWheelsGroup.Size = new System.Drawing.Size(224, 374);
                    this.RightWheelsPanel.Size = new System.Drawing.Size(218, 355);
                    this.MainLayout.RowCount = 4;
                    this.MainLayout.RowStyles[2] = new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 72.72727F);
                    this.MainLayout.RowStyles[3] = new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 27.27273F);
                    this.MiddleWheelsGroup.Visible = true;
                    this.MiddleWheelsPanel.Visible = true;
                    this.LeftBackWheelsPanel.Visible = false;
                    this.LeftBackWheelsGroup.Visible = false;
                    this.RightBackWheelsPanel.Visible = false;
                    this.RightBackWheelsGroup.Visible = false;
                    SynthesisGUI.Instance.SkeletonBase.driveTrainType = RigidNode_Base.DriveTrainType.H_DRIVE;
                    break;
                case 3: //Custom
                    this.LeftWheelsGroup.Size = new System.Drawing.Size(224, 480);
                    this.LeftWheelsPanel.Size = new System.Drawing.Size(218, 461);
                    this.RightWheelsGroup.Size = new System.Drawing.Size(224, 480);
                    this.RightWheelsPanel.Size = new System.Drawing.Size(218, 461);
                    this.MainLayout.RowCount = 3;
                    this.MiddleWheelsGroup.Visible = false;
                    this.MiddleWheelsPanel.Visible = false;
                    this.LeftBackWheelsPanel.Visible = false;
                    this.LeftBackWheelsGroup.Visible = false;
                    this.RightBackWheelsPanel.Visible = false;
                    this.RightBackWheelsGroup.Visible = false;
                    SynthesisGUI.Instance.SkeletonBase.driveTrainType = RigidNode_Base.DriveTrainType.CUSTOM;
                    break;
            }

            if (DriveTrainDropdown.SelectedIndex > 0)// if there is a drive train selected then enable the buttons, otherwise disable them until a drive train is selected
            {
                NodeListBox.Enabled = true;
                AutoFillButton.Enabled = true;
                DefineWheelsInstruction.Text = "Drag wheel parts from the list into the appropriate column below.";
            }
            else
            {
                NodeListBox.Enabled = false;
                AutoFillButton.Enabled = false;
                DefineWheelsInstruction.Text = "Please select a drive train to perform wheel setup.";
            }

            Initialize();
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
            foreach (KeyValuePair<string, WheelSetupPanel> panel in setupPanels)
            {
                if (panel.Value.Side != WheelSide.UNASSIGNED)
                    WizardData.Instance.wheels.Add(panel.Value.GetWheelData());
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

            while (MiddleWheelsPanel.Controls.Count > 0)
                MiddleWheelsPanel.Controls[0].Dispose();

            while (RightBackWheelsPanel.Controls.Count > 0)
                RightBackWheelsPanel.Controls[0].Dispose();

            while (LeftBackWheelsPanel.Controls.Count > 0)
                LeftBackWheelsPanel.Controls[0].Dispose();

            Dictionary<string, RigidNode_Base> availableNodes = new Dictionary<string, RigidNode_Base>(); // TODO: Rename this to availableNodes after a different merge
            setupPanels = new Dictionary<string, WheelSetupPanel>();
            leftOrder = new List<string>();
            rightOrder = new List<string>();
            middleOrder = new List<string>();
            leftBackOrder = new List<string>();
            rightBackOrder = new List<string>();

            // Find all nodes that can be wheels
            Dictionary<string, int> duplicatePartNames = new Dictionary<string, int>();

            numberOfJoints = 0;

            foreach (RigidNode_Base node in Utilities.GUI.SkeletonBase.ListAllNodes())
            {
                if ((node.GetSkeletalJoint() != null))
                {
                    numberOfJoints++;
                }
            }

            foreach (RigidNode_Base node in Utilities.GUI.SkeletonBase.ListAllNodes())
            {
                if (node.GetSkeletalJoint() != null && node.GetSkeletalJoint().GetJointType() == SkeletalJointType.ROTATIONAL)
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

                // Get default wheel type based on drive train
                WizardData.WizardWheelType type;
                if (SynthesisGUI.Instance.SkeletonBase.driveTrainType == RigidNode_Base.DriveTrainType.H_DRIVE)
                    type = WizardData.WizardWheelType.OMNI;
                else
                    type = WizardData.WizardWheelType.NORMAL;

                // Create panel
                WheelSetupPanel panel = new WheelSetupPanel(node.Value, node.Key, type);
                panel.removeHandler += RemoveNodeFromPanel;
                panel.mouseDownHandler += SetupPanel_StartDrag;
                
                setupPanels.Add(node.Key, panel);
            }

            _initialized = true;

            UpdateUI();
            OnInvalidatePage(); // Reset the next page in the wizard
        }

        private bool _initialized = false;
        public bool Initialized
        {
            get => _initialized;
            set
            {
                if (!value) // Page is being invalidated, reset interface
                {
                    foreach (KeyValuePair<string, WheelSetupPanel> panel in setupPanels)
                        panel.Value.Dispose();
                    setupPanels.Clear();
                    leftOrder.Clear();
                    rightOrder.Clear();
                    middleOrder.Clear();
                    leftBackOrder.Clear();
                    rightBackOrder.Clear();
                    UpdateUI();
                }

                _initialized = value;
            }
        }
        
        public event Action ActivateNext;
        private void OnActivateNext() => ActivateNext?.Invoke();

        public event Action DeactivateNext;
        private void OnDeactivateNext() => DeactivateNext?.Invoke();

        public event Action<bool> SetEndEarly;
        private void OnSetEndEarly(bool enabled) => SetEndEarly?.Invoke(enabled);

        // Called when the next page needs to be re-initialized
        public event InvalidatePageEventHandler InvalidatePage;
        public void OnInvalidatePage() => InvalidatePage?.Invoke(typeof(DefineMovingPartsPage));
        #endregion

        /// <summary>
        /// refills wheel panels from existing data in the exporter
        /// </summary>
        /// <param name="baseNode"> Top level node in the skeleton that we can read the other nodes off of.</param>
        public void FillFromPreviousSetup(RigidNode_Base baseNode)
        {
            this.DriveTrainDropdown.SelectedIndex = (int)SynthesisGUI.Instance.SkeletonBase.driveTrainType;
            DriveTrainDropdown_SelectedIndexChanged(null, null);

            foreach (RigidNode_Base node in baseNode.ListAllNodes())
            {
                //For the first filter, we take out any nodes that do not have parents and rotational joints.
                if (node.GetParent() != null && node.GetSkeletalJoint() != null &&
                        node.GetSkeletalJoint().GetJointType() == SkeletalJointType.ROTATIONAL && node.GetSkeletalJoint().cDriver != null
                        && (node.GetSkeletalJoint().cDriver.GetInfo(typeof(WheelDriverMeta))) != null)
                {
                    if (((WheelDriverMeta)node.GetSkeletalJoint().cDriver.GetInfo(typeof(WheelDriverMeta))).isDriveWheel)
                    {
                        this.DriveTrainDropdown.SelectedIndex = (int)SynthesisGUI.Instance.SkeletonBase.driveTrainType;
                        switch (((WheelDriverMeta)node.GetSkeletalJoint().cDriver.GetInfo(typeof(WheelDriverMeta))).type)
                        {
                            case WheelType.NORMAL:
                                SetWheelType(node, WizardData.WizardWheelType.NORMAL);
                                break;
                            case WheelType.MECANUM:
                                SetWheelType(node, WizardData.WizardWheelType.MECANUM);
                                break;
                            case WheelType.OMNI:
                                SetWheelType(node, WizardData.WizardWheelType.OMNI);
                                break;

                        }
                        if (node.GetSkeletalJoint().cDriver.port1 == 0)
                        {
                            SetWheelSide(node, WheelSide.RIGHT, true);
                        }
                        else if (node.GetSkeletalJoint().cDriver.port1 == 1)
                        {
                            SetWheelSide(node, WheelSide.LEFT, true);
                        }
                        else if (node.GetSkeletalJoint().cDriver.port1 == 2 && this.DriveTrainDropdown.SelectedIndex == 2)
                        {
                            SetWheelSide(node, WheelSide.MIDDLE, true);
                        }
                        if (((WheelDriverMeta)node.GetSkeletalJoint().cDriver.GetInfo(typeof(WheelDriverMeta))).forwardExtremeValue == 10)
                        {
                            SetWheelFriction(node, WizardData.WizardFrictionLevel.HIGH);
                        }
                        else if ((((WheelDriverMeta)node.GetSkeletalJoint().cDriver.GetInfo(typeof(WheelDriverMeta))).forwardExtremeValue == 7))
                        {
                            SetWheelFriction(node, WizardData.WizardFrictionLevel.MEDIUM);
                        }
                        else
                        {
                            SetWheelFriction(node, WizardData.WizardFrictionLevel.LOW);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Sets the side that a wheel is on. This will update the wizard UI to show the changes.
        /// </summary>
        /// <param name="nodeName">Readable name of the node to set side of.</param>
        /// <param name="side">Side to move node to.</param>
        /// <param name="insertBefore">Node to insert before in layout. Null indicates to add to end of column. Does not apply for unassigned wheels.</param>
        /// <param name="updateUI">Update the UI after the wheel has been moved. Disable this if moving multiple wheels at once.</param>
        public void SetWheelSide(string nodeName, WheelSide side, string insertBefore = null, bool updateUI = true)
        {
            if (nodeName == null)
                return;

            if (!setupPanels.ContainsKey(nodeName))
                return;

            // Remove from current orders
            leftOrder.Remove(nodeName);
            rightOrder.Remove(nodeName);
            middleOrder.Remove(nodeName);
            leftBackOrder.Remove(nodeName);
            rightBackOrder.Remove(nodeName);

            // Update side of wheel data
            setupPanels[nodeName].Side = side;

            // Insert into appropriate order
            if (side == WheelSide.LEFT)
            {
                if (insertBefore == null || !leftOrder.Contains(insertBefore))// determines where in the order the wheel should be placed
                    leftOrder.Add(nodeName);
                else
                    leftOrder.Insert(leftOrder.IndexOf(insertBefore), nodeName);
            }
            else if (side == WheelSide.RIGHT)
            {
                if (insertBefore == null || !rightOrder.Contains(insertBefore))
                    rightOrder.Add(nodeName);
                else
                    rightOrder.Insert(rightOrder.IndexOf(insertBefore), nodeName);
            }
            else if (side == WheelSide.MIDDLE)
            {
                if (insertBefore == null || !middleOrder.Contains(insertBefore))
                    middleOrder.Add(nodeName);
                else
                    middleOrder.Insert(middleOrder.IndexOf(insertBefore), nodeName);
            }
            else if (side == WheelSide.RIGHTBACK)
            {
                if (insertBefore == null || !rightBackOrder.Contains(insertBefore))
                    rightBackOrder.Add(nodeName);
                else
                    rightBackOrder.Insert(rightBackOrder.IndexOf(insertBefore), nodeName);
            }
            else if (side == WheelSide.LEFTBACK)
            {
                if (insertBefore == null || !leftBackOrder.Contains(insertBefore))
                    leftBackOrder.Add(nodeName);
                else
                    leftBackOrder.Insert(leftBackOrder.IndexOf(insertBefore), nodeName);
            }

            // Update the interface
            if (updateUI)
            {
                UpdateUI();
                OnInvalidatePage(); // Reset the next page in the wizard
            }
        }

        /// <summary>
        /// Sets the side that a wheel is on. This will update the wizard UI to show the changes.
        /// </summary>
        /// <param name="node">Node to set side of.</param>
        /// <param name="side">Side to move node to.</param>
        /// <param name="updateUI">Update the UI after the wheel has been moved. Disable this if moving multiple wheels at once.</param>
        public void SetWheelSide(RigidNode_Base node, WheelSide side, bool updateUI = true)
        {
            if (node == null)
                return;

            foreach(KeyValuePair<string, WheelSetupPanel> panel in setupPanels)
            {
                if (panel.Value.Node == node)
                {
                    SetWheelSide(panel.Key, side, null, updateUI);
                    return;
                }
            }
        }

        /// <summary>
        /// Sets the type of a specific wheel. Used in Auto Fill.
        /// </summary>
        /// <param name="node">Wheel to set type of</param>
        /// <param name="type">New wheel type</param>
        public void SetWheelFriction(RigidNode_Base node, WizardData.WizardFrictionLevel level)
        {
            if (node == null)
                return;

            foreach (KeyValuePair<string, WheelSetupPanel> panel in setupPanels)
            {
                if (panel.Value.Node == node)
                {
                    panel.Value.FrictionLevel = level;
                    return;
                }
            }
        }

        /// <summary>
        /// Sets the type of a specific wheel. Used in Auto Fill.
        /// </summary>
        /// <param name="node">Wheel to set type of</param>
        /// <param name="type">New wheel type</param>
        public void SetWheelType(RigidNode_Base node, WizardData.WizardWheelType type)
        {
            if (node == null)
                return;

            foreach (KeyValuePair<string, WheelSetupPanel> panel in setupPanels)
            {
                if (panel.Value.Node == node)
                {
                    panel.Value.WheelType = type;
                    return;
                }
            }
        }

        /// <summary>
        /// Removes a node from any wheel panel it is attached to.
        /// </summary>
        /// <param name="name">Name of the node to remove.</param>
        private void RemoveNodeFromPanel(string name)
        {
            SetWheelSide(name, WheelSide.UNASSIGNED);// remove the wheel by simply setting form none
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
            MiddleWheelsPanel.Controls.Clear();
            MiddleWheelsPanel.RowCount = 0;
            MiddleWheelsPanel.RowStyles.Clear();
            RightBackWheelsPanel.Controls.Clear();
            RightBackWheelsPanel.RowCount = 0;
            RightBackWheelsPanel.RowStyles.Clear();
            LeftBackWheelsPanel.Controls.Clear();
            LeftBackWheelsPanel.RowCount = 0;
            LeftBackWheelsPanel.RowStyles.Clear();

            // Pause layout calculations to prevent siezures
            SuspendLayout();

            // Add left items to left side
            foreach (string name in leftOrder)
                AddControlToNewTableRow(setupPanels[name], LeftWheelsPanel);

            // Add right items to right side
            foreach (string name in rightOrder)
                AddControlToNewTableRow(setupPanels[name], RightWheelsPanel);

            foreach (string name in middleOrder)
                AddControlToNewTableRow(setupPanels[name], MiddleWheelsPanel);

            foreach (string name in rightBackOrder)
                AddControlToNewTableRow(setupPanels[name], RightBackWheelsPanel);

            foreach (string name in leftBackOrder)
                AddControlToNewTableRow(setupPanels[name], LeftBackWheelsPanel);

            // Add all remaining items to list box
            int unassignedNodes = 0;
            foreach (KeyValuePair<string, WheelSetupPanel> slot in setupPanels)
            {
                if (slot.Value.Side == WheelSide.UNASSIGNED)
                {
                    NodeListBox.Items.Add(slot.Key);
                    unassignedNodes++;
                }
            }

            // Shrink items width if a scroll bar will appear
            if (LeftWheelsPanel.PreferredSize.Height < LeftWheelsGroup.Height)
                LeftWheelsPanel.ColumnStyles[1].Width = 0;
            else
                LeftWheelsPanel.ColumnStyles[1].Width = SystemInformation.VerticalScrollBarWidth + 2;

            // Shrink items width if a scroll bar will appear
            if (RightWheelsPanel.PreferredSize.Height < RightWheelsGroup.Height)
                RightWheelsPanel.ColumnStyles[1].Width = 0;
            else
                RightWheelsPanel.ColumnStyles[1].Width = SystemInformation.VerticalScrollBarWidth + 2;
            
            // Shrink items width if a scroll bar will appear
            if (MiddleWheelsPanel.PreferredSize.Height < MiddleWheelsGroup.Height)
                 MiddleWheelsPanel.ColumnStyles[1].Width = 0;
             else
                MiddleWheelsPanel.ColumnStyles[1].Width = SystemInformation.VerticalScrollBarWidth + 2;

            // Shrink items width if a scroll bar will appear
            if (RightWheelsPanel.PreferredSize.Height < RightBackWheelsGroup.Height)
                RightWheelsPanel.ColumnStyles[1].Width = 0;
            else
                RightWheelsPanel.ColumnStyles[1].Width = SystemInformation.VerticalScrollBarWidth + 2;

            // Shrink items width if a scroll bar will appear
            if (LeftWheelsPanel.PreferredSize.Height < LeftBackWheelsGroup.Height)
                LeftWheelsPanel.ColumnStyles[1].Width = 0;
            else
                LeftWheelsPanel.ColumnStyles[1].Width = SystemInformation.VerticalScrollBarWidth + 2;

            OnSetEndEarly(leftOrder.Count + rightOrder.Count + middleOrder.Count + rightBackOrder.Count + leftBackOrder.Count >= numberOfJoints); // Skip next page if no parts are left

            // Resume layout calculations
            ResumeLayout();
        }

        /// <summary>
        /// Makes sure nobody did something stupid and modified the side variable of a wheel setup panel without moving it to the correct list.
        /// </summary>
        private void ValidateLeftRightLists()
        {
            // Make sure all items in left orders are left wheels
            int n = 0;
            while (n < leftOrder.Count)
            {
                if (setupPanels[leftOrder[n]].Side != WheelSide.LEFT)
                    leftOrder.RemoveAt(n);
                else
                    n++;
            }

            // Make sure all items in right orders are right wheels
            n = 0;
            while (n < rightOrder.Count)
            {
                if (setupPanels[rightOrder[n]].Side != WheelSide.RIGHT)
                    rightOrder.RemoveAt(n);
                else
                    n++;
            }

            // Make sure all items in middle orders are middle wheels
            n = 0;
            while (n < middleOrder.Count)
            {
                if (setupPanels[middleOrder[n]].Side != WheelSide.MIDDLE)
                    middleOrder.RemoveAt(n);
                else
                    n++;
            }

            // Make sure all items in right back orders are right back wheels
            n = 0;
            while (n < rightBackOrder.Count)
            {
                if (setupPanels[rightBackOrder[n]].Side != WheelSide.RIGHTBACK)
                    rightBackOrder.RemoveAt(n);
                else
                    n++;
            }

            // Make sure all items in left back orders are left back wheels
            n = 0;
            while (n < leftBackOrder.Count)
            {
                if (setupPanels[leftBackOrder[n]].Side != WheelSide.LEFTBACK)
                    leftBackOrder.RemoveAt(n);
                else
                    n++;
            }

            foreach (KeyValuePair<string, WheelSetupPanel> panel in setupPanels)
            {
                // If it should exist in the left order, add it to the left order
                if (panel.Value.Side == WheelSide.LEFT)
                    if (!leftOrder.Contains(panel.Key))
                        leftOrder.Add(panel.Key);

                // If it should exist in the right order, add it to the right order
                if (panel.Value.Side == WheelSide.RIGHT)
                    if (!rightOrder.Contains(panel.Key))
                        rightOrder.Add(panel.Key);

                // If it should exist in the middle order, add it to the right order
                if (panel.Value.Side == WheelSide.MIDDLE)
                    if (!middleOrder.Contains(panel.Key))
                        middleOrder.Add(panel.Key);

                // If it should exist in the left order, add it to the left order
                if (panel.Value.Side == WheelSide.LEFTBACK)
                    if (!leftBackOrder.Contains(panel.Key))
                        leftBackOrder.Add(panel.Key);

                // If it should exist in the right order, add it to the right order
                if (panel.Value.Side == WheelSide.RIGHTBACK)
                    if (!rightBackOrder.Contains(panel.Key))
                        rightBackOrder.Add(panel.Key);

            }
        }

        /// <summary>
        /// Updates the calculated weight info.
        /// </summary>
        private void UpdateWeight()
        {
            if (WeightUnitSelector.SelectedIndex == 0)
                totalWeightKg = (float)WeightBox.Value / 2.20462f;
            else
                totalWeightKg = (float)WeightBox.Value;

            preferMetric = WeightUnitSelector.SelectedIndex == 1;
        }

        /// <summary>
        /// Sets the value of the weight box. This should be used to prevent going over the max/below the min.
        /// </summary>
        /// <param name="value"></param>
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
                InventorUtils.FocusAndHighlightNode(setupPanels[NodeListBox.SelectedItem.ToString()].Node, StandardAddInServer.Instance.MainApplication.ActiveView.Camera, 0.8);
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

            if (!setupPanels.ContainsKey(name))
                return;

            // Highlight node in Inventor
            InventorUtils.FocusAndHighlightNode(setupPanels[name].Node, StandardAddInServer.Instance.MainApplication.ActiveView.Camera, 0.8);
            // Start drag-and-drop process
            NodeListBox.DoDragDrop(name, DragDropEffects.Move);
        }

        /// <summary>
        /// Called when the user moves the mouse over a wheel panel while dragging.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Field_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;// allows the user to drop the panel
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

                WheelsPanel_DragDrop(nodeName, WheelSide.LEFT);
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

                WheelsPanel_DragDrop(nodeName, WheelSide.RIGHT);
            }
        }

        /// <summary>
        /// Called when the user drops a dragged item into the middle wheel panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MiddleWheelsPanel_DragDrop(object sender, DragEventArgs e)// called when the user "drops" a seleected value into the group
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string nodeName = (string)e.Data.GetData(DataFormats.StringFormat, true);

                WheelsPanel_DragDrop(nodeName, WheelSide.MIDDLE);
            }
        }

        /// <summary>
        /// Called when the user drops a dragged item into the middle wheel panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RightBackWheelsPanel_DragDrop(object sender, DragEventArgs e)// called when the user "drops" a seleected value into the group
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string nodeName = (string)e.Data.GetData(DataFormats.StringFormat, true);

                WheelsPanel_DragDrop(nodeName, WheelSide.RIGHTBACK);
            }
        }

        /// <summary>
        /// Called when the user drops a dragged item into the middle wheel panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeftBackWheelsPanel_DragDrop(object sender, DragEventArgs e)// called when the user "drops" a seleected value into the group
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string nodeName = (string)e.Data.GetData(DataFormats.StringFormat, true);

                WheelsPanel_DragDrop(nodeName, WheelSide.LEFTBACK);
            }
        }

        /// <summary>
        /// Called when a dragged part is placed in either wheel panel.
        /// </summary>
        /// <param name="nodeName">Name of the node being dragged.</param>
        /// <param name="side">Side that the node was placed in.</param>
        private void WheelsPanel_DragDrop(string nodeName, WheelSide side)// called when the user "drops" a seleected value into the group
        {
            TableLayoutPanel wheelPanel = null;
            if (side == WheelSide.LEFT)
            {
                wheelPanel = LeftWheelsPanel;
            } else if (side == WheelSide.RIGHT)
            {
                wheelPanel = RightWheelsPanel;
            }
            else if (side == WheelSide.MIDDLE)
            {
                wheelPanel = MiddleWheelsPanel;
            }
            else if (side == WheelSide.RIGHTBACK)
            {
                wheelPanel = RightBackWheelsPanel;
            }
            else if (side == WheelSide.LEFTBACK)
            {
                wheelPanel = LeftBackWheelsPanel;
            }

            // Find the wheel control located below the mouse and insert before that node
            foreach (Control c in wheelPanel.Controls)
            {
                if (c.PointToClient(MousePosition).Y < c.Height / 2) // Check if mouse is above the center of the control
                {
                    if (c is WheelSetupPanel)
                    {
                        SetWheelSide(nodeName, side, ((WheelSetupPanel)c).NodeName); // Insert above the control
                        return;
                    }
                }
            }

            // Mouse is below all other nodes, place at end
            SetWheelSide(nodeName, side);
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
                
                // Don't do drag action if the item is already in the nodeListBox. This allows user to click on items without updating the UI.
                if (setupPanels[nodeName].Side != WheelSide.UNASSIGNED) 
                    SetWheelSide(nodeName, WheelSide.UNASSIGNED);
            }
        }

        /// <summary>
        /// Adds a control to a new row at the end of the table.
        /// </summary>
        /// <param name="control">Control to append to table.</param>
        /// <param name="table">Table to add control to.</param>
        /// <param name="rowStyle">Style of the new row. Autosized if left null.</param>
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

        /// <summary>
        /// Called when the user selects a new part in the list box at the top of the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NodeListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (NodeListBox.SelectedItem != null)
            {
                // Highlight node in Inventor
                InventorUtils.FocusAndHighlightNode(setupPanels[NodeListBox.SelectedItem.ToString()].Node, StandardAddInServer.Instance.MainApplication.ActiveView.Camera, 0.8);
            }
        }
        /// <summary>
        /// attemps to fill all wheels into the correct side group
        /// </summary>
        private void AutoFill_Click(Object sender, EventArgs e) // Initializes autofill process
        {
            if (Utilities.GUI.SkeletonBase != null || Utilities.GUI.LoadRobotSkeleton()) // Load the robot skeleton
            {
                if (SynthesisGUI.Instance.SkeletonBase.driveTrainType == RigidNode_Base.DriveTrainType.H_DRIVE)
                {
                    if (WizardUtilities.DetectWheels(Utilities.GUI.SkeletonBase, out List<RigidNode_Base> leftWheels, out List<RigidNode_Base> rightWheels, out List<RigidNode_Base> middleWheels)) //finds wheels
                    {
                        foreach (RigidNode_Base wheel in leftWheels)
                            SetWheelSide(wheel, WheelSide.LEFT, false);

                        foreach (RigidNode_Base wheel in rightWheels)
                            SetWheelSide(wheel, WheelSide.RIGHT, false);

                        foreach (RigidNode_Base wheel in middleWheels)
                            SetWheelSide(wheel, WheelSide.MIDDLE, false);
                    }
                } else
                {
                    if (WizardUtilities.DetectWheels(Utilities.GUI.SkeletonBase, out List<RigidNode_Base> leftWheels, out List<RigidNode_Base> rightWheels)) //finds wheels
                    {
                        foreach (RigidNode_Base wheel in leftWheels)
                            SetWheelSide(wheel, WheelSide.LEFT, false);

                        foreach (RigidNode_Base wheel in rightWheels)
                            SetWheelSide(wheel, WheelSide.RIGHT, false);
                    }
                }
            }

            // Refresh the UI with new wheel information
            UpdateUI();
        }
        /// <summary>
        ///Remove all wheels form the form
        /// </summary>
        private void RemoveWheelsButton_Click(object sender, EventArgs e)
        {
            foreach (string name in leftOrder.ToList())
                RemoveNodeFromPanel(name);
            
            foreach (string name in rightOrder.ToList())
                RemoveNodeFromPanel(name);

            foreach (string name in middleOrder.ToList())
                RemoveNodeFromPanel(name);

            foreach (string name in rightBackOrder.ToList())
                RemoveNodeFromPanel(name);

            foreach (string name in leftBackOrder.ToList())
                RemoveNodeFromPanel(name);
                
        }
    }
}