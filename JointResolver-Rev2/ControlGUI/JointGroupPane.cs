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
using EditorsLibrary;

public partial class JointGroupPane : UserControl
{

    private List<JointGroup> jointGroups;
    private InventorTreeView unassignedJoints;
    private bool showUnassigned;

    private bool isMouseDown;
    private System.Drawing.Point dragStart;
    private Rectangle dragRect;

    public JointGroupPane()
    {
        InitializeComponent();

        jointGroups = new List<JointGroup>();

        unassignedJoints = new InventorTreeView(true);
        

        panelJoints.MouseDown += panelJoints_MouseDown;
        panelJoints.MouseMove += panelJoints_MouseMove;
        panelJoints.MouseUp += panelJoints_MouseUp;
    }

    public void UpdateComponents(List<ComponentOccurrence> components)
    {
        foreach (ComponentOccurrence component in components)
        {
            if (component.Joints.Count > 0) unassignedJoints.AddComponent(component);
        }
    }

    private void AddGroup(JointGroup group)
    {
        jointGroups.Add(group);
        group.closeButton.Click += (object sender, EventArgs e) =>
            {
                group.Visible = false;
                jointGroups.Remove(group);
                Controls.Remove(group);
                group.Dispose();

                using (Graphics g = CreateGraphics())
                {
                    g.Clear(Control.DefaultBackColor);
                }
            };
    }

    private void AddGroup(System.Drawing.Point pos, Size size)
    {
        AddGroup(new JointGroup(panelJoints, pos, size));
    }

    #region Events
    private void panelJoints_MouseDown(object sender, MouseEventArgs e)
    {
        dragStart = new System.Drawing.Point(e.X, e.Y);
        dragRect = new Rectangle(dragStart, new Size(0, 0));

        isMouseDown = true;
    }

    private void panelJoints_MouseMove(object sender, MouseEventArgs e)
    {
        if (!isMouseDown) return;

        if (e.X > dragStart.X)
        {
            dragRect.Width = e.X - dragStart.X;
        }
        else
        {
            dragRect.X = e.X;
            dragRect.Width = dragStart.X - e.X;
        }

        if (e.Y > dragStart.Y)
        {
            dragRect.Height = e.Y - dragStart.Y;
        }
        else
        {
            dragRect.Y = e.Y;
            dragRect.Height = dragStart.Y - e.Y;
        }

        //Actually draw the rectangle
        using (Graphics g = panelJoints.CreateGraphics())
        {
            Pen pen = new Pen(System.Drawing.Color.DarkGray, 2);

            g.Clear(Control.DefaultBackColor);
            g.DrawRectangle(pen, dragRect);
        }
    }

    private void panelJoints_MouseUp(object sender, MouseEventArgs e)
    {
        isMouseDown = false;
        if (dragStart == System.Drawing.Point.Empty) return;

        System.Drawing.Point dragEnd = new System.Drawing.Point(e.X, e.Y);

        if (Math.Abs(dragEnd.X - dragStart.X) > JointGroup.MIN_SIZE.Width && Math.Abs(dragEnd.Y - dragStart.Y) > JointGroup.MIN_SIZE.Height)
        {
            System.Drawing.Point pos = new System.Drawing.Point();
            Size size = new Size();

            pos.X = (dragEnd.X > dragStart.X) ? dragStart.X : dragEnd.X;
            pos.Y = (dragEnd.Y > dragStart.Y) ? dragStart.Y : dragEnd.Y;

            size.Width = (int)Math.Abs(dragEnd.X - dragStart.X);
            size.Height = (int)Math.Abs(dragEnd.Y - dragStart.Y);

            AddGroup(pos, size);
        }

        dragStart = System.Drawing.Point.Empty;

        using (Graphics g = panelJoints.CreateGraphics())
        {
            g.Clear(Control.DefaultBackColor);
        }
    }

    private void buttonAdd_Click(object sender, EventArgs e)
    {
        AddGroup(new JointGroup(panelJoints, 50, 50, 300, 300));
    }

    private void buttonShow_Click(object sender, EventArgs e)
    {
        if (!showUnassigned)
        {

        }
    }
    #endregion

    public void Cleanup()
    {

    }

    #region Nested classes
    public class JointGroup : Panel
    {

        public static readonly Size MIN_SIZE = new Size(100, 100);

        private Control parent;

        private InventorTreeView jointTree;

        private Panel menuPanel;
        private Button minimizeButton;
        private bool minimized;
        public Button closeButton;
        private Label nameLabel;

