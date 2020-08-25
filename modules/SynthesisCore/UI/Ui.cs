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
            
            Dictionary<string, string> defaultControls = new Dictionary<string, string>
            {
                {"Camera Forward", "W"},
                {"Camera Backward", "S"},
                {"Camera Left", "A"},
                {"Camera Right", "D"},
                {"Camera Up", "Space"},
                {"Camera Down", "Left Shift"},
                {"Entity Forward", "Up Arrow"},
                {"Entity Backward", "Down Arrow"},
                {"Entity Left", "Left Arrow"},
                {"Entity Right", "Right Arrow"}
            };
            
            PreferenceManager.SetPreferences("SynthesisCore", defaultControls);
            PreferenceManager.Save();
            
            var blankTabAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Tab.uxml");
            UIManager.SetBlankTabAsset(blankTabAsset);
            
            var titleBarAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/TitleBar.uxml");
            UIManager.SetTitleBar(titleBarAsset.GetElement("").Get(name: "title-bar"));

            EngineToolbar.CreateToolbar();
            
            UIManager.AddPanel(new SettingsWindow().Panel);
            UIManager.AddPanel(new ModuleWindow().Panel);
            
            Button hideToolbarButton = (Button)UIManager.RootElement.Get("hide-toolbar-button");
            hideToolbarButton.Subscribe(x => {
                hideToolbarButton.SetStyleProperty("background-image", IsToolbarVisible ? 
                    "/modules/synthesis_core/UI/images/toolbar-show-icon.png" : 
                    "/modules/synthesis_core/UI/images/toolbar-hide-icon.png");
                IsToolbarVisible = !IsToolbarVisible;
                UIManager.SetToolbarVisible(IsToolbarVisible);
            });

            Button helpButton = (Button) UIManager.RootElement.Get("help-button");
            helpButton.Subscribe(x => System.Diagnostics.Process.Start("https://synthesis.autodesk.com"));
            

        }

        public override void OnPhysicsUpdate() { }

        public override void OnUpdate() { }

        public override void Teardown() { }
    }
}