using Inventor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EditorsLibrary;

public class InventorTreeView : TreeView
{

    private static TreeNode DragNode;
    private static TreeNode TempDropNode;

    private Timer timer;

    public InventorTreeView(bool allowDragDrop)
    {
        HotTracking = true;
        AllowDrop = allowDragDrop;

        NodeMouseDoubleClick += InventorTreeView_NodeMouseDoubleClick;

        ItemDrag += InventorTreeView_ItemDrag;
        DragOver += InventorTreeView_DragOver;
        DragEnter += InventorTreeView_DragEnter;
        DragLeave += InventorTreeView_DragLeave;
        DragDrop += InventorTreeView_DragDrop;
        GiveFeedback += InventorTreeView_GiveFeedback;

        timer = new Timer();
        timer.Interval = 200;
        timer.Tick += timer_Tick;
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
    /// Adds the supplied components as nodes and sends progress updates.
    /// </summary>
    /// <param name="components"></param>
    /// <param name="progressUpdate"></param>
    public void AddComponents(List<ComponentOccurrence> components)
    {
        for (int i = 0; i < components.Count; i++)
        {
            AddComponent(components[i]);
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

    protected override void OnNodeMouseHover(TreeNodeMouseHoverEventArgs e)
    {
        base.OnNodeMouseHover(e);

        if (e.Node.Tag != null)
        {
            Exporter.INVENTOR_APPLICATION.ActiveDocument.SelectSet.Clear();
            Exporter.INVENTOR_APPLICATION.ActiveDocument.SelectSet.Select(e.Node.Tag);
        }
    }

    private void InventorTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Node.Tag == null)
        {
            JointGroupNameEditorForm nameEditorForm = new JointGroupNameEditorForm(e.Node.Text);
            nameEditorForm.ShowDialog();

            if(nameEditorForm.NewName != null) e.Node.Text = nameEditorForm.NewName;
            e.Node.Expand();
        }
    }

    private void InventorTreeView_ItemDrag(object sender, ItemDragEventArgs e)
    {
        // Get drag node and select it
        InventorTreeView.DragNode = (TreeNode)e.Item;
        SelectedNode = InventorTreeView.DragNode;

        // Get mouse position in client coordinates
        System.Drawing.Point p = PointToClient(Control.MousePosition);

        // Compute delta between mouse position and node bounds
        int dx = (int)p.X + Indent - Bounds.Left;
        int dy = (int)p.Y - Bounds.Top;

        DoDragDrop(InventorTreeView.DragNode.Text, DragDropEffects.Move);
    }

    private void InventorTreeView_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
    {
        // Get actual drop node
        TreeNode dropNode = GetNodeAt(PointToClient(new System.Drawing.Point(e.X, e.Y)));
        if (dropNode == null)
        {
            e.Effect = DragDropEffects.None;
            return;
        }

        e.Effect = DragDropEffects.Move;

        // if mouse is on a new node select it
        if (InventorTreeView.TempDropNode != dropNode)
        {
            SelectedNode = dropNode;
            InventorTreeView.TempDropNode = dropNode;
        }

        // Avoid that drop node is child of drag node 
        TreeNode tmpNode = dropNode;
        while (tmpNode.Parent != null)
        {
            if (tmpNode.Parent == InventorTreeView.DragNode) e.Effect = DragDropEffects.None;
            tmpNode = tmpNode.Parent;
        }
    }

    private void InventorTreeView_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
    {
        // Get drop node
        TreeNode dropNode = GetNodeAt(PointToClient(new System.Drawing.Point(e.X, e.Y)));

        // If drop node isn't equal to drag node, add drag node as child of drop node
        if (InventorTreeView.DragNode != dropNode)
        {
            TreeNode toAdd = new TreeNode(InventorTreeView.DragNode.Text);
            toAdd.Tag = InventorTreeView.DragNode.Tag;

            if (dropNode.Tag != null)
            {
                AddJointForm addForm = new AddJointForm();
                addForm.ShowDialog();
                SkeletalJointType skeletalType = addForm.chooseType;
                toAdd.Text += String.Format(" ({0})", skeletalType);

                AssemblyJointTypeEnum assemblyType = skeletalType.ToAssemblyJointType();
                AssemblyDocument asmDoc = (AssemblyDocument) Exporter.INVENTOR_APPLICATION.ActiveDocument;
                AssemblyComponentDefinition asmDef = asmDoc.ComponentDefinition;
            }

            // Add drag node to drop node
            dropNode.Nodes.Add(toAdd);

            // Set drag node to null
            InventorTreeView.DragNode = null;
            dropNode.Expand();

            // Disable scroll timer
            timer.Enabled = false;
        }
    }

    private void InventorTreeView_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
    {
        // Enable timer for scrolling dragged item
        timer.Enabled = true;
    }

    private void InventorTreeView_DragLeave(object sender, System.EventArgs e)
    {
        // Disable timer for scrolling dragged item
        timer.Enabled = false;
    }

    private void InventorTreeView_GiveFeedback(object sender, System.Windows.Forms.GiveFeedbackEventArgs e)
    {
        if (e.Effect == DragDropEffects.Move)
        {
            // Show pointer cursor while dragging
            e.UseDefaultCursors = false;
            Cursor = Cursors.Default;
        }
        else e.UseDefaultCursors = true;

    }

    private void timer_Tick(object sender, EventArgs e)
    {
        // get node at mouse position
        System.Drawing.Point pt = ExporterForm.Instance.PointToClient(Control.MousePosition);
        TreeNode node = GetNodeAt(pt);

        if (node == null) return;

        // if mouse is near to the top, scroll up
        if (pt.Y < 30)
        {
            // set actual node to the upper one
            if (node.PrevVisibleNode != null)
            {
                node = node.PrevVisibleNode;

                // scroll and refresh
                node.EnsureVisible();
                Refresh();
            }
        }
        // if mouse is near to the bottom, scroll down
        else if (pt.Y > Size.Height - 30)
        {
            if (node.NextVisibleNode != null)
            {
                node = node.NextVisibleNode;

                node.EnsureVisible();
                Refresh();
            }
        }
    }

}

