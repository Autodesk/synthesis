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
    /// Panel at the bottom of the <see cref="WizardForm"/> used for navigating through the pages.
    /// </summary>
    public partial class WizardNavigator : UserControl
    {
        public enum WizardNavigatorState : byte
        {
            /// <summary>
            /// <see cref="NextButton"/> reads "Start >". <see cref="BackButton"/> is disabled.
            /// </summary>
            StartEnabled = 0b00000001,
            /// <summary>
            /// <see cref="NextButton"/> reads "Finish". When clicked, it invokes the <see cref="WizardPageControl.FinishClicked"/> event.
            /// </summary>
            FinishEnabled = 0b00000010,
            /// <summary>
            /// The entire control is invisible. TODO: Use this to export the meshes within the Wizard.
            /// </summary>
            Hidden = 0b00000100,
            /// <summary>
            /// Default state. <see cref="NextButton"/> and <see cref="BackButton"/> both are enabled.
            /// </summary>
            Clean = 0b0001000,
            /// <summary>
            /// Same as <see cref="WizardNavigatorState.Clean"/> except <see cref="NextButton"/> is enabled.
            /// </summary>
            NextDisabled = 0b00010000
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
            switch(state)
            {
                case WizardNavigatorState.Clean:
                    this.Visible = true;
                    NextButton.Text = "Next >";
                    NextButton.Enabled = true;
                    BackButton.Text = "< Back";
                    BackButton.Enabled = true;
                    Progress.Text = "Click \'Next\' to continue.";
                    Progress.Enabled = true;
                    break;
                case WizardNavigatorState.NextDisabled:
                    this.Visible = true;
                    NextButton.Text = "Next >";
                    NextButton.Enabled = false;
                    BackButton.Text = "< Back";
                    BackButton.Enabled = true;
                    Progress.Text = "Click \'Next\' to continue.";
                    Progress.Enabled = true;
                    break;
                case WizardNavigatorState.Hidden:
                    this.Visible = false;
                    break;
                case WizardNavigatorState.FinishEnabled:
                    this.Visible = true;
                    NextButton.Text = "Finish";
                    NextButton.Enabled = true;
                    BackButton.Text = "< Back";
                    BackButton.Enabled = true;
                    Progress.Text = "Click \'Finish\' to exit the wizard.";
                    Progress.Enabled = true;
                    break;
                case WizardNavigatorState.StartEnabled:
                    this.Visible = true;
                    NextButton.Text = "Start >";
                    NextButton.Enabled = true;
                    BackButton.Text = "< Back";
                    BackButton.Enabled = false;
                    Progress.Text = "Click \'Start\' to begin.";
                    Progress.Enabled = true;
                    break;
            }

        }
    }
}
