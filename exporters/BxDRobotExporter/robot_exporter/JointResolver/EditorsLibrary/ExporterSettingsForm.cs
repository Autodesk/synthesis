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
    public delegate void SettingsEvent(Color Child, bool UseFancyColors, string SaveLocation);

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
            SaveLocationTextBox.Text = Values.GeneralSaveLocation;
            UseFancyColorsCheckBox.Checked = Values.GeneralUseFancyColors;
        }
        
        /// <summary>
        /// Save the form's values in a <see cref="PluginSettingsValues"/> structure
        /// </summary>
        private void SaveValues()
        {
            Values.InventorChildColor = ChildHighlight.BackColor;
            Values.GeneralSaveLocation = SaveLocationTextBox.Text;
            Values.GeneralUseFancyColors = UseFancyColorsCheckBox.Checked;
            Values.OnSettingsChanged(ChildHighlight.BackColor, Values.GeneralUseFancyColors, SaveLocationTextBox.Text);
        }

        /// <summary>
        /// Get the default values for the <see cref="PluginSettingsValues"/> structure
        /// </summary>
        /// <returns>Default values for the <see cref="Exporter"/></returns>
        public static PluginSettingsValues GetDefaultSettings()
        {
            return new PluginSettingsValues()
            {
                InventorChildColor = Color.FromArgb(255, 0, 125, 255),
                GeneralUseFancyColors = false,
                GeneralSaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Synthesis\Robots"
            };
        }

        /// <summary>
        /// The struct that stores settings for the <see cref="Exporter"/>
        /// </summary>
        public class PluginSettingsValues
        {

            public static event SettingsEvent SettingsChanged;
            internal void OnSettingsChanged(Color Child, bool UseFancyColors, string SaveLocation)
            {
                SettingsChanged.Invoke(Child, UseFancyColors, SaveLocation);
            }

            //General
            public string GeneralSaveLocation;
            public bool GeneralUseFancyColors;
            //Inventor
            public Color InventorChildColor;
        }

        /// <summary>
        /// Sets the <see cref="Color"/> of the <see cref="Button"/> and by extension the <see cref="PluginSettingsValues"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChildHighlight_Click(object sender, EventArgs e)
        {
            ColorDialog colorChoose = new ColorDialog();
            if(colorChoose.ShowDialog() == DialogResult.OK)
            {
                ChildHighlight.BackColor = colorChoose.Color;
            }
        }
        
        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.ShowDialog();
            if(folderBrowser.SelectedPath != null)
            {
                Values.GeneralSaveLocation = folderBrowser.SelectedPath;
            }
        }

        private void UseFancyColorsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (UseFancyColorsCheckBox.Checked)
            {
                UseFancyColorsCheckBox.ForeColor = Color.Red;
            }
            else
                UseFancyColorsCheckBox.ForeColor = DefaultForeColor;
        }

    }
}