        private bool resizing;
        private bool moving;
        private System.Drawing.Point dragStart;
        private Size oldSize;

        public JointGroup(Control p, System.Drawing.Point pos, Size size)
            : base()
        {
            parent = p;

            parent.Controls.Add(this);

            jointTree = new InventorTreeView(true);
            menuPanel = new Panel();
            minimizeButton = new Button();
            closeButton = new Button();
            nameLabel = new Label();
            SuspendLayout();

            Location = pos;
            Size = size;
            BorderStyle = BorderStyle.FixedSingle;
            Visible = true;
            Resize += JointGroup_Resize;

            jointTree.Dock = DockStyle.Bottom;
            jointTree.Height = Height - 15;
            jointTree.AllowDrop = true;
            jointTree.Scrollable = false;
            jointTree.MouseDown += jointTree_MouseDown;
            jointTree.MouseMove += jointTree_MouseMove;
            jointTree.NodeMouseDoubleClick += jointTree_NodeMouseDoubleClick;

            menuPanel.Dock = DockStyle.Top;
            menuPanel.Height = 15;
            menuPanel.BackColor = System.Drawing.Color.FromArgb(0xFF, System.Drawing.Color.CornflowerBlue);
            menuPanel.MouseDown += menuPanel_MouseDown;
            menuPanel.MouseMove += menuPanel_MouseMove;
            menuPanel.MouseUp += menuPanel_MouseUp;

            nameLabel.Dock = DockStyle.Left;
            nameLabel.Height = 15;
            nameLabel.Font = new Font("Microsoft Sans Serif", 6.6f);
            nameLabel.Text = "Joint Group";
            nameLabel.MouseDown += menuPanel_MouseDown;
            nameLabel.MouseMove += menuPanel_MouseMove;
            nameLabel.MouseUp += menuPanel_MouseUp;

            minimizeButton.Dock = DockStyle.Right;
            minimizeButton.Location.Offset(-20, 0);
            minimizeButton.Width = 15;
            minimizeButton.Height = 15;
            minimizeButton.BackColor = Control.DefaultBackColor;
            minimizeButton.Font = new Font("Microsoft Sans Serif", 5.5f);
            minimizeButton.Text = "-";
            minimizeButton.Click += minimizeButton_Click;

            closeButton.Dock = DockStyle.Right;
            closeButton.Height = 15;
            closeButton.Width = 15;
            closeButton.BackColor = Control.DefaultBackColor;
            closeButton.Font = new Font("Microsoft Sans Serif", 5.5f);
            closeButton.Text = "X";

            menuPanel.Controls.Add(nameLabel);
            menuPanel.Controls.Add(minimizeButton);
            menuPanel.Controls.Add(closeButton);
            nameLabel.SendToBack();

            parent.MouseMove += parent_MouseMove;
            parent.MouseUp += parent_MouseUp;

            Controls.Add(jointTree);
            Controls.Add(menuPanel);

            ResumeLayout(true);

            jointTree.Nodes.Add("Joint Group", "Joint Group");
        }

        public JointGroup(Control parent, int initX, int initY, int initW, int initH)
            : this(parent, new System.Drawing.Point(initX, initY), new Size(initW, initH))
        { }

        public void AddJoint(AssemblyJoint joint)
        {
            ComponentOccurrence occ1 = joint.OccurrenceOne;
            ComponentOccurrence occ2 = joint.OccurrenceTwo;

            TreeNode parent = new TreeNode(occ2.Name);
            parent.Tag = occ2;

            SkeletalJointType jointType = joint.Definition.JointType.ToSkeletalJointType();

            TreeNode child = new TreeNode(occ1.Name + String.Format(" ({0})", jointType));
            child.Tag = occ1;

            parent.Nodes.Add(child);

            //Don't add the same joint twice
            var collisions = from TreeNode node in jointTree.Nodes[0].Nodes
                             where (node.Tag == parent.Tag) && (node.Nodes[0].Tag == child.Tag)
                             select true;

            if (collisions.Count() == 0) jointTree.Nodes[0].Nodes.Add(parent);
            parent.Expand();
        }

        private void jointTree_MouseDown(object sender, MouseEventArgs e)
        {
            BringToFront();

            if (Cursor == Cursors.SizeNWSE)
            {
                resizing = true;
                Visible = false;

                dragStart = parent.PointToClient(PointToScreen(new System.Drawing.Point(e.X, e.Y)));
            }
        }

