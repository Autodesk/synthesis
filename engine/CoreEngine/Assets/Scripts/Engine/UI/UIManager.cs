using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.UIElements.Runtime;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public PanelRenderer UIContainer;
    public VisualTreeAsset tabTemplate;

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
            bool toBeVisible = !element.visualElement.visible;

            element.visualElement.visible = toBeVisible;

            SetChildrenVisibility(element.visualElement, toBeVisible);
        }
        else
        {
            Debug.LogError("Could not toggle UI element with key [" + uniqueKey + "] because the key does not exist");
        }
    }

    private void SetChildrenVisibility(VisualElement parent, bool toBeVisible)
    {
        if (parent.childCount > 0)
        {
            foreach (VisualElement child in parent.Children())
            {
                child.visible = toBeVisible;
                SetChildrenVisibility(child, toBeVisible);
            }
        }
    }

    /// <summary>
    /// Centers a UI element horizontally
    /// </summary>
    /// <param name="elementKey">Key of element</param>
    /// <param name="width">Width of element container</param>
    private void CenterUIElementHorizontally(string elementKey, float width)
    {
        Element element = elements[elementKey];
        var elementStyle = element.visualElement.style;
        
        elementStyle.position = Position.Absolute;
        elementStyle.left = new StyleLength(Length.Percent(50.0f));
        elementStyle.top = new StyleLength(Length.Percent(20.0f));
        elementStyle.marginLeft = width / -2;
    }
    
    /// <summary>
    /// Adds a tab to the toolbar
    /// </summary>
    /// <param name="tabName">Name of the tab - should match Module name</param>
    private void AddTab(string tabName)
    {
        VisualTreeAsset tabAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/Toolbar/Tab.uxml");
        VisualElement tabContainer = GetTree(UIContainer).Q<VisualElement>(name: "tab-container");
        
        Button customTab = tabAsset.CloneTree().Q<Button>(name: "blank-tab");
        customTab.name = tabName;
        customTab.text = tabName;
        
        customTab.clickable.clicked += () =>
        {
            HighlightTab(tabName);
            ToggleUIElement(tabName);
        };
        
        tabContainer.Add(customTab);
    }

    /// <summary>
    /// Removes a tab from the toolbar
    /// </summary>
    /// <param name="tabName">Name of the tab</param>
    private void RemoveTab(string tabName)
    {
        var tabContainer = GetTree(UIContainer).Q<VisualElement>(name: "tab-container");
        foreach (VisualElement childTab in tabContainer.Children())
        {
            if (childTab.name.Equals(tabName))
            {
                tabContainer.Remove(childTab);
                return;
            }
        }
    }
    
    private void HighlightTab(string tabName)
    {
        var tabContainer = GetTree(UIContainer).Q<VisualElement>(name: "tab-container");

        foreach (VisualElement childTab in tabContainer.Children())
        {
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
        RegisterUIElement(new Element("Assets/UI/Modules/Module.uxml", "module-list", "testModule2"), false);
        RegisterUIElement(new Element("Assets/UI/Modules/Module.uxml", "module-list", "testModule3"), false);
        
        RegisterUIElement(new Element("Assets/Scripts/Testing/Modules/Falcon/ui/Pane.uxml", "bottom", "Falcon"), false);
        
        CenterUIElementHorizontally("modules", 500f);
        CenterUIElementHorizontally("settings", 600f);
        
        AddTab("Falcon");
        AddTab("Test2");

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
        };

        Button settingsButton = mainTree.Q<Button>(name: "settings-button");
        settingsButton.clickable.clicked += () =>
        {
            ToggleUIElement("settings");
        };

        Button helpButton = mainTree.Q<Button>(name: "help-button");
        helpButton.clickable.clicked += () =>
        {
            Application.OpenURL("https://synthesis.autodesk.com/tutorials.html");
        };

    }

    VisualElement GetTree(PanelRenderer panel)
    {
        return panel.visualTree;
    }

}
