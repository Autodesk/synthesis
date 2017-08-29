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

        /// <summary>
        /// Set to true when <see cref="checkedCount"/> equals <see cref="WizardData.wheelCount"/>
        /// </summary>
        private bool disableChecked = false;

        /// <summary>
        /// Dictionary associating node file names with their respective <see cref="RigidNode_Base"/>s
        /// </summary>
        private Dictionary<string, RigidNode_Base> checkedListItems = new Dictionary<string, RigidNode_Base>();

        /// <summary>
        /// List of all the <see cref="WheelSlotPanel"/>s in this page. Referenced in <see cref="GetNextEmptyPanel()"/>
        /// </summary>
        private List<WheelSlotPanel> slots = new List<WheelSlotPanel>();

        public DefineWheelsPage()
        {
            InitializeComponent();

            NodeCheckedListBox.CheckOnClick = false;


            NodeCheckedListBox.SelectedIndexChanged += delegate (object sender, EventArgs e)
            {
                StandardAddInServer.Instance.WizardSelect(checkedListItems[NodeCheckedListBox.Items[NodeCheckedListBox.SelectedIndex].ToString()]);
            };

            NodeCheckedListBox.ItemCheck += NodeCheckedListBox_ItemCheck;

            this.VisibleChanged += delegate (object sender, EventArgs e)
            {
                if (this.Visible)
                {
                    UpdateProgress();
                }
                if(WizardData.Instance.wheelCount != slots.Count)
                {
                    _initialized = false;
                }
            };
        }

        /// <summary>
        /// Either fills or removes a <see cref="WheelSetupPanel"/> from a <see cref="WheelSlotPanel"/> 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NodeCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
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
                    case WizardData.WizardDriveTrain.WESTERN:
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
                    if (slot.Node == checkedListItems[NodeCheckedListBox.Items[e.Index].ToString()])
                        slot.FreeSlot();
                }
            }

            UpdateProgress();
        }

        /// <summary>
        /// Makes sure all of the wheels are set correctly.
        /// </summary>
        private void ValidateInput()
        {
            switch (WizardData.Instance.driveTrain)
            {
                case WizardData.WizardDriveTrain.MECANUM:
                    string BadPanels = string.Empty;
                    int BadPanelCount = 0;
                    foreach (WheelSlotPanel slot in slots)
                    {
                        if (slot.WheelType != WizardData.WizardWheelType.MECANUM)
                        {
                            BadPanels += slot.Node.ModelFileName + ", ";
                            BadPanelCount++;
                        }
                    }

                    if (!string.IsNullOrEmpty(BadPanels))
                    {
                        WarningLabel.Text = string.Format("Warning: Wheel {0} {1} {2} not set to Mecanum. This may cause problems with the exporter. If you have a custom drive train, select \'Other/Custom\' on the previous page.",
                            (BadPanelCount == 1) ? "node" : "nodes", BadPanels.Substring(0, (BadPanels.Length - 2)), (BadPanelCount == 1) ? "is" : "are");
                        OnDeactivateNext();
                    }
                    else
                    {
                        WarningLabel.Text = string.Empty;
                        OnActivateNext();
                    }
                    break;
                case WizardData.WizardDriveTrain.H_DRIVE:
                    BadPanels = string.Empty;
                    BadPanelCount = 0;
                    foreach (WheelSlotPanel slot in slots)
                    {
                        if (slot.WheelType != WizardData.WizardWheelType.OMNI)
                        {
                            BadPanels += slot.Node.ModelFileName + ", ";
                            BadPanelCount++;
                        }
                    }

                    if (!string.IsNullOrEmpty(BadPanels))
                    {
                        WarningLabel.Text = string.Format("Warning: Wheel {0} {1} {2} not set to Omni. This may cause problems with the exporter. If you have a custom drive train, select \'Other/Custom\' on the previous page.", (BadPanelCount == 1) ? "node" : "nodes", BadPanels.Substring(0, (BadPanels.Length - 2)), (BadPanelCount == 1) ? "is" : "are");
                        OnDeactivateNext();
                    }
                    else
                    {
                        WarningLabel.Text = string.Empty;
                        OnActivateNext();
                    }
                    break;
                case WizardData.WizardDriveTrain.CUSTOM:
                    break;
                default:
                    BadPanels = string.Empty;
                    BadPanelCount = 0;
                    foreach (WheelSlotPanel slot in slots)
                    {
                        if (slot.WheelType != WizardData.WizardWheelType.NORMAL)
                        {
                            BadPanels += slot.Node.ModelFileName + ", ";
                            BadPanelCount++;
                        }
                    }

                    if (!string.IsNullOrEmpty(BadPanels))
                    {
                        WarningLabel.Text = string.Format("Warning: Wheel {0} {1} {2} not set to Normal. This may cause problems with the exporter. If you have a custom drive train, select \'Other/Custom\' on the previous page.", (BadPanelCount == 1) ? "node" : "nodes", BadPanels.Substring(0, (BadPanels.Length - 2)), (BadPanelCount == 1) ? "is" : "are");
                        OnDeactivateNext();
                    }
                    else
                    {
                        WarningLabel.Text = string.Empty;
                        OnActivateNext();
                    }
                    break;
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
        /// Gets the next unfilled <see cref="WheelSlotPanel"/>. Referenced in <see cref="NodeCheckedListBox_ItemCheck(object, ItemCheckEventArgs)"/>
        /// </summary>
        /// <returns></returns>
        private WheelSlotPanel GetNextEmptyPanel()
        {
            for(int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsFilled)
                    return slots[i];
            }
            return null;
        }

        /// <summary>
        /// Updates <see cref="WheelProgressLabel"/>
        /// </summary>
        private void UpdateProgress()
        {
            WheelProgressLabel.Text = string.Format("{0} out of {1} wheels selected.", checkedCount, WizardData.Instance.wheelCount);
        }

        #region IWizardPage Implementation
        /// <summary>
        /// Adds all the <see cref="WizardData.WheelSetupData"/> from each <see cref="WheelSetupPanel"/> to <see cref="WizardData.wheels"/>
        /// </summary>
        public void OnNext()
        {
            WizardData.Instance.wheels = new List<WizardData.WheelSetupData>();
            foreach(var slot in slots)
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
                        NodeCheckedListBox.Items.Add(node.ModelFileName);
                        checkedListItems.Add(node.ModelFileName, node);
                    }
                }
            }
            else
            {
                foreach (RigidNode_Base node in Utilities.GUI.SkeletonBase.ListAllNodes())
                {
                    if (node.GetParent().GetParent() != null)
                    {
                        NodeCheckedListBox.Items.Add(node.ModelFileName);
                        checkedListItems.Add(node.ModelFileName, node);
                    }
                }
            }

            slots = new List<WheelSlotPanel>();
            for (int i = 0; i < WizardData.Instance.wheelCount; i++)
            {
                WheelSlotPanel panel = new WheelSlotPanel();
                panel.WheelTypeChanged += Panel_WheelTypeChanged;
                slots.Add(panel);
                WheelPropertiesPanel.Controls.Add(panel);                
            }

            UpdateProgress();
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
                    while (WheelPropertiesPanel.Controls.Count > 0)
                        WheelPropertiesPanel.Controls[0].Dispose(); 
                }
                _initialized = value;
            }
        }
        private bool _initialized = false;
        #endregion
    }
}