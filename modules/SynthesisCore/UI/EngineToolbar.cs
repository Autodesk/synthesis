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

            UIManager.AddPanel(new EntitiesWindow().Panel);
            UIManager.AddPanel(new EnvironmentsWindow().Panel);

            var engineTab = new Tab("Engine", Ui.ToolbarAsset, toolbarElement => {
                var designCategory = ToolbarTools.AddButtonCategory(toolbarElement, "ENTITIES");
                ToolbarTools.AddButton(designCategory, "add-entity-button", "/modules/synthesis_core/UI/images/add-icon.png",
                    _ => UIManager.TogglePanel("Entities"));
                ToolbarTools.AddButton(designCategory, "change-environment-button", "/modules/synthesis_core/UI/images/environments-icon.png",
                    _ => UIManager.TogglePanel("Environments"));
            });
            
            UIManager.AddTab(engineTab);
            UIManager.SetDefaultTab(engineTab.Name);
            
            toolbarCreated = true;
        }

    }

}
