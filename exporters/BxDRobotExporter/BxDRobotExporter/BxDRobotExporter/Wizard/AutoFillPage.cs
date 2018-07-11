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
    public partial class AutoFillPage : Form
    {
        private DefineWheelsPage wheelsPage = null;

        public AutoFillPage(DefineWheelsPage wheelsPage)
        {
            InitializeComponent();
            this.wheelsPage = wheelsPage;
        }

        //When Start Clicked
        private void StartButton_Click(object sender, EventArgs e)
        {
            DoAutoFill(); //Initializes autofill
            
            Close(); //closes popup
        }

        /// <summary>
        /// Exports the joints and meshes, prompts for a name, detects the wheels, sets the wheel properties, and merges other, unused nodes into their parents.
        /// </summary>

        private void DoAutoFill() //runs autofill
        {
            if (wheelsPage == null) //terminates autofill if DefineWheelsPage is not present
                return;

            if (Utilities.GUI.LoadMeshes()) //loads wheel meshes
            {
                var wheelsRaw = WizardUtilities.DetectWheels(Utilities.GUI.SkeletonBase, WizardData.Instance.driveTrain, (int)WheelUpDown.Value); //finds wheels
                var wheelsSorted = WizardUtilities.SortWheels(wheelsRaw);                                                    // sorts wheels left/right

                List<WizardData.WheelSetupData> oneClickWheels = new List<WizardData.WheelSetupData>(); //variable set for later
                
                for (int i = 0; i < (wheelsRaw.Count / 2); i++)
                {
                    wheelsPage.SetWheelSide(wheelsSorted[0][i], WheelSide.LEFT, false); 
                    wheelsPage.SetWheelSide(wheelsSorted[1][i], WheelSide.RIGHT, i >= (wheelsRaw.Count / 2 - 1)); //updates UI on last item
                }

                if (WizardData.Instance.driveTrain == WizardData.WizardDriveTrain.H_DRIVE)
                {
                    //TODO: Imliment HDRIVE
                }
            }
        }
    }
}
