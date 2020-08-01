using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Protobuf;
using Inventor;
using SynthesisInventorGltfExporter.Properties;
using Application = Inventor.Application;

namespace SynthesisInventorGltfExporter.GUI
{
    public partial class GltfExportSettings : Form
    {
        public GltfExportSettings(Application application)
        {
            InitializeComponent();
            LoadSettings();
            
            okButton.Click += (sender, args) =>
            {
                SaveSettings();
                var exporter = new GLTFDesignExporter();
                exporter.ExportDesign(application.ActiveDocument as AssemblyDocument, checkMaterials.Checked, checkFace.Checked, checkHidden.Checked, numericTolerance.Value);
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
            checkMaterials_CheckedChanged(null, null);
        }

        private void SaveSettings()
        {
            Settings.Default.ExportMaterials = checkMaterials.Checked;
            Settings.Default.ExportFaceMaterials = checkFace.Checked;
            Settings.Default.ExportHidden = checkHidden.Checked;
            Settings.Default.MeshTolerance = numericTolerance.Value;
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
