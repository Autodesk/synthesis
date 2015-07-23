using FieldExporter.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FieldExporter.Components
{
    public partial class ComponentPropertiesTabPage : TabPage
    {
        /// <summary>
        /// The parent PhysicsGroupsTabControl.
        /// </summary>
        public PhysicsGroupsTabControl parentControl
        {
            get;
            private set;
        }

        /// <summary>
        /// The child ComponentPropertiesForm
        /// </summary>
        public ComponentPropertiesForm childForm
        {
            get;
            private set;
        }

        /// <summary>
        /// Right-click menu options.
        /// </summary>
        ContextMenu rightClickMenu;

        /// <summary>
        /// Initalizes the component with a specified parent and name.
        /// </summary>
        /// <param name="physicsGroupsTabControl"></param>
        /// <param name="name"></param>
        public ComponentPropertiesTabPage(PhysicsGroupsTabControl physicsGroupsTabControl, string name)
        {
            InitializeComponent();

            parentControl = physicsGroupsTabControl;

            SetName(name);

            childForm = new ComponentPropertiesForm(this);
            Controls.Add(childForm);

            rightClickMenu = new ContextMenu();
            rightClickMenu.MenuItems.Add(new MenuItem("Delete", new EventHandler(deleteMenuItem_onClick)));
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
        /// Displays the right click menu.
        /// </summary>
        public void ShowRightClickMenu(MouseEventArgs e)
        {
            rightClickMenu.Show(this, new Point(e.X, e.Y));
        }

        /// <summary>
        /// Removes this component.
        /// </summary>
        public void Remove()
        {
            parentControl.TabPages.Remove(this);
        }

        /// <summary>
        /// Removes this control when the delete menu item is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void deleteMenuItem_onClick(object sender, EventArgs e)
        {
            Remove();
        }
    }
}
