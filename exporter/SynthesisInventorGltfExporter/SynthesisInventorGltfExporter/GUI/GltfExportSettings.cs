using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Protobuf;
using Inventor;
using SynthesisInventorGltfExporter.Properties;
using Application = Inventor.Application;
using FileDialog = Inventor.FileDialog;
using Path = Inventor.Path;

namespace SynthesisInventorGltfExporter.GUI
{
    public partial class GltfExportSettings : Form
    {
        public GltfExportSettings(Application application)
        {
            InitializeComponent();
            LoadSettings();
            
            // Create the ToolTip and associate with the Form container.
            ToolTip toolTip = new ToolTip();

            // Set up the delays for the ToolTip.
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 1000;
            toolTip.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip.ShowAlways = true;
      
            // Set up the ToolTip text for the Button and Checkbox.
            toolTip.SetToolTip(checkMaterials, "Include visual appearances in the exported model.");
            toolTip.SetToolTip(checkFace, "Include visual appearances assigned to faces in the exported model.");
            toolTip.SetToolTip(checkHidden, "Export components even if they are not visible in the viewport.");
            var toleranceLabel = "Allowed tolerance distance between true curved surfaces and their generated triangular mesh approximations. Lower distances result in higher quality.";
            toolTip.SetToolTip(numericTolerance, toleranceLabel);
            toolTip.SetToolTip(meshToleranceLabel, toleranceLabel);
            toolTip.SetToolTip(includeSynth, "Include Synthesis-specific metadata about joints and physical properties.");
            var extLabel = "File extension of the exported glTF file.";
            toolTip.SetToolTip(fileTypeLabel, extLabel);
            toolTip.SetToolTip(comboFileType, extLabel);
            
            okButton.Click += (sender, args) =>
            {
                var assemblyDocument = application.ActiveDocument as AssemblyDocument;
                SaveSettings();
                FileDialog dialog;
                application.CreateFileDialog(out dialog);
                dialog.DialogTitle = "Export assembly as glTF";
                dialog.Filter = Settings.Default.ExportGLB ? "glTF Binary (*.glb)|*.glb" : "glTF JSON (*.gltf)|*.gltf";
                dialog.FilterIndex = 1;
                dialog.InitialDirectory = Settings.Default.ExportFolder;
                var filename = assemblyDocument.DisplayName;
                foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                {
                    filename = filename.Replace(c, '_');
                }
                dialog.FileName = System.IO.Path.GetFileNameWithoutExtension(filename);
                dialog.InsertMode = true;
                dialog.OptionsEnabled = false;
                dialog.MultiSelectEnabled = false;
                dialog.CancelError = true;

                try
                {
                    dialog.ShowSave();
                }
                catch
                {
                    return; // Cancel button pressed
                }

                var exporter = new GLTFDesignExporter();
                exporter.ExportActiveDesign(application, assemblyDocument, dialog.FileName, Settings.Default.ExportGLB,checkMaterials.Checked, checkFace.Checked, checkHidden.Checked, numericTolerance.Value, includeSynth.Checked);
                Close();
            };
            cancelButton.Click += (sender, args) =>
            {
                Close();
            };
        }

        private void LoadSettings()
        {
            checkMaterials.Checked = Settings.Default.ExportMaterials;
            checkFace.Checked = Settings.Default.ExportFaceMaterials;
            checkHidden.Checked = Settings.Default.ExportHidden;
            numericTolerance.Value = Settings.Default.MeshTolerance;
            includeSynth.Checked = Settings.Default.IncludeSynthesisData;
            comboFileType.SelectedIndex = Settings.Default.ExportGLB ? 0 : 1;
            checkMaterials_CheckedChanged(null, null);
        }

        private void SaveSettings()
        {
            Settings.Default.ExportMaterials = checkMaterials.Checked;
            Settings.Default.ExportFaceMaterials = checkFace.Checked;
            Settings.Default.ExportHidden = checkHidden.Checked;
            Settings.Default.MeshTolerance = numericTolerance.Value;
            Settings.Default.IncludeSynthesisData = includeSynth.Checked;
            Settings.Default.ExportGLB = comboFileType.SelectedIndex == 0;
            Settings.Default.Save();
        }

        private void checkMaterials_CheckedChanged(object sender, EventArgs e)
        {
            if (checkMaterials.Checked)
            {
                checkFace.Enabled = true;
            }
            else
            {
                checkFace.Enabled = false;
                checkFace.Checked = false;
            }
        }
    }
}
