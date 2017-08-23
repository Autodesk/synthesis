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
using System.Diagnostics;

public partial class NodeEditorPane : UserControl
{

    public List<RigidNode_Base> nodes;

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

        ListViewItem item = new ListViewItem(new string[] { (node.GetParent() != null) ? node.GetParent().ModelFileName : "N/A", node.ModelFileName, 
                                                             "False", "False", "False" });

        item.Tag = node;
        listViewNodes.Items.Add(item);
        nodes.Add(node);
    }

    private void listViewNodes_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        foreach(RigidNode_Base node in nodes)
        {
            Debug.WriteLine("Model Full Name: " + node.ModelFileName + "\nModel Full ID: " + node.ModelFullID);
        }

        System.Drawing.Point clientPoint = e.Location;
        ListViewItem item = listViewNodes.GetItemAt(clientPoint.X, clientPoint.Y);
        if (item != null)
        {
            ListViewItem.ListViewSubItem subItem = item.GetSubItemAt(clientPoint.X, clientPoint.Y);
            if (subItem != null && item.SubItems.IndexOf(subItem) >= 2)
            {
                subItem.Text = (subItem.Text == "False") ? "True" : "False";
            }

            RigidNode_Base node = (RigidNode_Base)item.Tag;
            CustomRigidGroup group = (CustomRigidGroup)node.GetModel();
            ExporterHint newHint;
            newHint.Convex = item.SubItems[2].Text == "False";
            newHint.MultiColor = item.SubItems[3].Text == "True";
            newHint.HighResolution = item.SubItems[4].Text == "True";
            group.hint = newHint;
        }
    }

}
