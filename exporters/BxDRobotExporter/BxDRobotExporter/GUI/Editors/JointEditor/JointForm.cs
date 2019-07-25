using System.Collections.Generic;
using System.Windows.Forms;
using BxDRobotExporter.Editors.JointEditor;

namespace BxDRobotExporter.GUI.Editors.JointEditor
{
    public partial class JointForm : Form
    {
        private readonly List<JointCard> jointCards = new List<JointCard>();

        public JointForm()
        {
            AnalyticsUtils.LogPage("Joint Editor");
            InitializeComponent();

            Shown += (sender, args) => // First load
            {
                jointCards.ForEach(card => card.LoadPreviewIcon());
            };

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

        public void UpdateSkeleton(RobotData robotData)
        {
            SuspendLayout();

            foreach (RigidNode_Base node in robotData.SkeletonBase.ListAllNodes())
            {
                if (node.GetSkeletalJoint() != null) // create new part panels for every node
                {
                    JointCard panel = new JointCard(node, this, robotData);
                    panel.Dock = DockStyle.Top;

                    jointCards.Add(panel);

                    WinFormsUtils.AddControlToNewTableRow(panel, DefinePartsLayout);
                }
            }

            ResumeLayout();
        }

        public new void ShowDialog()
        {
            CollapseAllCards();
            jointCards.ForEach(card => card.LoadValuesRecursive());
            base.ShowDialog();
        }

        public void CollapseAllCards(JointCard besides = null)
        {
            jointCards.ForEach(card =>
            {
                if (card != besides)
                    card.SetCollapsed(true);
            });
        }

        public void ResetAllHighlight()
        {
            jointCards.ForEach(card => card.ResetHighlight());
        }
    }
}