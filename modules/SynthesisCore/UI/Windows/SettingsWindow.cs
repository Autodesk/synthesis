using System.Collections.Generic;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.PreferenceManager;
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
        private ControlsPage ControlsPage;

        private static Dictionary<string, object> PendingChanges = new Dictionary<string, object>();

        public SettingsWindow()
        {
            var generalAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/General.uxml");
            var controlsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Controls.uxml");

            GeneralPage = new GeneralPage(generalAsset);
            ControlsPage = new ControlsPage(controlsAsset);

            var settingsAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Settings.uxml");
            Panel = new Panel("Settings", settingsAsset, OnWindowCreate);

            Button settingsButton = (Button)UIManager.RootElement.Get("settings-button");
            settingsButton.Subscribe(x =>
            {
                UIManager.TogglePanel("Settings");
                Analytics.LogEvent(Analytics.EventCategory.MainSimulator, Analytics.EventAction.Clicked, "Settings Panel", 10);
                Analytics.UploadDump();
            });

            OnWindowClose();
        }

        private void OnWindowCreate(VisualElement settingsWindow)
        {
            Window = settingsWindow;
            Window.SetStyleProperty("position", "absolute");
            Window.IsDraggable = true;

            RegisterButtons();
            LoadWindowContent();
        }

        private void OnWindowClose()
        {
            EventBus.NewTypeListener<ClosePanelEvent>(info =>
            {
                if (info != null && ((ClosePanelEvent)info).Panel.Name.Equals("Settings"))
                {
                    GeneralPage.RefreshPreferences();
                    ControlsPage.RefreshPreferences();
                }
            });
        }

        private void LoadWindowContent()
        {
            SetPageContent(GeneralPage.Page);
        }

        private void RegisterButtons()
        {
            Button generalSettingsButton = (Button)Window.Get("general-settings-button");
            generalSettingsButton?.Subscribe(x =>
            {
                SetPageContent(GeneralPage.Page);
            });

            Button controlsSettingsButton = (Button)Window.Get("controls-settings-button");
            controlsSettingsButton?.Subscribe(x =>
            {
                SetPageContent(ControlsPage.Page);
            });

            Button okButton = (Button)Window.Get("ok-button");
            okButton?.Subscribe(x =>
            {
                if (PendingChanges.Count > 0)
                {
                    foreach (string key in PendingChanges.Keys)
                    {
                        PreferenceManager.SetPreference("SynthesisCore", key, PendingChanges[key]);
                    }
                }
                PreferenceManager.Save();

                UIManager.ClosePanel(Panel.Name);
            });

            Button closeButton = (Button)Window.Get("close-button");
            closeButton?.Subscribe(x =>
            {
                PendingChanges.Clear();

                UIManager.ClosePanel(Panel.Name);
            });
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

        public static void AddPendingChange(string key, object value)
        {
            PendingChanges[key] = value;
        }

    }
}