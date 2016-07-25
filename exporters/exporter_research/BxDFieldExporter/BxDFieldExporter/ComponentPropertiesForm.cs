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
        FieldDataType field;
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
                    field.colliderType = ColliderType.Box;
                    selectedType = typeof(BoxColliderPropertiesForm);
                    break;
                case 1: // Sphere
                    field.colliderType = ColliderType.Sphere;
                    selectedType = typeof(SphereColliderPropertiesForm);
                    break;
                case 2: // Mesh
                    field.colliderType = ColliderType.Mesh;
                    selectedType = typeof(MeshColliderPropertiesForm);
                    break;
            }

            if (meshPropertiesTable.Controls.Count > 1)
            {
                if (selectedType == null || meshPropertiesTable.Controls[1].GetType().Equals(selectedType))
                    return;

                meshPropertiesTable.Controls.RemoveAt(1);
            }
            UserControl controller = ((UserControl)Activator.CreateInstance(selectedType));
            meshPropertiesTable.Controls.Add(controller, 0, 1);
            if(field.colliderType == ColliderType.Sphere)
            {
                ((SphereColliderPropertiesForm)controller).readFromData(field);
            } else if(field.colliderType == ColliderType.Mesh)
            {
                ((MeshColliderPropertiesForm)controller).readFromData(field);
            } else
            {
                ((BoxColliderPropertiesForm)controller).readFromData(field);
            }
        }
        public void MassChanged(object sender, EventArgs e)
        {
            field.Mass = (double) massNumericUpDown.Value;
        }
        private void UpdateFrictionLabel()
        {
            frictionLabel.Text = "Friction:\n" + frictionTrackBar.Value + "/100";
            field.Friction = (double) frictionTrackBar.Value;
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
                field.Dynamic = true;
            }
            else
            {
                dynamicGroupBox.Enabled = false;
                massNumericUpDown.Value = 0;
                field.Dynamic = false;
            }
        }
        public void readFromData(FieldDataType d)
        {
            field = d;
            if (field.colliderType == ColliderType.Sphere)
            {
                colliderTypeCombobox.SelectedIndex = 1;
            } else if (field.colliderType == ColliderType.Mesh)
            {
                colliderTypeCombobox.SelectedIndex = 2;
            } else
            {
                colliderTypeCombobox.SelectedIndex = 0;
            }
            if (field.Dynamic)
            {
                dynamicGroupBox.Enabled = true;
            }
            else
            {
                dynamicGroupBox.Enabled = false;
                massNumericUpDown.Value = 0;
            }
            frictionTrackBar.Value = (int) field.Friction;
            massNumericUpDown.Value = (decimal)field.Mass;
        }
    }
}

