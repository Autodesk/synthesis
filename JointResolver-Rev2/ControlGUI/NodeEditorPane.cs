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

public partial class NodeEditorPane : UserControl
{

    List<RigidNode_Base> nodes;

    public NodeEditorPane()
    {
        InitializeComponent();

        nodes = new List<RigidNode_Base>();

        foreach (ColumnHeader header in listViewNodes.Columns)
        {
            header.Width = (Width / 5) - 1;
        }
    }

    public void AddNodes(List<RigidNode_Base> nodes)
    {
        foreach (RigidNode_Base node in nodes)
        {
            AddNode(node);
        }
    }

    public void AddNode(RigidNode_Base node)
    {
        if (listViewNodes.Items.Cast<ListViewItem>().FirstOrDefault(i => i.Tag != null && 
                                                                         ((RigidNode_Base)i.Tag).GetModelID() == node.GetModelID()) != null) return;

        ListViewItem item = new ListViewItem(new string[] { (node.GetParent() != null)?node.GetParent().modelFileName:"N/A", node.modelFileName, 
                                                             "false", "false", "false" });

        item.Tag = node;
        listViewNodes.Items.Add(item);
        nodes.Add(node);
    }

    private void listViewNodes_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        System.Drawing.Point clientPoint = e.Location;
        ListViewItem item = listViewNodes.GetItemAt(clientPoint.X, clientPoint.Y);
        if (item != null)
        {
            ListViewItem.ListViewSubItem subItem = item.GetSubItemAt(clientPoint.X, clientPoint.Y);
            if (subItem != null && item.SubItems.IndexOf(subItem) >= 2)
            {
                subItem.Text = (subItem.Text == "false") ? "true" : "false";
            }

            RigidNode_Base node = (RigidNode_Base)item.Tag;
            CustomRigidGroup group = (CustomRigidGroup)node.GetModel();
            ExporterHint newHint;
            newHint.Convex = item.SubItems[2].Text == "false";
            newHint.MultiColor = item.SubItems[3].Text == "true";
            newHint.HighResolution = item.SubItems[4].Text == "true";
            group.hint = newHint;
        }
    }

}
