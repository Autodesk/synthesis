using System.Diagnostics;
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
        private ButtonDefinition exportButton;

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
                exporterRibbonTab = ribbonTabs["id_TabEnvironments"];

                RibbonPanels ribbonPanels;
                ribbonPanels = exporterRibbonTab.RibbonPanels;

                var exportPanel = ribbonPanels.Add("Synthesis Exporter", "SynthesisGltfExporter:ExportPanel", clientId);

                exportPanel.CommandControls.AddButton(exportButton, true);
            }
        }

        protected override void ConfigureButtons()
        {
            exportButton = Application.CommandManager.ControlDefinitions.AddButtonDefinition("Export to glTF", "SynthesisGltfExporter:ExportButton", CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "Exports the open design to a glTF file.", ToIPictureDisp(new Bitmap(Resources.SynthesisLogo16)), ToIPictureDisp(new Bitmap(Resources.SynthesisLogo32)));
            exportButton.OnExecute += context =>
            {
                new GltfExportSettings(Application).ShowDialog();
            };
        }

        protected override void FirstLaunch()
        {
            MessageBox.Show(Resources.FirstLaunchMessage, Resources.FirstLaunchMessageTitle, MessageBoxButtons.OK);
        }
    }
}