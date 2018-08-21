using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InternalFieldExporter.Wizard
{
    /// <summary>
    /// Panel at the bottom of the <see cref="WizardForm"/> used for navigating through the pages.
    /// </summary>
    public partial class WizardNavigator : UserControl
    {
        public enum WizardNavigatorState : byte
        {
            /// <summary>
            /// <see cref="NextButton"/> reads "Start >".
            /// </summary>
            StartEnabled = 0b00000001,
            /// <summary>
            /// <see cref="NextButton"/> reads "Finish". When clicked, it invokes the <see cref="WizardPageControl.FinishClicked"/> event.
            /// </summary>
            FinishEnabled = 0b00000010,
            /// <summary>
            /// The entire control is invisible.
            /// </summary>
            Hidden = 0b00000100,
            /// <summary>
            /// Both buttons are hidden, but a loading message is displayed.
            /// </summary>
            Loading = 0b00001000,
            /// <summary>
            /// Default state. <see cref="NextButton"/> and <see cref="BackButton"/> both are enabled.
            /// </summary>
            Clean = 0b00010000,
            /// <summary>
            /// <see cref="NextButton"/> is disabled.
            /// </summary>
            NextDisabled = 0b00100000,
            /// <summary>
            /// <see cref="BackButton"/> is hidden.
            /// </summary>
            BackHidden = 0b01000000
        }
        public WizardNavigator()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Enables/Disables <see cref="NextButton"/>
        /// </summary>
        public void ToggleNext()
        {
            NextButton.Enabled = !NextButton.Enabled;
        }
        /// <summary>
        /// Enables/Disables <see cref="BackButton"/>
        /// </summary>
        public void ToggleBack()
        {
            BackButton.Enabled = !BackButton.Enabled;
        }

        /// <summary>
        /// Updates the active state of the navigator.
        /// </summary>
        /// <param name="state"></param>
        public void UpdateState(WizardNavigatorState state)
        {
            if ((state & WizardNavigatorState.Clean) > 0)
            {
                this.Visible = true;
                NextButton.Text = "Next";
                NextButton.Enabled = true;
                NextButton.Visible = true;
                BackButton.Text = "Back";
                BackButton.Enabled = true;
                BackButton.Visible = true;
                ProgressLabel.Text = "Click \'Next\' to continue.";
            }

            if ((state & WizardNavigatorState.NextDisabled) > 0)
            {
                this.Visible = true;
                NextButton.Text = "Next";
                NextButton.Enabled = false;
                NextButton.Visible = true;
            }

            if ((state & WizardNavigatorState.BackHidden) > 0)
            {
                this.Visible = true;
                BackButton.Text = "Back";
                BackButton.Visible = false;
            }

            if ((state & WizardNavigatorState.StartEnabled) > 0)
            {
                this.Visible = true;
                NextButton.Text = "Start";
                NextButton.Enabled = true;
                NextButton.Visible = true;
                ProgressLabel.Text = "Click \'Start\' to begin.";
            }

            if ((state & WizardNavigatorState.FinishEnabled) > 0)
            { 
                this.Visible = true;
                NextButton.Text = "Finish";
                NextButton.Enabled = true;
                NextButton.Visible = true;
                ProgressLabel.Text = "Click \'Finish\' to exit the wizard.";
            }

            if ((state & WizardNavigatorState.Loading) > 0)
            {
                this.Visible = true;
                NextButton.Enabled = false;
                NextButton.Visible = false;
                BackButton.Enabled = false;
                BackButton.Visible = false;
                ProgressLabel.Text = "Preparing menu...";
            }

            if ((state & WizardNavigatorState.Hidden) > 0)
            {
                this.Visible = false;
            }
        }
    }
}
