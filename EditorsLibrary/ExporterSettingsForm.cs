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
    public partial class ExporterSettingsForm : Form
    {

        /// <summary>
        /// The local copy of the setting values
        /// </summary>
        public ExporterSettingsValues values;

        public ExporterSettingsForm(ExporterSettingsValues defaultValues)
        {
            InitializeComponent();

            values = defaultValues;

            LoadValues();

            buttonOK.Click += delegate(object sender, EventArgs e)
            {
                SaveValues();

                Close();
            };

            buttonCancel.Click += delegate(object sender, EventArgs e)
            {
                Close();
            };
        }

        /// <summary>
        /// Load values into the form
        /// </summary>
        private void LoadValues()
        {
            checkboxSaveLog.Checked = values.generalSaveLog;
            textboxLogLocation.Text = values.generalSaveLogLocation;
            buttonChooseText.BackColor = Color.FromArgb((int) values.generalTextColor);
            buttonChooseBackground.BackColor = Color.FromArgb((int) values.generalBackgroundColor);

            checkboxSoftBodies.Checked = values.skeletonExportSoft;

            checkboxSaveLog_CheckedChanged(null, null); //To make sure things are enabled/disabled correctly
        }
        
        /// <summary>
        /// Save the form's values in a <see cref="ExporterSettingsValues"/> structure
        /// </summary>
        private void SaveValues()
        {
            values.generalSaveLog = checkboxSaveLog.Checked;
            values.generalSaveLogLocation = textboxLogLocation.Text;
            values.generalTextColor = (uint) buttonChooseText.BackColor.ToArgb();
            values.generalBackgroundColor = (uint)buttonChooseBackground.BackColor.ToArgb();

            values.skeletonExportSoft = checkboxSoftBodies.Checked;
        }

        /// <summary>
        /// Disable child controls under the "Save log to folder" checkbox
        /// </summary>
        /// <param name="sender">N/A</param>
        /// <param name="e">N/A</param>
        private void checkboxSaveLog_CheckedChanged(object sender, EventArgs e)
        {
            textboxLogLocation.Enabled = checkboxSaveLog.Checked;
            buttonChooseFolder.Enabled = checkboxSaveLog.Checked;
        }

        /// <summary>
        /// Choose the folder to save exporter logs in
        /// </summary>
        /// <param name="sender">N/A</param>
        /// <param name="e">N/A</param>
        private void buttonChooseFolder_Click(object sender, EventArgs e)
        {
            string dirPath = null;

            var dialogThread = new Thread(() =>
            {
                FolderBrowserDialog openDialog = new FolderBrowserDialog();
                openDialog.RootFolder = Environment.SpecialFolder.UserProfile;
                openDialog.ShowNewFolderButton = true;
                openDialog.Description = "Choose log folder";
                DialogResult openResult = openDialog.ShowDialog();

                if (openResult == DialogResult.OK) dirPath = openDialog.SelectedPath;
            });

            dialogThread.SetApartmentState(ApartmentState.STA);
            dialogThread.Start();
            dialogThread.Join();

            textboxLogLocation.Text = dirPath;
        }

        /// <summary>
        /// Choose the color of the exporter log text
        /// </summary>
        /// <param name="sender">N/A</param>
        /// <param name="e">N/A</param>
        private void buttonChooseText_Click(object sender, EventArgs e)
        {
            ColorDialog colorChooser = new ColorDialog();
            colorChooser.ShowDialog();
            buttonChooseText.BackColor = colorChooser.Color;
        }

        /// <summary>
        /// Choose the color of the exporter log background
        /// </summary>
        /// <param name="sender">N/A</param>
        /// <param name="e">N/A</param>
        private void buttonChooseBackground_Click(object sender, EventArgs e)
        {
            ColorDialog colorChooser = new ColorDialog();
            colorChooser.ShowDialog();
            buttonChooseBackground.BackColor = colorChooser.Color;
        }

        /// <summary>
        /// Get the default values for the <see cref="ExporterSettingsValues"/> structure
        /// </summary>
        /// <returns>Default values for the <see cref="Exporter"/></returns>
        public static ExporterSettingsValues GetDefaultSettings()
        {
            ExporterSettingsValues defaultValues = new ExporterSettingsValues();

            defaultValues.generalSaveLog = true;
            defaultValues.generalSaveLogLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BXD_Aardvark";
            defaultValues.generalTextColor = (uint) Color.Lime.ToArgb();
            defaultValues.generalBackgroundColor = 0xFF000000;

            defaultValues.skeletonExportSoft = false;

            defaultValues.meshResolutionValue = 0;
            defaultValues.meshFancyColors = false;

            return defaultValues;
        }

        /// <summary>
        /// The struct that stores settings for the <see cref="Exporter"/>
        /// </summary>
        public struct ExporterSettingsValues
        {
            public bool generalSaveLog;
            public string generalSaveLogLocation;
            public uint generalTextColor;
            public uint generalBackgroundColor;

            public bool skeletonExportSoft; //Not actually a thing yet

            public int meshResolutionValue;
            public bool meshFancyColors;
        }
        
    }
}
