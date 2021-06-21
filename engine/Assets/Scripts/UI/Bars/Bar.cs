using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.UI.Bars
{
    public class Bar : MonoBehaviour
    {
        private static List<GameObject> panels = new List<GameObject>();
        private static GameObject panelParent;

        void Awake()
        {
            if (panelParent == null) panelParent = GameObject.Find("Panels");
        }

        public void OpenPanel(GameObject prefab)
        {
            if (prefab == null) return;

            GameObject panel = Instantiate(prefab, panelParent.transform.position, panelParent.transform.rotation, panelParent.transform); //create
            // panel.transform.SetParent(panelParent.transform, false); //set parent

            panels.Add(panel); //for closing
        }
        public void CloseAllPanels()
        {
            foreach (GameObject panel in panels) Destroy(panel);
        }
    }
}
