using System;
using System.Windows.Forms;
using Inventor;
using FieldExporter.Components;
using FieldExporter.Forms;

namespace FieldExporter.Controls
{
    public partial class ComponentPropertiesForm : UserControl
    {
        /// <summary>
        /// The parent ComponentPropertiesTabPage.
        /// </summary>
        public ComponentPropertiesTabPage ParentTabPage
        {
            get;
            private set;
        }

        /// <summary>
        /// The events caused by user interaction with Inventor.
        /// </summary>
        public InteractionEvents InteractionEvents
        {
            get;
            private set;
        }

        /// <summary>
        /// The events triggered by object selection in Inventor.
        /// </summary>
        public SelectEvents SelectEvents
        {
            get;
            private set;
        }

        /// <summary>
        /// Used to determine if Inventor interaction is enabled.
        /// </summary>
        public bool InteractionEnabled
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new ComponentPropertiesForm instance.
        /// </summary>
        public ComponentPropertiesForm(ComponentPropertiesTabPage tabPage)
        {
            InitializeComponent();

            Dock = DockStyle.Fill;

            ParentTabPage = tabPage;

            colliderTypeCombobox.SelectedIndex = 0;

            InteractionEnabled = false;
        }

        /// <summary>
        /// Returns the selected type of collision.
        /// </summary>
        /// <returns></returns>
        public PropertySet.PropertySetCollider GetCollider()
        {
            return ((ColliderPropertiesForm)meshPropertiesTable.Controls[1]).GetCollider();
        }

        /// <summary>
        /// Returns the value of the friction track bar.
        /// </summary>
        /// <returns></returns>
        public int GetFriction()
        {
            return frictionTrackBar.Value;
        }

        /// <summary>
        /// Returns the value of the dynamic check box.
        /// </summary>
        /// <returns></returns>
        public bool IsDynamic()
        {
            return dynamicCheckBox.Checked;
        }

        /// <summary>
        /// Returns the value of the mass numeric up down.
        /// </summary>
        /// <returns></returns>
        public float GetMass()
        {
            return (float)Decimal.ToDouble(massNumericUpDown.Value);
        }

        /// <summary>
        /// Enables interaction events with Inventor.
        /// </summary>
        public void EnableInteractionEvents()
        {
            if (Program.INVENTOR_APPLICATION.ActiveDocument == Program.ASSEMBLY_DOCUMENT)
            {
                try
                {
                    InteractionEvents = Program.INVENTOR_APPLICATION.CommandManager.CreateInteractionEvents();
                    InteractionEvents.OnActivate += interactionEvents_OnActivate;
                    InteractionEvents.Start();

                    inventorSelectButton.Text = "Cancel Selection";

                    InteractionEnabled = true;
                }
                catch
                {
                    MessageBox.Show("Cannot enter select mode.", "Document not found.");
                }
            }
            else
            {
                MessageBox.Show("Can only enter select mode for " + Program.ASSEMBLY_DOCUMENT.DisplayName);
            }
        }

        /// <summary>
        /// Disables interaction events with Inventor.
        /// </summary>
        public void DisableInteractionEvents()
        {
            InteractionEvents.Stop();
            Program.ASSEMBLY_DOCUMENT.SelectSet.Clear();

            inventorSelectButton.Text = "Select in Inventor";
            addSelectionButton.Enabled = false;
            inventorSelectButton.Enabled = true;

            InteractionEnabled = false;
        }

        /// <summary>
        /// Updates the friction label with the value in the friction track bar.
        /// </summary>
        private void UpdateFrictionLabel()
        {
            frictionLabel.Text = "Friction:\n" + frictionTrackBar.Value + "/100";
        }

        /// <summary>
        /// Enables select events when interaction events are activated.
        /// </summary>
        private void interactionEvents_OnActivate()
        {
            SelectEvents = InteractionEvents.SelectEvents;
            SelectEvents.AddSelectionFilter(SelectionFilterEnum.kAssemblyOccurrenceFilter);
            SelectEvents.OnSelect += selectEvents_OnSelect;
        }

