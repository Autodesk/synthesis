using SynthesisAPI.EventBus;
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

            var engineTab = new Tab("Engine", Ui.ToolbarAsset, toolbarElement =>
            {
                var designCategory = ToolbarTools.AddButtonCategory(toolbarElement, "ENVIRONMENT");
                ToolbarTools.AddButton(designCategory, "add-entity-button", "Add Entity", "/modules/synthesis_core/UI/images/add-entity-icon-2.png",
                    _ =>
                    {
                        UIManager.TogglePanel("Entities");
                        Analytics.LogEvent(Analytics.EventCategory.EngineTab, Analytics.EventAction.Clicked, "Add Entity Panel", 10);
                        Analytics.UploadDump();
                    });
                ToolbarTools.AddButton(designCategory, "change-environment-button", "Change Environment", "/modules/synthesis_core/UI/images/environments-icon.png",
                    _ =>
                    {
                        UIManager.TogglePanel("Environments");
                        Analytics.LogEvent(Analytics.EventCategory.EngineTab, Analytics.EventAction.Clicked, "Environments Panel", 10);
                        Analytics.UploadDump();
                    });
            });

            UIManager.AddTab(engineTab);
            UIManager.SetDefaultTab(engineTab.Name);

            // Start analytics timer for current tab
            Analytics.StartTime(Analytics.TimingLabel.EngineTab, Analytics.TimingVariable.Customizing, 0); 

            toolbarCreated = true;
        }
    }
}
