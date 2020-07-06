using System.Collections.Generic;
using Unity.UIElements.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public GameObject UIContainer;
    public PanelRenderer synthesisToolbar;
    public VisualTreeAsset synthesisToolbarTab;
    private VisualElement synthesisToolbarTree;

    private readonly Dictionary<string, Button> tabs = new Dictionary<string, Button>();
    
    public void AddTab(string tabName)
    {
        if (!tabs.ContainsKey(tabName))
        {
            synthesisToolbarTree = synthesisToolbar.visualTree;
            var tabContainer = synthesisToolbarTree.Q<VisualElement>(name: "tab-panel");

            Button customTab = synthesisToolbarTab.CloneTree().Q<Button>(name: "blank-tab");

            customTab.clickable.clicked += () =>
            {
                DisplayPane(tabName);
                HighlightTab(tabName);
            };
            
            customTab.name = tabName;
            customTab.text = tabName;
            tabContainer.Add(customTab);

            tabs.Add(tabName, customTab);
        } else
        {
            Debug.LogError("Tab failed to be added because a tab with that name already exists");
        }

    }

    public void RemoveTab(string tabName)
    {
        if (tabs.ContainsKey(tabName))
        {
            var tabContainer = synthesisToolbarTree.Q<VisualElement>(name: "tab-panel");

            tabContainer.Remove(tabs[tabName]);
            tabs.Remove(tabName);
        } else
        {
            Debug.LogError("Removal of tab [" + tabName + "] failed because no such tab exists");
        }
    }

    private void DisplayPane(string moduleName)
    {
        // in editor function only
#if UNITY_EDITOR
        var modulePane = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Testing/Modules/" + moduleName + "/ui/Pane.uxml");
        if (modulePane != null)
        {
            var paneContainer = synthesisToolbarTree.Q<VisualElement>(name: "workspace-panel");

            for (int i = 0; i < paneContainer.childCount; i++)
            {
                if (paneContainer[i] != null)
                {
                    paneContainer.RemoveAt(i);
                }
            }
            
            paneContainer.Add(modulePane.CloneTree());
        }
        else
        {
            Debug.LogError("Failed to load pane for module [" + moduleName + "] because the UXML file could not be found");
        }
#endif
    }

    public void HighlightTab(string moduleName)
    {
        var tabContainer = synthesisToolbarTree.Q<VisualElement>(name: "tab-panel");
        
        foreach (VisualElement childTab in tabContainer.Children())
        {
            var activeTabColor = new Color(242.0f / 255.0f, 242.0f / 255.0f, 242.0f / 255.0f); // #F2F2F2

            childTab.style.backgroundColor = !childTab.name.Equals(moduleName) ? new StyleColor(StyleKeyword.Null) : activeTabColor;
        }
    }
    
    void Start()
    {
        synthesisToolbar.postUxmlReload += Bind;
    }

    IEnumerable<Object> Bind()
    {
        synthesisToolbarTree = synthesisToolbar.visualTree;

        Button setupButton = synthesisToolbarTree.Q<Button>(name: "load-setup");
        setupButton.clickable.clicked += () =>
        {
            TempAddTabs();
        };

        Button modulesButton = GetButton("modules-button");
        modulesButton.clickable.clicked += () =>
        {
            PanelRenderer moduleRenderer = GetUIGameObject("Modules").GetComponent<PanelRenderer>();
            moduleRenderer.enabled = !moduleRenderer.enabled;
        };

        return null;
    }

    Button GetButton(string buttonName)
    {
        return synthesisToolbarTree.Q<Button>(name: buttonName);
    }

    GameObject GetUIGameObject(string elementName)
    {
        for (int i = 0; i < UIContainer.transform.childCount; i++)
        {
            GameObject childObject = UIContainer.transform.GetChild(i).gameObject;
            if (childObject.name.Equals(elementName))
            {
                return childObject;
            }
        }
        return null;
    }

    private void TempAddTabs()
    {
        // in the future will iterate through all available modules and methods would be called externally
        AddTab("Falcon");
        AddTab("Apollo");
        AddTab("Engine");
    }

}
