using System;
using System.Collections.Generic;
using SynthesisAPI.Runtime;
using SynthesisAPI.UIManager.UIComponents;
using UnityEngine.UIElements;
using UnityVisualElement = UnityEngine.UIElements.VisualElement;
using UnityButton = UnityEngine.UIElements.Button;
using VisualElement = SynthesisAPI.UIManager.VisualElements.VisualElement;

// TODO for Nicolas: Move all panels (modules, settings, etc.) into Engine Module
namespace SynthesisAPI.UIManager
{
    public static class UIManager
    {
        private static UnityVisualElement CreateTab() =>
            ApiProvider.GetDefaultUIAsset("BlankTabAsset").CloneTree();
        
        public static void AddTab(Tab tab)
        {
            LoadedTabs.Add(tab.Name, tab);
            // TODO: Spawn in tab button
            var newButton = CreateTab().Q<UnityButton>(name: "blank-tab");
            newButton.name = $"tab-{tab.Name}";
            newButton.text = tab.Name;
            newButton.clickable.clicked += () =>
                EventBus.EventBus.Push("ui/select-tab", new SelectTabEvent(tab.Name));
            Instance.TabContainer.Add(newButton);
        }

        public static void RemoveTab(string tabName)
        {
            if (LoadedTabs.Remove(tabName))
            {
                var container = Instance.RootElement.UnityVisualElement.Q<UnityVisualElement>(name: "tab-container");
                container.Remove(container.Q<UnityVisualElement>(name: $"tab-{tabName}"));
            }
        }

        public static void SelectTab(string tabName)
        {
            var toolbarContainer = Instance.ToolbarContainer;
            if (toolbarContainer == null)
                throw new Exception("ToolbarContainer is null");

            if (tabName.Equals("__"))
            {
                var existingToolbar = toolbarContainer.Q(name: "active-toolbar");
                if (existingToolbar != null)
                    toolbarContainer.Remove(existingToolbar);

                Instance.SelectedTab = "__";
            }
            else
            {
                // Remove Existing
                var existingToolbar = toolbarContainer.Q(name: "active-toolbar");
                if (existingToolbar != null)
                    toolbarContainer.Remove(existingToolbar);

                if (Instance.SelectedTab == tabName)
                { 
                    Instance.SelectedTab = "__";
                }
                else
                {
                    if (LoadedTabs[tabName].Ui == null)
                        throw new Exception($"UI for tab \"{tabName}\" is null");
                    
                    // Add toolbar
                    var toolbar = LoadedTabs[tabName].Ui.GetElement("active-toolbar");
                    LoadedTabs[tabName].BindFunc(toolbar);
                    // toolbar.VisualElement.AddToClassList("custom-toolbar"); // May cause some kind of error
                    toolbarContainer.Add((UnityVisualElement) toolbar);

                    Instance.SelectedTab = tabName; 
                }
            }

            // TODO: Maybe some event
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

        private class Inner
        {
            public VisualElement RootElement
            {
                get => ApiProvider.GetRootVisualElement()!.GetVisualElement();
            }

            public UnityVisualElement PanelContainer
            {
                get => RootElement.UnityVisualElement.Q(name: "panel-center");
            }

            public UnityVisualElement TabContainer
            {
                get => RootElement.UnityVisualElement.Q(name: "tab-container");
            }

            public UnityVisualElement ToolbarContainer
            {
                get => RootElement.UnityVisualElement.Q(name: "bottom");
            }
            
            public string SelectedTab = "__";

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
                    // ApiProvider.Log($"Selecting Tab: {tabName}");
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

    }
}
