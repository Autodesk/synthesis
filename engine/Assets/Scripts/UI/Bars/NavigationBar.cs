using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.UI.Bars
{
    public class NavigationBar : MonoBehaviour
    {
        public GameObject homeTab;
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
        }
    }
}
