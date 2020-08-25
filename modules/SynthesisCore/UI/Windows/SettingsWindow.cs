using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisCore.UI
{
    public class SettingsWindow
    {
        public Panel Panel { get; }
        private VisualElement Window;
        private GeneralPage GeneralPage;
        private GraphicsPage GraphicsPage;
        private ControlsPage ControlsPage;

        public SettingsWindow()
        {
            var generalAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/General.uxml");
            var graphicsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Graphics.uxml");
            var controlsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Controls.uxml");
            
            GeneralPage = new GeneralPage(generalAsset);
            GraphicsPage = new GraphicsPage(graphicsAsset);
            ControlsPage = new ControlsPage(controlsAsset);
            
            var settingsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Settings.uxml");
            Panel = new Panel("Settings", settingsAsset, OnWindowOpen);
            
            Button settingsButton = (Button) UIManager.RootElement.Get("settings-button");
            settingsButton.Subscribe(x => UIManager.TogglePanel("Settings"));
        }

        private void OnWindowOpen(VisualElement settingsWindow)
        {
            Window = settingsWindow;

            RegisterButtons();
            LoadWindowContent();
        }

        private void LoadWindowContent()
        {
            SetPageContent(GeneralPage.Page);
        }

        private void RegisterButtons()
        {
            // TODO: save / apply changes implementation?

            Button generalSettingsButton = (Button) Window.Get("general-settings-button");
            generalSettingsButton?.Subscribe(x =>
            {
                SetPageContent(GeneralPage.Page);
            });
            
            Button graphicsSettingsButton = (Button) Window.Get("graphics-settings-button");
            graphicsSettingsButton?.Subscribe(x =>
            {
                SetPageContent(GraphicsPage.Page);
            });
            
            Button controlsSettingsButton = (Button) Window.Get("controls-settings-button");
            controlsSettingsButton?.Subscribe(x =>
            {
                SetPageContent(ControlsPage.Page);
            });
            
            Button okButton = (Button) Window.Get("ok-button");
            okButton?.Subscribe(x => UIManager.ClosePanel(Panel.Name));

            Button closeButton = (Button) Window.Get("close-button");
            closeButton?.Subscribe(x => UIManager.ClosePanel(Panel.Name));
        }

        private void SetPageContent(VisualElement newContent)
        {
            VisualElement pageContainer = Window.Get("page-container");
            foreach (VisualElement child in pageContainer.GetChildren())
            {
                child.RemoveFromHierarchy();
            }
            pageContainer.Add(newContent);
        }

    }
}