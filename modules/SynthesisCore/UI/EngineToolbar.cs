using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;

namespace SynthesisCore.UI
{
    public class EngineToolbar
    {
        private static bool toolbarCreated = false;

        public static void CreateToolbar()
        {
            if (toolbarCreated)
                return;

            var engineTab = new Tab("Engine", Ui.ToolbarAsset, toolbarElement => {
                var designCategory = ToolbarTools.AddButtonCategory(toolbarElement, "ENTITIES");
                ToolbarTools.AddButton(designCategory, "add-entity-button", "/modules/synthesis_core/UI/images/add-icon.png",
                    _ => Logger.Log("TODO"));
                ToolbarTools.AddButton(designCategory, "change-environment-button", "/modules/synthesis_core/UI/images/environments-icon.png",
                    _ => UIManager.TogglePanel("Environments"));
                ToolbarTools.AddButton(designCategory, "joints-button", "/modules/synthesis_core/UI/images/modules-icon.png",
                    _ => UIManager.TogglePanel("Joints"));
            });
            UIManager.AddTab(engineTab);
            UIManager.SetDefaultTab(engineTab.Name);

            var environmentsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Environments.uxml");
            Panel environmentsWindow = new Panel("Environments", environmentsAsset,
                element => Utilities.RegisterOKCloseButtons(element, "Environments"));

            UIManager.AddPanel(environmentsWindow);

            var jointsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Joints.uxml");
            Panel jointsWindow = new Panel("Joints", jointsAsset,
                element => Utilities.RegisterOKCloseButtons(element, "Joints"));

            UIManager.AddPanel(jointsWindow);

            toolbarCreated = true;
        }
    }
}
