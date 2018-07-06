using System;
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
        private Dictionary<string, RigidNode_Base> listItems = new Dictionary<string, RigidNode_Base>();

        /// <summary>
        /// List of all the <see cref="WheelSlotPanel"/>s in this page. Referenced in <see cref="GetNextEmptyPanel()"/>
        /// </summary>
        private List<WheelSlotPanel> rightSlots = new List<WheelSlotPanel>();
        private List<WheelSlotPanel> leftSlots = new List<WheelSlotPanel>();

        public DefineWheelsPage()
        {
            WheelSetupPanel.remove += new OnWheelSetupPanelRemove(this.RemoveWheelSetupPanel);
            WheelSetupPanel.hover += new OnWheelSetupPanelHover(this.WheelSetupHover);
            InitializeComponent();
            RightWheelsPanel.AllowDrop = true;
            LeftWheelsPanel.AllowDrop = true;
            NodeListBox.AllowDrop = true;

            DriveTrainDropdown.SelectedIndex = 0;
            
            rightSlots = new List<WheelSlotPanel>();
            leftSlots = new List<WheelSlotPanel>();

            NodeListBox.Enabled = false;

            // Load weight information
            preferMetric = Utilities.GUI.RMeta.PreferMetric;
            SetWeightBoxValue(Utilities.GUI.RMeta.TotalWeightKg * (preferMetric ? 1 : 2.20462f));
            WeightUnitSelector.SelectedIndex = Utilities.GUI.RMeta.PreferMetric ? 1 : 0;

            Initialize();
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
                    NodeListBox.Enabled = false;
                    break;
                case 1: //Tank
                    WizardData.Instance.driveTrain = WizardData.WizardDriveTrain.TANK;
                    NodeListBox.Enabled = true;
                    break;
                case 2: //Mecanum
                    WizardData.Instance.driveTrain = WizardData.WizardDriveTrain.MECANUM;
                    NodeListBox.Enabled = true;
                    break;
                case 3: //Swerve
                    WizardData.Instance.driveTrain = WizardData.WizardDriveTrain.SWERVE;
                    NodeListBox.Enabled = true;
                    break;
                case 4: //H-Drive
                    WizardData.Instance.driveTrain = WizardData.WizardDriveTrain.H_DRIVE;
                    NodeListBox.Enabled = true;
                    break;
                case 5: //Custom
                    WizardData.Instance.driveTrain = WizardData.WizardDriveTrain.CUSTOM;
                    NodeListBox.Enabled = true;
                    break;
            }
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
            foreach(var slot in rightSlots)
            {
                WizardData.Instance.wheels.Add(slot.WheelData);
                WizardData.WheelSetupData wheel = slot.WheelData;
            }
            foreach (var slot in leftSlots)
            {
                WizardData.Instance.wheels.Add(slot.WheelData);
                WizardData.WheelSetupData wheel = slot.WheelData;
            }
        }

        /// <summary>
        /// Adds as many <see cref="WheelSlotPanel"/>s as there are wheels
        /// </summary>
        public void Initialize()
        {
            if (WizardData.Instance.driveTrain != WizardData.WizardDriveTrain.SWERVE)
            {
                foreach (RigidNode_Base node in Utilities.GUI.SkeletonBase.ListAllNodes())
                {
                    if (node.GetSkeletalJoint() != null && node.GetSkeletalJoint().GetJointType() == SkeletalJointType.ROTATIONAL)
                    {
                        string readableName = node.ModelFileName.Replace('_', ' ').Replace(".bxda", "");
                        readableName = readableName.Substring(0, 1).ToUpperInvariant() + readableName.Substring(1); // Capitalize first character
                        NodeListBox.Items.Add(readableName);
                        listItems.Add(readableName, node);
                    }
                }
            }
            else
            {
                foreach (RigidNode_Base node in Utilities.GUI.SkeletonBase.ListAllNodes())
                {
                    if (node.GetParent().GetParent() != null)
                    {
                        string readableName = node.ModelFileName.Replace('_', ' ').Replace(".bxda", "");
                        readableName = readableName.Substring(0, 1).ToUpperInvariant() + readableName.Substring(1); // Capitalize first character
                        NodeListBox.Items.Add(readableName);
                        listItems.Add(readableName, node);
                    }
                }
            }

            leftSlots = new List<WheelSlotPanel>();
            rightSlots = new List<WheelSlotPanel>();
            for (int i = 0; i < WizardData.Instance.wheelCount/2; i++)
            {
                WheelSlotPanel leftPanel = new WheelSlotPanel();
                leftPanel.WheelTypeChanged += Panel_WheelTypeChanged;
                leftSlots.Add(leftPanel);
                LeftWheelsPanel.Controls.Add(leftPanel);

                WheelSlotPanel rightPanel = new WheelSlotPanel();
                rightPanel.WheelTypeChanged += Panel_WheelTypeChanged;
                rightSlots.Add(rightPanel);
                RightWheelsPanel.Controls.Add(rightPanel);
            }
            _initialized = true;
        }

        public void UpdateWheelPanes()
        {
             if (leftSlots.Count > WizardData.Instance.wheelCount)
            {
                int downTo = leftSlots.Count;
                for (int i = downTo - 1; i > WizardData.Instance.wheelCount / 2 - 1; i--)
                {
                    LeftWheelsPanel.Controls.Remove(leftSlots.ElementAt(i));
                    leftSlots.Remove(leftSlots.ElementAt(i));
                }
            }
           
            if (rightSlots.Count > WizardData.Instance.wheelCount)
            {
                int downTo = rightSlots.Count;
                for (int i = downTo - 1; i > WizardData.Instance.wheelCount / 2 - 1; i--)
                {
                    RightWheelsPanel.Controls.Remove(rightSlots.ElementAt(i));
                    rightSlots.Remove(rightSlots.ElementAt(i));
                }
            }
            _initialized = true;

        }
        
        public event Action ActivateNext;
        private void OnActivateNext()
        {
            this.ActivateNext?.Invoke();
        }

        public event Action DeactivateNext;
        private void OnDeactivateNext()
        {
            this.DeactivateNext?.Invoke();
        }

        public event InvalidatePageEventHandler InvalidatePage;
        public void OnInvalidatePage()
        {
            InvalidatePage?.Invoke(typeof(DefineMovingPartsPage));
        }
      
        public bool Initialized
        {
            get => _initialized;
            set
            {
                if (!value)
                {
                    while (LeftWheelsPanel.Controls.Count > 0)
                        LeftWheelsPanel.Controls[0].Dispose(); 
                }
                _initialized = value;
            }
        }
        private bool _initialized = false;
        #endregion

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

        private void NodeListBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                StandardAddInServer.Instance.WizardSelect(listItems[NodeListBox.SelectedItem.ToString()]);
                NodeListBox.DoDragDrop(NodeListBox.SelectedItem.ToString() + " From Node Box", DragDropEffects.Copy |
                            DragDropEffects.Move);
            }
            catch (Exception) { }
        }

        private void RightWheelsPanel_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void RightWheelsPanel_DragDrop(object sender, DragEventArgs e)
        {
            if (((String)e.Data.GetData(DataFormats.StringFormat, true)).Contains(" From Node Box"))// checks the origin of the drag and drop, checks if the drag is from the node list box
            {
                String toFind = ((String)e.Data.GetData(DataFormats.StringFormat, true)).Substring(0,
                    ((String)e.Data.GetData(DataFormats.StringFormat, true)).IndexOf(" From Node Box"));// isolate the name of the drag and drop object
                OnInvalidatePage();// not gonna lie. I've got no clue what this does. But everything else has it here so I assume its important. maybe not tho
                WheelSlotPanel panel = new WheelSlotPanel();// preps a wheel panel to place a wheel setup into
                panel.WheelTypeChanged += Panel_WheelTypeChanged;// ^^^^
                foreach (WheelSlotPanel wheel in RightWheelsPanel.Controls)// iterates over the existing wheel nodes to find if a node is below the cursor to add the wheel into that pos
                {
                    if ((wheel.PointToScreen(Point.Empty).Y > System.Windows.Forms.Control.MousePosition.Y)) // checks if the node is above or below the cursor
                    {
                        Control[] tempControls = new Control[RightWheelsPanel.Controls.Count - RightWheelsPanel.Controls.IndexOf(wheel)]; // makes a temp array for the controls below the cursor
                        List<WheelSlotPanel> tempSlots = new List<WheelSlotPanel>();//                                                         ^^
                        int pos = 0;// tracks the position of the temp array
                        for (int i = RightWheelsPanel.Controls.IndexOf(wheel); i < RightWheelsPanel.Controls.Count;)// iterates over the controls until there are only the controls above the cursor still exiting
                        {
                            tempControls[pos] = RightWheelsPanel.Controls[i];// copies the existing controls into a temp array
                            tempSlots.Add(rightSlots[i]);//                                  ^^^
                            pos++;// increase pos for the next loop thru
                            RightWheelsPanel.Controls.RemoveAt(i); // removes the control form the actual array so we can add the control into the right place
                            rightSlots.RemoveAt(i);//                  ^^
                        }

                        rightSlots.Add(panel);// adds a new panel
                        RightWheelsPanel.Controls.Add(panel);// ^^
                        switch (WizardData.Instance.driveTrain)// initializes the panel with the correct settings for the selected drive type
                        {
                            case WizardData.WizardDriveTrain.TANK:
                                panel.FillSlot(listItems[toFind], toFind, true);
                                break;
                            case WizardData.WizardDriveTrain.MECANUM:
                                panel.FillSlot(listItems[toFind], toFind, true, WizardData.WizardWheelType.MECANUM);
                                break;
                            case WizardData.WizardDriveTrain.H_DRIVE:
                                panel.FillSlot(listItems[toFind], toFind, true, WizardData.WizardWheelType.OMNI);
                                break;
                            case WizardData.WizardDriveTrain.SWERVE:
                                //TODO implement this
                                panel.FillSlot(listItems[toFind], toFind, true);
                                break;
                            case WizardData.WizardDriveTrain.CUSTOM:
                                panel.FillSlot(listItems[toFind], toFind, true);
                                break;
                        }
                        RightWheelsPanel.Controls.AddRange(tempControls);// add the temp controls back into the array
                        rightSlots.AddRange(tempSlots);//                               ^^
                        NodeListBox.Items.Remove(toFind);// removes the corresponding from the list of nodes 
                        return; // quites the method
                    }
                }
                rightSlots.Add(panel); // adds the panel into the end of the list
                RightWheelsPanel.Controls.Add(panel);// ^^^
                switch (WizardData.Instance.driveTrain)// initializes the panel with the correct settings for the selected drive type
                {
                    case WizardData.WizardDriveTrain.TANK:
                        panel.FillSlot(listItems[toFind], toFind, true);
                        break;
                    case WizardData.WizardDriveTrain.MECANUM:
                        panel.FillSlot(listItems[toFind], toFind, true, WizardData.WizardWheelType.MECANUM);
                        break;
                    case WizardData.WizardDriveTrain.H_DRIVE:
                        panel.FillSlot(listItems[toFind], toFind, true, WizardData.WizardWheelType.OMNI);
                        break;
                    case WizardData.WizardDriveTrain.SWERVE:
                        //TODO implement this
                        panel.FillSlot(listItems[toFind], toFind, true);
                        break;
                    case WizardData.WizardDriveTrain.CUSTOM:
                        panel.FillSlot(listItems[toFind], toFind, true);
                        break;
                }
                NodeListBox.Items.Remove(toFind);// removes the corresponding from the list of nodes 
            }
            else if (((String)e.Data.GetData(DataFormats.StringFormat, true)).Contains(" From Node Group")) // if the drag is from an existing panel
            {
                String toFind = ((String)e.Data.GetData(DataFormats.StringFormat, true)).Substring(0, ((String)e.Data.GetData(DataFormats.StringFormat, true)).IndexOf(" From Node Group"));
                OnInvalidatePage();
                WheelSlotPanel panel = null;
                foreach (WheelSlotPanel wantedPanel in leftSlots)
                {
                    panel = wantedPanel;
                    if (panel.name.Equals(toFind))
                    {
                        foreach (WheelSlotPanel wheel in RightWheelsPanel.Controls)// iterates over the existing wheel nodes to find if a node is below the cursor to add the wheel into that pos
                        {
                            if ((wheel.PointToScreen(Point.Empty).Y > System.Windows.Forms.Control.MousePosition.Y)) // checks if the node is above or below the cursor
                            {
                                if (panel.Equals(wheel))
                                {
                                    return;
                                }
                                foreach (WheelSlotPanel toRemove in LeftWheelsPanel.Controls)
                                {
                                    if (toRemove.name.Equals(toFind))
                                    {
                                        LeftWheelsPanel.Controls.Remove(toRemove);
                                    }
                                }

                                leftSlots.Remove(panel);
                                Control[] tempControls = new Control[RightWheelsPanel.Controls.Count - RightWheelsPanel.Controls.IndexOf(wheel)]; // makes a temp array for the controls below the cursor
                                List<WheelSlotPanel> tempSlots = new List<WheelSlotPanel>();//                                                         ^^
                                int pos = 0;// tracks the position of the temp array
                                for (int i = RightWheelsPanel.Controls.IndexOf(wheel); i < RightWheelsPanel.Controls.Count;)// iterates over the controls until there are only the controls above the cursor still exiting
                                {
                                    tempControls[pos] = RightWheelsPanel.Controls[i];// copies the existing controls into a temp array
                                    tempSlots.Add(rightSlots[i]);//                                  ^^^
                                    pos++;// increase pos for the next loop thru
                                    RightWheelsPanel.Controls.RemoveAt(i); // removes the control form the actual array so we can add the control into the right place
                                    rightSlots.RemoveAt(i);//                  ^^
                                }
                                rightSlots.Add(panel);//    adds a new panel
                                RightWheelsPanel.Controls.Add(panel);// ^^
                                RightWheelsPanel.Controls.AddRange(tempControls);// add the temp controls back into the array
                                rightSlots.AddRange(tempSlots);//                               ^^
                                return; // quites the method
                            }
                        }
                        foreach (WheelSlotPanel toRemove in LeftWheelsPanel.Controls)
                        {
                            if (toRemove.name.Equals(toFind))
                            {
                                LeftWheelsPanel.Controls.Remove(toRemove);
                            }
                        }

                        leftSlots.Remove(panel);
                        rightSlots.Add(panel);//    adds a new panel
                        RightWheelsPanel.Controls.Add(panel);// ^^
                        return; // quites the method
                    }
                }
                foreach (WheelSlotPanel wantedPanel in rightSlots.ToList())
                {
                    panel = wantedPanel;
                    if (panel.name.Equals(toFind))
                    {
                        foreach (WheelSlotPanel wheel in RightWheelsPanel.Controls)// iterates over the existing wheel nodes to find if a node is below the cursor to add the wheel into that pos
                        {
                            if ((wheel.PointToScreen(Point.Empty).Y > System.Windows.Forms.Control.MousePosition.Y)) // checks if the node is above or below the cursor
                            {
                                if (panel.Equals(wheel))
                                {
                                    return;
                                }
                                foreach (WheelSlotPanel toRemove in RightWheelsPanel.Controls)
                                {
                                    if (toRemove.name.Equals(toFind))
                                    {
                                        RightWheelsPanel.Controls.Remove(toRemove);
                                    }
                                }

                                rightSlots.Remove(panel);
                                Control[] tempControls = new Control[RightWheelsPanel.Controls.Count - RightWheelsPanel.Controls.IndexOf(wheel)]; // makes a temp array for the controls below the cursor
                                List<WheelSlotPanel> tempSlots = new List<WheelSlotPanel>();//                                                         ^^
                                int pos = 0;// tracks the position of the temp array
                                for (int i = RightWheelsPanel.Controls.IndexOf(wheel); i < RightWheelsPanel.Controls.Count;)// iterates over the controls until there are only the controls above the cursor still exiting
                                {
                                    tempControls[pos] = RightWheelsPanel.Controls[i];// copies the existing controls into a temp array
                                    tempSlots.Add(rightSlots[i]);//                                  ^^^
                                    pos++;// increase pos for the next loop thru
                                    RightWheelsPanel.Controls.RemoveAt(i); // removes the control form the actual array so we can add the control into the right place
                                    rightSlots.RemoveAt(i);//                  ^^
                                }
                                rightSlots.Add(panel);//    adds a new panel
                                RightWheelsPanel.Controls.Add(panel);// ^^
                                RightWheelsPanel.Controls.AddRange(tempControls);// add the temp controls back into the array
                                rightSlots.AddRange(tempSlots);//                               ^^
                                return; // quites the method
                            }
                        }
                        foreach (WheelSlotPanel toRemove in RightWheelsPanel.Controls)
                        {
                            if (toRemove.name.Equals(toFind))
                            {
                                RightWheelsPanel.Controls.Remove(toRemove);
                            }
                        }

                        rightSlots.Remove(panel);
                        rightSlots.Add(panel);//    adds a new panel
                        RightWheelsPanel.Controls.Add(panel);// ^^
                        return; // quites the method
                    }
                }
            }
        }

        private void LeftWheelsPanel_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }
       
        private void LeftWheelsPanel_DragDrop(object sender, DragEventArgs e)
        {
            if (((String)e.Data.GetData(DataFormats.StringFormat, true)).Contains(" From Node Box"))// checks the origin of the drag and drop, checks if the drag is from the node list box
            {
                String toFind = ((String)e.Data.GetData(DataFormats.StringFormat, true)).Substring(0, 
                    ((String)e.Data.GetData(DataFormats.StringFormat, true)).IndexOf(" From Node Box"));// isolate the name of the drag and drop object
                OnInvalidatePage();// not gonna lie. I've got no clue what this does. But everything else has it here so I assume its important. maybe not tho
                WheelSlotPanel panel = new WheelSlotPanel();// preps a wheel panel to place a wheel setup into
                panel.WheelTypeChanged += Panel_WheelTypeChanged;// ^^^^
                foreach (WheelSlotPanel wheel in LeftWheelsPanel.Controls)// iterates over the existing wheel nodes to find if a node is below the cursor to add the wheel into that pos
                {
                    if ((wheel.PointToScreen(Point.Empty).Y > System.Windows.Forms.Control.MousePosition.Y)) // checks if the node is above or below the cursor
                    {
                        Control[] tempControls = new Control[LeftWheelsPanel.Controls.Count - LeftWheelsPanel.Controls.IndexOf(wheel)]; // makes a temp array for the controls below the cursor
                        List<WheelSlotPanel> tempSlots = new List<WheelSlotPanel>();//                                                         ^^
                        int pos = 0;// tracks the position of the temp array
                        for(int i = LeftWheelsPanel.Controls.IndexOf(wheel); i < LeftWheelsPanel.Controls.Count;)// iterates over the controls until there are only the controls above the cursor still exiting
                        {
                            tempControls[pos] = LeftWheelsPanel.Controls[i];// copies the existing controls into a temp array
                            tempSlots.Add(leftSlots[i]);//                                  ^^^
                            pos++;// increase pos for the next loop thru
                            LeftWheelsPanel.Controls.RemoveAt(i); // removes the control form the actual array so we can add the control into the right place
                            leftSlots.RemoveAt(i);//                  ^^
                        }
                        
                        leftSlots.Add(panel);// adds a new panel
                        LeftWheelsPanel.Controls.Add(panel);// ^^
                        switch (WizardData.Instance.driveTrain)// initializes the panel with the correct settings for the selected drive type
                        {
                            case WizardData.WizardDriveTrain.TANK:
                                panel.FillSlot(listItems[toFind], toFind, true);
                                break;
                            case WizardData.WizardDriveTrain.MECANUM:
                                panel.FillSlot(listItems[toFind], toFind, true, WizardData.WizardWheelType.MECANUM);
                                break;
                            case WizardData.WizardDriveTrain.H_DRIVE:
                                panel.FillSlot(listItems[toFind], toFind, true, WizardData.WizardWheelType.OMNI);
                                break;
                            case WizardData.WizardDriveTrain.SWERVE:
                                //TODO implement this
                                panel.FillSlot(listItems[toFind], toFind, true);
                                break;
                            case WizardData.WizardDriveTrain.CUSTOM:
                                panel.FillSlot(listItems[toFind], toFind, true);
                                break;
                        }
                        LeftWheelsPanel.Controls.AddRange(tempControls);// add the temp controls back into the array
                        leftSlots.AddRange(tempSlots);//                               ^^
                        NodeListBox.Items.Remove(toFind);// removes the corresponding from the list of nodes

                       // MessageBox.Show("new panel " + panel.PointToScreen(Point.Empty).Y + " old panel " + wheel.PointToScreen(Point.Empty).Y);
                        return; // quites the method
                    }
                }
                leftSlots.Add(panel); // adds the panel into the end of the list
                LeftWheelsPanel.Controls.Add(panel);// ^^^
                switch (WizardData.Instance.driveTrain)// initializes the panel with the correct settings for the selected drive type
                {
                    case WizardData.WizardDriveTrain.TANK:
                        panel.FillSlot(listItems[toFind], toFind, true);
                        break;
                    case WizardData.WizardDriveTrain.MECANUM:
                        panel.FillSlot(listItems[toFind], toFind, true, WizardData.WizardWheelType.MECANUM);
                        break;
                    case WizardData.WizardDriveTrain.H_DRIVE:
                        panel.FillSlot(listItems[toFind], toFind, true, WizardData.WizardWheelType.OMNI);
                        break;
                    case WizardData.WizardDriveTrain.SWERVE:
                        //TODO implement this
                        panel.FillSlot(listItems[toFind], toFind, true);
                        break;
                    case WizardData.WizardDriveTrain.CUSTOM:
                        panel.FillSlot(listItems[toFind], toFind, true);
                        break;
                }
                NodeListBox.Items.Remove(toFind);// removes the corresponding from the list of nodes 
            }
            else if (((String)e.Data.GetData(DataFormats.StringFormat, true)).Contains(" From Node Group")) // if the drag is from an existing panel
            {
                String toFind = ((String)e.Data.GetData(DataFormats.StringFormat, true)).Substring(0, ((String)e.Data.GetData(DataFormats.StringFormat, true)).IndexOf(" From Node Group"));
                OnInvalidatePage();
                WheelSlotPanel panel = null;
                foreach (WheelSlotPanel wantedPanel in rightSlots)
                {
                    panel = wantedPanel;
                    if (panel.name.Equals(toFind))
                    {
                        foreach (WheelSlotPanel wheel in LeftWheelsPanel.Controls)// iterates over the existing wheel nodes to find if a node is below the cursor to add the wheel into that pos
                        {
                            if ((wheel.PointToScreen(Point.Empty).Y > System.Windows.Forms.Control.MousePosition.Y)) // checks if the node is above or below the cursor
                            {
                                if (panel.Equals(wheel))
                                {
                                    return;
                                }
                                foreach (WheelSlotPanel toRemove in RightWheelsPanel.Controls)
                                {
                                    if (toRemove.name.Equals(toFind))
                                    {
                                        RightWheelsPanel.Controls.Remove(toRemove);
                                    }
                                }

                                rightSlots.Remove(panel);
                                Control[] tempControls = new Control[LeftWheelsPanel.Controls.Count - LeftWheelsPanel.Controls.IndexOf(wheel)]; // makes a temp array for the controls below the cursor
                                List<WheelSlotPanel> tempSlots = new List<WheelSlotPanel>();//                                                         ^^
                                int pos = 0;// tracks the position of the temp array
                                for (int i = LeftWheelsPanel.Controls.IndexOf(wheel); i < LeftWheelsPanel.Controls.Count;)// iterates over the controls until there are only the controls above the cursor still exiting
                                {
                                    tempControls[pos] = LeftWheelsPanel.Controls[i];// copies the existing controls into a temp array
                                    tempSlots.Add(leftSlots[i]);//                                  ^^^
                                    pos++;// increase pos for the next loop thru
                                    LeftWheelsPanel.Controls.RemoveAt(i); // removes the control form the actual array so we can add the control into the right place
                                    leftSlots.RemoveAt(i);//                  ^^
                                }
                                leftSlots.Add(panel);//    adds a new panel
                                LeftWheelsPanel.Controls.Add(panel);// ^^
                                LeftWheelsPanel.Controls.AddRange(tempControls);// add the temp controls back into the array
                                leftSlots.AddRange(tempSlots);//                               ^^
                                return; // quites the method
                            }
                        }
                        foreach (WheelSlotPanel toRemove in RightWheelsPanel.Controls)
                        {
                            if (toRemove.name.Equals(toFind))
                            {
                                RightWheelsPanel.Controls.Remove(toRemove);
                            }
                        }
                        rightSlots.Remove(panel);
                        leftSlots.Add(panel);//    adds a new panel
                        LeftWheelsPanel.Controls.Add(panel);// ^^
                        return; // quites the method
                    }
                }
                foreach (WheelSlotPanel wantedPanel in leftSlots.ToList())
                {
                    panel = wantedPanel;
                    if (panel.name.Equals(toFind))
                    {
                        foreach (WheelSlotPanel wheel in LeftWheelsPanel.Controls)// iterates over the existing wheel nodes to find if a node is below the cursor to add the wheel into that pos
                        {
                            if ((wheel.PointToScreen(Point.Empty).Y > System.Windows.Forms.Control.MousePosition.Y)) // checks if the node is above or below the cursor
                            {
                                if (panel.Equals(wheel))
                                {
                                    return;
                                }
                                foreach (WheelSlotPanel toRemove in LeftWheelsPanel.Controls)
                                {
                                    if (toRemove.name.Equals(toFind))
                                    {
                                        LeftWheelsPanel.Controls.Remove(toRemove);
                                    }
                                }

                                leftSlots.Remove(panel);
                                Control[] tempControls = new Control[LeftWheelsPanel.Controls.Count - LeftWheelsPanel.Controls.IndexOf(wheel)]; // makes a temp array for the controls below the cursor
                                List<WheelSlotPanel> tempSlots = new List<WheelSlotPanel>();//                                                         ^^
                                int pos = 0;// tracks the position of the temp array
                                for (int i = LeftWheelsPanel.Controls.IndexOf(wheel); i < LeftWheelsPanel.Controls.Count;)// iterates over the controls until there are only the controls above the cursor still exiting
                                {
                                    tempControls[pos] = LeftWheelsPanel.Controls[i];// copies the existing controls into a temp array
                                    tempSlots.Add(leftSlots[i]);//                                  ^^^
                                    pos++;// increase pos for the next loop thru
                                    LeftWheelsPanel.Controls.RemoveAt(i); // removes the control form the actual array so we can add the control into the right place
                                    leftSlots.RemoveAt(i);//                  ^^
                                }
                                leftSlots.Add(panel);//    adds a new panel
                                LeftWheelsPanel.Controls.Add(panel);// ^^
                                LeftWheelsPanel.Controls.AddRange(tempControls);// add the temp controls back into the array
                                leftSlots.AddRange(tempSlots);//                               ^^
                                return; // quites the method
                            }
                        }
                        foreach (WheelSlotPanel toRemove in LeftWheelsPanel.Controls)
                        {
                            if (toRemove.name.Equals(toFind))
                            {
                                LeftWheelsPanel.Controls.Remove(toRemove);
                            }
                        }

                        leftSlots.Remove(panel);
                        leftSlots.Add(panel);//    adds a new panel
                        LeftWheelsPanel.Controls.Add(panel);// ^^
                        return; // quites the method
                    }
                }
            }
        }

        public String RemoveWheelSetupPanel(String s)
        {
            foreach (Object wheel in LeftWheelsPanel.Controls)
            {
                if (wheel.GetType().Equals(typeof(WheelSlotPanel)))
                {
                    if (((WheelSlotPanel)wheel).name.Equals(s))
                    {
                        LeftWheelsPanel.Controls.Remove(((WheelSlotPanel)wheel));
                        leftSlots.Remove(((WheelSlotPanel)wheel));
                    }
                }
            }
            foreach (Object wheel in RightWheelsPanel.Controls)
            {
                if (wheel.GetType().Equals(typeof(WheelSlotPanel)))
                {
                    if (((WheelSlotPanel)wheel).name.Equals(s))
                    {
                        RightWheelsPanel.Controls.Remove(((WheelSlotPanel)wheel));
                        rightSlots.Remove(((WheelSlotPanel)wheel));
                    }
                }
            }
            NodeListBox.Items.Add(s);
            object[] list;
            NodeListBox.Items.CopyTo(list = new object[NodeListBox.Items.Count], 0);
            ArrayList a = new ArrayList();
            a.AddRange(list);
            a.Sort();
            NodeListBox.Items.Clear();
            foreach(object sortedItem in a)
            {
                NodeListBox.Items.Add(sortedItem);
            }
            return "";
        }

        public String WheelSetupHover(String s)
        {

            StandardAddInServer.Instance.WizardSelect(listItems[s]);
            return "";
        }
            
        private void NodeListBox_SelectedIndexChanged(object sender, EventArgs e)
        { 
            try
            {
                StandardAddInServer.Instance.WizardSelect(listItems[NodeListBox.Items[NodeListBox.SelectedIndex].ToString()]);
            }
            catch (Exception) { }
        }

        private void DefineWheelsInstruction1_Click(object sender, EventArgs e)
        {

        }
    }
}