using Inventor;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace FieldExporter.Components
{
    public partial class InventorTreeView : TreeView
    {
        /// <summary>
        /// Initializes a new instance of the InventorTreeView class.
        /// </summary>
        public InventorTreeView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the InventorTreeView class and adds it to the supplied container.
        /// </summary>
        /// <param name="container"></param>
        public InventorTreeView(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        /// <summary>
        /// Adds children to a TreeNode based on its Inventor component correspondant.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private TreeNode AddComponentChildren(ComponentOccurrence component, TreeNode node)
        {
            IEnumerator enumerator = component.SubOccurrences.GetEnumerator();
            ComponentOccurrence subOccurrence;

            while (enumerator.MoveNext())
            {
                subOccurrence = (ComponentOccurrence)enumerator.Current;

                TreeNode currentNode = new TreeNode(subOccurrence.Name);
                currentNode.Name = currentNode.Text;
                currentNode.Tag = subOccurrence;

                node.Nodes.Add(currentNode);

                AddComponentChildren(subOccurrence, currentNode);
            }

            return node;
        }

        /// <summary>
        /// Adds parents to a TreeNode based on its Inventor component correspondant.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private TreeNode AddComponentParents(ComponentOccurrence component, TreeNode node)
        {
            ComponentOccurrence parentOccurrence;

            if (component.ParentOccurrence != null)
            {
                parentOccurrence = component.ParentOccurrence;

                TreeNode currentNode = new TreeNode(parentOccurrence.Name);
                currentNode.Name = currentNode.Text;
                currentNode.Tag = parentOccurrence;

                currentNode.Nodes.Add(node);

                AddComponentParents(parentOccurrence, currentNode);
            }

            return node;
        }

        /// <summary>
        /// Returns the root of a node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private TreeNode FindRootNode(TreeNode node)
        {
            while (node.Parent != null)
            {
                node = node.Parent;
            }
            return node;
        }

        /// <summary>
        /// Adds parent and child components to the given node and returns the root node.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="node"></param>
        /// <returns>The node</returns>
        private TreeNode GenerateFullTree(ComponentOccurrence component, TreeNode node)
        {
            node = AddComponentChildren(component, node);
            node = AddComponentParents(component, node);
            node = FindRootNode(node);

            return node;
        }

        /// <summary>
        /// Used for finding a component by the supplied name.
        /// </summary>
        /// <param name="enumerator"></param>
        /// <param name="fullname"></param>
        /// <returns></returns>
        private ComponentOccurrence FindComponentByFullName(IEnumerator enumerator, string fullname)
        {
            ComponentOccurrence component = null;
            string componentName = fullname.Contains('\\') ? fullname.Substring(0, fullname.IndexOf('\\')) : fullname;

            while (enumerator.MoveNext())
            {
                component = (ComponentOccurrence)enumerator.Current;

                if (component.Name.Equals(componentName))
                {
                    if (componentName != fullname)
                    {
                        fullname = fullname.Replace(componentName + "\\", "");
                        component = FindComponentByFullName(component.SubOccurrences.GetEnumerator(), fullname);
                    }
                    break;
                }
            }

            return component;
        }

        /// <summary>
        /// Adds the supplied component as a node with its corresponding parents and children.
        /// </summary>
        /// <param name="component"></param>
        public void AddComponent(ComponentOccurrence component)
        {
            if (!component.Visible)
                return;

            TreeNode node = new TreeNode(component.Name);
            node.Name = node.Text;
            node.Tag = component;
            
            node = GenerateFullTree(component, node);

            TreeNodeCollection iterator = Nodes;

            while (iterator.ContainsKey(node.Name) && node.Nodes.Count == 1)
            {
                iterator = iterator[iterator.IndexOfKey(node.Name)].Nodes;
                node = node.Nodes[0];
            }

            iterator.RemoveByKey(node.Name);
            iterator.Add(node);
        }

        /// <summary>
        /// Removes a node by the name supplied.
        /// </summary>
        /// <param name="name"></param>
        public void RemoveNode(string name)
        {
            Nodes.RemoveByKey(name);
        }

        /// <summary>
        /// Resets the InventorTreeView.
        /// </summary>
        public void Reset()
        {
            Nodes.Clear();
        }

        /// <summary>
        /// Removes a node when [delete] is pressed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyData == Keys.Delete)
            {
                if (SelectedNode != null)
                {
                    Nodes.Remove(SelectedNode);
                }
            }
        }

        /// <summary>
        /// When a node is selected, it's corresponding component will be selected.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnAfterSelect(TreeViewEventArgs e)
        {
            base.OnAfterSelect(e);

            Program.ASSEMBLY_DOCUMENT.SelectSet.Clear();

            try
            {
                Program.ASSEMBLY_DOCUMENT.SelectSet.Select((ComponentOccurrence)e.Node.Tag);
            }
            catch // Garbage collector has irresponsibly removed our reference; create a new one.
            {
                ComponentOccurrence component = FindComponentByFullName(Program.ASSEMBLY_DOCUMENT.ComponentDefinition.Occurrences.GetEnumerator(), e.Node.FullPath);

                if (component != null)
                {
                    e.Node.Tag = component;
                    Program.ASSEMBLY_DOCUMENT.SelectSet.Select(e.Node.Tag);
                }
            }
        }
    }
}
