using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inventor;

namespace BxDFieldExporter
{
    public partial class ComponentPropertiesForm : Form
    {
        public ComponentPropertiesForm()
        {
            InitializeComponent();
        }
        private void colliderTypeCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Type selectedType = null;

            switch (colliderTypeCombobox.SelectedIndex)
            {
                case 0: // Box
                    selectedType = typeof(BoxColliderPropertiesForm);
                    break;
                case 1: // Sphere
                    selectedType = typeof(SphereColliderPropertiesForm);
                    break;
                case 2: // Mesh
                    selectedType = typeof(MeshColliderPropertiesForm);
                    break;
            }

            if (meshPropertiesTable.Controls.Count > 1)
            {
                if (selectedType == null || meshPropertiesTable.Controls[1].GetType().Equals(selectedType))
                    return;

                meshPropertiesTable.Controls.RemoveAt(1);
            }

            meshPropertiesTable.Controls.Add((UserControl)Activator.CreateInstance(selectedType), 0, 1);
        }
        private void UpdateFrictionLabel()
        {
            frictionLabel.Text = "Friction:\n" + frictionTrackBar.Value + "/100";
        }
        private void frictionTrackBar_Scroll(object sender, EventArgs e)
        {
            UpdateFrictionLabel();
        }
        private void dynamicCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (dynamicCheckBox.Checked)
            {
                dynamicGroupBox.Enabled = true;
            }
            else
            {
                dynamicGroupBox.Enabled = false;
                massNumericUpDown.Value = 0;
            }
        }
    }
}

