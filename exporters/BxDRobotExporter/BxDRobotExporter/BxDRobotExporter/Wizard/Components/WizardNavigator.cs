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
    public partial class WizardNavigator : UserControl
    {
        public enum WizardNavigatorState : byte
        {
            StartEnabled = 0b00000001,
            FinishEnabled = 0b00001000,
            Hidden = 0b00010000,
            Clean = 0b00100000,
            NextDisabled = 0b01000000
        }
        public WizardNavigator()
        {
            InitializeComponent();
        }

        public void ToggleNext()
        {
            NextButton.Enabled = !NextButton.Enabled;
        }
        public void ToggleBack()
        {
            BackButton.Enabled = !BackButton.Enabled;
        }

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
