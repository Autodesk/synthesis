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

public partial class JointGroupPane : UserControl
{

    private List<JointGroup> jointGroups;
    private JointGroup defaultJointGroup;

    private bool isMouseDown;
    private System.Drawing.Point dragStart;
    private Rectangle dragRect;

    public JointGroupPane()
    {
        InitializeComponent();

        jointGroups = new List<JointGroup>();

        treeviewInventor.AllowDrop = true;

        panelJoints.MouseDown += panelJoints_MouseDown;
        panelJoints.MouseMove += panelJoints_MouseMove;
        panelJoints.MouseUp += panelJoints_MouseUp;
    }

    public void UpdateComponents(List<ComponentOccurrence> components)
    {
        treeviewInventor.AddComponents(components);

        foreach (ComponentOccurrence component in components)
        {
            UpdateComponent(component);
        }
    }

    private void UpdateComponent(ComponentOccurrence component)
    {
        foreach (ComponentOccurrence subComponent in component.SubOccurrences)
        {
            if (component.Joints.Count > 0)
            {
                if (defaultJointGroup == null)
                {
                    defaultJointGroup = new JointGroup(panelJoints, 50, 50, 300, 300);
                    AddGroup(defaultJointGroup);
                }

                foreach (AssemblyJoint joint in component.Joints)
                {
                    defaultJointGroup.AddJoint(joint);
                }
            }

            UpdateComponent(subComponent);
        }
    }

    private void AddGroup(JointGroup group)
    {
        jointGroups.Add(group);
    }

    private void AddGroup(System.Drawing.Point pos, Size size)
    {
        jointGroups.Add(new JointGroup(panelJoints, pos, size));
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

        if (Math.Abs(dragEnd.X - dragStart.X) > 30 && Math.Abs(dragEnd.Y - dragStart.Y) > 30)
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
    #endregion

    public void Cleanup()
    {
        
    }

    #region Nested classes
    private class JointGroup : Panel
    {

        private InventorTreeView jointTree;

        public JointGroup(Control parent, System.Drawing.Point pos, Size size)
            : base()
        {
            jointTree = new InventorTreeView(true);
            SuspendLayout();

            Location = pos;
            Size = size;
            BorderStyle = BorderStyle.FixedSingle;
            Visible = true;

            jointTree.Dock = DockStyle.Fill;
            jointTree.AllowDrop = true;
            Controls.Add(jointTree);

            ResumeLayout(false);

            parent.Controls.Add(this);

            jointTree.Nodes.Add("Joint Group");
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
        }

    }
    #endregion

}

