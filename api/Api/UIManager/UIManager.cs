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
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.InputManager.InputEvents;

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
            foreach (var i in Instance.TitleBarContainer.GetChildren())
            {
                Instance.TitleBarContainer.Remove(i);
            }
            Instance.TitleBarContainer.Add(titleBarElement);
        }
        public static void AddTab(Tab tab)
        {
            if (LoadedTabs.ContainsKey(tab.Name))
            {
                Logger.Log($"Adding tab with duplicate name {tab.Name}", LogLevel.Warning);
            }
            else
            {
                tab.buttonElement = new VisualElements.Button(CreateTab(tab.Name).Q<UnityButton>(name: "blank-tab"));
                LoadedTabs.Add(tab.Name, tab);
                // TODO: Spawn in tab button
                tab.buttonElement.Element.name = $"tab-{tab.Name}";
                tab.buttonElement.Element.text = tab.Name;
                tab.buttonElement.Element.clickable.clicked += () =>
                    EventBus.EventBus.Push("ui/select-tab", new SelectTabEvent(tab.Name));
                Instance.TabContainer.Add(tab.buttonElement);
                if (SelectedTabName == SelectedTabBlankName && tab.Name == DefaultSelectedTabName)
                    SelectTab(tab.Name);
            }
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
                var existingToolbar = toolbarContainer.Get(name: "active-toolbar");
                if (existingToolbar != null)
                    toolbarContainer.Remove(existingToolbar);
            }
            else if(LoadedTabs.ContainsKey(SelectedTabName)) // Add toolbar
            {
                var toolbar = LoadedTabs[SelectedTabName].ToobarAsset.GetElement("active-toolbar");
                if (LoadedTabs[SelectedTabName].ToolbarElement == null)
                {
                    LoadedTabs[SelectedTabName].BindToolbar(toolbar);
                    var x = LoadedTabs[SelectedTabName];
                    x.ToolbarElement = toolbar;
                    LoadedTabs[SelectedTabName] = x;
                }
                // toolbar.VisualElement.AddToClassList("custom-toolbar"); // May cause some kind of error
                toolbarContainer.Add(LoadedTabs[SelectedTabName].ToolbarElement);
            }
        }

        public static void AddPanel(Panel panel)
        {
            LoadedPanels.Add(panel.Name, panel);
        }

        public static void ShowPanel(string panelName)
        {
            var existingPanel = Instance.PanelContainer.Get(name: $"panel-{panelName}");
            if (existingPanel == null)
            {
                Instance.PanelContainer.Enabled = true;
                var elm = LoadedPanels[panelName].Ui.GetElement($"panel-{panelName}");
                if (LoadedPanels[panelName].PanelElement == null)
                {
                    LoadedPanels[panelName].BindPanel(elm);
                    var x = LoadedPanels[panelName];
                    x.PanelElement = elm;
                    LoadedPanels[panelName] = x;
                }
                Instance.PanelContainer.Add(LoadedPanels[panelName].PanelElement);
            }
            
            // TODO: Maybe some event
        }

        public static void ClosePanel(string panelName)
        {
            var existingPanel = Instance.PanelContainer.Get(name: $"panel-{panelName}");
            if (existingPanel != null)
            {
                Instance.PanelContainer.Remove(existingPanel);
                Instance.PanelContainer.Enabled = ((List<VisualElement>)Instance.PanelContainer.GetChildren()).Count > 0;
            }
        }

        public static void TogglePanel(string panelName)
        {
            var panelToToggle = Instance.PanelContainer.Get(name: $"panel-{panelName}");
            if (panelToToggle == null)
            {
                ShowPanel(panelName);
            }
            else
            {
                ClosePanel(panelName);
            }
        }

        internal static void Setup()
        {
            Instance.SetupCatchAllMouseDown(RootElement.Get("catch-all-mouse-down"));
            Instance.PanelContainer.Enabled = false;
            UIManager.RootElement.Get("tooltip-container").Enabled = false;
        }

        private class Inner
        {
            private VisualElement _panelContainer = null;
            public VisualElement PanelContainer
            {
                get
                {
                    if(_panelContainer == null)
                        _panelContainer = RootElement.UnityVisualElement.Q(name: "panel-center").GetVisualElement();
                    return _panelContainer;
                }
            }

            private VisualElement _tabContainer = null;

            public VisualElement TabContainer
            {
                get
                {
                    if (_tabContainer == null)
                        _tabContainer = RootElement.UnityVisualElement.Q(name: "tab-container").GetVisualElement() ;
                    return _tabContainer;
                }
            }

            private VisualElement _titleBarContainer = null;

            public VisualElement TitleBarContainer
            {
                get
                {
                    if (_titleBarContainer == null)
                        _titleBarContainer = RootElement.UnityVisualElement.Q(name: "title-bar-container").GetVisualElement();
                    return _titleBarContainer;
                }
            }

            private VisualElement _toolbarContainer = null;

            public VisualElement ToolbarContainer
            {
                get
                {
                    if (_toolbarContainer == null)
                        _toolbarContainer = RootElement.UnityVisualElement.Q(name: "bottom").GetVisualElement();
                    return _toolbarContainer;
                }
            }

            public bool CursorBlockedByUI { get; private set; }

            private bool[] nonUIMouseDown = new bool[3];
            private bool nonUIMouseForwardingSetup = false;

            public void SetupCatchAllMouseDown(VisualElement visualElement)
            {
                visualElement.UnityVisualElement.RegisterCallback<MouseDownEvent>(e =>
                {
                    nonUIMouseDown[e.button] = true;
                    SendNonUIMouseEvent(e.button, DigitalState.Down);
                });
                visualElement.UnityVisualElement.RegisterCallback<MouseEnterEvent>(e =>
                {
                    CursorBlockedByUI = false;
                });
                visualElement.UnityVisualElement.RegisterCallback<MouseLeaveEvent>(e =>
                {
                    CursorBlockedByUI = true;
                });
                if (!nonUIMouseForwardingSetup)
                {
                    ForwardNonUIMouseEvent(0);
                    ForwardNonUIMouseEvent(1);
                    ForwardNonUIMouseEvent(2);
                    nonUIMouseForwardingSetup = true;
                }
            }

            private const string InputNameMouse0 = "mouse 0 non-ui";
            private const string InputNameMouse1 = "mouse 1 non-ui";
            private const string InputNameMouse2 = "mouse 2 non-ui";

            private void ForwardNonUIMouseEvent(int button)
            {
                InputManager.InputManager.AssignDigitalInput($"_internal mouse {button} non-ui-forwarder", new Digital($"mouse {button}"), e =>
                {
                    if (nonUIMouseDown[button])
                    {
                        var digitalEvent = (DigitalEvent)e;
                        if (digitalEvent.State == DigitalState.Up)
                        {
                            nonUIMouseDown[button] = false;
                            SendNonUIMouseEvent(0, DigitalState.Up);
                        }
                        else if (digitalEvent.State == DigitalState.Held)
                        {
                            SendNonUIMouseEvent(0, DigitalState.Held);
                        }
                    }
                });
            }
            private void SendNonUIMouseEvent(int button, DigitalState state)
            {
                foreach (string name in InputManager.InputManager._mappedDigitalInputs.Keys)
                {
                    foreach (Input input in InputManager.InputManager._mappedDigitalInputs[name])
                    {
                        if (input.Name.EndsWith("non-ui") && input is Digital digitalInput)
                        {
                            if ((digitalInput.Name == InputNameMouse0 && button == 0) ||
                                (digitalInput.Name == InputNameMouse1 && button == 1) ||
                                (digitalInput.Name == InputNameMouse2 && button == 2))
                            {
                                EventBus.EventBus.Push($"input/{name}", new DigitalEvent(name, state));
                            }
                        }
                    }
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
                set
                {
                    _instance = value;
                }
                get
                {
                    if (_instance == null)
                        _instance = new Inner();
                    return _instance;
                }
            }
        }

        private static Inner Instance
        {
            get => Inner.InnerInstance;
            set => Inner.InnerInstance = value;
        }
        private static Dictionary<string, Tab> LoadedTabs
        {
            get => Instance.LoadedTabs;
            set => Instance.LoadedTabs = value;
        }
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

        public static bool CursorBlockedByUI => Instance.CursorBlockedByUI;
    }
}
