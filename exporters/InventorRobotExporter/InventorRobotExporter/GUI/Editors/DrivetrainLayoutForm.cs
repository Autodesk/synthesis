using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using InventorRobotExporter.Managers;
using InventorRobotExporter.Utilities;

namespace InventorRobotExporter.GUI.Editors
{
    public partial class DrivetrainLayoutForm : Form
    {
        private readonly RobotDataManager robotDataManager;
        private static readonly Color SelectedColor = Color.FromArgb(239, 195, 154);
        private static readonly Color HoverColor = Color.FromArgb(239, 217, 192);
        private static readonly Color DefaultColor = SystemColors.Control;

        private readonly Dictionary<RigidNode_Base.DriveTrainType, Control> controls = new Dictionary<RigidNode_Base.DriveTrainType, Control>();
        private RigidNode_Base.DriveTrainType driveTrainType;

        public DrivetrainLayoutForm(RobotDataManager robotDataManager)
        {
            this.robotDataManager = robotDataManager;
            InitializeComponent();

            controls.Add(RigidNode_Base.DriveTrainType.TANK, tankOption);
            controls.Add(RigidNode_Base.DriveTrainType.H_DRIVE, hdriveOption);
            controls.Add(RigidNode_Base.DriveTrainType.CUSTOM, customOption);

            foreach (var keyValuePair in controls)
            {
                WinFormsUtils.RecursiveControlNavigator(keyValuePair.Value, control => control.MouseDown += (sender, args) => SelectType(keyValuePair.Key));
                WinFormsUtils.RecursiveControlNavigator(keyValuePair.Value, control => control.MouseEnter += (sender, args) =>
                {
                    if (driveTrainType != keyValuePair.Key)
                        keyValuePair.Value.BackColor = HoverColor;
                });
                WinFormsUtils.RecursiveControlNavigator(keyValuePair.Value, control => control.MouseLeave += (sender, args) =>
                {
                    if (driveTrainType != keyValuePair.Key)
                        keyValuePair.Value.BackColor = DefaultColor;
                });
            }
            
            
            SelectType(robotDataManager.RobotBaseNode.driveTrainType);
        }

        private void SelectType(RigidNode_Base.DriveTrainType type)
        {
            driveTrainType = type;
            foreach (var keyValuePair in controls)
            {
                keyValuePair.Value.BackColor = DefaultColor;
            }

            if (controls.TryGetValue(type, out var value))
            {
                value.BackColor = SelectedColor;
            }
        }
        private void OkButton_Click(object sender, EventArgs e)
        {
            robotDataManager.RobotBaseNode.driveTrainType = driveTrainType;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
