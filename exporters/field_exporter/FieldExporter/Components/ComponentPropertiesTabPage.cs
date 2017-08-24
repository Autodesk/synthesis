using FieldExporter.Controls;
using FieldExporter.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FieldExporter.Components
{
    public partial class ComponentPropertiesTabPage : TabPage
    {
        /// <summary>
        /// The parent PhysicsGroupsTabControl.
        /// </summary>
        public PropertySetsTabControl parentControl
        {
            get;
            private set;
        }

        /// <summary>
        /// The child ComponentPropertiesForm.
        /// </summary>
        public ComponentPropertiesForm ChildForm
        {
            get;
            private set;
        }

        /// <summary>
        /// Right-click menu options.
        /// </summary>
        private ContextMenu rightClickMenu;

        /// <summary>
        /// Initalizes the component with a specified parent and name.
        /// </summary>
        /// <param name="physicsGroupsTabControl"></param>
        /// <param name="name"></param>
        public ComponentPropertiesTabPage(PropertySetsTabControl physicsGroupsTabControl, string name)
        {
            InitializeComponent();

            parentControl = physicsGroupsTabControl;

            SetName(name);

            ChildForm = new ComponentPropertiesForm(this);
            Controls.Add(ChildForm);

            rightClickMenu = new ContextMenu();
            rightClickMenu.MenuItems.Add(new MenuItem("Delete", new EventHandler(deleteMenuItem_onClick)));
            rightClickMenu.MenuItems.Add(new MenuItem("Change Name", new EventHandler(changeNameMenuItem_onClick)));
        }

        /// <summary>
        /// Allows for easy setting of the title and name of the component.
        /// </summary>
        /// <param name="name"></param>
        public void SetName(string name)
        {
            Text = Name = name;
        }

        /// <summary>
        /// Changes the name of the ComponentPropertiesTabPage if the name is not already taken.
        /// </summary>
        public void ChangeName()
        {
            EnterNameDialog nameDialog = new EnterNameDialog();

            if (nameDialog.ShowDialog(this).Equals(DialogResult.OK))
            {
                if (parentControl.TabPages.ContainsKey(nameDialog.nameTextBox.Text) && !Name.ToLower().Equals(nameDialog.nameTextBox.Text.ToLower()))
                {
                    MessageBox.Show("Name is already taken.", "Invalid name.");
                }
                else
                {
                    SetName(nameDialog.nameTextBox.Text);
                }
            }
        }

        /// <summary>
        /// Displays the right click menu.
        /// </summary>
        public void ShowRightClickMenu(MouseEventArgs e)
        {
            rightClickMenu.Show(this, new Point(e.X, e.Y));
        }

        /// <summary>
        /// Removes this component from its parent control.
        /// </summary>
        public void Remove()
        {
            parentControl.TabPages.Remove(this);
        }

        /// <summary>
        /// Removes this control when the "Delete" right-click option is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteMenuItem_onClick(object sender, EventArgs e)
        {
            Remove();
        }

        /// <summary>
        /// Changes the name when the "Change Name" right-click option is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void changeNameMenuItem_onClick(object sender, EventArgs e)
        {
            ChangeName();
        }
    }
}
