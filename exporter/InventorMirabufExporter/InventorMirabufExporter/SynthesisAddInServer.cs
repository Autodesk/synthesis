using InvAddIn.Properties;
using Inventor;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static InventorMirabufExporter.PictureDispConverter;
using Application = Inventor.Application;

namespace InventorMirabufExporter
{
    /// <summary>
    /// This is the primary AddIn Server class that implements the ApplicationAddInServer interface
    /// that all Inventor AddIns are required to implement. The communication between Inventor and
    /// the AddIn is via the methods on this interface.
    /// </summary>
    [GuidAttribute("37135cf6-eae5-47c5-8ae8-c5204255b1fb")]
    public class SynthesisAddInServer : ApplicationAddInServer
    {
        public Application Application { get; private set; }
        private const string clientId = "{37135cf6-eae5-47c5-8ae8-c5204255b1fb}";
        private ButtonDefinition exporterButton;

        public void Activate(Inventor.ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            // This method is called by Inventor when it loads the addin.
            // The AddInSiteObject provides access to the Inventor Application object.
            // The FirstTime flag indicates if the addin is loaded for the first time.


            // TODO: Add ApplicationAddInServer.Activate implementation.
            // e.g. event initialization, command creation etc.

            Application = addInSiteObject.Application;

            try
            {
                ConfigureButtons();
                if (firstTime)
                {
                    ConfigureRibbon();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            Application.ApplicationEvents.OnActivateDocument += ApplicationEvents_OnActivateDocument;
        }

        private void ApplicationEvents_OnActivateDocument(_Document documentObject, EventTimingEnum beforeOrAfter, NameValueMap context, out HandlingCodeEnum handlingCode)
        {

            handlingCode = HandlingCodeEnum.kEventNotHandled;
        }

        protected void ConfigureRibbon()
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

                var exportPanel = ribbonPanels.Add("Synthesis", "SynthesisExporter:ExportPanel", clientId);

                exportPanel.CommandControls.AddButton(exporterButton, true);
            }
        }

        protected void ConfigureButtons()
        {
            exporterButton = Application.CommandManager.ControlDefinitions.AddButtonDefinition("Export Model", "SynthesisExporter:ExportButton", CommandTypesEnum.kNonShapeEditCmdType, clientId, null, "Exports the open assembly to a Mirabuf file.", ToIPictureDisp( new Bitmap(Resources.SynthesisLogo16)), ToIPictureDisp(new Bitmap(Resources.SynthesisLogo32)));
            exporterButton.OnExecute += context =>
            {
                MessageBox.Show("HELLO", "Synthesis: An Autodesk Technology", MessageBoxButtons.OK);
            };
        }

        protected void FirstLaunch()
        {
            MessageBox.Show("Synthesis Addin Loaded Successfully!", "Synthesis: An Autodesk Technology", MessageBoxButtons.OK);
        }

        public void Deactivate()
        {
            // This method is called by Inventor when the AddIn is unloaded.
            // The AddIn will be unloaded either manually by the user or
            // when the Inventor session is terminated

            // TODO: Add ApplicationAddInServer.Deactivate implementation

            // Release objects
            Marshal.ReleaseComObject(Application);
            Application = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void ExecuteCommand(int commandId) { }

        public object Automation => null;
    }
}
