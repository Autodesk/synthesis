using Inventor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FieldExporter.Components
{
    public partial class InventorTreeView : TreeView
    {
        /// <summary>
        /// Initializes this component.
        /// </summary>
        public InventorTreeView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Adds this component to the supplied container and initializes this component.
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

            node = AddComponentChildren(component, node);
            node = AddComponentParents(component, node);

            while (true)
            {
                TreeNode[] searchResults = Nodes.Find(node.Name, true);

                // Keep an eye on this bad boy
                if (searchResults.Length > 0)
                {
                    if (searchResults[0].Parent != null)
                    {
                        //MessageBox.Show("Object and parent exist, so I'll add to the existing parent and remove the existing object.");
                        searchResults[0].Parent.Nodes.Insert(searchResults[0].Index, node);
                    }
                    else
                    {
                        //MessageBox.Show("Object exists but parent does not, so I'll add the component as a root and remove the existing object.");
                        Nodes.Add(node);
                    }
                    searchResults[0].Remove();
                    break;
                }
                else
                {
                    if (node.Parent != null)
                    {
                        TreeNode[] parentResults = Nodes.Find(node.Parent.Name, true);

                        if (parentResults.Length > 0)
                        {
                            //MessageBox.Show("Object does not exist but parent does, so I'll add the node as a child of the parent.");
                            parentResults[0].Nodes.Add(node);
                            break;
                        }
                        else
                        {
                            //MessageBox.Show("Object and parent do not exist, but an Inventor parent does, so I'll reiterate as the parent.");
                            node = node.Parent;
                        }
                    }
                    else
                    {
                        //MessageBox.Show("Neither object nor parent exist, so I'll just add the component as a root.");
                        Nodes.Add(node);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Removes the node by the name supplied.
        /// </summary>
        /// <param name="name"></param>
        public void RemoveNode(string name)
        {
            Nodes.RemoveByKey(name);
        }

        /// <summary>
        /// Selects a node based on its corresponding inventor component.
        /// </summary>
        /// <param name="component"></param>
        public void SelectByComponent(ComponentOccurrence component)
        {
            if (Nodes.Find(component.Name, true).Length > 0)
            {
                SelectedNode = Nodes.Find(component.Name, true)[0];
                SelectedNode.EnsureVisible();
            }
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
        /// When a node is hovered over, it's corresponding component will be selected
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNodeMouseHover(TreeNodeMouseHoverEventArgs e)
        {
            base.OnNodeMouseHover(e);
            Program.INVENTOR_APPLICATION.ActiveDocument.SelectSet.Clear();
            Program.INVENTOR_APPLICATION.ActiveDocument.SelectSet.Select((ComponentOccurrence)e.Node.Tag);
        }
    }
}
