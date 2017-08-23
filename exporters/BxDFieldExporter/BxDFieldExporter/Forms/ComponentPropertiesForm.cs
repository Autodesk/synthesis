using System;
using System.Windows.Forms;

namespace BxDFieldExporter
{
    // form that allows the user to enter properties for the field types
    public partial class ComponentPropertiesForm : Form {
        //keeps track of whether mass is in kg or lb, kg is true, lb is false
        Boolean massMode = true;

        FieldDataComponent field;
        public ComponentPropertiesForm() {
            InitializeComponent();// inits and populates the form
            comboBox1.SelectedItem = "kgs";//sets the massMode to kgs by default
        }
        // these methods react to changes in the fields so we can save the data
        private void colliderTypeCombobox_SelectedIndexChanged(object sender, EventArgs e) {// changes the collider box based on the selection
            Type selectedType = null;// make space for a form to add to the form

            switch (colliderTypeCombobox.SelectedIndex) {// change selected form based on selection
                case 0: // Box
                    field.colliderType = ColliderType.Box;
                    selectedType = typeof(BoxColliderPropertiesForm);// sets the type to the correct form
                    Height = 400;
                    break;
                case 1: // Sphere
                    field.colliderType = ColliderType.Sphere;
                    selectedType = typeof(SphereColliderPropertiesForm);
                    Height = 325;
                    break;
                case 2: // Mesh
                    field.colliderType = ColliderType.Mesh;
                    selectedType = typeof(MeshColliderPropertiesForm);
                    Height = 325;
                    break;
            }

            if (meshPropertiesTable.Controls.Count > 1) {
                if (selectedType == null || meshPropertiesTable.Controls[1].GetType().Equals(selectedType))
                    return;// clears the form so we don't get multiple forms

                meshPropertiesTable.Controls.RemoveAt(1);
            }
            UserControl controller = ((UserControl)Activator.CreateInstance(selectedType));
            meshPropertiesTable.Controls.Add(controller, 0, 1);
            if (field.colliderType == ColliderType.Sphere)// read the data from the file into the form
            {
                ((SphereColliderPropertiesForm)controller).readFromData(field);
            }
            else if (field.colliderType == ColliderType.Mesh) {
                ((MeshColliderPropertiesForm)controller).readFromData(field);
            }
            else {
                ((BoxColliderPropertiesForm)controller).readFromData(field);
            }
        }
        public void MassChanged(object sender, EventArgs e) {

            field.Mass = (double)getMass();
        }
        private void UpdateFrictionLabel() {
            frictionLabel.Text = "Friction:\n" + frictionTrackBar.Value + "/100";
            field.Friction = (double)frictionTrackBar.Value;
        }
        private void frictionTrackBar_Scroll(object sender, EventArgs e) {
            UpdateFrictionLabel();
        }
        private void dynamicCheckBox_CheckedChanged(object sender, EventArgs e) {
            if (dynamicCheckBox.Checked) {
                dynamicGroupBox.Enabled = true;
                field.Dynamic = true;
            }
            else {
                dynamicGroupBox.Enabled = false;
                massNumericUpDown.Value = 0;
                field.Dynamic = false;
            }
        }
        public void readFromData(FieldDataComponent d) {// reads from the data so user can see the same values from the last time they entered them
            try {
                field = d;
                if (field.colliderType == ColliderType.Sphere) {
                    colliderTypeCombobox.SelectedIndex = 1;
                }
                else if (field.colliderType == ColliderType.Mesh) {
                    colliderTypeCombobox.SelectedIndex = 2;
                }
                else {
                    colliderTypeCombobox.SelectedIndex = 0;
                }
                if (field.Dynamic) {
                    dynamicGroupBox.Enabled = true;
                }
                else {
                    dynamicGroupBox.Enabled = false;
                    massNumericUpDown.Value = 0;
                }
                frictionTrackBar.Value = (int)field.Friction;
                massNumericUpDown.Value = (decimal)field.Mass;
            }
            catch (Exception e) {
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {

            if (comboBox1.Text == "kgs") {

                if (massMode == false) {

                    massNumericUpDown.Value = massNumericUpDown.Value / (decimal)2.20462;
                }
                massNumericUpDown.Maximum = (decimal)453.5;
                massMode = true;
            }
            else {
                massNumericUpDown.Maximum = (decimal)1000;
                if (massMode == true) {
                    massNumericUpDown.Value = massNumericUpDown.Value * (decimal)2.20462;
                }

                massMode = false;
            }
        }

        private decimal getMass() {
            if (!massMode) {
                return massNumericUpDown.Value / (decimal)2.20462;
            }
            else {
                return massNumericUpDown.Value;
            }
        }

        private void btnSave_Click(object sender, EventArgs e) {
            Close();
        }

        /// <summary>
        /// Override ProcessCmdKey in order to collect escape and enter key input
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();

            }
            else if (keyData == Keys.Enter)
            {
                Close();
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}