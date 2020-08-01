using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Inventor;
using SynthesisInventorGltfExporter.Properties;
using SynthesisInventorGltfExporter.GUI;
using static SynthesisInventorGltfExporter.PictureDispConverter;

namespace SynthesisInventorGltfExporter
{
    /// <summary>
    /// This is where the magic happens. All top-level event handling, UI creation, and inventor communication is handled here.
    /// </summary>
    [Guid("fd06cbaa-b8e9-4c50-b78e-601cc74766fb")]
    public class RobotExporterAddInServer : RibbonAddInServer
    {
        private const string clientId = "{fd06cbaa-b8e9-4c50-b78e-601cc74766fb}";
        private ButtonDefinition settingsButton;
        private ButtonDefinition exportButton;
        private GltfExportSettings exporterSettingsForm;

        protected override void ConfigureRibbon()
        {
            UserInterfaceManager userInterfaceManager = Application.UserInterfaceManager;
            if (userInterfaceManager.InterfaceStyle == InterfaceStyleEnum.kRibbonInterface)
            {
                Ribbons ribbons;
                ribbons = userInterfaceManager.Ribbons;

                Ribbon partRibbon;
                partRibbon = ribbons["Assembly"];

                RibbonTabs ribbonTabs;
                ribbonTabs = partRibbon.RibbonTabs;

                RibbonTab exporterRibbonTab;
                exporterRibbonTab = ribbonTabs.Add("Synthesis glTF Exporter", "SynthesisGltfExporter:RibbonTab", clientId);

                RibbonPanels ribbonPanels;
                ribbonPanels = exporterRibbonTab.RibbonPanels;

                var exportPanel = ribbonPanels.Add("Export", "SynthesisGltfExporter:ExportPanel", clientId);
                // var settingsPanel = ribbonPanels.Add("Settings", "SynthesisGltfExporter:SettingsPanel", clientId);

                // settingsPanel.CommandControls.AddButton(settingsButton, true);
                exportPanel.CommandControls.AddButton(exportButton, true);
            }
        }

        protected override void ConfigureButtons()
        {
            // settingsButton = Application.CommandManager.ControlDefinitions.AddButtonDefinition("glTF Export Settings", "SynthesisGltfExporter:SettingsButton", CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "Configure add-in settings.", ToIPictureDisp(new Bitmap(Resources.Gears16)), ToIPictureDisp(new Bitmap(Resources.Gears32)));
            // settingsButton.OnExecute += context =>
            // {
            //     MessageBox.Show("Settings button!");
            // };

            exporterSettingsForm = new GltfExportSettings(Application);
            
            exportButton = Application.CommandManager.ControlDefinitions.AddButtonDefinition("Export glTF", "SynthesisGltfExporter:ExportButton", CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "Configure add-in settings.", ToIPictureDisp(new Bitmap(Resources.SynthesisLogo16)), ToIPictureDisp(new Bitmap(Resources.SynthesisLogo32)));
            exportButton.OnExecute += context =>
            {
                exporterSettingsForm.Show();
            };

        }

        protected override void FirstLaunch()
        {
            MessageBox.Show(Resources.FirstLaunchMessage, Resources.FirstLaunchMessageTitle, MessageBoxButtons.OK);
        }
    }
}