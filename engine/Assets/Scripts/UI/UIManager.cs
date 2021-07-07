using Synthesis.UI.Panels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UIManager
{

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

}