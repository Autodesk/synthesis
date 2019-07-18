using System.Collections.Generic;
using System.Windows.Forms;

namespace BxDRobotExporter.JointEditor
{
    public partial class JointForm : Form
    {
        private readonly List<JointCard> jointCards = new List<JointCard>();

        public JointForm()
        {
            InitializeComponent();
            SuspendLayout();

            foreach (RigidNode_Base node in Utilities.GUI.SkeletonBase.ListAllNodes())
            {
                if (node.GetSkeletalJoint() != null) // create new part panels for every node
                {
                    JointCard panel = new JointCard(node, this);
                    panel.Dock = DockStyle.Top;

                    jointCards.Add(panel);

                    WinFormsUtils.AddControlToNewTableRow(panel, DefinePartsLayout);
                }
            }

            ResumeLayout();
            
            Shown += (sender, args) => // First load
            {
                jointCards.ForEach(card => card.LoadPreviewIcon());
            };

            Closing += (sender, e) => // Every close
            {
                InventorUtils.FocusAndHighlightNodes(null, StandardAddInServer.Instance.MainApplication.ActiveView.Camera, 1);
                Utilities.GUI.ReloadPanels();
            };

            FormClosing += (sender, e) =>
            {
                Hide();
                e.Cancel = true; // this cancels the close event.
            };
        }

        public void OnShowButtonClick()
        {
            CollapseAllCards();
            jointCards.ForEach(card => card.LoadValuesRecursive());
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