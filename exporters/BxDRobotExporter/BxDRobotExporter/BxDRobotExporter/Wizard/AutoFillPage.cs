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
                var wheelsRaw = WizardUtilities.DetectWheels(Utilities.GUI.SkeletonBase, wheelsPage.DriveTrain, (int)WheelUpDown.Value); //finds wheels
                var wheelsSorted = WizardUtilities.SortWheels(wheelsRaw);                                                    // sorts wheels left/right

                List<WizardData.WheelSetupData> oneClickWheels = new List<WizardData.WheelSetupData>(); //variable set for later

                switch (wheelsPage.DriveTrain) 
                {
                    default:

                    case WizardData.WizardDriveTrain.TANK:  //sets wheel information for TANK drive

                        for (int i = 0; i < (wheelsRaw.Count / 2); i++)
                        {
                            oneClickWheels.Add(new WizardData.WheelSetupData
                            {
                                Node = wheelsSorted[0][i],  //specifies wheel being edited
                                FrictionLevel = WizardData.WizardFrictionLevel.MEDIUM, //sets friction for wheel
                                PWMPort = 0x01,   //sets PWMPort for wheel
                                WheelType = WizardData.WizardWheelType.NORMAL   //sets the type of wheel that the wheel is (based on drivetrain chosen not inventor)
                            });
                            oneClickWheels.Add(new WizardData.WheelSetupData
                            {
                                Node = wheelsSorted[1][i],
                                FrictionLevel = WizardData.WizardFrictionLevel.MEDIUM,
                                PWMPort = 0x02,
                                WheelType = WizardData.WizardWheelType.NORMAL
                            });
                        }
                        foreach (var wheelData in oneClickWheels)
                        {
                            wheelData.ApplyToNode(); //assigns the data to the wheel
                        }
                        break;

                    case WizardData.WizardDriveTrain.MECANUM:  //sets wheel information for MECANUM drive

                        for (int i = 0; i < (wheelsRaw.Count / 2); i++)
                        {
                            oneClickWheels.Add(new WizardData.WheelSetupData
                            {
                                Node = wheelsSorted[0][i],
                                FrictionLevel = WizardData.WizardFrictionLevel.MEDIUM,
                                PWMPort = 0x01,
                                WheelType = WizardData.WizardWheelType.MECANUM
                            });
                            oneClickWheels.Add(new WizardData.WheelSetupData
                            {
                                Node = wheelsSorted[1][i],
                                FrictionLevel = WizardData.WizardFrictionLevel.MEDIUM,
                                PWMPort = 0x02,
                                WheelType = WizardData.WizardWheelType.MECANUM
                            });
                        }
                        foreach (var wheelData in oneClickWheels)
                        {
                            wheelData.ApplyToNode();
                        }
                        break;

                    case WizardData.WizardDriveTrain.SWERVE:   //sets wheel information for SWERVE drive

                        for (int i = 0; i < (wheelsRaw.Count / 2); i++)
                        {
                            oneClickWheels.Add(new WizardData.WheelSetupData
                            {
                                Node = wheelsSorted[0][i],
                                FrictionLevel = WizardData.WizardFrictionLevel.LOW,
                                PWMPort = 0x01,
                                WheelType = WizardData.WizardWheelType.NORMAL
                            });
                            oneClickWheels.Add(new WizardData.WheelSetupData
                            {
                                Node = wheelsSorted[1][i],
                                FrictionLevel = WizardData.WizardFrictionLevel.LOW,
                                PWMPort = 0x02,
                                WheelType = WizardData.WizardWheelType.NORMAL
                            });
                        }
                        foreach (var wheelData in oneClickWheels)
                        {
                            wheelData.ApplyToNode();
                        }
                        break;

                    case WizardData.WizardDriveTrain.H_DRIVE:    //sets wheel information for H-DRIVE

                        for (int i = 0; i < (wheelsRaw.Count / 2); i++)
                        {
                            oneClickWheels.Add(new WizardData.WheelSetupData
                            {
                                Node = wheelsSorted[0][i],
                                FrictionLevel = WizardData.WizardFrictionLevel.LOW,
                                PWMPort = 0x01,
                                WheelType = WizardData.WizardWheelType.NORMAL
                            });
                            oneClickWheels.Add(new WizardData.WheelSetupData
                            {
                                Node = wheelsSorted[1][i],
                                FrictionLevel = WizardData.WizardFrictionLevel.LOW,
                                PWMPort = 0x02,
                                WheelType = WizardData.WizardWheelType.NORMAL
                            });
                        }

                        //5th wheel
                        oneClickWheels.Add(new WizardData.WheelSetupData
                        {
                            Node = wheelsSorted[4][5],
                            FrictionLevel = WizardData.WizardFrictionLevel.LOW,
                            PWMPort = 0x03,
                            WheelType = WizardData.WizardWheelType.OMNI
                        });
                        foreach (var wheelData in oneClickWheels)
                        {
                            wheelData.ApplyToNode();
                        }
                        break;

                        /// <summary>
                        /// Defines nodes, friction values, PWM ports used, and wheel type for selectible AutoFill drivetrains
                        /// </summary>
                }
            }
        }

        
    }
}
