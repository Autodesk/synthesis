using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace EditorsLibrary
{
    public partial class ViewerSettingsForm : Form
    {

        public ViewerSettingsValues values;

        public ViewerSettingsForm(ViewerSettingsValues defaultValues)
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

        private void LoadValues()
        {
            trackbarCameraSen.Value = values.cameraSensitivity;
            checkboxDebug.Checked = values.cameraDebugMode;

            checkboxHighlight.Checked = values.modelHighlight;
            buttonChooseHighlight.BackColor = Color.FromArgb((int) values.modelHighlightColor);
            checkboxTint.Checked = values.modelTint;
            buttonChooseTint.BackColor = Color.FromArgb((int) values.modelTintColor);
            checkboxDrawAxes.Checked = values.modelDrawAxes;
            checkboxActuate.Checked = values.modelActuateJoints;

            checkboxHighlight_CheckedChanged(null, null); //To make sure that things are enabled/disabled correctly
            checkboxTint_CheckedChanged(null, null);
        }

        private void SaveValues()
        {
            values.cameraSensitivity = trackbarCameraSen.Value;
            values.cameraDebugMode = checkboxDebug.Checked;

            values.modelHighlight = checkboxHighlight.Checked;
            values.modelHighlightColor = (uint) buttonChooseHighlight.BackColor.ToArgb();
            values.modelTint = checkboxTint.Checked;
            values.modelTintColor = (uint) buttonChooseTint.BackColor.ToArgb();
            values.modelDrawAxes = checkboxDrawAxes.Checked;
            values.modelActuateJoints = checkboxActuate.Checked;
        }

        private void checkboxHighlight_CheckedChanged(object sender, EventArgs e)
        {
            bool enabled = checkboxHighlight.Checked;

            labelHighlightColor.Enabled = enabled;
            buttonChooseHighlight.Enabled = enabled;
            checkboxTint.Enabled = enabled;
            labelTintColor.Enabled = enabled && checkboxTint.Checked;
            buttonChooseTint.Enabled = enabled && checkboxTint.Checked;
            checkboxDrawAxes.Enabled = enabled;
        }

        private void buttonChooseHighlight_Click(object sender, EventArgs e)
        {
            ColorDialog colorChooser = new ColorDialog();
            colorChooser.ShowDialog();
            buttonChooseHighlight.BackColor = colorChooser.Color;
        }

        private void checkboxTint_CheckedChanged(object sender, EventArgs e)
        {
            bool enabled = checkboxTint.Checked;

            labelTintColor.Enabled = enabled;
            buttonChooseTint.Enabled = enabled;
        }

        private void buttonChooseTint_Click(object sender, EventArgs e)
        {
            ColorDialog colorChooser = new ColorDialog();
            colorChooser.ShowDialog();
            buttonChooseTint.BackColor = colorChooser.Color;
        }

        public static ViewerSettingsValues GetDefaultSettings()
        {
            ViewerSettingsValues defaultValues = new ViewerSettingsValues();

            defaultValues.cameraSensitivity = 5;
            defaultValues.cameraDebugMode = false;

            defaultValues.modelHighlight = true;
            defaultValues.modelHighlightColor = 0xFF0000FF;
            defaultValues.modelTint = true;
            defaultValues.modelTintColor = 0xFF00FF00;
            defaultValues.modelDrawAxes = false;
            defaultValues.modelActuateJoints = true;

            return defaultValues;
        }

        public struct ViewerSettingsValues
        {
            public int cameraSensitivity;
            public bool cameraDebugMode;

            public bool modelHighlight;
            public uint modelHighlightColor;
            public bool modelTint;
            public uint modelTintColor;
            public bool modelDrawAxes;
            public bool modelActuateJoints;
        }

    }
}
