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
using FieldExporter.Components;
using FieldExporter.Forms;

namespace FieldExporter.Controls
{
    public partial class ComponentPropertiesForm : UserControl
    {
        /// <summary>
        /// The parent ComponentPropertiesTabPage.
        /// </summary>
        private ComponentPropertiesTabPage parentTabPage;

        /// <summary>
        /// The events caused by user interaction with Inventor.
        /// </summary>
        private InteractionEvents interactionEvents;

        /// <summary>
        /// The events triggered by object selection in Inventor
        /// </summary>
        private SelectEvents selectEvents;

        /// <summary>
        /// Used to determine if Inventor interaction is enabled.
        /// </summary>
        public bool interactionEnabled
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the component.
        /// </summary>
        public ComponentPropertiesForm(ComponentPropertiesTabPage tabPage)
        {
            InitializeComponent();

            Dock = DockStyle.Fill;

            parentTabPage = tabPage;

            colliderTypeCombobox.SelectedIndex = 0;

            interactionEnabled = false;
        }

        /// <summary>
        /// Returns the selected type of collision.
        /// </summary>
        /// <returns></returns>
        public PhysicsGroupCollisionType GetCollisionType()
        {
            switch (colliderTypeCombobox.SelectedIndex)
            {
                case 0:
                    return PhysicsGroupCollisionType.MESH;
                case 1:
                    return PhysicsGroupCollisionType.BOX;
                default:
                    return PhysicsGroupCollisionType.NONE;
            }
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
        public double GetMass()
        {
            return Decimal.ToDouble(massNumericUpDown.Value);
        }

        /// <summary>
        /// Enables interaction events.
        /// </summary>
        private void EnableInteractionEvents()
        {
            try
            {
                interactionEvents = Program.INVENTOR_APPLICATION.CommandManager.CreateInteractionEvents();
                interactionEvents.OnActivate += interactionEvents_OnActivate;
                interactionEvents.Start();

                inventorSelectButton.Text = "Cancel Selection";

                interactionEnabled = true;
            }
            catch
            {
                MessageBox.Show("Cannot enter select mode.", "Document not found.");
            }
        }

        /// <summary>
        /// Disables interaction events.
        /// </summary>
        private void DisableInteractionEvents()
        {
            interactionEvents.Stop();

            Program.INVENTOR_APPLICATION.ActiveDocument.SelectSet.Clear();

            inventorSelectButton.Text = "Select in Inventor";
            addSelectionButton.Enabled = false;

            interactionEnabled = false;
        }

        /// <summary>
        /// Enables select events when interaction events are activated.
        /// </summary>
        void interactionEvents_OnActivate()
        {
            selectEvents = interactionEvents.SelectEvents;
            selectEvents.AddSelectionFilter(SelectionFilterEnum.kAssemblyOccurrenceFilter);
            selectEvents.OnSelect += selectEvents_OnSelect;
        }

        /// <summary>
        /// Enables the "Add Selection" button when an object in Inventor is selected.
        /// </summary>
        /// <param name="JustSelectedEntities"></param>
        /// <param name="SelectionDevice"></param>
        /// <param name="ModelPosition"></param>
        /// <param name="ViewPosition"></param>
        /// <param name="View"></param>
        void selectEvents_OnSelect(ObjectsEnumerator JustSelectedEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
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
            if (interactionEnabled)
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
            Program.INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = true;

            addSelectionButton.Enabled = false;
            inventorSelectButton.Enabled = false;

            Program.progressWindow = new ProgressWindow(this, "Adding Selection...", "Processing...",
                0, selectEvents.SelectedEntities.Count,
                new Action(() =>
                    {
                        DialogResult permanentChoice = DialogResult.None;

                        for (int i = 0; i < selectEvents.SelectedEntities.Count; i++)
                        {
                            if (Program.progressWindow.currentState.Equals(ProgressWindow.ProcessState.CANCELLED))
                                return;

                            Program.progressWindow.SetProgress(i, "Processing: " + (Math.Round((i / (float)selectEvents.SelectedEntities.Count) * 100.0f, 2)).ToString() + "%");

                            if (parentTabPage.parentControl.NodeExists(selectEvents.SelectedEntities[i + 1].Name, parentTabPage))
                            {
                                Invoke(new Action(() =>
                                    {
                                        switch (permanentChoice)
                                        {
                                            case DialogResult.None:
                                                ConfirmMoveDialog confirmDialog = new ConfirmMoveDialog(
                                                    selectEvents.SelectedEntities[i + 1].Name + " has already been added to another PhysicsGroup. Move " +
                                                    selectEvents.SelectedEntities[i + 1].Name + " to " + parentTabPage.Name + "?");

                                                DialogResult result = confirmDialog.ShowDialog(Program.progressWindow);

                                                if (result == DialogResult.OK)
                                                {
                                                    parentTabPage.parentControl.RemoveNode(selectEvents.SelectedEntities[i + 1].Name, parentTabPage);
                                                    inventorTreeView.Invoke(new Action(() =>
                                                    {
                                                        inventorTreeView.AddComponent(selectEvents.SelectedEntities[i + 1]);
                                                    }));
                                                }

                                                if (confirmDialog.futureCheckBox.Checked)
                                                {
                                                    permanentChoice = result;
                                                }
                                                break;
                                            case DialogResult.OK:
                                                parentTabPage.parentControl.RemoveNode(selectEvents.SelectedEntities[i + 1].Name, parentTabPage);
                                                    inventorTreeView.Invoke(new Action(() =>
                                                    {
                                                        inventorTreeView.AddComponent(selectEvents.SelectedEntities[i + 1]);
                                                    }));
                                                break;
                                        }
                                        
                                    }));
                            }
                            else
                            {
                                inventorTreeView.Invoke(new Action(() =>
                                    {
                                        inventorTreeView.AddComponent(selectEvents.SelectedEntities[i + 1]);
                                    }));
                            }
                            
                        }
                    }),
                new Action(() =>
                    {
                        Program.INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = false;

                        DisableInteractionEvents();
                        inventorSelectButton.Enabled = true;
                    }));

            Program.progressWindow.StartProcess();
        }

        /// <summary>
        /// Removes the tab when the remove button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove \"" + parentTabPage.Name + "\"?", "Dangerous operation.", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                parentTabPage.Remove();
            }
        }

        /// <summary>
        /// Allows for changing the name of the PhysicsGroup.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void changeNameButton_Click(object sender, EventArgs e)
        {
            parentTabPage.ChangeName();
        }

        /// <summary>
        /// Changes the friction label when the friction trackbar's value changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frictionTrackBar_Scroll(object sender, EventArgs e)
        {
            frictionLabel.Text = "Friction:\n" + frictionTrackBar.Value + "/10";
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
    }
}
