using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditorsLibrary
{
    public partial class BXDAEditorForm : Form
    {

        BXDAEditorPane.BXDAEditorNode editNode;

        public BXDAEditorForm(BXDAEditorPane.BXDAEditorNode edit)
        {
            InitializeComponent();

            editNode = edit;

            fieldName.Text = editNode.Name;

            if (editNode.type == BXDAEditorPane.BXDAEditorNode.NodeType.VECTOR3)
            {
                valueX.Text = String.Format("{0}", editNode.data[0]);
                valueGen.Text = String.Format("{0}", editNode.data[1]);
                valueZ.Text = String.Format("{0}", editNode.data[2]);
            }
            else
            {
                valueX.Enabled = false;
                valueZ.Enabled = false;

                valueGen.Text = String.Format("{0}", editNode.data[0]);
                valueGen.Enabled = true;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (editNode.type == BXDAEditorPane.BXDAEditorNode.NodeType.VECTOR3)
            {
                editNode.data[0] = Double.Parse(valueX.Text);
                editNode.data[1] = Double.Parse(valueGen.Text);
                editNode.data[2] = Double.Parse(valueZ.Text);
            }
            else if (editNode.type == BXDAEditorPane.BXDAEditorNode.NodeType.INTEGER)
            {
                editNode.data[0] = Int32.Parse(valueGen.Text);
            }
            else if (editNode.type == BXDAEditorPane.BXDAEditorNode.NodeType.DOUBLE)
            {
                editNode.data[0] = Double.Parse(valueGen.Text);
            }
            else if (editNode.type == BXDAEditorPane.BXDAEditorNode.NodeType.STRING)
            {
                editNode.data[0] = String.Copy(valueGen.Text);
            }
            else throw new NotImplementedException("Unsupported edit type");

            editNode.updateName();
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
