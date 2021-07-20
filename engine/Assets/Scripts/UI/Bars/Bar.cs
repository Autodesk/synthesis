using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.UI.Bars
{
    public class Bar : MonoBehaviour
    {
        public void OpenPanel(GameObject prefab)
        {
            PanelManager.OpenPanel(prefab, true);
        }
        public void CloseAllPanels()
        {
            PanelManager.ClosePanel();
        }
    }
}
