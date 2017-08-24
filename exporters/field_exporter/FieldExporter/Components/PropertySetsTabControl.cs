using FieldExporter.Controls;
using FieldExporter.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FieldExporter.Components
{
    public partial class PropertySetsTabControl : TabControl
    {
        /// <summary>
        /// The TabPage for creating new instances.
        /// </summary>
        private CreatePropertySetTabPage createTabPage;

        /// <summary>
        /// Initializes this component.
        /// </summary>
        public PropertySetsTabControl()
        {
            InitializeComponent();

            createTabPage = new CreatePropertySetTabPage(this, "Create");
            TabPages.Add(createTabPage);
        }

        /// <summary>
        /// Adds a ComponentPropertiesTab if the EnterNameDialog returns OK and the name isn't already taken.
        /// </summary>
        /// <returns>true if OK is pressed</returns>
        public bool AddComponentPropertiesTab()
        {
            EnterNameDialog nameDialog = new EnterNameDialog();

            if (nameDialog.ShowDialog(this).Equals(DialogResult.OK))
            {
                if (TabPages.ContainsKey(nameDialog.nameTextBox.Text))
                {
                    MessageBox.Show("Name is already taken.", "Invalid name.");
                    return false;
                }
                else
                {
                    ComponentPropertiesTabPage page = new ComponentPropertiesTabPage(this, nameDialog.nameTextBox.Text);
                    TabPages.Insert(TabPages.Count - 1, page);
                    SelectedTab = page;
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Translates the information in each ComponentPropertiesTabPage to a List of PhysicsGroups.
        /// </summary>
        /// <returns>The translation</returns>
        public List<PropertySet> TranslateToPropertySets()
        {
            List<PropertySet> translation = new List<PropertySet>();

            foreach (TabPage t in TabPages)
            {
                if (t is ComponentPropertiesTabPage)
                {
                    ComponentPropertiesTabPage tabPage = (ComponentPropertiesTabPage)t;
                    tabPage.Invoke(new Action(() =>
                        {
                            translation.Add(new PropertySet(
                                tabPage.Name,
                                tabPage.ChildForm.GetCollider(),
                                tabPage.ChildForm.GetFriction(),
                                tabPage.ChildForm.GetMass()));
                        }));
                }
            }

            return translation;
        }

        /// <summary>
        /// Scans each nonexcluded InventorTreeView and determines if they contian the supplied key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="excludedPages"></param>
        /// <returns></returns>
        public bool NodeExists(string key, params ComponentPropertiesTabPage[] excludedPages)
        {
            foreach (TabPage t in TabPages)
            {
                if (t is ComponentPropertiesTabPage)
                {
                    ComponentPropertiesTabPage tabPage = (ComponentPropertiesTabPage)t;
                    if (!excludedPages.Contains<ComponentPropertiesTabPage>(tabPage))
                    {
                        if (tabPage.ChildForm.inventorTreeView.Nodes.Find(key, true).Length > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the TabPage in which the given node is located.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The TabPage (if it exists, otherwise null)</returns>
        public ComponentPropertiesTabPage GetParentTabPage(string key)
        {
            foreach (TabPage t in TabPages)
            {
                if (t is ComponentPropertiesTabPage)
                {
                    ComponentPropertiesTabPage tabPage = (ComponentPropertiesTabPage)t;
                    if (tabPage.ChildForm.inventorTreeView.Nodes.Find(key, true).Length > 0)
                    {
                        return tabPage;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Scans each nonexcluded InventorTreeView and removes nodes with the supplied key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="excludedPages"></param>
        public void RemoveNode(string key, params ComponentPropertiesTabPage[] excludedPages)
        {
            foreach (TabPage t in TabPages)
            {
                if (t is ComponentPropertiesTabPage)
                {
                    ComponentPropertiesTabPage tabPage = (ComponentPropertiesTabPage)t;
                    if (!excludedPages.Contains<ComponentPropertiesTabPage>(tabPage))
                    {
                        TreeNode[] nodeCollection = tabPage.ChildForm.inventorTreeView.Nodes.Find(key, true);
                        for (int i = 0; i < nodeCollection.Length; i++)
                        {
                            nodeCollection[i].Remove();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if interaction events are enabled before switching tabs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PropertySetsTabControl_Deselecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPage != null)
            {
                Control[] controlCollection = SelectedTab.Controls.Find("ComponentPropertiesForm", false);

                if (controlCollection.Length > 0)
                {
                    ComponentPropertiesForm form = (ComponentPropertiesForm)controlCollection[0];
                    if (form != null)
                    {
                        if (form.InteractionEnabled)
                        {
                            e.Cancel = true;
                            MessageBox.Show("Please exit select mode before changing or adding tabs.", "Invalid operation.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new tab if tabs already exist.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PropertySetsTabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (SelectedTab == createTabPage)
            {
                if (TabPages.Count > 1)
                {
                    if (!AddComponentPropertiesTab())
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        /// <summary>
        /// Allows the user to right-click the tabs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PropertySetsTabControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button.Equals(MouseButtons.Right))
            {
                for (int i = 0; i < TabPages.Count; i++)
                {
                    if (GetTabRect(i).Contains(e.Location))
                    {
                        if (TabPages[i] is ComponentPropertiesTabPage)
                        {
                            SelectedTab = TabPages[i];
                            ((ComponentPropertiesTabPage)TabPages[i]).ShowRightClickMenu(e);
                        }
                    }
                }
            }
        }
    }
}
