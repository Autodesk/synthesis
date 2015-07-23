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

            parentTabPage = tabPage;

            nameLabel.Text = "Name: " + tabPage.Name;

            colliderTypeCombobox.SelectedIndex = 0;

            interactionEnabled = false;
        }

        /// <summary>
        /// Returns the selected type of collision.
        /// </summary>
        /// <returns></returns>
        public FieldNodeCollisionType GetCollisionType()
        {
            switch (colliderTypeCombobox.SelectedIndex)
            {
                case 0:
                    return FieldNodeCollisionType.MESH;
                case 1:
                    return FieldNodeCollisionType.BOX;
                default:
                    return FieldNodeCollisionType.NONE;
            }
        }

        /// <summary>
        /// Returns true if convex is selected.
        /// </summary>
        /// <returns></returns>
        public bool IsConvex()
        {
            return convexCheckBox.Checked;
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
        /// Enables interaction events.
        /// </summary>
        private void EnableInteractionEvents()
        {
            try
            {
                interactionEvents = Program.INVENTOR_APPLICATION.CommandManager.CreateInteractionEvents();
                interactionEvents.OnActivate += interactionEvents_OnActivate;
                interactionEvents.Start();

                inventorTreeView.HotTracking = false;

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

            inventorTreeView.HotTracking = true;

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
            selectEvents.OnPreSelect += selectEvents_OnPreSelect;
        }

        /// <summary>
        /// Allows the user to see if they have already added a collision component in select mode.
        /// </summary>
        /// <param name="PreSelectEntity"></param>
        /// <param name="DoHighlight"></param>
        /// <param name="MorePreSelectEntities"></param>
        /// <param name="SelectionDevice"></param>
        /// <param name="ModelPosition"></param>
        /// <param name="ViewPosition"></param>
        /// <param name="View"></param>
        void selectEvents_OnPreSelect(ref object PreSelectEntity, out bool DoHighlight, ref ObjectCollection MorePreSelectEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
        {
            DoHighlight = true;

            if (PreSelectEntity is ComponentOccurrence)
            {
                ComponentOccurrence componentOccurrence = (ComponentOccurrence)PreSelectEntity;

                inventorTreeView.Invoke(new Action(() =>
                    {
                        inventorTreeView.SelectByComponent(componentOccurrence);
                    }));
            }
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
            EnterNameDialog nameDialog = new EnterNameDialog();

            if (nameDialog.ShowDialog(this).Equals(DialogResult.OK))
            {
                if (!parentTabPage.Name.Equals(nameDialog.nameTextBox.Text))
                {
                    if (parentTabPage.parentControl.TabPages.ContainsKey(nameDialog.nameTextBox.Text))
                    {
                        MessageBox.Show("Name is already taken.", "Invalid name.");
                    }
                    else
                    {
                        parentTabPage.SetName(nameDialog.nameTextBox.Text);
                        nameLabel.Text = "Name: " + nameDialog.nameTextBox.Text;
                    }
                }
            }
        }

        /// <summary>
        /// Changes the friction label when the friction trackbar's value changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frictionTrackBar_Scroll(object sender, EventArgs e)
        {
            frictionLabel.Text = "Friction:\n" + frictionTrackBar.Value;
        }

        /// <summary>
        /// Enables or disables the convex check box depending on the collider type.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void colliderTypeCombobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (colliderTypeCombobox.SelectedIndex)
            {
                case 0:
                    convexCheckBox.Enabled = true;
                    convexCheckBox.Checked = false;
                    break;
                case 1:
                    convexCheckBox.Enabled = false;
                    convexCheckBox.Checked = true;
                    break;
            }
        }
    }
}
