using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BxDRobotExporter.Wizard
{
    /// <summary>
    /// Prompts the user to define all the wheels on the robot.
    /// </summary>
    public partial class DefineWheelsPage : UserControl, IWizardPage
    {
        /// <summary>
        /// Active counter of how many <see cref="RigidNode_Base"/>s have been selected
        /// </summary>
        private int checkedCount = 0;
        private int totalMass = 0;

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
            this.VisibleChanged += delegate (object sender, EventArgs e)
            {
                if (Visible) ValidateInput();
            };
            RightWheelPropertiesPanel.AllowDrop = true;
            LeftWheelPropertiesPanel.AllowDrop = true;
            NodeListBox.AllowDrop = true;

            DriveTrainDropdown.SelectedIndex = 0;
            
            rightSlots = new List<WheelSlotPanel>();
            leftSlots = new List<WheelSlotPanel>();

            NodeListBox.Enabled = false;
            Initialize();
            
        }

        /// <summary>
        /// Either fills or removes a <see cref="WheelSetupPanel"/> from a <see cref="WheelSlotPanel"/> 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void NodeListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if(e.NewValue == CheckState.Checked)
            {
                if(disableChecked)
                {
                    e.NewValue = CheckState.Unchecked;
                    return;
                }
                OnInvalidatePage();
                switch (WizardData.Instance.driveTrain)
                {
                    case WizardData.WizardDriveTrain.TANK:
                        GetNextEmptyPanel().FillSlot(checkedListItems.Values.ElementAt(e.Index));
                        break;
                    case WizardData.WizardDriveTrain.MECANUM:
                        GetNextEmptyPanel().FillSlot(checkedListItems.Values.ElementAt(e.Index), WizardData.WizardWheelType.MECANUM);
                        break;
                    case WizardData.WizardDriveTrain.H_DRIVE:
                        GetNextEmptyPanel().FillSlot(checkedListItems.Values.ElementAt(e.Index), WizardData.WizardWheelType.OMNI);
                        break;
                    case WizardData.WizardDriveTrain.SWERVE:
                        //TODO implement this crap
                        GetNextEmptyPanel().FillSlot(checkedListItems.Values.ElementAt(e.Index));
                        break;
                    case WizardData.WizardDriveTrain.CUSTOM:
                        GetNextEmptyPanel().FillSlot(checkedListItems.Values.ElementAt(e.Index));
                        break;
                }
                checkedCount++;

                if (checkedCount == WizardData.Instance.wheelCount)
                    disableChecked = true;

            }
            else
            {
                OnInvalidatePage();
                checkedCount--;
                disableChecked = false;

                foreach(var slot in slots)
                {
                    if (slot.Node == checkedListItems[NodeListBox.Items[e.Index].ToString()])
                        slot.FreeSlot();
                }
            }

            UpdateProgress();
        }*/

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
                case 1: //Western
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
            ValidateInput();
            //checkedListItems.Clear();
            //UpdateWheelPanes();
        }

        /// <summary>
        /// Makes sure all of the wheels are set correctly.
        /// </summary>
        private void ValidateInput()
        {
            if (totalMass != 0.0f)
            {
                WarningLabel.Text = string.Empty;
                OnActivateNext();
            }
            else
            {
                WarningLabel.Text = string.Format("Please enter a Robot Weight");
                OnDeactivateNext();
            }
            
        }

        /// <summary>
        /// Validates input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Panel_WheelTypeChanged(object sender, WheelTypeChangedEventArgs e)
        {
            ValidateInput();
        }

        /// <summary>
        /// Gets the next unfilled <see cref="WheelSlotPanel"/>. Referenced in <see cref="NodeListBox_ItemCheck(object, ItemCheckEventArgs)"/>
        /// </summary>
        /// <returns></returns>
        private WheelSlotPanel GetNextEmptyPanelLeft()
        {
            for (int i = 0; i < leftSlots.Count; i++)
            {
                if (!leftSlots[i].IsFilled)
                    return leftSlots[i];
            }
            return null;
        }
        private WheelSlotPanel GetNextEmptyPanelRight()
        {
            for (int i = 0; i < rightSlots.Count; i++)
            {
                if (!rightSlots[i].IsFilled)
                    return rightSlots[i];
            }
            return null;
        }

        #region IWizardPage Implementation
        /// <summary>
        /// Adds all the <see cref="WizardData.WheelSetupData"/> from each <see cref="WheelSetupPanel"/> to <see cref="WizardData.wheels"/>
        /// </summary>
        public void OnNext()
        {
            WizardData.Instance.massMode = WizardData.WizardMassMode.SIMPLE_USER;
            WizardData.Instance.mass = totalMass;
            WizardData.Instance.wheels = new List<WizardData.WheelSetupData>();
            foreach(var slot in rightSlots)
            {
                WizardData.Instance.wheels.Add(slot.WheelData);
            }
            foreach (var slot in leftSlots)
            {
                WizardData.Instance.wheels.Add(slot.WheelData);
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
                        NodeListBox.Items.Add(node.ModelFileName);
                        listItems.Add(node.ModelFileName, node);
                    }
                }
            }
            else
            {
                foreach (RigidNode_Base node in Utilities.GUI.SkeletonBase.ListAllNodes())
                {
                    if (node.GetParent().GetParent() != null)
                    {
                        NodeListBox.Items.Add(node.ModelFileName);
                        listItems.Add(node.ModelFileName, node);
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
                LeftWheelPropertiesPanel.Controls.Add(leftPanel);
                WheelSlotPanel righPanel = new WheelSlotPanel();
                righPanel.WheelTypeChanged += Panel_WheelTypeChanged;
                rightSlots.Add(righPanel);
                RightWheelPropertiesPanel.Controls.Add(righPanel);
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
                    //NodeListBox.

                    LeftWheelPropertiesPanel.Controls.Remove(leftSlots.ElementAt(i));
                    leftSlots.Remove(leftSlots.ElementAt(i));
                }
            }
           
            if (rightSlots.Count > WizardData.Instance.wheelCount)
            {
                int downTo = rightSlots.Count;
                for (int i = downTo - 1; i > WizardData.Instance.wheelCount / 2 - 1; i--)
                {
                    RightWheelPropertiesPanel.Controls.Remove(rightSlots.ElementAt(i));
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
                    while (LeftWheelPropertiesPanel.Controls.Count > 0)
                        LeftWheelPropertiesPanel.Controls[0].Dispose(); 
                }
                _initialized = value;
            }
        }
        private bool _initialized = false;
        #endregion
        private void MetricCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateMassCount();
        }
        private void UpdateMassCount()
        {
             ValidateInput();
        }
        private void NodeListBox_MouseDown(object sender, MouseEventArgs e)
        {
            StandardAddInServer.Instance.WizardSelect(listItems[NodeListBox.SelectedItem.ToString()]);
            NodeListBox.DoDragDrop(NodeListBox.SelectedItems, DragDropEffects.Copy |   
                        DragDropEffects.Move);  
        }

        private void RightWheelPropertiesPanel_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }
        private void RightWheelPropertiesPanel_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                foreach (String r in NodeListBox.SelectedItems)
                {
                    OnInvalidatePage();
                    switch (WizardData.Instance.driveTrain)
                    {
                        case WizardData.WizardDriveTrain.TANK:
                            WheelSlotPanel panel = new WheelSlotPanel();
                            panel.WheelTypeChanged += Panel_WheelTypeChanged;
                            rightSlots.Add(panel);
                            RightWheelPropertiesPanel.Controls.Add(panel);
                            GetNextEmptyPanelRight().FillSlot(listItems[r], r);
                            break;
                        case WizardData.WizardDriveTrain.MECANUM:
                            WheelSlotPanel panel1 = new WheelSlotPanel();
                            panel1.WheelTypeChanged += Panel_WheelTypeChanged;
                            rightSlots.Add(panel1);
                            RightWheelPropertiesPanel.Controls.Add(panel1);
                            GetNextEmptyPanelRight().FillSlot(listItems[r], r, WizardData.WizardWheelType.MECANUM);
                            break;
                        case WizardData.WizardDriveTrain.H_DRIVE:
                            WheelSlotPanel panel2 = new WheelSlotPanel();
                            panel2.WheelTypeChanged += Panel_WheelTypeChanged;
                            rightSlots.Add(panel2);
                            RightWheelPropertiesPanel.Controls.Add(panel2);
                            GetNextEmptyPanelRight().FillSlot(listItems[r], r, WizardData.WizardWheelType.OMNI);
                            break;
                        case WizardData.WizardDriveTrain.SWERVE:
                            //TODO implement this crap
                            WheelSlotPanel panel3 = new WheelSlotPanel();
                            panel3.WheelTypeChanged += Panel_WheelTypeChanged;
                            rightSlots.Add(panel3);
                            RightWheelPropertiesPanel.Controls.Add(panel3);
                            GetNextEmptyPanelRight().FillSlot(listItems[r], r);
                            break;
                        case WizardData.WizardDriveTrain.CUSTOM:
                            WheelSlotPanel panel4 = new WheelSlotPanel();
                            panel4.WheelTypeChanged += Panel_WheelTypeChanged;
                            rightSlots.Add(panel4);
                            RightWheelPropertiesPanel.Controls.Add(panel4);
                            GetNextEmptyPanelRight().FillSlot(listItems[r], r);
                            break;
                    }
                    NodeListBox.Items.Remove(r);
                }
            }
            catch (Exception f) { }
        }
        private void LeftWheelPropertiesPanel_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }
        private void LeftWheelPropertiesPanel_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                foreach (String r in NodeListBox.SelectedItems)
                {
                    OnInvalidatePage();
                    switch (WizardData.Instance.driveTrain)
                    {
                        case WizardData.WizardDriveTrain.TANK:
                            WheelSlotPanel panel = new WheelSlotPanel();
                            panel.WheelTypeChanged += Panel_WheelTypeChanged;
                            leftSlots.Add(panel);
                            LeftWheelPropertiesPanel.Controls.Add(panel);
                            GetNextEmptyPanelLeft().FillSlot(listItems[r],r );
                            break;
                        case WizardData.WizardDriveTrain.MECANUM:
                            WheelSlotPanel panel1 = new WheelSlotPanel();
                            panel1.WheelTypeChanged += Panel_WheelTypeChanged;
                            leftSlots.Add(panel1);
                            LeftWheelPropertiesPanel.Controls.Add(panel1);
                            GetNextEmptyPanelLeft().FillSlot(listItems[r], r, WizardData.WizardWheelType.MECANUM);
                            break;
                        case WizardData.WizardDriveTrain.H_DRIVE:
                            WheelSlotPanel panel2 = new WheelSlotPanel();
                            panel2.WheelTypeChanged += Panel_WheelTypeChanged;
                            leftSlots.Add(panel2);
                            LeftWheelPropertiesPanel.Controls.Add(panel2);
                            GetNextEmptyPanelLeft().FillSlot(listItems[r], r, WizardData.WizardWheelType.OMNI);
                            break;
                        case WizardData.WizardDriveTrain.SWERVE:
                            //TODO implement this crap
                            WheelSlotPanel panel3 = new WheelSlotPanel();
                            panel3.WheelTypeChanged += Panel_WheelTypeChanged;
                            leftSlots.Add(panel3);
                            LeftWheelPropertiesPanel.Controls.Add(panel3);
                            GetNextEmptyPanelLeft().FillSlot(listItems[r], r);
                            break;
                        case WizardData.WizardDriveTrain.CUSTOM:
                            WheelSlotPanel panel4 = new WheelSlotPanel();
                            panel4.WheelTypeChanged += Panel_WheelTypeChanged;
                            leftSlots.Add(panel4);
                            LeftWheelPropertiesPanel.Controls.Add(panel4);
                            GetNextEmptyPanelLeft().FillSlot(listItems[r], r);
                            break;
                    }
                    NodeListBox.Items.Remove(r);
                }
            }
            catch (Exception f) { }
            }
        public String RemoveWheelSetupPanel(String s)
        {
            foreach (Object wheel in LeftWheelPropertiesPanel.Controls)
            {
                if (wheel.GetType().Equals(typeof(WheelSlotPanel)))
                {
                    if (((WheelSlotPanel)wheel).name.Equals(s))
                    {
                        LeftWheelPropertiesPanel.Controls.Remove(((WheelSlotPanel)wheel));
                        leftSlots.Remove(((WheelSlotPanel)wheel));
                    }
                }
            }
            foreach (Object wheel in RightWheelPropertiesPanel.Controls)
            {
                if (wheel.GetType().Equals(typeof(WheelSlotPanel)))
                {
                    if (((WheelSlotPanel)wheel).name.Equals(s))
                    {
                        RightWheelPropertiesPanel.Controls.Remove(((WheelSlotPanel)wheel));
                        rightSlots.Remove(((WheelSlotPanel)wheel));
                    }
                }
            }
            //     MessageBox.Show(listItems[s].ToString());
            NodeListBox.Items.Add(s);
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
            catch (Exception f) { }
        }
       
        private void NumericUpDown1_ValueChanged(Object sender, EventArgs e)
        {

            totalMass = (int)Math.Round(this.numericUpDown1.Value);
            ValidateInput();

        }
    }
}