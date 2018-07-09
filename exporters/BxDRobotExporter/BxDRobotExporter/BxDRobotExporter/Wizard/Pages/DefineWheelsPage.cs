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
            WheelSetupPanel.remove += new OnWheelSetupPanelRemove(this.RemoveWheelSetupPanel);// subscribes the removal handler to the removal handler in the setupPanel so we can properly remove the panel
            WheelSetupPanel.hover += new OnWheelSetupPanelHover(this.WheelSetupHover);// subscribes the hover handler to the hover handler in the setupPanel so we can properly select the node in Inventor
            InitializeComponent();
            RightWheelsPanel.AllowDrop = true;// allow the drops in the panel
            LeftWheelsPanel.AllowDrop = true;// allow the drops in the panel
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
                slot.wheelSetupPanel.isRightWheel = true;// tells the backend whether or not to give the wheel a right or left PWM value
                WizardData.Instance.wheels.Add(slot.WheelData);
                WizardData.WheelSetupData wheel = slot.WheelData;
            }
            foreach (var slot in leftSlots)
            {
                slot.wheelSetupPanel.isRightWheel = false;// tells the backend whether or not to give the wheel a right or left PWM value
                WizardData.Instance.wheels.Add(slot.WheelData);
                WizardData.WheelSetupData wheel = slot.WheelData;
            }
        }

        /// <summary>
        /// Adds as many <see cref="WheelSlotPanel"/>s as there are wheels
        /// </summary>
        public void Initialize()
        {
            Dictionary<string, int> duplicatePartNames = new Dictionary<string, int>();

            foreach (RigidNode_Base node in Utilities.GUI.SkeletonBase.ListAllNodes())
            {
                if ((node.GetSkeletalJoint() != null && node.GetSkeletalJoint().GetJointType() == SkeletalJointType.ROTATIONAL) ||
                    (WizardData.Instance.driveTrain == WizardData.WizardDriveTrain.SWERVE && node.GetParent().GetParent() != null))
                {
                    string readableName = node.ModelFileName.Replace('_', ' ').Replace(".bxda", "");
                    readableName = readableName.Substring(0, 1).ToUpperInvariant() + readableName.Substring(1); // Capitalize first character

                    if (listItems.ContainsKey(readableName))
                    {
                        // Add the part name to the list of duplicate parts
                        if (!duplicatePartNames.ContainsKey(node.ModelFileName))
                            duplicatePartNames.Add(node.ModelFileName, 2);

                        // Find the next available name
                        int identNum = duplicatePartNames[node.ModelFileName];
                        while (listItems.ContainsKey(readableName + ' ' + identNum) && identNum <= 100)
                            identNum++;

                        // Add the joint to the list with the new unique name
                        readableName += ' ' + identNum.ToString();

                        // Update the next available ID
                        duplicatePartNames[node.ModelFileName] = identNum;
                    }

                    listItems.Add(readableName, node);
                    NodeListBox.Items.Add(readableName);
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

        private void NodeListBox_MouseDown(object sender, MouseEventArgs e)// catches the mouse down even in the nodelistbox to begin the drag and drop event
        {
            try
            {
                StandardAddInServer.Instance.WizardSelect(listItems[NodeListBox.SelectedItem.ToString()]);// selects/highlights the appriate node in Inventor
                NodeListBox.DoDragDrop(NodeListBox.SelectedItem.ToString() + " From Node Box", DragDropEffects.Copy | DragDropEffects.Move);// activates the drag and drop method with the name of the selected object so the when the user drops the node we can tell which node was dragged
            }
            catch (Exception) { }// catches any freakiness with the user trying to drag with nothing selected
        }

        private void RightWheelsPanel_DragDrop(object sender, DragEventArgs e)// called when the user "drops" a seleected value into the group
        {
            if (((String)e.Data.GetData(DataFormats.StringFormat, true)).Contains(" From Node Box"))// checks the origin of the drag and drop, if the drag is from the node list box
            {
                InitializePanelFormNodeList((String)e.Data.GetData(DataFormats.StringFormat, true), RightWheelsPanel, rightSlots);// calls the initialize method with the correct side values
            }
            else if (((String)e.Data.GetData(DataFormats.StringFormat, true)).Contains(" From Node Group")) // if the drag is from an existing panel
            {
                MovePanelFromGroup((String)e.Data.GetData(DataFormats.StringFormat, true), RightWheelsPanel, rightSlots, LeftWheelsPanel, leftSlots);// calls the method to handle a panel move with an existing panel with the appropriate side values
            }
        }

        private void Panel_DragEnter(object sender, DragEventArgs e)// called when the drag hovers over the panel
        {
            e.Effect = DragDropEffects.Copy;// activates the copy event so the user can drop the node
        }
       
        private void LeftWheelsPanel_DragDrop(object sender, DragEventArgs e)// called when the user "drops" a seleected value into the group
        {
            if (((String)e.Data.GetData(DataFormats.StringFormat, true)).Contains(" From Node Box"))// checks the origin of the drag and drop, checks if the drag is from the node list box
            {
                InitializePanelFormNodeList((String)e.Data.GetData(DataFormats.StringFormat, true), LeftWheelsPanel, leftSlots);// calls the initialize method with the correct side values
            }
            else if (((String)e.Data.GetData(DataFormats.StringFormat, true)).Contains(" From Node Group")) // if the drag is from an existing panel
            {
                MovePanelFromGroup((String)e.Data.GetData(DataFormats.StringFormat, true), LeftWheelsPanel, leftSlots, RightWheelsPanel, rightSlots);// calls the method to handle a panel move with an existing panel with the appropriate side values
            }
        }

        private void MovePanelFromGroup(String nodeName, FlowLayoutPanel placementPanel, List<WheelSlotPanel> placementList, FlowLayoutPanel otherPlacementPanel, List<WheelSlotPanel> otherPlacementList)// handles panel moves with existing panels both within the same group and between the two opposite groups
        {
            String toFind = nodeName.Substring(0, nodeName.IndexOf(" From Node Group"));// confirms that the data is from an existing node
            OnInvalidatePage();// invalidates the page or something... idk lol
            foreach (WheelSlotPanel wantedPanel in otherPlacementList)// iterates over all the panels in the other panel
            {
                if (wantedPanel.name.Equals(toFind))// checks if its the panel we want
                {
                    foreach (WheelSlotPanel wheel in placementPanel.Controls)// iterates over the existing wheel nodes to find if a panel is below the cursor to add the wheel into that pos
                    {
                        if ((wheel.PointToScreen(Point.Empty).Y > System.Windows.Forms.Control.MousePosition.Y)) // checks if the panel is above or below the cursor
                        {
                            if (wantedPanel.Equals(wheel))// if the wanted area already contains the panel
                            {
                                return;// quit
                            }
                            foreach (WheelSlotPanel toRemove in otherPlacementPanel.Controls)// iterates over all the panels to find the one we want to remove
                            {
                                if (toRemove.name.Equals(toFind))// if the panel is the one we want
                                {
                                    otherPlacementPanel.Controls.Remove(toRemove);// removes the panel form the group view
                                    break;
                                }
                            }

                            otherPlacementList.Remove(wantedPanel);// removes the panel from the internal array
                            Control[] tempControls = new Control[placementPanel.Controls.Count - placementPanel.Controls.IndexOf(wheel)]; // makes a temp array for the controls below the cursor
                            List<WheelSlotPanel> tempSlots = new List<WheelSlotPanel>();//                                                         ^^
                            int pos = 0;// tracks the position of the temp array
                            for (int i = placementPanel.Controls.IndexOf(wheel); i < placementPanel.Controls.Count;)// iterates over the controls until there are only the controls above the cursor still exiting
                            {
                                tempControls[pos] = placementPanel.Controls[i];// copies the existing controls into a temp array
                                tempSlots.Add(placementList[i]);//                                  ^^^
                                pos++;// increase pos for the next loop thru
                                placementPanel.Controls.RemoveAt(i); // removes the control form the actual array so we can add the control into the right place
                                placementList.RemoveAt(i);//                  ^^
                            }
                            placementList.Add(wantedPanel);//    adds a new panel
                            placementPanel.Controls.Add(wantedPanel);// ^^
                            placementPanel.Controls.AddRange(tempControls);// add the temp controls back into the array
                            placementList.AddRange(tempSlots);//                               ^^
                            return; // quits the method
                        }
                    }
                    // this gets called if the panel should be placed at the bottom
                    foreach (WheelSlotPanel toRemove in otherPlacementPanel.Controls)// goes over the other panel lists to remove the panel
                    {
                        if (toRemove.name.Equals(toFind))// checks if the node is the one we want
                        {
                            otherPlacementPanel.Controls.Remove(toRemove);// removes the panel from the controls
                            break;
                        }
                    }
                    otherPlacementList.Remove(wantedPanel);// removes the panel from the other internal array
                    placementList.Add(wantedPanel);//    adds a new panel
                    placementPanel.Controls.Add(wantedPanel);// ^^
                    return; // quites the method
                }
            }
            foreach (WheelSlotPanel wantedPanel in placementList.ToList())// grabs a list form the left slot so when we mess with it c# does get annoyed
            {
                if (wantedPanel.name.Equals(toFind))// checks if this panel is the one we want
                {
                    foreach (WheelSlotPanel wheel in placementPanel.Controls)// iterates over the existing wheel nodes to find if a node is below the cursor to add the wheel into that pos
                    {
                        if ((wheel.PointToScreen(Point.Empty).Y > System.Windows.Forms.Control.MousePosition.Y)) // checks if the node is above or below the cursor
                        {
                            if (wantedPanel.Equals(wheel))// if the place where the panel should be put is the panel
                            {
                                return;// quit
                            }
                            foreach (WheelSlotPanel toRemove in placementPanel.Controls)// goes over the existing controll to find the one we want to remove
                            {
                                if (toRemove.name.Equals(toFind))// checks if its the panel we want
                                {
                                    placementPanel.Controls.Remove(toRemove);// removes the pane
                                    break;// ends the loop
                                }
                            }
                            placementList.Remove(wantedPanel);// removes the panel from the internal list
                            Control[] tempControls = new Control[placementPanel.Controls.Count - placementPanel.Controls.IndexOf(wheel)]; // makes a temp array for the controls below the cursor
                            List<WheelSlotPanel> tempSlots = new List<WheelSlotPanel>();//                                                         ^^
                            int pos = 0;// tracks the position of the temp array
                            for (int i = placementPanel.Controls.IndexOf(wheel); i < placementPanel.Controls.Count;)// iterates over the controls until there are only the controls above the cursor still exiting
                            {
                                tempControls[pos] = placementPanel.Controls[i];// copies the existing controls into a temp array
                                tempSlots.Add(placementList[i]);//                                  ^^^
                                pos++;// increase pos for the next loop thru
                                placementPanel.Controls.RemoveAt(i); // removes the control form the actual array so we can add the control into the right place
                                placementList.RemoveAt(i);//                  ^^
                            }
                            placementList.Add(wantedPanel);//    adds a new panel
                            placementPanel.Controls.Add(wantedPanel);// ^^
                            placementPanel.Controls.AddRange(tempControls);// add the temp controls back into the array
                            placementList.AddRange(tempSlots);//                               ^^
                            return; // quites the method
                        }
                    }
                    //this just adds the panel at the bottom
                    foreach (WheelSlotPanel toRemove in placementPanel.Controls) // goes over the panels in this group
                    {
                        if (toRemove.name.Equals(toFind))// checks if its the panel we want 
                        {
                            placementPanel.Controls.Remove(toRemove);// removes the panel from the group
                            break;
                        }
                    }

                    placementList.Remove(wantedPanel); // removes the panel
                    placementList.Add(wantedPanel);//    adds the panel back in
                    placementPanel.Controls.Add(wantedPanel);// ^^
                    return; // quites the method
                }
            }
        }

        private void InitializePanelFormNodeList(String nodeName, FlowLayoutPanel placementPanel, List<WheelSlotPanel> placementList)// handles the creation of new panels from the nodelistbox
        {
            String toFind = nodeName.Substring(0, nodeName.IndexOf(" From Node Box"));// isolate the name of the drag and drop object
            OnInvalidatePage();// not gonna lie. I've got no clue what this does. But everything else has it here so I assume its important. maybe not tho
            WheelSlotPanel panel = new WheelSlotPanel();// preps a wheel panel to place a wheel setup into
            panel.WheelTypeChanged += Panel_WheelTypeChanged;// ^^^^
            foreach (WheelSlotPanel wheel in placementPanel.Controls)// iterates over the existing wheel nodes to find if a node is below the cursor to add the wheel into that pos
            {
                if ((wheel.PointToScreen(Point.Empty).Y > System.Windows.Forms.Control.MousePosition.Y)) // checks if the node is above or below the cursor
                {
                    Control[] tempControls = new Control[placementPanel.Controls.Count - placementPanel.Controls.IndexOf(wheel)]; // makes a temp array for the controls below the cursor
                    List<WheelSlotPanel> tempSlots = new List<WheelSlotPanel>();//                                                         ^^
                    int pos = 0;// tracks the position of the temp array
                    for (int i = placementPanel.Controls.IndexOf(wheel); i < placementPanel.Controls.Count;)// iterates over the controls until there are only the controls above the cursor still exiting
                    {
                        tempControls[pos] = placementPanel.Controls[i];// copies the existing controls into a temp array
                        tempSlots.Add(placementList[i]);//                                  ^^^
                        pos++;// increase pos for the next loop thru
                        placementPanel.Controls.RemoveAt(i); // removes the control form the actual array so we can add the control into the right place
                        placementList.RemoveAt(i);//                  ^^
                    }

                    placementList.Add(panel);// adds a new panel
                    placementPanel.Controls.Add(panel);// ^^
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

                    placementPanel.Controls.AddRange(tempControls);// add the temp controls back into the array
                    placementList.AddRange(tempSlots);//                               ^^
                    NodeListBox.Items.Remove(toFind);// removes the corresponding from the list of nodes

                    if (NodeListBox.Items.Count < 1)
                        OnSetEndEarly(true); // Skip next page, no parts are left

                    return; // quites the method
                }
            }
            placementList.Add(panel); // adds the panel into the end of the list
            placementPanel.Controls.Add(panel);// ^^^
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

            if (NodeListBox.Items.Count < 1)
                OnSetEndEarly(true); // Skip next page, no parts are left
        }

        public String RemoveWheelSetupPanel(String s)// handles the user clicking the remove button in a wheel panel/ slot
        {
            foreach (Object wheel in LeftWheelsPanel.Controls)// goes over all the objects in the panel
            {
                if (wheel.GetType().Equals(typeof(WheelSlotPanel)))// confirms that the object is atucally a wheel panel, needed because there are things other than wheel panes in the panel
                {
                    if (((WheelSlotPanel)wheel).name.Equals(s))// sees if this is the panel the remove event came from
                    {
                        LeftWheelsPanel.Controls.Remove(((WheelSlotPanel)wheel));// removes the pane from the panel
                        leftSlots.Remove(((WheelSlotPanel)wheel));//                  ^^
                    }
                }
            }
            foreach (Object wheel in RightWheelsPanel.Controls)// goes over all the objects in the panel, second foreach needed because we need to look in both the left and right panel for the pane and there's no good way to search the panel ahead of time
            {
                if (wheel.GetType().Equals(typeof(WheelSlotPanel)))// confirms that the object is atucally a wheel panel, needed because there are things other than wheel panes in the panel
                {
                    if (((WheelSlotPanel)wheel).name.Equals(s))// sees if this is the panel the remove event came from
                    {
                        RightWheelsPanel.Controls.Remove(((WheelSlotPanel)wheel));// removes the pane from the panel
                        rightSlots.Remove(((WheelSlotPanel)wheel));//                  ^^
                    }
                }
            }
            NodeListBox.Items.Add(s);// adds the node back into the nodelist
            object[] list;// makes a temporary array so we can sort the array
            NodeListBox.Items.CopyTo(list = new object[NodeListBox.Items.Count], 0);// copies the nodes into the array
            ArrayList a = new ArrayList();// makes a new arraylist so we can sort super easily
            a.AddRange(list);// add the list to the arraylist
            a.Sort();//sort the array
            NodeListBox.Items.Clear();// clear the node list so we can readd the ordered list
            foreach(object sortedItem in a)// goes over all the sorted values
            {
                NodeListBox.Items.Add(sortedItem);// add the ordered nodes back to the list
            }
            OnSetEndEarly(false);
            return "";// needed because c# gets real ticked if this isn't here
        }

        public String WheelSetupHover(String s)// handler for the hover event in the wheel slot/ panel
        {
            StandardAddInServer.Instance.WizardSelect(listItems[s]);// seleects/ highlghts the appropriate node in Inventor
            return "";// needed because c# gets real ticked if this isn't here
        }
            
        private void NodeListBox_SelectedIndexChanged(object sender, EventArgs e)//  handles when the selected value in the node box is changed so we can show the node in Inventor
        { 
            try
            {
                StandardAddInServer.Instance.WizardSelect(listItems[NodeListBox.Items[NodeListBox.SelectedIndex].ToString()]);// selects/highlights the appriate node in Inventor
            }
            catch (Exception) { }// catches any weird things the user tries to do, handles when weird things called this method that we dont really care about
        }

        private void DefineWheelsInstruction1_Click(object sender, EventArgs e)
        {

        }

        private void AutoFill_Click(Object sender, EventArgs e) // Initializes autofill process
        {
            AutoFillPage autoForm = new AutoFillPage(this);
            autoForm.ShowDialog(); // opens page that takes info on number of wheels
        }
    }
}