using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisCore.UI.Windows;

namespace SynthesisCore.UI
{
    public class Ui : SystemBase
    {
        public static VisualElementAsset ToolbarAsset;
        public static VisualElementAsset ToolbarButtonAsset;
        public static VisualElementAsset ToolbarCategoryAsset;

        private static bool IsToolbarVisible = true;

        public override void Setup()
        {
            ToolbarAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Toolbar.uxml");
            ToolbarButtonAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/ToolbarButton.uxml");
            ToolbarCategoryAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/ToolbarCategory.uxml");
            
            var blankTabAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Tab.uxml");
            UIManager.SetBlankTabAsset(blankTabAsset);
            
            var titleBarAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/TitleBar.uxml");
            UIManager.SetTitleBar(titleBarAsset.GetElement("").Get(name: "title-bar"));

            EngineToolbar.CreateToolbar();
            EntityToolbar.CreateToolbar();
            
            var settingsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Settings.uxml");
            
            UIManager.AddPanel(new ModuleWindow().Panel);
            UIManager.AddPanel(new HelpWindow().Panel);

            Panel settingsWindow = new Panel("Settings", settingsAsset,
                element => Utilities.RegisterOKCloseButtons(element, "Settings"));

            UIManager.AddPanel(settingsWindow);

            Button hideToolbarButton = (Button)UIManager.RootElement.Get("hide-toolbar-button");
            hideToolbarButton.Subscribe(x => {
                hideToolbarButton.SetStyleProperty("background-image", IsToolbarVisible ? 
                    "/modules/synthesis_core/UI/images/toolbar-show-icon.png" : 
                    "/modules/synthesis_core/UI/images/toolbar-hide-icon.png");
                IsToolbarVisible = !IsToolbarVisible;
                UIManager.SetToolbarVisible(IsToolbarVisible);
            });
            
            Button settingsButton = (Button) UIManager.RootElement.Get("settings-button");
            settingsButton.Subscribe(x => UIManager.TogglePanel("Settings"));
        }
        public override void OnPhysicsUpdate() { }

        public override void OnUpdate() { }

        public override void Teardown() { }
    }
}