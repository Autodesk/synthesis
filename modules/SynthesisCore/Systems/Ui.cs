using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;

namespace SynthesisCore.Systems
{
    [ModuleExport]
    public class Ui : SystemBase
    {
        public override void Setup()
        {
            var tabAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Tab.uxml");
            var environmentsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Environments.uxml");
            var modulesAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Modules.uxml");
            var settingsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Settings.uxml");
            var jointsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Joints.uxml");

            Tab engineTab = new Tab("Engine", tabAsset, _ => { });
            Tab jointsTab = new Tab("Joints Tab", tabAsset, _ => { });
            Panel environmentsWindow = new Panel("Environments", environmentsAsset,
                element => RegisterOKCloseButtons(element, "Environments"));
            Panel modulesWindow = new Panel("Modules", modulesAsset,
                element => RegisterOKCloseButtons(element, "Modules"));
            Panel settingsWindow = new Panel("Settings", settingsAsset,
                element => RegisterOKCloseButtons(element, "Settings"));
            Panel jointsWindow = new Panel("Joints", jointsAsset,
                element => RegisterOKCloseButtons(element, "Joints"));

            Logger.RegisterLogger(new ToastLogger());

            UIManager.AddTab(engineTab);
            UIManager.AddTab(jointsTab);
            UIManager.AddPanel(environmentsWindow);
            UIManager.AddPanel(modulesWindow);
            UIManager.AddPanel(settingsWindow);
            UIManager.AddPanel(jointsWindow);

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
            Button jointsButton = (Button)UIManager.RootElement.Get("joints-button");
            jointsButton.Subscribe(x =>
            {
                UIManager.TogglePanel("Joints");
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

        public override void OnUpdate()
        {
            ToastLogger.ScrollToBottom();
        }

        public override void Teardown() { }
    }
}