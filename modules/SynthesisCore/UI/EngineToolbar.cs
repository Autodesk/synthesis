using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisCore.UI.Windows;

namespace SynthesisCore.UI
{
    public static class EngineToolbar
    {
        private static bool toolbarCreated = false;

        public static void CreateToolbar()
        {
            if (toolbarCreated)
                return;

            UIManager.AddPanel(new EntitiesWindow().Panel);
            UIManager.AddPanel(new EnvironmentsWindow().Panel);
            UIManager.AddPanel(new JointsWindow().Panel);

            var engineTab = new Tab("Engine", Ui.ToolbarAsset, toolbarElement => {
                var designCategory = ToolbarTools.AddButtonCategory(toolbarElement, "ENVIRONMENT");
                ToolbarTools.AddButton(designCategory, "add-entity-button", "/modules/synthesis_core/UI/images/add-entity-icon-2.png",
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
