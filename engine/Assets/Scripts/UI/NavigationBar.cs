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

        public TMP_FontAsset artifaktRegular;
        public TMP_FontAsset artifaktBold;

        private GameObject _currentButton = null;

        private void Start()
        {
            OpenTab(homeTab);
        }
        public void OpenPanel(GameObject prefab)
        {
            LayoutManager.OpenPanel(prefab, true);
        }
        public void CloseAllPanels()
        {
            LayoutManager.ClosePanel();
        }
        public void OpenTab(GameObject tab)
        {
            LayoutManager.OpenTab(tab);
            //revert previous button's font and underline
            if(_currentButton!=null){
                changeTabButton(artifaktRegular,1,new Color(0.8f,0.8f,0.8f,1));
            }
            _currentButton = EventSystem.current.currentSelectedGameObject;
            if(_currentButton == null) _currentButton = homeButton; //On the first call, there is no button pressed
            changeTabButton(artifaktBold,2,new Color(0.02352941f,0.5882353f,0.8431373f,1));
        }

        private void changeTabButton(TMP_FontAsset f, float underlineHeight, Color c){
            //set font
            TextMeshProUGUI text = _currentButton.GetComponent<TextMeshProUGUI>();
            text.font = f;

            //set underline
            Transform underline = _currentButton.transform.GetChild(0);
            RectTransform rt = underline.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2 (rt.sizeDelta.x, underlineHeight);//height
            Image img = underline.GetComponent<Image>();
            img.color = c;//color
        }
    }
}
