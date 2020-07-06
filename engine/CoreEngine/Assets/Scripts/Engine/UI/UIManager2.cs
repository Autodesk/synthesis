using System.Collections;
using System.Collections.Generic;
using Unity.UIElements.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class UIManager2 : MonoBehaviour
{
    public PanelRenderer UIContainer;

    private Dictionary<string, Element> elements = new Dictionary<string, Element>();

    /// <summary>
    /// Adds a UI element
    /// </summary>
    /// <param name="element">Element to add</param>
    /// <param name="isVisible">Whether the element is initially visible or hidden</param>
    private void RegisterUIElement(Element element, bool isVisible)
    {
        if (!elements.ContainsKey(element.uniqueKey))
        {
            VisualElement parentElement = UIContainer.visualTree.Q<VisualElement>(name: element.treeName);
            
            element.visualElement.visible = isVisible;
            parentElement.Add(element.visualElement);

            elements.Add(element.uniqueKey, element);
        }
        else
        {
            Debug.LogError("Could not add UI element with key [" + element.uniqueKey + "] because the key is not unique");
        }
    }

    /// <summary>
    /// Removes a UI element entirely, consider ToggleUIElement first
    /// </summary>
    /// <param name="uniqueKey">Key identifier for UI element</param>
    private void UnregisterUIElement(string uniqueKey)
    {
        if (elements.ContainsKey(uniqueKey))
        {
            Element elementToRemove = elements[uniqueKey];

            elements.Remove(uniqueKey);
            elementToRemove.visualElement.RemoveFromHierarchy();
        }
        else
        {
            Debug.LogError("Could not unregister UI element with key [" + uniqueKey + "] because the key does not exist");
        }
    }

    /// <summary>
    /// Toggles a UI element's visibility
    /// </summary>
    /// <param name="uniqueKey">Key identifier for UI element</param>
    private void ToggleUIElement(string uniqueKey)
    {
        if (elements.ContainsKey(uniqueKey))
        {
            Element element = elements[uniqueKey];
            element.visualElement.visible = !element.visualElement.visible;
        }
        else
        {
            Debug.LogError("Could not toggle UI element with key [" + uniqueKey + "] because the key does not exist");
        }
    }

    private void AddTab(string tabName, string uniqueKey)
    {
        Element tabElement = new Element("Assets/UI/Toolbar/Tab.uxml", "tab-container", uniqueKey);

        Button customTab = tabElement.visualElement.Q<Button>(name: "blank-tab");
        customTab.name = tabName;
        customTab.text = tabName;

        customTab.clickable.clicked += () =>
        {
            HighlightTab(tabName);
            ToggleUIElement(tabName);
        };
        
        RegisterUIElement(tabElement, true);
    }

    private void RemoveTab(string tabName, string uniqueKey)
    {
        
    }

    private void HighlightTab(string tabName)
    {
        var tabContainer = GetTree(UIContainer).Q<VisualElement>(name: "tab-container");

        foreach (VisualElement childTab in tabContainer.Children())
        {
            Debug.Log("Child name : [" + childTab.name + "]");
            var activeTabColor = new Color(242.0f / 255.0f, 242.0f / 255.0f, 242.0f / 255.0f); // #F2F2F2

            childTab.style.backgroundColor = !childTab.name.Equals(tabName) ? new StyleColor(StyleKeyword.Null) : activeTabColor;
        }
    }

    void Start()
    {
        UIContainer.postUxmlReload += Bind;
    }

    IEnumerable<Object> Bind()
    {
        RegisterUIElement(new Element("Assets/UI/Toolbar/Toolbar.uxml", "root", "toolbar"), true);
        RegisterUIElement(new Element("Assets/UI/Modules/Modules.uxml", "root", "modules"), false);
        RegisterUIElement(new Element("Assets/UI/Settings/Settings.uxml", "root", "settings"), false);
        RegisterUIElement(new Element("Assets/UI/Modules/Module.uxml", "module-list", "testModule"), false);

        RegisterUIElement(new Element("Assets/Scripts/Testing/Modules/Falcon/ui/Pane.uxml", "bottom", "Falcon"), false);
        
        AddTab("Falcon", "testFalconTab");
        AddTab("Test2", "testTab2");

        RegisterCoreButtons();

        return null;
    }

    private void RegisterCoreButtons()
    {
        VisualElement mainTree = GetTree(UIContainer);

        Button modulesButton = mainTree.Q<Button>(name: "modules-button");
        modulesButton.clickable.clicked += () =>
        {
            ToggleUIElement("modules");
            ToggleUIElement("testModule");
        };

        Button settingsButton = mainTree.Q<Button>(name: "settings-button");
        settingsButton.clickable.clicked += () =>
        {
            ToggleUIElement("settings");
        };

    }

    VisualElement GetTree(PanelRenderer panel)
    {
        return panel.visualTree;
    }

}
