using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace JointResolver.EditorsLibrary
{
    public partial class DriveTrainTypeForm : Form
    {
        private static readonly Color SelectedColor = Color.FromArgb(239, 195, 154);
        private static readonly Color HoverColor = Color.FromArgb(239, 217, 192);
        private static readonly Color DefaultColor = SystemColors.Control;

        private readonly Dictionary<RigidNode_Base.DriveTrainType, Control> controls = new Dictionary<RigidNode_Base.DriveTrainType, Control>();
        private RigidNode_Base.DriveTrainType driveTrainType;

        public DriveTrainTypeForm()
        {
            InitializeComponent();

            controls.Add(RigidNode_Base.DriveTrainType.TANK, tankOption);
            controls.Add(RigidNode_Base.DriveTrainType.H_DRIVE, hdriveOption);
            controls.Add(RigidNode_Base.DriveTrainType.CUSTOM, customOption);

            foreach (var keyValuePair in controls)
            {
                RecursiveControlNavigator(keyValuePair.Value, control => control.MouseDown += (sender, args) => SelectType(keyValuePair.Key));
                RecursiveControlNavigator(keyValuePair.Value, control => control.MouseEnter += (sender, args) =>
                {
                    if (driveTrainType != keyValuePair.Key)
                        keyValuePair.Value.BackColor = HoverColor;
                });
                RecursiveControlNavigator(keyValuePair.Value, control => control.MouseLeave += (sender, args) =>
                {
                    if (driveTrainType != keyValuePair.Key)
                        keyValuePair.Value.BackColor = DefaultColor;
                });
            }
            
            
            SelectType(SynthesisGUI.Instance.SkeletonBase.driveTrainType);
        }

        private void RecursiveControlNavigator(Control control, Action<Control> action)
        {
            action.Invoke(control);
            foreach (Control subControl in control.Controls)
            {
                RecursiveControlNavigator(subControl, action);
            }
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
        private void BtnOk_Click(object sender, EventArgs e)
        {
            SynthesisGUI.Instance.SkeletonBase.driveTrainType = driveTrainType;
            Close();
        }
    }
}
