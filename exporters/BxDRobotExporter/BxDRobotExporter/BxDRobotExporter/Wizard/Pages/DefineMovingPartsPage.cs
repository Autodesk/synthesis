using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SynthesisRobotExporter.Wizard
{
    /// <summary>
    /// Prompts the user to set the properties of each of the remaining <see cref="RigidNode_Base"/> objects.
    /// </summary>
    public partial class DefineMovingPartsPage : UserControl, IWizardPage
    {
        /// <summary>
        /// List containing all of the <see cref="DefinePartPanel"/>s 
        /// </summary>
        private List<DefinePartPanel> panels = new List<DefinePartPanel>();

        public DefineMovingPartsPage()
        {
            InitializeComponent();
            DefinePartsLayout.ColumnStyles[1].Width = SystemInformation.VerticalScrollBarWidth + 2;

            // Hide horizontal scrollbar
            DefinePartsLayout.AutoScroll = false;
            DefinePartsLayout.HorizontalScroll.Maximum = 0;
            DefinePartsLayout.AutoScroll = true;

            Initialized = false;
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
                rowStyle = new RowStyle();

            table.RowCount++;
            table.RowStyles.Add(rowStyle);
            table.Controls.Add(control);
            table.SetRow(control, table.RowCount - 1);
            table.SetColumn(control, 0);
        }

        #region IWizardPage Implementation
        private bool _initialized = false;
        public bool Initialized
        {
            get => _initialized;
            set
            {
                if (!value) // Page is being invalidated, reset interface
                {
                    DefinePartsLayout.Controls.Clear();
                    DefinePartsLayout.RowCount = 0;
                    DefinePartsLayout.RowStyles.Clear();
                    foreach (DefinePartPanel panel in panels)
                        panel.Dispose();
                    panels.Clear();
                }

                _initialized = value;
            }
        }

        /// <summary>
        /// Adds all of the remaining <see cref="RigidNode_Base"/> objects to <see cref="DefinePartPanel"/>s and adds them to <see cref="DefinePartsPanelLayout"/>
        /// </summary>
        public void Initialize()
        {
            SuspendLayout();

            foreach (RigidNode_Base node in Utilities.GUI.SkeletonBase.ListAllNodes())
            {
                if (node.GetSkeletalJoint() != null && !WizardData.Instance.WheelNodes.Contains(node))
                {
                    DefinePartPanel panel = new DefinePartPanel(node);
                    panels.Add(panel);
                    AddControlToNewTableRow(panel, DefinePartsLayout);
                    if (node.GetSkeletalJoint().cDriver != null)
                    {
                        panel.refillValues(node.GetSkeletalJoint());
                    }
                }
            }
            ResumeLayout();

            _initialized = true;
        }
        
        public event Action DeactivateNext;
        private void OnDeactivateNext() => DeactivateNext?.Invoke();

        public event Action ActivateNext;
        private void OnActivateNext() => ActivateNext?.Invoke();

        public event InvalidatePageEventHandler InvalidatePage;
        private void OnInvalidatePage()
        {
            InvalidatePage?.Invoke(null);
        }
        /// <summary>
        /// Passes the <see cref="JointDriver"/> from each <see cref="DefinePartPanel"/> to <see cref="WizardData.Instance"/>
        /// </summary>
        public void OnNext()
        {
            foreach(var panel in panels)
            {
                WizardData.Instance.JointDrivers.Add(panel.node, panel.GetJointDriver());
            }
        } 
        #endregion
    }
}
