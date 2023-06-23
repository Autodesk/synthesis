using Synthesis.UI.Panels;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Synthesis.UI.Tabs;
using Synthesis.UI.Bars;

public static class LayoutManager {
    // PANEL MANAGER
    private static Panel _currentPanel = null;
    public static Panel CurrentPanel { get => _currentPanel; }

    public static (bool success, Panel panel) OpenPanel(GameObject p, bool forceClose = false) {
        if (_currentPanel != null)
            if (forceClose)
                _currentPanel.Close();
            else
                return (false, null);
        var panel     = Object.Instantiate(p, GameObject.Find("Panels").transform); // set transform
        _currentPanel = panel.GetComponent<Panel>();
        return (true, _currentPanel);
    }

    public static void ClosePanel() {
        if (_currentPanel != null)
            _currentPanel.Close();
    }

    // TAB MANAGER

    private static Tab _currentTab  = null;
    public static Tab CurrentTab   => _currentTab;

    public static bool OpenTab(Tab tab, bool forceClose = true) {
        if (_currentTab != null) {
            if (!forceClose)
                return false;
            foreach (var t in NavigationBar.Instance.ModalTab.transform.GetComponentsInChildren<Transform>()) {
                if (t == NavigationBar.Instance.ModalTab.transform)
                    continue;
                Object.Destroy(t.gameObject);
            }
        }
        _currentTab = tab;
        tab.Create();
        return true;
    }

    // private static GameObject _currentTab = null;
    // public static GameObject CurrentTab
    // {
    //     get => _currentTab;
    // }

    // public static bool OpenTab(GameObject t, bool forceClose = true)
    // {
    //     if (_currentTab != null)
    //         if (forceClose)
    //             Object.Destroy(_currentTab);
    //         else
    //             return false;

    //     _currentTab = Object.Instantiate(t, GameObject.Find("Tabs").transform);//set transform
    //     return true;
    // }
}