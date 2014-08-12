using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class DriveNode : Form
{
    OGL_RigidNode node;
    public DriveNode()
    {
        InitializeComponent();
    }

    public void ShowDriveDialog(OGL_RigidNode node)
    {
        this.node = node;
        bool cache = node.animate;
        node.animate = false;
        base.ShowDialog();
        node.animate = cache;
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
    }

    private void textBox1_KeyUp(object sender, KeyEventArgs e)
    {
        try
        {
            node.animate = false;
            node.requestedRotation = Convert.ToSingle(textBox1.Text.Trim());
        }
        catch(Exception es)
        {
            Console.WriteLine(es.ToString());
        }
    }
}