        /// <summary>
        /// Enables the "Add Selection" button when an object in Inventor is selected.
        /// </summary>
        /// <param name="JustSelectedEntities"></param>
        /// <param name="SelectionDevice"></param>
        /// <param name="ModelPosition"></param>
        /// <param name="ViewPosition"></param>
        /// <param name="View"></param>
        private void selectEvents_OnSelect(ObjectsEnumerator JustSelectedEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
        {
            if (!addSelectionButton.Enabled)
            {
                addSelectionButton.Invoke(new Action(() =>
                {
                    addSelectionButton.Enabled = true;
                }));
            }
        }

        /// <summary>
        /// Enables or disables interaction events based what the current state is.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void inventorSelectButton_Click(object sender, EventArgs e)
        {
            if (InteractionEnabled)
            {
                DisableInteractionEvents();
            }
            else
            {
                EnableInteractionEvents();
            }
        }

        /// <summary>
        /// Adds the Inventor selection to the InventorTreeView.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addSelectionButton_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            Program.LockInventor();

            addSelectionButton.Enabled = false;
            inventorSelectButton.Enabled = false;

            DialogResult permanentChoice = DialogResult.None;

            for (int i = 0; i < SelectEvents.SelectedEntities.Count; i++)
            {
                if (ParentTabPage.parentControl.NodeExists(SelectEvents.SelectedEntities[i + 1].Name, ParentTabPage))
                {
                    switch (permanentChoice)
                    {
                        case DialogResult.None:
                            ConfirmMoveDialog confirmDialog = new ConfirmMoveDialog(
                                SelectEvents.SelectedEntities[i + 1].Name + " has already been added to another PhysicsGroup. Move " +
                                SelectEvents.SelectedEntities[i + 1].Name + " to " + ParentTabPage.Name + "?");

                            DialogResult result = confirmDialog.ShowDialog(Program.MAINWINDOW);

                            if (result == DialogResult.OK)
                            {
                                ParentTabPage.parentControl.RemoveNode(SelectEvents.SelectedEntities[i + 1].Name, ParentTabPage);
                                inventorTreeView.AddComponent(SelectEvents.SelectedEntities[i + 1]);
                            }

                            if (confirmDialog.IsChecked())
                            {
                                permanentChoice = result;
                            }
                            break;
                        case DialogResult.OK:
                            ParentTabPage.parentControl.RemoveNode(SelectEvents.SelectedEntities[i + 1].Name, ParentTabPage);
                            inventorTreeView.AddComponent(SelectEvents.SelectedEntities[i + 1]);
                            break;
                    }
                }
                else
                {
                    inventorTreeView.AddComponent(SelectEvents.SelectedEntities[i + 1]);
                }
            }

            Program.UnlockInventor();

            DisableInteractionEvents();

            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// Removes the tab when the remove button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove \"" + ParentTabPage.Name + "\"?", "Dangerous operation.", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ParentTabPage.Remove();
            }
        }

        /// <summary>
        /// Allows for changing the name of the PhysicsGroup.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void changeNameButton_Click(object sender, EventArgs e)
        {
            ParentTabPage.ChangeName();
        }

        /// <summary>
        /// Updates the collider properties form when the selected collider is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Changes the friction label when the friction trackbar's value changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frictionTrackBar_Scroll(object sender, EventArgs e)
        {
            UpdateFrictionLabel();
        }

        /// <summary>
        /// Enables or disables the DynamicGroupBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Allows the user to enter an exact value for the friction.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frictionLabel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            EnterFrictionDialog frictionDialog = new EnterFrictionDialog(frictionTrackBar.Value);

            if (frictionDialog.ShowDialog() == DialogResult.OK)
            {
                frictionTrackBar.Value = frictionDialog.Friction;
                UpdateFrictionLabel();
            }
        }
    }
}
