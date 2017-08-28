using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace BxDRobotExporter.Wizard
{
    /// <summary>
    /// Displays the "finish" button and allows the user to launch Synthesis on exiting the wizard.
    /// </summary>
    public partial class ReviewAndFinishPage : UserControl, IWizardPage
    {
        /// <summary>
        /// Dictionary associating field names with field paths.
        /// </summary>
        Dictionary<string, string> fields = new Dictionary<string, string>();

        public ReviewAndFinishPage()
        {
            InitializeComponent();
        }
        
        private void LauchSimCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            FieldSelectComboBox.Enabled = LauchSimCheckBox.Checked;
        }

        #region IWizardPage Implementation
        public bool Initialized { get => _initialized; set => _initialized = value; }
        private bool _initialized = false;

        public event Action DeactivateNext;
        private void OnDeactivateNext()
        {
            DeactivateNext?.Invoke();
        }
        public event Action ActivateNext;
        private void OnActivateNext()
        {
            ActivateNext?.Invoke();
        }

        public event InvalidatePageEventHandler InvalidatePage;
        private void OnInvalidatePage()
        {
            InvalidatePage?.Invoke(null);
        }

        /// <summary>
        /// Gets the paths for all the synthesis fields.
        /// </summary>
        public void Initialize()
        {
            var dirs = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\synthesis\fields");

            foreach (var dir in dirs)
            {
                fields.Add(dir.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last(), dir);
                FieldSelectComboBox.Items.Add(dir.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last());
            }

            _initialized = true;
        }

        public void OnNext()
        {
            if(LauchSimCheckBox.Checked)
            {
                Utilities.GUI.RobotSave();
                Process.Start(Utilities.SYTHESIS_PATH, string.Format("-robot \"{0}\" -field \"{1}\"", Properties.Settings.Default.SaveLocation + "\\" + Utilities.GUI.RMeta.ActiveRobotName, fields[(string)FieldSelectComboBox.SelectedItem]));
            }
        }
        #endregion

    }
}
