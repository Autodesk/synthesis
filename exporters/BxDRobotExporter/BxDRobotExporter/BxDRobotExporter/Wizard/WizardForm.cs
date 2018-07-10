using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BxDRobotExporter.Wizard
{
    public partial class WizardForm : Form
    {        
        WizardData data = new WizardData();

        // Reduce flickering - https://stackoverflow.com/questions/2612487/how-to-fix-the-flickering-in-user-controls
        
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED // This forces all children to use double buffering, which eliminates the flickering caused by moving elements
                cp.Style &= ~0x02000000;  // Turn off WS_CLIPCHILDREN // This prevents parts of the form from appearing black at first
                return cp;
            }
        }

        public WizardForm()
        {
            InitializeComponent();

            BXDJSkeleton.SetupFileNames(Utilities.GUI.SkeletonBase);
            
            this.Resize += WizardForm_Resize;

            //Step 1: Define Wheels
            DefineWheelsPage defineWheelsPage = new DefineWheelsPage();
            defineWheelsPage.ActivateNext += ActivateNext;
            defineWheelsPage.DeactivateNext += DeactivateNext;
            defineWheelsPage.SetEndEarly += SetEndEarly;
            WizardPages.Add(defineWheelsPage, WizardNavigator.WizardNavigatorState.Clean | WizardNavigator.WizardNavigatorState.BackHidden);

            //Step 2: Define other moving parts
            DefineMovingPartsPage defineMovingPartsPage = new DefineMovingPartsPage();
            defineMovingPartsPage.ActivateNext += ActivateNext;
            defineMovingPartsPage.DeactivateNext += DeactivateNext;
            WizardPages.Add(defineMovingPartsPage, WizardNavigator.WizardNavigatorState.Clean | WizardNavigator.WizardNavigatorState.FinishEnabled);
            
            WizardPages.BeginWizard();
            WizardPages.FinishClicked += delegate ()
            {
                WizardData.Instance.Apply();
                StandardAddInServer.Instance.PendingChanges = true;
                Close();
            };
        }

        private void WizardForm_Resize(object sender, EventArgs e)
        {
            WizardPages.Size = new Size(460, this.Size.Height - 63);
        }
        
        private void ActivateNext()
        {
            WizardPages.WizardNavigator.NextButton.Enabled = true;
        }

        private void DeactivateNext()
        {
            WizardPages.WizardNavigator.NextButton.Enabled = false;
        }

        private void SetEndEarly(bool enabled)
        {
            WizardPages.EndEarly = enabled;
        }
    }
}