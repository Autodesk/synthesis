using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InternalFieldExporter.FieldWizard
{
    public partial class ComponentPropertiesTabPage : TabPage
    {
        /// <summary>
        /// parent PhysicsGroupsTabControl
        /// </summary>
        public PropertySetsTabControl parentControl
        {
            get;
            private set;
        }

        /// <summary>
        /// child PhysicsGroupsTabControl
        /// </summary>
        public ComponentPropertiesForm ChildForm
        {
            get;
            private set;
        }

        /// <summary>
        /// Right Click menu options
        /// </summary>
        private ContextMenu rightClickMenu;

        /// <summary>
        /// Inits the component with a specified parent and name
        /// </summary>
        /// <param name="physicsGroupsTabControl"></param>
        /// <param name="name"></param>
        public ComponentPropertiesTabPage(PropertySetsTabControl physicsGroupsTabControl, string name)
        {
            InitializeComponent();

            parentControl = physicsGroupsTabControl;

            SetName(name);

            ChildForm = new ComponentPropertiesTabPage(this);
            Controls.Add(ChildForm);

            rightClickMenu = new ContextMenu();
            rightClickMenu.MenuItems.Add(new MenuItem("Delete", new EventHandler(deleteMenuItem_onClick)));
            rightClickMenu.MenuItems.Add(new MenuItem("Change Name", new EventHandler(changeNameMenuItem_onClick)));
        }

        /// <summary>
        /// Allows for easy changing of the title and name of the component
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
        /// Displays the right click menu
        /// </summary>
        /// <param name="e"></param>
        public void ShowRightClickMenu(MouseEventArgs e)
        {
            rightClickMenu.Show(this, new Point(e.X, e.Y));
        }

        /// <summary>
        /// Removes this component from its parent control
        /// </summary>
        public void Remove()
        {
            parentControl.TabPages.Remove(this);
        }

        /// <summary>
        /// Removes the menu when "Delete" right click option is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteMenuItem_onClick(object sender, EventArgs e)
        {
            Remove();
        }

        private void changeNameMenuItem_onClick(object sender, EventArgs e)
        {
            ChangeName();
        }
    }
}
