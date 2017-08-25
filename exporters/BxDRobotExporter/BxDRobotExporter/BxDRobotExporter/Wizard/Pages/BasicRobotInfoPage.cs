using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BxDRobotExporter.Wizard;

namespace BxDRobotExporter.Wizard
{
    public partial class BasicRobotInfoPage : UserControl, IWizardPage
    {
        private float totalMass = 0.0f;
        public BasicRobotInfoPage()
        {
            InitializeComponent();

            MassModeDropdown.SelectedIndex = 0;
            MassModeDropdown_SelectedIndexChanged(null, null);
            TeamNumberTextBox.Enabled = false;
            DriveTrainDropdown.SelectedIndex = 0;

            RobotNameTextBox.Text = Utilities.GUI.RMeta.ActiveRobotName;

            WheelCountUpDown.Maximum = Utilities.GUI.SkeletonBase.ListAllNodes().Count - 1;

            this.VisibleChanged += delegate (object sender, EventArgs e)
            {
                ValidateInput();
            };
        }

        private void AnalyticsCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ValidateInput();
        }

        private void FRCRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            TeamNumberTextBox.Enabled = true;
            TeamNumberTextBox.Mask = "0000";

            ValidateInput();
        }

        private void FTCRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            TeamNumberTextBox.Enabled = true;
            TeamNumberTextBox.Mask = "00000";

            ValidateInput();
        }

        private void MassModeDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (MassModeDropdown.SelectedIndex)
            {
                case 0:
                    ClearPanel();
                    totalMass = 0.0f;

                    UpdateMassCount();
                    this.AddPanelRow("Robot", null);

                    MassPropertyTitleLabel.Text = "Selected Mode: Single User Defined (Recommended)";
                    MassPropertyInfoLabel.Text = "In this mode, you input the mass of your robot and the exporter will distribute the mass evenly throughout the robot. Slightly less accurate than the Complex User Defined mode, but far easier.";

                    break;
                case 1:
                    ClearPanel();
                    totalMass = 0.0f;
                    UpdateMassCount();

                    int i = 0;
                    foreach (RigidNode_Base node in Utilities.GUI.SkeletonBase.ListAllNodes())
                    {
                        this.AddPanelRow("node_" + i.ToString() + ".bxda", node);
                        i++;
                    }

                    MassPropertyTitleLabel.Text = "Selected Mode: Complex User Defined";
                    MassPropertyInfoLabel.Text = "In this mode, you manually set the masses of each node. Recommended for maximum accuracy.";

                    break;
                case 2:
                    ClearPanel();
                    totalMass = 0.0f;
                    UpdateMassCount();

                    MassPropertyTitleLabel.Text = "Selected Mode: Materials Based";
                    MassPropertyInfoLabel.Text = "In this mode, the mass of the exporter is implied from the material of each part.";

                    //Do materials math calculation
                    totalMass = 0.0f;
                    foreach(BXDAMesh mesh in Utilities.GUI.Meshes)
                    {
                        totalMass += mesh.physics.mass;
                    }
                    if(!MetricCheckBox.Checked)
                    {
                        totalMass *= 2.20462f;
                    }

                    TotalMassLabel.Text = string.Format("Total Mass: {0} {1}", (decimal)totalMass, (MetricCheckBox.Checked) ? "kg" : "lbs");
                    break;
            }

            ValidateInput();
        }

        private void AddPanelRow(string name, RigidNode_Base node)
        {
            MassDataRow row = new MassDataRow(name, node);
            row.Width = MassPanel.Width - 17;
            row.Location = new Point(0, 20 * MassPanel.Controls.Count);
            row.MassChanged += delegate (decimal amount)
            {
                totalMass += (float)amount;
                UpdateMassCount();
            };

            MassPanel.Controls.Add(row);
        }

        private void ClearPanel()
        {
            while (MassPanel.Controls.Count > 0)
                MassPanel.Controls[0].Dispose();
        }

        private void UpdateMassCount()
        {
            TotalMassLabel.Text = string.Format("Total Mass: {0} {1}", totalMass, (MetricCheckBox.Checked) ? "kg" : "lbs");
            ValidateInput();
        }

        private void MetricCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if(MassModeDropdown.SelectedIndex == 2)
            {
                totalMass = 0.0f;
                foreach (BXDAMesh mesh in Utilities.GUI.Meshes)
                {
                    totalMass += mesh.physics.mass;
                }
                if (!MetricCheckBox.Checked)
                {
                    totalMass *= 2.20462f;
                }

                TotalMassLabel.Text = string.Format("Total Mass: {0} {1}", (decimal)totalMass, (MetricCheckBox.Checked) ? "kg" : "lbs");
            }
            else
                UpdateMassCount();
        }

        private void ValidateInput()
        {
            if (!string.IsNullOrEmpty(RobotNameTextBox.Text) && DriveTrainDropdown.SelectedIndex != 0 && totalMass != 0.0f)
                OnActivateNext();
            else
                OnDeactivateNext();
        }

        private void RobotNameTextBox_TextChanged(object sender, EventArgs e)
        {
            ValidateInput();
        }
        
        private void DriveTrainDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(DriveTrainDropdown.SelectedIndex)
            {
                case 0: //Undefined
                    this.WheelCountUpDown.Minimum = 1;
                    this.WheelCountUpDown.Increment = 1;
                    this.WheelCountUpDown.Maximum = Utilities.GUI.SkeletonBase.ListAllNodes().Count - 1;
                    break;
                case 1: //Western
                    this.WheelCountUpDown.Maximum = 8;
                    this.WheelCountUpDown.Minimum = 4;
                    this.WheelCountUpDown.Value = 6;
                    this.WheelCountUpDown.Increment = 2;
                    break;
                case 2: //Mecanum
                    this.WheelCountUpDown.Minimum = 4;
                    this.WheelCountUpDown.Maximum = 4;
                    this.WheelCountUpDown.Value = 4;
                    break;
                case 3: //Swerve
                    this.WheelCountUpDown.Minimum = 4;
                    this.WheelCountUpDown.Maximum = 4;
                    this.WheelCountUpDown.Value = 4;
                    break;
                case 4: //H-Drive
                    this.WheelCountUpDown.Minimum = 5;
                    this.WheelCountUpDown.Maximum = 5;
                    this.WheelCountUpDown.Value = 5;
                    break;
                case 5: //Custom
                    this.WheelCountUpDown.Minimum = 1;
                    this.WheelCountUpDown.Maximum = Utilities.GUI.SkeletonBase.ListAllNodes().Count - 1;
                    this.WheelCountUpDown.Increment = 1;
                    break;
            }
            OnInvalidatePage();
            ValidateInput();
        }
        
        #region IWizardPage Implementation
        public void Initialize() { _initialized = true; }

        public void OnNext()
        {
            WizardData.Instance.RobotName = RobotNameTextBox.Text;
            WizardData.Instance.Analytics_TeamNumber = TeamNumberTextBox.Text;
            WizardData.Instance.Analytics_TeamLeague = (FTCRadioButton.Checked) ? "FTC" : "FRC";
            WizardData.Instance.DriveTrain = (WizardData.WizardDriveTrain)DriveTrainDropdown.SelectedIndex;
            WizardData.Instance.WheelCount = (int)WheelCountUpDown.Value;
            WizardData.Instance.MassMode = (WizardData.WizardMassMode)MassModeDropdown.SelectedIndex;
            WizardData.Instance.Mass = totalMass;
            if (MassModeDropdown.SelectedIndex == 1)
            {
                WizardData.Instance.Masses = new float[Utilities.GUI.SkeletonBase.ListAllNodes().Count];
                for (int i = 0; i < WizardData.Instance.Masses.Length; i++)
                {
                    WizardData.Instance.Masses[i] = (MetricCheckBox.Checked) ? ((MassDataRow)MassPanel.Controls[i]).Value : ((MassDataRow)MassPanel.Controls[i]).Value / 2.20462f;
                }
            }
        }
        
        public event Action ActivateNext;
        private void OnActivateNext()
        {
            this.ActivateNext?.Invoke();
        }

        public event Action DeactivateNext;
        private void OnDeactivateNext()
        {
            this.DeactivateNext?.Invoke();
        }

        public event InvalidatePageEventHandler InvalidatePage;
        public void OnInvalidatePage()
        {
            InvalidatePage?.Invoke(typeof(DefineWheelsPage));
        }

        public bool Initialized { get => _initialized; set => _initialized = value; }
        private bool _initialized = false;
        #endregion
    }
}
