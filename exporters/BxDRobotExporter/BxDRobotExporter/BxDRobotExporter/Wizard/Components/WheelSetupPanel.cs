using System;
using System.Drawing;
using System.Windows.Forms;

namespace BxDRobotExporter.Wizard
{
    /// <summary>
    /// Used in the <see cref="DefineWheelsPage"/> to set wheel properties. At some point, we probably could detect whether the wheel is left or right, but, gotta crank out that release on time.
    /// </summary>
    public partial class WheelSetupPanel : UserControl
    {
        public event OnWheelSetupPanelRemove removeHandler;// sends the remove event to the class that actually needs it
        public event OnWheelSlotMouseDown mouseDownHandler;// sends the mouse down event to the class that actually needs it(WheelSlot)
        public String NodeName;// the name of the node, makes removal/ adding easier for the definewheel class
        public RigidNode_Base Node;
        public WheelSide Side = WheelSide.UNASSIGNED;// helps in automatically assigning PWM ports

        public WheelSetupPanel(RigidNode_Base node, String name, WizardData.WizardWheelType WheelType = WizardData.WizardWheelType.NORMAL)
        {
            NodeName = name;// sets the internal name so we can easily work with the panels
            InitializeComponent();
            MinimumSize = new Size(0, 0); // Min size is only used in editor
            Dock = DockStyle.Top;

            WheelTypeComboBox.SelectedIndex = ((int)WheelType) - 1;
            FrictionComboBox.SelectedIndex = 1;

            this.Node = node;

            MainGroupBox.Text = name;

            this.MouseClick += delegate (object sender, MouseEventArgs e)
            {
                if (this.Node != null)
                    InventorUtils.FocusAndHighlightNode(this.Node, StandardAddInServer.Instance.MainApplication.ActiveView.Camera, 0.8);
            };
            
            BackColor = Color.White;

            AddInteractEventsToAll(this);
        }

        /// <summary>
        /// Gets the <see cref="WizardData.WheelSetupData"/> for this panel. The parent page then adds it to <see cref="WizardData.wheels"/>
        /// </summary>
        /// <returns></returns>
        public WizardData.WheelSetupData GetWheelData()
        {
            byte wantedPWM;
            if(Side == WheelSide.RIGHT)
            {
                wantedPWM = (byte)0x00;
            } else if( Side == WheelSide.LEFT){
                wantedPWM = (byte)0x01;
            } else if(Side == WheelSide.MIDDLE || Side == WheelSide.RIGHTBACK){
                wantedPWM = (byte)0x02;
            } else
            {
                wantedPWM = (byte)0x03;
            }
            return new WizardData.WheelSetupData
            {
                FrictionLevel = (WizardData.WizardFrictionLevel)this.FrictionComboBox.SelectedIndex,
                WheelType = (WizardData.WizardWheelType)(this.WheelTypeComboBox.SelectedIndex + 1),
                PWMPort = wantedPWM, // Engine uses port 1 for left and port 1 for right. We wanted to change this but it would impact a large amount of controller code
                Node = this.Node
            };
        }

        /// <summary>
        /// Add HighlightNode to every element's hover event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddInteractEventsToAll(Control baseControl)
        {
            if (!(baseControl is Button || baseControl is ComboBox)) // Add other interactable items to this check as needed
                baseControl.MouseDown += WheelSetupPanel_MouseDown;

            baseControl.MouseHover += HighlightNode;

            foreach (Control control in baseControl.Controls)
                AddInteractEventsToAll(control);
        }

        /// <summary>
        /// Highlights the node in inventor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HighlightNode(object sender, EventArgs e)
        {
            InventorUtils.FocusAndHighlightNode(Node, StandardAddInServer.Instance.MainApplication.ActiveView.Camera, 0.8);
        }

        /// <summary>
        /// Gets the <see cref="WizardData.WizardFrictionLevel"/> from the FrictionComboBox
        /// </summary>
        public WizardData.WizardFrictionLevel FrictionLevel
        {
            get => (WizardData.WizardFrictionLevel)FrictionComboBox.SelectedIndex;
            set => FrictionComboBox.SelectedIndex = (int)value;
        }

        /// <summary>
        /// Gets the <see cref="WizardData.WizardWheelType"/> from the WheelTypeComboBox.
        /// </summary>
        public WizardData.WizardWheelType WheelType
        {
            get => (WizardData.WizardWheelType)(WheelTypeComboBox.SelectedIndex + 1);
            set => WheelTypeComboBox.SelectedIndex = (int)value - 1;
        }

        /// <summary>
        /// Used to invoke the <see cref="WheelSlotPanel.WheelTypeChanged"/> event in the parent <see cref="WheelSlotPanel"/>
        /// </summary>
        public event Action _WheelTypeChangedInternal;

        private void WheelTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _WheelTypeChangedInternal?.Invoke();
        }

        private void remove_Click(object sender, EventArgs e)// handles the remove button click
        {
            removeHandler(NodeName);// sends the remove event so we can remove this panel
        }

        private void WheelSetupPanel_MouseDown(object sender, MouseEventArgs e)// handles mouse down events
        {
            mouseDownHandler(NodeName);// sends that there was a mouse down event and the name of the panel, allows us to disable the drag/ drop
        }
    }

    public delegate void OnWheelSetupPanelRemove(string str);// sends the remove event to other classes
    public delegate void OnWheelSlotMouseDown(string str);// sends the mouse down event to other classes
}
