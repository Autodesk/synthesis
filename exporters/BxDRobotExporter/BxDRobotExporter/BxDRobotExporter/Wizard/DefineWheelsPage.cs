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
    public partial class DefineWheelsPage : UserControl, IWizardPage
    {
        private int checkedCount = 0;

        private bool disableChecked = false;

        private Dictionary<string, RigidNode_Base> checkedListItems = new Dictionary<string, RigidNode_Base>();

        private List<WheelSlotPanel> slots = new List<WheelSlotPanel>();

        public DefineWheelsPage()
        {
            InitializeComponent();

            NodeCheckedListBox.CheckOnClick = false;

            foreach (RigidNode_Base node in Utilities.GUI.SkeletonBase.ListAllNodes())
            {
                if (node.GetSkeletalJoint() != null && node.GetSkeletalJoint().GetJointType() == SkeletalJointType.ROTATIONAL)
                {
                    NodeCheckedListBox.Items.Add(node.ModelFileName);
                    checkedListItems.Add(node.ModelFileName, node);
                }
            }
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
                if(WizardData.Instance.WheelCount != slots.Count)
                {
                    _initialized = false;
                }
            };
        }

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
                switch (WizardData.Instance.DriveTrain)
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
                        GetNextEmptyPanel().FillSlot(checkedListItems.Values.ElementAt(e.Index), WizardData.WizardWheelType.NORMAL);
                        break;
                    case WizardData.WizardDriveTrain.CUSTOM:
                        GetNextEmptyPanel().FillSlot(checkedListItems.Values.ElementAt(e.Index));
                        break;
                }
                checkedCount++;

                if (checkedCount == WizardData.Instance.WheelCount)
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

        private void ValidateInput()
        {
            switch (WizardData.Instance.DriveTrain)
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
                        WarningLabel.Text = string.Format("Warning: Wheel {0} {1} {2} not set to Mecanum. This may cause problems with the exporter. If you have a custom drive train, select \'Other/Custom\' on the previous page.", (BadPanelCount == 1) ? "node" : "nodes", BadPanels.Substring(0, (BadPanels.Length - 2)), (BadPanelCount == 1) ? "is" : "are");
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

        private void Panel_WheelTypeChanged(object sender, WheelTypeChangedEventArgs e)
        {
            ValidateInput();
        }

        private WheelSlotPanel GetNextEmptyPanel()
        {
            foreach(WheelSlotPanel panel in slots)
            {
                if (!panel.IsFilled)
                    return panel;
            }
            return null;
        }

        #region IWizardPage Implementation
        public void OnNext()
        {
            WizardData.Instance.Wheels = new List<WizardData.WheelSetupData>();
            foreach(var slot in slots)
            {
                WizardData.Instance.Wheels.Add(slot.WheelData);
            }
        }

        public void Initialize()
        {
            slots = new List<WheelSlotPanel>();
            for (int i = 0; i < WizardData.Instance.WheelCount; i++)
            {
                WheelSlotPanel panel = new WheelSlotPanel();
                panel.WheelTypeChanged += Panel_WheelTypeChanged;
                slots.Add(panel);
                WheelPropertiesPanel.Controls.Add(panel);
                
            }

            UpdateProgress();
            _initialized = true;
        }
        
        private void UpdateProgress()
        {
            WheelProgressLabel.Text = string.Format("{0} out of {1} wheels selected.", checkedCount, WizardData.Instance.WheelCount);
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