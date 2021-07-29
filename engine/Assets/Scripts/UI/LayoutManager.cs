using Synthesis.UI.Panels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LayoutManager
{


    //PANEL MANAGER
    private static Panel _currentPanel = null;
    public static Panel CurrentPanel
    {
        get => _currentPanel;
    }

    public static bool OpenPanel(GameObject p, bool forceClose=false)
    {
        if (_currentPanel != null)
            if (forceClose)
                _currentPanel.Close();
            else
                return false;
        var panel = Object.Instantiate(p, GameObject.Find("Panels").transform);//set transform
        _currentPanel = panel.GetComponent<Panel>();
        return true;
    }

    public static void ClosePanel()
    {
        if (_currentPanel != null)
            _currentPanel.Close();
    }


    //TAB MANAGER
    private static GameObject _currentTab = null;
    public static GameObject CurrentTab
    {
        get => _currentTab;
    }

    public static bool OpenTab(GameObject t, bool forceClose = true)
    {
        if (_currentTab != null)
            if (forceClose)
                Object.Destroy(_currentTab);
            else
                return false;

        _currentTab = Object.Instantiate(t, GameObject.Find("Bottom-Tabs").transform);//set transform    
        return true;
    }

}