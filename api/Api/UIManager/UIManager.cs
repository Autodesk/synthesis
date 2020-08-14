using System;
using System.Collections.Generic;
using SynthesisAPI.Runtime;
using SynthesisAPI.UIManager.UIComponents;
using UnityEngine.UIElements;
using UnityVisualElement = UnityEngine.UIElements.VisualElement;
using UnityButton = UnityEngine.UIElements.Button;
using VisualElement = SynthesisAPI.UIManager.VisualElements.VisualElement;
using SynthesisAPI.AssetManager;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.UIManager
{
    public static class UIManager
    {
        private const string SelectedTabBlankName = "__";

        public static VisualElement RootElement {
            get => ApiProvider.GetRootVisualElement()?.GetVisualElement();
        }
        private static UnityVisualElement CreateTab(string tabName) => BlankTabAsset.GetElement(tabName).UnityVisualElement;

        public static void SetBlankTabAsset(VisualElementAsset blankTabAsset)
        {
            if (blankTabAsset.GetElement("test").Get("blank-tab") == null)
            {
                throw new SynthesisException("Blank tab asset must have an element with name \"blank-tab\"");
            }
            BlankTabAsset = blankTabAsset;
        }

        public static void SetTitleBar(VisualElement titleBarElement)
        {
            if(titleBarElement.Get("tab-container") == null)
            {
                throw new SynthesisException("Title bar must have an element with name \"tab-container\"");
            }
            foreach (var i in Instance.TitleBarContainer.Children())
            {
                Instance.TitleBarContainer.Remove(i);
            }
            Instance.TitleBarContainer.Add(titleBarElement.UnityVisualElement);
        }
        public static void AddTab(Tab tab)
        {
            tab.buttonElement = new VisualElements.Button(CreateTab(tab.Name).Q<UnityButton>(name: "blank-tab"));
            LoadedTabs.Add(tab.Name, tab);
            // TODO: Spawn in tab button
            tab.buttonElement.Element.name = $"tab-{tab.Name}";
            tab.buttonElement.Element.text = tab.Name;
            tab.buttonElement.Element.clickable.clicked += () =>
                EventBus.EventBus.Push("ui/select-tab", new SelectTabEvent(tab.Name));
            Instance.TabContainer.Add(tab.buttonElement.Element);
            if (SelectedTabName == SelectedTabBlankName && tab.Name == DefaultSelectedTabName)
                SelectTab(tab.Name);
        }

        public static void RemoveTab(string tabName)
        {
            if (LoadedTabs.Remove(tabName))
            {
                var container = RootElement.UnityVisualElement.Q<UnityVisualElement>(name: "tab-container");
                container.Remove(container.Q<UnityVisualElement>(name: $"tab-{tabName}"));

                if (tabName == SelectedTabName)
                    SelectTab(DefaultSelectedTabName);
            }
        }

        public static void SelectTab(string tabName)
        {
            if (SelectedTabName == tabName)
            {
                return;
            }
            // Remove Existing
            SetToolbarVisible(false, false);

            if (tabName == SelectedTabBlankName)
            {
                SelectedTabName = SelectedTabBlankName;
            }
            else
            {
                if (LoadedTabs[tabName].ToobarAsset == null)
                    throw new Exception($"UI for tab \"{tabName}\" is null");

                SelectedTabName = tabName;

                SetToolbarVisible(IsToolbarVisible);
            }

            // Add class active-tab to currently selected tab
            foreach (var i in LoadedTabs)
            {
                if (i.Value.buttonElement != null)
                {
                    i.Value.buttonElement.RemoveFromClassList("active-tab");
                    i.Value.buttonElement.AddToClassList("inactive-tab");
                    StyleSheetManager.ApplyClassFromStyleSheets("inactive-tab", i.Value.buttonElement.UnityVisualElement);
                }
            }
            if (LoadedTabs.ContainsKey(SelectedTabName))
            {
                LoadedTabs[SelectedTabName].buttonElement.AddToClassList("active-tab");
                LoadedTabs[SelectedTabName].buttonElement.RemoveFromClassList("inactive-tab");
                StyleSheetManager.ApplyClassFromStyleSheets("active-tab", LoadedTabs[SelectedTabName].buttonElement.UnityVisualElement);
            }

            // TODO: Maybe some event
        }

        public static void SetDefaultTab(string tabName)
        {
            if (LoadedTabs.ContainsKey(tabName))
            {
                DefaultSelectedTabName = tabName;

                if (SelectedTabName == SelectedTabBlankName)
                    SelectTab(DefaultSelectedTabName);
            }
            else
            {
                Utilities.Logger.Log($"Cannot set default tab to non-existent tab {tabName}", Utilities.LogLevel.Warning);
            }
        }

        public static void ResetDefaultTab()
        {
            DefaultSelectedTabName = SelectedTabBlankName;
        }

        public static void SetToolbarVisible(bool visible, bool rememberVisibility = true)
        {
            if(rememberVisibility)
                IsToolbarVisible = visible;
            var toolbarContainer = Instance.ToolbarContainer;
            if (!visible) // Remove toolbar
            {
                var existingToolbar = toolbarContainer.Q(name: "active-toolbar");
                if (existingToolbar != null)
                    toolbarContainer.Remove(existingToolbar);
            }
            else if(LoadedTabs.ContainsKey(SelectedTabName)) // Add toolbar
            {
                var toolbar = LoadedTabs[SelectedTabName].ToobarAsset.GetElement("active-toolbar");
                LoadedTabs[SelectedTabName].BindToolbar(toolbar);
                // toolbar.VisualElement.AddToClassList("custom-toolbar"); // May cause some kind of error
                toolbarContainer.Add(toolbar.UnityVisualElement);
            }
        }

        public static void AddPanel(Panel panel)
        {
            LoadedPanels.Add(panel.Name, panel);
        }

        public static void ShowPanel(string panelName)
        {
            var existingPanel = Instance.PanelContainer.Q(name: $"panel-{panelName}");
            if (existingPanel == null)
            {
                UnityVisualElement elm = LoadedPanels[panelName].Ui.GetElement($"panel-{panelName}").UnityVisualElement;
                Instance.PanelContainer.Add(elm);
                LoadedPanels[panelName].BindFunc(elm.GetVisualElement());
            }
            
            // TODO: Maybe some event
        }

        public static void ClosePanel(string panelName)
        {
            var existingPanel = Instance.PanelContainer.Q(name: $"panel-{panelName}");
            if (existingPanel != null)
                Instance.PanelContainer.Remove(existingPanel);
        }

        public static void TogglePanel(string panelName)
        {
            var panelToToggle = Instance.PanelContainer.Q(name: $"panel-{panelName}");
            if (panelToToggle == null)
            {
                ShowPanel(panelName);
            }
            else
            {
                ClosePanel(panelName);
            }
        }

        private class Inner
        {
            public UnityVisualElement PanelContainer
            {
                get => RootElement.UnityVisualElement.Q(name: "panel-center");
            }

            public UnityVisualElement TabContainer
            {
                get => RootElement.UnityVisualElement.Q(name: "tab-container");
            }

            public UnityVisualElement TitleBarContainer
            {
                get => RootElement.UnityVisualElement.Q(name: "title-bar-container");
            }

            public UnityVisualElement ToolbarContainer
            {
                get
                {
                    var toolbarContainer = RootElement.UnityVisualElement.Q(name: "bottom");
                    if (toolbarContainer == null)
                        throw new Exception("Could not find toolbar container");
                    return toolbarContainer;
                }
            }

            public VisualElementAsset BlankTabAsset;
            
            public string SelectedTabName = SelectedTabBlankName;
            public string DefaultSelectedTabName = SelectedTabBlankName;
            public bool IsToolbarVisible = true;

            public Dictionary<string, Tab> LoadedTabs;
            public Dictionary<string, Panel> LoadedPanels;

            private Inner()
            {
                LoadedTabs = new Dictionary<string, Tab>();
                LoadedPanels = new Dictionary<string, Panel>();
                
                // Setup events
                EventBus.EventBus.NewTagListener($"ui/select-tab", info =>
                {
                    string tabName = (info as SelectTabEvent)!.TabName;
                    UIManager.SelectTab(tabName);
                    // Logger.Log($"Selecting Tab: {tabName}");
                });
            }
            
            private static Inner _instance = null!;

            public static Inner InnerInstance
            {
                get
                {
                    if (_instance == null)
                        _instance = new Inner();
                    return _instance;
                }
            }
        }

        private static Inner Instance => Inner.InnerInstance;
        private static Dictionary<string, Tab> LoadedTabs => Instance.LoadedTabs;
        private static Dictionary<string, Panel> LoadedPanels => Instance.LoadedPanels;
        private static string SelectedTabName {
            get => Instance.SelectedTabName;
            set => Instance.SelectedTabName = value;
        }

        private static bool IsToolbarVisible
        {
            get => Instance.IsToolbarVisible;
            set => Instance.IsToolbarVisible = value;
        }
        private static string DefaultSelectedTabName
        {
            get => Instance.DefaultSelectedTabName;
            set => Instance.DefaultSelectedTabName = value;
        }
        private static VisualElementAsset BlankTabAsset
        {
            get => Instance.BlankTabAsset;
            set => Instance.BlankTabAsset = value;
        }
    }
}
