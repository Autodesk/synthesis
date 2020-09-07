using System.Collections.Generic;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager.InputEvents;
using SynthesisAPI.PreferenceManager;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
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
            
            Dictionary<string, object> defaultPreferences = new Dictionary<string, object>
            {
                {"Camera Forward", "W"},
                {"Camera Backward", "S"},
                {"Camera Left", "A"},
                {"Camera Right", "D"},
                {"Camera Up", "Space"},
                {"Camera Down", "LeftShift"},
                {"Entity Forward", "UpArrow"},
                {"Entity Backward", "DownArrow"},
                {"Entity Left", "LeftArrow"},
                {"Entity Right", "RightArrow"},
                {"Graphics Quality", "Low"},
                {"Screen Resolution", "1920x1080"},
                {"Screen Mode", "Windowed"},
                {"Analytics", "True"},
                {"Measurement Units", "Metric"}
            };

            PreferenceManager.Load();

            foreach (string option in defaultPreferences.Keys)
            {
                if (!PreferenceManager.ContainsPreference("SynthesisCore", option))
                {
                    PreferenceManager.SetPreference("SynthesisCore", option, defaultPreferences[option]);
                }
            }
            PreferenceManager.Save();

            var blankTabAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Tab.uxml");
            UIManager.SetBlankTabAsset(blankTabAsset);
            
            var titleBarAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/TitleBar.uxml");
            UIManager.SetTitleBar(titleBarAsset.GetElement("").Get(name: "title-bar"));

            EngineToolbar.CreateToolbar();
            EntityToolbar.CreateToolbar();
            
            UIManager.AddPanel(new SettingsWindow().Panel);
            UIManager.AddPanel(new ModuleWindow().Panel);
            UIManager.AddPanel(new HelpWindow().Panel);

            DialogInfo dialogInfo = new DialogInfo();
            dialogInfo.Title = "Update Available";
            dialogInfo.Prompt = "An update is available for Synthesis.";
            dialogInfo.Description =
                "Would you like to automatically upgrade your current Synthesis installation and restart?";
            dialogInfo.SubmitButtonText = "Update";
            dialogInfo.CloseButtonText = "Close";
            dialogInfo.SubmitButtonAction = ev =>
            {
                Logger.Log("Submit button clicked");
            };
            dialogInfo.CloseButtonAction = ev =>
            {
                Logger.Log("Close button clicked");
            };
            Dialog.SendDialog(dialogInfo);
            
            Button hideToolbarButton = (Button)UIManager.RootElement.Get("hide-toolbar-button");
            hideToolbarButton.Subscribe(x => {
                hideToolbarButton.SetStyleProperty("background-image", IsToolbarVisible ? 
                    "/modules/synthesis_core/UI/images/toolbar-show-icon.png" : 
                    "/modules/synthesis_core/UI/images/toolbar-hide-icon.png");
                IsToolbarVisible = !IsToolbarVisible;
                UIManager.SetToolbarVisible(IsToolbarVisible);
            });
        }
        public override void OnPhysicsUpdate() { }

        public override void OnUpdate() { }

        public override void Teardown() { }
    }
}
