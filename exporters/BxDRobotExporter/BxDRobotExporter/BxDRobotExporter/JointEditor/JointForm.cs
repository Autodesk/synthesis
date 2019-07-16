using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using JointResolver.ControlGUI;

namespace BxDRobotExporter.JointEditor
{
    public partial class JointForm : Form
    {
        private readonly List<JointCard> jointCards = new List<JointCard>();
        private readonly ProgressBarForm progressBar = new ProgressBarForm("Loading Joint Editor");

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

            Activated += (sender, args) => // Every time the form is displayed
            {
                progressBar.Hide();
                progressBar.SetProgress("Loading Joint Parameters...", 0, 5);
            };
        }

        public async Task PreShow()
        {
            CollapseAllCards();
            jointCards.ForEach(card => card.LoadValuesRecursive());
            progressBar.SetProgress("Loading Joint Parameters...", 4, 5);
            progressBar.Show();
            await Task.Delay(500); // Wait for progress bar to fully load
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