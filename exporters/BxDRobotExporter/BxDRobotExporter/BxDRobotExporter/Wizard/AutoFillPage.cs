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

        //Done Clicked
        private void StartButton_Click(object sender, EventArgs e)
        {
            DoAutoFill();

            Close();
        }

        /// <summary>
        /// Exports the joints and meshes, prompts for a name, detects the wheels, sets the wheel properties, and merges other, unused nodes into their parents.
        /// </summary>

        private void DoAutoFill()
        {
            if (wheelsPage == null)
                return;

            if (Utilities.GUI.LoadMeshes())
            {
                var wheelsRaw = WizardUtilities.DetectWheels(Utilities.GUI.SkeletonBase, wheelsPage.DriveTrain, (int)WheelUpDown.Value);
                var wheelsSorted = WizardUtilities.SortWheels(wheelsRaw);

                List<WizardData.WheelSetupData> oneClickWheels = new List<WizardData.WheelSetupData>();

                switch (wheelsPage.DriveTrain)
                {
                    default:

                    case WizardData.WizardDriveTrain.TANK:

                        for (int i = 0; i < (wheelsRaw.Count / 2); i++)
                        {
                            oneClickWheels.Add(new WizardData.WheelSetupData
                            {
                                Node = wheelsSorted[0][i],
                                FrictionLevel = WizardData.WizardFrictionLevel.MEDIUM,
                                PWMPort = 0x04,
                                WheelType = WizardData.WizardWheelType.NORMAL
                            });
                            oneClickWheels.Add(new WizardData.WheelSetupData
                            {
                                Node = wheelsSorted[1][i],
                                FrictionLevel = WizardData.WizardFrictionLevel.MEDIUM,
                                PWMPort = 0x05,
                                WheelType = WizardData.WizardWheelType.NORMAL
                            });
                        }
                        foreach (var wheelData in oneClickWheels)
                        {
                            wheelData.ApplyToNode();
                        }
                        break;

                    case WizardData.WizardDriveTrain.MECANUM:

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

                    case WizardData.WizardDriveTrain.SWERVE:

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

                    case WizardData.WizardDriveTrain.H_DRIVE:

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
