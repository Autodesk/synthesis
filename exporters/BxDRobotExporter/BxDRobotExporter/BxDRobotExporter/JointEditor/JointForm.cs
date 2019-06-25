using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BxDRobotExporter.Wizard
{
    public partial class JointForm : Form
    {        
        WizardData data = new WizardData();
        
        /// <summary>
        /// List containing all of the <see cref="DefinePartPanel"/>s 
        /// </summary>
        private List<JointCard> panels = new List<JointCard>();


        public JointForm()
        {
            InitializeComponent();
            
            SuspendLayout();

            foreach (RigidNode_Base node in Utilities.GUI.SkeletonBase.ListAllNodes())
            {
                if (node.GetSkeletalJoint() != null)// create new part panels for every node
                {
                    JointCard panel = new JointCard(node, this);
                    panels.Add(panel);
                    AddControlToNewTableRow(panel, DefinePartsLayout);
                }
            }
            ResumeLayout();

            
        }
        
        /// <summary>
        /// Adds a control to a new row at the end of the table.
        /// </summary>
        /// <param name="control">Control to append to table.</param>
        /// <param name="table">Table to add control to.</param>
        /// <param name="rowStyle">Style of the new row. Autosized if left null.</param>
        private void AddControlToNewTableRow(Control control, TableLayoutPanel table, RowStyle rowStyle = null)
        {
            if (rowStyle == null)
            {
                rowStyle = new RowStyle();
                rowStyle.SizeType = SizeType.AutoSize;
            }

            table.RowCount++;
            table.RowStyles.Add(rowStyle);
            table.Controls.Add(control);
            table.SetRow(control, table.RowCount - 1);
            table.SetColumn(control, 0);
        }

        public void CollapseAllCards(JointCard besides = null)
        {
            foreach (var control in DefinePartsLayout.Controls)
            {
                if (control is JointCard && control != besides)
                {
                    JointCard card = (JointCard) control;
                    card.SetCollapsed(true);
                }
            }
        }
    }
}