using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace BxDRobotExporter.Wizard
{
    /// <summary>
    /// <see cref="Form"/> shown when the user selects "One Click Export". Prompts the user to set the number of wheels and the drive train of their robot and hopefully does the work from there.
    /// </summary>
    public partial class OneClickExportForm : Form
    {
        /// <summary>
        /// Dictionary associating field names with field paths
        /// </summary>
        private Dictionary<string, string> fields = new Dictionary<string, string>();

        public OneClickExportForm()
        {
            InitializeComponent();
            DriveTrainComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// The drive train to export for. Currently only supports <see cref="WizardData.WizardDriveTrain.WESTERN"/> and <see cref="WizardData.WizardDriveTrain.MECANUM"/>
        /// </summary>
        private WizardData.WizardDriveTrain DriveTrain
        {
            get
            {
                switch(DriveTrainComboBox.SelectedIndex)
                {
                    default:
                    case 0:
                        return WizardData.WizardDriveTrain.WESTERN;
                    case 1:
                        return WizardData.WizardDriveTrain.MECANUM;
                    case 2:
                        return WizardData.WizardDriveTrain.SWERVE;
                }
            }
        }

        /// <summary>
        /// Detects all of the fields in the default directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OneClickExportForm_Load(object sender, EventArgs e)
        {
            var dirs = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\synthesis\fields");
            foreach (var dir in dirs)
            {
                fields.Add(dir.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last(), dir);
                FieldSelectComboBox.Items.Add(dir.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last());
            }
        }

        private void LaunchSynthesisCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            FieldSelectComboBox.Enabled = LaunchSynthesisCheckBox.Checked;
        }

        private void CancelExportButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Exports the joints and meshes, prompts for a name, detects the wheels, sets the wheel properties, and merges other, unused nodes into their parents.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportButton_Click(object sender, EventArgs e)
        {
            Hide();
            if (Utilities.GUI.ExportMeshes())
            {
                var wheelsRaw = WizardUtilities.DetectWheels(Utilities.GUI.SkeletonBase, DriveTrain, (int)WheelCountUpDown.Value);
                var wheelsSorted = WizardUtilities.SortWheels(wheelsRaw);
                switch (DriveTrain)
                {
                    default:
                    case WizardData.WizardDriveTrain.WESTERN:
                        List<WizardData.WheelSetupData> oneClickWheels = new List<WizardData.WheelSetupData>();
                        for (int i = 0; i < (wheelsRaw.Count / 2); i++)
                        {
                            oneClickWheels.Add(new WizardData.WheelSetupData
                            {
                                Node = wheelsSorted[0][i],
                                FrictionLevel = WizardData.WizardFrictionLevel.MEDIUM,
                                PWMPort = 0x01,
                                WheelType = WizardData.WizardWheelType.NORMAL
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
                            wheelData.ApplyToNode();
                        }
                        break;
                    case WizardData.WizardDriveTrain.MECANUM:
                        oneClickWheels = new List<WizardData.WheelSetupData>();
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
                }
                if(MergeNodesCheckBox.Checked)
                {
                    foreach(var node in Utilities.GUI.SkeletonBase.ListAllNodes())
                    {
                        if (!wheelsRaw.Contains(node) && node.GetParent() != null)
                            Utilities.GUI.MergeNodeIntoParent(node);
                    }
                }
                Utilities.GUI.ReloadPanels();
                Utilities.GUI.RobotSave();
                DialogResult = DialogResult.OK;
                Process.Start(Utilities.SYTHESIS_PATH, string.Format("-robot \"{0}\" -field \"{1}\"", Properties.Settings.Default.SaveLocation + "\\" + Utilities.GUI.RMeta.ActiveRobotName, fields[(string)FieldSelectComboBox.SelectedItem]));

            }
            else
                DialogResult = DialogResult.None;
            Close();
        }
        
        private void DriveTrainComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(DriveTrainComboBox.SelectedIndex)
            {
                case 0:
                    WheelCountUpDown.Minimum = 4;
                    WheelCountUpDown.Maximum = 8;
                    WheelCountUpDown.Value = 6;
                    break;
                case 1:
                case 2:
                    WheelCountUpDown.Minimum = 4;
                    WheelCountUpDown.Maximum = 4;
                    WheelCountUpDown.Value = 4;
                    break;

            }
        }
    }
}