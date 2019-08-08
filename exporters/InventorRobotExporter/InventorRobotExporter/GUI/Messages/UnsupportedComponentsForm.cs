using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InventorRobotExporter.Utilities;

namespace InventorRobotExporter.GUI.Messages
{

    public partial class UnsupportedComponentsForm : Form
    {
        private UnsupportedComponentsForm()
        {
            InitializeComponent();
        }

        private void AddUnsupportedComponent(string component, string category, string type)
        {
            ListViewItem item = new ListViewItem(component);
            item.SubItems.Add(category);
            item.SubItems.Add(type);
            componentListView.Items.Add(item);
        }

        private void ContinueButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ComponentListView_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.NewWidth = componentListView.Columns[e.ColumnIndex].Width;
            e.Cancel = true;
        }

        public static void CheckUnsupportedComponents(IEnumerable<RigidNode_Base> nodeList)
        {
            
            var allValidJoints = true;
            var unsupportedComponents = new UnsupportedComponentsForm();

            foreach (var node in nodeList)
            {
                if (node.GetSkeletalJoint() != null)
                {
                    if (node.GetSkeletalJoint().GetJointType() != SkeletalJointType.ROTATIONAL &&
                        node.GetSkeletalJoint().GetJointType() != SkeletalJointType.LINEAR)
                    {
                        unsupportedComponents.AddUnsupportedComponent(ToStringUtils.NodeNameString(node), "Joint", node.GetSkeletalJoint().GetJointType().ToString());
                        allValidJoints = false;
                    }
                }
            }

            if (!allValidJoints)
            {
                unsupportedComponents.ShowDialog();
            }
        }
    }
}
