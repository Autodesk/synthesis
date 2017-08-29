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

        public WizardForm()
        {
            InitializeComponent();

            BXDJSkeleton.SetupFileNames(Utilities.GUI.SkeletonBase, true);
            
            this.Resize += WizardForm_Resize;

            //Start page
            StartPage startPage = new StartPage();
            startPage.ActivateNext += ActivateNext;
            startPage.DeactivateNext += DeactivateNext;
            WizardPages.Add(startPage, WizardNavigator.WizardNavigatorState.StartEnabled);

            //Step 1: Basic Robot Information
            BasicRobotInfoPage basicRobotInfoPage = new BasicRobotInfoPage();
            basicRobotInfoPage.ActivateNext += ActivateNext;
            basicRobotInfoPage.DeactivateNext += DeactivateNext;
            WizardPages.Add(basicRobotInfoPage, WizardNavigator.WizardNavigatorState.NextDisabled);
            
            //Step 2: Define Wheels
            DefineWheelsPage defineWheelsPage = new DefineWheelsPage();
            defineWheelsPage.ActivateNext += ActivateNext;
            defineWheelsPage.DeactivateNext += DeactivateNext;
            WizardPages.Add(defineWheelsPage, WizardNavigator.WizardNavigatorState.Clean);

            //Step 3: Define other moving parts
            DefineMovingPartsPage defineMovingPartsPage = new DefineMovingPartsPage();
            defineMovingPartsPage.ActivateNext += ActivateNext;
            defineMovingPartsPage.DeactivateNext += DeactivateNext;
            WizardPages.Add(defineMovingPartsPage, WizardNavigator.WizardNavigatorState.Clean);

            //Step 4: Review and finish
            ReviewAndFinishPage reviewAndFinishPage = new ReviewAndFinishPage();
            reviewAndFinishPage.ActivateNext += ActivateNext;
            reviewAndFinishPage.DeactivateNext += DeactivateNext;
            WizardPages.Add(reviewAndFinishPage, WizardNavigator.WizardNavigatorState.FinishEnabled);

            WizardPages.BeginWizard();
            WizardPages.FinishClicked += delegate ()
            {
                WizardData.Instance.Apply();
                Utilities.GUI.ReloadPanels();
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


    }
}