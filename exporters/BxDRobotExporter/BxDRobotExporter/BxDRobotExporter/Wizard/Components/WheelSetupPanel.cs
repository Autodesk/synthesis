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
    /// Used in the <see cref="DefineWheelsPage"/> to set wheel properties. At some point, we probably could detect whether the wheel is left or right, but, gotta crank out that release on time.
    /// </summary>
    public partial class WheelSetupPanel : UserControl
    {
        public static event OnWheelSetupPanelRemove remove;// sends the remove event to the class that actually needs it
        public static event OnWheelSetupPanelHover hover;// sends the hover event to the class that actually needs it
        public static event OnWheelSlotMouseDown mouseDownHandler;// sends the mouse down event to the class that actually needs it(WheelSlot)
        public static event OnWheelSlotMouseUp mouseUpHandler;// sends the mouse up event to the class that actually needs it(WheelSlot)
        public static event OnWheelSetupPanelMouseMove mouseMoveHandler;// sends the mouse moving event to the class that actually needs it(WheelSlot)
        public String name;// the name of the node, makes removal/ adding easier for the definewheel class
        public bool isRightWheel;// helps in automatically assigning PWM ports

        public WheelSetupPanel(RigidNode_Base node, String name, WizardData.WizardWheelType WheelType = WizardData.WizardWheelType.NORMAL)
        {
            this.name = name;// sets the internal name so we can easily work with the panels
            InitializeComponent();
            
            WheelTypeComboBox.SelectedIndex = ((int)WheelType) - 1;
            FrictionComboBox.SelectedIndex = 1;

            this.node = node;

            MainGroupBox.Text = name;

            this.MouseClick += delegate (object sender, MouseEventArgs e)
            {
                if (this.node != null)
                    StandardAddInServer.Instance.WizardSelect(this.node);
            };

            this.BackColor = Control.DefaultBackColor;
        }

        public RigidNode_Base node;

        

        /// <summary>
        /// Gets the <see cref="WizardData.WheelSetupData"/> for this panel. The parent page then adds it to <see cref="WizardData.wheels"/>
        /// </summary>
        /// <returns></returns>
        public WizardData.WheelSetupData GetWheelData()
        {
            return new WizardData.WheelSetupData
            {
                FrictionLevel = (WizardData.WizardFrictionLevel)this.FrictionComboBox.SelectedIndex,
                WheelType = (WizardData.WizardWheelType)(this.WheelTypeComboBox.SelectedIndex + 1),
                PWMPort = isRightWheel ? (byte)0x02 : (byte)0x01,
                Node = this.node
            };
        }

        /// <summary>
        /// Gets the <see cref="WizardData.WizardFrictionLevel"/> from the FrictionComboBox
        /// </summary>
        public WizardData.WizardFrictionLevel FrictionLevel { get => (WizardData.WizardFrictionLevel)this.FrictionComboBox.SelectedIndex; }

        /// <summary>
        /// Gets the <see cref="WizardData.WizardWheelType"/> from the WheelTypeComboBox.
        /// </summary>
        public WizardData.WizardWheelType WheelType { get => (WizardData.WizardWheelType)(this.WheelTypeComboBox.SelectedIndex + 1); }

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
            remove(name);// sends the remove event so we can remove this panel
        }

        private void WheelSetupPanel_MouseHover(object sender, EventArgs e)// handles mouse hover events
        {
           
            hover(name);// sends the hover event so we can highlight the node in Inventor
         //   this.backgroundLabel.BackColor = System.Drawing.SystemColors.Highlight;// highlights the background, could make it easier for user to see what panel is what node in Inventor
        }

        private void WheelSetupPanel_MouseDown(object sender, MouseEventArgs e)// handles mouse down events
        {
            mouseDownHandler(name);// sends that there was a mouse down event and the name of the panel, allows us to disable the drag/ drop
        }

        private void WheelSetupPanel_MouseUp(object sender, MouseEventArgs e)// handles mouse up events
        {
            mouseUpHandler(name);// sends that there was a mouse up event and the name of the panel, allows us to enable to=he drag/ drop
        }

        private void backgroundLabel_MouseMove(object sender, MouseEventArgs e)// handles mouse movement over the background
        {
            mouseMoveHandler(name);// sends that there was a mouse move event and the name of the panel, this helps in activating the drag and drop if the mouse is already down
        }
    }

    public delegate string OnWheelSetupPanelRemove(string str);// sends the remove event to other classes
    public delegate string OnWheelSlotMouseDown(string str);// sends the mouse down event to other classes
    public delegate string OnWheelSlotMouseUp(string str);// sends the mouse down event to other classes
    public delegate string OnWheelSetupPanelHover(string str);// sends the mouse hover event to other classes
    public delegate string OnWheelSetupPanelMouseMove(string str);// sends the mouse move event to other classes
}
