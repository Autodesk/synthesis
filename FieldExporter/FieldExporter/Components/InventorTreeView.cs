using Inventor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FieldExporter
{
    public class InventorTreeView : TreeView
    {
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
            TreeNode node = new TreeNode(component.Name);
            node.Name = node.Text;
            node.Tag = component;

            node = AddComponentChildren(component, node);
            node = AddComponentParents(component, node);

            while (true)
            {
                TreeNode[] searchResults = Nodes.Find(node.Name, true);
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
                        //MessageBox.Show("Object does not exist, but potential parent does, so I'll reiterate as the parent.");
                        node = node.Parent;
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
        /// Adds the supplied components as nodes and sends progress updates.
        /// </summary>
        /// <param name="components"></param>
        /// <param name="progressUpdate"></param>
        public void AddComponents(ObjectsEnumerator components, Action<int, int> progressUpdate)
        {
            for (int i = 0; i < components.Count; i++)
            {
                AddComponent(components[i + 1]);
                progressUpdate(i + 1, components.Count);
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
    }
}
