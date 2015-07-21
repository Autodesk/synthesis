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

public class InventorTreeView : TreeView
{

    private static TreeNode DragNode;
    private static TreeNode TempDropNode;

    private ImageList imageListDrag;
    private ImageList imageListTreeView;

    private Timer timer;

    public InventorTreeView()
    {
        imageListDrag = new ImageList();
        imageListTreeView = new ImageList();

        HotTracking = true;

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

    private void InventorTreeView_ItemDrag(object sender, ItemDragEventArgs e)
    {
        // Get drag node and select it
        InventorTreeView.DragNode = (TreeNode)e.Item;
        SelectedNode = InventorTreeView.DragNode;

        // Reset image list used for drag image
        this.imageListDrag.Images.Clear();
        this.imageListDrag.ImageSize =
              new Size(InventorTreeView.DragNode.Bounds.Size.Width
              + Indent, InventorTreeView.DragNode.Bounds.Height);

        // Create new bitmap
        // This bitmap will contain the tree node image to be dragged
        Bitmap bmp = new Bitmap(InventorTreeView.DragNode.Bounds.Width
            + Indent, Bounds.Height);

        // Get graphics from bitmap
        Graphics gfx = Graphics.FromImage(bmp);

        // Draw node icon into the bitmap
        //gfx.DrawImage(imageListTreeView.Images[0], 0, 0);

        // Draw node label into bitmap
        gfx.DrawString(InventorTreeView.DragNode.Text,
            Font,
            new SolidBrush(ForeColor),
            (float)Indent, 1.0f);

        // Add bitmap to imagelist
        this.imageListDrag.Images.Add(bmp);

        // Get mouse position in client coordinates
        System.Drawing.Point p = PointToClient(Control.MousePosition);

        // Compute delta between mouse position and node bounds
        int dx = (int)p.X + Indent - Bounds.Left;
        int dy = (int)p.Y - Bounds.Top;

        // Begin dragging image
        if (DragHelper.ImageList_BeginDrag(this.imageListDrag.Handle, 0, dx, dy))
        {
            // Begin dragging
            DoDragDrop(bmp, DragDropEffects.Move);
            // End dragging image
            DragHelper.ImageList_EndDrag();
        }
    }

    private void InventorTreeView_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
    {
        // Compute drag position and move image
        System.Drawing.Point formP = ExporterProgressForm.Instance.PointToClient(new System.Drawing.Point(e.X, e.Y));
        DragHelper.ImageList_DragMove(formP.X - Left, formP.Y - Top);

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
            DragHelper.ImageList_DragShowNolock(false);
            SelectedNode = dropNode;
            DragHelper.ImageList_DragShowNolock(true);
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
        // Unlock updates
        DragHelper.ImageList_DragLeave(Handle);

        // Get drop node
        TreeNode dropNode = GetNodeAt(PointToClient(new System.Drawing.Point(e.X, e.Y)));

        // If drop node isn't equal to drag node, add drag node as child of drop node
        if (InventorTreeView.DragNode != dropNode)
        {
            // Remove drag node from parent
            if (InventorTreeView.DragNode.Parent == null)
            {
                Nodes.Remove(InventorTreeView.DragNode);
            }
            else
            {
                InventorTreeView.DragNode.Parent.Nodes.Remove(InventorTreeView.DragNode);
            }

            if (dropNode.Tag != null)
            {
                AddJointForm addForm = new AddJointForm();
                addForm.ShowDialog();

            }

            // Add drag node to drop node
            dropNode.Nodes.Add(InventorTreeView.DragNode);

            // Set drag node to null
            InventorTreeView.DragNode = null;
            dropNode.Expand();

            // Disable scroll timer
            this.timer.Enabled = false;
        }
    }

    private void InventorTreeView_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
    {
        DragHelper.ImageList_DragEnter(Handle, e.X - Left,
            e.Y - Top);

        // Enable timer for scrolling dragged item
        this.timer.Enabled = true;
    }

    private void InventorTreeView_DragLeave(object sender, System.EventArgs e)
    {
        DragHelper.ImageList_DragLeave(Handle);

        // Disable timer for scrolling dragged item
        this.timer.Enabled = false;
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
        System.Drawing.Point pt = ExporterProgressForm.Instance.PointToClient(Control.MousePosition);
        TreeNode node = GetNodeAt(pt);

        if (node == null) return;

        // if mouse is near to the top, scroll up
        if (pt.Y < 30)
        {
            // set actual node to the upper one
            if (node.PrevVisibleNode != null)
            {
                node = node.PrevVisibleNode;

                // hide drag image
                DragHelper.ImageList_DragShowNolock(false);
                // scroll and refresh
                node.EnsureVisible();
                Refresh();
                // show drag image
                DragHelper.ImageList_DragShowNolock(true);

            }
        }
        // if mouse is near to the bottom, scroll down
        else if (pt.Y > Size.Height - 30)
        {
            if (node.NextVisibleNode != null)
            {
                node = node.NextVisibleNode;

                DragHelper.ImageList_DragShowNolock(false);
                node.EnsureVisible();
                Refresh();
                DragHelper.ImageList_DragShowNolock(true);
            }
        }
    }

    private class DragHelper
    {
        [DllImport("comctl32.dll")]
        public static extern bool InitCommonControls();

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        public static extern bool ImageList_BeginDrag(
            IntPtr himlTrack, // Handler of the image list containing the image to drag
            int iTrack,       // Index of the image to drag 
            int dxHotspot,    // x-delta between mouse position and drag image
            int dyHotspot     // y-delta between mouse position and drag image
        );

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        public static extern bool ImageList_DragMove(
            int x,   // X-coordinate (relative to the form,
            // not the treeview) at which to display the drag image.
            int y   // Y-coordinate (relative to the form,
            // not the treeview) at which to display the drag image.
        );

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        public static extern void ImageList_EndDrag();

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        public static extern bool ImageList_DragEnter(
            IntPtr hwndLock,  // Handle to the control that owns the drag image.
            int x,            // X-coordinate (relative to the treeview)
            // at which to display the drag image. 
            int y             // Y-coordinate (relative to the treeview)
            // at which to display the drag image. 
        );

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        public static extern bool ImageList_DragLeave(
            IntPtr hwndLock  // Handle to the control that owns the drag image.
        );

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        public static extern bool ImageList_DragShowNolock(
            bool fShow       // False to hide, true to show the image
        );

        static DragHelper()
        {
            InitCommonControls();
        }
    }

}

