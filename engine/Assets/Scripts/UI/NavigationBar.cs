using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Synthesis.UI.Bars
{
    
    public class NavigationBar : MonoBehaviour
    {
        public GameObject homeTab;
        public GameObject homeButton;

        public TMP_Text VersionNumber;

        public TMP_FontAsset artifaktRegular;
        public TMP_FontAsset artifaktBold;

        private GameObject _currentTabButton;
        private GameObject _currentPanelButton;

        public NavigationBar navBarPrefab;

        private string lastOpenedPanel;

        private void Start() {
            VersionNumber.text = $"v {AutoUpdater.LocalVersion}  ALPHA";
            OpenTab(homeTab);
        }

        public void Exit() {
            if (Application.isEditor)
                Debug.Log("Would exit, but it's editor mode");
            else
            {
                var update = new AnalyticsEvent(category: "Exit", action: "Closed", label: $"Closed Synthesis");
                AnalyticsManager.LogEvent(update);
                AnalyticsManager.PostData();

                Application.Quit();
            }
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                navBarPrefab.CloseAllPanels();
            }
        }

        public void PanelAnalytics(string prefabName, string status)
        {
            var panel = new AnalyticsEvent(category: "Panel", action: status, label: prefabName);
            AnalyticsManager.LogEvent(panel);
            AnalyticsManager.PostData();
        }

        public void OpenPanel(GameObject prefab)
        {
            if(prefab!=null){
                  
                LayoutManager.OpenPanel(prefab, true);
                if(_currentPanelButton!=null) changePanelButton(artifaktRegular,new Color(1,1,1,1));

                //set current panel button to the button clicked
                _currentPanelButton = EventSystem.current.currentSelectedGameObject;
                changePanelButton(artifaktBold,new Color(0.8705882f,0.8705882f,0.8705882f,1));

                // Analytics Stuff
                lastOpenedPanel = prefab.name; // this will need to be an array for movable panels
                PanelAnalytics(prefab.name, "Opened");
            }
        }
        public void CloseAllPanels()
        {
            LayoutManager.ClosePanel();
            if(_currentPanelButton!=null) changePanelButton(artifaktRegular,new Color(1,1,1,1));

            PanelAnalytics(lastOpenedPanel, "Closed");
        }
        private void changePanelButton(TMP_FontAsset f, Color c){ //changes color and font of the clicked button    
            //set font
            TextMeshProUGUI text = _currentPanelButton.transform.parent.GetComponentInChildren<TextMeshProUGUI>();
            if(text!=null)text.font = f;

            Image img = _currentPanelButton.GetComponent<Image>();
            img.color = c;//color
            
        }

        public void OpenTab(GameObject tab)
        {
            LayoutManager.OpenTab(tab);
            //revert previous button's font and underline
            if(_currentTabButton!=null){
                changeTabButton(artifaktRegular,1,new Color(0.8f,0.8f,0.8f,1));
            }
            _currentTabButton = EventSystem.current.currentSelectedGameObject;
            if(_currentTabButton == null) _currentTabButton = homeButton; //On the first call, there is no button pressed
            changeTabButton(artifaktBold,2,new Color(0.02352941f,0.5882353f,0.8431373f,1));
        }

        private void changeTabButton(TMP_FontAsset f, float underlineHeight, Color c){
            //set font
            TextMeshProUGUI text = _currentTabButton.GetComponent<TextMeshProUGUI>();
            text.font = f;

            //set underline
            Transform underline = _currentTabButton.transform.GetChild(0);
            RectTransform rt = underline.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2 (rt.sizeDelta.x, underlineHeight);//height
            Image img = underline.GetComponent<Image>();
            img.color = c;//color
        }
    }
}
