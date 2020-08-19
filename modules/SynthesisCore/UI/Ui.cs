using System;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;

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
            
            var settingsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Settings.uxml");
            
            UIManager.AddPanel(new ModuleWindow().Panel);

            Panel settingsWindow = new Panel("Settings", settingsAsset,
                element => Utilities.RegisterOKCloseButtons(element, "Settings"));
            
            UIManager.AddPanel(settingsWindow);

            // DialogInfo dialogInfo = new DialogInfo();
            // dialogInfo.Title = "Update Available";
            // dialogInfo.Prompt = "An update is available for Synthesis.";
            // dialogInfo.Description =
            //     "Would you like to automatically upgrade your current Synthesis installation and restart?";
            // dialogInfo.SubmitButtonText = "Update";
            // dialogInfo.CloseButtonText = "Close";
            // dialogInfo.SubmitButtonAction = ev =>
            // {
            //     Logger.Log("Submit button clicked");
            // };
            // dialogInfo.CloseButtonAction = ev =>
            // {
            //     Logger.Log("Close button clicked");
            // };
            // Dialog.SendDialog(dialogInfo);
            // to be used when updater is added

            Button hideToolbarButton = (Button) UIManager.RootElement.Get("hide-toolbar-button");
            hideToolbarButton.Subscribe(x => {
                hideToolbarButton.SetStyleProperty("background-image", IsToolbarVisible ? 
                    "/modules/synthesis_core/UI/images/toolbar-show-icon.png" : 
                    "/modules/synthesis_core/UI/images/toolbar-hide-icon.png");
                IsToolbarVisible = !IsToolbarVisible;
                UIManager.SetToolbarVisible(IsToolbarVisible);
            });
            
            Button settingsButton = (Button) UIManager.RootElement.Get("settings-button");
            settingsButton.Subscribe(x => UIManager.TogglePanel("Settings"));

            Button helpButton = (Button) UIManager.RootElement.Get("help-button");
            helpButton.Subscribe(x => System.Diagnostics.Process.Start("https://synthesis.autodesk.com"));
        }

        public override void OnPhysicsUpdate() { }

        public override void OnUpdate() { }

        public override void Teardown() { }
    }
}