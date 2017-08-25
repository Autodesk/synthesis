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
    public partial class DefineMovingPartsPage : UserControl, IWizardPage
    {
        private List<DefinePartPanel> panels = new List<DefinePartPanel>();

        public DefineMovingPartsPage()
        {
            InitializeComponent();
        }

        #region IWizardPage Implementation
        public bool Initialized { get => _initialized; set => _initialized = value; }
        private bool _initialized = false;

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
