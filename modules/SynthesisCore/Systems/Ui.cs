using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Modules;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;

namespace SynthesisCore.Systems
{
    public class Ui : SystemBase
    {
        public static VisualElementAsset TabAsset;
        public static VisualElementAsset ToolbarAsset;
        public static VisualElementAsset ToolbarButtonAsset;

        private static bool IsToolbarVisible = true;

        public override void Setup()
        {
            TabAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Tab.uxml");
            var environmentsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Environments.uxml");
            var modulesAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Modules.uxml");
            var settingsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Settings.uxml");

            ToolbarAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Toolbar.uxml");
            ToolbarButtonAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/ToolbarButton.uxml");

            Tab engineTab = new Tab("Engine", ToolbarAsset, _ => { });
            Panel environmentsWindow = new Panel("Environments", environmentsAsset,
                element => RegisterOKCloseButtons(element, "Environments"));
            Panel modulesWindow = new Panel("Modules", modulesAsset, 
                element =>
                {
                    RegisterOKCloseButtons(element, "Modules");
                    LoadModulesWindowContent(element);
                });
            Panel settingsWindow = new Panel("Settings", settingsAsset,
                element => RegisterOKCloseButtons(element, "Settings"));

            UIManager.AddTab(engineTab);
            UIManager.SetDefaultTab(engineTab.Name);
            UIManager.AddPanel(environmentsWindow);
            UIManager.AddPanel(modulesWindow);
            UIManager.AddPanel(settingsWindow);

            Button environmentsButton = (Button) UIManager.RootElement.Get("environments-button");
            environmentsButton.Subscribe(x => UIManager.TogglePanel("Environments"));

            Button hideToolbarButton = (Button)UIManager.RootElement.Get("hide-toolbar-button");
            hideToolbarButton.Subscribe(x => {
                hideToolbarButton.SetStyleProperty("background-image", IsToolbarVisible ? 
                    "/modules/synthesis_core/UI/images/toolbar-show-icon.png" : 
                    "/modules/synthesis_core/UI/images/toolbar-hide-icon.png");
                IsToolbarVisible = !IsToolbarVisible;
                UIManager.SetToolbarVisible(IsToolbarVisible);
            });

            Button modulesButton = (Button) UIManager.RootElement.Get("modules-button");
            modulesButton.Subscribe(x => UIManager.TogglePanel("Modules"));

            Button settingsButton = (Button) UIManager.RootElement.Get("settings-button");
            settingsButton.Subscribe(x => UIManager.TogglePanel("Settings"));

            Button helpButton = (Button) UIManager.RootElement.Get("help-button");
            helpButton.Subscribe(x => System.Diagnostics.Process.Start("https://synthesis.autodesk.com"));
        }

        private void RegisterOKCloseButtons(VisualElement visualElement, string panelName)
        {
            Button okButton = (Button) visualElement.Get("ok-button");
            okButton?.Subscribe(x =>
            {
                UIManager.ClosePanel(panelName);
            });
            
            Button closeButton = (Button) visualElement.Get("close-button");
            closeButton?.Subscribe(x =>
            {
                UIManager.ClosePanel(panelName);
            });
        }

        private void LoadModulesWindowContent(VisualElement visualElement)
        {
            var moduleAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Module.uxml");

            foreach (var moduleInfo in ModuleManager.GetLoadedModules())
            {
                VisualElement moduleElement = moduleAsset?.GetElement("module");
                
                Label titleText = (Label) moduleElement?.Get("title");
                Label authorText = (Label) moduleElement?.Get("author");
                Label descriptionText = (Label) moduleElement?.Get("description");

                titleText.Text = titleText.Text
                    .Replace("%name%", moduleInfo.Name)
                    .Replace("%version%", moduleInfo.Version);

                authorText.Text = authorText.Text.Replace("%author%",
                    string.IsNullOrEmpty(moduleInfo.Author) ? "Unknown" : moduleInfo.Author);
                descriptionText.Text = descriptionText.Text.Replace("%description%",
                    string.IsNullOrEmpty(moduleInfo.Description) ? "No description" : moduleInfo.Description);

                ListView moduleList = (ListView) visualElement.Get("module-list");
                moduleList.Add(moduleElement);
            }
            
        }

        public override void OnPhysicsUpdate() { }

        public override void OnUpdate() { }

        public override void Teardown() { }
    }
}