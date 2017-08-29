using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace BxDRobotExporter.Wizard
{
    /// <summary>
    /// First page the user sees. Has a link to the tutorials page. TODO: Add more stuff here.
    /// </summary>
    public partial class StartPage : UserControl, IWizardPage
    {
        public StartPage()
        {
            InitializeComponent();
        }

        private void TutorialsLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://" + TutorialsLinkLabel.Text);
        }


        #region IWizardPage Implementation
        public void OnNext()
        {
        }
        public event Action ActivateNext;
        private void OnActivateNext()
        {
            this.ActivateNext?.Invoke();
        }

        public event Action DeactivateNext;

        private void OnDeactivateNext()
        {
            this.DeactivateNext?.Invoke();
        }

        public event InvalidatePageEventHandler InvalidatePage;
        private void OnInvalidatePage()
        {
            InvalidatePage?.Invoke(null);
        }

        public void Initialize() { _initialized = true; }
        public bool Initialized { get => _initialized; set => _initialized = value; }
        private bool _initialized = false; 
        #endregion
    }
}
