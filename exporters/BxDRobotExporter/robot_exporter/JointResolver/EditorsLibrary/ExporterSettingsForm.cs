using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditorsLibrary
{
    public delegate void SettingsEvent(Color Child, Color Parent, bool IsParentHighlight, string SaveLocation);

    public partial class PluginSettingsForm : Form
    {
        /// <summary>
        /// The local copy of the setting values
        /// </summary>
        public static PluginSettingsValues Values = new PluginSettingsValues();

        public PluginSettingsForm()
        {
            InitializeComponent();

            LoadValues();

            buttonOK.Click += delegate (object sender, EventArgs e)
            {
                SaveValues();
                Close();
            };

            buttonCancel.Click += delegate (object sender, EventArgs e)
            {
                Close();
            };
        }

        /// <summary>
        /// Load values into the form
        /// </summary>
        private void LoadValues()
        {
            Values = SynthesisGUI.PluginSettings;
            ChildHighlight.BackColor = Values.InventorChildColor;
            ParentHighlight.BackColor = Values.InventorParentColor;
            HighlightParentsCheckBox.Checked = Values.InventorHighlightParent;
            SaveLocationTextBox.Text = Values.GeneralSaveLocation;
            if (!Values.InventorHighlightParent)
                ParentLabel.ForeColor = Color.Gray;
            HighlightParentsCheckBox_CheckedChanged(null, null);
            
        }
        
        /// <summary>
        /// Save the form's values in a <see cref="PluginSettingsValues"/> structure
        /// </summary>
        private void SaveValues()
        {
            Values.InventorChildColor = ChildHighlight.BackColor;
            Values.InventorParentColor = ParentHighlight.BackColor;
            Values.InventorHighlightParent = HighlightParentsCheckBox.Checked;
            Values.OnSettingsChanged(ChildHighlight.BackColor, ParentHighlight.BackColor, HighlightParentsCheckBox.Checked, SaveLocationTextBox.Text);
        }

        /// <summary>
        /// Get the default values for the <see cref="PluginSettingsValues"/> structure
        /// </summary>
        /// <returns>Default values for the <see cref="Exporter"/></returns>
        public static PluginSettingsValues GetDefaultSettings()
        {
            return new PluginSettingsValues()
            {
                InventorParentColor = Color.FromArgb(255, 125, 0, 255),
                InventorChildColor = Color.FromArgb(255, 0, 125, 255),
                InventorHighlightParent = false,
                GeneralSaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Synthesis\Robots"
            };
        }

        /// <summary>
        /// The struct that stores settings for the <see cref="Exporter"/>
        /// </summary>
        public struct PluginSettingsValues
        {
            public Color InventorParentColor;
            public Color InventorChildColor;

            public bool InventorHighlightParent;

            public static event SettingsEvent SettingsChanged;
            internal void OnSettingsChanged(Color Child, Color Parent, bool IsParentHighlight, string SaveLocation)
            {
                SettingsChanged.Invoke(Child, Parent, IsParentHighlight, SaveLocation);
            }

            public string GeneralSaveLocation;
        }

        /// <summary>
        /// Handles the 'Highlight Parents' checkbox being changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HighlightParentsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ParentHighlight.Enabled = HighlightParentsCheckBox.Checked;
            if (ParentHighlight.Enabled)
                ParentLabel.ForeColor = Color.Black;
            else
                ParentLabel.ForeColor = Color.Gray;
        }

        /// <summary>
        /// Sets the <see cref="Color"/> of the , and by extension the <see cref="PluginSettingsValues"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChildHighlight_Click(object sender, EventArgs e)
        {
            ColorDialog colorChoose = new ColorDialog();
            colorChoose.ShowDialog();
            ChildHighlight.BackColor = colorChoose.Color;
        }

        /// <summary>
        /// Sets the <see cref="Color"/> of the background, and by extension the <see cref="PluginSettingsValues"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParentHighlight_Click(object sender, EventArgs e)
        {
            ColorDialog colorChoose = new ColorDialog();
            colorChoose.ShowDialog();
            ParentHighlight.BackColor = colorChoose.Color;
        }
        
        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog()
            {
                RootFolder = Environment.SpecialFolder.MyDocuments
            };
            folderBrowser.ShowDialog();
            if(folderBrowser.SelectedPath != null)
            {
                Values.GeneralSaveLocation = folderBrowser.SelectedPath;
            }

        }
    }
}
