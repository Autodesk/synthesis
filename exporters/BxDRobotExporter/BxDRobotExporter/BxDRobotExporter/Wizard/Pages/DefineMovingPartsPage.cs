using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BxDRobotExporter.Wizard
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
        }

        #region IWizardPage Implementation
        public bool Initialized { get => _initialized; set => _initialized = value; }
        private bool _initialized = false;

        /// <summary>
        /// Adds all of the remaining <see cref="RigidNode_Base"/> objects to <see cref="DefinePartPanel"/>s and adds them to <see cref="DefinePartsPanelLayout"/>
        /// </summary>
        public void Initialize()
        {
            foreach (RigidNode_Base node in Utilities.GUI.SkeletonBase.ListAllNodes())
            {
                if (node.GetSkeletalJoint() != null && !WizardData.Instance.WheelNodes.Contains(node))
                {
                    DefinePartPanel panel = new DefinePartPanel(node);
                    panels.Add(panel);
                    DefinePartsPanelLayout.Controls.Add(panel);
                }
            }

            _initialized = true;
        }
        
        public event Action DeactivateNext;
        private void OnDeactivateNext() => DeactivateNext?.Invoke();

        public event Action ActivateNext;
        private void OnActivateNext() => ActivateNext?.Invoke();

        public event InvalidatePageEventHandler InvalidatePage;
        private void OnInvalidatePage() => InvalidatePage?.Invoke(typeof(ReviewAndFinishPage));
        
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
