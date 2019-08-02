using System.Collections.Generic;
using System.Windows.Forms;
using InventorRobotExporter.Managers;
using InventorRobotExporter.Utilities;

namespace InventorRobotExporter.GUI.Editors.JointEditor
{
    public partial class JointEditorForm : Form
    {
        private readonly List<JointCardUC> jointCards = new List<JointCardUC>();

        public JointEditorForm()
        {
            AnalyticsUtils.LogPage("Joint Editor");
            InitializeComponent();

            Closing += (sender, e) => // Every close
            {
                InventorUtils.FocusAndHighlightNodes(null, RobotExporterAddInServer.Instance.Application.ActiveView.Camera, 1);
            };

            FormClosing += (sender, e) =>
            {
                Hide();
                e.Cancel = true; // this cancels the close event.
            };
        }

        public void LoadRobot(RobotDataManager robotDataManager)
        {
            SuspendLayout();
            
            DefinePartsLayout.Controls.Clear();
            DefinePartsLayout.RowStyles.Clear();

            foreach (var node in robotDataManager.RobotBaseNode.ListAllNodes())
            {
                if (node.GetSkeletalJoint() == null || JointDriver.GetAllowedDrivers(node.GetSkeletalJoint()).Length == 0) continue;
                
                var panel = new JointCardUC(node, this, robotDataManager) {Dock = DockStyle.Top};
                jointCards.Add(panel);
                AddControlToNewTableRow(panel, DefinePartsLayout);
            }
            
            jointCards.ForEach(card => card.LoadPreviewIcon());

            ResumeLayout();
        }
        
        public static void AddControlToNewTableRow(Control control, TableLayoutPanel table, RowStyle rowStyle = null)
        {
            if (rowStyle == null)
            {
                rowStyle = new RowStyle();
                rowStyle.SizeType = SizeType.AutoSize;
            }

            table.RowCount++;
            table.RowStyles.Add(rowStyle);
            table.Controls.Add(control);
            table.SetRow(control, table.RowCount - 2);
            table.SetColumn(control, 0);
        }

        public new void PreShow() // This can't be an event listener because this should only fire when the user has pressed the button to show the form
        {
            jointCards.ForEach(card => card.LoadValues());
        }

        public void ResetAllHighlight()
        {
            jointCards.ForEach(card => card.ResetHighlight());
        }

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void OkButton_Click(object sender, System.EventArgs e)
        {
            jointCards.ForEach(card => card.SaveValues());
            Close();
        }
    }
}