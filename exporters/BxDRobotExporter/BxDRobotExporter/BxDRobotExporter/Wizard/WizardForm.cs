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
                if (Properties.Settings.Default.ShowExportOrAdvancedForm)
                {
                    Form finishDialog = new ExportOrAdvancedForm();
                    finishDialog.ShowDialog();
                }
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