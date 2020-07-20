using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SynthesisAPI.Runtime;
using SynthesisAPI.UIManager.UIComponents;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using UnityEngine.UIElements;

// TODO for Nicolas: Move all panels (modules, settings, etc.) into Engine Module
namespace SynthesisAPI.UIManager
{
    public static class UIManager
    {
        private static VisualElement CreateTab() =>
            ApiProvider.GetDefaultUIAsset("BlankTabAsset").CloneTree();
        
        public static void AddTab(Tab tab)
        {
            LoadedTabs.Add(tab.Name, tab);
            // TODO: Spawn in tab button
            var newButton = CreateTab().Q<Button>(name: "blank-tab");
            newButton.name = $"tab-{tab.Name}";
            newButton.text = tab.Name;
            newButton.clickable.clicked += () =>
                EventBus.EventBus.Push("ui/select-tab", new SelectTabEvent(tab.Name));
            ApiProvider.Log("Created new tab");
            Instance.TabContainer.Add(newButton);
            ApiProvider.Log("Added tab to top bar");
            
            RecursivePrint(Instance.TabContainer);
        }

        public static void RecursivePrint(VisualElement e, int depth = 0)
        {
            ApiProvider.Log(e.name);
            foreach (var child in e.Children())
            {
                RecursivePrint(child, depth + 1);
            }
        }

        public static void RemoveTab(string tabName)
        {
            if (LoadedTabs.Remove(tabName))
            {
                var container = Instance.RootElement.VisualElement.Q<VisualElement>(name: "tab-container");
                container.Remove(container.Q<VisualElement>(name: $"tab-{tabName}"));
            }
        }

        public static void SelectTab(string tabName)
        {
            var toolbarContainer = Instance.ToolbarContainer;
            if (toolbarContainer == null)
                ApiProvider.Log("ToolbarContainer Null");

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
                        ApiProvider.Log("Toolbar asset is null");
                    
                    // Add toolbar
                    var toolbar = LoadedTabs[tabName].Ui.GetElement("active-toolbar");
                    LoadedTabs[tabName].BindFunc(toolbar);
                    // toolbar.VisualElement.AddToClassList("custom-toolbar"); // May cause some kind of error
                    toolbarContainer.Add((VisualElement) toolbar);

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
                VisualElement elm = LoadedPanels[panelName].Ui.GetElement($"panel-{panelName}").VisualElement;
                Instance.PanelContainer.Add(elm);
                LoadedPanels[panelName].BindFunc(elm.GetSynVisualElement());
            }
            
            // TODO: Maybe some event
        }

        private static void ClosePanel(string panelName)
        {
            var existingPanel = Instance.PanelContainer.Q(name: $"panel-{panelName}");
            if (existingPanel != null)
                Instance.PanelContainer.Remove(existingPanel);
        }

        private class Inner
        {
            public SynVisualElement RootElement
            {
                get => ApiProvider.GetRootVisualElement()!.GetSynVisualElement();
            }

            public VisualElement PanelContainer
            {
                get => RootElement.VisualElement.Q(name: "panel-center");
            }

            public VisualElement TabContainer
            {
                get => RootElement.VisualElement.Q(name: "tab-container");
            }

            public VisualElement ToolbarContainer
            {
                get => RootElement.VisualElement.Q(name: "bottom");
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
                    ApiProvider.Log($"Selecting Tab: {tabName}");
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
