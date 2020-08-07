using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using System.Collections.Generic;

namespace SynthesisCore.Systems
{
    [ModuleExport]
    public class Ui : SystemBase
    {
        public override void Setup()
        {
            var tabAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/SynthesisCore/UI/uxml/Tab.uxml");
            var environmentsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/SynthesisCore/UI/uxml/Environments.uxml");
            var modulesAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/SynthesisCore/UI/uxml/Modules.uxml");
            var settingsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/SynthesisCore/UI/uxml/Settings.uxml");

            Tab engineTab = new Tab("Engine", tabAsset, null);
            Panel environmentsWindow = new Panel("Environments", environmentsAsset,
                element => RegisterOKCloseButtons(element, "Environments"));
            Panel modulesWindow = new Panel("Modules", modulesAsset,
                element => RegisterOKCloseButtons(element, "Modules"));
            Panel settingsWindow = new Panel("Settings", settingsAsset,
                element => RegisterOKCloseButtons(element, "Settings"));

            Logger.RegisterLogger(new ToastLogger());
            Logger.Log("Test log\nline 2");

            UIManager.AddTab(engineTab);
            UIManager.AddPanel(environmentsWindow);
            UIManager.AddPanel(modulesWindow);
            UIManager.AddPanel(settingsWindow);

            Button environmentsButton = (Button)UIManager.RootElement.Get("environments-button");
            environmentsButton.Subscribe(x =>
            {
                UIManager.TogglePanel("Environments");
            });

            Button modulesButton = (Button)UIManager.RootElement.Get("modules-button");
            modulesButton.Subscribe(x =>
            {
                UIManager.TogglePanel("Modules");
            });

            Button settingsButton = (Button)UIManager.RootElement.Get("settings-button");
            settingsButton.Subscribe(x =>
            {
                UIManager.TogglePanel("Settings");
            });
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

        public override void OnPhysicsUpdate() { }

        public override void OnUpdate() { }

        public override void Teardown() { }
    }
}