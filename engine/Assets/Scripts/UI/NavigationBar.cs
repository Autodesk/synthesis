using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Synthesis.UI.Tabs;
using Synthesis.UI.Dynamic;

using Image = UnityEngine.UI.Image;

namespace Synthesis.UI.Bars {
    // TODO: Needs a big rework. We'll tackle this with the rest of the UI system later
    public class NavigationBar : MonoBehaviour {
        // public GameObject homeTab;
        // public GameObject homeButton;

        public GameObject TopButtonContainer;
        public GameObject TopButtonPrefab;

        // TODO: Centralize colors
        public Color SelectedTopButtonColor;
        public Color UnselectedTopButtonColor;

        public TMP_Text VersionNumber;

        public TMP_FontAsset artifaktRegular;
        public TMP_FontAsset artifaktBold;

        private GameObject _currentTabButton;
        private GameObject _currentPanelButton;

        public NavigationBar navBarPrefab;

        private string lastOpenedPanel;

        public GameObject ModalTab;
        [Header("Tab Prefabs"), SerializeField]
        public GameObject TabPanelButtonPrefab;
        [SerializeField]
        public GameObject TabDividerPrefab;

        public static NavigationBar Instance { get; private set; }

        private string _currentTab = string.Empty;
        private Dictionary<string, (TopButton topButton, Tab tab)> _registeredTabs =
            new Dictionary<string, (TopButton topButton, Tab tab)>();

        // private readonly Color unselectedPanelButton = new Color(0.23529f,0.23529f,0.23529f,1);
        // private readonly Color selectedPanelButton = new Color(0.1f, 0.1f, 0.1f, 1);

        private void Start() {

            Instance = this;

            // TabButtonPrefabStatic = TabButtonPrefab;
            // TabDividerPrefabStatic = TabDividerPrefab;

            VersionNumber.text = $"v {AutoUpdater.LocalVersion}  ALPHA";

            RegisterTab("Home", new HomeTab());
            RegisterTab("Config", new ConfigTab());
            SelectTab("Home");

            // OpenTab(homeTab);
        }

        public void Exit() {
            if (Application.isEditor)
                Debug.Log("Would exit, but it's editor mode");
            else {
                var update = new AnalyticsEvent(category: "Exit", action: "Closed", label: $"Closed Synthesis");
                AnalyticsManager.LogEvent(update);
                AnalyticsManager.PostData();

                DynamicUIManager.CreateModal<ExitSynthesisModal>();
                // Application.Quit();
            }
        }

        // Eh?
        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                navBarPrefab.CloseAllPanels();
            }
        }

        public void PanelAnalytics(string prefabName, string status) {
            var panel = new AnalyticsEvent(category: "Panel", action: status, label: prefabName);
            AnalyticsManager.LogEvent(panel);
            AnalyticsManager.PostData();
        }

        public void OpenPanel(GameObject prefab) {
            if (prefab != null) {

                LayoutManager.OpenPanel(prefab, true);
                if (_currentPanelButton != null)
                    changePanelButton(artifaktRegular, 1f);

                // set current panel button to the button clicked
                _currentPanelButton = EventSystem.current.currentSelectedGameObject;
                changePanelButton(artifaktBold, 0.6f);

                // Analytics Stuff
                lastOpenedPanel = prefab.name; // this will need to be an array for movable panels
                PanelAnalytics(prefab.name, "Opened");
            }
        }

        public void CloseAllPanels() {
            LayoutManager.ClosePanel();
            if (_currentPanelButton != null)
                changePanelButton(artifaktRegular, 1f);

            PanelAnalytics(lastOpenedPanel, "Closed");
        }

        private void changePanelButton(TMP_FontAsset f, float opacity) {
            // set font
            TextMeshProUGUI text = _currentPanelButton.transform.parent.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.font = f;

            Image img = _currentPanelButton.GetComponent<Image>();
            img.color = new Color(img.color.r, img.color.g, img.color.b, opacity);
        }

        public void RegisterTab(string name, Tab t) {
            var obj       = Instantiate(TopButtonPrefab, TopButtonContainer.transform);
            var topB      = obj.GetComponent<TopButton>();
            topB.Tag.text = name;
            topB.ActualButton.onClick.AddListener(() => { SelectTab(name); });
            _registeredTabs[name] = (topB, t);
        }

        public void SelectTab(string name) {
            if (_currentTab == name)
                return;
            if (_currentTab != string.Empty) {
                var prevTopButton = _registeredTabs[_currentTab].topButton;
                prevTopButton.SetUnderlineColor(UnselectedTopButtonColor);
                prevTopButton.SetUnderlineHeight(1f);
            }
            var currentTopButton = _registeredTabs[name].topButton;
            currentTopButton.SetUnderlineColor(SelectedTopButtonColor);
            currentTopButton.SetUnderlineHeight(2f);
            LayoutManager.OpenTab(_registeredTabs[name].tab);
            _currentTab = name;
        }

        // public void OpenTab(GameObject tab)
        // {
        //     // LayoutManager.OpenTab(tab);
        //     //revert previous button's font and underline
        //     if(_currentTabButton!=null){
        //         changeTabButton(artifaktRegular,1,new Color(0.8f,0.8f,0.8f,1));
        //     }
        //     _currentTabButton = EventSystem.current.currentSelectedGameObject;
        //     if(_currentTabButton == null) _currentTabButton = homeButton; //On the first call, there is no button
        //     pressed changeTabButton(artifaktBold,2,new Color(0.02352941f,0.5882353f,0.8431373f,1));
        // }

        // private void changeTabButton(TMP_FontAsset f, float underlineHeight, Color c){
        //     //set font
        //     TextMeshProUGUI text = _currentTabButton.GetComponent<TextMeshProUGUI>();
        //     text.font = f;

        //     //set underline
        //     Transform underline = _currentTabButton.transform.GetChild(0);
        //     RectTransform rt = underline.GetComponent<RectTransform>();
        //     rt.sizeDelta = new Vector2 (rt.sizeDelta.x, underlineHeight);//height
        //     Image img = underline.GetComponent<Image>();
        //     img.color = c;//color
        // }
    }
}