        private void menuPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (Cursor == Cursors.Hand)
            {
                moving = true;
                Visible = false;

                dragStart = parent.PointToClient(PointToScreen(new System.Drawing.Point(e.X, e.Y)));
            }
        }

        private void jointTree_MouseMove(object sender, MouseEventArgs e)
        {
            if (!resizing)
            {
                if (Math.Abs(Width - e.X) < 30 && Math.Abs(Height - e.Y) < 30)
                {
                    Cursor = Cursors.SizeNWSE;
                }
                else
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void menuPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!moving)
            {
                Cursor = Cursors.Hand;
            }
            else
            {
                System.Drawing.Point parentPoint = parent.PointToClient(PointToScreen(new System.Drawing.Point(e.X, e.Y)));

                parent_MouseMove(null, new MouseEventArgs(e.Button, e.Clicks, parentPoint.X, parentPoint.Y, e.Delta));
            }
        }

        private void menuPanel_MouseUp(object sender, MouseEventArgs e)
        {
            System.Drawing.Point parentPoint = parent.PointToClient(PointToScreen(new System.Drawing.Point(e.X, e.Y)));

            parent_MouseUp(null, new MouseEventArgs(e.Button, e.Clicks, parentPoint.X, parentPoint.Y, e.Delta));
        }

        private void parent_MouseMove(object sender, MouseEventArgs e)
        {
            System.Drawing.Point deltaMove = System.Drawing.Point.Empty;
            if (moving || resizing) deltaMove = new System.Drawing.Point(e.X - dragStart.X, e.Y - dragStart.Y);

            if (resizing)
            {
                using (Graphics g = parent.CreateGraphics())
                {
                    Size rectangleSize = new Size(Width + deltaMove.X, Height + deltaMove.Y);

                    g.Clear(Control.DefaultBackColor);
                    g.DrawRectangle(new Pen(System.Drawing.Color.DarkGray),
                                    Location.X, Location.Y, rectangleSize.Width, rectangleSize.Height);
                }
            }
            else if (moving)
            {
                using (Graphics g = parent.CreateGraphics())
                {
                    g.Clear(Control.DefaultBackColor);
                    g.DrawRectangle(new Pen(System.Drawing.Color.DarkGray),
                                    Location.X + deltaMove.X, Location.Y + deltaMove.Y, Width, Height);
                }
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }

        private void parent_MouseUp(object sender, MouseEventArgs e)
        {
            System.Drawing.Point deltaMove = System.Drawing.Point.Empty;
            if (moving || resizing) deltaMove = new System.Drawing.Point(e.X - dragStart.X, e.Y - dragStart.Y);

            if (resizing)
            {
                int newWidth = Math.Max(Width + deltaMove.X, MIN_SIZE.Width);
                int newHeight = Math.Max(Height + deltaMove.Y, MIN_SIZE.Height);

                Width = newWidth;
                Height = newHeight;
                OnResize(null);
                base.OnResize(null);

                resizing = false;
                Visible = true;

                BringToFront();
                

                using (Graphics g = parent.CreateGraphics())
                {
                    g.Clear(Control.DefaultBackColor);
                }
            }
            else if (moving)
            {
                Location = new System.Drawing.Point(Location.X + deltaMove.X, Location.Y + deltaMove.Y);
                OnMove(null);
                base.OnMove(null);

                moving = false;
                Visible = true;

                BringToFront();

                using (Graphics g = parent.CreateGraphics())
                {
                    g.Clear(Control.DefaultBackColor);
                }
            }
        }

        private void JointGroup_Resize(object sender, EventArgs e)
        {
            jointTree.Height = Height - 15;
        }

        private void jointTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Name == "Joint Group")
            {
                JointGroupNameEditorForm nameEditorForm = new JointGroupNameEditorForm(e.Node.Text);
                nameEditorForm.ShowDialog();

                if (nameEditorForm.NewName != null)
                {
                    e.Node.Text = nameEditorForm.NewName;
                    nameLabel.Text = nameEditorForm.NewName;
                }
                e.Node.Expand();
            }
        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
            if (minimized)
            {
                Width = oldSize.Width;
                Height = oldSize.Height;
                jointTree.Visible = true;
                minimized = false;
            }
            else
            {
                oldSize = new Size(Width, Height);
                Height = 15;
                jointTree.Visible = false;
                minimized = true;
            }
        }

    }
    #endregion

}

